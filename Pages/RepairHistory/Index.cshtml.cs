using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Pages.RepairHistory
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<RepairOrder> CompletedOrders { get; set; } = default!;

        public async Task OnGetAsync()
        {
            CompletedOrders = await _context.RepairOrders
                .Include(r => r.Vehicle)
                .ThenInclude(v => v.Customer)
                .Where(r => r.Status == "Zakończone" || r.Status == "Opłacone")
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();
        }
    }
}
