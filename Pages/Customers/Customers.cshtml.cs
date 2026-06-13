using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Pages;

public class CustomersModel : PageModel
{
    private readonly AppDbContext _context;

    public CustomersModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Customer> Customers { get; set; } = new List<Customer>();

    public async Task OnGetAsync()
    {
        // Pobieranie klientów i pojazdów z bazy
        Customers = await _context.Customers
            .Include(c => c.Vehicles)
            .ToListAsync();
    }
}
