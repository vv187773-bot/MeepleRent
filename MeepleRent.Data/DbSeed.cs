using MeepleRent.Data.Models;

namespace MeepleRent.Data;

/// <summary>Тестовые данные для клуба-проката настольных игр.</summary>
public static class DbSeed
{
    /// <summary>
    /// Добавляет тестовые записи, только если база пустая:
    /// повторный запуск программы не удваивает данные.
    /// </summary>
    public static void Seed(AppDbContext db)
    {
        if (db.Clients.Any() || db.Games.Any() || db.Rentals.Any())
        {
            return;
        }

        var clients = new List<Client>
        {
            new() { FullName = "Анна Смирнова", Phone = "+7 912 345-67-89", BonusPoints = 120, FavoriteGenre = "кооперативная" },
            new() { FullName = "Дмитрий Ковалёв", Phone = "+7 922 111-22-33", BonusPoints = 45, FavoriteGenre = "стратегия" },
            new() { FullName = "Мария Орлова", Phone = "+7 950 777-88-99", BonusPoints = 310, FavoriteGenre = "вечериночная" },
            new() { FullName = "Илья Фомин", Phone = "+7 904 555-66-77", BonusPoints = 0, FavoriteGenre = null },
            new() { FullName = "Софья Лебедева", Phone = "+7 999 123-45-67", BonusPoints = 80, FavoriteGenre = "детектив" },
        };

        var games = new List<Game>
        {
            new() { Title = "Колонизаторы", MinPlayers = 3, MaxPlayers = 4, PlayTimeMinutes = 90, PricePerDay = 350, Genre = "стратегия" },
            new() { Title = "Каркассон", MinPlayers = 2, MaxPlayers = 5, PlayTimeMinutes = 45, PricePerDay = 250, Genre = "стратегия" },
            new() { Title = "Мрачная гавань", MinPlayers = 1, MaxPlayers = 4, PlayTimeMinutes = 120, PricePerDay = 450, Genre = "кооперативная" },
            new() { Title = "Диксит", MinPlayers = 3, MaxPlayers = 8, PlayTimeMinutes = 30, PricePerDay = 200, Genre = "вечериночная" },
            new() { Title = "Кодовые имена", MinPlayers = 4, MaxPlayers = 8, PlayTimeMinutes = 25, PricePerDay = 180, Genre = "вечериночная" },
            new() { Title = "Детектив: сцена преступления", MinPlayers = 1, MaxPlayers = 5, PlayTimeMinutes = 150, PricePerDay = 500, Genre = "детектив" },
        };

        db.Clients.AddRange(clients);
        db.Games.AddRange(games);
        db.SaveChanges();

        var rentals = new List<Rental>
        {
            new() { ClientId = clients[0].Id, GameId = games[0].Id, StartDate = new DateTime(2026, 10, 12), Days = 5, TotalPrice = 1750, IsReturned = true },
            new() { ClientId = clients[1].Id, GameId = games[2].Id, StartDate = new DateTime(2026, 10, 25), Days = 7, TotalPrice = 3150, IsReturned = true },
            new() { ClientId = clients[2].Id, GameId = games[3].Id, StartDate = new DateTime(2026, 11, 2), Days = 3, TotalPrice = 600, IsReturned = false },
            new() { ClientId = clients[3].Id, GameId = games[5].Id, StartDate = new DateTime(2026, 11, 8), Days = 4, TotalPrice = 2000, IsReturned = false },
            new() { ClientId = clients[0].Id, GameId = games[4].Id, StartDate = new DateTime(2026, 11, 10), Days = 2, TotalPrice = 360, IsReturned = false },
            new() { ClientId = clients[4].Id, GameId = games[1].Id, StartDate = new DateTime(2026, 11, 12), Days = 6, TotalPrice = 1500, IsReturned = false },
        };

        db.Rentals.AddRange(rentals);
        db.SaveChanges();
    }
}
