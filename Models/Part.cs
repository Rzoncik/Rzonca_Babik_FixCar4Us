using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class Part
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? PartNumber { get; set; }

    public double? PurchasePrice { get; set; }

    public double? SalePrice { get; set; }

    public int? StockQuantity { get; set; }

    public virtual ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();
}
