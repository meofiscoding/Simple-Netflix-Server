using EventBus.Message.Common;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Movie.API.EventBusConsumer;
using Movie.API.Infrastructure.Data;
using Movie.API.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // Uncomment this when in development
        // options.RequireHttpsMetadata = false;
        options.Authority = builder.Configuration["IdentityUrl"];
        options.Audience = "movies";
        // options.TokenValidationParameters = new TokenValidationParameters
        // {
        //     ValidateAudience = false
        // };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMovieContext, MovieContext>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();

// MassTransit-RabbitMQ Configuration
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<MovieTransferConsumer>();

    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        // cfg.UseHealthCheck(ctx);

        cfg.ReceiveEndpoint(EventBusConstants.MovieCrawlQueue, c => c.ConfigureConsumer<MovieTransferConsumer>(ctx));
    });
});

// General Configuration
builder.Services.AddScoped<MovieTransferConsumer>();
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
