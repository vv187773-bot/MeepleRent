using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MeepleRent.Data;
using MeepleRent.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MeepleRent.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        // Создаём базу и заполняем её тестовыми данными, если она ещё пустая.
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            DbSeed.Seed(db);
        }

        RefreshLists();
    }

    /// <summary>
    /// Перечитывает все три таблицы и обновляет списки и выпадающие списки
    /// на всех вкладках. Вызывается после каждого изменения данных.
    /// </summary>
    private void RefreshLists()
    {
        using (var db = new AppDbContext())
        {
            ClientsList.ItemsSource = db.Clients
                .OrderBy(c => c.FullName)
                .ToList();

            GamesList.ItemsSource = db.Games
                .OrderBy(g => g.Title)
                .ToList();

            RentalsList.ItemsSource = db.Rentals
                .Include(r => r.Client)
                .Include(r => r.Game)
                .OrderBy(r => r.StartDate)
                .ToList();

            // В выпадающих списках те же записи, что на вкладках главных таблиц.
            RentalClientBox.ItemsSource = db.Clients
                .OrderBy(c => c.FullName)
                .ToList();

            RentalGameBox.ItemsSource = db.Games
                .OrderBy(g => g.Title)
                .ToList();
        }
    }

    // ---------- Клиенты ----------

    private void ClientsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Выбор сбрасывается после обновления списка — это нормально.
        if (ClientsList.SelectedItem is not Client client)
        {
            return;
        }

        ClientFullNameBox.Text = client.FullName;
        ClientPhoneBox.Text = client.Phone;
        ClientBonusBox.Text = client.BonusPoints.ToString();
        ClientGenreBox.Text = client.FavoriteGenre ?? "";
    }

    private void AddClient_Click(object? sender, RoutedEventArgs e)
    {
        var fullName = ClientFullNameBox.Text?.Trim() ?? "";
        var phone = ClientPhoneBox.Text?.Trim() ?? "";
        var genre = ClientGenreBox.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ClientsMessage.Text = "Ошибка: введите фамилию и имя клиента.";
            return;
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            ClientsMessage.Text = "Ошибка: введите телефон клиента.";
            return;
        }

        if (!int.TryParse(ClientBonusBox.Text?.Trim(), out var bonus))
        {
            ClientsMessage.Text = "Ошибка: бонусные баллы должны быть целым числом.";
            return;
        }

        if (bonus < 0)
        {
            ClientsMessage.Text = "Ошибка: бонусные баллы не могут быть отрицательными.";
            return;
        }

        var client = new Client
        {
            FullName = fullName,
            Phone = phone,
            BonusPoints = bonus,
            FavoriteGenre = string.IsNullOrWhiteSpace(genre) ? null : genre,
        };

        using (var db = new AppDbContext())
        {
            db.Clients.Add(client);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ClientsMessage.Text = "Не удалось добавить клиента: " + ex.Message;
                return;
            }
        }

        ClearClientFields();
        RefreshLists();
        ClientsMessage.Text = $"Клиент «{client.FullName}» добавлен.";
    }

    private void SaveClient_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientsList.SelectedItem is not Client selected)
        {
            ClientsMessage.Text = "Ошибка: выберите клиента в списке.";
            return;
        }

        var fullName = ClientFullNameBox.Text?.Trim() ?? "";
        var phone = ClientPhoneBox.Text?.Trim() ?? "";
        var genre = ClientGenreBox.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ClientsMessage.Text = "Ошибка: введите фамилию и имя клиента.";
            return;
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            ClientsMessage.Text = "Ошибка: введите телефон клиента.";
            return;
        }

        if (!int.TryParse(ClientBonusBox.Text?.Trim(), out var bonus) || bonus < 0)
        {
            ClientsMessage.Text = "Ошибка: бонусные баллы должны быть целым неотрицательным числом.";
            return;
        }

        using (var db = new AppDbContext())
        {
            // Запись для изменения берём из базы заново, по Id выбранной строки.
            var client = db.Clients.FirstOrDefault(c => c.Id == selected.Id);
            if (client is null)
            {
                ClientsMessage.Text = "Ошибка: запись уже удалена, обновите список.";
                RefreshLists();
                return;
            }

            client.FullName = fullName;
            client.Phone = phone;
            client.BonusPoints = bonus;
            client.FavoriteGenre = string.IsNullOrWhiteSpace(genre) ? null : genre;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ClientsMessage.Text = "Не удалось сохранить изменения: " + ex.Message;
                return;
            }

            RefreshLists();
            ClientsMessage.Text = $"Данные клиента «{client.FullName}» сохранены.";
        }
    }

    private void DeleteClient_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientsList.SelectedItem is not Client selected)
        {
            ClientsMessage.Text = "Ошибка: выберите клиента в списке.";
            return;
        }

        using (var db = new AppDbContext())
        {
            var client = db.Clients.FirstOrDefault(c => c.Id == selected.Id);
            if (client is null)
            {
                ClientsMessage.Text = "Ошибка: запись уже удалена, обновите список.";
                RefreshLists();
                return;
            }

            // Без клиента не может быть аренд: удаляем их вместе с ним.
            var rentals = db.Rentals.Where(r => r.ClientId == client.Id).ToList();

            try
            {
                db.Rentals.RemoveRange(rentals);
                db.Clients.Remove(client);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ClientsMessage.Text = "Не удалось удалить клиента: " + ex.Message;
                return;
            }

            ClearClientFields();
            RefreshLists();
            ClientsMessage.Text = rentals.Count > 0
                ? $"Клиент «{client.FullName}» удалён вместе с его арендами ({rentals.Count} шт.)."
                : $"Клиент «{client.FullName}» удалён.";
        }
    }

    private void ClearClientFields()
    {
        ClientFullNameBox.Text = "";
        ClientPhoneBox.Text = "";
        ClientBonusBox.Text = "";
        ClientGenreBox.Text = "";
    }

    // ---------- Игры ----------

    private void GamesList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesList.SelectedItem is not Game game)
        {
            return;
        }

        GameTitleBox.Text = game.Title;
        GameMinPlayersBox.Text = game.MinPlayers.ToString();
        GameMaxPlayersBox.Text = game.MaxPlayers.ToString();
        GamePlayTimeBox.Text = game.PlayTimeMinutes.ToString();
        GamePriceBox.Text = game.PricePerDay.ToString();
        GameGenreBox.Text = game.Genre;
    }

    private void AddGame_Click(object? sender, RoutedEventArgs e)
    {
        if (!TryReadGameFields(out var title, out var minPlayers, out var maxPlayers,
                out var playTime, out var price, out var genre, out var error))
        {
            GamesMessage.Text = error;
            return;
        }

        var game = new Game
        {
            Title = title,
            MinPlayers = minPlayers,
            MaxPlayers = maxPlayers,
            PlayTimeMinutes = playTime,
            PricePerDay = price,
            Genre = genre,
        };

        using (var db = new AppDbContext())
        {
            db.Games.Add(game);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GamesMessage.Text = "Не удалось добавить игру: " + ex.Message;
                return;
            }
        }

        ClearGameFields();
        RefreshLists();
        GamesMessage.Text = $"Игра «{game.Title}» добавлена в каталог.";
    }

    private void SaveGame_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is not Game selected)
        {
            GamesMessage.Text = "Ошибка: выберите игру в списке.";
            return;
        }

        if (!TryReadGameFields(out var title, out var minPlayers, out var maxPlayers,
                out var playTime, out var price, out var genre, out var error))
        {
            GamesMessage.Text = error;
            return;
        }

        using (var db = new AppDbContext())
        {
            var game = db.Games.FirstOrDefault(g => g.Id == selected.Id);
            if (game is null)
            {
                GamesMessage.Text = "Ошибка: запись уже удалена, обновите список.";
                RefreshLists();
                return;
            }

            game.Title = title;
            game.MinPlayers = minPlayers;
            game.MaxPlayers = maxPlayers;
            game.PlayTimeMinutes = playTime;
            game.PricePerDay = price;
            game.Genre = genre;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GamesMessage.Text = "Не удалось сохранить изменения: " + ex.Message;
                return;
            }

            RefreshLists();
            GamesMessage.Text = $"Данные игры «{game.Title}» сохранены.";
        }
    }

    private void DeleteGame_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is not Game selected)
        {
            GamesMessage.Text = "Ошибка: выберите игру в списке.";
            return;
        }

        using (var db = new AppDbContext())
        {
            var game = db.Games.FirstOrDefault(g => g.Id == selected.Id);
            if (game is null)
            {
                GamesMessage.Text = "Ошибка: запись уже удалена, обновите список.";
                RefreshLists();
                return;
            }

            // Аренды удалённой игры тоже становятся не нужны.
            var rentals = db.Rentals.Where(r => r.GameId == game.Id).ToList();

            try
            {
                db.Rentals.RemoveRange(rentals);
                db.Games.Remove(game);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                GamesMessage.Text = "Не удалось удалить игру: " + ex.Message;
                return;
            }

            ClearGameFields();
            RefreshLists();
            GamesMessage.Text = rentals.Count > 0
                ? $"Игра «{game.Title}» удалена вместе со связанными арендами ({rentals.Count} шт.)."
                : $"Игра «{game.Title}» удалена.";
        }
    }

    private bool TryReadGameFields(
        out string title, out int minPlayers, out int maxPlayers,
        out int playTime, out int price, out string genre, out string error)
    {
        title = GameTitleBox.Text?.Trim() ?? "";
        genre = GameGenreBox.Text?.Trim() ?? "";
        minPlayers = 0;
        maxPlayers = 0;
        playTime = 0;
        price = 0;
        error = "";

        if (string.IsNullOrWhiteSpace(title))
        {
            error = "Ошибка: введите название игры.";
            return false;
        }

        if (!int.TryParse(GameMinPlayersBox.Text?.Trim(), out minPlayers) || minPlayers < 1)
        {
            error = "Ошибка: минимальное число игроков должно быть целым числом от 1.";
            return false;
        }

        if (!int.TryParse(GameMaxPlayersBox.Text?.Trim(), out maxPlayers) || maxPlayers < minPlayers)
        {
            error = "Ошибка: максимальное число игроков должно быть целым числом не меньше минимального.";
            return false;
        }

        if (!int.TryParse(GamePlayTimeBox.Text?.Trim(), out playTime) || playTime < 1)
        {
            error = "Ошибка: длительность партии должна быть целым числом минут от 1.";
            return false;
        }

        if (!int.TryParse(GamePriceBox.Text?.Trim(), out price) || price < 0)
        {
            error = "Ошибка: цена за сутки должна быть целым неотрицательным числом.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(genre))
        {
            error = "Ошибка: введите жанр игры.";
            return false;
        }

        return true;
    }

    private void ClearGameFields()
    {
        GameTitleBox.Text = "";
        GameMinPlayersBox.Text = "";
        GameMaxPlayersBox.Text = "";
        GamePlayTimeBox.Text = "";
        GamePriceBox.Text = "";
        GameGenreBox.Text = "";
    }

    // ---------- Аренды ----------

    private void RentalsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (RentalsList.SelectedItem is not Rental rental)
        {
            return;
        }

        var clients = RentalClientBox.ItemsSource as IEnumerable<Client>;
        var games = RentalGameBox.ItemsSource as IEnumerable<Game>;

        RentalClientBox.SelectedItem = clients?.FirstOrDefault(c => c.Id == rental.ClientId);
        RentalGameBox.SelectedItem = games?.FirstOrDefault(g => g.Id == rental.GameId);
        RentalDatePicker.SelectedDate = rental.StartDate;
        RentalDaysBox.Text = rental.Days.ToString();
        RentalTotalBox.Text = rental.TotalPrice.ToString();
        RentalReturnedCheck.IsChecked = rental.IsReturned;
    }

    private void AddRental_Click(object? sender, RoutedEventArgs e)
    {
        if (!TryReadRentalFields(out var clientId, out var gameId, out var startDate,
                out var days, out var total, out var isReturned, out var error))
        {
            RentalsMessage.Text = error;
            return;
        }

        var rental = new Rental
        {
            // Записываем Id выбранных записей, а не объекты из другого контекста.
            ClientId = clientId,
            GameId = gameId,
            StartDate = startDate,
            Days = days,
            TotalPrice = total,
            IsReturned = isReturned,
        };

        using (var db = new AppDbContext())
        {
            db.Rentals.Add(rental);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                RentalsMessage.Text = "Не удалось добавить аренду: " + ex.Message;
                return;
            }
        }

        ClearRentalFields();
        RefreshLists();
        RentalsMessage.Text = "Аренда добавлена.";
    }

    private void SaveRental_Click(object? sender, RoutedEventArgs e)
    {
        if (RentalsList.SelectedItem is not Rental selected)
        {
            RentalsMessage.Text = "Ошибка: выберите аренду в списке.";
            return;
        }

        if (!TryReadRentalFields(out var clientId, out var gameId, out var startDate,
                out var days, out var total, out var isReturned, out var error))
        {
            RentalsMessage.Text = error;
            return;
        }

        using (var db = new AppDbContext())
        {
            var rental = db.Rentals.FirstOrDefault(r => r.Id == selected.Id);
            if (rental is null)
            {
                RentalsMessage.Text = "Ошибка: запись уже удалена, обновите список.";
                RefreshLists();
                return;
            }

            rental.ClientId = clientId;
            rental.GameId = gameId;
            rental.StartDate = startDate;
            rental.Days = days;
            rental.TotalPrice = total;
            rental.IsReturned = isReturned;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                RentalsMessage.Text = "Не удалось сохранить изменения: " + ex.Message;
                return;
            }

            RefreshLists();
            RentalsMessage.Text = "Изменения аренды сохранены.";
        }
    }

    private void DeleteRental_Click(object? sender, RoutedEventArgs e)
    {
        if (RentalsList.SelectedItem is not Rental selected)
        {
            RentalsMessage.Text = "Ошибка: выберите аренду в списке.";
            return;
        }

        using (var db = new AppDbContext())
        {
            var rental = db.Rentals.FirstOrDefault(r => r.Id == selected.Id);
            if (rental is null)
            {
                RentalsMessage.Text = "Ошибка: запись уже удалена, обновите список.";
                RefreshLists();
                return;
            }

            try
            {
                db.Rentals.Remove(rental);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                RentalsMessage.Text = "Не удалось удалить аренду: " + ex.Message;
                return;
            }

            ClearRentalFields();
            RefreshLists();
            RentalsMessage.Text = "Аренда удалена.";
        }
    }

    private bool TryReadRentalFields(
        out int clientId, out int gameId, out DateTime startDate,
        out int days, out int total, out bool isReturned, out string error)
    {
        clientId = 0;
        gameId = 0;
        startDate = default;
        days = 0;
        total = 0;
        isReturned = false;
        error = "";

        if (RentalClientBox.SelectedItem is not Client client)
        {
            error = "Ошибка: выберите клиента в выпадающем списке.";
            return false;
        }

        if (RentalGameBox.SelectedItem is not Game game)
        {
            error = "Ошибка: выберите игру в выпадающем списке.";
            return false;
        }

        if (RentalDatePicker.SelectedDate is not DateTime date)
        {
            error = "Ошибка: выберите дату выдачи.";
            return false;
        }

        if (!int.TryParse(RentalDaysBox.Text?.Trim(), out days) || days < 1)
        {
            error = "Ошибка: срок аренды должен быть целым числом дней от 1.";
            return false;
        }

        if (!int.TryParse(RentalTotalBox.Text?.Trim(), out total) || total < 0)
        {
            error = "Ошибка: сумма должна быть целым неотрицательным числом.";
            return false;
        }

        clientId = client.Id;
        gameId = game.Id;
        startDate = date;
        isReturned = RentalReturnedCheck.IsChecked == true;
        return true;
    }

    private void ClearRentalFields()
    {
        RentalClientBox.SelectedItem = null;
        RentalGameBox.SelectedItem = null;
        RentalDatePicker.SelectedDate = null;
        RentalDaysBox.Text = "";
        RentalTotalBox.Text = "";
        RentalReturnedCheck.IsChecked = false;
    }
}
