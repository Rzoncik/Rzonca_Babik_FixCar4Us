using System;
using System.Linq;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    // 1. Interfejs Mediatora - koordynuje współpracę obiektów
    public interface IWorkshopMediator
    {
        bool CheckAvailability(string resourceType, int? resourceId, DateTime start, DateTime end);
        bool TryScheduleAppointment(Appointment appointment, out string message);
    }

    // 2. Klasa bazowa (lub interfejs) dla Kolegów (Colleagues)
    public abstract class WorkshopResourceColleague
    {
        protected IWorkshopMediator Mediator;

        public WorkshopResourceColleague(IWorkshopMediator mediator)
        {
            Mediator = mediator;
        }

        public abstract bool IsAvailable(int resourceId, DateTime start, DateTime end);

        // Pomocnicza metoda do sprawdzania nakładania się dat (jako studenci używamy prostych ifów)
        protected bool DatesOverlap(string? dbStartStr, string? dbEndStr, DateTime checkStart, DateTime checkEnd)
        {
            if (string.IsNullOrEmpty(dbStartStr) || string.IsNullOrEmpty(dbEndStr)) 
                return false;

            if (DateTime.TryParse(dbStartStr, out DateTime dbStart) && DateTime.TryParse(dbEndStr, out DateTime dbEnd))
            {
                // Jeśli start nowej naprawy jest przed końcem starej, a koniec nowej jest po starcie starej to jest konflikt
                return checkStart < dbEnd && dbStart < checkEnd;
            }

            return false;
        }
    }

    // 3. Konkretni Koledzy sprawdzający poszczególne zasoby
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
            
            return !overlapping; // true, jeśli nie ma nakładających się terminów
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
                .Any(a => a.ToolId == resourceId && DatesOverlap(a.PlannedStart, a.PlannedEnd, start, end));
            
            return !overlapping;
        }
    }

    // 4. Konkretny Mediator (Orchestrator)
    public class WorkshopMediator : IWorkshopMediator
    {
        private EmployeeColleague? _employeeColleague;
        private WorkstationColleague? _workstationColleague;
        private ToolColleague? _toolColleague;
        private readonly AppDbContext _context;

        // Wstrzykujemy kontekst bazy danych, aby móc utworzyć kolegów wewnątrz Mediatora
        // (W podejściu szkolnym tak jest łatwiej zarządzać zależnościami)
        public WorkshopMediator(AppDbContext context)
        {
            _context = context;
            
            // Inicjalizujemy kolegów i przekazujemy im referencję do samego siebie (this)
            _employeeColleague = new EmployeeColleague(this, _context);
            _workstationColleague = new WorkstationColleague(this, _context);
            _toolColleague = new ToolColleague(this, _context);
        }

        public bool CheckAvailability(string resourceType, int? resourceId, DateTime start, DateTime end)
        {
            if (!resourceId.HasValue) return true; // Jeśli zasób nie jest wymagany, uznajemy go za dostępny

            if (resourceType == "Employee" && _employeeColleague != null)
                return _employeeColleague.IsAvailable(resourceId.Value, start, end);
            
            if (resourceType == "Workstation" && _workstationColleague != null)
                return _workstationColleague.IsAvailable(resourceId.Value, start, end);
            
            if (resourceType == "Tool" && _toolColleague != null)
                return _toolColleague.IsAvailable(resourceId.Value, start, end);

            return false;
        }

        // Główna metoda Orkiestratora - koordynuje wszystkie zasoby na raz
        public bool TryScheduleAppointment(Appointment appointment, out string message)
        {
            if (!DateTime.TryParse(appointment.PlannedStart, out DateTime start) || 
                !DateTime.TryParse(appointment.PlannedEnd, out DateTime end))
            {
                message = "Wprowadzono nieprawidłowy format daty.";
                return false;
            }

            // Sprawdzamy, czy start nie jest po końcu
            if (start >= end)
            {
                message = "Czas zakończenia musi być późniejszy niż czas rozpoczęcia.";
                return false;
            }

            // Zgodnie z wymaganiami, naprawa wymaga jednoczesnej dostępności 3 elementów:
            
            // 1. Sprawdzamy mechanika
            if (!CheckAvailability("Employee", appointment.EmployeeId, start, end))
            {
                message = "Wybrany mechanik ma już zaplanowaną inną pracę w tym czasie.";
                return false;
            }

            // 2. Sprawdzamy stanowisko / podnośnik
            if (!CheckAvailability("Workstation", appointment.WorkstationId, start, end))
            {
                message = "Wybrane stanowisko (podnośnik) jest już zajęte w tym terminie.";
                return false;
            }

            // 3. Sprawdzamy narzędzia specjalistyczne
            if (!CheckAvailability("Tool", appointment.ToolId, start, end))
            {
                message = "Wybrane narzędzie specjalistyczne jest używane przy innej naprawie w tym czasie.";
                return false;
            }

            message = "Wszystkie zasoby są dostępne. Rezerwacja może zostać dodana do grafiku.";
            return true;
        }
    }
}
