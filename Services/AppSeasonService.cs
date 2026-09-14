using System.ComponentModel;
using RacingTimeClock.Models;

namespace RacingTimeClock.Services;

public class AppSeasonService : INotifyPropertyChanged
{
    private static readonly Lazy<AppSeasonService> instance =
        new(() => new AppSeasonService());

    public static AppSeasonService Instance =>
        instance.Value;

    private Season? currentSeason;

    public Season? CurrentSeason
    {
        get => currentSeason;

        private set
        {
            currentSeason = value;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    nameof(CurrentSeason)));
        }
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    private AppSeasonService()
    {
    }

    public void SetSeason(Season season)
    {
        CurrentSeason = season;
    }

    public bool HasSeason =>
        CurrentSeason != null;
}
