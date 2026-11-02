using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models.Entities;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

public class WeatherController : Controller
{
    private readonly IWeatherService _weatherService;
    private readonly IFileExportService _exportService;
    private readonly WeatherDbContext _dbContext;

    public WeatherController(IWeatherService weatherService, IFileExportService exportService, WeatherDbContext dbContext)
    {
        _weatherService = weatherService;
        _exportService = exportService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? city, double? lat, double? lon)
    {
        var targetCity = string.IsNullOrWhiteSpace(city) ? "London" : city.Trim();
        var model = await _weatherService.GetWeatherForCityAsync(targetCity, lat, lon);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Json(new List<object>());

        var results = await _weatherService.SearchCitiesAsync(query);
        return Json(results.Select(r => new
        {
            r.Name,
            r.Country,
            r.AdminArea,
            r.Latitude,
            r.Longitude
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(string cityName, string country, double lat, double lon, string? adminArea)
    {
        var existing = await _dbContext.FavoriteCities
            .FirstOrDefaultAsync(f => f.CityName.ToLower() == cityName.ToLower());

        if (existing != null)
        {
            _dbContext.FavoriteCities.Remove(existing);
            TempData["SuccessMessage"] = $"Removed {cityName} from saved cities.";
        }
        else
        {
            _dbContext.FavoriteCities.Add(new FavoriteCity
            {
                CityName = cityName,
                Country = country,
                Latitude = lat,
                Longitude = lon,
                AdminArea = adminArea,
                AddedAt = DateTime.UtcNow
            });
            TempData["SuccessMessage"] = $"Added {cityName} to your favorite cities!";
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { city = cityName, lat, lon });
    }

    [HttpGet]
    public async Task<IActionResult> Export(string city, string format = "json", double? lat = null, double? lon = null)
    {
        var model = await _weatherService.GetWeatherForCityAsync(city, lat, lon);

        switch (format.ToLowerInvariant())
        {
            case "csv":
                var csvBytes = _exportService.ExportWeatherReportAsCsv(model);
                return File(csvBytes, "text/csv", $"Weather_{model.CityName}_{DateTime.UtcNow:yyyyMMdd}.csv");

            case "txt":
                var txtBytes = _exportService.ExportWeatherReportAsText(model);
                return File(txtBytes, "text/plain", $"Weather_{model.CityName}_{DateTime.UtcNow:yyyyMMdd}.txt");

            case "json":
            default:
                var jsonBytes = _exportService.ExportWeatherReportAsJson(model);
                return File(jsonBytes, "application/json", $"Weather_{model.CityName}_{DateTime.UtcNow:yyyyMMdd}.json");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Compare(string? city1 = "London", string? city2 = "Tokyo")
    {
        var weather1 = await _weatherService.GetWeatherForCityAsync(city1 ?? "London");
        var weather2 = await _weatherService.GetWeatherForCityAsync(city2 ?? "Tokyo");

        ViewBag.Weather1 = weather1;
        ViewBag.Weather2 = weather2;
        return View();
    }
}
