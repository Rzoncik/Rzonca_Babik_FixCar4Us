using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Pages.Catalog
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Service> Services { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Pobranie danych z bazy
            if (_context.Services != null)
            {
                Services = await _context.Services.ToListAsync();
            }
        }
    }
}
