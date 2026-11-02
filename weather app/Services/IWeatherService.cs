using WeatherApp.Models.Weather;

namespace WeatherApp.Services;

public interface IWeatherService
{
    Task<List<GeocodingItem>> SearchCitiesAsync(string query, CancellationToken cancellationToken = default);
    Task<WeatherViewModel> GetWeatherForCityAsync(string cityName, double? lat = null, double? lon = null, CancellationToken cancellationToken = default);
    Task<WeatherViewModel> GetWeatherByCoordinatesAsync(double latitude, double longitude, string? customName = null, CancellationToken cancellationToken = default);
}
