using System.Security.Cryptography.X509Certificates;
using Identity.API;
using Identity.API.Configuration;
using Identity.API.Data;
using Identity.API.Entity;
using IdentityServer4;
using Microsoft.AspNetCore.Identity;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Azure.Security.KeyVault.Secrets;
using System.Text;
using System.Security.Cryptography;

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

builder.Services.AddIdentityServer(option =>
    {
        if (builder.Environment.IsDevelopment())
        {
            option.IssuerUri = configuration["IdentityUrl"];
        }
    }
)
.AddInMemoryClients(Config.Clients)
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiResources(Config.ApiResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddAspNetIdentity<ApplicationUser>()
.AddSigningCredential(GetIdentityServerCertificate());
// .AddDeveloperSigningCredential(); // Not recommended for production - you need to store your key material somewhere secure

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
    app.UseForwardedHeaders();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
// This cookie policy fixes login issues with Chrome 80+ using HHTP
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
});

//app.UseHttpsRedirection();
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


X509Certificate2 GetIdentityServerCertificate()
{
    string keyVaultUrl = configuration["KeyVault:AzureKeyVaultURL"];
    string clientId = configuration["KeyVault:ClientId"];
    string clientSecret = configuration["KeyVault:ClientSecret"];
    string tenantId = configuration["KeyVault:AzureClientTenantId"];

    // Create a new SecretClient using ClientSecretCredential​
    var client = new SecretClient(new Uri(keyVaultUrl), new ClientSecretCredential(tenantId, clientId, clientSecret));

    try
    {
        // get certificate from key vault
        var secret = client.GetSecret("IdenittyServer4Certificate");
        var pemContent = secret.Value.Value;
        // Separate the private key and certificate portions
        string[] pemParts = pemContent.Split(new string[] { "-----END PRIVATE KEY-----" }, StringSplitOptions.RemoveEmptyEntries);
        string privateKeyPem = pemParts[0] + "-----END PRIVATE KEY-----";
        string certificatePem = pemParts[1];

        // Load the private key into a RSA private key
        var privateKey = RSA.Create();
        privateKey.ImportFromPem(privateKeyPem);

        // Load the certificate
        var certificate = new X509Certificate2(Encoding.UTF8.GetBytes(certificatePem));

        // Attach the private key to the certificate
        return certificate.CopyWithPrivateKey(privateKey);
    }
    catch (System.Exception ex)
    {
        // handle case where certificate is not found in key vault
        throw new System.Exception(ex.Message);
    }
}

await app.RunAsync();

