using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rzonca_Babik_FixCar4Us.Pages
{
    public class FleetBookAppointmentModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly Services.RepairPricingEngine _pricingEngine;

        public FleetBookAppointmentModel(AppDbContext context, Services.RepairPricingEngine pricingEngine)
        {
            _context = context;
            _pricingEngine = pricingEngine;
        }

        public IList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public SelectList ServicesList { get; set; } = default!;

        [BindProperty]
        public List<int> SelectedVehicles { get; set; } = new List<int>();

        public bool IsSuccess { get; set; } = false;
        public List<string> SuccessMessages { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("LoggedCustomerId", out var cookieValue) || !int.TryParse(cookieValue, out int customerId))
            {
                TempData["SuccessMessage"] = "Musisz być zalogowany, aby umówić wizytę flotową.";
                return RedirectToPage("/CustomerLogin");
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null || customer.IsFleet == 0)
            {
                TempData["SuccessMessage"] = "Ta strona jest dostępna tylko dla klientów flotowych.";
                return RedirectToPage("/MyVehicles/Index");
            }

            Vehicles = await _context.Vehicles.Where(v => v.CustomerId == customerId).ToListAsync();
            var services = await _context.Services.ToListAsync();
            ServicesList = new SelectList(services, "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("LoggedCustomerId", out var cookieValue) || !int.TryParse(cookieValue, out int customerId))
            {
                return RedirectToPage("/CustomerLogin");
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null || customer.IsFleet == 0)
            {
                return RedirectToPage("/Index");
            }

            if (SelectedVehicles == null || !SelectedVehicles.Any())
            {
                Vehicles = await _context.Vehicles.Where(v => v.CustomerId == customerId).ToListAsync();
                var services = await _context.Services.ToListAsync();
                ServicesList = new SelectList(services, "Id", "Name");
                ModelState.AddModelError("", "Proszę zaznaczyć przynajmniej jeden pojazd.");
                return Page();
            }

            int maxOrderId = 0;
            if (await _context.RepairOrders.AnyAsync()) maxOrderId = await _context.RepairOrders.MaxAsync(r => r.Id);

            int maxOrderServiceId = 0;
            if (await _context.OrderServices.AnyAsync()) maxOrderServiceId = await _context.OrderServices.MaxAsync(o => o.Id);

            var allServices = await _context.Services.ToListAsync();
            
            var rnd = new Random();
            int carIndex = 0;

            foreach (var vehicleId in SelectedVehicles)
            {
                string serviceIdStr = Request.Form[$"ServiceForVehicle_{vehicleId}"];
                if (int.TryParse(serviceIdStr, out int serviceId))
                {
                    var selectedService = allServices.FirstOrDefault(s => s.Id == serviceId);
                    var vehicle = Vehicles.FirstOrDefault(v => v.Id == vehicleId) ?? await _context.Vehicles.FindAsync(vehicleId);
                    string vehicleName = vehicle != null ? $"{vehicle.Model} ({vehicle.LicensePlate})" : $"Pojazd ID {vehicleId}";
                    string issueText = selectedService?.Name ?? "Wizyta flotowa";

                    // ========================================================
                    // Logika wyceny flotowej (Pricing Engine)
                    // ========================================================
                    var activeDecorators = new List<string>();
                    
                    // 1. Zniżka flotowa progresywna: każde kolejne auto obniża cenę o 2%
                    double discountPercent = carIndex * 0.02;
                    if (discountPercent > 0)
                    {
                        activeDecorators.Add($"FleetDiscount:{discountPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                    }

                    // 2. Randomizacja: 20% szans na wystąpienie nieprzewidzianych trudności w naprawie (np. urwane śruby)
                    bool hasDifficultAccess = false;
                    if (rnd.NextDouble() < 0.20)
                    {
                        activeDecorators.Add("DifficultAccess");
                        hasDifficultAccess = true;
                    }

                    // Symulacja ceny bazowej
                    var strategy = new Services.FlatRatePricingStrategy();
                    double baseRate = selectedService?.BaseHourlyRate ?? 150.0;
                    
                    // Kalkulacja za pomocą Silnika Wycen
                    var estimatedCost = _pricingEngine.CreatePricing(0, baseRate, 0, 0, baseRate * 2, strategy, activeDecorators);

                    issueText += " (Zgłoszenie flotowe). Wycena szacunkowa:\n" + estimatedCost.GetDescription();
                    
                    SuccessMessages.Add($"{vehicleName}: Wyceniono na <strong>{estimatedCost.GetTotalCost():C}</strong>. " +
                                        $"{(discountPercent > 0 ? $"<span class='badge bg-success'>Rabat {discountPercent * 100}%</span>" : "")} " +
                                        $"{(hasDifficultAccess ? "<span class='badge bg-danger'>⚠️ Trudny dostęp!</span>" : "")}");

                    maxOrderId++;
                    var newOrder = new RepairOrder
                    {
                        Id = maxOrderId,
                        VehicleId = vehicleId,
                        Status = "Oczekujące na termin",
                        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                        ReportedIssues = issueText
                    };
                    _context.RepairOrders.Add(newOrder);

                    if (selectedService != null)
                    {
                        maxOrderServiceId++;
                        _context.OrderServices.Add(new OrderService
                        {
                            Id = maxOrderServiceId,
                            RepairOrderId = newOrder.Id,
                            ServiceId = serviceId,
                            CustomerId = customerId,
                            LoggedHours = 0,
                            FinalPrice = estimatedCost.GetTotalCost()
                        });
                    }
                    
                    carIndex++;
                }
            }

            await _context.SaveChangesAsync();
            IsSuccess = true;
            return Page();
        }
    }
}
