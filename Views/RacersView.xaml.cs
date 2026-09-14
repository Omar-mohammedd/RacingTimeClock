using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RacingTimeClock.Data;
using RacingTimeClock.Models;
using RacingTimeClock.Services;

namespace RacingTimeClock.Views;

public partial class RacersView : UserControl
{
    private readonly ObservableCollection<RacerDisplayItem> allRacers = new();

    public RacersView()
    {
        InitializeComponent();

        CategoryTabs.Items.Add("All");
        CategoryTabs.Items.Add("Seniors");
        CategoryTabs.Items.Add("Juniors");
        CategoryTabs.Items.Add("Youth");

        CategoryTabs.SelectedIndex = 0;

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
            using RacingTimeClockDbContext db = new();

            List<Racer> racers =
                await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .ToListAsync(db.Racers);

            Season? season =
                AppSeasonService.Instance.CurrentSeason;

            if (season == null)
            {
                RacersDataGrid.ItemsSource = null;
                return;
            }

            allRacers.Clear();

            foreach (Racer racer in racers)
            {
                allRacers.Add(
                    new RacerDisplayItem
                    {
                        Racer = racer,
                        BirthYear = racer.YearOfBirth,
                        CategoryDisplay =
                            SeasonService.GetCategory(
                                racer,
                                season)
                    });
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to load racers.\n\n{ex}",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ApplyFilters()
    {
        string search =
            SearchTextBox.Text.Trim();

        string selectedCategory =
            CategoryTabs.SelectedItem?.ToString() ?? "All";

        IEnumerable<RacerDisplayItem> filtered =
            allRacers;

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(
                r => r.Name.Contains(
                         search,
                         StringComparison.OrdinalIgnoreCase)
                     || r.RacingNumber.Contains(
                         search,
                         StringComparison.OrdinalIgnoreCase));
        }

        if (selectedCategory != "All")
        {
            string category =
                selectedCategory.TrimEnd('s');

            filtered = filtered.Where(
                r => r.CategoryDisplay == category);
        }

        RacersDataGrid.ItemsSource =
            filtered.ToList();
    }

    private void SearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        SearchPlaceholder.Visibility =
            string.IsNullOrEmpty(SearchTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

        ApplyFilters();
    }

    private void CategoryTabs_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
            return;

        ApplyFilters();
    }

    private void AddRacerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        AddRacerDialog dialog =
            new AddRacerDialog
            {
                Owner = Window.GetWindow(this)
            };

        bool? result = dialog.ShowDialog();

        if (result == true)
            _ = LoadRacersAsync();
    }

    private void RacersDataGrid_MouseDoubleClick(
        object sender,
        MouseButtonEventArgs e)
    {
        if (RacersDataGrid.SelectedItem
            is not RacerDisplayItem item)
            return;

        MessageBox.Show(
            $"Name: {item.Name}\n" +
            $"Racing Number: {item.RacingNumber}\n" +
            $"Gender: {item.GenderDisplay}\n" +
            $"Year of Birth: {item.BirthYear}\n" +
            $"Category: {item.CategoryDisplay}",
            "Racer Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
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

    public int BirthYear { get; set; }

    public string CategoryDisplay { get; set; } =
        string.Empty;
}










