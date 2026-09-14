using System.Windows;
using RacingTimeClock.Services;

namespace RacingTimeClock;

public partial class App : Application
{
    protected override async void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            DatabaseService databaseService =
                new DatabaseService();

            await databaseService.InitializeAsync();

            List<Models.Season> seasons =
                await databaseService.GetSeasonsAsync();

            Models.Season detectedSeason =
                SeasonService
                    .GetAutomaticallyDetectedSeason(
                        seasons,
                        DateTime.Now);

            AppSeasonService.Instance
                .SetSeason(detectedSeason);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not initialize the application.\n\n{ex}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }
}
