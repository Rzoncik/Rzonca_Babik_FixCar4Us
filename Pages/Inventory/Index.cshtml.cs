using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rzonca_Babik_FixCar4Us.Pages.Inventory
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        // Lista do wyświetlenia stanu magazynowego
        public IList<Part> Parts { get; set; } = default!;

        // Pola zbindowane do formularza
        [BindProperty]
        public int PartId { get; set; }
        
        [BindProperty]
        public int QuantityChange { get; set; }

        public async Task OnGetAsync()
        {
            // Pobranie aktualnego stanu magazynowego wszystkich części
            if (_context.Parts != null)
            {
                Parts = await _context.Parts.ToListAsync();
            }
        }

        // Metoda obsługująca formularz aktualizacji stanu
        public async Task<IActionResult> OnPostUpdateStockAsync()
        {
            var part = await _context.Parts.FindAsync(PartId);
            
            if (part != null)
            {
                // Aktualizacja ilości - wartość może być ujemna (rozchód) lub dodatnia (przychód)
                part.StockQuantity = (part.StockQuantity ?? 0) + QuantityChange;
                
                // Zabezpieczenie na wypadek zmniejszenia poniżej 0
                if (part.StockQuantity < 0)
                {
                    part.StockQuantity = 0;
                }

                await _context.SaveChangesAsync();
            }

            // Przeładowanie strony aby zobaczyć zmienione dane
            return RedirectToPage("./Index");
        }
    }
}
