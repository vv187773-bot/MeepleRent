using System.Linq;
using Avalonia.Controls;
using MeepleRent.Data;
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
        }
    }
}
