using System;
using System.Windows;
using RacingTimeClock.Models;

namespace RacingTimeClock.Views;

public partial class AddRacerDialog : Window
{
    public Racer CreatedRacer { get; private set; } = new();

    public AddRacerDialog()
    {
        InitializeComponent();

        RacerIdTextBox.Focus();
    }

    private void AddButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;

        string racerId =
            RacerIdTextBox.Text.Trim();

        string name =
            NameTextBox.Text.Trim();

        string yearText =
            YearOfBirthTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(racerId))
        {
            ErrorText.Text =
                "Please enter a racer ID.";

            RacerIdTextBox.Focus();

            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorText.Text =
                "Please enter the racer's name.";

            NameTextBox.Focus();

            return;
        }

        if (!int.TryParse(
                yearText,
                out int yearOfBirth))
        {
            ErrorText.Text =
                "Year of birth must be a number.";

            YearOfBirthTextBox.Focus();

            return;
        }

        int currentYear =
            DateTime.Now.Year;

        if (yearOfBirth < 1900 ||
            yearOfBirth > currentYear)
        {
            ErrorText.Text =
                "Please enter a valid year of birth.";

            YearOfBirthTextBox.Focus();

            return;
        }

        CreatedRacer = new Racer
        {
            RacerId = racerId,
            Name = name,
            YearOfBirth = yearOfBirth,
            IsActive = true
        };

        DialogResult = true;
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
