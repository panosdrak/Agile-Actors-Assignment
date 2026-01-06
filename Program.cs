using Agile_Actors_Assignment.Interfaces;
using Agile_Actors_Assignment.Models;
using Agile_Actors_Assignment.Services;
using Microsoft.Extensions.Options;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register named options for Weather API
builder.Services
    .AddOptions<ExternalApiOptions>("Weather")
    .Bind(builder.Configuration.GetSection("ExternalApis:Weather"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "Weather BaseUrl is required")
    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Weather ApiKey is required")
    .ValidateOnStart();

// Register named options for PlaceToCoordinates API
builder.Services
    .AddOptions<ExternalApiOptions>("PlaceToCoordinates")
    .Bind(builder.Configuration.GetSection("ExternalApis:PlaceToCoordinates"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "PlaceToCoordinates BaseUrl is required")
    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "PlaceToCoordinates ApiKey is required")
    .ValidateOnStart();

// Register named options for News API
builder.Services
    .AddOptions<ExternalApiOptions>("News API")
    .Bind(builder.Configuration.GetSection("ExternalApis:News API"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "News API BaseUrl is required")
    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "News API ApiKey is required")
    .ValidateOnStart();

// Register typed HttpClient for Weather API
builder.Services.AddHttpClient<WeatherApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptionsMonitor<ExternalApiOptions>>().Get("Weather");
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Register typed HttpClient for PlaceToCoordinates API
builder.Services.AddHttpClient<PlaceToCoordsClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptionsMonitor<ExternalApiOptions>>().Get("PlaceToCoordinates");
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Register typed HttpClient for News API
builder.Services.AddHttpClient<NewsApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptionsMonitor<ExternalApiOptions>>().Get("News API");
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MyDemoApp/1.0");   // thelei User Agent ypoxrewtika
});

builder.Services.AddSingleton<IApiStatistics, ApiStatistics>();
builder.Services.AddScoped<AggregationService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app;

try
{
    app = builder.Build();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to build application: {ex.GetType().Name}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}");
        Console.WriteLine($"Inner Message: {ex.InnerException.Message}");
    }
    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
