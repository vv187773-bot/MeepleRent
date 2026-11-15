using MeepleRent.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MeepleRent.Data;

/// <summary>Контекст базы данных клуба-проката настольных игр.</summary>
public class AppDbContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Rental> Rentals => Set<Rental>();

    /// <summary>Путь к файлу базы. Одинаково работает на Windows и на Android.</summary>
    public static string DbPath
    {
        get
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "meeplerent.db");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
