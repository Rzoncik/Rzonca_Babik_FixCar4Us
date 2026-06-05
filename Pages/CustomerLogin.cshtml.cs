using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rzonca_Babik_FixCar4Us.Data;

namespace Rzonca_Babik_FixCar4Us.Pages
{
    public class CustomerLoginModel : PageModel
    {
        private readonly AppDbContext _context;

        public CustomerLoginModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LoginForm Input { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Szukamy klienta po adresie email
            var customer = _context.Customers.FirstOrDefault(c => c.Email == Input.Email);

            if (customer == null)
            {
                ModelState.AddModelError(string.Empty, "Nieprawidłowy email lub hasło.");
                return Page();
            }

            // Hashowanie podanego hasła do sprawdzenia (identycznie jak przy rejestracji)
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Input.Password));
            var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();

            // Porównanie hashy
            if (customer.PasswordHash != hash)
            {
                ModelState.AddModelError(string.Empty, "Nieprawidłowy email lub hasło.");
                return Page();
            }

            // Logowanie poprawne - z reguły tutaj zapisuje się sesję lub cookie.
            // Najpierw usuwamy stare ciasteczko, na wypadek gdyby ktoś był już zalogowany
            HttpContext.Response.Cookies.Delete("LoggedCustomerId");

            // Zapisujemy CustomerId w prostym ciasteczku (jako 2 rok studiów)
            var cookieOptions = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
            HttpContext.Response.Cookies.Append("LoggedCustomerId", customer.Id.ToString(), cookieOptions);

            TempData["SuccessMessage"] = $"Zalogowano pomyślnie! Witaj, {customer.FirstName}.";
            return RedirectToPage("/Index");
        }

        public class LoginForm
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
