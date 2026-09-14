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

            int automaticStartYear =
                SeasonService.GetAutomaticSeasonStartYear(
                    DateTime.Now);

            seasons =
                (await databaseService.GetSeasonsAsync())
                .Where(s =>
                    s.StartYear >= automaticStartYear - 5 &&
                    s.StartYear <= automaticStartYear + 5)
                .OrderBy(s => s.StartYear)
                .ToList();

            SeasonComboBox.Items.Clear();

            Season? detectedSeason =
                AppSeasonService.Instance.CurrentSeason;

            string autoText =
                detectedSeason != null
                    ? $"Auto ({detectedSeason.Name})"
                    : "Auto";

            SeasonComboBox.Items.Add(autoText);

            foreach (Season season in seasons)
            {
                SeasonComboBox.Items.Add(season.Name);
            }

            SeasonComboBox.SelectedIndex = 0;

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

        if (SeasonComboBox.SelectedIndex == 0)
        {
            Season? detectedSeason =
                AppSeasonService.Instance.CurrentSeason;

            if (detectedSeason != null)
            {
                AppSeasonService.Instance
                    .SetAutomaticSeason(detectedSeason);
            }

            UpdateSeasonInfo();

            return;
        }

        if (SeasonComboBox.SelectedItem is string seasonName)
        {
            Season? season =
                seasons.FirstOrDefault(
                    s => s.Name == seasonName);

            if (season != null)
            {
                AppSeasonService.Instance
                    .SetSeason(season);
            }

            UpdateSeasonInfo();
        }
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

        int juniorMinimum =
            season.StartYear - 17;

        int juniorMaximum =
            season.StartYear - 14;

        int youthStart =
            season.StartYear - 13;

        SeasonInfoText.Text =
            $"Senior: {seniorCutoff} and older\n" +
            $"Junior: {juniorMinimum}-{juniorMaximum}\n" +
            $"Youth: {youthStart} and younger";
    }
}
