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
        NewRaceView newRaceView =
            new NewRaceView();

        PageContent.Content =
            newRaceView;
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

    private void Settings_Click(
        object sender,
        RoutedEventArgs e)
    {
        PageContent.Content =
            new SettingsView();
    }
}
