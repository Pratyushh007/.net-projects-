using System.Text;
using System.Text.Json;
using WeatherApp.Models.Weather;

namespace WeatherApp.Services;

public class FileExportService : IFileExportService
{
    public byte[] ExportWeatherReportAsJson(WeatherViewModel model)
    {
        var exportObject = new
        {
            City = model.CityName,
            Country = model.Country,
            Coordinates = new { model.Latitude, model.Longitude },
            GeneratedAtUtc = DateTime.UtcNow,
            CurrentWeather = new
            {
                TemperatureC = model.CurrentTempC,
                TemperatureF = model.CurrentTempF,
                FeelsLikeC = model.FeelsLikeC,
                Condition = model.ConditionText,
                HumidityPercent = model.Humidity,
                PressureHpa = model.PressureHpa,
                WindSpeedKmh = model.WindSpeedKmh,
                WindDirection = model.WindDirection,
                PrecipitationMm = model.PrecipitationMm,
                UvIndex = model.UvIndex,
                ComfortGrade = model.ComfortGrade,
                ComfortScore = model.ComfortScore
            },
            DailyForecast = model.DailyForecast.Select(d => new
            {
                d.DateLabel,
                d.DayName,
                d.MaxTempC,
                d.MinTempC,
                d.Description,
                d.RainSumMm,
                d.UvMax
            }),
            HourlyNext24Hours = model.HourlyForecast.Select(h => new
            {
                h.TimeLabel,
                h.TempC,
                h.RainProbability
            })
        };

        var json = JsonSerializer.Serialize(exportObject, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
    }

    public byte[] ExportWeatherReportAsCsv(WeatherViewModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Weather Report Export");
        sb.AppendLine($"City,{model.CityName}");
        sb.AppendLine($"Country,{model.Country}");
        sb.AppendLine($"Report Date UTC,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Current Temp (C),{model.CurrentTempC}");
        sb.AppendLine($"Feels Like (C),{model.FeelsLikeC}");
        sb.AppendLine($"Condition,{model.ConditionText}");
        sb.AppendLine($"Humidity (%),{model.Humidity}");
        sb.AppendLine($"Pressure (hPa),{model.PressureHpa}");
        sb.AppendLine($"Wind Speed (km/h),{model.WindSpeedKmh}");
        sb.AppendLine($"Comfort Grade,{model.ComfortGrade}");
        sb.AppendLine();
        sb.AppendLine("7-Day Forecast");
        sb.AppendLine("Day,Date,Condition,Max Temp (C),Min Temp (C),Rain (mm),UV Index");

        foreach (var day in model.DailyForecast)
        {
            sb.AppendLine($"\"{day.DayName}\",\"{day.DateLabel}\",\"{day.Description}\",{day.MaxTempC},{day.MinTempC},{day.RainSumMm},{day.UvMax}");
        }

        sb.AppendLine();
        sb.AppendLine("Hourly Forecast (24h)");
        sb.AppendLine("Time,Temp (C),Rain Probability (%)");
        foreach (var hour in model.HourlyForecast)
        {
            sb.AppendLine($"\"{hour.TimeLabel}\",{hour.TempC},{hour.RainProbability}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportWeatherReportAsText(WeatherViewModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine("==================================================");
        sb.AppendLine($"   ATMOSPHERE 360 - WEATHER REPORT: {model.CityName.ToUpper()}, {model.Country.ToUpper()}");
        sb.AppendLine("==================================================");
        sb.AppendLine($"Generated on: {DateTime.UtcNow:dddd, MMMM dd, yyyy HH:mm} UTC");
        sb.AppendLine($"Coordinates: {model.Latitude:F4}, {model.Longitude:F4} | Timezone: {model.Timezone}");
        sb.AppendLine("--------------------------------------------------");
        sb.AppendLine($"CURRENT STATUS: {model.ConditionText} {model.ConditionIcon}");
        sb.AppendLine($"Temperature:       {model.CurrentTempC:F1} °C ({model.CurrentTempF:F1} °F)");
        sb.AppendLine($"Feels Like:        {model.FeelsLikeC:F1} °C ({model.FeelsLikeF:F1} °F)");
        sb.AppendLine($"Relative Humidity: {model.Humidity:F0} %");
        sb.AppendLine($"Barometric Press:  {model.PressureHpa:F0} hPa");
        sb.AppendLine($"Wind Speed:        {model.WindSpeedKmh:F1} km/h ({model.WindDirection})");
        sb.AppendLine($"UV Index:          {model.UvIndex:F1}");
        sb.AppendLine($"Comfort Rating:    {model.ComfortGrade} ({model.ComfortScore}/100) - {model.ComfortSummary}");
        sb.AppendLine("--------------------------------------------------");
        sb.AppendLine("7-DAY FORECAST SUMMARY:");
        foreach (var day in model.DailyForecast)
        {
            sb.AppendLine($"  * {day.DayName,-9} ({day.DateLabel}): High {day.MaxTempC,5:F1}°C | Low {day.MinTempC,5:F1}°C | {day.Description,-16} | Rain {day.RainSumMm:F1}mm");
        }
        sb.AppendLine("==================================================");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
