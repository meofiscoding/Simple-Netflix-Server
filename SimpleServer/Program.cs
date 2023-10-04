// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using MongoConnector;
using MongoConnector.Models;
using SimpleServer.Configuration;
using SimpleServer.src.Movie;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SimpleServer.src.Auth;
using AspNetCore.Identity.MongoDbCore.Models;
using SimpleServer.Utils.Mediator.Commands.Auth;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var mongoDbIdentityConfig = new MongoDbIdentityConfiguration
{
    MongoDbSettings = new MongoDbSettings
    {
        ConnectionString = "mongodb://localhost:27017",
        DatabaseName = "sampleDB"
        // ConnectionString = Environment.GetEnvironmentVariable("MongoDB_ConnectionURI"),
        // DatabaseName = Environment.GetEnvironmentVariable("MongoDB_DatabaseName")
    },
    IdentityOptionsAction = options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;

        // Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
        options.User.RequireUniqueEmail = true;
    }
};

// Configure the MongoDb with Identity
builder.Services.ConfigureMongoDbIdentity<User, MongoIdentityRole, Guid>(mongoDbIdentityConfig)
                .AddUserManager<UserManager<User>>()
                .AddSignInManager<SignInManager<User>>()
                .AddRoles<MongoIdentityRole>()
                .AddRoleManager<RoleManager<MongoIdentityRole>>()
                .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        jwt.SaveToken = true;
        jwt.RequireHttpsMetadata = false;
        jwt.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = configuration["JWT:ValidAudience"],
            ValidIssuer = configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
            ClockSkew = TimeSpan.Zero
        };
        //jwt.TokenValidationParameters = tokenSettings;
    }
);

builder.Services.AddAuthorization(options =>
{
    // Only users with the "Admin" role will be authorized to access resources or perform actions
    options.AddPolicy("ElevatedRights", policy =>
        policy.RequireRole(Role.Admin));
    // User has either the "Admin" role or the "User" role to access resources 
    options.AddPolicy("StandardRights", policy =>
        policy.RequireRole(Role.Admin, Role.User));
});

// Add services to the container.
builder.Services.Configure<JwtConfig>(
         configuration.GetSection(JwtConfig.Position));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Config MongoDB 
builder.Services.AddSingleton<MongoDbService>();

// Server services register
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// add CORS rule
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddMediatR(typeof(LoginUserCommand));


var app = builder.Build();

await SeedData(app).ConfigureAwait(false);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.UseCors("AngularClient");

app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature is not null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(contextFeature.Error.Message);
                }
            });
        });

app.Run();

static async Task SeedData(WebApplication app)
{
    using var scope = app.Services.CreateAsyncScope();

    var mongoDBService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
    await mongoDBService.SeedMovieData().ConfigureAwait(false);
}

public partial class Program { }

