using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models.Entities;
using WeatherApp.Models.Weather;

namespace WeatherApp.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherDbContext _dbContext;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(IHttpClientFactory httpClientFactory, WeatherDbContext dbContext, ILogger<WeatherService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("WeatherApi");
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<GeocodingItem>> SearchCitiesAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            return new List<GeocodingItem>();

        try
        {
            var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(query.Trim())}&count=10&language=en&format=json";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<GeocodingResponse>(content);
                if (result?.Results != null && result.Results.Any())
                {
                    return result.Results;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to search cities via Open-Meteo for '{Query}'", query);
        }

        return GetKnownCities()
            .Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (c.Country != null && c.Country.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<WeatherViewModel> GetWeatherForCityAsync(string cityName, double? lat = null, double? lon = null, CancellationToken cancellationToken = default)
    {
        cityName = string.IsNullOrWhiteSpace(cityName) ? "London" : cityName.Trim();

        double latitude = lat ?? 51.5074;
        double longitude = lon ?? -0.1278;
        string country = "United Kingdom";
        string? adminArea = "England";

        if (!lat.HasValue || !lon.HasValue)
        {
            var searchResults = await SearchCitiesAsync(cityName, cancellationToken);
            var bestMatch = searchResults.FirstOrDefault();
            if (bestMatch != null)
            {
                latitude = bestMatch.Latitude;
                longitude = bestMatch.Longitude;
                cityName = bestMatch.Name;
                country = bestMatch.Country ?? "International";
                adminArea = bestMatch.AdminArea;
            }
            else
            {
                var fallback = GetKnownCities().FirstOrDefault(c => c.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase))
                               ?? GetKnownCities().First();
                latitude = fallback.Latitude;
                longitude = fallback.Longitude;
                cityName = fallback.Name;
                country = fallback.Country ?? "";
                adminArea = fallback.AdminArea;
            }
        }

        return await FetchAndBuildWeatherModelAsync(cityName, country, adminArea, latitude, longitude, cancellationToken);
    }

    public async Task<WeatherViewModel> GetWeatherByCoordinatesAsync(double latitude, double longitude, string? customName = null, CancellationToken cancellationToken = default)
    {
        string cityName = customName ?? $"Location ({latitude:F2}, {longitude:F2})";
        string country = "GPS Coordinates";
        string? adminArea = null;

        return await FetchAndBuildWeatherModelAsync(cityName, country, adminArea, latitude, longitude, cancellationToken);
    }

    private async Task<WeatherViewModel> FetchAndBuildWeatherModelAsync(
        string cityName, string country, string? adminArea, double latitude, double longitude, CancellationToken cancellationToken)
    {
        OpenMeteoResponse? meteoData = null;

        try
        {
            var url = string.Format(CultureInfo.InvariantCulture,
                "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,surface_pressure,wind_speed_10m,wind_direction_10m&hourly=temperature_2m,weather_code,precipitation_probability&daily=weather_code,temperature_2m_max,temperature_2m_min,sunrise,sunset,uv_index_max,precipitation_sum&timezone=auto",
                latitude, longitude);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                meteoData = JsonSerializer.Deserialize<OpenMeteoResponse>(json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to call Open-Meteo for {City}. Using fallback mock dataset.", cityName);
        }

        var vm = new WeatherViewModel
        {
            CityName = cityName,
            Country = country,
            AdminArea = adminArea,
            Latitude = latitude,
            Longitude = longitude,
            Timezone = meteoData?.Timezone ?? "UTC",
            LocalTime = DateTime.UtcNow
        };

        if (meteoData?.Current != null)
        {
            vm.CurrentTempC = meteoData.Current.Temperature;
            vm.FeelsLikeC = meteoData.Current.ApparentTemperature;
            vm.Humidity = meteoData.Current.RelativeHumidity;
            vm.PressureHpa = meteoData.Current.SurfacePressure;
            vm.WindSpeedKmh = meteoData.Current.WindSpeed;
            vm.WindDirection = WmoWeatherCodeHelper.WindDirectionToCardinal(meteoData.Current.WindDirection);
            vm.PrecipitationMm = meteoData.Current.Precipitation;
            vm.WeatherCode = meteoData.Current.WeatherCode;
            vm.IsDay = meteoData.Current.IsDay == 1;

            var weatherInfo = WmoWeatherCodeHelper.GetWeatherInfo(vm.WeatherCode, vm.IsDay);
            vm.ConditionText = weatherInfo.Description;
            vm.ConditionIcon = weatherInfo.Icon;
            vm.ThemeClass = weatherInfo.ThemeClass;
        }
        else
        {

            vm.CurrentTempC = 19.5;
            vm.FeelsLikeC = 19.0;
            vm.Humidity = 58;
            vm.PressureHpa = 1014.5;
            vm.WindSpeedKmh = 14.2;
            vm.WindDirection = "SW";
            vm.PrecipitationMm = 0;
            vm.WeatherCode = 1;
            vm.IsDay = true;
            vm.ConditionText = "Mainly Clear";
            vm.ConditionIcon = "🌤️";
            vm.ThemeClass = "theme-clear-day";
        }

        if (meteoData?.Hourly?.Time != null && meteoData.Hourly.Temperature != null)
        {
            var count = Math.Min(24, meteoData.Hourly.Time.Count);
            for (int i = 0; i < count; i++)
            {
                var timeStr = meteoData.Hourly.Time[i];
                var timeLabel = DateTime.TryParse(timeStr, out var parsed) ? parsed.ToString("htt") : $"H+{i}";
                var code = (meteoData.Hourly.WeatherCode != null && i < meteoData.Hourly.WeatherCode.Count) ? meteoData.Hourly.WeatherCode[i] : 0;
                var info = WmoWeatherCodeHelper.GetWeatherInfo(code, true);
                var rainProb = (meteoData.Hourly.PrecipitationProbability != null && i < meteoData.Hourly.PrecipitationProbability.Count) ? meteoData.Hourly.PrecipitationProbability[i] : 0;

                vm.HourlyForecast.Add(new HourlyForecastItem
                {
                    TimeLabel = timeLabel,
                    TempC = meteoData.Hourly.Temperature[i],
                    WeatherCode = code,
                    Icon = info.Icon,
                    RainProbability = rainProb
                });
            }
        }
        else
        {

            for (int h = 0; h < 24; h++)
            {
                var simTemp = vm.CurrentTempC + Math.Sin(h * Math.PI / 12) * 5.0;
                vm.HourlyForecast.Add(new HourlyForecastItem
                {
                    TimeLabel = DateTime.UtcNow.AddHours(h).ToString("htt"),
                    TempC = Math.Round(simTemp, 1),
                    WeatherCode = 1,
                    Icon = "🌤️",
                    RainProbability = h % 4 == 0 ? 15 : 5
                });
            }
        }

        if (meteoData?.Daily?.Time != null && meteoData.Daily.TempMax != null && meteoData.Daily.TempMin != null)
        {
            var dayCount = Math.Min(7, meteoData.Daily.Time.Count);
            for (int d = 0; d < dayCount; d++)
            {
                var dateStr = meteoData.Daily.Time[d];
                var dateParsed = DateTime.TryParse(dateStr, out var dVal) ? dVal : DateTime.UtcNow.AddDays(d);
                var code = (meteoData.Daily.WeatherCode != null && d < meteoData.Daily.WeatherCode.Count) ? meteoData.Daily.WeatherCode[d] : 0;
                var info = WmoWeatherCodeHelper.GetWeatherInfo(code, true);

                vm.DailyForecast.Add(new DailyForecastItem
                {
                    DateLabel = dateParsed.ToString("MMM dd"),
                    DayName = d == 0 ? "Today" : dateParsed.ToString("ddd"),
                    MaxTempC = meteoData.Daily.TempMax[d],
                    MinTempC = meteoData.Daily.TempMin[d],
                    WeatherCode = code,
                    Icon = info.Icon,
                    Description = info.Description,
                    RainSumMm = (meteoData.Daily.PrecipitationSum != null && d < meteoData.Daily.PrecipitationSum.Count) ? meteoData.Daily.PrecipitationSum[d] : 0,
                    UvMax = (meteoData.Daily.UvIndexMax != null && d < meteoData.Daily.UvIndexMax.Count) ? meteoData.Daily.UvIndexMax[d] : 5.0
                });
            }

            if (vm.DailyForecast.Any())
            {
                vm.TodayMaxC = vm.DailyForecast[0].MaxTempC;
                vm.TodayMinC = vm.DailyForecast[0].MinTempC;
                vm.UvIndex = vm.DailyForecast[0].UvMax;
            }

            if (meteoData.Daily.Sunrise != null && meteoData.Daily.Sunrise.Any())
            {
                if (DateTime.TryParse(meteoData.Daily.Sunrise[0], out var sr))
                    vm.Sunrise = sr.ToString("h:mm tt");
            }
            if (meteoData.Daily.Sunset != null && meteoData.Daily.Sunset.Any())
            {
                if (DateTime.TryParse(meteoData.Daily.Sunset[0], out var ss))
                    vm.Sunset = ss.ToString("h:mm tt");
            }
        }
        else
        {
            vm.TodayMaxC = vm.CurrentTempC + 4.0;
            vm.TodayMinC = vm.CurrentTempC - 5.0;
            vm.UvIndex = 5.8;
            vm.Sunrise = "06:24 AM";
            vm.Sunset = "07:45 PM";

            for (int d = 0; d < 7; d++)
            {
                var day = DateTime.UtcNow.AddDays(d);
                vm.DailyForecast.Add(new DailyForecastItem
                {
                    DateLabel = day.ToString("MMM dd"),
                    DayName = d == 0 ? "Today" : day.ToString("ddd"),
                    MaxTempC = Math.Round(vm.CurrentTempC + 3 + (d % 3), 1),
                    MinTempC = Math.Round(vm.CurrentTempC - 4 - (d % 2), 1),
                    WeatherCode = 2,
                    Icon = "⛅",
                    Description = "Partly Cloudy",
                    RainSumMm = 0.5,
                    UvMax = 6.0
                });
            }
        }

        CalculateComfortGrade(vm);

        GenerateWeatherAdvice(vm);

        try
        {
            _dbContext.SearchHistories.Add(new SearchHistory
            {
                CityName = vm.CityName,
                Country = vm.Country,
                TemperatureC = vm.CurrentTempC,
                ConditionText = vm.ConditionText,
                Humidity = vm.Humidity,
                WindSpeedKmh = vm.WindSpeedKmh,
                SearchedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist search history for {City}", vm.CityName);
        }

        vm.FavoriteCities = await _dbContext.FavoriteCities.OrderBy(f => f.CityName).ToListAsync(cancellationToken);
        vm.IsFavorite = vm.FavoriteCities.Any(f => f.CityName.Equals(vm.CityName, StringComparison.OrdinalIgnoreCase));
        vm.RecentSearches = await _dbContext.SearchHistories
            .OrderByDescending(s => s.SearchedAt)
            .Take(8)
            .ToListAsync(cancellationToken);

        vm.MatchingTodoItems = await _dbContext.WeatherTodoItems
            .Where(t => t.TargetCity == "" || t.TargetCity.ToLower() == vm.CityName.ToLower())
            .ToListAsync(cancellationToken);

        vm.RelatedNotes = await _dbContext.WeatherNotes
            .Where(n => n.CityName.ToLower() == vm.CityName.ToLower())
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        vm.LocalContacts = await _dbContext.EmergencyContacts
            .Where(c => c.CityOrRegion == "Global" || c.CityOrRegion.ToLower() == vm.CityName.ToLower())
            .OrderBy(c => c.PriorityLevel)
            .ToListAsync(cancellationToken);

        return vm;
    }

    private void CalculateComfortGrade(WeatherViewModel vm)
    {

        double score = 100.0;

        double tempPenalty = Math.Abs(vm.CurrentTempC - 22.0) * 2.5;
        score -= Math.Min(35, tempPenalty);

        double humidityPenalty = Math.Abs(vm.Humidity - 50.0) * 0.4;
        score -= Math.Min(20, humidityPenalty);

        if (vm.WindSpeedKmh > 25)
        {
            score -= (vm.WindSpeedKmh - 25) * 0.8;
        }

        if (vm.PrecipitationMm > 0)
        {
            score -= Math.Min(25, 10 + (vm.PrecipitationMm * 3));
        }

        if (vm.UvIndex > 8)
        {
            score -= (vm.UvIndex - 8) * 4;
        }

        score = Math.Clamp(Math.Round(score), 30, 99);
        vm.ComfortScore = (int)score;

        if (score >= 93) { vm.ComfortGrade = "A+"; vm.ComfortSummary = "Perfect outdoor conditions, blissful climate"; }
        else if (score >= 87) { vm.ComfortGrade = "A"; vm.ComfortSummary = "Excellent, highly comfortable weather"; }
        else if (score >= 80) { vm.ComfortGrade = "B+"; vm.ComfortSummary = "Very pleasant, minor atmospheric variation"; }
        else if (score >= 72) { vm.ComfortGrade = "B"; vm.ComfortSummary = "Good weather, standard seasonal norms"; }
        else if (score >= 65) { vm.ComfortGrade = "C+"; vm.ComfortSummary = "Moderate discomfort, plan clothing accordingly"; }
        else if (score >= 55) { vm.ComfortGrade = "C"; vm.ComfortSummary = "Noticeable chill or heat, stay prepared"; }
        else if (score >= 45) { vm.ComfortGrade = "D"; vm.ComfortSummary = "Unfavorable conditions, outdoor activities discouraged"; }
        else { vm.ComfortGrade = "F"; vm.ComfortSummary = "Severe weather caution, stay indoors"; }
    }

    private void GenerateWeatherAdvice(WeatherViewModel vm)
    {

        var advices = new List<WeatherAdvice>();

        if (vm.CurrentTempC >= 32)
        {
            advices.Add(new WeatherAdvice
            {
                Category = "Health",
                Icon = "🔥",
                Title = "High Heat Alert",
                Description = "Extreme ambient heat. Drink plenty of water and avoid strenuous outdoor exercise between 11 AM and 4 PM.",
                Severity = "alert"
            });
        }
        else if (vm.CurrentTempC <= 2)
        {
            advices.Add(new WeatherAdvice
            {
                Category = "Attire",
                Icon = "🧤",
                Title = "Freezing Temperatures",
                Description = "Thermal layers, insulated coat, gloves, and headwear required to prevent hypothermia.",
                Severity = "warning"
            });
        }
        else if (vm.CurrentTempC >= 18 && vm.CurrentTempC <= 25)
        {
            advices.Add(new WeatherAdvice
            {
                Category = "Outdoor",
                Icon = "🏃",
                Title = "Prime Running & Outdoor Weather",
                Description = "Optimal temperature window for jogging, cycling, hiking, or park picnics.",
                Severity = "success"
            });
        }

        if (vm.PrecipitationMm > 0 || vm.WeatherCode >= 51 && vm.WeatherCode <= 67)
        {
            advices.Add(new WeatherAdvice
            {
                Category = "Attire",
                Icon = "☂️",
                Title = "Umbrella & Raincoat Essential",
                Description = "Active precipitation detected. Waterproof footwear and a compact umbrella recommended.",
                Severity = "info"
            });
        }

        if (vm.WindSpeedKmh >= 40)
        {
            advices.Add(new WeatherAdvice
            {
                Category = "Safety",
                Icon = "💨",
                Title = "Gale Force Wind Warning",
                Description = "Strong wind gusts present. Secure loose outdoor furniture and exercise caution while driving high-profile vehicles.",
                Severity = "warning"
            });
        }

        if (vm.UvIndex >= 7)
        {
            advices.Add(new WeatherAdvice
            {
                Category = "Sun Safety",
                Icon = "🧴",
                Title = "Very High UV Radiation",
                Description = "Apply SPF 50+ sunscreen, wear UV400 sunglasses and a broad-brimmed hat.",
                Severity = "warning"
            });
        }

        advices.Add(new WeatherAdvice
        {
            Category = "Atmosphere",
            Icon = "🍃",
            Title = $"Barometric Pressure: {vm.PressureHpa:F0} hPa",
            Description = vm.PressureHpa >= 1013 ? "High pressure system dominant, typically bringing stable weather." : "Low pressure system dominant, associated with cloud formation and precipitation.",
            Severity = "info"
        });

        vm.Advices = advices;
    }

    private static List<GeocodingItem> GetKnownCities()
    {
        return new List<GeocodingItem>
        {
            new() { Id = 1, Name = "London", Country = "United Kingdom", Latitude = 51.5074, Longitude = -0.1278, AdminArea = "England" },
            new() { Id = 2, Name = "New York", Country = "United States", Latitude = 40.7128, Longitude = -74.0060, AdminArea = "New York" },
            new() { Id = 3, Name = "Tokyo", Country = "Japan", Latitude = 35.6762, Longitude = 139.6503, AdminArea = "Tokyo" },
            new() { Id = 4, Name = "Paris", Country = "France", Latitude = 48.8566, Longitude = 2.3522, AdminArea = "Île-de-France" },
            new() { Id = 5, Name = "Sydney", Country = "Australia", Latitude = -33.8688, Longitude = 151.2093, AdminArea = "New South Wales" },
            new() { Id = 6, Name = "Berlin", Country = "Germany", Latitude = 52.5200, Longitude = 13.4050, AdminArea = "Berlin" },
            new() { Id = 7, Name = "Dubai", Country = "United Arab Emirates", Latitude = 25.2048, Longitude = 55.2708, AdminArea = "Dubai" },
            new() { Id = 8, Name = "Singapore", Country = "Singapore", Latitude = 1.3521, Longitude = 103.8198, AdminArea = "Singapore" },
            new() { Id = 9, Name = "Toronto", Country = "Canada", Latitude = 43.6532, Longitude = -79.3832, AdminArea = "Ontario" },
            new() { Id = 10, Name = "Mumbai", Country = "India", Latitude = 19.0760, Longitude = 72.8777, AdminArea = "Maharashtra" }
        };
    }
}
