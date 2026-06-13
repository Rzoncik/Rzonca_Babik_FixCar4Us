using System;
using System.Linq;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    // Interfejs mediator
    public interface IWorkshopMediator
    {
        bool CheckAvailability(string resourceType, int? resourceId, DateTime start, DateTime end);
        bool TryScheduleAppointment(Appointment appointment, out string message);
    }

    public abstract class WorkshopResourceColleague
    {
        protected IWorkshopMediator Mediator;

        public WorkshopResourceColleague(IWorkshopMediator mediator)
        {
            Mediator = mediator;
        }

        public abstract bool IsAvailable(int resourceId, DateTime start, DateTime end);

        protected bool DatesOverlap(string? dbStartStr, string? dbEndStr, DateTime checkStart, DateTime checkEnd)
        {
            if (string.IsNullOrEmpty(dbStartStr) || string.IsNullOrEmpty(dbEndStr))
                return false;

            if (DateTime.TryParse(dbStartStr, out DateTime dbStart) && DateTime.TryParse(dbEndStr, out DateTime dbEnd))
            {
                return checkStart < dbEnd && dbStart < checkEnd;
            }

            return false;
        }
    }

    public class EmployeeColleague : WorkshopResourceColleague
    {
        private readonly AppDbContext _context;

        public EmployeeColleague(IWorkshopMediator mediator, AppDbContext context) : base(mediator)
        {
            _context = context;
        }

        public override bool IsAvailable(int resourceId, DateTime start, DateTime end)
        {
            var overlapping = _context.Appointments.AsEnumerable()
                .Any(a => a.EmployeeId == resourceId && DatesOverlap(a.PlannedStart, a.PlannedEnd, start, end));

            return !overlapping;
        }
    }

    public class WorkstationColleague : WorkshopResourceColleague
    {
        private readonly AppDbContext _context;

        public WorkstationColleague(IWorkshopMediator mediator, AppDbContext context) : base(mediator)
        {
            _context = context;
        }

        public override bool IsAvailable(int resourceId, DateTime start, DateTime end)
        {
            var overlapping = _context.Appointments.AsEnumerable()
                .Any(a => a.WorkstationId == resourceId && DatesOverlap(a.PlannedStart, a.PlannedEnd, start, end));

            return !overlapping;
        }
    }

    public class ToolColleague : WorkshopResourceColleague
    {
        private readonly AppDbContext _context;

        public ToolColleague(IWorkshopMediator mediator, AppDbContext context) : base(mediator)
        {
            _context = context;
        }

        public override bool IsAvailable(int resourceId, DateTime start, DateTime end)
        {
            var overlapping = _context.Appointments.AsEnumerable()
                .Any(a => (a.ToolId == resourceId || a.ToolId2 == resourceId || a.ToolId3 == resourceId)
                          && DatesOverlap(a.PlannedStart, a.PlannedEnd, start, end));

            return !overlapping;
        }
    }

    public class WorkshopMediator : IWorkshopMediator
    {
        private EmployeeColleague? _employeeColleague;
        private WorkstationColleague? _workstationColleague;
        private ToolColleague? _toolColleague;
        private readonly AppDbContext _context;

        public WorkshopMediator(AppDbContext context)
        {
            _context = context;

            _employeeColleague = new EmployeeColleague(this, _context);
            _workstationColleague = new WorkstationColleague(this, _context);
            _toolColleague = new ToolColleague(this, _context);
        }

        public bool CheckAvailability(string resourceType, int? resourceId, DateTime start, DateTime end)
        {
            if (!resourceId.HasValue) return true;

            if (resourceType == "Employee" && _employeeColleague != null)
                return _employeeColleague.IsAvailable(resourceId.Value, start, end);

            if (resourceType == "Workstation" && _workstationColleague != null)
                return _workstationColleague.IsAvailable(resourceId.Value, start, end);

            if (resourceType == "Tool" && _toolColleague != null)
                return _toolColleague.IsAvailable(resourceId.Value, start, end);

            return false;
        }

        public bool TryScheduleAppointment(Appointment appointment, out string message)
        {
            if (!DateTime.TryParse(appointment.PlannedStart, out DateTime start) ||
                !DateTime.TryParse(appointment.PlannedEnd, out DateTime end))
            {
                message = "Wprowadzono nieprawidłowy format daty.";
                return false;
            }

            if (start >= end)
            {
                message = "Czas zakończenia musi być późniejszy niż czas rozpoczęcia.";
                return false;
            }

            // Sprawdzanie wymagan do rozpoczenia naprawy

            // 1. Sprawdzenie mechanika
            if (!CheckAvailability("Employee", appointment.EmployeeId, start, end))
            {
                message = "Wybrany mechanik ma już zaplanowaną inną pracę w tym czasie.";
                return false;
            }

            // 2. Sprawdzenie stanowiska
            if (!CheckAvailability("Workstation", appointment.WorkstationId, start, end))
            {
                message = "Wybrane stanowisko (podnośnik) jest już zajęte w tym terminie.";
                return false;
            }

            // 3. Sprawdzenie narzędzi
            if (!CheckAvailability("Tool", appointment.ToolId, start, end) ||
                !CheckAvailability("Tool", appointment.ToolId2, start, end) ||
                !CheckAvailability("Tool", appointment.ToolId3, start, end))
            {
                message = "Jedno z wybranych narzędzi specjalistycznych jest używane przy innej naprawie w tym czasie.";
                return false;
            }

            message = "Wszystkie zasoby są dostępne. Rezerwacja może zostać dodana do grafiku.";
            return true;
        }
    }
}
