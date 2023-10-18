using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = true;

builder.Services
    .AddAuthentication("IdentityApiKey")
    .AddJwtBearer("IdentityApiKey", options =>
    {
        options.RequireHttpsMetadata = false;
        options.Authority = builder.Configuration["IdentityUrl"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
        // options.Configuration = new OpenIdConnectConfiguration();
    });

builder.Services.AddOcelot();
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(Path.GetRelativePath(builder.Environment.ContentRootPath, $"ocelot.{builder.Environment.EnvironmentName}.json"), optional: false, reloadOnChange: true);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// add CORS rule
builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Config ocelot
app.UseCors(builder =>
{
    builder.AllowAnyOrigin();
    builder.AllowAnyHeader();
    builder.AllowAnyMethod();
});

app.UseOcelot().Wait();
app.MapControllers();
app.Run();
