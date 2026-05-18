using System;

namespace Rzonca_Babik_FixCar4Us.Models;

public class TechnicalInspection
{
    public int Id { get; set; }
    public int? VehicleId { get; set; }
    
    public DateTime InspectionDate { get; set; }
    public string? Result { get; set; }
    public string? Comments { get; set; }
    
    public virtual Vehicle? Vehicle { get; set; }
}
