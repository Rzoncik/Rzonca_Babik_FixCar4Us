using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System;
using System.Threading.Tasks;

namespace Rzonca_Babik_FixCar4Us.Pages
{
    public class BookAppointmentModel : PageModel
    {
        private readonly AppDbContext _context;

        public BookAppointmentModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int SelectedVehicleId { get; set; }
        
        [BindProperty]
        public string? ReportedIssues { get; set; }

        public IList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public Vehicle? PreSelectedVehicle { get; set; }

        [BindProperty]
        public int SelectedServiceId { get; set; }

        public SelectList ServicesList { get; set; } = default!;

        public bool IsSuccess { get; set; } = false;

        public async Task<IActionResult> OnGetAsync([FromQuery] int? vehicleId)
        {
            if (!HttpContext.Request.Cookies.TryGetValue("LoggedCustomerId", out var cookieValue) || !int.TryParse(cookieValue, out int customerId))
            {
                TempData["SuccessMessage"] = "Musisz być zalogowany, aby umówić wizytę.";
                return RedirectToPage("/CustomerLogin");
            }

            Vehicles = await _context.Vehicles.Where(v => v.CustomerId == customerId).ToListAsync();
            var services = await _context.Services.ToListAsync();
            ServicesList = new SelectList(services, "Id", "Name");
            
            if (vehicleId.HasValue && vehicleId.Value != 0)
            {
                SelectedVehicleId = vehicleId.Value;
                PreSelectedVehicle = Vehicles.FirstOrDefault(v => v.Id == SelectedVehicleId);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("LoggedCustomerId", out var cookieValue) || !int.TryParse(cookieValue, out int customerId))
            {
                return RedirectToPage("/CustomerLogin");
            }

            if (!ModelState.IsValid || SelectedVehicleId == 0)
            {
                Vehicles = await _context.Vehicles.Where(v => v.CustomerId == customerId).ToListAsync();
                var services = await _context.Services.ToListAsync();
                ServicesList = new SelectList(services, "Id", "Name");
                if (SelectedVehicleId == 0) ModelState.AddModelError("SelectedVehicleId", "Proszę wybrać pojazd.");
                return Page();
            }

            int vehicleId = SelectedVehicleId;

            int maxOrderId = 0;
            if (await _context.RepairOrders.AnyAsync()) maxOrderId = await _context.RepairOrders.MaxAsync(r => r.Id);

            var selectedService = await _context.Services.FindAsync(SelectedServiceId);
            string issueText = selectedService?.Name ?? "Wybrana usługa";
            
            if (!string.IsNullOrWhiteSpace(ReportedIssues))
            {
                issueText += " | Opis usterki: " + ReportedIssues;
            }

            var newOrder = new RepairOrder
            {
                Id = maxOrderId + 1,
                VehicleId = vehicleId,
                Status = "Oczekujące na termin",
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                ReportedIssues = issueText
            };

            _context.RepairOrders.Add(newOrder);
            await _context.SaveChangesAsync();

            if (selectedService != null)
            {
                int maxOrderServiceId = 0;
                if (await _context.OrderServices.AnyAsync()) maxOrderServiceId = await _context.OrderServices.MaxAsync(o => o.Id);
                
                _context.OrderServices.Add(new OrderService
                {
                    Id = maxOrderServiceId + 1,
                    RepairOrderId = newOrder.Id,
                    ServiceId = SelectedServiceId,
                    CustomerId = customerId,
                    LoggedHours = 0,
                    FinalPrice = 0
                });
                await _context.SaveChangesAsync();
            }

            IsSuccess = true;
            return Page();
        }
    }
}
