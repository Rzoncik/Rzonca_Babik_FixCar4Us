using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class RepairOrder
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public string? Status { get; set; }

    public string? ReportedIssues { get; set; }

    public string? CreatedAt { get; set; }

    public string? CompletedAt { get; set; }

    public virtual ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();
    
    public virtual ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();
    
    public virtual Vehicle? Vehicle { get; set; }
}
