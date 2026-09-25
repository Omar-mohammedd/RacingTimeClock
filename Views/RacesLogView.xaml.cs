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
                    SeasonComboBox.SelectedItem =
                        matchingSeason;
            }

            if (SeasonComboBox.SelectedItem == null &&
                seasons.Count > 0)
            {
                Season? newestSeason =
                    seasons.OrderByDescending(
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

            RacingTimeClock.Services.RacingPopupService.Show(
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
        if (!isLoading)
            RefreshRaceList();
    }

    private void RefreshRaceList()
    {
        RacesPanel.Children.Clear();

        if (SeasonComboBox.SelectedItem
            is not Season selectedSeason)
        {
            ShowEmptyMessage("No season selected.");
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
            AddRaceRow(race);
    }

    private void ShowEmptyMessage(string message)
    {
        TextBlock emptyText = new TextBlock
        {
            Text = message,
            FontSize = 13,
            Foreground =
                (Brush)FindResource(
                    "TextSecondaryBrush"),
            HorizontalAlignment =
                HorizontalAlignment.Center,
            Margin =
                new Thickness(0, 30, 0, 0)
        };

        RacesPanel.Children.Add(emptyText);
    }

    private void RacesLogTableBorder_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (RacesLogTableBorder.ActualWidth <= 0 ||
            RacesLogTableBorder.ActualHeight <= 0)
        {
            return;
        }

        RacesLogTableBorder.Clip =
            new System.Windows.Media.RectangleGeometry(
                new System.Windows.Rect(
                    0,
                    0,
                    RacesLogTableBorder.ActualWidth,
                    RacesLogTableBorder.ActualHeight),
                10,
                10);
    }
    private void AddRaceRow(Race race)
    {
        RacerResult? winner =
            race.Results
                .FirstOrDefault(
                    r => r.Position == 1);

        string ageCategory = string.Empty;

        if (winner?.Racer != null &&
            race.Season != null)
        {
            ageCategory =
                SeasonService.GetCategory(
                    winner.Racer,
                    race.Season);
        }

        string winnerName =
            winner?.Racer?.Name ?? "Guest";

        string winnerTime =
            winner == null
                ? string.Empty
                : FormatTime(
                    winner.FinishTime);

        Border row = new Border
        {
            Background =
                (Brush)FindResource(
                    "SurfaceBrush"),

            BorderBrush =
                (Brush)FindResource(
                    "BorderBrush"),

            BorderThickness =
                new Thickness(0, 0, 0, 1),

            Height = 44,

            Cursor = Cursors.Hand,

            Tag = race
        };

        row.MouseLeftButtonDown +=
            RaceRow_MouseLeftButtonDown;

        Grid grid = new Grid();

        for (int i = 0; i < 6; i++)
        {
            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });
        }

        AddCell(
            grid,
            race.Distance,
            0);

        AddCell(
            grid,
            ageCategory,
            1,
            true);

        AddCell(
            grid,
            winnerName,
            2);

        AddCell(
            grid,
            winnerTime,
            3,
            true,
            true);

        AddCell(
            grid,
            race.Season?.Name ?? string.Empty,
            4,
            true);

        AddCell(
            grid,
            race.CompetitionType,
            5,
            true,
            false,
            true);

        row.Child = grid;

        RacesPanel.Children.Add(row);
    }

    private void AddCell(
        Grid grid,
        string text,
        int column,
        bool bold = false,
        bool accent = false,
        bool lastColumn = false)
    {
        Border cell = new Border
        {
            BorderBrush =
                (Brush)FindResource(
                    "BorderBrush"),

            BorderThickness = new Thickness(0)
        };

        TextBlock block = new TextBlock
        {
            Text = text,

            FontSize = 11,

            FontWeight =
                bold || accent
                    ? FontWeights.SemiBold
                    : FontWeights.Normal,

            Foreground =
                accent
                    ? (Brush)FindResource(
                        "AccentBrush")
                    : (Brush)FindResource(
                        "TextPrimaryBrush"),

            HorizontalAlignment =
                HorizontalAlignment.Center,

            VerticalAlignment =
                VerticalAlignment.Center
        };

        cell.Child = block;

        Grid.SetColumn(cell, column);

        grid.Children.Add(cell);
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









