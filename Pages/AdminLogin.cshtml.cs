using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Rzonca_Babik_FixCar4Us.Data;
using System.Linq;

namespace Rzonca_Babik_FixCar4Us.Pages
{
    public class AdminLoginModel : PageModel
    {
        private readonly AppDbContext _context;

        public AdminLoginModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AdminLoginForm Input { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var employee = _context.Employees.FirstOrDefault(e => e.Email == Input.Email);

            if (employee == null || employee.PasswordHash == null)
            {
                ModelState.AddModelError(string.Empty, "Nieprawidłowy email lub hasło.");
                return Page();
            }

            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Input.Password));
            var hash = System.BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();

            if (employee.PasswordHash != hash)
            {
                ModelState.AddModelError(string.Empty, "Nieprawidłowy email lub hasło.");
                return Page();
            }

            // Usunięcie starych ciasteczek logowania
            HttpContext.Response.Cookies.Delete("LoggedEmployeeId");
            HttpContext.Response.Cookies.Delete("LoggedCustomerId");

            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions { Expires = System.DateTime.Now.AddDays(1) };
            HttpContext.Response.Cookies.Append("LoggedEmployeeId", employee.Id.ToString(), cookieOptions);

            TempData["SuccessMessage"] = $"Zalogowano pomyślnie do panelu admina! Witaj, {employee.FirstName}.";
            return RedirectToPage("/Admin");
        }

        public class AdminLoginForm
        {
            [Required(ErrorMessage = "Email jest wymagany")]
            [EmailAddress(ErrorMessage = "Niepoprawny format adresu email")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Hasło jest wymagane")]
            [DataType(DataType.Password)]
            [Display(Name = "Hasło")]
            public string Password { get; set; }
        }
    }
}
