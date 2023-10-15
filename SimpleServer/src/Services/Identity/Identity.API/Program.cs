using Identity.API;
using Identity.API.Configuration;
using Identity.API.Data;
using Identity.API.Entity;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("IdentityDB")));

// Add Google support
builder.Services.AddAuthentication().AddGoogle("Google", options =>
{
    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
    // Uncomment this when in development
    options.ClientId = configuration["Google:ClientId"];
    options.ClientSecret = configuration["Google:ClientSecret"];

    // This part is use when dockerize the application
    // options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
    // options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(config =>
    {
        config.Password.RequiredLength = 6;
        config.Password.RequireDigit = true;
        config.Password.RequireNonAlphanumeric = true;
        config.Password.RequireUppercase = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityServer()
.AddInMemoryClients(Config.Clients)
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiResources(Config.ApiResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddAspNetIdentity<ApplicationUser>()
.AddDeveloperSigningCredential(); // Not recommended for production - you need to store your key material somewhere secure

builder.Services.AddHealthChecks()
    .AddNpgSql(_ => configuration.GetRequiredConnectionString("IdentityDB"),
        name: "IdentityDB-check",
        tags: new string[] { "IdentityDB" });

builder.Services.ConfigureApplicationCookie(config =>
{
    config.Cookie.Name = "IdentityServer.Cookie";
    config.Cookie.SameSite = SameSiteMode.None;
    config.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    config.LoginPath = "/Auth/Login";
    config.LogoutPath = "/Auth/Logout";
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// This cookie policy fixes login issues with Chrome 80+ using HHTP
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseIdentityServer();

app.MapDefaultControllerRoute();

// Apply database migration automatically. Note that this approach is not
// recommended for production scenarios. Consider generating SQL scripts from
// migrations instead.
using (var scope = app.Services.CreateScope())
{
    await SeedData.EnsureSeedData(scope, app.Configuration, app.Logger);
}

await app.RunAsync();

