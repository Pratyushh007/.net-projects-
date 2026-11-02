using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models.Entities;

namespace WeatherApp.Controllers;

public class ContactsController : Controller
{
    private readonly WeatherDbContext _dbContext;

    public ContactsController(WeatherDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? city, string? category)
    {
        var query = _dbContext.EmergencyContacts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(c => c.CityOrRegion.ToLower() == city.ToLower() || c.CityOrRegion == "Global");

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            query = query.Where(c => c.Category == category);

        var contacts = await query
            .OrderBy(c => c.PriorityLevel)
            .ThenBy(c => c.Name)
            .ToListAsync();

        ViewBag.SelectedCity = city;
        ViewBag.SelectedCategory = category ?? "All";
        return View(contacts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmergencyContact contact)
    {
        if (ModelState.IsValid)
        {
            _dbContext.EmergencyContacts.Add(contact);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Emergency contact '{contact.Name}' added to directory!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _dbContext.EmergencyContacts.FindAsync(id);
        if (contact != null)
        {
            _dbContext.EmergencyContacts.Remove(contact);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Emergency contact removed.";
        }
        return RedirectToAction(nameof(Index));
    }
}

[ApiController]
[Route("api/[controller]")]
public class ContactsApiController : ControllerBase
{
    private readonly WeatherDbContext _dbContext;

    public ContactsApiController(WeatherDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmergencyContact>>> GetContacts([FromQuery] string? city, [FromQuery] string? category)
    {
        var query = _dbContext.EmergencyContacts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(c => c.CityOrRegion.ToLower() == city.ToLower() || c.CityOrRegion == "Global");

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(c => c.Category.ToLower() == category.ToLower());

        return await query.OrderBy(c => c.PriorityLevel).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmergencyContact>> GetContact(int id)
    {
        var contact = await _dbContext.EmergencyContacts.FindAsync(id);
        if (contact == null)
            return NotFound(new { message = $"Contact with ID {id} not found." });

        return contact;
    }

    [HttpPost]
    public async Task<ActionResult<EmergencyContact>> CreateContact([FromBody] EmergencyContact contact)
    {
        if (string.IsNullOrWhiteSpace(contact.Name) || string.IsNullOrWhiteSpace(contact.Phone))
            return BadRequest(new { message = "Contact Name and Phone are required." });

        _dbContext.EmergencyContacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(int id, [FromBody] EmergencyContact contact)
    {
        if (id != contact.Id)
            return BadRequest(new { message = "ID mismatch." });

        var existing = await _dbContext.EmergencyContacts.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Contact with ID {id} not found." });

        existing.Name = contact.Name;
        existing.Organization = contact.Organization;
        existing.Phone = contact.Phone;
        existing.Email = contact.Email;
        existing.CityOrRegion = contact.CityOrRegion;
        existing.Category = contact.Category;
        existing.PriorityLevel = contact.PriorityLevel;
        existing.Notes = contact.Notes;

        await _dbContext.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var contact = await _dbContext.EmergencyContacts.FindAsync(id);
        if (contact == null)
            return NotFound(new { message = $"Contact with ID {id} not found." });

        _dbContext.EmergencyContacts.Remove(contact);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = $"Contact '{contact.Name}' deleted successfully." });
    }
}
