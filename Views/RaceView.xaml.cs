using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RacingTimeClock.Models;
using RacingTimeClock.Services;
using RacingTimeClock.Timing;

namespace RacingTimeClock.Views;

public partial class RaceView : UserControl
{
    private readonly string raceDistance;
    private readonly string raceType;
    private readonly int racerCount;
    private readonly List<NewRaceView.RacerSelectionItem> selectedRacers;

    private readonly TimingService timingService = new();
    private readonly DatabaseService databaseService = new();

    private readonly List<bool> racerFinished = new();

    private bool raceStarted;
    private bool raceCompleted;
    private bool raceSaved;

    public RaceView(
        string distance,
        string type,
        int numberOfRacers,
        List<NewRaceView.RacerSelectionItem> racers)
    {
        InitializeComponent();

        raceDistance = distance;
        raceType = type;
        racerCount = numberOfRacers;
        selectedRacers = racers;

        RaceTitleText.Text = $"{raceDistance} Race";
        RaceInfoText.Text = $"{raceType} • {racerCount} Racers";

        for (int i = 0; i < racerCount; i++)
        {
            racerFinished.Add(false);
        }

        BuildRacerRows();

        Loaded += RaceView_Loaded;
    }

    private void RaceView_Loaded(object sender, RoutedEventArgs e)
    {
        Focus();
    }

    private void BuildRacerRows()
    {
        RacersPanel.Children.Clear();

        for (int i = 1; i <= racerCount; i++)
        {
            Border row = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 10),
                Tag = i
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });

            string displayName = selectedRacers[i - 1].DisplayName;

            TextBlock racerNumber = new TextBlock
            {
                Text = $"#{i}  {displayName}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(32, 35, 42))
            };
            Grid.SetColumn(racerNumber, 0);

            TextBlock status = new TextBlock
            {
                Text = "READY",
                FontSize = 17,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Gray
            };
            Grid.SetColumn(status, 1);

            TextBlock finishTime = new TextBlock
            {
                Text = "--",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(finishTime, 2);

            grid.Children.Add(racerNumber);
            grid.Children.Add(status);
            grid.Children.Add(finishTime);

            row.Child = grid;
            RacersPanel.Children.Add(row);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (raceCompleted)
            return;

        if (e.Key == Key.Space)
        {
            StartRace();
            e.Handled = true;
            return;
        }

        int racerNumber = GetRacerNumber(e.Key);

        if (racerNumber >= 1 && racerNumber <= racerCount)
        {
            FinishRacer(racerNumber);
            e.Handled = true;
        }
    }

    private int GetRacerNumber(Key key)
    {
        return key switch
        {
            Key.D1 or Key.NumPad1 => 1,
            Key.D2 or Key.NumPad2 => 2,
            Key.D3 or Key.NumPad3 => 3,
            Key.D4 or Key.NumPad4 => 4,
            Key.D5 or Key.NumPad5 => 5,
            Key.D6 or Key.NumPad6 => 6,
            Key.D7 or Key.NumPad7 => 7,
            Key.D8 or Key.NumPad8 => 8,
            Key.D9 or Key.NumPad9 => 9,
            _ => 0
        };
    }

    private void StartRace()
    {
        if (raceStarted)
            return;

        timingService.Start();
        raceStarted = true;

        StatusText.Text = "RACE RUNNING";
        StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 70));

        StartTimerDisplay();
    }

    private async void StartTimerDisplay()
    {
        while (raceStarted && !raceCompleted)
        {
            UpdateTimerDisplay();
            await System.Threading.Tasks.Task.Delay(10);
        }
    }

    private void UpdateTimerDisplay()
    {
        TimeSpan elapsed = timingService.GetElapsedTime();
        TimerText.Text = FormatTime(elapsed);
    }

    private void FinishRacer(int racerNumber)
    {
        if (!raceStarted)
            return;

        int index = racerNumber - 1;

        if (racerFinished[index])
            return;

        TimeSpan? finishTime = timingService.FinishRacer(racerNumber);

        if (!finishTime.HasValue)
            return;

        racerFinished[index] = true;

        if (RacersPanel.Children[index] is Border row && row.Child is Grid grid)
        {
            if (grid.Children[1] is TextBlock status)
            {
                status.Text = "FINISHED";
                status.Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 70));
            }

            if (grid.Children[2] is TextBlock time)
            {
                time.Text = FormatTime(finishTime.Value);
            }
        }

        CheckRaceComplete();
    }

    private void CheckRaceComplete()
    {
        if (!timingService.AllRacersFinished(racerCount))
            return;

        timingService.Complete();

        raceCompleted = true;
        raceStarted = false;

        StatusText.Text = "RACE COMPLETE";
        StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0, 100, 180));

        SaveRaceButton.Visibility = Visibility.Visible;
        UpdateTimerDisplay();
    }

    private async void SaveRaceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!raceCompleted || raceSaved)
            return;

        Season? activeSeason = await databaseService.GetActiveSeasonAsync();

        if (activeSeason == null)
        {
            MessageBox.Show("No active season exists.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Race race = new Race
        {
            Distance = raceDistance,
            RaceType = raceType,
            StartDateTime = timingService.StartDateTime,
            RacerCount = racerCount,
            IsCompleted = true,
            SeasonId = activeSeason.Id
        };

        int position = 1;

        foreach (var result in timingService.GetResults())
        {
            int racerNumber = result.Key;
            TimeSpan finishTime = result.Value;
            long finishTimestamp = timingService.GetFinishTimestamp(racerNumber) ?? 0;

            int? racerDatabaseId = selectedRacers[racerNumber - 1].Racer?.Id;

            race.Results.Add(new RacerResult
            {
                RacerNumber = racerNumber,
                RacerId = racerDatabaseId,
                Position = position,
                FinishTime = finishTime,
                FinishTimestamp = finishTimestamp
            });

            position++;
        }

        try
        {
            await databaseService.SaveRaceAsync(race);

            raceSaved = true;
            SaveRaceButton.Content = "RACE SAVED";
            SaveRaceButton.IsEnabled = false;

            StatusText.Text = "RACE SAVED";
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 70));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string FormatTime(TimeSpan time)
    {
        return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
    }
}
