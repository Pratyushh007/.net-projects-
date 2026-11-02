using System.Text.Json.Serialization;

namespace WeatherApp.Models.Weather;

public class GeocodingResponse
{
    [JsonPropertyName("results")]
    public List<GeocodingItem>? Results { get; set; }
}

public class GeocodingItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("admin1")]
    public string? AdminArea { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("population")]
    public long? Population { get; set; }
}

public class OpenMeteoResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("current")]
    public CurrentWeatherDto? Current { get; set; }

    [JsonPropertyName("hourly")]
    public HourlyWeatherDto? Hourly { get; set; }

    [JsonPropertyName("daily")]
    public DailyWeatherDto? Daily { get; set; }
}

public class CurrentWeatherDto
{
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public double RelativeHumidity { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }

    [JsonPropertyName("precipitation")]
    public double Precipitation { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }

    [JsonPropertyName("surface_pressure")]
    public double SurfacePressure { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }

    [JsonPropertyName("wind_direction_10m")]
    public double WindDirection { get; set; }
}

public class HourlyWeatherDto
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("temperature_2m")]
    public List<double>? Temperature { get; set; }

    [JsonPropertyName("weather_code")]
    public List<int>? WeatherCode { get; set; }

    [JsonPropertyName("precipitation_probability")]
    public List<int>? PrecipitationProbability { get; set; }
}

public class DailyWeatherDto
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("weather_code")]
    public List<int>? WeatherCode { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public List<double>? TempMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public List<double>? TempMin { get; set; }

    [JsonPropertyName("sunrise")]
    public List<string>? Sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public List<string>? Sunset { get; set; }

    [JsonPropertyName("uv_index_max")]
    public List<double>? UvIndexMax { get; set; }

    [JsonPropertyName("precipitation_sum")]
    public List<double>? PrecipitationSum { get; set; }
}
