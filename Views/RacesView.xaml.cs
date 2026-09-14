using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Data;
using RacingTimeClock.Models;

namespace RacingTimeClock.Views;

public partial class RacesView : UserControl
{
    public class RaceLogDisplayItem
    {
        public int RaceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public string FormattedDate => StartDateTime.ToString("yyyy-MM-dd HH:mm");
        public string RaceType { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string WinnerName { get; set; } = "N/A";
        public TimeSpan? WinnerTime { get; set; }
        public string FormattedWinnerTime => WinnerTime.HasValue
            ? $"{(int)WinnerTime.Value.TotalMinutes:00}:{WinnerTime.Value.Seconds:00}.{WinnerTime.Value.Milliseconds:000}"
            : "N/A";
        public int TotalRacers { get; set; }
    }

    public RacesView()
    {
        InitializeComponent();
        Loaded += RacesView_Loaded;
    }

    private async void RacesView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadRacesLogAsync();
    }

    private async Task LoadRacesLogAsync()
    {
        try
        {
            using RacingTimeClockDbContext db = new();

            var races = await db.Races
                .OrderByDescending(r => r.StartDateTime)
                .ToListAsync();

            var allResults = await db.RacerResults
                .Include(rr => rr.Racer)
                .ToListAsync();

            var logItems = new List<RaceLogDisplayItem>();

            foreach (var r in races)
            {
                var raceResults = allResults.Where(rr => rr.RaceId == r.Id).ToList();

                var winner = raceResults
                    .Where(rr => rr.Position == 1)
                    .FirstOrDefault();

                if (winner == null && raceResults.Any())
                {
                    winner = raceResults.OrderBy(rr => rr.Position).ThenBy(rr => rr.FinishTime).FirstOrDefault();
                }

                logItems.Add(new RaceLogDisplayItem
                {
                    RaceId = r.Id,
                    StartDateTime = r.StartDateTime,
                    RaceType = r.RaceType ?? "Standard",
                    Distance = r.Distance ?? "N/A",
                    WinnerName = winner?.Racer?.Name ?? (winner != null ? $"Racer #{winner.RacerNumber}" : "No Finishers"),
                    WinnerTime = winner?.FinishTime,
                    TotalRacers = raceResults.Count
                });
            }

            RacesDataGrid.ItemsSource = logItems;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not load races log.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RacesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (RacesDataGrid.SelectedItem is RaceLogDisplayItem selectedRace)
        {
            RaceDetailsDialog dialog = new RaceDetailsDialog(selectedRace.RaceId)
            {
                Owner = Window.GetWindow(this)
            };

            dialog.ShowDialog();
        }
    }
}
