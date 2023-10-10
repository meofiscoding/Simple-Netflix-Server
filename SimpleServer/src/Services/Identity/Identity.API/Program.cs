using Domain.Mediator.Commands.Auth;
using Identity.API.Configuration;
using Identity.API.Db;
using Identity.API.Domain.Entities;
using Identity.API.Extensions;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("IdentityDB")));

builder.Services.RegisterAuthentication(configuration)
    .AddMediatR(typeof(LoginUserCommand));

builder.Services.AddHttpClient();
builder.Services.RegisterSwagger();

builder.Services.AddIdentityServer(options =>
    {
        options.UserInteraction = new UserInteractionOptions()
        {
            // LogoutUrl = "https://www.youtube.com/watch?v=euDyxWDgSUU",
            // LoginUrl = "https://www.youtube.com/watch?v=euDyxWDgSUUn",
            // LoginReturnUrlParameter = "https://www.youtube.com/watch?v=dtthbiP3SE0"
        };
    })
    .AddAspNetIdentity<User>()
    .AddInMemoryClients(Config.GetClients(builder.Configuration))
    .AddInMemoryIdentityResources(Config.GetResources())
    .AddInMemoryApiResources(Config.GetApis())
    .AddInMemoryApiScopes(Config.GetApiScopes())
    .AddDeveloperSigningCredential(); // Not recommended for production - you need to store your key material somewhere secure

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseIdentityServer();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
// Config time stamp
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await SeedManager.Seed(services);
}

app.Run();
