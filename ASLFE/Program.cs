using ASLFE.Components;
using ASLFE.Components.Pages;
using ASLFE.JWT;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();

// Session(JWT)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<JwtSessionHandler>();
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7206/"); // API base
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<JwtSessionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapPost("/local-login", async (HttpContext httpContext, SignIn.LoginResponse loginResponse) =>
{
    // 1. Set the Session (using "AccessToken" to match your Handler)
    httpContext.Session.SetString("AccessToken", loginResponse.Token);

    // 2. Create Claims from the passed user data
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, loginResponse.UserId.ToString()),
        new Claim(ClaimTypes.Name, loginResponse.Username),
        new Claim(ClaimTypes.Email, loginResponse.Email ?? string.Empty),
        new Claim("Avatar", loginResponse.Avatar ?? string.Empty),
        new Claim("Gender", loginResponse.Gender.ToString())
    };

    if (loginResponse.Roles != null)
    {
        foreach (var role in loginResponse.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
    }

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

    // 3. Sign In the user using standard HttpContext
    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        claimsPrincipal,
        new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
        });

    return Results.Ok();
}).DisableAntiforgery();
app.MapPost("/local-logout", async (HttpContext httpContext) =>
{
    httpContext.Session.Clear();
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
