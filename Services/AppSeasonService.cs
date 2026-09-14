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
    private bool isAuto;

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

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    nameof(CurrentSeasonDisplay)));
        }
    }

    public bool IsAuto
    {
        get => isAuto;
        private set
        {
            isAuto = value;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    nameof(IsAuto)));
        }
    }

    public string CurrentSeasonDisplay =>
        CurrentSeason?.Name ?? string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    private AppSeasonService()
    {
    }

    public void SetSeason(Season season)
    {
        IsAuto = false;
        CurrentSeason = season;
    }

    public void SetAutomaticSeason(Season season)
    {
        IsAuto = true;
        CurrentSeason = season;
    }

    public bool HasSeason =>
        CurrentSeason != null;
}
