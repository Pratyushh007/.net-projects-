using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models.Entities;
using WeatherApp.Models.Tools;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

public class ToolsController : Controller
{
    private readonly IWeatherCalculations _calculations;
    private readonly WeatherDbContext _dbContext;

    public ToolsController(IWeatherCalculations calculations, WeatherDbContext dbContext)
    {
        _calculations = calculations;
        _dbContext = dbContext;
    }

    // ==========================================
    // Project 2: Simple Calculator
    // ==========================================
    [HttpGet]
    public IActionResult Calculator(double? temp, double? humidity, double? wind)
    {
        var model = new AtmosphericCalculatorModel();
        if (temp.HasValue) model.TemperatureC = temp.Value;
        if (humidity.HasValue) model.RelativeHumidity = humidity.Value;
        if (wind.HasValue) model.WindSpeedKmh = wind.Value;

        model = _calculations.ComputeAtmosphericMetrics(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Calculator(AtmosphericCalculatorModel model)
    {
        model = _calculations.ComputeAtmosphericMetrics(model);
        return View(model);
    }

    // ==========================================
    // Project 3: Unit Converter
    // ==========================================
    [HttpGet]
    public IActionResult Converter(string? category, double? value, string? from, string? to)
    {
        var model = new UnitConverterModel
        {
            Category = category ?? "Temperature",
            InputValue = value ?? 22.0,
            FromUnit = from ?? "Celsius",
            ToUnit = to ?? "Fahrenheit"
        };

        model = _calculations.ConvertUnits(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Converter(UnitConverterModel model)
    {
        model = _calculations.ConvertUnits(model);
        return View(model);
    }

    // ==========================================
    // Project 8: Password Generator
    // ==========================================
    [HttpGet]
    public IActionResult Password()
    {
        var model = new PasswordGeneratorModel();
        model = _calculations.GenerateWeatherPassword(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Password(PasswordGeneratorModel model)
    {
        model = _calculations.GenerateWeatherPassword(model);
        return View(model);
    }

    // ==========================================
    // Project 7: Bank Account Simulator (Weather Energy Budget)
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> EnergyBudget(string? city = "London", double? outdoorTemp = 9.5)
    {
        var account = await _dbContext.EnergyAccounts
            .Include(a => a.Transactions.OrderByDescending(t => t.Date))
            .FirstOrDefaultAsync();

        if (account == null)
        {
            account = new EnergyAccount { AccountName = "Smart Home Climate Wallet", CurrentBalance = 500m };
            _dbContext.EnergyAccounts.Add(account);
            await _dbContext.SaveChangesAsync();
        }

        var temp = outdoorTemp ?? 9.5;
        var dailyCost = _calculations.EstimateDailyHvacCost(temp, account.TargetIndoorTempC, account.CostPerKwh);

        var vm = new EnergyBankViewModel
        {
            Account = account,
            OutdoorTempC = temp,
            CurrentCity = city ?? "London",
            EstimatedDailyCost = dailyCost,
            ProjectedMonthlyBill = dailyCost * 30,
            EnergySavingTips = GetEnergyTips(temp, account.TargetIndoorTempC)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEnergyTarget(int accountId, double targetTemp, decimal costPerKwh)
    {
        var account = await _dbContext.EnergyAccounts.FindAsync(accountId);
        if (account != null)
        {
            account.TargetIndoorTempC = targetTemp;
            account.CostPerKwh = costPerKwh;
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Thermostat target updated to {targetTemp}°C!";
        }
        return RedirectToAction(nameof(EnergyBudget));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEnergyTransaction(int accountId, string description, decimal amount, bool isDeposit, string reason)
    {
        var account = await _dbContext.EnergyAccounts.FindAsync(accountId);
        if (account != null)
        {
            if (isDeposit)
                account.CurrentBalance += amount;
            else
                account.CurrentBalance -= amount;

            var transaction = new EnergyTransaction
            {
                EnergyAccountId = accountId,
                Description = description,
                Amount = amount,
                IsDeposit = isDeposit,
                WeatherImpactReason = reason,
                Date = DateTime.UtcNow
            };

            _dbContext.EnergyTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Transaction of {amount:C} successfully recorded!";
        }
        return RedirectToAction(nameof(EnergyBudget));
    }

    private static List<string> GetEnergyTips(double outdoorTemp, double targetIndoor)
    {
        var tips = new List<string>();
        double delta = Math.Abs(outdoorTemp - targetIndoor);

        if (outdoorTemp < targetIndoor)
        {
            tips.Add("Keep curtains open during sunny midday hours to capture passive solar heat.");
            tips.Add("Lower thermostat by 1°C while sleeping to reduce heating consumption by ~7%.");
            if (delta > 15)
                tips.Add("Extreme thermal gradient: check door weatherstripping and seal window drafts.");
        }
        else
        {
            tips.Add("Close blinds during peak sunshine hours to minimize solar heat gain.");
            tips.Add("Utilize ceiling fans set counterclockwise to create a wind-chill breeze effect.");
        }

        tips.Add("Schedule heat pump cycles during off-peak electricity tariff hours.");
        return tips;
    }
}
