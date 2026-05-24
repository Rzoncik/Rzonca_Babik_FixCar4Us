using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rzonca_Babik_FixCar4Us.Pages.Vehicles
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Vehicle Vehicle { get; set; } = default!;
        
        // Zlecenia napraw powiązane z tym pojazdem
        public IList<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
        
        // Historia badań technicznych pojazdu
        public IList<TechnicalInspection> TechnicalInspections { get; set; } = new List<TechnicalInspection>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Pobieramy dane wybranego pojazdu
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(m => m.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }
            
            Vehicle = vehicle;

            // Pobranie zleceń napraw wraz z pod-tabelą wymienionych części (Include/ThenInclude)
            if (_context.RepairOrders != null)
            {
                RepairOrders = await _context.RepairOrders
                    .Include(r => r.OrderParts)
                    .ThenInclude(op => op.Part)
                    .Where(r => r.VehicleId == id)
                    .OrderByDescending(r => r.CreatedAt) // Od najnowszych napraw
                    .ToListAsync();
            }

            // Pobranie zapisanych badań technicznych
            if (_context.TechnicalInspections != null)
            {
                TechnicalInspections = await _context.TechnicalInspections
                    .Where(t => t.VehicleId == id)
                    .OrderByDescending(t => t.InspectionDate) // Od najnowszych badań
                    .ToListAsync();
            }

            return Page();
        }
    }
}
