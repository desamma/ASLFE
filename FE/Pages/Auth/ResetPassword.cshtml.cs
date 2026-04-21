using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FE.Pages.Auth;

public class ResetPasswordModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ResetPasswordModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;
    public bool IsTokenValid { get; set; } = true;

    public void OnGet(Guid userId, string token)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
        {
            IsTokenValid = false;
            ErrorMessage = "Invalid reset password request.";
            return;
        }

        Input.UserId = userId;
        Input.Token = token;
        IsTokenValid = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Input.UserId == Guid.Empty || string.IsNullOrWhiteSpace(Input.Token))
        {
            IsTokenValid = false;
            ErrorMessage = "Invalid reset password request.";
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("Api");

            var resetRequest = new
            {
                userId = Input.UserId,
                token = Input.Token,
                newPassword = Input.NewPassword
            };

            var response = await client.PostAsJsonAsync("api/auth/reset-password", resetRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = "Failed to reset password. Please try again or request a new reset link.";
                IsTokenValid = false;
                return Page();
            }

            TempData["Success"] = "Your password has been successfully reset. You can now login with your new password.";
            return RedirectToPage("Login");
        }
        catch
        {
            ErrorMessage = "An error occurred while resetting your password. Please try again.";
            return Page();
        }
    }

    public class InputModel
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
