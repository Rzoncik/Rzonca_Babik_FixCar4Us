using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Models;

public partial class Workstation
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? MaxLiftWeight { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
