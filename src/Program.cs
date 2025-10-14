using System.Text.Json;
using DotNetEnv;

Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var portValue = 3000;
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(portEnv) && int.TryParse(portEnv, out var parsedPort) && parsedPort > 0)
{
    portValue = parsedPort;
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(portValue);
});

var greetingsFile = Path.Combine(AppContext.BaseDirectory, "data", "greetings.json");
if (!File.Exists(greetingsFile))
{
    throw new FileNotFoundException($"Greeting data file not found at '{greetingsFile}'.");
}

var greetings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(greetingsFile))
    ?? throw new InvalidOperationException("Failed to load greetings from configuration.");

var normalizedGreetings = greetings
    .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
    .ToDictionary(
        pair => pair.Key.Trim().ToUpperInvariant(),
        pair => pair.Value.Trim());

var configuredCountry = Environment.GetEnvironmentVariable("COUNTRY_CODE");
var countryCode = string.IsNullOrWhiteSpace(configuredCountry)
    ? "EN"
    : configuredCountry.Trim().ToUpperInvariant();

var app = builder.Build();

app.MapGet("/", () =>
{
    if (!normalizedGreetings.TryGetValue(countryCode, out var greeting))
    {
        return Results.NotFound(new { error = $"Unknown country code '{countryCode}'" });
    }

    return Results.Json(new
    {
        code = countryCode,
        message = greeting
    });
});

app.Run();
