using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FE.Pages;

public class DownloadModel : PageModel
{
    private const string GameFileName = "ashenLightRPG.zip";
    private readonly IConfiguration _configuration;

    public DownloadModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsFileAvailable { get; private set; }
    public string DownloadUrl { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
        LoadFileState();

        if (TempData.TryGetValue("DownloadError", out var error))
        {
            ErrorMessage = error?.ToString();
        }
    }

    public IActionResult OnPostDownload()
    {
        var downloadUrl = GetDownloadUrl();
        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            TempData["DownloadError"] = "Game download URL is not configured.";
            return RedirectToPage();
        }

        return Redirect(downloadUrl);
    }

    private void LoadFileState()
    {
        DownloadUrl = GetDownloadUrl();
        IsFileAvailable = !string.IsNullOrWhiteSpace(DownloadUrl);
    }

    private string GetDownloadUrl()
    {
        var configuredUrl = _configuration["Download:GameFileUrl"];
        if (!string.IsNullOrWhiteSpace(configuredUrl))
            return configuredUrl;

        var bucket = _configuration["Firebase:StorageBucket"];
        if (string.IsNullOrWhiteSpace(bucket))
            return string.Empty;

        var cloudPath = _configuration["Download:GameFilePath"];
        if (string.IsNullOrWhiteSpace(cloudPath))
            cloudPath = $"downloads/{GameFileName}";

        return $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{Uri.EscapeDataString(cloudPath)}?alt=media";
    }
}
