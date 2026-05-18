using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rzonca_Babik_FixCar4Us.Pages;

public class VehiclesModel : PageModel
{
    private readonly AppDbContext _context;

    public VehiclesModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public async Task OnGetAsync()
    {
        // Pobieramy listę pojazdów z bazy danych
        Vehicles = await _context.Vehicles.ToListAsync();
    }
}
