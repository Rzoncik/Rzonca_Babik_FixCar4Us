using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class Service
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public double? BaseHourlyRate { get; set; }

    public virtual ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();
}
