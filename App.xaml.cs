using System;
using System.Windows;
using RacingTimeClock.Services;

namespace RacingTimeClock;

public partial class App : Application
{
    private async Task SeedTestRacersAndExitAsync()
    {
        DatabaseService databaseService =
            new DatabaseService();

        await databaseService.InitializeAsync();

        int added =
            await databaseService.SeedTestRacersAsync(500);

        RacingTimeClock.Services.RacingPopupService.Show(
            $"Added {added} test racers.",
            "Test Racers",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    protected override async void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Contains(
                "--seed-test-racers",
                StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                await SeedTestRacersAndExitAsync();
            }
            catch (Exception ex)
            {
                RacingTimeClock.Services.RacingPopupService.Show(
                    ex.ToString(),
                    "Test Racer Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            Shutdown();
            return;
        }

        ThemeService.Initialize();

        try
        {
            DatabaseService databaseService =
                new DatabaseService();

            await databaseService.InitializeAsync();

            List<Models.Season> seasons =
                await databaseService.GetSeasonsAsync();

            Models.Season detectedSeason =
                SeasonService.GetAutomaticallyDetectedSeason(
                    seasons,
                    DateTime.Now);

            AppSeasonService.Instance
                .SetAutomaticSeason(detectedSeason);

            MainWindow mainWindow =
                new MainWindow();

            MainWindow = mainWindow;

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            RacingTimeClock.Services.RacingPopupService.Show(
                $"Could not initialize the application.\n\n{ex}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }
}


