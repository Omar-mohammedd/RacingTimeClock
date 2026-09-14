using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RacingTimeClock.Models;
using RacingTimeClock.Services;

namespace RacingTimeClock.Views;

public partial class SettingsView : UserControl
{
    private List<Season> seasons = new();

    private bool loading;

    public SettingsView()
    {
        InitializeComponent();

        Loaded += SettingsView_Loaded;
    }

    private async void SettingsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadSeasonsAsync();
    }

    private async Task LoadSeasonsAsync()
    {
        try
        {
            loading = true;

            DatabaseService databaseService =
                new DatabaseService();

            seasons =
                await databaseService.GetSeasonsAsync();

            SeasonComboBox.ItemsSource =
                seasons;

            SeasonComboBox.DisplayMemberPath =
                "Name";

            Season? currentSeason =
                AppSeasonService.Instance.CurrentSeason;

            if (currentSeason != null)
            {
                Season? matchingSeason =
                    seasons.FirstOrDefault(
                        s => s.Id == currentSeason.Id);

                if (matchingSeason != null)
                {
                    SeasonComboBox.SelectedItem =
                        matchingSeason;
                }
            }

            UpdateSeasonInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load seasons.\n\n{ex}",
                "Settings Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            loading = false;
        }
    }

    private void SeasonComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (loading)
            return;

        if (SeasonComboBox.SelectedItem
            is not Season season)
        {
            return;
        }

        AppSeasonService.Instance
            .SetSeason(season);

        UpdateSeasonInfo();
    }

    private void UpdateSeasonInfo()
    {
        Season? season =
            AppSeasonService.Instance.CurrentSeason;

        if (season == null)
        {
            SeasonInfoText.Text =
                "No season selected.";

            return;
        }

        int seniorCutoff =
            season.StartYear - 18;

        int youthStart =
            season.StartYear - 14;

        int juniorStart =
            seniorCutoff + 1;

        int juniorEnd =
            youthStart - 1;

        SeasonInfoText.Text =
            $"Senior: {seniorCutoff} and older\n" +
            $"Junior: {juniorStart}-{juniorEnd}\n" +
            $"Youth: {youthStart} and younger";
    }
}
