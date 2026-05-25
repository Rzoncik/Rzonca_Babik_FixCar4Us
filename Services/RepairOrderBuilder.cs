using System;
using System.Collections.Generic;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    // =========================================================================
    // PRODUKT (Kompleksowy obiekt zlecenia naprawy)
    // Łączy w sobie rekord Naprawy, Terminarza oraz Części.
    // =========================================================================
    public class ComplexRepairOrder
    {
        public RepairOrder Order { get; set; } = new RepairOrder();
        public Appointment Schedule { get; set; } = new Appointment();
        public List<OrderPart> RequiredParts { get; set; } = new List<OrderPart>();
    }

    // =========================================================================
    // INTERFEJS BUILDERA
    // Definiuje kroki budowania skomplikowanego zlecenia naprawy
    // =========================================================================
    public interface IRepairOrderBuilder
    {
        IRepairOrderBuilder SetVehicle(int vehicleId);
        IRepairOrderBuilder SetReportedIssues(string issues);
        IRepairOrderBuilder AddRequiredPart(int partId, int quantity, double currentPrice);
        IRepairOrderBuilder AssignMechanic(int employeeId);
        IRepairOrderBuilder SetEstimatedTime(string start, string end);
        ComplexRepairOrder Build();
    }

    // =========================================================================
    // KONKRETNY BUILDER
    // Krok po kroku składa skomplikowane zlecenie, obsługując powiązania
    // =========================================================================
    public class RepairOrderBuilder : IRepairOrderBuilder
    {
        private ComplexRepairOrder _repairOrder = new ComplexRepairOrder();

        public RepairOrderBuilder()
        {
            Reset();
        }

        // Przygotowuje "czystą kartę" do tworzenia nowego zlecenia
        public void Reset()
        {
            _repairOrder = new ComplexRepairOrder();
            _repairOrder.Order.Status = "Nowe Zlecenie";
            _repairOrder.Order.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public IRepairOrderBuilder SetVehicle(int vehicleId)
        {
            _repairOrder.Order.VehicleId = vehicleId;
            _repairOrder.Schedule.VehicleId = vehicleId; // Automatycznie wiążemy terminarz z pojazdem
            return this;
        }

        public IRepairOrderBuilder SetReportedIssues(string issues)
        {
            _repairOrder.Order.ReportedIssues = issues;
            return this;
        }

        public IRepairOrderBuilder AddRequiredPart(int partId, int quantity, double currentPrice)
        {
            var partUse = new OrderPart 
            { 
                PartId = partId, 
                Quantity = quantity, 
                PriceAtTheTime = currentPrice 
            };
            
            _repairOrder.RequiredParts.Add(partUse);
            return this;
        }

        public IRepairOrderBuilder AssignMechanic(int employeeId)
        {
            _repairOrder.Schedule.EmployeeId = employeeId;
            return this;
        }

        public IRepairOrderBuilder SetEstimatedTime(string start, string end)
        {
            _repairOrder.Schedule.PlannedStart = start;
            _repairOrder.Schedule.PlannedEnd = end;
            return this;
        }

        // Zwraca w pełni poprawnie skonstruowany i powiązany obiekt
        public ComplexRepairOrder Build()
        {
            var result = _repairOrder;
            
            // Wiążemy dodane części bezpośrednio z encją zamówienia przed zapisem do bazy
            foreach (var part in result.RequiredParts)
            {
                result.Order.OrderParts.Add(part);
            }

            // Po wywołaniu Build, Builder jest gotowy do tworzenia kolejnego obiektu
            Reset();
            return result;
        }
    }

    // =========================================================================
    // KIEROWNIK (Director) - Opcjonalnie
    // Zarządza procesem budowania, przydaje się przy z góry określonych szablonach.
    // =========================================================================
    public class RepairOrderDirector
    {
        private IRepairOrderBuilder _builder;

        public RepairOrderDirector(IRepairOrderBuilder builder)
        {
            _builder = builder;
        }

        // Przykład metody składającej predefiniowany pakiet: Standardowy Przegląd Olejowy
        public ComplexRepairOrder ConstructStandardOilChange(int vehicleId, int mechanicId, int oilPartId, int filterPartId)
        {
            return _builder
                .SetVehicle(vehicleId)
                .SetReportedIssues("Standardowa wymiana oleju i filtra")
                .AddRequiredPart(oilPartId, 1, 150.0)    // bańka oleju
                .AddRequiredPart(filterPartId, 1, 40.0)  // filtr oleju
                .AssignMechanic(mechanicId)
                .SetEstimatedTime("2024-06-01 10:00", "2024-06-01 11:00")
                .Build();
        }
    }
}
