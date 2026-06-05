using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rzonca_Babik_FixCar4Us.Pages.MyVehicles
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Vehicle> Vehicles { get; set; } = default!;
        public bool IsFleet { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("LoggedCustomerId", out var cookieValue) || !int.TryParse(cookieValue, out int customerId))
            {
                TempData["SuccessMessage"] = "Musisz być zalogowany, aby przeglądać swoje pojazdy.";
                return RedirectToPage("/CustomerLogin");
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null && customer.IsFleet == 1)
            {
                IsFleet = true;
            }

            Vehicles = await _context.Vehicles
                .Where(v => v.CustomerId == customerId)
                .ToListAsync();

            return Page();
        }
    }
}
