using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RacingTimeClock.Models;
using RacingTimeClock.Services;

namespace RacingTimeClock.Views;

public partial class RacesLogView : UserControl
{
    private readonly DatabaseService databaseService = new();

    private List<Race> allRaces = new();
    private List<Season> seasons = new();

    private bool isLoading = true;

    public RacesLogView()
    {
        InitializeComponent();

        Loaded += RacesLogView_Loaded;
    }

    private async void RacesLogView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            isLoading = true;

            seasons =
                await databaseService.GetSeasonsAsync();

            allRaces =
                await databaseService.GetRacesAsync();

            SeasonComboBox.ItemsSource = seasons;

            Season? currentSeason =
                AppSeasonService.Instance.CurrentSeason;

            if (currentSeason != null)
            {
                Season? matchingSeason =
                    seasons.FirstOrDefault(
                        s => s.Id == currentSeason.Id);

                if (matchingSeason != null)
                {
                    SeasonComboBox.SelectedItem =
                        matchingSeason;
                }
            }

            if (SeasonComboBox.SelectedItem == null &&
                seasons.Count > 0)
            {
                Season? newestSeason =
                    seasons
                        .OrderByDescending(
                            s => s.StartYear)
                        .FirstOrDefault();

                SeasonComboBox.SelectedItem =
                    newestSeason;
            }

            isLoading = false;

            RefreshRaceList();
        }
        catch (Exception ex)
        {
            isLoading = false;

            MessageBox.Show(
                $"Could not load races.\n\n{ex.Message}",
                "Races Log Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SeasonComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (isLoading)
            return;

        RefreshRaceList();
    }

    private void RefreshRaceList()
    {
        RacesPanel.Children.Clear();

        if (SeasonComboBox.SelectedItem
            is not Season selectedSeason)
        {
            ShowEmptyMessage(
                "No season selected.");

            return;
        }

        List<Race> races =
            allRaces
                .Where(r =>
                    r.SeasonId ==
                    selectedSeason.Id)
                .OrderByDescending(
                    r => r.StartDateTime)
                .ToList();

        if (races.Count == 0)
        {
            ShowEmptyMessage(
                $"No races saved for {selectedSeason.Name}.");

            return;
        }

        foreach (Race race in races)
        {
            AddRaceRow(race);
        }
    }

    private void ShowEmptyMessage(
        string message)
    {
        TextBlock emptyText = new TextBlock
        {
            Text = message,
            FontSize = 20,
            Foreground = Brushes.Gray,
            HorizontalAlignment =
                HorizontalAlignment.Center,
            Margin =
                new Thickness(0, 30, 0, 0)
        };

        RacesPanel.Children.Add(emptyText);
    }

    private void AddRaceRow(Race race)
    {
        Border row = new Border
        {
            Background = Brushes.White,
            CornerRadius =
                new CornerRadius(8),
            Padding =
                new Thickness(15),
            Margin =
                new Thickness(0, 0, 0, 10),
            Cursor = Cursors.Hand,
            Tag = race
        };

        row.MouseLeftButtonDown +=
            RaceRow_MouseLeftButtonDown;

        Grid grid = new Grid();

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(
                    1.5,
                    GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(
                    1.2,
                    GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(
                    1.5,
                    GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(
                    1.2,
                    GridUnitType.Star)
            });

        TextBlock dateText = new TextBlock
        {
            Text =
                race.StartDateTime.ToString(
                    "dd/MM/yyyy  HH:mm"),
            FontSize = 16,
            FontWeight =
                FontWeights.SemiBold,
            Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        32,
                        35,
                        42))
        };

        Grid.SetColumn(dateText, 0);

        TextBlock raceText = new TextBlock
        {
            Text = race.Distance,
            FontSize = 16,
            FontWeight =
                FontWeights.SemiBold,
            Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        32,
                        35,
                        42))
        };

        Grid.SetColumn(raceText, 1);

        RacerResult? winner =
            race.Results
                .FirstOrDefault(
                    r => r.Position == 1);

        string winnerName =
            winner?.Racer?.Name ?? "Guest";

        TextBlock winnerText = new TextBlock
        {
            Text = winnerName,
            FontSize = 16,
            FontWeight =
                FontWeights.SemiBold,
            Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        32,
                        35,
                        42))
        };

        Grid.SetColumn(winnerText, 2);

        string winnerTime =
            winner == null
                ? "--"
                : FormatTime(
                    winner.FinishTime);

        TextBlock timeText = new TextBlock
        {
            Text = winnerTime,
            FontSize = 16,
            FontWeight =
                FontWeights.Bold,
            Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        0,
                        100,
                        180))
        };

        Grid.SetColumn(timeText, 3);

        grid.Children.Add(dateText);
        grid.Children.Add(raceText);
        grid.Children.Add(winnerText);
        grid.Children.Add(timeText);

        row.Child = grid;

        RacesPanel.Children.Add(row);
    }

    private static string FormatTime(
        TimeSpan time)
    {
        return
            $"{(int)time.TotalMinutes:00}:" +
            $"{time.Seconds:00}." +
            $"{time.Milliseconds:000}";
    }

    private void RaceRow_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 &&
            sender is Border border &&
            border.Tag is Race race)
        {
            RaceDetailsDialog dialog =
                new RaceDetailsDialog(race.Id)
                {
                    Owner =
                        Window.GetWindow(this)
                };

            dialog.ShowDialog();
        }
    }
}
