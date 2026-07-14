using System.Text.Json;
using System.Text.Json.Serialization;
using APO_BOT.DemoApi.Data;
using APO_BOT.DemoApi.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var createDatabaseOnly = args.Any(argument => string.Equals(argument, "--create-database", StringComparison.OrdinalIgnoreCase));

var configuredPath = builder.Configuration["Demo:DatabasePath"] ?? "Data/apobot-demo.db";
var databasePath = Path.IsPathRooted(configuredPath)
    ? configuredPath
    : Path.Combine(builder.Environment.ContentRootPath, configuredPath);
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

builder.Services.AddDbContext<DemoDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

var app = builder.Build();
app.UseExceptionHandler();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await DemoDataSeeder.InitializeAsync(db);
}

if (createDatabaseOnly)
{
    app.Logger.LogInformation("Base de datos SQLite creada en {DatabasePath}", databasePath);
    return;
}

app.MapDemoApi();
app.Run();

public partial class Program;
