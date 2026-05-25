using System;
using System.Collections.Generic;
using System.Linq;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    // Prosty model DTO (Data Transfer Object) do komunikacji z widokiem mechanika
    public class PartUsageDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public double CurrentPrice { get; set; }
    }

    // =========================================================================
    // INTERFEJS FASADY (Facade)
    // =========================================================================
    public interface IMechanicPanelFacade
    {
        bool LogWorkAndCompleteStage(int repairOrderId, int serviceId, double loggedHours, List<PartUsageDto> partsUsed, string nextStatus);
    }

    // =========================================================================
    // FASADA (Facade)
    // Ukrywa złożoność operacji wykonywanych na bazie danych przed kontrolerem/widokiem.
    // Jednym wywołaniem aktualizuje status, loguje czas i pobiera z magazynu.
    // =========================================================================
    public class MechanicPanelFacade : IMechanicPanelFacade
    {
        private readonly AppDbContext _context;
        private readonly IRepairOrderNotifier _notifier;

        // Wstrzykujemy kontekst bazy danych oraz system powiadomień
        public MechanicPanelFacade(AppDbContext context, IRepairOrderNotifier notifier)
        {
            _context = context;
            _notifier = notifier;
            
            // Rejestracja obserwatorów (w prawdziwej aplikacji można to robić w DI lub konfiguracji)
            _notifier.Attach(new EmailNotificationObserver());
            _notifier.Attach(new SmsNotificationObserver());
        }

        /// <summary>
        /// Wykonuje wszystkie akcje przypisane jednemu "kliknięciu" mechanika
        /// </summary>
        public bool LogWorkAndCompleteStage(int repairOrderId, int serviceId, double loggedHours, List<PartUsageDto> partsUsed, string nextStatus)
        {
            var order = _context.RepairOrders.Find(repairOrderId);
            if (order == null) return false;

            // KROK 1: Logowanie czasu pracy i usługi
            var loggedService = new OrderService 
            {
                RepairOrderId = repairOrderId,
                ServiceId = serviceId,
                LoggedHours = loggedHours
            };
            _context.OrderServices.Add(loggedService);

            // KROK 2: Pobieranie użytych części z magazynu oraz dodanie ich do zamówienia
            if (partsUsed != null && partsUsed.Any())
            {
                foreach (var usage in partsUsed)
                {
                    var partInDb = _context.Parts.Find(usage.PartId);
                    if (partInDb != null)
                    {
                        // a) Pomniejszenie stanu magazynowego 
                        partInDb.StockQuantity -= usage.Quantity;

                        // b) Przypisanie części do zamówienia jako "zużytej"
                        var orderPart = new OrderPart
                        {
                            RepairOrderId = repairOrderId,
                            PartId = usage.PartId,
                            Quantity = usage.Quantity,
                            PriceAtTheTime = usage.CurrentPrice
                        };
                        _context.OrderParts.Add(orderPart);
                    }
                }
            }

            // KROK 3: Aktualizacja statusu zlecenia 
            order.Status = nextStatus;
            if (nextStatus == "Gotowe do odbioru" || nextStatus == "Zakończone")
            {
                order.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            }

            // KROK 4: Dodanie wpisu do historii zlecenia, że mechanik zakończył dany etap
            var historyLog = new RepairHistoryLog
            {
                RepairOrderId = repairOrderId,
                StageAction = $"[ZMIANA FASADOWA] Przejście na status: {nextStatus}, zalogowano {loggedHours}h",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SnapshotData = "Brak Memento"
            };
            _context.RepairHistoryLogs.Add(historyLog);

            // Zapisujemy wszystkie wyżej wymienione zmiany RAZ (wzorzec UnitOfWork realizowany przez SaveChanges EF)
            _context.SaveChanges();

            // KROK 5: Użycie wzorca Observer do powiadomienia klienta
            // Najpierw pobieramy klienta powiązanego z tym autem
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
