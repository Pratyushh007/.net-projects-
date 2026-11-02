namespace WeatherApp.Models.Entities;

public class FavoriteCity
{
    public int Id { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? AdminArea { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public class SearchHistory
{
    public int Id { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double TemperatureC { get; set; }
    public string ConditionText { get; set; } = string.Empty;
    public double Humidity { get; set; }
    public double WindSpeedKmh { get; set; }
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
}

public class WeatherNote
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public double? TemperatureC { get; set; }
    public string? Condition { get; set; }
    public string Category { get; set; } = "General"; // Travel, Severe Weather, Photography, Agriculture, Daily
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class WeatherTodoItem
{
    public int Id { get; set; }
    public string TaskDescription { get; set; } = string.Empty;
    public string TargetCity { get; set; } = string.Empty;
    public string RequiredWeatherType { get; set; } = "Any"; // Sunny, Clear, Rain, Mild, Snow, Indoors
    public double? MinTemp { get; set; }
    public double? MaxTemp { get; set; }
    public bool RequiresNoRain { get; set; }
    public bool IsCompleted { get; set; }
    public string Priority { get; set; } = "Normal"; // Low, Normal, High
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EmergencyContact
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CityOrRegion { get; set; } = string.Empty;
    public string Category { get; set; } = "Emergency Services"; // Storm Shelter, Flood Hotline, Road Assistance, Coast Guard, Medical
    public int PriorityLevel { get; set; } = 1; // 1 = Critical, 2 = Warning, 3 = Advisory
    public string? Notes { get; set; }
}

public class QuizScore
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = "Weather Explorer";
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public string Difficulty { get; set; } = "Standard";
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}

public class EnergyAccount
{
    public int Id { get; set; }
    public string AccountName { get; set; } = "Home Energy Savings";
    public decimal CurrentBalance { get; set; } = 500.00m;
    public decimal MonthlyBudget { get; set; } = 250.00m;
    public double TargetIndoorTempC { get; set; } = 21.0;
    public decimal CostPerKwh { get; set; } = 0.16m;
    public List<EnergyTransaction> Transactions { get; set; } = new();
}

public class EnergyTransaction
{
    public int Id { get; set; }
    public int EnergyAccountId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsDeposit { get; set; } // true = deposit/rebate, false = expense
    public string WeatherImpactReason { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
