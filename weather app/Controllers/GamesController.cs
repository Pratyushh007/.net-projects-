using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models.Entities;
using WeatherApp.Models.Tools;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

public class GamesController : Controller
{
    private readonly WeatherDbContext _dbContext;
    private readonly IWeatherService _weatherService;

    // Preset cities for the guessing game
    private static readonly (string City, string Country, double Lat, double Lon)[] GuessCities = new[]
    {
        ("Reykjavik", "Iceland", 64.1466, -21.9426),
        ("Dubai", "United Arab Emirates", 25.2048, 55.2708),
        ("Tokyo", "Japan", 35.6762, 139.6503),
        ("London", "United Kingdom", 51.5074, -0.1278),
        ("Cairo", "Egypt", 30.0444, 31.2357),
        ("Sydney", "Australia", -33.8688, 151.2093),
        ("Toronto", "Canada", 43.6532, -79.3832),
        ("Singapore", "Singapore", 1.3521, 103.8198),
        ("Oslo", "Norway", 59.9139, 10.7522),
        ("Buenos Aires", "Argentina", -34.6037, -58.3816)
    };

    private static readonly List<QuizQuestion> QuestionBank = new()
    {
        new QuizQuestion
        {
            Id = 1,
            QuestionText = "Which atmospheric layer contains almost all of Earth's weather phenomena and clouds?",
            Options = new() { "Stratosphere", "Troposphere", "Mesosphere", "Thermosphere" },
            CorrectOptionIndex = 1,
            Explanation = "The Troposphere extends from Earth's surface up to roughly 8-15 km and holds ~75% of the atmosphere's mass and almost all water vapor.",
            Category = "Atmosphere"
        },
        new QuizQuestion
        {
            Id = 2,
            QuestionText = "What does a rapidly falling barometric pressure reading usually indicate?",
            Options = new() { "Approaching sunny skies", "Incoming stormy or wet weather system", "Sudden freezing temperatures", "Calm wind conditions" },
            CorrectOptionIndex = 1,
            Explanation = "A falling barometer means atmospheric pressure is dropping, which draws in surrounding moist air and causes cloud formation and storms.",
            Category = "Instruments"
        },
        new QuizQuestion
        {
            Id = 3,
            QuestionText = "Which cloud type is known as the 'thunderhead' capable of producing violent lightning and hail?",
            Options = new() { "Cirrus", "Altocumulus", "Cumulonimbus", "Stratocumulus" },
            CorrectOptionIndex = 2,
            Explanation = "Cumulonimbus clouds are towering vertical clouds associated with thunderstorms, severe turbulence, and torrential downpours.",
            Category = "Clouds"
        },
        new QuizQuestion
        {
            Id = 4,
            QuestionText = "What is the Coriolis effect primarily caused by?",
            Options = new() { "Earth's rotation on its axis", "Ocean tidal forces", "Solar flare radiation", "Earth's elliptical orbit around the Sun" },
            CorrectOptionIndex = 0,
            Explanation = "Because the Earth rotates beneath moving air, winds are deflected to the right in the Northern Hemisphere and to the left in the Southern Hemisphere.",
            Category = "Climate"
        },
        new QuizQuestion
        {
            Id = 5,
            QuestionText = "What temperature scale sets the triple point of water at exactly 273.16 units?",
            Options = new() { "Celsius", "Fahrenheit", "Rankine", "Kelvin" },
            CorrectOptionIndex = 3,
            Explanation = "Kelvin is the SI fundamental unit of thermodynamic temperature, starting at absolute zero (0 K).",
            Category = "Atmosphere"
        },
        new QuizQuestion
        {
            Id = 6,
            QuestionText = "What meteorological phenomenon is characterized by periodic warming of sea surface temperatures across the central and east-central equatorial Pacific?",
            Options = new() { "La Niña", "El Niño", "Indian Ocean Dipole", "Arctic Oscillation" },
            CorrectOptionIndex = 1,
            Explanation = "El Niño warms the equatorial Pacific waters, causing widespread disruptions to global wind patterns and precipitation.",
            Category = "Climate"
        }
    };

    public GamesController(WeatherDbContext dbContext, IWeatherService weatherService)
    {
        _dbContext = dbContext;
        _weatherService = weatherService;
    }

    // ==========================================
    // Project 1: Number Guessing Game (Weather Temperature Guesser)
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> GuessTemperature(int? cityIndex, int? guess, int attempts = 0, int streak = 0)
    {
        // Pick a city index
        int idx = cityIndex ?? Random.Shared.Next(GuessCities.Length);
        if (idx < 0 || idx >= GuessCities.Length) idx = 0;

        var selected = GuessCities[idx];
        var weather = await _weatherService.GetWeatherForCityAsync(selected.City, selected.Lat, selected.Lon);
        int actualRounded = (int)Math.Round(weather.CurrentTempC);

        var vm = new GuessGameViewModel
        {
            TargetCity = selected.City,
            Country = selected.Country,
            ActualTempC = actualRounded,
            ClueWeatherCondition = weather.ConditionText,
            ClueWind = $"{weather.WindSpeedKmh:F0} km/h ({weather.WindDirection})",
            ClueSeasonOrDate = $"{weather.LocalTime:MMM yyyy} ({weather.Timezone})",
            UserGuess = guess,
            AttemptsUsed = attempts,
            CurrentStreak = streak,
            BestStreak = Math.Max(streak, HttpContext.Session?.GetInt32("BestGuessStreak") ?? streak)
        };

        if (guess.HasValue)
        {
            vm.AttemptsUsed++;
            int diff = guess.Value - actualRounded;

            if (diff == 0)
            {
                vm.IsGameOver = true;
                vm.IsWinner = true;
                vm.HintDirection = "Exact";
                vm.CurrentStreak++;
                vm.FeedbackMessage = $"🎯 BINGO! You nailed it! The temperature in {selected.City} is exactly {actualRounded}°C!";
                if (vm.CurrentStreak > vm.BestStreak)
                {
                    vm.BestStreak = vm.CurrentStreak;
                }
            }
            else if (vm.AttemptsUsed >= vm.MaxAttempts)
            {
                vm.IsGameOver = true;
                vm.IsWinner = false;
                vm.CurrentStreak = 0;
                vm.FeedbackMessage = $"Out of attempts! The actual temperature in {selected.City} was {actualRounded}°C.";
            }
            else if (diff < 0)
            {
                vm.HintDirection = "Higher";
                int gap = Math.Abs(diff);
                string heatClue = gap > 10 ? "WAY higher!" : (gap > 5 ? "Warmer!" : "Just a bit higher!");
                vm.FeedbackMessage = $"🌡️ Higher! {heatClue} Try guessing above {guess.Value}°C.";
            }
            else
            {
                vm.HintDirection = "Lower";
                int gap = Math.Abs(diff);
                string chillClue = gap > 10 ? "WAY lower!" : (gap > 5 ? "Colder!" : "Just a bit lower!");
                vm.FeedbackMessage = $"❄️ Lower! {chillClue} Try guessing below {guess.Value}°C.";
            }
        }

        ViewBag.CityIndex = idx;
        return View(vm);
    }

    // ==========================================
    // Project 6: Quiz App (Meteorology & Extreme Weather Quiz)
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> Quiz(int qIndex = 0, int currentScore = 0)
    {
        var leaderboard = await _dbContext.QuizScores
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.PlayedAt)
            .Take(5)
            .ToListAsync();

        if (qIndex >= QuestionBank.Count)
        {
            // Quiz completed
            var completedVm = new QuizSessionViewModel
            {
                CurrentQuestionIndex = qIndex,
                Score = currentScore,
                TotalQuestions = QuestionBank.Count,
                IsFinished = true,
                Leaderboard = leaderboard
            };
            return View(completedVm);
        }

        var vm = new QuizSessionViewModel
        {
            CurrentQuestionIndex = qIndex,
            Score = currentScore,
            TotalQuestions = QuestionBank.Count,
            CurrentQuestion = QuestionBank[qIndex],
            IsFinished = false,
            Leaderboard = leaderboard
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuizAnswer(int qIndex, int selectedOption, int currentScore)
    {
        if (qIndex < 0 || qIndex >= QuestionBank.Count)
            return RedirectToAction(nameof(Quiz));

        var q = QuestionBank[qIndex];
        bool isCorrect = selectedOption == q.CorrectOptionIndex;
        int newScore = isCorrect ? currentScore + 1 : currentScore;

        TempData["WasCorrect"] = isCorrect;
        TempData["FeedbackText"] = isCorrect
            ? $"✅ Correct! {q.Explanation}"
            : $"❌ Not quite. The correct answer was \"{q.Options[q.CorrectOptionIndex]}\". {q.Explanation}";

        return RedirectToAction(nameof(Quiz), new { qIndex = qIndex + 1, currentScore = newScore });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveQuizScore(string playerName, int finalScore, int totalQuestions)
    {
        var name = string.IsNullOrWhiteSpace(playerName) ? "Storm Chaser" : playerName.Trim();

        _dbContext.QuizScores.Add(new QuizScore
        {
            PlayerName = name,
            Score = finalScore,
            TotalQuestions = totalQuestions,
            Difficulty = "Standard",
            PlayedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Score saved to global leaderboard, {name}!";
        return RedirectToAction(nameof(Quiz), new { qIndex = 0, currentScore = 0 });
    }
}
