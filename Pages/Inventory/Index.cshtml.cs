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

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task OnGetAsync()
        {
            // Pobranie aktualnego stanu magazynowego wszystkich części
            if (_context.Parts != null)
            {
                var query = _context.Parts.AsQueryable();

                if (!string.IsNullOrEmpty(SearchString))
                {
                    var lowerSearch = SearchString.ToLower();
                    query = query.Where(p => 
                        (p.Name != null && p.Name.ToLower().Contains(lowerSearch)) ||
                        (p.PartNumber != null && p.PartNumber.ToLower().Contains(lowerSearch))
                    );
                }

                Parts = await query.ToListAsync();
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

        public async Task<IActionResult> OnPostDeleteAsync(int partId)
        {
            var part = await _context.Parts
                .Include(p => p.OrderParts)
                .FirstOrDefaultAsync(p => p.Id == partId);
                
            if (part != null)
            {
                if ((part.StockQuantity ?? 0) == 0)
                {
                    if (part.OrderParts != null && part.OrderParts.Any())
                    {
                        TempData["ErrorMessage"] = "Nie można usunąć części, ponieważ widnieje w historii zleceń (jest używana w systemie).";
                    }
                    else
                    {
                        _context.Parts.Remove(part);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Część '{part.Name}' została usunięta ze słownika.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Nie można usunąć części. Stan magazynowy musi wynosić 0.";
                }
            }
            return RedirectToPage("./Index");
        }
    }
}
