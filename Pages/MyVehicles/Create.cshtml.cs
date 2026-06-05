using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using System.Threading.Tasks;

namespace Rzonca_Babik_FixCar4Us.Pages.MyVehicles
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Vehicle NewVehicle { get; set; } = new Vehicle();

        public IActionResult OnGet()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("LoggedCustomerId"))
            {
                return RedirectToPage("/CustomerLogin");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!HttpContext.Request.Cookies.TryGetValue("LoggedCustomerId", out var cookieValue) || !int.TryParse(cookieValue, out int customerId))
            {
                return RedirectToPage("/CustomerLogin");
            }

            int maxVehicleId = 0;
            if (await _context.Vehicles.AnyAsync()) 
                maxVehicleId = await _context.Vehicles.MaxAsync(v => v.Id);
                
            NewVehicle.Id = maxVehicleId + 1;
            NewVehicle.CustomerId = customerId;
            
            _context.Vehicles.Add(NewVehicle);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
