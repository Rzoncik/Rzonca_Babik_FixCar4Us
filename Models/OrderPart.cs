using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class OrderPart
{
    public int Id { get; set; }

    public int? RepairOrderId { get; set; }

    public int? PartId { get; set; }

    public int? Quantity { get; set; }

    public double? PriceAtTheTime { get; set; }

    public virtual Part? Part { get; set; }

    public virtual RepairOrder? RepairOrder { get; set; }
}
