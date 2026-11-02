using WeatherApp.Models.Tools;

namespace WeatherApp.Services;

public interface IWeatherCalculations
{
    // Project 2: Simple Calculator
    AtmosphericCalculatorModel ComputeAtmosphericMetrics(AtmosphericCalculatorModel model);

    // Project 3: Unit Converter
    UnitConverterModel ConvertUnits(UnitConverterModel model);

    // Project 8: Password Generator
    PasswordGeneratorModel GenerateWeatherPassword(PasswordGeneratorModel model);

    // Project 7: Bank Account & Energy Cost Simulator
    decimal EstimateDailyHvacCost(double outdoorTempC, double indoorTempC, decimal costPerKwh);
}
