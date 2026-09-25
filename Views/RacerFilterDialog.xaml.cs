﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace RacingTimeClock.Views;

public partial class RacerFilterDialog : Window
{
    public RacerFilterCriteria Criteria { get; private set; }

    public RacerFilterDialog(
        RacerFilterCriteria initialCriteria)
    {
        InitializeComponent();

        Criteria =
            initialCriteria.Clone();

        LoadCriteria();
    }

    private void LoadCriteria()
    {
        MinBirthYearTextBox.Text =
            Criteria.MinBirthYear?.ToString()
            ?? string.Empty;

        MaxBirthYearTextBox.Text =
            Criteria.MaxBirthYear?.ToString()
            ?? string.Empty;

        MinRacingNumberTextBox.Text =
            Criteria.MinRacingNumber?.ToString()
            ?? string.Empty;

        MaxRacingNumberTextBox.Text =
            Criteria.MaxRacingNumber?.ToString()
            ?? string.Empty;

        GenderComboBox.SelectedIndex =
            Criteria.Gender switch
            {
                "Male" => 1,
                "Female" => 2,
                _ => 0
            };

        StatusComboBox.SelectedIndex =
            Criteria.Status switch
            {
                "Active" => 1,
                "Not Active" => 2,
                _ => 0
            };

        CategoryComboBox.SelectedIndex =
            Criteria.Category switch
            {
                "Senior" => 1,
                "Junior" => 2,
                "Youth" => 3,
                _ => 0
            };
    }

    private void ApplyButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryParseOptionalInt(
                MinBirthYearTextBox.Text,
                "minimum year of birth",
                out int? minAge))
        {
            return;
        }

        if (!TryParseOptionalInt(
                MaxBirthYearTextBox.Text,
                "maximum year of birth",
                out int? maxAge))
        {
            return;
        }

        if (!TryParseOptionalInt(
                MinRacingNumberTextBox.Text,
                "minimum racing number",
                out int? minRacingNumber))
        {
            return;
        }

        if (!TryParseOptionalInt(
                MaxRacingNumberTextBox.Text,
                "maximum racing number",
                out int? maxRacingNumber))
        {
            return;
        }

        if (minAge.HasValue &&
            maxAge.HasValue &&
            minAge > maxAge)
        {
            ShowValidation(
                "Minimum age cannot be greater than maximum year of birth.");

            return;
        }

        if (minRacingNumber.HasValue &&
            maxRacingNumber.HasValue &&
            minRacingNumber > maxRacingNumber)
        {
            ShowValidation(
                "Minimum racing number cannot be greater than maximum racing number.");

            return;
        }

        if (minAge is < 0 ||
            maxAge is < 0)
        {
            ShowValidation(
                "Age cannot be negative.");

            return;
        }

        if (minRacingNumber is < 0 ||
            maxRacingNumber is < 0)
        {
            ShowValidation(
                "Racing number cannot be negative.");

            return;
        }

        Criteria =
            new RacerFilterCriteria
            {
                MinBirthYear = minAge,
                MaxBirthYear = maxAge,
                MinRacingNumber = minRacingNumber,
                MaxRacingNumber = maxRacingNumber,

                Gender =
                    (GenderComboBox.SelectedItem
                        as ComboBoxItem)
                    ?.Content?.ToString()
                    ?? "All",

                Status =
                    (StatusComboBox.SelectedItem
                        as ComboBoxItem)
                    ?.Content?.ToString()
                    ?? "All",

                Category =
                    (CategoryComboBox.SelectedItem
                        as ComboBoxItem)
                    ?.Content?.ToString()
                    ?? "All"
            };

        DialogResult = true;
    }

    private void ResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Criteria =
            new RacerFilterCriteria();

        LoadCriteria();
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private bool TryParseOptionalInt(
        string value,
        string fieldName,
        out int? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return true;
        }

        if (int.TryParse(
                value.Trim(),
                out int parsed))
        {
            result = parsed;
            return true;
        }

        ShowValidation(
            $"Please enter a valid number for {fieldName}.");

        result = null;
        return false;
    }

    private void ShowValidation(
        string message)
    {
        MessageBox.Show(
            this,
            message,
            "Invalid Filter",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}

public class RacerFilterCriteria
{
    public int? MinBirthYear { get; set; }

    public int? MaxBirthYear { get; set; }

    public string Gender { get; set; } =
        "All";

    public string Status { get; set; } =
        "All";

    public string Category { get; set; } =
        "All";

    public int? MinRacingNumber { get; set; }

    public int? MaxRacingNumber { get; set; }

    public int ActiveCount
    {
        get
        {
            int count = 0;

            if (MinBirthYear.HasValue)
                count++;

            if (MaxBirthYear.HasValue)
                count++;

            if (!string.Equals(
                    Gender,
                    "All",
                    StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }

            if (!string.Equals(
                    Status,
                    "All",
                    StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }

            if (!string.Equals(
                    Category,
                    "All",
                    StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }

            if (MinRacingNumber.HasValue)
                count++;

            if (MaxRacingNumber.HasValue)
                count++;

            return count;
        }
    }

    public RacerFilterCriteria Clone()
    {
        return new RacerFilterCriteria
        {
            MinBirthYear = MinBirthYear,
            MaxBirthYear = MaxBirthYear,
            Gender = Gender,
            Status = Status,
            Category = Category,
            MinRacingNumber =
                MinRacingNumber,
            MaxRacingNumber =
                MaxRacingNumber
        };
    }
}

