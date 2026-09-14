using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    private int selectedRacerCount = 1;
    private string selectedCategory = "All";

    private List<Racer> availableRacers = new();

    private readonly List<ComboBox> racerComboBoxes = new();

    public class RacerSelectionItem
    {
        public Racer? Racer { get; set; }

        public string DisplayName { get; set; } =
            string.Empty;

        public override string ToString()
        {
            return DisplayName;
        }
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
        BuildRacerCountButtons();
        UpdateRaceTypeButtons();
        UpdateDistanceButtons();
        UpdateRacerCountButtons();
        UpdateCategoryButtons();

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

            availableRacers = await db.Racers
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();

            RebuildRacerSelectionUI();
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

    private Season? GetSelectedSeason()
    {
        return AppSeasonService.Instance.CurrentSeason;
    }

    private bool IsRacerInSelectedCategory(
        Racer racer)
    {
        Season? season = GetSelectedSeason();

        if (season == null)
            return false;

        int seniorMaximumBirthYear =
            season.StartYear - 18;

        int juniorMinimumBirthYear =
            season.StartYear - 17;

        int juniorMaximumBirthYear =
            season.StartYear - 15;

        int youthMinimumBirthYear =
            season.StartYear - 14;

        return selectedCategory switch
        {
            "Seniors" =>
                racer.YearOfBirth <= seniorMaximumBirthYear,

            "Juniors" =>
                racer.YearOfBirth >= juniorMinimumBirthYear &&
                racer.YearOfBirth <= juniorMaximumBirthYear,

            "Youth" =>
                racer.YearOfBirth >= youthMinimumBirthYear,

            _ => true
        };
    }

    private void RebuildRacerSelectionUI()
    {
        RacerSelectionPanel.Children.Clear();
        racerComboBoxes.Clear();

        if (GetSelectedSeason() == null)
            return;

        List<Racer> filteredRacers =
            availableRacers
                .Where(IsRacerInSelectedCategory)
                .ToList();

        for (int i = 1;
             i <= selectedRacerCount;
             i++)
        {
            Grid container = new Grid
            {
                Margin =
                    new Thickness(0, 0, 0, 10)
            };

            container.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(100)
                });

            container.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(
                        1,
                        GridUnitType.Star)
                });

            TextBlock label = new TextBlock
            {
                Text = $"Lane {i}:",
                FontSize = 16,
                FontWeight =
                    FontWeights.SemiBold,
                VerticalAlignment =
                    VerticalAlignment.Center
            };

            Grid.SetColumn(label, 0);

            ComboBox comboBox = new ComboBox
            {
                Height = 38,
                FontSize = 15,
                VerticalContentAlignment =
                    VerticalAlignment.Center
            };

            comboBox.Items.Add(
                new RacerSelectionItem
                {
                    Racer = null,
                    DisplayName = $"Guest {i}"
                });

            foreach (Racer racer in filteredRacers)
            {
                comboBox.Items.Add(
                    new RacerSelectionItem
                    {
                        Racer = racer,
                        DisplayName =
                            $"{racer.Name} ({racer.RacerId})"
                    });
            }

            comboBox.SelectedIndex = 0;

            Grid.SetColumn(comboBox, 1);

            container.Children.Add(label);
            container.Children.Add(comboBox);

            RacerSelectionPanel.Children.Add(
                container);

            racerComboBoxes.Add(comboBox);
        }
    }

    private void AllCategoryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        selectedCategory = "All";

        UpdateCategoryButtons();
        RebuildRacerSelectionUI();
    }

    private void SeniorCategoryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        selectedCategory = "Seniors";

        UpdateCategoryButtons();
        RebuildRacerSelectionUI();
    }

    private void JuniorCategoryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        selectedCategory = "Juniors";

        UpdateCategoryButtons();
        RebuildRacerSelectionUI();
    }

    private void YouthCategoryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        selectedCategory = "Youth";

        UpdateCategoryButtons();
        RebuildRacerSelectionUI();
    }

    private void UpdateCategoryButtons()
    {
        SetButtonSelected(
            AllCategoryButton,
            selectedCategory == "All");

        SetButtonSelected(
            SeniorCategoryButton,
            selectedCategory == "Seniors");

        SetButtonSelected(
            JuniorCategoryButton,
            selectedCategory == "Juniors");

        SetButtonSelected(
            YouthCategoryButton,
            selectedCategory == "Youth");
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
                Width = 110,
                Height = 50,
                Margin =
                    new Thickness(0, 0, 10, 10),
                FontSize = 15,
                FontWeight =
                    FontWeights.SemiBold,
                Tag = distance
            };

            button.Click += DistanceButton_Click;

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

    private void BuildRacerCountButtons()
    {
        RacerCountPanel.Children.Clear();

        for (int i = 1; i <= 9; i++)
        {
            Button button = new Button
            {
                Content = i.ToString(),
                Width = 55,
                Height = 50,
                Margin =
                    new Thickness(0, 0, 10, 10),
                FontSize = 16,
                FontWeight =
                    FontWeights.SemiBold,
                Tag = i
            };

            button.Click +=
                RacerCountButton_Click;

            RacerCountPanel.Children.Add(button);
        }
    }

    private void RacerCountButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is int count)
        {
            selectedRacerCount = count;

            UpdateRacerCountButtons();
            RebuildRacerSelectionUI();
        }
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

    private void UpdateRacerCountButtons()
    {
        foreach (Button button
                 in RacerCountPanel.Children)
        {
            if (button.Tag is int count)
            {
                SetButtonSelected(
                    button,
                    count == selectedRacerCount);
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
                new SolidColorBrush(
                    Color.FromRgb(
                        32,
                        35,
                        42));

            button.Foreground =
                Brushes.White;
        }
        else
        {
            button.Background =
                Brushes.White;

            button.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        32,
                        35,
                        42));
        }
    }

    private void StartRaceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;

        Season? season =
            AppSeasonService.Instance.CurrentSeason;

        if (season == null)
        {
            ErrorText.Text =
                "No season is selected. Please select a season in Settings.";

            return;
        }

        List<RacerSelectionItem> selections =
            racerComboBoxes
                .Select(combo =>
                    combo.SelectedItem
                    as RacerSelectionItem
                    ?? new RacerSelectionItem
                    {
                        Racer = null,
                        DisplayName = "Guest"
                    })
                .ToList();

        HashSet<int> selectedIds =
            new HashSet<int>();

        foreach (RacerSelectionItem selection
                 in selections)
        {
            if (selection.Racer == null)
                continue;

            if (!selectedIds.Add(
                    selection.Racer.Id))
            {
                ErrorText.Text =
                    "The same racer cannot be assigned " +
                    "to more than one lane.";

                return;
            }
        }

        if (selections.Count !=
            selectedRacerCount)
        {
            ErrorText.Text =
                "Please select all race lanes.";

            return;
        }

        RaceStarted?.Invoke(
            selectedDistance,
            selectedRaceType,
            selectedRacerCount,
            season.Id,
            selections);
    }
}
