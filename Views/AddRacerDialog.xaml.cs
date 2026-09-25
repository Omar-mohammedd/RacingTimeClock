using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
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

        LoadBirthYears();
    }

    private void LoadBirthYears()
    {
        Season? season =
            AppSeasonService.Instance.CurrentSeason;

        if (season == null)
            return;

        int youngestYear =
            season.StartYear - 4;

        int oldestYear =
            season.StartYear - 40;

        YobComboBox.Items.Clear();

        for (int year = youngestYear;
             year >= oldestYear;
             year--)
        {
            YobComboBox.Items.Add(year);
        }

        if (YobComboBox.Items.Count > 0)
            YobComboBox.SelectedIndex = 0;
    }

    private async void AddButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string racerId =
            RacerIdTextBox.Text.Trim();

        string name =
            NameTextBox.Text.Trim();

        string racingNumber =
            RacingNumberTextBox.Text.Trim();

        if (!RacerValidationService.IsValidRacerId(racerId))
        {
            RacingTimeClock.Services.RacingPopupService.Show(
                "Racer ID must contain exactly 14 digits.",
                "Invalid Racer ID",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (!RacerValidationService.IsValidName(name))
        {
            RacingTimeClock.Services.RacingPopupService.Show(
                "Name must contain English or Arabic letters separated by spaces only.",
                "Invalid Name",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (GenderComboBox.SelectedItem
            is not ComboBoxItem genderItem)
        {
            RacingTimeClock.Services.RacingPopupService.Show(
                "Please select a gender.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        bool isMale =
            genderItem.Content?.ToString() == "Male";

        Season? season =
            AppSeasonService.Instance.CurrentSeason;

        if (season == null)
        {
            RacingTimeClock.Services.RacingPopupService.Show(
                "No season is selected. Please select a season in Settings.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (YobComboBox.SelectedItem is not int yob)
        {
            RacingTimeClock.Services.RacingPopupService.Show(
                "Please select a year of birth.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            using RacingTimeClockDbContext db =
                new();

            bool idExists =
                await db.Racers.AnyAsync(
                    r => r.RacerId == racerId);

            if (idExists)
            {
                RacingTimeClock.Services.RacingPopupService.Show(
                    "This Racer ID already exists. Racer IDs must be unique.",
                    "Duplicate Racer ID",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (RacerValidationService.IsNewRacingNumber(
                    racingNumber))
            {
                string[] usedNumbers =
                    await db.Racers
                        .Select(r => r.RacingNumber)
                        .ToArrayAsync();

                racingNumber =
                    RacerValidationService.GenerateUniqueRacingNumber(
                        usedNumbers);
            }
            else if (!RacerValidationService.IsValidNumericRacingNumber(
                         racingNumber))
            {
                RacingTimeClock.Services.RacingPopupService.Show(
                    "Racing number must contain 1 to 4 digits, or be \"جديد\".",
                    "Invalid Racing Number",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            bool racingNumberExists =
                await db.Racers.AnyAsync(
                    r => r.RacingNumber == racingNumber);

            if (racingNumberExists)
            {
                RacingTimeClock.Services.RacingPopupService.Show(
                    "That racing number is already in use.",
                    "Duplicate Racing Number",
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
            RacingTimeClock.Services.RacingPopupService.Show(
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


