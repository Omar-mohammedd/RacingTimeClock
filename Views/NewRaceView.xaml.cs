using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RacingTimeClock.Views;

public partial class NewRaceView : UserControl
{
    private string selectedRaceType = "Short";
    private string selectedDistance = "100m";
    private int selectedRacerCount = 1;

    public event Action<string, string, int>? RaceStarted;

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
    }

    private void ShortButton_Click(object sender, RoutedEventArgs e)
    {
        selectedRaceType = "Short";
        selectedDistance = "100m";

        BuildDistanceButtons();

        UpdateRaceTypeButtons();
        UpdateDistanceButtons();
    }

    private void LongButton_Click(object sender, RoutedEventArgs e)
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
                Margin = new Thickness(0, 0, 10, 10),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Tag = distance
            };

            button.Click += DistanceButton_Click;

            DistancePanel.Children.Add(button);
        }
    }

    private void DistanceButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string distance)
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
                Margin = new Thickness(0, 0, 10, 10),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Tag = i
            };

            button.Click += RacerCountButton_Click;

            RacerCountPanel.Children.Add(button);
        }
    }

    private void RacerCountButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int count)
        {
            selectedRacerCount = count;

            UpdateRacerCountButtons();
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
        foreach (Button button in DistancePanel.Children)
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
        foreach (Button button in RacerCountPanel.Children)
        {
            if (button.Tag is int count)
            {
                SetButtonSelected(
                    button,
                    count == selectedRacerCount);
            }
        }
    }

    private void SetButtonSelected(Button button, bool selected)
    {
        if (selected)
        {
            button.Background = new SolidColorBrush(
                Color.FromRgb(32, 35, 42));

            button.Foreground = Brushes.White;
        }
        else
        {
            button.Background = Brushes.White;

            button.Foreground = new SolidColorBrush(
                Color.FromRgb(32, 35, 42));
        }
    }

    private void StartRaceButton_Click(object sender, RoutedEventArgs e)
    {
        RaceStarted?.Invoke(
            selectedDistance,
            selectedRaceType,
            selectedRacerCount);
    }
}
