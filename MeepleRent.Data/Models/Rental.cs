namespace MeepleRent.Data.Models;

/// <summary>Аренда игры: кто, что, на какой срок и за какую сумму взял.</summary>
public class Rental
{
    public int Id { get; set; }

    /// <summary>Внешний ключ на клиента.</summary>
    public int ClientId { get; set; }

    /// <summary>Клиент, взявший игру.</summary>
    public Client? Client { get; set; }

    /// <summary>Внешний ключ на игру.</summary>
    public int GameId { get; set; }

    /// <summary>Игра, которую взяли.</summary>
    public Game? Game { get; set; }

    /// <summary>Дата выдачи игры.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Срок аренды в днях.</summary>
    public int Days { get; set; }

    /// <summary>Итоговая сумма аренды в рублях.</summary>
    public int TotalPrice { get; set; }

    /// <summary>Игра уже возвращена в клуб.</summary>
    public bool IsReturned { get; set; }

    public override string ToString() =>
        $"{Client?.FullName ?? "клиент не выбран"} — {Game?.Title ?? "игра не выбрана"}, " +
        $"с {StartDate:dd.MM.yyyy}, {Days} дн., {TotalPrice} ₽, " +
        $"{(IsReturned ? "возвращена" : "на руках")}";
}
