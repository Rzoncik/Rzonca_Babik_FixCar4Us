using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class Appointment
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? EmployeeId { get; set; }

    public int? WorkstationId { get; set; }

    public int? ToolId { get; set; }

    public int? ToolId2 { get; set; }

    public int? ToolId3 { get; set; }

    public string? PlannedStart { get; set; }

    public string? PlannedEnd { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Workstation? Workstation { get; set; }

    public string? Status { get; set; } = "Zaplanowane";
}
