using System.Windows;
using RacingTimeClock.Data;

namespace RacingTimeClock;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using var db = new RacingTimeClockDbContext();
        db.Database.EnsureCreated();
    }
}
