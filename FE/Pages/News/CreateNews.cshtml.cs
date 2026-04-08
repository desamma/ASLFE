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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CreateNewsModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _httpClientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
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

        public IActionResult OnGetImages()
        {
            try
            {
                var imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                if (!Directory.Exists(imagesPath))
                {
                    return new JsonResult(new { success = false, images = new List<object>() });
                }

                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                var images = Directory.GetFiles(imagesPath)
                    .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .Select(file => new
                    {
                        name = Path.GetFileName(file),
                        path = $"/images/{Path.GetFileName(file)}",
                        size = new FileInfo(file).Length
                    })
                    .OrderBy(x => x.name)
                    .ToList();

                return new JsonResult(new { success = true, images = images });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message, images = new List<object>() });
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
    }
}
