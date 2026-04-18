using ASLFE.JWT;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddTransient<JwtSessionHandler>();

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7206/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<JwtSessionHandler>();

var app = builder.Build();

// --- Payment Proxy ---
app.MapGet("/payment-proxy", async (string path, HttpContext ctx, IHttpClientFactory factory) =>
{
    try
    {
        var client = factory.CreateClient("Api");
        var response = await client.GetAsync($"api/payment/{path}");
        var content = await response.Content.ReadAsStringAsync();

        // ✅ Return response status code
        return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"[Payment Proxy Error] GET {path}: {ex.Message}");
        return Results.Json(new { error = "Backend connection failed", details = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Payment Proxy Error] GET {path}: {ex.Message}");
        return Results.Json(new { error = "Proxy error", details = ex.Message }, statusCode: 500);
    }
});

app.MapPost("/payment-proxy", async (string path, HttpContext ctx, IHttpClientFactory factory) =>
{
    try
    {
        var client = factory.CreateClient("Api");
        using var reader = new StreamReader(ctx.Request.Body);
        var body = await reader.ReadToEndAsync();
        var httpContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"api/payment/{path}", httpContent);
        var content = await response.Content.ReadAsStringAsync();

        // ✅ Return response status code
        return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"[Payment Proxy Error] POST {path}: {ex.Message}");
        return Results.Json(new { error = "Backend connection failed", details = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Payment Proxy Error] POST {path}: {ex.Message}");
        return Results.Json(new { error = "Proxy error", details = ex.Message }, statusCode: 500);
    }
});

// --- Gacha Proxy ---
app.MapGet("/gacha-proxy", async (string path, HttpContext ctx, IHttpClientFactory factory) =>
{
    try
    {
        Console.WriteLine($"[GACHA Proxy] GET /api/gacha/{path}");
        var client = factory.CreateClient("Api");
        var response = await client.GetAsync($"api/gacha/{path}");
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"[GACHA Proxy] Status: {response.StatusCode}");

        // ✅ Return response status code
        return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"[GACHA Proxy Error] GET {path}: {ex.Message}");
        return Results.Json(new { error = "Backend connection failed", details = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GACHA Proxy Error] GET {path}: {ex.Message}");
        return Results.Json(new { error = "Proxy error", details = ex.Message }, statusCode: 500);
    }
});

app.MapPost("/gacha-proxy", async (string path, HttpContext ctx, IHttpClientFactory factory) =>
{
    try
    {
        Console.WriteLine($"[GACHA Proxy] POST /api/gacha/{path}");
        var client = factory.CreateClient("Api");
        using var reader = new StreamReader(ctx.Request.Body);
        var body = await reader.ReadToEndAsync();
        var httpContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"api/gacha/{path}", httpContent);
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"[GACHA Proxy] Status: {response.StatusCode}");

        // ✅ Return response status code
        return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"[GACHA Proxy Error] POST {path}: {ex.Message}");
        return Results.Json(new { error = "Backend connection failed", details = ex.Message }, statusCode: 503);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GACHA Proxy Error] POST {path}: {ex.Message}");
        return Results.Json(new { error = "Proxy error", details = ex.Message }, statusCode: 500);
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();