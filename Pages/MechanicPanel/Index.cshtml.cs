using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using Rzonca_Babik_FixCar4Us.Services;

namespace Rzonca_Babik_FixCar4Us.Pages.MechanicPanel
{
    public class CalendarEventDto
    {
        public string Title { get; set; } = string.Empty;
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IMechanicPanelFacade _facade;
        private readonly IWorkshopMediator _mediator;

        public IndexModel(AppDbContext context, IMechanicPanelFacade facade, IWorkshopMediator mediator)
        {
            _context = context;
            _facade = facade;
            _mediator = mediator;
        }

        public IList<RepairOrder> ActiveOrders { get; set; } = default!;
        public IList<CalendarEventDto> CalendarEvents { get; set; } = new List<CalendarEventDto>();

        public SelectList ServicesList { get; set; } = default!;
        public SelectList PartsList { get; set; } = default!;

        [BindProperty]
        public int SelectedOrderId { get; set; }

        [BindProperty]
        public int PartIdToConsume { get; set; }

        [BindProperty]
        public int QuantityToConsume { get; set; }

        [BindProperty]
        public string PlannedStart { get; set; } = string.Empty;

        [BindProperty]
        public string PlannedEnd { get; set; } = string.Empty;

        [BindProperty]
        public int? SelectedToolId { get; set; }

        [BindProperty]
        public int SelectedWorkstationId { get; set; }

        [BindProperty]
        public int SelectedEmployeeId { get; set; }

        public async Task OnGetAsync()
        {
            await LoadActiveOrders();
        }

        public async Task<IActionResult> OnPostAdvanceStageAsync()
        {
            var order = await _context.RepairOrders
                .Include(r => r.OrderServices)
                .FirstOrDefaultAsync(r => r.Id == SelectedOrderId);
            if (order == null) return RedirectToPage();

            // Używamy wzorca State, by ustalić prawidłowy następny status
            var stateContext = new RepairOrderContext(order);
            stateContext.NextState();
            string newValidStatus = stateContext.GetStatusName();

            if (Request.Form.ContainsKey("AdditionalFee") && double.TryParse(Request.Form["AdditionalFee"].ToString().Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double fee))
            {
                order.AdditionalFee = fee;
            }
            if (Request.Form.ContainsKey("DifficultyDescription"))
            {
                order.DifficultyDescription = Request.Form["DifficultyDescription"];
            }

            // Używamy Fasady, która za jednym zamachem: loguje godziny, zużywa części i zmienia status
            var parts = new List<PartUsageDto>();
            if (PartIdToConsume >= 0 && QuantityToConsume > 0)
            {
                var part = await _context.Parts.FindAsync(PartIdToConsume);
                if (part != null)
                {
                    double price = part.SalePrice ?? 0;
                    if (Request.Form.ContainsKey("IsReplacementPart") && Request.Form["IsReplacementPart"] == "on")
                    {
                        price = price * 0.5;
                    }

                    parts.Add(new PartUsageDto
                    {
                        PartId = part.Id,
                        Quantity = QuantityToConsume,
                        CurrentPrice = price
                    });
                }
            }

            // Czas pracy będzie teraz automatycznie obliczany w Fasadzie na podstawie historii logów
            double calculatedHours = 0;

            int serviceId = order.OrderServices.FirstOrDefault()?.ServiceId ?? 1;

            // Jeden strzał z Fasady zamiast wielkiego spaghetti
            _facade.LogWorkAndCompleteStage(order.Id, serviceId, calculatedHours, parts, newValidStatus);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRollbackStageAsync()
        {
            var order = await _context.RepairOrders
                .Include(r => r.OrderServices)
                .FirstOrDefaultAsync(r => r.Id == SelectedOrderId);
            if (order == null || order.Status == "Zakończone" || order.Status == "Opłacone")
                return RedirectToPage();

            // Używamy wzorca State, by cofnąć status
            var stateContext = new RepairOrderContext(order);
            stateContext.PreviousState();
            string newValidStatus = stateContext.GetStatusName();

            // Fasada aktualizuje status w logach.
            // Przy cofaniu nie dodajemy części ani czasu
            int serviceId = order.OrderServices.FirstOrDefault()?.ServiceId ?? 1;
            _facade.LogWorkAndCompleteStage(order.Id, serviceId, 0, new List<PartUsageDto>(), newValidStatus);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostScheduleAsync()
        {
            var order = await _context.RepairOrders.FindAsync(SelectedOrderId);
            if (order == null) return RedirectToPage();

            int maxId = 0;
            if (await _context.Appointments.AnyAsync()) maxId = await _context.Appointments.MaxAsync(a => a.Id);

            var appointment = new Appointment
            {
                Id = maxId + 1,
                VehicleId = order.VehicleId,
                PlannedStart = PlannedStart,
                PlannedEnd = PlannedEnd,
                ToolId = SelectedToolId,
                WorkstationId = SelectedWorkstationId,
                EmployeeId = SelectedEmployeeId,
                Status = "Zaplanowane"
            };

            // Sprawdzamy, czy można zaplanować wizytę (Mediator)
            if (!_mediator.TryScheduleAppointment(appointment, out string errorMessage))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                await LoadActiveOrders();
                return Page();
            }

            _context.Appointments.Add(appointment);

            // Po zaplanowaniu przenosimy do statusu "Przyjęte" i przypisujemy pracownika
            order.Status = "Przyjęte";
            order.EmployeeId = SelectedEmployeeId;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        private async Task LoadActiveOrders()
        {
            ActiveOrders = await _context.RepairOrders
                .Include(r => r.Vehicle)
                .Include(r => r.Employee)
                .Include(r => r.OrderServices)
                .ThenInclude(os => os.Service)
                .Where(r => r.Status != "Gotowe do odbioru" && r.Status != "Zakończone" && r.Status != "Opłacone") // Pokazuj tylko niezakończone i nieopłacone
                .ToListAsync();

            var appointments = await _context.Appointments
                .Where(a => a.PlannedStart != null && a.PlannedEnd != null)
                .ToListAsync();

            CalendarEvents.Clear();
            foreach (var app in appointments)
            {
                string licensePlate = "Brak";
                if (app.VehicleId.HasValue)
                {
                    var vehicle = await _context.Vehicles.FindAsync(app.VehicleId.Value);
                    if (vehicle != null) licensePlate = vehicle.LicensePlate ?? "Brak";
                }

                CalendarEvents.Add(new CalendarEventDto
                {
                    Title = $"Naprawa: {licensePlate} - {app.Status}",
                    Start = app.PlannedStart!,
                    End = app.PlannedEnd!,
                    Color = app.Status == "Zaplanowane" ? "#0d6efd" : (app.Status == "W trakcie" ? "#ffc107" : "#198754")
                });
            }

            var services = await _context.Services.ToListAsync();
            ServicesList = new SelectList(services, "Id", "Name");

            var parts = await _context.Parts.Where(p => p.StockQuantity > 0).ToListAsync();
            PartsList = new SelectList(parts, "Id", "Name");

            var tools = await _context.Tools.ToListAsync();
            ViewData["ToolsList"] = new SelectList(tools, "Id", "Name");

            var workstations = await _context.Workstations.ToListAsync();
            ViewData["WorkstationsList"] = new SelectList(workstations, "Id", "Name");

            var employees = await _context.Employees.ToListAsync();
            ViewData["EmployeesList"] = new SelectList(employees.Select(e => new {
                Id = e.Id,
                FullName = e.FirstName + " " + e.LastName + " (" + e.Speciality + ")"
            }), "Id", "FullName");
        }
    }
}
