// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MongoConnector;
using SimpleServer.src.Movie;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Config MongoDB 
builder.Services.AddSingleton<MongoDbService>();

// Server services register
builder.Services.AddScoped<IMovieService, MovieService>();

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

app.MapControllers();

app.UseCors("AngularClient");

app.Run();

static async Task SeedData(WebApplication app)
{
    using var scope = app.Services.CreateAsyncScope();

    var mongoDBService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
    await mongoDBService.SeedMovieData().ConfigureAwait(false);
}



