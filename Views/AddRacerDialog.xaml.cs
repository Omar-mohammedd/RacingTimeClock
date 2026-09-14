using System;
using System.Windows;
using RacingTimeClock.Data;
using RacingTimeClock.Models;

namespace RacingTimeClock.Views;

public partial class AddRacerDialog : Window
{
    public AddRacerDialog()
    {
        InitializeComponent();
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        string name = NameTextBox.Text.Trim();
        string racerIdInput = RacerIdTextBox.Text.Trim();
        string yobInput = YobTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a racer name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(yobInput, out int yob) || yob < 1900 || yob > DateTime.Now.Year)
        {
            MessageBox.Show("Please enter a valid Year of Birth (e.g., 2010).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using RacingTimeClockDbContext db = new();

            Racer racer = new Racer
            {
                RacerId = racerIdInput,
                Name = name,
                YearOfBirth = yob,
                IsActive = true
            };

            db.Racers.Add(racer);
            await db.SaveChangesAsync();

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save racer to database.\n\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
