using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Payment.API;
using Payment.API.Data;
using Payment.API.GrpcService;
using Payment.API.GrpcService.Protos;
using Payment.API.Service.Stripe;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
//  Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
  {
      options.Limits.MinRequestBodyDataRate = null;

      options.ListenAnyIP(5031,
            listenOptions => listenOptions.Protocols = HttpProtocols.Http1);

      options.ListenAnyIP(50051,
         listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
  });
IdentityModelEventSource.ShowPII = true;
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
builder.Services.AddHttpContextAccessor();

var configuration = builder.Configuration;

// Configure DbContext
builder.Services.AddDbContext<PaymentDBContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("PaymentDB")));

// add CORS rule
builder.Services.AddCors();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register services
builder.Services.AddScoped<IStripeService, StripeService>();
// Grpc Configuration
builder.Services.AddGrpcClient<PaymentProtoService.PaymentProtoServiceClient>
            (o => o.Address = new Uri(builder.Configuration["GrpcUrl"] ?? throw new Exception("GrpcUrl is missing")));
builder.Services.AddScoped<PaymentGrpcService>();

// Add authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.Authority = builder.Configuration["IdentityUrl"];
        opt.Audience = "payment";
    });

// add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder =>
{
    builder.AllowAnyOrigin();
    builder.AllowAnyHeader();
    builder.AllowAnyMethod();
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await SeedData.InitializeDatabase(app);

app.Run();