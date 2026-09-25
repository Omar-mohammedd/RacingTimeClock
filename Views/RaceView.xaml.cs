using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly string competitionType;
    private readonly int racerCount;
    private readonly int seasonId;

    private readonly List<
        NewRaceView.RacerSelectionItem> selectedRacers;

    private readonly TimingService timingService = new();
    private readonly ITimingInput timingInput = new KeyboardTimingInput();
    private readonly DatabaseService databaseService = new();
    private readonly System.Windows.Threading.DispatcherTimer raceTimer;

    private readonly List<bool> racerFinished = new();

    private bool raceStarted;
    private bool raceCompleted;
    private bool raceSaved;

    public RaceView(
        string distance,
        string type,
        string competition,
        int numberOfRacers,
        int selectedSeasonId,
        List<NewRaceView.RacerSelectionItem> racers)
    {
        InitializeComponent();

        raceTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };

        raceTimer.Tick += RaceTimer_Tick;

        raceDistance = distance;
        raceType = type;
        competitionType = competition;
        racerCount = numberOfRacers;
        seasonId = selectedSeasonId;
        selectedRacers = racers;

        RaceTitleText.Text =
            $"{raceDistance} Race";

        RaceInfoText.Text =
            $"{raceType} • {racerCount} Racers";

        for (int i = 0;
             i < racerCount;
             i++)
        {
            racerFinished.Add(false);
        }

        BuildRacerRows();

        Loaded += RaceView_Loaded;
        Unloaded += RaceView_Unloaded;
        timingInput.TimingEventReceived += TimingInput_TimingEventReceived;
    }

    private void RaceView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        timingInput.Start();
        Focus();
    }

    private void RaceView_Unloaded(
        object sender,
        RoutedEventArgs e)
    {
        timingInput.Stop();
    }

    private void TimingInput_TimingEventReceived(
        object? sender,
        TimingEvent e)
    {
        if (raceCompleted)
            return;

        switch (e.Type)
        {
            case TimingEventType.Start:
                StartRace();
                break;

            case TimingEventType.Finish:
                if (e.RacerNumber >= 1 &&
                    e.RacerNumber <= racerCount)
                {
                    FinishRacer(e.RacerNumber);
                }

                break;

            case TimingEventType.Lap:
                break;

            case TimingEventType.FalseStart:
                break;
        }
    }

    private void BuildRacerRows()
    {
        RacersPanel.Children.Clear();

        for (int i = 1;
             i <= racerCount;
             i++)
        {
            Border row = new Border
            {
                Background = (Brush)FindResource("SurfaceBrush"),
                CornerRadius =
                    new CornerRadius(10),
                Padding =
                    new Thickness(18),
                Margin =
                    new Thickness(0, 0, 0, 8),
                Tag = i
            };

            Grid grid = new Grid();

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(250)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(180)
                });

            string displayName =
                selectedRacers[i - 1]
                    .DisplayName;

            TextBlock racerNumber =
                new TextBlock
                {
                    Text =
                        $"#{i}  {displayName}",
                    FontSize = 15,
                    FontWeight =
                        FontWeights.Bold,
                    Foreground =
                        (Brush)FindResource("TextPrimaryBrush")
                };

            Grid.SetColumn(
                racerNumber,
                0);

            TextBlock status =
                new TextBlock
                {
                    Text = "READY",
                    FontSize = 14,
                    FontWeight =
                        FontWeights.SemiBold,
                    Foreground =
                        Brushes.Gray
                };

            Grid.SetColumn(status, 1);

            TextBlock finishTime =
                new TextBlock
                {
                    Text = "--",
                    FontSize = 15,
                    FontWeight =
                        FontWeights.Bold,
                    HorizontalAlignment =
                        HorizontalAlignment.Right
                };

            Grid.SetColumn(
                finishTime,
                2);

            grid.Children.Add(
                racerNumber);

            grid.Children.Add(status);
            grid.Children.Add(finishTime);

            row.Child = grid;

            RacersPanel.Children.Add(row);
        }
    }

    protected override void OnKeyDown(
        KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (raceCompleted)
            return;

        bool handled =
            e.Key == Key.Space ||
            e.Key is
                Key.D1 or Key.D2 or Key.D3 or
                Key.D4 or Key.D5 or Key.D6 or
                Key.D7 or Key.D8 or Key.D9 or
                Key.NumPad1 or Key.NumPad2 or Key.NumPad3 or
                Key.NumPad4 or Key.NumPad5 or Key.NumPad6 or
                Key.NumPad7 or Key.NumPad8 or Key.NumPad9;

        if (!handled)
            return;

        if (timingInput is KeyboardTimingInput keyboardInput)
        {
            keyboardInput.ProcessKey(e.Key);
        }

        e.Handled = true;
    }
    private void StartRace()
    {
        if (raceStarted)
            return;

        timingService.Start();

        raceStarted = true;

        StatusText.Text =
            "RACE RUNNING";

        StatusText.Foreground =
            (Brush)FindResource("SuccessBrush");

        StartTimerDisplay();
    }

    private void StartTimerDisplay()
    {
        raceTimer.Start();
        UpdateTimerDisplay();
    }

    private void RaceTimer_Tick(
        object? sender,
        EventArgs e)
    {
        if (!raceStarted || raceCompleted)
        {
            raceTimer.Stop();
            return;
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        TimeSpan elapsed =
            timingService.GetElapsedTime();

        TimerText.Text =
            FormatTime(elapsed);
    }

    private void FinishRacer(
        int racerNumber)
    {
        if (!raceStarted)
            return;

        int index =
            racerNumber - 1;

        if (racerFinished[index])
            return;

        TimeSpan? finishTime =
            timingService.FinishRacer(
                racerNumber);

        if (!finishTime.HasValue)
            return;

        racerFinished[index] = true;

        if (RacersPanel.Children[index]
            is Border row &&
            row.Child is Grid grid)
        {
            if (grid.Children[1]
                is TextBlock status)
            {
                status.Text =
                    "FINISHED";

                status.Foreground =
                    (Brush)FindResource("SuccessBrush");
            }

            if (grid.Children[2]
                is TextBlock time)
            {
                time.Text =
                    FormatTime(
                        finishTime.Value);
            }
        }

        CheckRaceComplete();
    }

    private void CheckRaceComplete()
    {
        if (!timingService
            .AllRacersFinished(
                racerCount))
        {
            return;
        }

        timingService.Complete();

        raceCompleted = true;
        raceTimer.Stop();
        raceStarted = false;

        StatusText.Text =
            "RACE COMPLETE";

        StatusText.Foreground =
            (Brush)FindResource("AccentBrush");

        SaveRaceButton.Visibility =
            Visibility.Visible;

        UpdateTimerDisplay();
    }

    private async void SaveRaceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!raceCompleted ||
            raceSaved)
        {
            return;
        }

        if (seasonId <= 0)
        {
            MessageBox.Show(
                "The race has no valid season.",
                "Save Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return;
        }

        Race race = new Race
        {
            Distance = raceDistance,
            RaceType = raceType,
            CompetitionType = competitionType,
            StartDateTime =
                timingService.StartDateTime,
            RacerCount = racerCount,
            IsCompleted = true,
            SeasonId = seasonId
        };

        var orderedResults =
            timingService
                .GetResults()
                .OrderBy(result =>
                    result.Value)
                .ToList();

        int position = 1;

        foreach (var result
                 in orderedResults)
        {
            int racerNumber =
                result.Key;

            TimeSpan finishTime =
                result.Value;

            long finishTimestamp =
                timingService
                    .GetFinishTimestamp(
                        racerNumber)
                ?? 0;

            int? racerDatabaseId =
                selectedRacers[
                    racerNumber - 1]
                    .Racer?.Id;

            race.Results.Add(
                new RacerResult
                {
                    RacerNumber =
                        racerNumber,

                    RacerId =
                        racerDatabaseId,

                    Position =
                        position,

                    FinishTime =
                        finishTime,

                    FinishTimestamp =
                        finishTimestamp
                });

            position++;
        }

        try
        {
            await databaseService
                .SaveRaceAsync(race);

            raceSaved = true;

            SaveRaceButton.Content =
                "RACE SAVED";

            SaveRaceButton.IsEnabled =
                false;

            StatusText.Text =
                "RACE SAVED";

            StatusText.Foreground =
                (Brush)FindResource("SuccessBrush");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "Save Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    protected override void OnVisualParentChanged(
        DependencyObject oldParent)
    {
        if (VisualParent == null)
        {
            raceTimer.Stop();
            timingInput.Stop();
        }

        base.OnVisualParentChanged(oldParent);
    }

    private string FormatTime(
        TimeSpan time)
    {
        return
            $"{(int)time.TotalMinutes:00}:" +
            $"{time.Seconds:00}." +
            $"{time.Milliseconds:000}";
    }
}











