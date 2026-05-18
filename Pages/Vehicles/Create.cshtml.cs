using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System.Threading.Tasks;

namespace Rzonca_Babik_FixCar4Us.Pages.Vehicles;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Vehicle Vehicle { get; set; } = default!;

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Zapisujemy nowy pojazd do bazy
        _context.Vehicles.Add(Vehicle);
        await _context.SaveChangesAsync();

        // Przekierowanie z powrotem do głównej listy pojazdów
        return RedirectToPage("/Vehicles");
    }
}
