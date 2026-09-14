using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Data;
using RacingTimeClock.Models;

namespace RacingTimeClock.Views;

public partial class RacerDetailsDialog : Window
{
    private readonly int racerId;
    public bool WasDeleted { get; private set; }

    public class RaceHistoryDisplayItem
    {
        public DateTime StartDateTime { get; set; }
        public string RaceType { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public int Position { get; set; }
        public string PositionDisplay => Position switch
        {
            1 => "1st 🥇",
            2 => "2nd 🥈",
            3 => "3rd 🥉",
            _ => $"{Position}th"
        };
        public TimeSpan FinishTime { get; set; }
        public string FormattedFinishTime => $"{(int)FinishTime.TotalMinutes:00}:{FinishTime.Seconds:00}.{FinishTime.Milliseconds:000}";
    }

    public RacerDetailsDialog(int racerDbId)
    {
        InitializeComponent();
        racerId = racerDbId;
        Loaded += RacerDetailsDialog_Loaded;
    }

    private async void RacerDetailsDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadRacerDetailsAsync();
    }

    private async Task LoadRacerDetailsAsync()
    {
        try
        {
            using RacingTimeClockDbContext db = new();

            var racer = await db.Racers
                .FirstOrDefaultAsync(r => r.Id == racerId);

            if (racer == null)
            {
                MessageBox.Show("Racer not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            RacerNameText.Text = racer.Name;
            RacerInfoText.Text = $"Racer ID: {racer.RacerId}";

            var results = await db.RacerResults
                .Where(rr => rr.RacerId == racerId)
                .Include(rr => rr.Race)
                .OrderByDescending(rr => rr.Race!.StartDateTime)
                .Select(rr => new RaceHistoryDisplayItem
                {
                    StartDateTime = rr.Race!.StartDateTime,
                    RaceType = rr.Race.RaceType,
                    Distance = rr.Race.Distance,
                    Position = rr.Position,
                    FinishTime = rr.FinishTime
                })
                .ToListAsync();

            HistoryDataGrid.ItemsSource = results;

            int totalRaces = results.Count;
            int wins = results.Count(r => r.Position == 1);
            int podiums = results.Count(r => r.Position <= 3);

            TotalRacesText.Text = totalRaces.ToString();
            WinsText.Text = wins.ToString();
            PodiumsText.Text = podiums.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load racer details.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to delete this racer? Historical race logs will keep performance statistics, but the racer record will be removed.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using RacingTimeClockDbContext db = new();
            var racer = await db.Racers.FindAsync(racerId);

            if (racer != null)
            {
                db.Racers.Remove(racer);
                await db.SaveChangesAsync();
                WasDeleted = true;
                MessageBox.Show("Racer deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not delete racer.\n\n{ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
