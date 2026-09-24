using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Data;
using RacingTimeClock.Models;
using RacingTimeClock.Services;

namespace RacingTimeClock.Views;

public partial class RacersView : UserControl
{
    private readonly ObservableCollection<RacerDisplayItem> racers =
        new();

    private string selectedCategory = "All";

    public RacersView()
    {
        InitializeComponent();

        Loaded += RacersView_Loaded;
    }

    private async void RacersView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadRacersAsync();
    }

    private async Task LoadRacersAsync()
    {
        try
        {
            DatabaseService databaseService =
                new DatabaseService();

            List<Racer> racerList =
                await databaseService.GetRacersAsync();

            racers.Clear();

            Season? season =
                AppSeasonService.Instance.CurrentSeason;

            foreach (Racer racer in racerList)
            {
                string category =
                    season != null
                        ? SeasonService.GetCategory(racer, season)
                        : "Unknown";

                racers.Add(
                    new RacerDisplayItem
                    {
                        Racer = racer,
                        CategoryDisplay = category
                    });
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load racers.\n\n{ex.Message}",
                "Racers Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<RacerDisplayItem> filtered =
            racers;

        if (!string.Equals(
                selectedCategory,
                "All",
                StringComparison.OrdinalIgnoreCase))
        {
            string category =
                selectedCategory.TrimEnd('s');

            filtered =
                filtered.Where(
                    r => string.Equals(
                        r.CategoryDisplay,
                        category,
                        StringComparison.OrdinalIgnoreCase));
        }

        string search =
            SearchTextBox?.Text?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered =
                filtered.Where(
                    r =>
                        r.Name.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        r.RacingNumber.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        r.Racer.RacerId.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase));
        }

        RacersDataGrid.ItemsSource =
            filtered.ToList();
    }

    private void SearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (SearchPlaceholder != null)
        {
            SearchPlaceholder.Visibility =
                string.IsNullOrWhiteSpace(
                    SearchTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        ApplyFilters();
    }

    private void CategoryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        selectedCategory =
            button.Content?.ToString() ?? "All";

        ApplyFilters();
    }

    private void AddRacerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        AddRacerDialog dialog =
            new AddRacerDialog();

        dialog.Owner =
            Window.GetWindow(this);

        bool? result =
            dialog.ShowDialog();

        if (result == true)
            _ = LoadRacersAsync();
    }

    private async void RacersDataGrid_MouseDoubleClick(
        object sender,
        MouseButtonEventArgs e)
    {
        if (RacersDataGrid.SelectedItem
            is not RacerDisplayItem item)
            return;

        RacerDetailsDialog dialog =
            new RacerDetailsDialog(item.Racer.Id);

        dialog.Owner =
            Window.GetWindow(this);

        dialog.ShowDialog();

        if (dialog.WasDeleted)
        {
            await LoadRacersAsync();
        }
    }
}

public class RacerDisplayItem
{
    public Racer Racer { get; set; } = null!;

    public int Id =>
        Racer.Id;

    public string Name =>
        Racer.Name;

    public string RacingNumber =>
        Racer.RacingNumber;

    public string GenderDisplay =>
        Racer.IsMale ? "Male" : "Female";

    public int BirthYear =>
        Racer.YearOfBirth;

    public string CategoryDisplay { get; set; } =
        string.Empty;
}

