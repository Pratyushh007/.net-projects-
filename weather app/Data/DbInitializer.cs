using WeatherApp.Models.Entities;

namespace WeatherApp.Data;

public static class DbInitializer
{
    public static void Initialize(WeatherDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.FavoriteCities.Any())
        {
            context.FavoriteCities.AddRange(
                new FavoriteCity { CityName = "London", Country = "United Kingdom", Latitude = 51.5074, Longitude = -0.1278, AdminArea = "England" },
                new FavoriteCity { CityName = "New York", Country = "United States", Latitude = 40.7128, Longitude = -74.0060, AdminArea = "New York" },
                new FavoriteCity { CityName = "Tokyo", Country = "Japan", Latitude = 35.6762, Longitude = 139.6503, AdminArea = "Tokyo" },
                new FavoriteCity { CityName = "Sydney", Country = "Australia", Latitude = -33.8688, Longitude = 151.2093, AdminArea = "New South Wales" },
                new FavoriteCity { CityName = "Paris", Country = "France", Latitude = 48.8566, Longitude = 2.3522, AdminArea = "Île-de-France" }
            );
            context.SaveChanges();
        }

        if (!context.EmergencyContacts.Any())
        {
            context.EmergencyContacts.AddRange(
                new EmergencyContact
                {
                    Name = "National Weather Severe Alert Desk",
                    Organization = "Meteorological Disaster Office",
                    Phone = "+1-800-555-WTHR",
                    Email = "alerts@weather-ops.gov",
                    CityOrRegion = "Global",
                    Category = "Emergency Services",
                    PriorityLevel = 1,
                    Notes = "24/7 National hotline for tornado, hurricane, and flash flood alerts"
                },
                new EmergencyContact
                {
                    Name = "Storm Response & Rescue Dispatch",
                    Organization = "Civil Defense & Rescue",
                    Phone = "+44-20-7946-0120",
                    Email = "dispatch@civilrescue.org",
                    CityOrRegion = "London",
                    Category = "Storm Shelter",
                    PriorityLevel = 1,
                    Notes = "Emergency Thames barrier flooding dispatch and sheltering"
                },
                new EmergencyContact
                {
                    Name = "Metropolitan Heatwave Relief Center",
                    Organization = "Public Health Climate Bureau",
                    Phone = "+1-212-555-COOL",
                    Email = "heatrelief@nyc-climate.org",
                    CityOrRegion = "New York",
                    Category = "Medical",
                    PriorityLevel = 2,
                    Notes = "Cooling stations and high-heat emergency assistance"
                },
                new EmergencyContact
                {
                    Name = "Typhoon & Heavy Rain Rapid Response",
                    Organization = "Tokyo Disaster Preparedness",
                    Phone = "+81-3-5555-0199",
                    Email = "safety@bousai.metro.tokyo.jp",
                    CityOrRegion = "Tokyo",
                    Category = "Emergency Services",
                    PriorityLevel = 1,
                    Notes = "Urgent typhoon warnings and evacuation route coordination"
                }
            );
            context.SaveChanges();
        }

        if (!context.WeatherNotes.Any())
        {
            context.WeatherNotes.AddRange(
                new WeatherNote
                {
                    Title = "Summer Heat Wave Checklist",
                    Content = "Checked AC filters, closed blinds during peak sunlight hours between 12:00 and 16:00, stocked up on electrolyte hydration.",
                    CityName = "New York",
                    TemperatureC = 31.5,
                    Condition = "Sunny",
                    Category = "Daily",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new WeatherNote
                {
                    Title = "Golden Hour Photography Session",
                    Content = "Scouted Westminster Bridge during light cloud cover. Sunset diffusion created stunning purple and amber reflections on the Thames.",
                    CityName = "London",
                    TemperatureC = 16.2,
                    Condition = "Partly Cloudy",
                    Category = "Photography",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new WeatherNote
                {
                    Title = "Garden Frost Protection Plan",
                    Content = "Covered tomato beds and sensitive succulents with horticultural fleece ahead of sudden evening freeze alert.",
                    CityName = "Paris",
                    TemperatureC = 4.1,
                    Condition = "Clear Sky",
                    Category = "Agriculture",
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                }
            );
            context.SaveChanges();
        }

        if (!context.WeatherTodoItems.Any())
        {
            context.WeatherTodoItems.AddRange(
                new WeatherTodoItem
                {
                    TaskDescription = "Morning 5K jog in Hyde Park",
                    TargetCity = "London",
                    RequiredWeatherType = "Mild",
                    MinTemp = 12,
                    MaxTemp = 22,
                    RequiresNoRain = true,
                    Priority = "High",
                    IsCompleted = false
                },
                new WeatherTodoItem
                {
                    TaskDescription = "Clean rooftop solar panels & gutters",
                    TargetCity = "New York",
                    RequiredWeatherType = "Clear",
                    MinTemp = 15,
                    RequiresNoRain = true,
                    Priority = "Normal",
                    IsCompleted = false
                },
                new WeatherTodoItem
                {
                    TaskDescription = "Inspect storm drain covers before weekend precipitation",
                    TargetCity = "Tokyo",
                    RequiredWeatherType = "Any",
                    RequiresNoRain = false,
                    Priority = "High",
                    IsCompleted = true
                }
            );
            context.SaveChanges();
        }

        if (!context.EnergyAccounts.Any())
        {
            var account = new EnergyAccount
            {
                AccountName = "Smart Home Climate Wallet",
                CurrentBalance = 420.50m,
                MonthlyBudget = 200.00m,
                TargetIndoorTempC = 21.5,
                CostPerKwh = 0.16m
            };
            context.EnergyAccounts.Add(account);
            context.SaveChanges();

            context.EnergyTransactions.AddRange(
                new EnergyTransaction
                {
                    EnergyAccountId = account.Id,
                    Description = "Monthly Energy Budget Allocation",
                    Amount = 200.00m,
                    IsDeposit = true,
                    WeatherImpactReason = "Regular monthly utility reserve allowance",
                    Date = DateTime.UtcNow.AddDays(-15)
                },
                new EnergyTransaction
                {
                    EnergyAccountId = account.Id,
                    Description = "Heat Pump Heating Cycle (Chilly spell)",
                    Amount = 45.20m,
                    IsDeposit = false,
                    WeatherImpactReason = "Outside temp dropped to 6°C; automated thermostat maintained 21°C",
                    Date = DateTime.UtcNow.AddDays(-10)
                },
                new EnergyTransaction
                {
                    EnergyAccountId = account.Id,
                    Description = "Solar Excess Grid Feedback Credit",
                    Amount = 28.75m,
                    IsDeposit = true,
                    WeatherImpactReason = "High UV & clear sky generated 18 kWh surplus power",
                    Date = DateTime.UtcNow.AddDays(-3)
                }
            );
            context.SaveChanges();
        }
    }
}
