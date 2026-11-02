using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models.Entities;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

public class PlannerController : Controller
{
    private readonly WeatherDbContext _dbContext;
    private readonly IGradeManagerService _gradeService;
    private readonly IWeatherService _weatherService;

    public PlannerController(WeatherDbContext dbContext, IGradeManagerService gradeService, IWeatherService weatherService)
    {
        _dbContext = dbContext;
        _gradeService = gradeService;
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> Todo(string? city = "London", string? filter = "all")
    {
        var targetCity = string.IsNullOrWhiteSpace(city) ? "London" : city.Trim();
        var weather = await _weatherService.GetWeatherForCityAsync(targetCity);

        var query = _dbContext.WeatherTodoItems.AsQueryable();

        if (filter == "active")
            query = query.Where(t => !t.IsCompleted);
        else if (filter == "completed")
            query = query.Where(t => t.IsCompleted);
        else if (filter == "city")
            query = query.Where(t => t.TargetCity.ToLower() == targetCity.ToLower() || t.TargetCity == "");

        var todos = await query
            .OrderBy(t => t.IsCompleted)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentCity = targetCity;
        ViewBag.CurrentWeather = weather;
        ViewBag.Filter = filter ?? "all";

        return View(todos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTodo(string taskDescription, string targetCity, string requiredWeatherType, double? minTemp, double? maxTemp, bool requiresNoRain, string priority)
    {
        if (!string.IsNullOrWhiteSpace(taskDescription))
        {
            var item = new WeatherTodoItem
            {
                TaskDescription = taskDescription.Trim(),
                TargetCity = string.IsNullOrWhiteSpace(targetCity) ? "" : targetCity.Trim(),
                RequiredWeatherType = requiredWeatherType,
                MinTemp = minTemp,
                MaxTemp = maxTemp,
                RequiresNoRain = requiresNoRain,
                Priority = priority,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.WeatherTodoItems.Add(item);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Weather-smart task added successfully!";
        }

        return RedirectToAction(nameof(Todo), new { city = targetCity });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTodo(int id, string? city)
    {
        var item = await _dbContext.WeatherTodoItems.FindAsync(id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Todo), new { city });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTodo(int id, string? city)
    {
        var item = await _dbContext.WeatherTodoItems.FindAsync(id);
        if (item != null)
        {
            _dbContext.WeatherTodoItems.Remove(item);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Task deleted.";
        }

        return RedirectToAction(nameof(Todo), new { city });
    }

    [HttpGet]
    public async Task<IActionResult> Grades()
    {
        var reportCard = await _gradeService.GenerateReportCardAsync();
        return View(reportCard);
    }
}
