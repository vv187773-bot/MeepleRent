using System.Text;
using MeepleRent.Data;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine($"Файл базы данных: {AppDbContext.DbPath}");
Console.WriteLine();

using (var db = new AppDbContext())
{
    db.Database.EnsureCreated();
    DbSeed.Seed(db);
}

using (var db = new AppDbContext())
{
    Console.WriteLine("=== Клиенты ===");
    foreach (var client in db.Clients.OrderBy(c => c.FullName).ToList())
    {
        Console.WriteLine(client);
    }

    Console.WriteLine();
    Console.WriteLine("=== Игры ===");
    foreach (var game in db.Games.OrderBy(g => g.Title).ToList())
    {
        Console.WriteLine(game);
    }

    Console.WriteLine();
    Console.WriteLine("=== Аренды ===");
    var rentals = db.Rentals
        .Include(r => r.Client)
        .Include(r => r.Game)
        .OrderBy(r => r.StartDate)
        .ToList();

    foreach (var rental in rentals)
    {
        Console.WriteLine(rental);
    }
}
