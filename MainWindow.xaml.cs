using System.Collections.Generic;
using System.Windows;
using RacingTimeClock.Views;

namespace RacingTimeClock;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ShowNewRace();
    }

    private void ShowNewRace()
    {
        NewRaceView newRaceView = new NewRaceView();

        newRaceView.RaceStarted += OnRaceStarted;

        PageContent.Content = newRaceView;
    }

    private void OnRaceStarted(
        string distance,
        string raceType,
        int racerCount,
        List<NewRaceView.RacerSelectionItem> racers)
    {
        PageContent.Content =
            new RaceView(distance, raceType, racerCount, racers);
    }

    private void NewRace_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowNewRace();
    }

    private void Racers_Click(
        object sender,
        RoutedEventArgs e)
    {
        PageContent.Content =
            new RacersView();
    }

    private void RacesLog_Click(
        object sender,
        RoutedEventArgs e)
    {
        PageContent.Content =
            new RacesLogView();
    }
}
