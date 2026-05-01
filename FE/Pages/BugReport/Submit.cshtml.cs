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
                    Title = Input.Title.Trim(),
                    Description = Input.Description.Trim(),
                    Steps = string.IsNullOrWhiteSpace(Input.Steps) ? null : Input.Steps.Trim(),
                    ExpectedBehavior = string.IsNullOrWhiteSpace(Input.ExpectedBehavior) ? null : Input.ExpectedBehavior.Trim(),
                    ActualBehavior = string.Empty,
                    Severity = string.IsNullOrWhiteSpace(Input.Severity) ? "Medium" : Input.Severity.Trim()
                };

                var response = await client.PostAsJsonAsync("api/bugreport/submit", bugReportRequest);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Bug report submitted successfully!";
                    Input = new InputModel();
                    // Success message will be cleared by JavaScript after 4 seconds
                    return Page();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = ExtractErrorMessage(errorContent);

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        private static string ExtractErrorMessage(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return "Failed to submit bug report. Please try again.";

            try
            {
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, jsonOptions);

                if (errorResponse.ValueKind != JsonValueKind.Object)
                    return "Failed to submit bug report. Please try again.";

                if (errorResponse.TryGetProperty("message", out var messageProperty))
                {
                    var message = messageProperty.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                        return message;
                }

                if (errorResponse.TryGetProperty("errors", out var errorsProperty))
                {
                    if (errorsProperty.ValueKind == JsonValueKind.Array)
                    {
                        var messages = errorsProperty
                            .EnumerateArray()
                            .Select(error => error.GetString())
                            .Where(message => !string.IsNullOrWhiteSpace(message))
                            .Take(3)
                            .ToList();

                        if (messages.Count > 0)
                            return string.Join(" ", messages!);
                    }
                    else if (errorsProperty.ValueKind == JsonValueKind.Object)
                    {
                        var messages = new List<string>();

                        foreach (var property in errorsProperty.EnumerateObject())
                        {
                            if (property.Value.ValueKind == JsonValueKind.Array)
                            {
                                messages.AddRange(property.Value.EnumerateArray()
                                    .Select(error => error.GetString())
                                    .Where(message => !string.IsNullOrWhiteSpace(message)));
                            }
                        }

                        if (messages.Count > 0)
                            return string.Join(" ", messages.Take(3));
                    }
                }
            }
            catch
            {
            }

            return "Failed to submit bug report. Please try again.";
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

            [MaxLength(50, ErrorMessage = "Severity cannot exceed 50 characters")]
            public string? Severity { get; set; }
        }
    }
}
