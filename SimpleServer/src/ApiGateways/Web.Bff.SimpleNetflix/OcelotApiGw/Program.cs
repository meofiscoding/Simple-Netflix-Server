using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("IdentityServerApiKey")
    .AddJwtBearer("IdentityServerApiKey", options =>
    {
        options.Authority = "https://localhost:5001";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddOcelot();
builder.Services.AddCors();
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(Path.GetRelativePath(builder.Environment.ContentRootPath, $"ocelot.{builder.Environment.EnvironmentName}.json"), optional: false, reloadOnChange: true);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

// Config ocelot
app.UseCors(builder =>
{
    builder.AllowAnyOrigin();
    builder.AllowAnyHeader();
    builder.AllowAnyMethod();
});
app.UseOcelot().Wait();

app.Run();
