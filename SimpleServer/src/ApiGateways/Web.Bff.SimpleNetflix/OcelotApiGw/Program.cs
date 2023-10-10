using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("IdentityApiKey")
    .AddJwtBearer("IdentityApiKey", options =>
    {
        options.Authority = "https://localhost:5186";
        options.TokenValidationParameters = new TokenValidationParameters
        {
             ValidateAudience = false
        };
        options.Configuration = new OpenIdConnectConfiguration();
    });

builder.Services.AddOcelot();
// add CORS rule
builder.Services.AddCors();
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(Path.GetRelativePath(builder.Environment.ContentRootPath, $"ocelot.{builder.Environment.EnvironmentName}.json"), optional: false, reloadOnChange: true);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}
// Config ocelot
// app.UseCors("AllowAngularDevClient");
app.UseCors(builder =>
{
    builder.AllowAnyOrigin();
    builder.AllowAnyHeader();
    builder.AllowAnyMethod();
});
app.UseOcelot().Wait();

app.Run();
