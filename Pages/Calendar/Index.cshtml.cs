using System;
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

        public Dictionary<int, string> VehiclesDict { get; set; } = new Dictionary<int, string>();

        [BindProperty]
        public Appointment NewAppointment { get; set; } = new Appointment();

        public SelectList WorkstationsList { get; set; } = default!;
        public SelectList VehiclesList { get; set; } = default!;
        public SelectList ServicesList { get; set; } = default!;
        public SelectList ToolsList { get; set; } = default!;
        public SelectList PartsList { get; set; } = default!;

        [BindProperty]
        public int? SelectedServiceId { get; set; }

        [BindProperty]
        public int? SelectedPartId { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnGetRecommendAsync(int serviceId, string start, string end)
        {
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddHours(1);
            if (!string.IsNullOrEmpty(start)) DateTime.TryParse(start, out startTime);
            if (!string.IsNullOrEmpty(end)) DateTime.TryParse(end, out endTime);

            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return new JsonResult(new { });

            int? recommendedWorkstationId = null;
            int? recommendedToolId = null;
            int? recommendedPartId = null;

            var workstations = await _context.Workstations.ToListAsync();
            var tools = await _context.Tools.ToListAsync();
            var parts = await _context.Parts.Where(p => p.StockQuantity > 0).ToListAsync();

            if (service.Name.Contains("silnik", StringComparison.OrdinalIgnoreCase) ||
                service.Name.Contains("rozrząd", StringComparison.OrdinalIgnoreCase) ||
                service.Name.Contains("sprzęgł", StringComparison.OrdinalIgnoreCase) ||
                service.Name.Contains("klock", StringComparison.OrdinalIgnoreCase))
            {
                recommendedWorkstationId = workstations.FirstOrDefault(w => (w.Name ?? "").Contains("Podnośnik dwukolumnowy") && _mediator.CheckAvailability("Workstation", w.Id, startTime, endTime))?.Id;
            }
            else if (service.Name.Contains("diagnost", StringComparison.OrdinalIgnoreCase) ||
                     service.Name.Contains("przebieg", StringComparison.OrdinalIgnoreCase))
            {
                recommendedWorkstationId = workstations.FirstOrDefault(w => (w.Name ?? "").Contains("Stanowisko diagnostyczne") && _mediator.CheckAvailability("Workstation", w.Id, startTime, endTime))?.Id;
            }
            else if (service.Name.Contains("elektryczn", StringComparison.OrdinalIgnoreCase))
            {
                recommendedWorkstationId = workstations.FirstOrDefault(w => (w.Name ?? "").Contains("Stanowisko elektryczne") && _mediator.CheckAvailability("Workstation", w.Id, startTime, endTime))?.Id;
            }
            else if (service.Name.Contains("blachar", StringComparison.OrdinalIgnoreCase))
            {
                recommendedWorkstationId = workstations.FirstOrDefault(w => (w.Name ?? "").Contains("Stanowisko blacharskie") && _mediator.CheckAvailability("Workstation", w.Id, startTime, endTime))?.Id;
            }
            else if (service.Name.Contains("lakier", StringComparison.OrdinalIgnoreCase))
            {
                recommendedWorkstationId = workstations.FirstOrDefault(w => (w.Name ?? "").Contains("Kabina lakiernicza") && _mediator.CheckAvailability("Workstation", w.Id, startTime, endTime))?.Id;
            }
            else if (service.Name.Contains("klimatyzacj", StringComparison.OrdinalIgnoreCase))
            {
                recommendedWorkstationId = workstations.FirstOrDefault(w => (w.Name ?? "").Contains("Stanowisko klimatyzacji") && _mediator.CheckAvailability("Workstation", w.Id, startTime, endTime))?.Id;
            }
            else if (service.Name.Contains("geometria", StringComparison.OrdinalIgnoreCase))
            {
                recommendedWorkstationId = workstations.FirstOrDefault(w => (w.Name ?? "").Contains("Stanowisko do geometrii") && _mediator.CheckAvailability("Workstation", w.Id, startTime, endTime))?.Id;
            }
            else
            {
                recommendedWorkstationId = workstations.FirstOrDefault(w => _mediator.CheckAvailability("Workstation", w.Id, startTime, endTime))?.Id;
            }

            if (service.Name.Contains("rozrząd", StringComparison.OrdinalIgnoreCase))
                recommendedToolId = tools.FirstOrDefault(t => (t.Name ?? "").Contains("blokad", StringComparison.OrdinalIgnoreCase) && _mediator.CheckAvailability("Tool", t.Id, startTime, endTime))?.Id;
            else if (service.Name.Contains("silnik", StringComparison.OrdinalIgnoreCase))
                recommendedToolId = tools.FirstOrDefault(t => (t.Name ?? "").Contains("Stojak", StringComparison.OrdinalIgnoreCase) && _mediator.CheckAvailability("Tool", t.Id, startTime, endTime))?.Id;
            else if (service.Name.Contains("sprzęgł", StringComparison.OrdinalIgnoreCase))
                recommendedToolId = tools.FirstOrDefault(t => (t.Name ?? "").Contains("Belka", StringComparison.OrdinalIgnoreCase) && _mediator.CheckAvailability("Tool", t.Id, startTime, endTime))?.Id;
            else
                recommendedToolId = tools.FirstOrDefault(t => _mediator.CheckAvailability("Tool", t.Id, startTime, endTime))?.Id;

            if (service.Name.Contains("rozrząd", StringComparison.OrdinalIgnoreCase))
                recommendedPartId = parts.FirstOrDefault(p => (p.Name ?? "").Contains("rozrząd", StringComparison.OrdinalIgnoreCase))?.Id;
            else if (service.Name.Contains("silnik", StringComparison.OrdinalIgnoreCase))
                recommendedPartId = parts.FirstOrDefault(p => (p.Name ?? "").Contains("uszczel", StringComparison.OrdinalIgnoreCase))?.Id;
            else if (service.Name.Contains("elektryczn", StringComparison.OrdinalIgnoreCase))
                recommendedPartId = parts.FirstOrDefault(p => (p.Name ?? "").Contains("przewod", StringComparison.OrdinalIgnoreCase))?.Id;

            return new JsonResult(new
            {
                workstationId = recommendedWorkstationId,
                toolId = recommendedToolId,
                partId = recommendedPartId
            });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Sprawdzanie czy wybrano podstawowe informacje
            if (NewAppointment.VehicleId != null && NewAppointment.WorkstationId != null)
            {
                // Sprawdzanie czy można zaplanować wizytę bez konfliktów na warsztacie dzięki mediatorowi
                if (!_mediator.TryScheduleAppointment(NewAppointment, out string errorMessage))
                {
                    // Mediator zwraca błąd gdy nie można zaplanować wizyty
                    ModelState.AddModelError(string.Empty, errorMessage);
                    await LoadDataAsync();
                    return Page();
                }

                int maxId = 0;
                if (await _context.Appointments.AnyAsync())
                {
                    maxId = await _context.Appointments.MaxAsync(a => a.Id);
                }
                NewAppointment.Id = maxId + 1;

                _context.Appointments.Add(NewAppointment);

                int maxOrderId = 0;
                if (await _context.RepairOrders.AnyAsync())
                {
                    maxOrderId = await _context.RepairOrders.MaxAsync(r => r.Id);
                }

                string reportedIssuesText = "Przegląd ogólny";
                if (SelectedServiceId.HasValue)
                {
                    var selectedServiceObj = await _context.Services.FindAsync(SelectedServiceId.Value);
                    if (selectedServiceObj != null)
                        reportedIssuesText = selectedServiceObj.Name ?? "Wybrana usługa";
                }

                var newRepairOrder = new RepairOrder
                {
                    Id = maxOrderId + 1,
                    VehicleId = NewAppointment.VehicleId,
                    Status = "Przyjęte",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    ReportedIssues = reportedIssuesText
                };
                _context.RepairOrders.Add(newRepairOrder);

                if (SelectedServiceId.HasValue)
                {
                    int maxOrderServiceId = 0;
                    if (await _context.OrderServices.AnyAsync()) maxOrderServiceId = await _context.OrderServices.MaxAsync(o => o.Id);

                    _context.OrderServices.Add(new OrderService
                    {
                        Id = maxOrderServiceId + 1,
                        RepairOrderId = newRepairOrder.Id,
                        ServiceId = SelectedServiceId.Value,
                        LoggedHours = 0,
                        FinalPrice = 0
                    });
                }

                if (SelectedPartId.HasValue)
                {
                    var part = await _context.Parts.FindAsync(SelectedPartId.Value);
                    if (part != null)
                    {
                        int maxOrderPartId = 0;
                        if (await _context.OrderParts.AnyAsync()) maxOrderPartId = await _context.OrderParts.MaxAsync(o => o.Id);

                        _context.OrderParts.Add(new OrderPart
                        {
                            Id = maxOrderPartId + 1,
                            RepairOrderId = newRepairOrder.Id,
                            PartId = SelectedPartId.Value,
                            Quantity = 1,
                            PriceAtTheTime = part.SalePrice ?? 0
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }

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
                    .Include(a => a.Workstation)
                    .OrderBy(a => a.PlannedStart)
                    .ToListAsync();
            }

            // Pobieranie aut z bazy
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

            if (_context.Services != null)
            {
                var services = await _context.Services.ToListAsync();
                ServicesList = new SelectList(services, "Id", "Name");
            }

            if (_context.Tools != null)
            {
                var tools = await _context.Tools.ToListAsync();
                ToolsList = new SelectList(tools, "Id", "Name");
            }

            if (_context.Parts != null)
            {
                var parts = await _context.Parts.Where(p => p.StockQuantity > 0).ToListAsync();
                PartsList = new SelectList(parts, "Id", "Name");
            }
        }
    }
}
