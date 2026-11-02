using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

public class HomeDashboardViewModel
{
    public int FavoriteCitiesCount { get; set; }
    public int NotesCount { get; set; }
    public int PendingTasksCount { get; set; }
    public int EmergencyContactsCount { get; set; }
    public decimal ClimateWalletBalance { get; set; }
    public List<WeatherCardSummary> FeaturedCities { get; set; } = new();
}

public class WeatherCardSummary
{
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double TempC { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Icon { get; set; } = "☀️";
    public string ComfortGrade { get; set; } = "A";
    public double WindSpeed { get; set; }
}

public class HomeController : Controller
{
    private readonly WeatherDbContext _dbContext;
    private readonly IWeatherService _weatherService;

    public HomeController(WeatherDbContext dbContext, IWeatherService weatherService)
    {
        _dbContext = dbContext;
        _weatherService = weatherService;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeDashboardViewModel
        {
            FavoriteCitiesCount = await _dbContext.FavoriteCities.CountAsync(),
            NotesCount = await _dbContext.WeatherNotes.CountAsync(),
            PendingTasksCount = await _dbContext.WeatherTodoItems.CountAsync(t => !t.IsCompleted),
            EmergencyContactsCount = await _dbContext.EmergencyContacts.CountAsync(),
            ClimateWalletBalance = await _dbContext.EnergyAccounts.Select(a => a.CurrentBalance).FirstOrDefaultAsync()
        };

        // Fetch quick snapshots for 3 global featured cities
        string[] sampleCities = { "London", "Tokyo", "New York" };
        foreach (var c in sampleCities)
        {
            try
            {
                var w = await _weatherService.GetWeatherForCityAsync(c);
                vm.FeaturedCities.Add(new WeatherCardSummary
                {
                    CityName = w.CityName,
                    Country = w.Country,
                    TempC = w.CurrentTempC,
                    Condition = w.ConditionText,
                    Icon = w.ConditionIcon,
                    ComfortGrade = w.ComfortGrade,
                    WindSpeed = w.WindSpeedKmh
                });
            }
            catch
            {
                // Graceful fallback
            }
        }

        return View(vm);
    }

    public IActionResult Roadmap()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
