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
            DatabaseService databaseService = new();

            await databaseService.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Database initialization failed.\n\n{ex.Message}",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }
}
