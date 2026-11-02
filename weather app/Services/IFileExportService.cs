using WeatherApp.Models.Weather;

namespace WeatherApp.Services;

public interface IFileExportService
{
    byte[] ExportWeatherReportAsJson(WeatherViewModel model);
    byte[] ExportWeatherReportAsCsv(WeatherViewModel model);
    byte[] ExportWeatherReportAsText(WeatherViewModel model);
}
