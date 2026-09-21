using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Data;
using RacingTimeClock.Models;
using RacingTimeClock.Services;

namespace RacingTimeClock.Views;

public partial class RacerDetailsDialog : Window
{
    private readonly int racerId;

    private bool editing;

    private Racer? loadedRacer;

    public bool WasDeleted { get; private set; }

    public class RaceHistoryDisplayItem
    {
        public DateTime StartDateTime { get; set; }

        public string RaceType { get; set; } =
            string.Empty;

        public string Distance { get; set; } =
            string.Empty;

        public int Position { get; set; }

        public string PositionDisplay =>
            Position switch
            {
                1 => "1st 🥇",
                2 => "2nd 🥈",
                3 => "3rd 🥉",
                _ => $"{Position}th"
            };

        public TimeSpan FinishTime { get; set; }

        public string FormattedFinishTime =>
            $"{(int)FinishTime.TotalMinutes:00}:" +
            $"{FinishTime.Seconds:00}." +
            $"{FinishTime.Milliseconds:000}";
    }

    public RacerDetailsDialog(int racerDbId)
    {
        InitializeComponent();

        racerId = racerDbId;

        Loaded += RacerDetailsDialog_Loaded;
    }

    private async void RacerDetailsDialog_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadRacerDetailsAsync();
    }

    private async Task LoadRacerDetailsAsync()
    {
        try
        {
            using RacingTimeClockDbContext db =
                new RacingTimeClockDbContext();

            Racer? racer =
                await db.Racers
                    .FirstOrDefaultAsync(
                        r => r.Id == racerId);

            if (racer == null)
            {
                MessageBox.Show(
                    "The racer could not be found.",
                    "Racer Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();

                return;
            }

            loadedRacer = racer;

            RacerNameText.Text =
                racer.Name;

            RacerIdTextBox.Text =
                racer.RacerId;

            RacingNumberTextBox.Text =
                racer.RacingNumber;

            LoadBirthYears(racer.YearOfBirth);

            GenderComboBox.SelectedIndex =
                racer.IsMale ? 0 : 1;

            Season? season =
                AppSeasonService.Instance.CurrentSeason;

            if (season != null)
            {
                CategoryText.Text =
                    SeasonService.GetCategory(
                        racer,
                        season);
            }
            else
            {
                CategoryText.Text =
                    "Unknown";
            }

            var results =
                await db.RacerResults
                    .Where(
                        rr => rr.RacerId == racerId)
                    .Include(rr => rr.Race)
                    .OrderByDescending(
                        rr => rr.Race!.StartDateTime)
                    .Select(
                        rr => new RaceHistoryDisplayItem
                        {
                            StartDateTime =
                                rr.Race!.StartDateTime,

                            RaceType =
                                rr.Race.RaceType,

                            Distance =
                                rr.Race.Distance,

                            Position =
                                rr.Position,

                            FinishTime =
                                rr.FinishTime
                        })
                    .ToListAsync();

            HistoryDataGrid.ItemsSource =
                results;

            TotalRacesText.Text =
                results.Count.ToString();

            WinsText.Text =
                results.Count(
                    r => r.Position == 1)
                    .ToString();

            PodiumsText.Text =
                results.Count(
                    r => r.Position <= 3)
                    .ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load racer details.\n\n{ex.Message}",
                "Racer Details Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LoadBirthYears(int selectedYear)
    {
        Season? season =
            AppSeasonService.Instance.CurrentSeason;

        if (season == null)
            return;

        int youngestYear =
            season.StartYear - 4;

        int oldestYear =
            season.StartYear - 40;

        BirthYearComboBox.Items.Clear();

        for (int year = youngestYear;
             year >= oldestYear;
             year--)
        {
            BirthYearComboBox.Items.Add(year);
        }

        BirthYearComboBox.SelectedItem = selectedYear;
    }
    private void EditButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        editing = true;

        RacerIdTextBox.IsReadOnly = false;
        RacingNumberTextBox.IsReadOnly = false;
        BirthYearComboBox.IsEnabled = true;
        GenderComboBox.IsEnabled = true;

        EditButton.Visibility =
            Visibility.Collapsed;

        SaveButton.Visibility =
            Visibility.Visible;

        CancelEditButton.Visibility =
            Visibility.Visible;

        DeleteButton.IsEnabled = false;
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!int.TryParse(
                BirthYearComboBox.SelectedItem?.ToString() ?? string.Empty,
                out int birthYear))
        {
            MessageBox.Show(
                "Year of birth must be a valid number.",
                "Invalid Year",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (birthYear < 1900 ||
            birthYear > DateTime.Now.Year)
        {
            MessageBox.Show(
                "Please enter a valid year of birth.",
                "Invalid Year",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        string racerIdValue =
            RacerIdTextBox.Text.Trim();

        string racingNumber =
            RacingNumberTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(racerIdValue))
        {
            MessageBox.Show(
                "Racer ID cannot be empty.",
                "Invalid Racer ID",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (string.IsNullOrWhiteSpace(racingNumber))
        {
            MessageBox.Show(
                "Racing number cannot be empty.",
                "Invalid Racing Number",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (GenderComboBox.SelectedIndex < 0)
        {
            MessageBox.Show(
                "Please select a gender.",
                "Invalid Gender",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            using RacingTimeClockDbContext db =
                new RacingTimeClockDbContext();

            Racer? racer =
                await db.Racers
                    .FirstOrDefaultAsync(
                        r => r.Id == racerId);

            if (racer == null)
            {
                MessageBox.Show(
                    "The racer could not be found.",
                    "Racer Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            bool duplicateId =
                await db.Racers.AnyAsync(
                    r =>
                        r.Id != racerId &&
                        r.RacerId == racerIdValue);

            if (duplicateId)
            {
                MessageBox.Show(
                    "That Racer ID is already in use.",
                    "Duplicate Racer ID",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            racer.RacerId =
                racerIdValue;

            racer.RacingNumber =
                racingNumber;

            racer.YearOfBirth =
                birthYear;

            racer.IsMale =
                GenderComboBox.SelectedIndex == 0;

            await db.SaveChangesAsync();

            editing = false;

            MessageBox.Show(
                "Racer information updated successfully.",
                "Racer Updated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await LoadRacerDetailsAsync();

            SetEditMode(false);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not save racer changes.\n\n{ex.Message}",
                "Save Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CancelEditButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (loadedRacer != null)
        {
            RacerIdTextBox.Text =
                loadedRacer.RacerId;

            RacingNumberTextBox.Text =
                loadedRacer.RacingNumber;

            BirthYearComboBox.SelectedItem =
                loadedRacer.YearOfBirth;

            GenderComboBox.SelectedIndex =
                loadedRacer.IsMale ? 0 : 1;
        }

        editing = false;

        SetEditMode(false);
    }

    private void SetEditMode(bool value)
    {
        editing = value;

        RacerIdTextBox.IsReadOnly =
            !value;

        RacingNumberTextBox.IsReadOnly =
            !value;

        BirthYearComboBox.IsEnabled =
            value;

        GenderComboBox.IsEnabled =
            value;

        EditButton.Visibility =
            value
                ? Visibility.Collapsed
                : Visibility.Visible;

        SaveButton.Visibility =
            value
                ? Visibility.Visible
                : Visibility.Collapsed;

        CancelEditButton.Visibility =
            value
                ? Visibility.Visible
                : Visibility.Collapsed;

        DeleteButton.IsEnabled =
            !value;
    }

    private async void DeleteButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBoxResult result =
            MessageBox.Show(
                "Are you sure you want to deactivate this racer?\n\n" +
                "The racer will disappear from active racer lists, " +
                "but all historical race results will be preserved.",
                "Confirm Deactivation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using RacingTimeClockDbContext db =
                new RacingTimeClockDbContext();

            Racer? racer =
                await db.Racers.FindAsync(racerId);

            if (racer == null)
            {
                MessageBox.Show(
                    "The racer could not be found.",
                    "Racer Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            racer.IsActive = false;

            await db.SaveChangesAsync();

            WasDeleted = true;

            MessageBox.Show(
                "Racer deactivated successfully.",
                "Racer Deactivated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not deactivate the racer.\n\n{ex.Message}",
                "Deactivation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}




