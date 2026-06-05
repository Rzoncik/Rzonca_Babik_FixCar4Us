using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Rzonca_Babik_FixCar4Us.Pages;

public class AdminModel : PageModel
{
    private readonly ILogger<AdminModel> _logger;

    public AdminModel(ILogger<AdminModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        if (!HttpContext.Request.Cookies.ContainsKey("LoggedEmployeeId"))
        {
            TempData["SuccessMessage"] = "Brak dostępu. Zaloguj się jako pracownik.";
            return RedirectToPage("/AdminLogin");
        }
        return Page();
    }
}
