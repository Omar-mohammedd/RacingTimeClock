using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Data;
using RacingTimeClock.Models;
using RacingTimeClock.Services;

namespace RacingTimeClock.Views;

public partial class NewRaceView : UserControl
{
    private string selectedRaceType = "Short";
    private string selectedDistance = "100m";
    private int selectedRacerCount = 4;
    private string selectedGender = "All";
    private string selectedCategory = "All";

    private List<Racer> availableRacers = new();
    private readonly List<Racer> selectedRacers = new();

    public class RacerSelectionItem
    {
        public Racer? Racer { get; set; }

        public string DisplayName { get; set; } =
            string.Empty;

        public bool IsPlaceholder { get; set; }

        public override string ToString() =>
            DisplayName;
    }

    public event Action<
        string,
        string,
        int,
        int,
        List<RacerSelectionItem>>? RaceStarted;

    private readonly string[] shortDistances =
    {
        "100m",
        "200m",
        "400m",
        "500m",
        "1000m"
    };

    private readonly string[] longDistances =
    {
        "5K",
        "10K",
        "15K"
    };

    public NewRaceView()
    {
        InitializeComponent();

        BuildDistanceButtons();
        UpdateRaceTypeButtons();
        UpdateDistanceButtons();
        UpdateDistanceHint();
        UpdateRacerCountDisplay();

        Loaded += NewRaceView_Loaded;
    }

    private async void NewRaceView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            using RacingTimeClockDbContext db =
                new RacingTimeClockDbContext();

            availableRacers =
                await db.Racers
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.Name)
                    .ToListAsync();

            RebuildSearchResults();
        RebuildRacerPicker();
            RebuildRacerTable();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load race setup data.\n\n{ex}",
                "New Race Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void BuildDistanceButtons()
    {
        DistancePanel.Children.Clear();

        string[] distances =
            selectedRaceType == "Short"
                ? shortDistances
                : longDistances;

        foreach (string distance in distances)
        {
            Button button = new Button
            {
                Content = distance,

                Style =
                    (Style)FindResource(
                        "RaceChoiceButtonStyle"),

                Width =
                    distance == "1000m"
                        ? 70
                        : 66,

                Height = 34,

                Margin =
                    new Thickness(0, 0, 8, 0),

                Tag = distance
            };

            button.Click +=
                DistanceButton_Click;

            DistancePanel.Children.Add(button);
        }
    }

    private void DistanceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is string distance)
        {
            selectedDistance = distance;
            UpdateDistanceButtons();
        }
    }

    private void ShortButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        selectedRaceType = "Short";
        selectedDistance = "100m";

        BuildDistanceButtons();
        UpdateRaceTypeButtons();
        UpdateDistanceButtons();
        UpdateDistanceHint();
    }

    private void LongButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        selectedRaceType = "Long";
        selectedDistance = "5K";

        BuildDistanceButtons();
        UpdateRaceTypeButtons();
        UpdateDistanceButtons();
        UpdateDistanceHint();
    }

    private void UpdateRaceTypeButtons()
    {
        SetButtonSelected(
            ShortButton,
            selectedRaceType == "Short");

        SetButtonSelected(
            LongButton,
            selectedRaceType == "Long");
    }

    private void UpdateDistanceButtons()
    {
        foreach (Button button
                 in DistancePanel.Children)
        {
            if (button.Tag is string distance)
            {
                SetButtonSelected(
                    button,
                    distance == selectedDistance);
            }
        }
    }

    private void SetButtonSelected(
        Button button,
        bool selected)
    {
        if (selected)
        {
            button.Background =
                (Brush)FindResource(
                    "AccentBrush");

            button.Foreground =
                Brushes.White;

            button.BorderBrush =
                (Brush)FindResource(
                    "AccentBrush");
        }
        else
        {
            button.Background =
                (Brush)FindResource(
                    "SurfaceBrush");

            button.Foreground =
                (Brush)FindResource(
                    "TextPrimaryBrush");

            button.BorderBrush =
                (Brush)FindResource(
                    "BorderBrush");
        }
    }

    private void UpdateDistanceHint()
    {
        DistanceHintText.Text =
            selectedRaceType == "Short"
                ? "Short: 100m · 200m · 400m · 500m · 1000m"
                : "Long: 5K · 10K · 15K";
    }

    private void UpdateRacerCountDisplay()
    {
        RacerCountText.Text =
            selectedRacerCount.ToString();

        SelectedCountText.Text =
            $"{selectedRacers.Count} / {selectedRacerCount} racers selected";
    }

    private void DecreaseRacerCountButton_Click(
        object sender,
        MouseButtonEventArgs e)
    {
        if (selectedRacerCount <= 1)
            return;

        selectedRacerCount--;

        while (selectedRacers.Count >
               selectedRacerCount)
        {
            selectedRacers.RemoveAt(
                selectedRacers.Count - 1);
        }

        UpdateRacerCountDisplay();
        RebuildRacerTable();
        RebuildSearchResults();
        RebuildRacerPicker();
    }

    private void IncreaseRacerCountButton_Click(
        object sender,
        MouseButtonEventArgs e)
    {
        if (selectedRacerCount >= 9)
            return;

        selectedRacerCount++;

        UpdateRacerCountDisplay();
        RebuildRacerTable();
        RebuildSearchResults();
        RebuildRacerPicker();
    }

    private IEnumerable<Racer> GetFilteredRacers()
    {
        IEnumerable<Racer> racers =
            availableRacers;

        if (selectedGender == "Male")
        {
            racers =
                racers.Where(r => r.IsMale);
        }
        else if (selectedGender == "Female")
        {
            racers =
                racers.Where(r => !r.IsMale);
        }

        if (selectedCategory != "All")
        {
            Season? season =
                AppSeasonService.Instance.CurrentSeason;

            if (season != null)
            {
                racers =
                    racers.Where(r =>
                        SeasonService.GetCategory(
                            r,
                            season) ==
                        selectedCategory);
            }
        }

        string search =
            SearchTextBox.Text ==
            "Search for player"
                ? string.Empty
                : SearchTextBox.Text.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            racers =
                racers.Where(r =>
                    r.Name.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    r.RacingNumber.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase));
        }

        return racers
            .Where(r =>
                !selectedRacers.Any(
                    selected =>
                        selected.Id == r.Id))
            .OrderBy(r => r.Name);
    }

    private void RebuildSearchResults()
    {
        SearchResultsPanel.Children.Clear();

        string search =
            SearchTextBox.Text ==
            "Search for player"
                ? string.Empty
                : SearchTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(search))
        {
            SearchResultsBorder.Visibility =
                Visibility.Collapsed;

            return;
        }

        List<Racer> racers =
            GetFilteredRacers().ToList();

        if (racers.Count == 0)
        {
            SearchResultsPanel.Children.Add(
                new TextBlock
                {
                    Text = "No matching racers",
                    FontSize = 12,
                    Foreground =
                        (Brush)FindResource(
                            "TextSecondaryBrush"),
                    Padding =
                        new Thickness(
                            12,
                            10,
                            12,
                            10)
                });

            SearchResultsBorder.Visibility =
                Visibility.Visible;

            return;
        }

        foreach (Racer racer in racers)
        {
            Button resultButton =
                new Button
                {
                    Content =
                        $"{racer.Name}  |  {racer.RacingNumber}",

                    Tag = racer,

                    Style =
                        (Style)FindResource(
                            "SearchResultButtonStyle")
                };

            resultButton.Click +=
                SearchResultButton_Click;

            SearchResultsPanel.Children.Add(
                resultButton);
        }

        SearchResultsBorder.Visibility =
            Visibility.Visible;
    }

    private void SearchResultButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not Racer racer)
        {
            return;
        }

        if (selectedRacers.Count >=
            selectedRacerCount)
        {
            ErrorText.Text =
                "Increase the number of racers before adding another racer.";

            return;
        }

        if (selectedRacers.Any(
                r => r.Id == racer.Id))
        {
            return;
        }

        ErrorText.Text =
            string.Empty;

        selectedRacers.Add(racer);

        SearchTextBox.Text =
            string.Empty;

        SearchTextBox.Foreground =
            (Brush)FindResource(
                "TextMutedBrush");

        SearchResultsBorder.Visibility =
            Visibility.Collapsed;

        UpdateRacerCountDisplay();
        RebuildRacerTable();
        RebuildSearchResults();
        RebuildRacerPicker();
    }

    private void RebuildRacerPicker(){RacerPickerComboBox.SelectionChanged -= RacerPickerComboBox_SelectionChanged;RacerPickerComboBox.Items.Clear();RacerPickerComboBox.Items.Add(new RacerSelectionItem { Racer = null, DisplayName = "Select racer", IsPlaceholder = true });foreach (Racer racer in GetFilteredRacers()){RacerPickerComboBox.Items.Add(new RacerSelectionItem { Racer = racer, DisplayName = $"{racer.Name} | {racer.RacingNumber}" });}RacerPickerComboBox.SelectedIndex = 0;RacerPickerComboBox.SelectionChanged += RacerPickerComboBox_SelectionChanged;}

    private void RacerPickerComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (RacerPickerComboBox.SelectedItem
            is not RacerSelectionItem item ||
            item.Racer == null)
        {
            return;
        }

        if (selectedRacers.Count >=
            selectedRacerCount)
        {
            ErrorText.Text =
                "Increase the number of racers before adding another racer.";

            RacerPickerComboBox.SelectedIndex = 0;
            return;
        }

        if (selectedRacers.Any(
                r => r.Id == item.Racer.Id))
        {
            RacerPickerComboBox.SelectedIndex = 0;
            return;
        }

        ErrorText.Text =
            string.Empty;

        selectedRacers.Add(item.Racer);

        UpdateRacerCountDisplay();
        RebuildRacerTable();
        RebuildSearchResults();
        RebuildRacerPicker();

        RacerPickerComboBox.SelectedIndex = 0;
    }
    private void GenderFilter_Changed(
        object sender,
        RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;

        if (MaleRadio.IsChecked == true)
        {
            selectedGender = "Male";
        }
        else if (FemaleRadio.IsChecked == true)
        {
            selectedGender = "Female";
        }
        else
        {
            selectedGender = "All";
        }

        RebuildSearchResults();
        RebuildRacerPicker();
    }

    private void CategoryComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
            return;

        selectedCategory =
            CategoryComboBox.SelectedItem
                is ComboBoxItem item
                ? item.Content?.ToString() ??
                  "All"
                : "All";

        RebuildSearchResults();
        RebuildRacerPicker();
    }

    private void RebuildRacerTable()
    {
        RacerTablePanel.Children.Clear();

        Season? season =
            AppSeasonService.Instance.CurrentSeason;

        for (int i = 0;
             i < selectedRacers.Count;
             i++)
        {
            Racer racer =
                selectedRacers[i];

            string category =
                season == null
                    ? "—"
                    : SeasonService.GetCategory(
                        racer,
                        season);

            Grid row =
                new Grid
                {
                    Height = 44,
                    Margin =
                        new Thickness(
                            8,
                            0,
                            8,
                            0)
                };

            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(65)
                });

            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(105)
                });

            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(75)
                });

            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(75)
                });

            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(80)
                });

            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(75)
                });


            Border rowBorder =
                new Border
                {
                    Background =
                        (Brush)FindResource(
                            "SurfaceBrush"),

                    BorderBrush =
                        (Brush)FindResource(
                            "BorderBrush"),

                    BorderThickness =
                        new Thickness(
                            0,
                            0,
                            0,
                            1)
                };

            Grid rowContent =
                new Grid();

            for (int c = 0; c < 7; c++)
            {
                rowContent.ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width =
                            row.ColumnDefinitions[c]
                                .Width
                    });
            }


            TextBlock laneText = new TextBlock { Text = (i + 1).ToString(), FontSize = 11, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("TextPrimaryBrush"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }; Grid.SetColumn(laneText, 0); rowContent.Children.Add(laneText);


            TextBlock name =
                CreateTableText(
                    racer.Name,
                    true);

            Grid.SetColumn(
                name,
                1);

            rowContent.Children.Add(
                name);


            TextBlock racingNumber =
                CreateTableText(
                    racer.RacingNumber,
                    true);

            Grid.SetColumn(
                racingNumber,
                2);

            rowContent.Children.Add(
                racingNumber);


            TextBlock gender =
                CreateTableText(
                    racer.IsMale
                        ? "Male"
                        : "Female");

            Grid.SetColumn(
                gender,
                3);

            rowContent.Children.Add(
                gender);


            TextBlock yearOfBirth =
                CreateTableText(
                    racer.YearOfBirth.ToString());

            Grid.SetColumn(
                yearOfBirth,
                4);

            rowContent.Children.Add(
                yearOfBirth);


            TextBlock categoryText =
                CreateTableText(
                    category,
                    true);

            Grid.SetColumn(
                categoryText,
                5);

            rowContent.Children.Add(
                categoryText);


            Button removeButton =
                new Button
                {
                    Content = "Remove",

                    Tag = racer,

                    Style =
                        (Style)FindResource(
                            "RemoveTextButtonStyle"),

                    HorizontalAlignment =
                        HorizontalAlignment.Right,

                    VerticalAlignment =
                        VerticalAlignment.Center,

                    Margin =
                        new Thickness(
                            0,
                            0,
                            4,
                            0)
                };

            removeButton.Click +=
                RemoveRacerButton_Click;

            Grid.SetColumn(
                removeButton,
                6);

            rowContent.Children.Add(
                removeButton);


            rowBorder.Child =
                rowContent;

            RacerTablePanel.Children.Add(
                rowBorder);
        }

        UpdateRacerCountDisplay();
    }

    private TextBlock CreateTableText(
        string text,
        bool semiBold = false)
    {
        return new TextBlock
        {
            Text = text,

            FontSize = 11,

            FontWeight =
                semiBold
                    ? FontWeights.SemiBold
                    : FontWeights.Normal,

            Foreground =
                (Brush)FindResource(
                    "TextPrimaryBrush"),

            VerticalAlignment =
                VerticalAlignment.Center
        };
    }

    private void RemoveRacerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is Racer racer)
        {
            selectedRacers.RemoveAll(
                r => r.Id == racer.Id);

            ErrorText.Text =
                string.Empty;

            UpdateRacerCountDisplay();
            RebuildRacerTable();
            RebuildSearchResults();
        RebuildRacerPicker();
        }
    }

    private void SearchTextBox_GotFocus(
        object sender,
        RoutedEventArgs e)
    {
        if (SearchTextBox.Text ==
            "Search for player")
        {
            SearchTextBox.Text =
                string.Empty;

            SearchTextBox.Foreground =
                (Brush)FindResource(
                    "TextPrimaryBrush");
        }
    }

    private void SearchTextBox_LostFocus(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(
                SearchTextBox.Text))
        {
            SearchTextBox.Text =
                "Search for player";

            SearchTextBox.Foreground =
                (Brush)FindResource(
                    "TextMutedBrush");

            SearchResultsBorder.Visibility =
                Visibility.Collapsed;
        }
    }

    private void SearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (!IsLoaded)
            return;

        RebuildSearchResults();
        RebuildRacerPicker();
    }

    private void StartRaceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ErrorText.Text =
            string.Empty;

        Season? season =
            AppSeasonService.Instance
                .CurrentSeason;

        if (season == null)
        {
            ErrorText.Text =
                "No season is selected. Please select a season in Settings.";

            return;
        }

        if (selectedRacers.Count !=
            selectedRacerCount)
        {
            ErrorText.Text =
                "Please select all race lanes.";

            return;
        }

        List<RacerSelectionItem> selections =
            selectedRacers
                .Select(r =>
                    new RacerSelectionItem
                    {
                        Racer = r,

                        DisplayName =
                            $"{r.Name} | {r.RacingNumber}"
                    })
                .ToList();

        RaceStarted?.Invoke(
            selectedDistance,
            selectedRaceType,
            selectedRacerCount,
            season.Id,
            selections);
    }
}









