﻿using Microsoft.EntityFrameworkCore;
using Payment.API;
using Payment.API.Data;
using Payment.API.Service.Stripe;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await SeedData.InitializeDatabase(app);

app.Run();