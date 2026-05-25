using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Pages.Calendar
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly Services.IWorkshopMediator _mediator;

        public IndexModel(AppDbContext context, Services.IWorkshopMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        // Lista wizyt do wyświetlenia
        public IList<Appointment> Appointments { get; set; } = default!;

        // Słownik używany do szybkiego wyszukiwania nazwy pojazdu w widoku (ponieważ nie ma Navigation Property dla Vehicle w Appointment)
        public Dictionary<int, string> VehiclesDict { get; set; } = new Dictionary<int, string>();

        [BindProperty]
        public Appointment NewAppointment { get; set; } = new Appointment();

        // Listy dla DropDowns (Select) w HTML
        public SelectList WorkstationsList { get; set; } = default!;
        public SelectList VehiclesList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Walidacja czy wybrano podstawowe informacje
            if (NewAppointment.VehicleId != null && NewAppointment.WorkstationId != null)
            {
                // WZORZEC MEDIATOR: Sprawdzamy, czy można zaplanować wizytę bez konfliktów na warsztacie
                if (!_mediator.TryScheduleAppointment(NewAppointment, out string errorMessage))
                {
                    // Jeśli Mediator zablokował rezerwację, zwracamy błąd do interfejsu (np. stanowisko jest zajęte)
                    ModelState.AddModelError(string.Empty, errorMessage);
                    await LoadDataAsync();
                    return Page();
                }

                // Rozwiązanie dla ValueGeneratedNever() skonfigurowanego w DbContext:
                int maxId = 0;
                if (await _context.Appointments.AnyAsync())
                {
                    maxId = await _context.Appointments.MaxAsync(a => a.Id);
                }
                NewAppointment.Id = maxId + 1; // Ręczna inkrementacja ID

                _context.Appointments.Add(NewAppointment);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }

            // W razie braku walidacji ładujemy listy na nowo
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Zakończone";
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./Index");
        }

        private async Task LoadDataAsync()
        {
            // Ładowanie wizyt posortowanych chronologicznie
            if (_context.Appointments != null)
            {
                Appointments = await _context.Appointments
                    .Include(a => a.Workstation) // Wciągamy dane powiązanego stanowiska
                    .OrderBy(a => a.PlannedStart)
                    .ToListAsync();
            }

            // Pobieranie aut, by wypełnić listę wyboru i słownik do widoku
            if (_context.Vehicles != null)
            {
                var vehicles = await _context.Vehicles.ToListAsync();
                VehiclesDict = vehicles.ToDictionary(v => v.Id, v => $"{v.LicensePlate} ({v.Model})");

                var vehicleOptions = vehicles.Select(v => new { v.Id, DisplayName = $"{v.LicensePlate} - {v.Model}" });
                VehiclesList = new SelectList(vehicleOptions, "Id", "DisplayName");
            }

            // Pobieranie stanowisk
            if (_context.Workstations != null)
            {
                var workstations = await _context.Workstations.ToListAsync();
                WorkstationsList = new SelectList(workstations, "Id", "Name");
            }
        }
    }
}
