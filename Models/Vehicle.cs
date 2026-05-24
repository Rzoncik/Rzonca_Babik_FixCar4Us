using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class Vehicle
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public virtual Customer? Customer { get; set; }

    public string? LicensePlate { get; set; }

    public string? Model { get; set; }

    public int? Mileage { get; set; }

    public string? VIN { get; set; }

    public virtual ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
    public virtual ICollection<TechnicalInspection> TechnicalInspections { get; set; } = new List<TechnicalInspection>();
}
