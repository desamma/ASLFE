using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using BussinessObjects.Models;

namespace FE.Pages.News
{
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

        public void OnGet()
        {
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var gameNews = new GameNews
                {
                    Id = Guid.NewGuid(),
                    Title = Input.Title,
                    BannerPath = Input.BannerPath ?? string.Empty,
                    Description = Input.Description,
                    Content = Input.Content
                };

                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PostAsJsonAsync("api/gamenews", gameNews);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "News created successfully!";
                    Input = new InputModel();
                    return Page();
                }

                ErrorMessage = "Failed to create news. Please try again.";
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
