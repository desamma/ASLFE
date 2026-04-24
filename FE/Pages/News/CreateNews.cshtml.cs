using BussinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FE.Pages.News
{
    [Authorize(Roles = "admin")]
    public class CreateNewsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateNewsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public Guid? EditingNewsId { get; set; }
        public bool IsEditing { get; set; }

        public async Task OnGetAsync(Guid? id)
        {
            if (id.HasValue)
            {
                IsEditing = true;
                EditingNewsId = id;

                try
                {
                    var client = _httpClientFactory.CreateClient("Api");
                    var response = await client.GetAsync($"api/gamenews/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var content = await response.Content.ReadAsStringAsync();

                        var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                        if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                        {
                            var news = JsonSerializer.Deserialize<GameNews>(dataProperty.GetRawText(), jsonOptions);
                            if (news != null)
                            {
                                Input = new InputModel
                                {
                                    Title = news.Title,
                                    BannerPath = news.BannerPath,
                                    Description = news.Description,
                                    Content = news.Content
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Failed to load news for editing: {ex.Message}";
                }
            }
        }

        public async Task<IActionResult> OnGetGetImagesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync("api/firebasestorage/list?folderPath=images/news");

                if (!response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = false, images = new List<object>() });
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("files", out var filesProperty))
                {
                    var files = JsonSerializer.Deserialize<List<string>>(filesProperty.GetRawText(), jsonOptions);
                    if (files != null)
                    {
                        var images = new List<ImageItem>();

                        foreach (var file in files.Where(IsValidImageExtension))
                        {
                            string url = file;

                            try
                            {
                                var urlResponse = await client.GetAsync($"api/firebasestorage/url?filePath={Uri.EscapeDataString(file)}");
                                if (urlResponse.IsSuccessStatusCode)
                                {
                                    var urlContent = await urlResponse.Content.ReadAsStringAsync();
                                    var urlJson = JsonSerializer.Deserialize<JsonElement>(urlContent, jsonOptions);
                                    if (urlJson.ValueKind == JsonValueKind.Object && urlJson.TryGetProperty("url", out var urlProperty))
                                    {
                                        url = urlProperty.GetString() ?? file;
                                    }
                                }
                            }
                            catch
                            {
                                // Fall back to the original file path if the URL lookup fails.
                            }

                            images.Add(new ImageItem
                            {
                                name = Path.GetFileName(file),
                                path = file,
                                url = url,
                                size = 0
                            });
                        }

                        images = images
                            .OrderBy(x => x.name)
                            .ToList();

                        return new JsonResult(new { success = true, images = images });
                    }
                }

                return new JsonResult(new { success = false, images = new List<object>() });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message, images = new List<object>() });
            }
        }

        public async Task<IActionResult> OnPostUploadImageAsync()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return new JsonResult(new { success = false, error = "No file provided" });
                }

                if (!IsValidImageExtension(file.FileName))
                {
                    return new JsonResult(new { success = false, error = "Invalid image format" });
                }

                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                {
                    return new JsonResult(new { success = false, error = "File size exceeds 5MB limit" });
                }

                var client = _httpClientFactory.CreateClient("Api");
                using var formContent = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();

                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                formContent.Add(streamContent, "file", file.FileName);
                formContent.Add(new StringContent("images/news/"), "folderPath");

                var uploadResponse = await client.PostAsync("api/firebasestorage/upload", formContent);

                if (!uploadResponse.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = false, error = "Failed to upload image" });
                }

                var uploadContent = await uploadResponse.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var uploadResult = JsonSerializer.Deserialize<UploadResponse>(uploadContent, jsonOptions);

                if (uploadResult?.Url != null)
                {
                    return new JsonResult(new { success = true, url = uploadResult.Url, fileName = file.FileName });
                }

                return new JsonResult(new { success = false, error = "Upload succeeded but URL not returned" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                if (id.HasValue)
                {
                    // Edit existing news
                    var gameNews = new GameNews
                    {
                        Id = id.Value,
                        Title = Input.Title,
                        BannerPath = Input.BannerPath ?? string.Empty,
                        Description = Input.Description,
                        Content = Input.Content
                    };

                    var response = await client.PutAsJsonAsync($"api/gamenews/{id}", gameNews);

                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "News updated successfully!";
                        return RedirectToPage("/News/ReadNews", new { id = id });
                    }

                    ErrorMessage = "Failed to update news. Please try again.";
                }
                else
                {
                    // Create new news
                    var gameNews = new GameNews
                    {
                        Id = Guid.NewGuid(),
                        Title = Input.Title,
                        BannerPath = Input.BannerPath ?? string.Empty,
                        Description = Input.Description,
                        Content = Input.Content
                    };

                    var response = await client.PostAsJsonAsync("api/gamenews", gameNews);

                    if (response.IsSuccessStatusCode)
                    {
                        SuccessMessage = "News created successfully!";
                        Input = new InputModel();
                        return Page();
                    }

                    ErrorMessage = "Failed to create news. Please try again.";
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        private bool IsValidImageExtension(string fileName)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
            return imageExtensions.Contains(Path.GetExtension(fileName).ToLower());
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Title is required")]
            [MaxLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
            public string Title { get; set; } = string.Empty;

            [MaxLength(500, ErrorMessage = "Banner path cannot exceed 500 characters")]
            public string? BannerPath { get; set; }

            [Required(ErrorMessage = "Description is required")]
            [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
            public string Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Content is required")]
            [MaxLength(10000, ErrorMessage = "Content cannot exceed 10000 characters")]
            public string Content { get; set; } = string.Empty;
        }

        public class UploadResponse
        {
            public string? Url { get; set; }
            public string? FileName { get; set; }
        }

        public class ImageItem
        {
            public string name { get; set; } = string.Empty;
            public string path { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public long size { get; set; }
        }
    }
}
