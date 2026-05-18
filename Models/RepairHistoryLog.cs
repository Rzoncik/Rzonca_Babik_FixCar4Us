using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class RepairHistoryLog
{
    public int Id { get; set; }

    public int? RepairOrderId { get; set; }

    public string? StageAction { get; set; }

    public string? Timestamp { get; set; }

    public string? SnapshotData { get; set; }
}
