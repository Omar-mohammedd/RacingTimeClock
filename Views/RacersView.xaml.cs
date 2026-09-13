using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RacingTimeClock.Data;
using RacingTimeClock.Models;

namespace RacingTimeClock.Views;

public partial class RacersView : UserControl
{
    private List<Racer> racers = new();
    private List<Season> seasons = new();
    private Season? selectedSeason;

    public RacersView()
    {
        InitializeComponent();

        Loaded += RacersView_Loaded;
    }

    private async void RacersView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            await LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load racers.\n\n{ex}",
                "Racers Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task LoadData()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        await db.Database.EnsureCreatedAsync();

        racers = await Task.Run(() =>
            db.Racers
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToList());

        seasons = await Task.Run(() =>
            db.Seasons
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.Name)
                .ToList());

        if (seasons.Count == 0)
        {
            Season defaultSeason = new Season
            {
                Name = "2026/27",
                SeniorMinimumBirthYear = 2008,
                IsActive = true
            };

            db.Seasons.Add(defaultSeason);

            await db.SaveChangesAsync();

            seasons.Add(defaultSeason);
        }

        SeasonComboBox.ItemsSource = seasons;
        SeasonComboBox.DisplayMemberPath = "Name";

        if (seasons.Count > 0)
        {
            SeasonComboBox.SelectedIndex = 0;
        }
    }

    private void SeasonComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        selectedSeason =
            SeasonComboBox.SelectedItem as Season;

        RefreshRacers();
    }

    private void RefreshRacers()
    {
        RacersPanel.Children.Clear();

        if (selectedSeason == null)
            return;

        foreach (Racer racer in racers)
        {
            bool isSenior =
                racer.YearOfBirth <=
                selectedSeason.SeniorMinimumBirthYear;

            AddRacerRow(racer, isSenior);
        }
    }

    private void AddRacerRow(
        Racer racer,
        bool isSenior)
    {
        Border row = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(15),
            Margin = new Thickness(0, 0, 0, 10)
        };

        Grid grid = new Grid();

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1.2, GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(2.5, GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

        TextBlock idText = new TextBlock
        {
            Text = racer.RacerId,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold
        };

        Grid.SetColumn(idText, 0);

        TextBlock nameText = new TextBlock
        {
            Text = racer.Name,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold
        };

        Grid.SetColumn(nameText, 1);

        TextBlock yearText = new TextBlock
        {
            Text = racer.YearOfBirth.ToString(),
            FontSize = 16
        };

        Grid.SetColumn(yearText, 2);

        TextBlock categoryText = new TextBlock
        {
            Text = isSenior ? "SENIOR" : "JUNIOR",
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Foreground = isSenior
                ? new SolidColorBrush(
                    Color.FromRgb(0, 100, 180))
                : new SolidColorBrush(
                    Color.FromRgb(120, 80, 0))
        };

        Grid.SetColumn(categoryText, 3);

        TextBlock statusText = new TextBlock
        {
            Text = "ACTIVE",
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(
                Color.FromRgb(0, 120, 70))
        };

        Grid.SetColumn(statusText, 4);

        grid.Children.Add(idText);
        grid.Children.Add(nameText);
        grid.Children.Add(yearText);
        grid.Children.Add(categoryText);
        grid.Children.Add(statusText);

        row.Child = grid;

        RacersPanel.Children.Add(row);
    }

    private void AddRacerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        AddRacerDialog dialog =
            new AddRacerDialog();

        dialog.Owner =
            Window.GetWindow(this);

        bool? result =
            dialog.ShowDialog();

        if (result != true)
            return;

        Racer racer = dialog.CreatedRacer;

        try
        {
            using RacingTimeClockDbContext db =
                new RacingTimeClockDbContext();

            db.Database.EnsureCreated();

            db.Racers.Add(racer);

            db.SaveChanges();

            racers.Add(racer);

            racers = racers
                .OrderBy(r => r.Name)
                .ToList();

            RefreshRacers();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not add racer.\n\n{ex}",
                "Add Racer Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}