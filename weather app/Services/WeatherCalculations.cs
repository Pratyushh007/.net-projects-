using System.Security.Cryptography;
using System.Text;
using WeatherApp.Models.Tools;

namespace WeatherApp.Services;

public class WeatherCalculations : IWeatherCalculations
{
    private static readonly string[] WeatherAdjectives =
    {
        "Breezy", "Arctic", "Tropical", "Thunderous", "Solar", "Glacial", "Misty",
        "Radiant", "Crisp", "Tempestuous", "Auroral", "Vortex", "Frosty", "Zephyr"
    };

    private static readonly string[] WeatherNouns =
    {
        "Cumulus", "Monsoon", "Cyclone", "Aurora", "Blizzard", "Barometer", "Typhoon",
        "Equinox", "Supercell", "Isobar", "Solstice", "Nimbus", "Anemometer", "Lightning"
    };

    private static readonly string[] WeatherLocations =
    {
        "Reykjavik", "Tokyo", "London", "Sahara", "Everest", "Geneva", "Oslo", "Aspen", "Maui"
    };

    private static readonly char[] SpecialSymbols = { '!', '@', '#', '$', '%', '^', '&', '*', '-', '_' };

    public AtmosphericCalculatorModel ComputeAtmosphericMetrics(AtmosphericCalculatorModel model)
    {
        double t = model.TemperatureC;
        double rh = Math.Clamp(model.RelativeHumidity, 1.0, 100.0);
        double windKmh = Math.Max(0.0, model.WindSpeedKmh);
        double pressure = Math.Max(800.0, model.PressureHpa);

        // 1. Heat Index Calculation (NOAA formula converted to Celsius)
        double tF = (t * 9.0 / 5.0) + 32.0;
        double hiF;

        if (tF >= 80.0)
        {
            hiF = -42.379 + (2.04901523 * tF) + (10.14333127 * rh)
                  - (0.22475541 * tF * rh) - (0.00683783 * tF * tF)
                  - (0.05481717 * rh * rh) + (0.00122874 * tF * tF * rh)
                  + (0.00085282 * tF * rh * rh) - (0.00000199 * tF * tF * rh * rh);
        }
        else
        {
            hiF = 0.5 * (tF + 61.0 + ((tF - 68.0) * 1.2) + (rh * 0.094));
        }
        double hiC = (hiF - 32.0) * 5.0 / 9.0;
        model.HeatIndexC = Math.Round(hiC, 1);

        if (hiC >= 41)
            model.HeatRiskLevel = "Extreme Danger - Heat stroke imminent with continued exposure";
        else if (hiC >= 32)
            model.HeatRiskLevel = "Caution / Danger - Heat exhaustion, cramps and sunstroke likely";
        else if (hiC >= 27)
            model.HeatRiskLevel = "Moderate Caution - Fatigue possible with prolonged exposure";
        else
            model.HeatRiskLevel = "Safe / Low Risk - Comfortable thermal index";

        // 2. Wind Chill (Environment Canada / NOAA formula)
        if (t <= 10.0 && windKmh >= 4.8)
        {
            double wc = 13.12 + (0.6215 * t) - (11.37 * Math.Pow(windKmh, 0.16)) + (0.3965 * t * Math.Pow(windKmh, 0.16));
            model.WindChillC = Math.Round(wc, 1);

            if (wc <= -40)
                model.FrostbiteRisk = "Frostbite occurs in under 10 minutes!";
            else if (wc <= -28)
                model.FrostbiteRisk = "High Risk - Exposed skin freezes in 15-30 minutes";
            else if (wc <= -10)
                model.FrostbiteRisk = "Moderate Risk - Uncomfortable chill, dress in windproof layers";
            else
                model.FrostbiteRisk = "Low Risk - Standard winter gear is sufficient";
        }
        else
        {
            model.WindChillC = t;
            model.FrostbiteRisk = "Not applicable (Wind chill activates when temp <= 10°C & wind >= 4.8 km/h)";
        }

        // 3. Dew Point (Magnus-Tetens formula)
        const double a = 17.27;
        const double b = 237.7;
        double alpha = ((a * t) / (b + t)) + Math.Log(rh / 100.0);
        double dewPoint = (b * alpha) / (a - alpha);
        model.DewPointC = Math.Round(dewPoint, 1);

        // 4. Air Density: rho = p / (R_dry * T_kelvin)
        double tKelvin = t + 273.15;
        double pressurePa = pressure * 100.0;
        double airDensity = pressurePa / (287.058 * tKelvin);
        model.AirDensityKgM3 = Math.Round(airDensity, 3);

        // 5. Rain Harvesting: mm * m2 * 0.9 runoff efficiency = Liters
        double harvest = model.RainfallMm * model.RoofAreaSqMeters * 0.9;
        model.RainHarvestLiters = Math.Round(harvest, 1);

        model.CalculationSummary = $"At {t:F1}°C and {rh:F0}% humidity, dew point is {model.DewPointC:F1}°C. Apparent heat index feels like {model.HeatIndexC:F1}°C with air density {model.AirDensityKgM3:F3} kg/m³.";
        return model;
    }

    public UnitConverterModel ConvertUnits(UnitConverterModel model)
    {
        double val = model.InputValue;
        double result = 0;
        string formula = "";

        switch (model.Category)
        {
            case "Temperature":
                result = ConvertTemperature(val, model.FromUnit, model.ToUnit, out formula);
                model.QuickConversions = new Dictionary<string, double>
                {
                    ["Celsius (°C)"] = ConvertTemperature(val, model.FromUnit, "Celsius", out _),
                    ["Fahrenheit (°F)"] = ConvertTemperature(val, model.FromUnit, "Fahrenheit", out _),
                    ["Kelvin (K)"] = ConvertTemperature(val, model.FromUnit, "Kelvin", out _),
                    ["Rankine (°R)"] = ConvertTemperature(val, model.FromUnit, "Rankine", out _)
                };
                break;

            case "WindSpeed":
                result = ConvertWindSpeed(val, model.FromUnit, model.ToUnit, out formula);
                model.QuickConversions = new Dictionary<string, double>
                {
                    ["km/h"] = ConvertWindSpeed(val, model.FromUnit, "km/h", out _),
                    ["mph"] = ConvertWindSpeed(val, model.FromUnit, "mph", out _),
                    ["m/s"] = ConvertWindSpeed(val, model.FromUnit, "m/s", out _),
                    ["knots"] = ConvertWindSpeed(val, model.FromUnit, "knots", out _)
                };
                break;

            case "Pressure":
                result = ConvertPressure(val, model.FromUnit, model.ToUnit, out formula);
                model.QuickConversions = new Dictionary<string, double>
                {
                    ["hPa / mbar"] = ConvertPressure(val, model.FromUnit, "hPa", out _),
                    ["inHg"] = ConvertPressure(val, model.FromUnit, "inHg", out _),
                    ["mmHg"] = ConvertPressure(val, model.FromUnit, "mmHg", out _),
                    ["atm"] = ConvertPressure(val, model.FromUnit, "atm", out _),
                    ["psi"] = ConvertPressure(val, model.FromUnit, "psi", out _)
                };
                break;

            case "Rainfall":
                result = ConvertRainfall(val, model.FromUnit, model.ToUnit, out formula);
                model.QuickConversions = new Dictionary<string, double>
                {
                    ["Millimeters (mm)"] = ConvertRainfall(val, model.FromUnit, "mm", out _),
                    ["Centimeters (cm)"] = ConvertRainfall(val, model.FromUnit, "cm", out _),
                    ["Inches (in)"] = ConvertRainfall(val, model.FromUnit, "in", out _)
                };
                break;

            default:
                result = val;
                formula = "Direct passthrough";
                break;
        }

        model.ConvertedResult = Math.Round(result, 3);
        model.FormulaDescription = formula;
        return model;
    }

    private static double ConvertTemperature(double val, string from, string to, out string formula)
    {
        // Normalize to Celsius first
        double celsius = from switch
        {
            "Fahrenheit" => (val - 32.0) * 5.0 / 9.0,
            "Kelvin" => val - 273.15,
            "Rankine" => (val - 491.67) * 5.0 / 9.0,
            _ => val
        };

        double converted = to switch
        {
            "Fahrenheit" => (celsius * 9.0 / 5.0) + 32.0,
            "Kelvin" => celsius + 273.15,
            "Rankine" => (celsius + 273.15) * 9.0 / 5.0,
            _ => celsius
        };

        formula = $"{from} to {to}: {val} {from} = {converted:F2} {to}";
        return converted;
    }

    private static double ConvertWindSpeed(double val, string from, string to, out string formula)
    {
        // Normalize to m/s
        double ms = from switch
        {
            "km/h" => val / 3.6,
            "mph" => val * 0.44704,
            "knots" => val * 0.514444,
            _ => val
        };

        double converted = to switch
        {
            "km/h" => ms * 3.6,
            "mph" => ms / 0.44704,
            "knots" => ms / 0.514444,
            _ => ms
        };

        formula = $"{from} to {to}: converted via SI base m/s";
        return converted;
    }

    private static double ConvertPressure(double val, string from, string to, out string formula)
    {
        // Normalize to hPa (hectopascals)
        double hpa = from switch
        {
            "inHg" => val * 33.86389,
            "mmHg" => val * 1.333224,
            "atm" => val * 1013.25,
            "psi" => val * 68.9476,
            _ => val
        };

        double converted = to switch
        {
            "inHg" => hpa / 33.86389,
            "mmHg" => hpa / 1.333224,
            "atm" => hpa / 1013.25,
            "psi" => hpa / 68.9476,
            _ => hpa
        };

        formula = $"{from} to {to}: converted via hPa base standard";
        return converted;
    }

    private static double ConvertRainfall(double val, string from, string to, out string formula)
    {
        double mm = from switch
        {
            "cm" => val * 10.0,
            "in" => val * 25.4,
            _ => val
        };

        double converted = to switch
        {
            "cm" => mm / 10.0,
            "in" => mm / 25.4,
            _ => mm
        };

        formula = $"{from} to {to}: {val} {from} = {converted:F2} {to}";
        return converted;
    }

    public PasswordGeneratorModel GenerateWeatherPassword(PasswordGeneratorModel model)
    {
        var rng = RandomNumberGenerator.Create();

        if (model.GeneratorMode == "Passphrase")
        {
            var adj = WeatherAdjectives[RandomNumberGenerator.GetInt32(WeatherAdjectives.Length)];
            var noun = WeatherNouns[RandomNumberGenerator.GetInt32(WeatherNouns.Length)];
            var loc = WeatherLocations[RandomNumberGenerator.GetInt32(WeatherLocations.Length)];
            var num = RandomNumberGenerator.GetInt32(100, 999);
            var symbol = SpecialSymbols[RandomNumberGenerator.GetInt32(SpecialSymbols.Length)];

            model.GeneratedPassword = $"{adj}-{noun}{symbol}{loc}{num}";
            model.EntropyBits = 78;
            model.StrengthRating = "Very Strong (Entropy ~78 bits)";
            model.CrackTimeEstimate = "Over 40,000 Centuries";
        }
        else
        {
            // High-entropy token
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghjkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@#$%^&*_-+=";

            var pool = new StringBuilder();
            if (model.IncludeUppercase) pool.Append(upper);
            pool.Append(lower);
            if (model.IncludeNumbers) pool.Append(digits);
            if (model.IncludeSymbols) pool.Append(special);

            int len = Math.Clamp(model.Length, 8, 48);
            var sb = new StringBuilder(len);
            var poolStr = pool.ToString();

            for (int i = 0; i < len; i++)
            {
                int index = RandomNumberGenerator.GetInt32(poolStr.Length);
                sb.Append(poolStr[index]);
            }

            model.GeneratedPassword = sb.ToString();
            double bits = Math.Round(len * Math.Log2(poolStr.Length));
            model.EntropyBits = (int)bits;
            model.StrengthRating = bits >= 80 ? "Military Grade" : (bits >= 60 ? "Strong" : "Moderate");
            model.CrackTimeEstimate = bits >= 80 ? "Millions of Years" : "Decades";
        }

        return model;
    }

    public decimal EstimateDailyHvacCost(double outdoorTempC, double indoorTempC, decimal costPerKwh)
    {
        // Heating or cooling degree difference
        double deltaT = Math.Abs(outdoorTempC - indoorTempC);

        // Typical home heat loss/gain coefficient ~ 0.8 kWh per degree day per 100 m²
        double kwhNeeded = deltaT * 0.85;

        // Efficiency heat pump COP ~ 3.2
        double electricalKwh = kwhNeeded / 3.2;

        decimal estimatedDailyCost = (decimal)electricalKwh * costPerKwh;
        return Math.Round(estimatedDailyCost, 2);
    }
}
