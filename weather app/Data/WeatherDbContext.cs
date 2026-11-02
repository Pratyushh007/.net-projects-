using Microsoft.EntityFrameworkCore;
using WeatherApp.Models.Entities;

namespace WeatherApp.Data;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
    {
    }

    public DbSet<FavoriteCity> FavoriteCities => Set<FavoriteCity>();
    public DbSet<SearchHistory> SearchHistories => Set<SearchHistory>();
    public DbSet<WeatherNote> WeatherNotes => Set<WeatherNote>();
    public DbSet<WeatherTodoItem> WeatherTodoItems => Set<WeatherTodoItem>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<QuizScore> QuizScores => Set<QuizScore>();
    public DbSet<EnergyAccount> EnergyAccounts => Set<EnergyAccount>();
    public DbSet<EnergyTransaction> EnergyTransactions => Set<EnergyTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FavoriteCity>()
            .HasIndex(f => f.CityName);

        modelBuilder.Entity<EmergencyContact>()
            .HasIndex(c => c.CityOrRegion);

        modelBuilder.Entity<WeatherNote>()
            .HasIndex(n => n.CityName);
    }
}
