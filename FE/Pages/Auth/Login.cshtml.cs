using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FE.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
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

        try
        {
            var client = _httpClientFactory.CreateClient("Api");

            // 1. Call external API
            var response = await client.PostAsJsonAsync("api/auth/login", Input);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                ErrorMessage = "Invalid login response.";
                return Page();
            }

            // 2. Store JWT in Session
            HttpContext.Session.SetString("AccessToken", loginResponse.Token);

            // 3. Create Claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, loginResponse.UserId.ToString()),
                new(ClaimTypes.Name, loginResponse.Username),
                new(ClaimTypes.Email, loginResponse.Email ?? ""),
                new("Avatar", loginResponse.Avatar ?? ""),
                new("Gender", loginResponse.Gender.ToString())
            };

            if (loginResponse.Roles != null)
            {
                foreach (var role in loginResponse.Roles)
                {
                    claims.Add(new(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
                });

            return RedirectToPage("/Index");
        }
        catch
        {
            ErrorMessage = "Login failed. Try again.";
            return Page();
        }
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public byte Gender { get; set; }
        public List<string>? Roles { get; set; }
    }
}
