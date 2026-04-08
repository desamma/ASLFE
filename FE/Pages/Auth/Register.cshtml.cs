using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FE.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Input.Password != Input.ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                var response = await client.PostAsJsonAsync("api/auth/register", Input);

                var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = result?.message ?? "Registration successful!";
                    return RedirectToPage("/Auth/Login");
                }

                ErrorMessage = result?.message ?? "Registration failed.";
                return Page();
            }
            catch
            {
                ErrorMessage = "Something went wrong.";
                return Page();
            }
        }

        public class InputModel
        {
            [Required]
            public string Username { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;

            [Required, Compare("Password")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public class RegisterResponse
        {
            public string message { get; set; } = string.Empty;
        }
    }
}
