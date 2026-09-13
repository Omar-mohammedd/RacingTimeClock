using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RacingTimeClock.Models;
using RacingTimeClock.Services;

namespace RacingTimeClock.Views;

public partial class RacesLogView : UserControl
{
    private readonly DatabaseService databaseService = new();

    public RacesLogView()
    {
        InitializeComponent();

        Loaded += RacesLogView_Loaded;
    }

    private async void RacesLogView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadRaces();
    }

    private async Task LoadRaces()
    {
        try
        {
            List<Race> races =
                await databaseService.GetRacesAsync();

            RacesPanel.Children.Clear();

            if (races.Count == 0)
            {
                TextBlock emptyText = new TextBlock
                {
                    Text = "No saved races yet.",
                    FontSize = 20,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 30, 0, 0)
                };

                RacesPanel.Children.Add(emptyText);

                return;
            }

            foreach (Race race in races)
            {
                AddRaceRow(race);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load races.\n\n{ex.Message}",
                "Races Log Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddRaceRow(Race race)
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
                Width = new GridLength(2, GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1.5, GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1.5, GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1)
            });

        TextBlock dateText = new TextBlock
        {
            Text = race.StartDateTime.ToString(
                "dd/MM/yyyy  HH:mm"),
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(
                Color.FromRgb(32, 35, 42))
        };

        Grid.SetColumn(dateText, 0);

        TextBlock raceText = new TextBlock
        {
            Text = race.Distance,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(
                Color.FromRgb(32, 35, 42))
        };

        Grid.SetColumn(raceText, 1);

        TextBlock typeText = new TextBlock
        {
            Text = race.RaceType,
            FontSize = 16,
            Foreground = Brushes.Gray
        };

        Grid.SetColumn(typeText, 2);

        TextBlock racersText = new TextBlock
        {
            Text = race.RacerCount.ToString(),
            FontSize = 16,
            Foreground = Brushes.Gray
        };

        Grid.SetColumn(racersText, 3);

        grid.Children.Add(dateText);
        grid.Children.Add(raceText);
        grid.Children.Add(typeText);
        grid.Children.Add(racersText);

        row.Child = grid;

        RacesPanel.Children.Add(row);
    }
}
