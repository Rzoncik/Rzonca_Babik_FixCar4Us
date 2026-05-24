using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public int? IsFleet { get; set; }

    public double? DiscountRatePercent { get; set; }

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
