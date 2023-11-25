using Identity.Grpc.Data;
using Identity.Grpc.Entity;
using Identity.Grpc.Service;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MinRequestBodyDataRate = null;

    // options.ListenAnyIP(50500,
    //    listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
    //TODO: Uncomment when deploy
    options.ListenAnyIP(80,
    listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
});
var configuration = builder.Configuration;
// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("IdentityDB")));

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(config =>
    {
        config.Password.RequiredLength = 6;
        config.Password.RequireDigit = true;
        config.Password.RequireNonAlphanumeric = true;
        config.Password.RequireUppercase = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
// Add Grpc Service
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
var app = builder.Build();
// Map Grpc Service
app.MapGrpcService<UserService>();
app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

