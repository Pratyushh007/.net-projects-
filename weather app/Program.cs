using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register SQLite EF Core DbContext
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlite("Data Source=weatherapp.db"));

// Register named HttpClient for weather and geocoding APIs
builder.Services.AddHttpClient("WeatherApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "Atmosphere360-WeatherApp/1.0 (ASP.NET Core)");
});

// Register Domain & Tool Services
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IWeatherCalculations, WeatherCalculations>();
builder.Services.AddScoped<IGradeManagerService, GradeManagerService>();
builder.Services.AddScoped<IFileExportService, FileExportService>();

var app = builder.Build();

// Initialize and seed SQLite database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<WeatherDbContext>();
        DbInitializer.Initialize(db);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating or seeding the SQLite database.");
    }
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
