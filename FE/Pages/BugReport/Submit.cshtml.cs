using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FE.Pages.BugReport
{
    [Authorize]
    public class SubmitModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SubmitModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                var bugReportRequest = new
                {
                    title = Input.Title,
                    description = Input.Description,
                    steps = Input.Steps,
                    expectedBehavior = Input.ExpectedBehavior,
                    actualBehavior = Input.ActualBehavior,
                    severity = Input.Severity ?? "Medium"
                };

                var response = await client.PostAsJsonAsync("api/bugreport/submit", bugReportRequest);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Bug report submitted successfully!";
                    Input = new InputModel();
                    return Page();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent, jsonOptions);
                    if (errorResponse.ValueKind == JsonValueKind.Object && errorResponse.TryGetProperty("message", out var messageProperty))
                    {
                        ErrorMessage = messageProperty.GetString() ?? "Failed to submit bug report";
                    }
                    else
                    {
                        ErrorMessage = "Failed to submit bug report. Please try again.";
                    }
                }
                catch
                {
                    ErrorMessage = "Failed to submit bug report. Please try again.";
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Title is required")]
            [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Description is required")]
            [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
            public string Description { get; set; } = string.Empty;

            [MaxLength(500, ErrorMessage = "Steps cannot exceed 500 characters")]
            public string? Steps { get; set; }

            [MaxLength(200, ErrorMessage = "Expected behavior cannot exceed 200 characters")]
            public string? ExpectedBehavior { get; set; }

            [MaxLength(200, ErrorMessage = "Actual behavior cannot exceed 200 characters")]
            public string? ActualBehavior { get; set; }

            [MaxLength(50, ErrorMessage = "Severity cannot exceed 50 characters")]
            public string? Severity { get; set; }
        }
    }
}
