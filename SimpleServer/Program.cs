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


var app = builder.Build();

await SeedData(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


static async Task SeedData(WebApplication app)
{
    using var scope = app.Services.CreateAsyncScope();

    var mongoDBService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
    await mongoDBService.SeedMovieData();
}

