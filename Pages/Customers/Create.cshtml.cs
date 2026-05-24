using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System.Threading.Tasks;

namespace Rzonca_Babik_FixCar4Us.Pages.Customers;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Customer Customer { get; set; } = default!;

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

        _context.Customers.Add(Customer);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Customers");
    }
}
