using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Data;

namespace RacingTimeClock.Views;

public partial class RaceDetailsDialog : Window
{
    private readonly int raceId;

    public class RaceResultDisplayItem
    {
        public int Position { get; set; }
        public string PositionDisplay => Position switch
        {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            _ => GetOrdinal(Position)
        };
        private static string GetOrdinal(int position)
        {
            int lastTwo = position % 100;

            if (lastTwo is >= 11 and <= 13)
                return $"{position}th";

            return (position % 10) switch
            {
                1 => $"{position}st",
                2 => $"{position}nd",
                3 => $"{position}rd",
                _ => $"{position}th"
            };
        }
        public string RacerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public string FormattedFinishTime => $"{(int)FinishTime.TotalMinutes:00}:{FinishTime.Seconds:00}.{FinishTime.Milliseconds:000}";
    }

    public RaceDetailsDialog(int selectedRaceId)
    {
        InitializeComponent();
        raceId = selectedRaceId;
        Loaded += RaceDetailsDialog_Loaded;
        SizeChanged += (_, _) => UpdateLeaderboardDataGridClip();
    }

    private void UpdateLeaderboardDataGridClip()
    {
        if (LeaderboardDataGrid.ActualWidth <= 0 ||
            LeaderboardDataGrid.ActualHeight <= 0)
        {
            return;
        }

        LeaderboardDataGrid.Clip =
            new System.Windows.Media.RectangleGeometry(
                new System.Windows.Rect(
                    0,
                    0,
                    LeaderboardDataGrid.ActualWidth,
                    LeaderboardDataGrid.ActualHeight),
                10,
                10);
    }
    private async void RaceDetailsDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadRaceDetailsAsync();
    }

    private async Task LoadRaceDetailsAsync()
    {
        try
        {
            using RacingTimeClockDbContext db = new();

            var race = await db.Races.FirstOrDefaultAsync(r => r.Id == raceId);

            if (race == null)
            {
                RacingTimeClock.Services.RacingPopupService.Show("Race record not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            var results = await db.RacerResults
                .Where(rr => rr.RaceId == raceId)
                .Include(rr => rr.Racer)
                .OrderBy(rr => rr.Position)
                .ToListAsync();

            RaceTitleText.Text = $"{race.RaceType} - {race.Distance}";
            RaceMetaText.Text = $"Date: {race.StartDateTime:yyyy-MM-dd HH:mm} | Total Racers: {results.Count}";

            var leaderboard = results
                .Select(rr => new RaceResultDisplayItem
                {
                    Position = rr.Position,
                    RacerId = rr.Racer?.RacerId?.ToString() ?? rr.RacerNumber.ToString(),
                    Name = rr.Racer?.Name ?? $"Racer #{rr.RacerNumber}",
                    FinishTime = rr.FinishTime
                })
                .ToList();

            LeaderboardDataGrid.ItemsSource = leaderboard;
        }
        catch (Exception ex)
        {
            RacingTimeClock.Services.RacingPopupService.Show($"Failed to load race details.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}




