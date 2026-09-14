using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Data;
using RacingTimeClock.Models;

namespace RacingTimeClock.Views;

public partial class RacersView : UserControl
{
    public class RacerDisplayItem
    {
        public Racer Racer { get; set; } = null!;
        public int Id => Racer.Id;
        public string RacerId => Racer.RacerId?.ToString() ?? string.Empty;
        public string Name => Racer.Name;
        public string CategoryDisplay { get; set; } = string.Empty;
        public int? BirthYear { get; set; }
        public bool IsSenior { get; set; }
    }

    private List<RacerDisplayItem> displayRacers = new();

    public RacersView()
    {
        InitializeComponent();
        Loaded += RacersView_Loaded;
    }

    private async void RacersView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadRacersAsync();
    }

    private async Task LoadRacersAsync()
    {
        try
        {
            using RacingTimeClockDbContext db = new();
            var rawRacers = await db.Racers
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();

            int currentYear = DateTime.Now.Year;

            displayRacers = rawRacers.Select(r =>
            {
                int? yob = r.YearOfBirth > 0 ? r.YearOfBirth : null;

                bool isSenior = yob.HasValue ? (currentYear - yob.Value >= 18) : true;
                string categoryStr = isSenior ? "Seniors" : (yob.HasValue ? yob.Value.ToString() : "Seniors");

                return new RacerDisplayItem
                {
                    Racer = r,
                    BirthYear = yob,
                    IsSenior = isSenior,
                    CategoryDisplay = categoryStr
                };
            }).ToList();

            PopulateCategoryTabs();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not load racers.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PopulateCategoryTabs()
    {
        CategoryTabs.SelectionChanged -= CategoryTabs_SelectionChanged;

        CategoryTabs.Items.Clear();
        CategoryTabs.Items.Add("All");

        var juniorYears = displayRacers
            .Where(r => !r.IsSenior && r.BirthYear.HasValue)
            .Select(r => r.BirthYear!.Value.ToString())
            .Distinct()
            .OrderByDescending(y => y);

        foreach (var year in juniorYears)
        {
            CategoryTabs.Items.Add(year);
        }

        CategoryTabs.Items.Add("Seniors");
        CategoryTabs.SelectedIndex = 0;

        CategoryTabs.SelectionChanged += CategoryTabs_SelectionChanged;
    }

    private void ApplyFilter()
    {
        string query = SearchTextBox.Text.Trim().ToLower();
        string selectedTab = CategoryTabs.SelectedItem as string ?? "All";

        var filtered = displayRacers.AsEnumerable();

        if (selectedTab != "All")
        {
            if (selectedTab == "Seniors")
            {
                filtered = filtered.Where(r => r.IsSenior);
            }
            else
            {
                filtered = filtered.Where(r => !r.IsSenior && r.BirthYear.HasValue && r.BirthYear.Value.ToString() == selectedTab);
            }
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(r => r.Name.ToLower().Contains(query) ||
                                           r.RacerId.ToLower().Contains(query));
        }

        RacersDataGrid.ItemsSource = filtered.ToList();
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void CategoryTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private async void AddRacerButton_Click(object sender, RoutedEventArgs e)
    {
        AddRacerDialog dialog = new AddRacerDialog
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            await LoadRacersAsync();
        }
    }

    private async void RacersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (RacersDataGrid.SelectedItem is RacerDisplayItem selectedItem)
        {
            RacerDetailsDialog dialog = new RacerDetailsDialog(selectedItem.Id)
            {
                Owner = Window.GetWindow(this)
            };

            dialog.ShowDialog();

            if (dialog.WasDeleted)
            {
                await LoadRacersAsync();
            }
        }
    }
}
