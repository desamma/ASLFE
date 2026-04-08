using BussinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FE.Pages.News
{
    public class ReadNewsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReadNewsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public GameNews? News { get; set; }
        public List<GameNews> RelatedNews { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsAdminOrAuthor { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                // Fetch the specific news item
                var response = await client.GetAsync($"api/gamenews/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "News not found.";
                    return NotFound();
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize the response which has { message: "...", data: {...} } structure
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                {
                    News = JsonSerializer.Deserialize<GameNews>(dataProperty.GetRawText(), jsonOptions);
                }

                if (News == null)
                {
                    ErrorMessage = "News not found.";
                    return NotFound();
                }

                // Fetch related news (other recent news items)
                var allNewsResponse = await client.GetAsync("api/gamenews");
                if (allNewsResponse.IsSuccessStatusCode)
                {
                    var allNewsContent = await allNewsResponse.Content.ReadAsStringAsync();
                    var allNewsElement = JsonSerializer.Deserialize<JsonElement>(allNewsContent, jsonOptions);
                    if (allNewsElement.ValueKind == JsonValueKind.Object && allNewsElement.TryGetProperty("data", out var allDataProperty))
                    {
                        var allNews = JsonSerializer.Deserialize<List<GameNews>>(allDataProperty.GetRawText(), jsonOptions);
                        if (allNews != null)
                        {
                            RelatedNews = allNews
                                .Where(n => n.Id != id)
                                .Take(4)
                                .ToList();
                        }
                    }
                }

                IsAdminOrAuthor = User.Identity?.IsAuthenticated ?? false;
                ViewData["Title"] = News.Title;
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        public IActionResult OnGetEdit(Guid id)
        {
            return RedirectToPage("/News/CreateNews", new { id = id });
        }

        public string ProcessNewsContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            var lines = content.Split('\n');
            var htmlBuilder = new System.Text.StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmedLine = line.Trim();

                if (trimmedLine == "[SEPARATOR]")
                {
                    htmlBuilder.Append(@"<div class=""separator-preview"" style=""display: flex; align-items: center; justify-content: center; margin: 24px 0; gap: 0;""><div role=""separator"" aria-hidden=""true"" style=""display: flex; align-items: center; justify-content: center; width: 100%; gap: 12px;""><svg width=""19"" height=""16"" viewBox=""0 0 19 16"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"" style=""flex-shrink: 0;""><rect y=""1.44135"" width=""7.02738"" height=""7.02738"" transform=""matrix(0.693276 0.720673 -0.693276 0.720673 13.4194 2.053)"" stroke=""currentColor"" stroke-width=""2""></rect><path d=""M9 0.75L2 7.75L9 14.75"" stroke=""currentColor"" stroke-width=""2""></path></svg><div style=""flex: 1; height: 2px; background-color: currentColor;""></div><svg width=""21"" height=""18"" viewBox=""0 0 21 18"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"" style=""flex-shrink: 0;""><rect y=""1.44135"" width=""8.03936"" height=""8.03936"" transform=""matrix(0.693276 0.720673 -0.693276 0.720673 8.28148 2.35573)"" stroke=""currentColor"" stroke-width=""2""></rect><path d=""M11.2214 1L19.2214 9L11.2214 17"" stroke=""currentColor"" stroke-width=""2""></path></svg></div></div>");
                }
                else if (trimmedLine.StartsWith("[IMAGE:"))
                {
                    var match = Regex.Match(trimmedLine, @"\[IMAGE:(.*?)\]");
                    if (match.Success && !string.IsNullOrEmpty(match.Groups[1].Value))
                    {
                        var imageName = HtmlEncode(match.Groups[1].Value);
                        htmlBuilder.Append($@"<img src=""/images/{imageName}"" alt=""News Image"" style=""max-width: 100%; height: auto; margin: 12px 0; border-radius: 6px;"" />");
                    }
                }
                else if (trimmedLine.Length > 0)
                {
                    var encodedLine = HtmlEncode(trimmedLine);
                    htmlBuilder.Append($@"<p style=""margin: 8px 0; line-height: 1.6;"">{encodedLine}</p>");
                }
                else if (i > 0 && i < lines.Length - 1)
                {
                    htmlBuilder.Append("<br />");
                }
            }

            return htmlBuilder.ToString();
        }

        private string HtmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return System.Net.WebUtility.HtmlEncode(text);
        }
    }
}
