using Identity.API.Configuration;
using Identity.API.Data;
using Identity.API.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("IdentityDB")));


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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseIdentityServer();

app.MapDefaultControllerRoute();

app.Run();

