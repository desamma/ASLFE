using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FE.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class AdminBugReportsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminBugReportsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public string FilterStatus { get; set; } = "Open";

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public List<BugReportDto> BugReports { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadBugReports();
        }

        private async Task LoadBugReports()
        {
            var client = _httpClientFactory.CreateClient("Api");

            try
            {
                var response = await client.GetAsync("api/bugreport/admin/all");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);

                    List<BugReportDto>? bugReports = null;

                    if (jsonElement.ValueKind == JsonValueKind.Array)
                    {
                        bugReports = JsonSerializer.Deserialize<List<BugReportDto>>(content, jsonOptions);
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                    {
                        bugReports = JsonSerializer.Deserialize<List<BugReportDto>>(dataProperty.GetRawText(), jsonOptions);
                    }

                    if (bugReports != null)
                    {
                        BugReports = bugReports;

                        // Filter by status if specified
                        if (!string.IsNullOrEmpty(FilterStatus))
                        {
                            BugReports = BugReports.Where(b => b.Status.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase)).ToList();
                        }

                        // Sort by most recent first
                        BugReports = BugReports.OrderByDescending(b => b.CreatedDate).ToList();
                    }
                    else
                    {
                        ErrorMessage = "Failed to load bug reports";
                    }
                }
                else
                {
                    ErrorMessage = "Failed to load bug reports";
                }
            }
            catch (JsonException jsonEx)
            {
                ErrorMessage = $"Error parsing bug reports: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading bug reports: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient("Api");

            try
            {
                var actualBehavior = Request.Form["actualBehavior"].ToString();
                var status = Request.Form["status"].ToString();

                var updateRequest = new
                {
                    actualBehavior = string.IsNullOrWhiteSpace(actualBehavior) ? null : actualBehavior.Trim(),
                    status = string.IsNullOrWhiteSpace(status) ? "Open" : status.Trim()
                };

                var response = await client.PutAsJsonAsync($"api/bugreport/{id}/status", updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Bug report updated successfully!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = ExtractErrorMessage(errorContent);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating bug report: {ex.Message}";
            }

            await LoadBugReports();
            return Page();
        }

        private static string ExtractErrorMessage(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return "Failed to update bug report";

            try
            {
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, jsonOptions);

                if (errorResponse.ValueKind != JsonValueKind.Object)
                    return "Failed to update bug report";

                if (errorResponse.TryGetProperty("message", out var messageProperty))
                {
                    var message = messageProperty.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                        return message;
                }
            }
            catch
            {
            }

            return "Failed to update bug report";
        }

        public class BugReportDto
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Steps { get; set; }
            public string ExpectedBehavior { get; set; }
            public string ActualBehavior { get; set; }
            public string Severity { get; set; }
            public string Status { get; set; }
            public Guid UserId { get; set; }
            public string UserName { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? UpdatedDate { get; set; }
        }

    }
}
