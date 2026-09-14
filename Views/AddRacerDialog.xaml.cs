using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using RacingTimeClock.Data;
using RacingTimeClock.Models;
using RacingTimeClock.Services;

namespace RacingTimeClock.Views;

public partial class AddRacerDialog : Window
{
    public AddRacerDialog()
    {
        InitializeComponent();

        GenderComboBox.SelectedIndex = 0;
    }

    private async void AddButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string racerId =
            RacerIdTextBox.Text.Trim();

        string name =
            NameTextBox.Text.Trim();

        string yobText =
            YobTextBox.Text.Trim();

        string racingNumber =
            RacingNumberTextBox.Text.Trim();

        // -------------------------
        // Racer ID
        // -------------------------

        if (!Regex.IsMatch(
                racerId,
                @"^\d{5}$"))
        {
            MessageBox.Show(
                "Racer ID must be exactly 5 numbers.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        // -------------------------
        // Name
        // -------------------------

        if (string.IsNullOrWhiteSpace(name) ||
            name.Length > 20 ||
            !Regex.IsMatch(
                name,
                @"^[A-Za-z ]+$"))
        {
            MessageBox.Show(
                "Name must contain letters only and be no more than 20 characters.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        // -------------------------
        // Gender
        // -------------------------

        if (GenderComboBox.SelectedItem
            is not ComboBoxItem genderItem)
        {
            MessageBox.Show(
                "Please select a gender.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        bool isMale =
            genderItem.Content?.ToString() == "Male";

        // -------------------------
        // Racing Number
        // -------------------------

        if (string.IsNullOrWhiteSpace(racingNumber) ||
            racingNumber.Length > 4 ||
            !Regex.IsMatch(
                racingNumber,
                @"^\d{1,4}$"))
        {
            MessageBox.Show(
                "Racing number must contain numbers only and be no more than 4 digits.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        // -------------------------
        // Current Season
        // -------------------------

        Season? season =
            AppSeasonService.Instance.CurrentSeason;

        if (season == null)
        {
            MessageBox.Show(
                "No season is selected. Please select a season in Settings.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        // -------------------------
        // Year of Birth
        // -------------------------

        if (!int.TryParse(
                yobText,
                out int yob))
        {
            MessageBox.Show(
                "Year of Birth must be a valid year.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        int latestAllowedBirthYear =
            season.StartYear - 4;

        if (yob > latestAllowedBirthYear)
        {
            MessageBox.Show(
                $"Year of Birth must be {latestAllowedBirthYear} or earlier for the {season.Name} season.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        // -------------------------
        // Database
        // -------------------------

        try
        {
            using RacingTimeClockDbContext db =
                new();

            bool idExists =
                db.Racers.Any(
                    r => r.RacerId == racerId);

            if (idExists)
            {
                MessageBox.Show(
                    "This Racer ID already exists. Racer IDs must be unique.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Racer racer = new Racer
            {
                RacerId = racerId,
                Name = name,
                YearOfBirth = yob,
                IsMale = isMale,
                RacingNumber = racingNumber,
                IsActive = true
            };

            db.Racers.Add(racer);

            await db.SaveChangesAsync();

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to save racer.\n\n{ex}",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

