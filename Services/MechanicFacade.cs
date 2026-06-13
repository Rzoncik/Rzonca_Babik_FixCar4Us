using System;
using System.Collections.Generic;
using System.Linq;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    public class PartUsageDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public double CurrentPrice { get; set; }
    }

    // Interfejs facade
    public interface IMechanicPanelFacade
    {
        bool LogWorkAndCompleteStage(int repairOrderId, int serviceId, double loggedHours, List<PartUsageDto> partsUsed, string nextStatus);
    }

    public class MechanicPanelFacade : IMechanicPanelFacade
    {
        private readonly AppDbContext _context;
        private readonly IRepairOrderNotifier _notifier;
        private readonly RepairPricingEngine _pricingEngine;

        // Ładowanie kontekstu bazy danych
        public MechanicPanelFacade(AppDbContext context, IRepairOrderNotifier notifier, RepairPricingEngine pricingEngine)
        {
            _context = context;
            _notifier = notifier;
            _pricingEngine = pricingEngine;

            // Rejestracja obserwatorów
            _notifier.Attach(new EmailNotificationObserver());
            _notifier.Attach(new SmsNotificationObserver());
        }

        public bool LogWorkAndCompleteStage(int repairOrderId, int serviceId, double loggedHours, List<PartUsageDto> partsUsed, string nextStatus)
        {
            var order = _context.RepairOrders.Find(repairOrderId);
            if (order == null) return false;

            // Logowanie czasu pracy i usługi
            var existingService = _context.OrderServices.FirstOrDefault(os => os.RepairOrderId == repairOrderId && os.ServiceId == serviceId);

            // Automatyczne liczenie czasu naprawy
            double autoCalculatedHours = loggedHours;
            if (nextStatus == "Wycena dodatkowa")
            {
                var startLog = _context.RepairHistoryLogs
                    .Where(l => l.RepairOrderId == repairOrderId && l.StageAction != null && l.StageAction.Contains("Przejście na status: W naprawie"))
                    .OrderByDescending(l => l.Id)
                    .FirstOrDefault();

                if (startLog != null && DateTime.TryParse(startLog.Timestamp, out DateTime startTime))
                {
                    autoCalculatedHours = (DateTime.Now - startTime).TotalHours;
                    autoCalculatedHours = Math.Round(autoCalculatedHours, 2);
                    if (autoCalculatedHours < 0.5) autoCalculatedHours = 0.5;
                }
            }
            else if (existingService != null)
            {
                autoCalculatedHours = existingService.LoggedHours ?? 0;
            }
            loggedHours = autoCalculatedHours;

            if (existingService != null)
            {
                // Aktualizujemy istniejacego rekordu
                existingService.LoggedHours = loggedHours;

                if (existingService.CustomerId == null && order.VehicleId.HasValue)
                {
                    var orderVehicle = _context.Vehicles.Find(order.VehicleId.Value);
                    if (orderVehicle != null) existingService.CustomerId = orderVehicle.CustomerId;
                }
            }
            else
            {
                int maxOrderServiceId = _context.OrderServices.Any() ? _context.OrderServices.Max(o => o.Id) : 0;

                // Pobieranie ID klienta z pojazdu
                int? customerId = null;
                if (order.VehicleId.HasValue)
                {
                    var orderVehicle = _context.Vehicles.Find(order.VehicleId.Value);
                    if (orderVehicle != null) customerId = orderVehicle.CustomerId;
                }

                var loggedService = new OrderService
                {
                    Id = maxOrderServiceId + 1,
                    RepairOrderId = repairOrderId,
                    ServiceId = serviceId,
                    CustomerId = customerId,
                    LoggedHours = loggedHours
                };
                _context.OrderServices.Add(loggedService);
            }

            // Pobieranie użytych części z magazynu oraz dodanie ich do zamówienia
            if (partsUsed != null && partsUsed.Any())
            {
                int maxOrderPartId = _context.OrderParts.Any() ? _context.OrderParts.Max(o => o.Id) : 0;
                foreach (var usage in partsUsed)
                {
                    var partInDb = _context.Parts.Find(usage.PartId);
                    if (partInDb != null)
                    {
                        // Pomniejszenie stanu magazynowego 
                        partInDb.StockQuantity -= usage.Quantity;
                        _context.Parts.Update(partInDb);

                        // Przypisanie części do zamówienia jako zuzytej
                        maxOrderPartId++;
                        var orderPart = new OrderPart
                        {
                            Id = maxOrderPartId,
                            RepairOrderId = repairOrderId,
                            PartId = usage.PartId,
                            Quantity = usage.Quantity,
                            PriceAtTheTime = usage.CurrentPrice
                        };
                        _context.OrderParts.Add(orderPart);
                    }
                }
            }

            // Aktualizacja statusu zlecenia 
            order.Status = nextStatus;
            if (nextStatus == "Gotowe do odbioru" || nextStatus == "Zakończone")
            {
                order.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                // Generowanie ostatecznego kosztorysu przy użyciu Pricing Engine
                var orderParts = _context.OrderParts.Where(p => p.RepairOrderId == repairOrderId).ToList();
                double partsTotal = orderParts.Sum(p => (p.Quantity ?? 0) * (p.PriceAtTheTime ?? 0.0));

                var service = _context.Services.Find(serviceId);
                double baseHourlyRate = service?.BaseHourlyRate ?? 150.0;

                double totalHours = existingService != null ? (existingService.LoggedHours ?? 0) : loggedHours;

                ILaborPricingStrategy strategy = new RealTimePricingStrategy();
                IRepairCost finalCost = new BaseRepairCost(partsTotal, strategy.CalculateLaborCost(baseHourlyRate, totalHours, 0, 0));

                // Doliczenie oplaty dodatkowej do ostatecznego kosztorysu
                if (order.AdditionalFee > 0)
                {
                    finalCost = new CustomFeeDecorator(finalCost, order.AdditionalFee, order.DifficultyDescription ?? "Opłata dodatkowa");
                }

                // Dodanie rabatu flotowego jeśli to zlecenie flotowe
                if (order.IsFleet == 1)
                {
                    finalCost = new FleetDiscountDecorator(finalCost, 0.15);
                }

                // Zapisz całkowity kosztorys do baazy
                if (existingService != null)
                {
                    existingService.FinalPrice = finalCost.GetTotalCost();
                }
                else
                {
                    var newService = _context.OrderServices.Local.FirstOrDefault(os => os.RepairOrderId == repairOrderId && os.ServiceId == serviceId);
                    if (newService != null) newService.FinalPrice = finalCost.GetTotalCost();
                }
            }

            // Dodanie wpisu do histori zlecenia że mechanik zakończył dany etap
            int maxHistoryLogId = _context.RepairHistoryLogs.Any() ? _context.RepairHistoryLogs.Max(h => h.Id) : 0;
            var historyLog = new RepairHistoryLog
            {
                Id = maxHistoryLogId + 1,
                RepairOrderId = repairOrderId,
                StageAction = $"[ZMIANA FASADOWA] Przejście na status: {nextStatus}, zalogowano {loggedHours}h",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SnapshotData = "Brak Memento"
            };
            _context.RepairHistoryLogs.Add(historyLog);

            // Zapisanie wszystkich zmian
            _context.SaveChanges();

            // Użycie wzorca Observer do powiadomienia klienta
            // Pobieranie klienta powiązanego z danym autem
            var vehicle = _context.Vehicles.Find(order.VehicleId);
            if (vehicle != null && vehicle.CustomerId.HasValue)
            {
                var customer = _context.Customers.Find(vehicle.CustomerId.Value);
                if (customer != null)
                {
                    _notifier.NotifyAll(customer, order, $"Status Twojej naprawy zmienił się na: {nextStatus}");
                }
            }

            return true;
        }
    }
}
