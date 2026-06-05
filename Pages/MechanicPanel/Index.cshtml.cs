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

            // Odczytujemy wartości bezpośrednio z formularza, pomijając potencjalne błędy bindowania
            int partId = 0;
            int quantity = 0;
            
            if (Request.Form.ContainsKey("PartIdToConsume") && int.TryParse(Request.Form["PartIdToConsume"], out int pId))
            {
                partId = pId;
            }
            if (Request.Form.ContainsKey("QuantityToConsume") && int.TryParse(Request.Form["QuantityToConsume"], out int q))
            {
                quantity = q;
            }

            // Używamy Fasady, która za jednym zamachem: loguje godziny, zużywa części i zmienia status
            var parts = new List<PartUsageDto>();
            if (partId > 0 && quantity > 0)
            {
                var part = await _context.Parts.FindAsync(partId);
                if (part != null)
                {
                    parts.Add(new PartUsageDto
                    {
                        PartId = part.Id,
                        Quantity = quantity,
                        CurrentPrice = part.SalePrice ?? 0
                    });
                }
            }

            // Obliczanie przepracowanych godzin na podstawie terminów wizyty
            double calculatedHours = 1; // domyślna wartość
            var appointment = await _context.Appointments
                .Where(a => a.VehicleId == order.VehicleId)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();

            if (appointment != null && !string.IsNullOrEmpty(appointment.PlannedStart) && !string.IsNullOrEmpty(appointment.PlannedEnd))
            {
                if (DateTime.TryParse(appointment.PlannedStart, out DateTime start) && DateTime.TryParse(appointment.PlannedEnd, out DateTime end))
                {
                    calculatedHours = (end - start).TotalHours;
                    if (calculatedHours < 0.5) calculatedHours = 0.5; // minimum pół godziny
                }
            }

            int serviceId = order.OrderServices.FirstOrDefault()?.ServiceId ?? 1;

            // Jeden strzał z Fasady zamiast wielkiego spaghetti
            _facade.LogWorkAndCompleteStage(order.Id, serviceId, calculatedHours, parts, newValidStatus);

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
            
            // Po zaplanowaniu przenosimy do statusu "Przyjęte"
            order.Status = "Przyjęte";
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        private async Task LoadActiveOrders()
        {
            ActiveOrders = await _context.RepairOrders
                .Include(r => r.Vehicle)
                .Include(r => r.OrderServices)
                .ThenInclude(os => os.Service)
                .Where(r => r.Status != "Gotowe do odbioru" && r.Status != "Zakończone") // Pokazuj tylko niezakończone
                .ToListAsync();

            var services = await _context.Services.ToListAsync();
            ServicesList = new SelectList(services, "Id", "Name");

            var parts = await _context.Parts.Where(p => p.StockQuantity > 0).ToListAsync();
            PartsList = new SelectList(parts, "Id", "Name");

            var tools = await _context.Tools.ToListAsync();
            ViewData["ToolsList"] = new SelectList(tools, "Id", "Name");

            var workstations = await _context.Workstations.ToListAsync();
            ViewData["WorkstationsList"] = new SelectList(workstations, "Id", "Name");
        }
    }
}
