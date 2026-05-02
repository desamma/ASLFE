using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FE.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class AdminNPCModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminNPCModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public List<NPCDto> NPCs { get; set; } = new List<NPCDto>();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");
            try
            {
                var response = await client.GetAsync("api/npc");
                if (response.IsSuccessStatusCode)
                {
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);

                    // Deserialize the response which has { message: "...", data: [...] } structure
                    if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                    {
                        NPCs = JsonSerializer.Deserialize<List<NPCDto>>(dataProperty.GetRawText(), jsonOptions) ?? new List<NPCDto>();
                    }
                    else
                    {
                        NPCs = new List<NPCDto>();
                    }
                }
                else
                {
                    ErrorMessage = $"Error loading NPCs. Status: {response.StatusCode}";
                    NPCs = new List<NPCDto>();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Connection error: " + ex.Message;
                NPCs = new List<NPCDto>();
            }
        }

        public async Task<IActionResult> OnGetGetImagesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync("api/firebasestorage/list?folderPath=images/npcs");

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
                            catch (Exception ex)
                            {
                                return new JsonResult(new { success = false, error = ex.Message, images = new List<object>() });
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
                formContent.Add(new StringContent("images/npcs/"), "folderPath");

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

        public async Task<IActionResult> OnPostCreateAsync(string name, string description, string imagePath, string location, string npcType, IFormFile imageFile)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var finalImagePath = imagePath;

            // If image file is uploaded, use the uploaded URL
            if (imageFile != null && imageFile.Length > 0)
            {
                using var formContent = new MultipartFormDataContent();
                using var fileStream = imageFile.OpenReadStream();

                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                formContent.Add(streamContent, "file", imageFile.FileName);
                formContent.Add(new StringContent("images/npcs/"), "folderPath");

                var uploadResponse = await client.PostAsync("api/firebasestorage/upload", formContent);
                if (uploadResponse.IsSuccessStatusCode)
                {
                    var uploadContent = await uploadResponse.Content.ReadAsStringAsync();
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var uploadResult = JsonSerializer.Deserialize<UploadResponse>(uploadContent, jsonOptions);
                    if (uploadResult?.Url != null)
                    {
                        finalImagePath = uploadResult.Url;
                    }
                }
            }

            var request = new CreateNPCRequest
            {
                Name = name ?? "",
                Description = description ?? "",
                ImagePath = finalImagePath ?? "",
                Location = location ?? "",
                NPCType = npcType ?? ""
            };

            var response = await client.PostAsJsonAsync("api/npc", request);
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Error creating NPC.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(Guid id, string name, string description, string imagePath, string location, string npcType, IFormFile imageFile)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var finalImagePath = imagePath;

            // If image file is uploaded, use the uploaded URL
            if (imageFile != null && imageFile.Length > 0)
            {
                using var formContent = new MultipartFormDataContent();
                using var fileStream = imageFile.OpenReadStream();

                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                formContent.Add(streamContent, "file", imageFile.FileName);
                formContent.Add(new StringContent("images/npcs/"), "folderPath");

                var uploadResponse = await client.PostAsync("api/firebasestorage/upload", formContent);
                if (uploadResponse.IsSuccessStatusCode)
                {
                    var uploadContent = await uploadResponse.Content.ReadAsStringAsync();
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var uploadResult = JsonSerializer.Deserialize<UploadResponse>(uploadContent, jsonOptions);
                    if (uploadResult?.Url != null)
                    {
                        finalImagePath = uploadResult.Url;
                    }
                }
            }

            var request = new UpdateNPCRequest
            {
                Name = name ?? "",
                Description = description ?? "",
                ImagePath = finalImagePath ?? "",
                Location = location ?? "",
                NPCType = npcType ?? ""
            };

            var response = await client.PutAsJsonAsync($"api/npc/{id}", request);
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Error updating NPC.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.DeleteAsync($"api/npc/{id}");

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Error deleting NPC.";
            }

            return RedirectToPage();
        }

        private static bool IsValidImageExtension(string fileName)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
            var extension = Path.GetExtension(fileName).ToLower();
            return validExtensions.Contains(extension);
        }
    }

    public class NPCDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string NPCType { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreateNPCRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string Location { get; set; }
        public string NPCType { get; set; }
    }

    public class UpdateNPCRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string NPCType { get; set; } = string.Empty;
    }

    public class ImageItem
    {
        public string name { get; set; }
        public string path { get; set; }
        public string url { get; set; }
        public int size { get; set; }
    }

    public class UploadResponse
    {
        public string Url { get; set; }
        public string FileName { get; set; }
    }
}
