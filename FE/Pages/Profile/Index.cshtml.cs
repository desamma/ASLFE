using BussinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;

namespace FE.Pages.Profile;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public UserProfileModel UserProfile { get; set; } = new();

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "edit")]
    public bool IsEditing { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.GetAsync($"api/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to load profile.";
                return Page();
            }

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var content = await response.Content.ReadAsStringAsync();

            User? user = null;

            // Deserialize the response which has { message: "...", data: [...] } structure
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
            if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
            {
                user = JsonSerializer.Deserialize<User>(dataProperty.GetRawText(), jsonOptions);
            }

            if (user != null)
            {
                UserProfile = new UserProfileModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    UserDOB = user.UserDOB,
                    Gender = user.Gender,
                    CurrencyAmount = user.CurrencyAmount,
                    UserAvatar = user.UserAvatar
                };
            }

            return Page();
        }
        catch
        {
            ErrorMessage = "An error occurred while loading your profile.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!IsEditing)
        {
            ErrorMessage = "Editing is disabled.";
            return Page();
        }

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("Api");
            var user = await client.GetFromJsonAsync<User>($"api/user/{userId}");

            if (user == null)
                return RedirectToPage("/Auth/Login");

            string? avatarPath = UserProfile.UserAvatar;

            // Handle avatar upload
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (AvatarFile.Length > maxFileSize)
                {
                    ErrorMessage = "Avatar file must be less than 5MB.";
                    return Page();
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(AvatarFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ErrorMessage = "Only image files are allowed (jpg, jpeg, png, gif, webp).";
                    return Page();
                }

                // Delete old avatar from Firebase Storage if it exists
                if (!string.IsNullOrEmpty(user.UserAvatar))
                {
                    try
                    {
                        var deleteRequest = new DeleteMultipleFilesRequest
                        {
                            FilePaths = allowedExtensions
                                .Select(ext => $"images/userAvatar/{userId}{ext}")
                                .ToList()
                        };

                        var deleteContent = new StringContent(JsonSerializer.Serialize(deleteRequest), System.Text.Encoding.UTF8, "application/json");
                        var deleteMessage = new HttpRequestMessage(HttpMethod.Delete, "api/firebasestorage/delete-multiple")
                        {
                            Content = deleteContent
                        };
                        await client.SendAsync(deleteMessage);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with new upload
                        Console.WriteLine($"Error deleting old avatar: {ex.Message}");
                    }
                }

                // Upload new avatar to Firebase Storage
                using var formContent = new MultipartFormDataContent();
                using var fileStream = AvatarFile.OpenReadStream();

                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(AvatarFile.ContentType);
                var avatarFileName = $"{userId}{fileExtension}";
                formContent.Add(streamContent, "file", avatarFileName);
                formContent.Add(new StringContent("images/userAvatar/"), "folderPath");

                var uploadResponse = await client.PostAsync("api/firebasestorage/upload", formContent);

                if (!uploadResponse.IsSuccessStatusCode)
                {
                    ErrorMessage = "Failed to upload avatar.";
                    return Page();
                }

                var uploadContent = await uploadResponse.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var uploadResult = JsonSerializer.Deserialize<UploadResponse>(uploadContent, jsonOptions);

                if (uploadResult?.Url != null)
                {
                    avatarPath = uploadResult.Url;
                }
            }

            var request = new UserUpdateRequest
            {
                UserName = UserProfile.UserName,
                Email = UserProfile.Email,
                PhoneNumber = UserProfile.PhoneNumber ?? string.Empty,
                Gender = UserProfile.Gender,
                UserAvatar = avatarPath ?? string.Empty,
                UserDOB = UserProfile.UserDOB,
                CurrencyAmount = user.CurrencyAmount,
                IsBanned = user.IsBanned,
                PityCounter = user.PityCounter,
            };

            var response = await client.PutAsJsonAsync($"api/user/{userId}", request);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Profile updated successfully!";
                return await OnGetAsync();
            }
            else
            {
                ErrorMessage = "Failed to update profile.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while updating your profile: {ex.Message}";
            return Page();
        }
    }

    public class UserProfileModel
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(11, ErrorMessage = "The phone number must be at most 11 digits.")]
        [RegularExpression(@"^\d{1,11}$", ErrorMessage = "The phone number must only contain digits")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateOnly? UserDOB { get; set; }

        [Display(Name = "Gender")]
        [Range(0, 3)]
        public byte Gender { get; set; } // 0: male, 1: female, 3: other

        [Display(Name = "Currency Amount")]
        public decimal CurrencyAmount { get; set; }

        public string? UserAvatar { get; set; }
    }

    public class UserUpdateRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateOnly? UserDOB { get; set; }
        public byte Gender { get; set; }
        public string UserAvatar { get; set; }
        public bool IsBanned { get; set; }
        public decimal CurrencyAmount { get; set; }
        public int PityCounter { get; set; }
    }

    public class UploadResponse
    {
        public string? Url { get; set; }
        public string? FileName { get; set; }
    }

    public class DeleteMultipleFilesRequest
    {
        public List<string> FilePaths { get; set; } = new();
    }
}
