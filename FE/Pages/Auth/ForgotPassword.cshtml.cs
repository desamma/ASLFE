using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FE.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ForgotPasswordModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var client = _httpClientFactory.CreateClient("Api");

            var response = await client.PostAsJsonAsync("api/auth/forgot-password", new { email = Input.Email });

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to process forgot password request. Please try again.";
                return Page();
            }

            TempData["Success"] = "If an account exists with this email, you will receive a password reset link shortly.";
            return RedirectToPage("Login");
        }
        catch
        {
            ErrorMessage = "An error occurred. Please try again later.";
            return Page();
        }
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
