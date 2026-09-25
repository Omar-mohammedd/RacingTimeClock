using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

    private ConfirmationAction pendingConfirmation =
        ConfirmationAction.None;

    public bool WasDeleted { get; private set; }

    private enum ConfirmationAction
    {
        None,
        Deactivate,
        Activate,
        PermanentDelete
    }

    public class RaceHistoryDisplayItem
    {
        public DateTime StartDateTime { get; set; }

        public int RaceId { get; set; }

        public string RaceType { get; set; } =
            string.Empty;

        public string Distance { get; set; } =
            string.Empty;

        public int Position { get; set; }

        public string PositionDisplay =>
            Position switch
            {
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                _ => GetOrdinal(Position)
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
        SizeChanged += (_, _) => UpdateHistoryDataGridClip();
    }

    private void UpdateHistoryDataGridClip()
    {
        if (HistoryDataGrid.ActualWidth <= 0 ||
            HistoryDataGrid.ActualHeight <= 0)
        {
            return;
        }

        HistoryDataGrid.Clip =
            new System.Windows.Media.RectangleGeometry(
                new System.Windows.Rect(
                    0,
                    0,
                    HistoryDataGrid.ActualWidth,
                    HistoryDataGrid.ActualHeight),
                10,
                10);
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

            RacerIdText.Text =
                racer.RacerId;

            RacerIdTextBox.Text =
                racer.RacerId;

            RacingNumberText.Text =
                racer.RacingNumber;

            RacingNumberTextBox.Text =
                racer.RacingNumber;

            BirthYearText.Text =
                racer.YearOfBirth.ToString();

            LoadBirthYears(racer.YearOfBirth);

            GenderText.Text =
                racer.IsMale ? "Male" : "Female";

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

                            RaceId =
                                rr.Race.Id,

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

            UpdateStatusControls();

            SetEditMode(false);
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

    private void HistoryDataGrid_PreviewMouseDown(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not System.Windows.DependencyObject source)
            return;

        System.Windows.Controls.DataGridRow? row =
            FindVisualParent<System.Windows.Controls.DataGridRow>(source);

        if (row == null)
            HistoryDataGrid.SelectedItem = null;
    }

    private void HistoryDataGrid_MouseDoubleClick(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        if (HistoryDataGrid.SelectedItem is not RaceHistoryDisplayItem item)
            return;

        if (e.OriginalSource is not System.Windows.DependencyObject source)
            return;

        System.Windows.Controls.DataGridRow? row =
            FindVisualParent<System.Windows.Controls.DataGridRow>(source);

        if (row == null)
            return;

        RaceDetailsDialog dialog =
            new RaceDetailsDialog(item.RaceId)
            {
                Owner = this
            };

        dialog.ShowDialog();

        HistoryDataGrid.SelectedItem = null;
        e.Handled = true;
    }

    private static T? FindVisualParent<T>(
        System.Windows.DependencyObject? child)
        where T : System.Windows.DependencyObject
    {
        System.Windows.DependencyObject? current = child;

        while (current != null)
        {
            if (current is T result)
                return result;

            current =
                System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private void Window_PreviewMouseDown(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not System.Windows.DependencyObject source)
            return;

        System.Windows.Controls.DataGridRow? row =
            FindVisualParent<System.Windows.Controls.DataGridRow>(
                source);

        if (row == null)
        {
            HistoryDataGrid.SelectedItem = null;
            HistoryDataGrid.UnselectAll();
            HistoryDataGrid.UnselectAllCells();
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

        BirthYearComboBox.SelectedItem =
            selectedYear;
    }

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

    private void EditButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SetEditMode(true);
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!int.TryParse(
                BirthYearComboBox.SelectedItem?.ToString() ??
                string.Empty,
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

            await LoadRacerDetailsAsync();

            MessageBox.Show(
                "Racer information updated successfully.",
                "Racer Updated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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

        SetEditMode(false);
    }

    private void SetEditMode(bool value)
    {
        editing = value;

        RacerIdText.Visibility =
            value
                ? Visibility.Collapsed
                : Visibility.Visible;

        RacerIdTextBox.Visibility =
            value
                ? Visibility.Visible
                : Visibility.Collapsed;

        RacingNumberText.Visibility =
            value
                ? Visibility.Collapsed
                : Visibility.Visible;

        RacingNumberTextBox.Visibility =
            value
                ? Visibility.Visible
                : Visibility.Collapsed;

        BirthYearText.Visibility =
            value
                ? Visibility.Collapsed
                : Visibility.Visible;

        BirthYearComboBox.Visibility =
            value
                ? Visibility.Visible
                : Visibility.Collapsed;

        GenderText.Visibility =
            value
                ? Visibility.Collapsed
                : Visibility.Visible;

        GenderComboBox.Visibility =
            value
                ? Visibility.Visible
                : Visibility.Collapsed;

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

        CloseButton.Visibility =
            value
                ? Visibility.Collapsed
                : Visibility.Visible;

        UpdateStatusControls();
    }
    private void UpdateStatusControls()
    {
        if (!editing || loadedRacer == null)
        {
            StatusButton.Visibility =
                Visibility.Collapsed;

            DeletePermanentButton.Visibility =
                Visibility.Collapsed;

            return;
        }

        StatusButton.Visibility =
            Visibility.Visible;

        StatusButton.Background =
            (System.Windows.Media.Brush)
                FindResource("DangerActionBackgroundBrush");

        StatusButton.Foreground =
            (System.Windows.Media.Brush)
                FindResource("DangerActionForegroundBrush");

        StatusButton.BorderBrush =
            (System.Windows.Media.Brush)
                FindResource("DangerBrush");

        if (loadedRacer.IsActive)
        {
            StatusButton.Content =
                "DEACTIVATE";

            DeletePermanentButton.Visibility =
                Visibility.Collapsed;
        }
        else
        {
            StatusButton.Content =
                "ACTIVATE";

            DeletePermanentButton.Visibility =
                Visibility.Visible;
        }
    }
    private void StatusButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (loadedRacer == null)
            return;

        if (loadedRacer.IsActive)
        {
            ShowConfirmation(
                ConfirmationAction.Deactivate,
                "DEACTIVATE RACER",
                "Are you sure you want to deactivate this racer?\n\n" +
                "The racer will no longer appear in active racer lists, " +
                "but all historical race results will be preserved.");
        }
        else
        {
            ShowConfirmation(
                ConfirmationAction.Activate,
                "ACTIVATE RACER",
                "Activate this racer again?\n\n" +
                "The racer will become available in active racer lists.");
        }
    }

    private void DeletePermanentButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowConfirmation(
            ConfirmationAction.PermanentDelete,
            "DELETE RACER PERMANENTLY",
            "This action cannot be undone.\n\n" +
            "The racer will be permanently removed from the database. " +
            "Historical race results will be preserved without the racer record.");
    }

    private void ShowConfirmation(
        ConfirmationAction action,
        string title,
        string message)
    {
        pendingConfirmation =
            action;

        ConfirmationTitleText.Text =
            title;

        ConfirmationMessageText.Text =
            message;

        ConfirmationConfirmButton.Content =
            action == ConfirmationAction.PermanentDelete
                ? "DELETE"
                : "CONFIRM";

        ConfirmationOverlay.Visibility =
            Visibility.Visible;
    }

    private void ConfirmationCancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        pendingConfirmation =
            ConfirmationAction.None;

        ConfirmationOverlay.Visibility =
            Visibility.Collapsed;
    }

    private async void ConfirmationConfirmButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ConfirmationAction action =
            pendingConfirmation;

        pendingConfirmation =
            ConfirmationAction.None;

        ConfirmationOverlay.Visibility =
            Visibility.Collapsed;

        switch (action)
        {
            case ConfirmationAction.Deactivate:
                await SetRacerActiveStateAsync(false);
                break;

            case ConfirmationAction.Activate:
                await SetRacerActiveStateAsync(true);
                break;

            case ConfirmationAction.PermanentDelete:
                await PermanentlyDeleteRacerAsync();
                break;
        }
    }

    private async Task SetRacerActiveStateAsync(
        bool active)
    {
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

            racer.IsActive =
                active;

            await db.SaveChangesAsync();

            WasDeleted = true;

            await LoadRacerDetailsAsync();

            SetEditMode(true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not update racer status.\n\n{ex.Message}",
                "Status Update Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task PermanentlyDeleteRacerAsync()
    {
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

            List<RacerResult> historicalResults =
                await db.RacerResults
                    .Where(
                        rr => rr.RacerId == racerId)
                    .ToListAsync();

            foreach (RacerResult result in historicalResults)
            {
                result.RacerId =
                    null;
            }

            db.Racers.Remove(racer);

            await db.SaveChangesAsync();

            WasDeleted = true;

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not permanently delete the racer.\n\n{ex.Message}",
                "Delete Error",
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





