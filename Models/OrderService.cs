using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class OrderService
{
    public int Id { get; set; }

    public int? RepairOrderId { get; set; }

    public int? ServiceId { get; set; }

    public int? CustomerId { get; set; }

    public virtual Customer? Customer { get; set; }

    public double? LoggedHours { get; set; }

    public double? FinalPrice { get; set; }

    public virtual Service? Service { get; set; }

    public virtual RepairOrder? RepairOrder { get; set; }
}
