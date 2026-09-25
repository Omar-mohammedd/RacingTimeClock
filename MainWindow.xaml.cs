using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using RacingTimeClock.Services;
using RacingTimeClock.Views;

namespace RacingTimeClock;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        AppSeasonService.Instance.PropertyChanged +=
            AppSeasonService_PropertyChanged;

        UpdateSeasonLabel();

        ShowNewRace();
    }

    private void AppSeasonService_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName ==
            nameof(AppSeasonService.CurrentSeason))
        {
            UpdateSeasonLabel();
        }
    }

    private void UpdateSeasonLabel()
    {
        SeasonLabel.Text =
            AppSeasonService.Instance.CurrentSeason != null
                ? $"Season {AppSeasonService.Instance.CurrentSeason.Name}"
                : "Season";
    }

    private void SetActiveNavigation(
        string page)
    {
        NewRaceNavButton.Tag =
            page == "NewRace"
                ? "Active"
                : null;

        RacersNavButton.Tag =
            page == "Racers"
                ? "Active"
                : null;

        RacesLogNavButton.Tag =
            page == "RacesLog"
                ? "Active"
                : null;

        SettingsNavButton.Tag =
            page == "Settings"
                ? "Active"
                : null;
    }

    private void ShowNewRace()
    {
        SetActiveNavigation("NewRace");

        NewRaceView newRaceView =
            new NewRaceView();

        newRaceView.RaceStarted +=
            OnRaceStarted;

        PageContent.Content =
            newRaceView;
    }

    private void OnRaceStarted(
        string distance,
        string raceType,
        string competitionType,
        int racerCount,
        int seasonId,
        List<NewRaceView.RacerSelectionItem> racers)
    {
        PageContent.Content =
            new RaceView(
                distance,
                raceType,
                competitionType,
                racerCount,
                seasonId,
                racers);
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
        SetActiveNavigation("Racers");

        PageContent.Content =
            new RacersView();
    }

    private void RacesLog_Click(
        object sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation("RacesLog");

        PageContent.Content =
            new RacesLogView();
    }

    private void Settings_Click(
        object sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation("Settings");

        PageContent.Content =
            new SettingsView();
    }
}

