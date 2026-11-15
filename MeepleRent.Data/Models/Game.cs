namespace MeepleRent.Data.Models;

/// <summary>Настольная игра из каталога клуба.</summary>
public class Game
{
    public int Id { get; set; }

    /// <summary>Название игры.</summary>
    public string Title { get; set; } = "";

    /// <summary>Минимальное число игроков.</summary>
    public int MinPlayers { get; set; }

    /// <summary>Максимальное число игроков.</summary>
    public int MaxPlayers { get; set; }

    /// <summary>Средняя длительность партии в минутах.</summary>
    public int PlayTimeMinutes { get; set; }

    /// <summary>Цена аренды за сутки в рублях.</summary>
    public int PricePerDay { get; set; }

    /// <summary>Жанр игры.</summary>
    public string Genre { get; set; } = "";

    /// <summary>Аренды этой игры.</summary>
    public List<Rental> Rentals { get; set; } = new();

    public override string ToString() =>
        $"{Title} ({MinPlayers}–{MaxPlayers} игроков, {PlayTimeMinutes} мин, {Genre}) — {PricePerDay} ₽/сутки";
}
