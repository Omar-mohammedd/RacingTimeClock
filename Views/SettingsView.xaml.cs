using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RacingTimeClock.Models;
using RacingTimeClock.Services;
using RacingThemeMode = RacingTimeClock.Services.ThemeMode;

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
        LoadTheme();
        LoadAccent();

        await LoadSeasonsAsync();
    }

    private void LoadTheme()
    {
        loading = true;

        ThemeComboBox.SelectedIndex =
            ThemeService.CurrentMode switch
            {
                RacingThemeMode.Light => 0,
                RacingThemeMode.Dark => 1,
                _ => 2
            };

        loading = false;
    }

    private void LoadAccent()
    {
        loading = true;

        AccentComboBox.SelectedIndex =
            ThemeService.CurrentAccent switch
            {
                AccentColor.Red => 0,
                AccentColor.Blue => 1,
                AccentColor.Green => 2,
                AccentColor.Purple => 3,
                AccentColor.Orange => 4,
                _ => 0
            };

        loading = false;
    }

    private void AccentComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (loading)
            return;

        AccentColor accent =
            AccentComboBox.SelectedIndex switch
            {
                1 => AccentColor.Blue,
                2 => AccentColor.Green,
                3 => AccentColor.Purple,
                4 => AccentColor.Orange,
                _ => AccentColor.Red
            };

        ThemeService.SetAccent(accent);
    }

    private void ThemeComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (loading)
            return;

        RacingThemeMode mode =
            ThemeComboBox.SelectedIndex switch
            {
                0 => RacingThemeMode.Light,
                1 => RacingThemeMode.Dark,
                _ => RacingThemeMode.System
            };

        ThemeService.Apply(mode);
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

            Season detectedSeason =
                SeasonService.GetAutomaticallyDetectedSeason(
                    seasons,
                    DateTime.Now);

            SeasonComboBox.Items.Clear();

            SeasonComboBox.Items.Add(
                $"Auto ({detectedSeason.Name})");

            foreach (Season season in seasons)
                SeasonComboBox.Items.Add(season.Name);

            if (AppSeasonService.Instance.IsAuto)
            {
                SeasonComboBox.SelectedIndex = 0;
            }
            else
            {
                Season? currentSeason =
                    AppSeasonService.Instance.CurrentSeason;

                int index =
                    currentSeason == null
                        ? -1
                        : seasons.FindIndex(
                            s => s.Id == currentSeason.Id);

                SeasonComboBox.SelectedIndex =
                    index >= 0
                        ? index + 1
                        : 0;
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

        if (SeasonComboBox.SelectedIndex == 0)
        {
            Season detectedSeason =
                SeasonService.GetAutomaticallyDetectedSeason(
                    seasons,
                    DateTime.Now);

            AppSeasonService.Instance
                .SetAutomaticSeason(detectedSeason);

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

