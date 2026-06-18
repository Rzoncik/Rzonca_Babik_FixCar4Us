using System;
using System.Collections.Generic;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    public class ComplexRepairOrder
    {
        public RepairOrder Order { get; set; } = new RepairOrder();
        public Appointment Schedule { get; set; } = new Appointment();
        public List<OrderPart> RequiredParts { get; set; } = new List<OrderPart>();
    }

    // Interfejs builder
    public interface IRepairOrderBuilder
    {
        IRepairOrderBuilder SetVehicle(int vehicleId);
        IRepairOrderBuilder SetReportedIssues(string issues);
        IRepairOrderBuilder AddRequiredPart(int partId, int quantity, double currentPrice);
        IRepairOrderBuilder AssignMechanic(int employeeId);
        IRepairOrderBuilder SetEstimatedTime(string start, string end);
        ComplexRepairOrder Build();
    }

    public class RepairOrderBuilder : IRepairOrderBuilder
    {
        private ComplexRepairOrder _repairOrder = new ComplexRepairOrder();

        public RepairOrderBuilder()
        {
            Reset();
        }

        public void Reset()
        {
            _repairOrder = new ComplexRepairOrder();
            _repairOrder.Order.Status = "Nowe Zlecenie";
            _repairOrder.Order.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public IRepairOrderBuilder SetVehicle(int vehicleId)
        {
            _repairOrder.Order.VehicleId = vehicleId;
            _repairOrder.Schedule.VehicleId = vehicleId;
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

        public ComplexRepairOrder Build()
        {
            var result = _repairOrder;

            foreach (var part in result.RequiredParts)
            {
                result.Order.OrderParts.Add(part);
            }

            Reset();
            return result;
        }
    }

    public class RepairOrderDirector
    {
        private IRepairOrderBuilder _builder;

        public RepairOrderDirector(IRepairOrderBuilder builder)
        {
            _builder = builder;
        }

        public ComplexRepairOrder ConstructStandardOilChange(int vehicleId, int mechanicId, int oilPartId, int filterPartId)
        {
            return _builder
                .SetVehicle(vehicleId)
                .SetReportedIssues("Standardowa wymiana oleju i filtra")
                .AddRequiredPart(oilPartId, 1, 150.0)
                .AddRequiredPart(filterPartId, 1, 40.0)
                .AssignMechanic(mechanicId)
                .SetEstimatedTime("2024-06-01 10:00", "2024-06-01 11:00")
                .Build();
        }
    }
}
