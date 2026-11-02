using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models.Tools;

namespace WeatherApp.Services;

public class GradeManagerService : IGradeManagerService
{
    private readonly WeatherDbContext _dbContext;

    public GradeManagerService(WeatherDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GradeReportCardViewModel> GenerateReportCardAsync(CancellationToken cancellationToken = default)
    {
        // LINQ: Retrieve distinct cities from recent search history and favorites
        var historyList = await _dbContext.SearchHistories
            .OrderByDescending(s => s.SearchedAt)
            .Take(30)
            .ToListAsync(cancellationToken);

        // Group by city name using LINQ to get the latest weather metrics
        var latestPerCity = historyList
            .GroupBy(h => h.CityName.ToLower())
            .Select(g => g.First())
            .ToList();

        var gradedList = new List<CityGradeModel>();

        foreach (var item in latestPerCity)
        {
            var grade = GradeSingleCity(
                item.CityName,
                item.Country,
                item.TemperatureC,
                item.Humidity,
                item.WindSpeedKmh,
                uv: 5.0,
                rain: 0.0);

            gradedList.Add(grade);
        }

        // If not enough cities in history, provide standard global cities
        if (gradedList.Count < 3)
        {
            gradedList.Add(GradeSingleCity("Tokyo", "Japan", 21.0, 52.0, 12.0, 4.5, 0.0));
            gradedList.Add(GradeSingleCity("Paris", "France", 18.5, 60.0, 16.0, 3.8, 0.0));
            gradedList.Add(GradeSingleCity("San Diego", "United States", 23.2, 55.0, 10.0, 6.2, 0.0));
            gradedList.Add(GradeSingleCity("Reykjavik", "Iceland", 4.0, 80.0, 34.0, 1.2, 2.5));
            gradedList.Add(GradeSingleCity("Dubai", "United Arab Emirates", 36.5, 45.0, 18.0, 9.5, 0.0));
        }

        // LINQ: Ordering, Average, and Extremes
        var sorted = gradedList.OrderByDescending(c => c.ComfortScore).ToList();
        double avgScore = sorted.Average(c => c.ComfortScore);

        return new GradeReportCardViewModel
        {
            GradedCities = sorted,
            AverageScore = Math.Round(avgScore, 1),
            ClassAverageGrade = ScoreToLetter(avgScore),
            ValedictorianCity = sorted.FirstOrDefault(),
            NeedsImprovementCity = sorted.LastOrDefault()
        };
    }

    public CityGradeModel GradeSingleCity(string cityName, string country, double tempC, double humidity, double windSpeed, double uv, double rain)
    {
        double score = 100.0;
        var strengths = new List<string>();
        var improvements = new List<string>();

        // 1. Temperature Grading (Ideal: 20 - 24 °C)
        double tempDelta = Math.Abs(tempC - 22.0);
        if (tempDelta <= 2.5)
        {
            strengths.Add($"Optimal thermal equilibrium ({tempC:F1}°C)");
        }
        else
        {
            double penalty = tempDelta * 2.8;
            score -= penalty;
            improvements.Add(tempC > 24 ? $"High heat deviation (+{tempDelta:F1}°C above optimal)" : $"Cold chill deviation (-{tempDelta:F1}°C below optimal)");
        }

        // 2. Humidity Grading (Ideal: 40 - 55%)
        if (humidity >= 40 && humidity <= 55)
        {
            strengths.Add($"Crisp, comfortable relative humidity ({humidity:F0}%)");
        }
        else
        {
            double hDelta = humidity > 55 ? humidity - 55 : 40 - humidity;
            score -= hDelta * 0.5;
            improvements.Add(humidity > 55 ? "Muggy moisture level" : "Arid dry air");
        }

        // 3. Wind speed Grading (Ideal: < 18 km/h)
        if (windSpeed <= 18)
        {
            strengths.Add("Gentle atmospheric breezes");
        }
        else
        {
            score -= (windSpeed - 18) * 0.9;
            improvements.Add($"Brisk wind buffeting ({windSpeed:F0} km/h)");
        }

        // 4. Rain & UV
        if (rain > 0)
        {
            score -= 15 + (rain * 2);
            improvements.Add($"Active precipitation ({rain:F1} mm)");
        }
        else
        {
            strengths.Add("Dry outdoor conditions");
        }

        if (uv >= 8)
        {
            score -= (uv - 7) * 3;
            improvements.Add("High ultraviolet solar index");
        }

        score = Math.Clamp(Math.Round(score, 1), 25.0, 99.0);
        string letter = ScoreToLetter(score);
        string gpa = ScoreToGpa(score);

        return new CityGradeModel
        {
            CityName = cityName,
            Country = country,
            TemperatureC = tempC,
            Humidity = humidity,
            WindSpeedKmh = windSpeed,
            UvIndex = uv,
            PrecipitationMm = rain,
            ComfortScore = score,
            LetterGrade = letter,
            GPAEquivalent = gpa,
            PrimaryComment = GetGradeComment(letter),
            StrengthPoints = strengths,
            ImprovementPoints = improvements
        };
    }

    private static string ScoreToLetter(double score)
    {
        if (score >= 94) return "A+";
        if (score >= 88) return "A";
        if (score >= 82) return "B+";
        if (score >= 75) return "B";
        if (score >= 68) return "C+";
        if (score >= 60) return "C";
        if (score >= 50) return "D";
        return "F";
    }

    private static string ScoreToGpa(double score)
    {
        if (score >= 94) return "4.0";
        if (score >= 88) return "3.8";
        if (score >= 82) return "3.4";
        if (score >= 75) return "3.0";
        if (score >= 68) return "2.5";
        if (score >= 60) return "2.0";
        if (score >= 50) return "1.0";
        return "0.0";
    }

    private static string GetGradeComment(string letter)
    {
        return letter switch
        {
            "A+" => "Exceptional atmospheric comfort! Ideal weather for all activities.",
            "A" => "Superb weather conditions across all comfort metrics.",
            "B+" => "Very good climate with mild seasonal variations.",
            "B" => "Respectable conditions; minor apparel adjustments recommended.",
            "C+" => "Average weather score; pack an umbrella or jacket.",
            "C" => "Noticeable discomfort due to humidity or chill.",
            "D" => "Challenging atmospheric conditions, caution advised outdoors.",
            _ => "Inclement weather alert; outdoor activities not recommended."
        };
    }
}
