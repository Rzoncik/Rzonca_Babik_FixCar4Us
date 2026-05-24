using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        // Pobieramy klientów i od razu ich pojazdy (Include)
        Customers = await _context.Customers
            .Include(c => c.Vehicles)
            .ToListAsync();
    }
}
