using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models.Entities;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

public class NotesController : Controller
{
    private readonly WeatherDbContext _dbContext;
    private readonly IWeatherService _weatherService;

    public NotesController(WeatherDbContext dbContext, IWeatherService weatherService)
    {
        _dbContext = dbContext;
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? city, string? category, string? search)
    {
        var query = _dbContext.WeatherNotes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(n => n.CityName.ToLower() == city.ToLower());

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            query = query.Where(n => n.Category == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(n => n.Title.Contains(search) || n.Content.Contains(search));

        var notes = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();

        ViewBag.SelectedCity = city;
        ViewBag.SelectedCategory = category ?? "All";
        ViewBag.SearchTerm = search;
        return View(notes);
    }

    [HttpGet]
    public async Task<IActionResult> Create(string? city)
    {
        var note = new WeatherNote
        {
            CityName = string.IsNullOrWhiteSpace(city) ? "London" : city
        };

        try
        {
            var weather = await _weatherService.GetWeatherForCityAsync(note.CityName);
            note.TemperatureC = weather.CurrentTempC;
            note.Condition = weather.ConditionText;
        }
        catch
        {
            note.TemperatureC = 20.0;
            note.Condition = "Clear";
        }

        return View(note);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WeatherNote note)
    {
        if (string.IsNullOrWhiteSpace(note.Title) || string.IsNullOrWhiteSpace(note.Content))
        {
            ModelState.AddModelError("", "Title and content are required.");
            return View(note);
        }

        note.CreatedAt = DateTime.UtcNow;
        _dbContext.WeatherNotes.Add(note);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Weather note '{note.Title}' saved successfully!";
        return RedirectToAction(nameof(Index), new { city = note.CityName });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var note = await _dbContext.WeatherNotes.FindAsync(id);
        if (note == null)
            return NotFound();

        return View(note);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(WeatherNote note)
    {
        var existing = await _dbContext.WeatherNotes.FindAsync(note.Id);
        if (existing == null)
            return NotFound();

        existing.Title = note.Title;
        existing.Content = note.Content;
        existing.Category = note.Category;
        existing.CityName = note.CityName;
        existing.TemperatureC = note.TemperatureC;
        existing.Condition = note.Condition;
        existing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Note updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var note = await _dbContext.WeatherNotes.FindAsync(id);
        if (note != null)
        {
            _dbContext.WeatherNotes.Remove(note);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Note deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
