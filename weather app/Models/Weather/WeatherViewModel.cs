using WeatherApp.Models.Entities;

namespace WeatherApp.Models.Weather;

public class WeatherViewModel
{
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? AdminArea { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; } = "UTC";
    public DateTime LocalTime { get; set; } = DateTime.UtcNow;

    public double CurrentTempC { get; set; }
    public double CurrentTempF => Math.Round(CurrentTempC * 9 / 5 + 32, 1);
    public double FeelsLikeC { get; set; }
    public double FeelsLikeF => Math.Round(FeelsLikeC * 9 / 5 + 32, 1);
    public double Humidity { get; set; }
    public double PressureHpa { get; set; }
    public double WindSpeedKmh { get; set; }
    public double WindSpeedMph => Math.Round(WindSpeedKmh * 0.621371, 1);
    public string WindDirection { get; set; } = "N";
    public double PrecipitationMm { get; set; }
    public int WeatherCode { get; set; }
    public string ConditionText { get; set; } = string.Empty;
    public string ConditionIcon { get; set; } = string.Empty;
    public string ThemeClass { get; set; } = "theme-clear-day";
    public bool IsDay { get; set; } = true;

    public double TodayMaxC { get; set; }
    public double TodayMinC { get; set; }
    public double UvIndex { get; set; }
    public string Sunrise { get; set; } = string.Empty;
    public string Sunset { get; set; } = string.Empty;

    public string ComfortGrade { get; set; } = "B+";
    public int ComfortScore { get; set; } = 85;
    public string ComfortSummary { get; set; } = "Pleasant conditions";

    public List<HourlyForecastItem> HourlyForecast { get; set; } = new();
    public List<DailyForecastItem> DailyForecast { get; set; } = new();

    public bool IsFavorite { get; set; }
    public List<FavoriteCity> FavoriteCities { get; set; } = new();
    public List<SearchHistory> RecentSearches { get; set; } = new();

    public List<WeatherTodoItem> MatchingTodoItems { get; set; } = new();
    public List<WeatherNote> RelatedNotes { get; set; } = new();
    public List<EmergencyContact> LocalContacts { get; set; } = new();

    public List<WeatherAdvice> Advices { get; set; } = new();
}

public class HourlyForecastItem
{
    public string TimeLabel { get; set; } = string.Empty;
    public double TempC { get; set; }
    public double TempF => Math.Round(TempC * 9 / 5 + 32, 1);
    public int WeatherCode { get; set; }
    public string Icon { get; set; } = "☀️";
    public int RainProbability { get; set; }
}

public class DailyForecastItem
{
    public string DateLabel { get; set; } = string.Empty;
    public string DayName { get; set; } = string.Empty;
    public double MaxTempC { get; set; }
    public double MinTempC { get; set; }
    public int WeatherCode { get; set; }
    public string Icon { get; set; } = "☀️";
    public string Description { get; set; } = string.Empty;
    public double RainSumMm { get; set; }
    public double UvMax { get; set; }
}

public class WeatherAdvice
{
    public string Category { get; set; } = "General";
    public string Icon { get; set; } = "💡";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
}
