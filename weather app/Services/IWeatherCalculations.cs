using WeatherApp.Models.Tools;

namespace WeatherApp.Services;

public interface IWeatherCalculations
{

    AtmosphericCalculatorModel ComputeAtmosphericMetrics(AtmosphericCalculatorModel model);

    UnitConverterModel ConvertUnits(UnitConverterModel model);

    PasswordGeneratorModel GenerateWeatherPassword(PasswordGeneratorModel model);

    decimal EstimateDailyHvacCost(double outdoorTempC, double indoorTempC, decimal costPerKwh);
}
