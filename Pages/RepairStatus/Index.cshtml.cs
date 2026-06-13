using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Pages.RepairStatus
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<RepairOrder> CustomerOrders { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("LoggedCustomerId", out var cookieValue) || !int.TryParse(cookieValue, out int customerId))
            {
                TempData["SuccessMessage"] = "Musisz być zalogowany, aby sprawdzić status naprawy.";
                return RedirectToPage("/CustomerLogin");
            }

            var query = _context.RepairOrders
                .Include(r => r.Vehicle)
                .Include(r => r.Employee)
                .Include(r => r.OrderServices)
                .ThenInclude(os => os.Service)
                .Include(r => r.OrderParts)
                .Where(r => r.Vehicle != null && r.Vehicle.CustomerId == customerId);

            if (!string.IsNullOrEmpty(SearchString))
            {
                var lowerSearch = SearchString.ToLower();
                var searchNumberString = SearchString.Replace("#", "").Trim();
                bool isNumeric = int.TryParse(searchNumberString, out int searchId);

                query = query.Where(r => 
                    (isNumeric && r.Id == searchId) ||
                    (r.Status != null && r.Status.ToLower().Contains(lowerSearch)) ||
                    (r.Vehicle != null && ((r.Vehicle.Model != null && r.Vehicle.Model.ToLower().Contains(lowerSearch)) || (r.Vehicle.LicensePlate != null && r.Vehicle.LicensePlate.ToLower().Contains(lowerSearch)))) ||
                    (r.ReportedIssues != null && r.ReportedIssues.ToLower().Contains(lowerSearch))
                );
            }

            CustomerOrders = await query.OrderByDescending(r => r.Id).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostPayAsync(int orderId)
        {
            if (!HttpContext.Request.Cookies.TryGetValue("LoggedCustomerId", out var cookieValue) || !int.TryParse(cookieValue, out int customerId))
            {
                return RedirectToPage("/CustomerLogin");
            }

            var order = await _context.RepairOrders
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == orderId && r.Vehicle != null && r.Vehicle.CustomerId == customerId);

            if (order != null && order.Status == "Zakończone")
            {
                order.Status = "Opłacone";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Zlecenie #{order.Id} zostało opłacone pomyślnie. Dziękujemy!";
            }

            return RedirectToPage();
        }
    }
}
