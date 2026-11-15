namespace MeepleRent.Data.Models;

/// <summary>Клиент клуба-проката настольных игр.</summary>
public class Client
{
    public int Id { get; set; }

    /// <summary>Фамилия и имя.</summary>
    public string FullName { get; set; } = "";

    /// <summary>Телефон для связи.</summary>
    public string Phone { get; set; } = "";

    /// <summary>Накопленные бонусные баллы.</summary>
    public int BonusPoints { get; set; }

    /// <summary>Любимый жанр игр, может быть не указан.</summary>
    public string? FavoriteGenre { get; set; }

    /// <summary>Аренды клиента.</summary>
    public List<Rental> Rentals { get; set; } = new();

    public override string ToString() =>
        $"{FullName} — {Phone}, бонусов: {BonusPoints}";
}
