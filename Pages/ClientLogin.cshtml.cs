using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rzonca_Babik_FixCar4Us.Models;
using System.Security.Cryptography;
using System.Text;
using Rzonca_Babik_FixCar4Us.Data;

namespace Rzonca_Babik_FixCar4Us.Pages
{
    public class ClientLoginModel : PageModel
    {
        private readonly AppDbContext _context;

        public ClientLoginModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ClientLoginForm Input { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Proste hashowanie SHA256 (poziom drugiego roku studiów)
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Input.Password));
            var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();

            // Generujemy nowe ID dla klienta (lub używamy autoinkrementacji, jeśli jest ustawiona)
            int newId = _context.Customers.Any() ? _context.Customers.Max(c => c.Id) + 1 : 1;

            var newCustomer = new Customer
            {
                Id = newId,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                PhoneNumber = Input.PhoneNumber,
                Email = Input.Email,
                PasswordHash = hash
            };

            _context.Customers.Add(newCustomer);
            _context.SaveChanges();

            // Przekierowanie po udanej rejestracji (na stronę główną)
            return RedirectToPage("/Index");
        }

        public class ClientLoginForm
        {
            [Required(ErrorMessage = "Imię jest wymagane")]
            [Display(Name = "Imię")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Nazwisko jest wymagane")]
            [Display(Name = "Nazwisko")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Numer telefonu jest wymagany")]
            [Display(Name = "Numer telefonu")]
            public int? PhoneNumber { get; set; }

            [Required(ErrorMessage = "Email jest wymagany")]
            [EmailAddress(ErrorMessage = "Niepoprawny format adresu email")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Hasło jest wymagane")]
            [DataType(DataType.Password)]
            [Display(Name = "Hasło")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Powtórz hasło jest wymagane")]
            [DataType(DataType.Password)]
            [Display(Name = "Powtórz hasło")]
            [Compare("Password", ErrorMessage = "Hasła muszą być identyczne")]
            public string ConfirmPassword { get; set; }
        }
    }
}
