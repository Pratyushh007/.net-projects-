namespace WeatherApp.Models.Weather;

public record WeatherInfo(string Description, string Icon, string ThemeClass, string LottieOrCssType);

public static class WmoWeatherCodeHelper
{
    public static WeatherInfo GetWeatherInfo(int code, bool isDay = true)
    {
        return code switch
        {
            0 => isDay 
                ? new WeatherInfo("Clear Sky", "☀️", "theme-clear-day", "sunny")
                : new WeatherInfo("Clear Night", "🌙", "theme-clear-night", "clear-night"),
            1 => isDay
                ? new WeatherInfo("Mainly Clear", "🌤️", "theme-clear-day", "sunny")
                : new WeatherInfo("Mainly Clear", "🌕", "theme-clear-night", "clear-night"),
            2 => new WeatherInfo("Partly Cloudy", "⛅", "theme-clouds", "cloudy"),
            3 => new WeatherInfo("Overcast", "☁️", "theme-overcast", "cloudy"),
            45 => new WeatherInfo("Foggy", "🌫️", "theme-fog", "fog"),
            48 => new WeatherInfo("Depositing Rime Fog", "🌫️", "theme-fog", "fog"),
            51 => new WeatherInfo("Light Drizzle", "🌦️", "theme-rain", "rain"),
            53 => new WeatherInfo("Moderate Drizzle", "🌦️", "theme-rain", "rain"),
            55 => new WeatherInfo("Dense Drizzle", "🌧️", "theme-rain", "rain"),
            56 => new WeatherInfo("Light Freezing Drizzle", "🌨️", "theme-snow", "snow"),
            57 => new WeatherInfo("Dense Freezing Drizzle", "🌨️", "theme-snow", "snow"),
            61 => new WeatherInfo("Slight Rain", "🌧️", "theme-rain", "rain"),
            63 => new WeatherInfo("Moderate Rain", "🌧️", "theme-rain", "rain"),
            65 => new WeatherInfo("Heavy Rain", "🌧️🌧️", "theme-heavy-rain", "rain"),
            66 => new WeatherInfo("Light Freezing Rain", "🧊🌧️", "theme-snow", "snow"),
            67 => new WeatherInfo("Heavy Freezing Rain", "🧊🌧️", "theme-snow", "snow"),
            71 => new WeatherInfo("Slight Snow Fall", "❄️", "theme-snow", "snow"),
            73 => new WeatherInfo("Moderate Snow Fall", "❄️", "theme-snow", "snow"),
            75 => new WeatherInfo("Heavy Snow Fall", "❄️❄️", "theme-snow", "snow"),
            77 => new WeatherInfo("Snow Grains", "🌨️", "theme-snow", "snow"),
            80 => new WeatherInfo("Slight Rain Showers", "🌦️", "theme-rain", "rain"),
            81 => new WeatherInfo("Moderate Rain Showers", "🌧️", "theme-rain", "rain"),
            82 => new WeatherInfo("Violent Rain Showers", "⛈️", "theme-heavy-rain", "thunder"),
            85 => new WeatherInfo("Slight Snow Showers", "🌨️", "theme-snow", "snow"),
            86 => new WeatherInfo("Heavy Snow Showers", "🌨️❄️", "theme-snow", "snow"),
            95 => new WeatherInfo("Thunderstorm", "⛈️", "theme-thunder", "thunder"),
            96 => new WeatherInfo("Thunderstorm with Slight Hail", "⛈️⚡", "theme-thunder", "thunder"),
            99 => new WeatherInfo("Severe Thunderstorm with Heavy Hail", "⛈️⚡🌩️", "theme-thunder", "thunder"),
            _ => new WeatherInfo("Atmospheric", "🌥️", "theme-clouds", "cloudy")
        };
    }

    public static string WindDirectionToCardinal(double degrees)
    {
        string[] cardinals = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
        int index = (int)Math.Round(((degrees % 360) / 22.5)) % 16;
        return cardinals[index];
    }
}
