using WeatherApp.Models.Tools;

namespace WeatherApp.Services;

public interface IGradeManagerService
{
    Task<GradeReportCardViewModel> GenerateReportCardAsync(CancellationToken cancellationToken = default);
    CityGradeModel GradeSingleCity(string cityName, string country, double tempC, double humidity, double windSpeed, double uv, double rain);
}
