using WeatherApp.Models.Entities;

namespace WeatherApp.Models.Tools;

// Project 2: Simple Calculator
public class AtmosphericCalculatorModel
{
    // Inputs
    public double TemperatureC { get; set; } = 25.0;
    public double RelativeHumidity { get; set; } = 60.0;
    public double WindSpeedKmh { get; set; } = 15.0;
    public double PressureHpa { get; set; } = 1013.25;
    public double RainfallMm { get; set; } = 10.0;
    public double RoofAreaSqMeters { get; set; } = 50.0;

    // Outputs
    public double HeatIndexC { get; set; }
    public double WindChillC { get; set; }
    public double DewPointC { get; set; }
    public double AirDensityKgM3 { get; set; }
    public double RainHarvestLiters { get; set; }
    public string HeatRiskLevel { get; set; } = string.Empty;
    public string FrostbiteRisk { get; set; } = string.Empty;
    public string CalculationSummary { get; set; } = string.Empty;
}

// Project 3: Unit Converter
public class UnitConverterModel
{
    public double InputValue { get; set; } = 20.0;
    public string Category { get; set; } = "Temperature"; // Temperature, WindSpeed, Pressure, Rainfall, Distance

    public string FromUnit { get; set; } = "Celsius";
    public string ToUnit { get; set; } = "Fahrenheit";
    public double ConvertedResult { get; set; }
    public string FormulaDescription { get; set; } = string.Empty;

    // Quick Matrix of common weather equivalents
    public Dictionary<string, double> QuickConversions { get; set; } = new();
}

// Project 8: Password Generator
public class PasswordGeneratorModel
{
    public int Length { get; set; } = 16;
    public bool IncludeWeatherTerms { get; set; } = true;
    public bool IncludeNumbers { get; set; } = true;
    public bool IncludeSymbols { get; set; } = true;
    public bool IncludeUppercase { get; set; } = true;
    public string GeneratorMode { get; set; } = "Passphrase"; // Passphrase vs CrypticToken

    public string GeneratedPassword { get; set; } = string.Empty;
    public int EntropyBits { get; set; }
    public string StrengthRating { get; set; } = "Strong";
    public string CrackTimeEstimate { get; set; } = "Centuries";
    public List<string> History { get; set; } = new();
}

// Project 5: Student Grade Manager (Adapted to City & Forecast Weather Report Cards)
public class CityGradeModel
{
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double TemperatureC { get; set; }
    public double Humidity { get; set; }
    public double WindSpeedKmh { get; set; }
    public double UvIndex { get; set; }
    public double PrecipitationMm { get; set; }

    public double ComfortScore { get; set; } // 0 - 100
    public string LetterGrade { get; set; } = "B"; // A+, A, B, C, D, F
    public string GPAEquivalent { get; set; } = "3.0";
    public string PrimaryComment { get; set; } = string.Empty;
    public List<string> StrengthPoints { get; set; } = new();
    public List<string> ImprovementPoints { get; set; } = new();
}

public class GradeReportCardViewModel
{
    public List<CityGradeModel> GradedCities { get; set; } = new();
    public double AverageScore { get; set; }
    public string ClassAverageGrade { get; set; } = "B+";
    public CityGradeModel? ValedictorianCity { get; set; }
    public CityGradeModel? NeedsImprovementCity { get; set; }
}

// Project 1: Number Guessing Game (Adapted to Guess The Weather!)
public class GuessGameViewModel
{
    public string TargetCity { get; set; } = "London";
    public string Country { get; set; } = "United Kingdom";
    public double ActualTempC { get; set; }
    public string ClueWeatherCondition { get; set; } = "Partly Cloudy";
    public string ClueWind { get; set; } = "15 km/h";
    public string ClueSeasonOrDate { get; set; } = "Autumn Afternoon";

    public int? UserGuess { get; set; }
    public int AttemptsUsed { get; set; } = 0;
    public int MaxAttempts { get; set; } = 5;
    public bool IsGameOver { get; set; } = false;
    public bool IsWinner { get; set; } = false;
    public string FeedbackMessage { get; set; } = "Guess the current temperature in this city!";
    public string HintDirection { get; set; } = ""; // Higher, Lower, Exact
    public List<GuessAttemptHistory> GuessHistory { get; set; } = new();
    public int CurrentStreak { get; set; } = 0;
    public int BestStreak { get; set; } = 0;
}

public class GuessAttemptHistory
{
    public int Guess { get; set; }
    public string Result { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

// Project 6: Quiz App (Meteorology & Extreme Weather Quiz)
public class QuizQuestion
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectOptionIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // Atmosphere, Storms, Climate, Instruments
}

public class QuizSessionViewModel
{
    public int CurrentQuestionIndex { get; set; } = 0;
    public int Score { get; set; } = 0;
    public int TotalQuestions { get; set; } = 5;
    public QuizQuestion? CurrentQuestion { get; set; }
    public bool IsFinished { get; set; }
    public string? SelectedAnswer { get; set; }
    public bool? WasCorrect { get; set; }
    public string? FeedbackText { get; set; }
    public List<QuizScore> Leaderboard { get; set; } = new();
}

// Project 7: Bank Account Simulator (Weather Energy Budget)
public class EnergyBankViewModel
{
    public EnergyAccount Account { get; set; } = new();
    public double OutdoorTempC { get; set; } = 8.5;
    public string CurrentCity { get; set; } = "London";
    public decimal EstimatedDailyCost { get; set; }
    public decimal ProjectedMonthlyBill { get; set; }
    public decimal MonthlyBudgetRemaining => Account.MonthlyBudget - Account.Transactions
        .Where(t => !t.IsDeposit && t.Date.Month == DateTime.UtcNow.Month)
        .Sum(t => t.Amount);
    public List<string> EnergySavingTips { get; set; } = new();
}
