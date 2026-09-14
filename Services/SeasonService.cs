using RacingTimeClock.Models;

namespace RacingTimeClock.Services;

public static class SeasonService
{
    public static Season CreateSeason(int startYear)
    {
        return new Season
        {
            Name = $"{startYear}/{(startYear + 1) % 100:00}",
            StartYear = startYear,
            IsActive = true
        };
    }

    public static string GetCategory(
        Racer racer,
        Season season)
    {
        int seniorCutoff =
            season.StartYear - 18;

        int juniorMinimum =
            season.StartYear - 17;

        int juniorMaximum =
            season.StartYear - 14;

        if (racer.YearOfBirth <= seniorCutoff)
            return "Senior";

        if (racer.YearOfBirth >= juniorMinimum &&
            racer.YearOfBirth <= juniorMaximum)
            return "Junior";

        return "Youth";
    }

    public static bool IsSenior(
        Racer racer,
        Season season)
        => GetCategory(racer, season) == "Senior";

    public static bool IsJunior(
        Racer racer,
        Season season)
        => GetCategory(racer, season) == "Junior";

    public static bool IsYouth(
        Racer racer,
        Season season)
        => GetCategory(racer, season) == "Youth";

    public static int GetAutomaticSeasonStartYear(
        DateTime date)
    {
        return date.Month >= 8
            ? date.Year
            : date.Year - 1;
    }

    public static Season GetAutomaticallyDetectedSeason(
        IEnumerable<Season> seasons,
        DateTime date)
    {
        int startYear =
            GetAutomaticSeasonStartYear(date);

        Season? exactSeason =
            seasons.FirstOrDefault(
                s => s.StartYear == startYear);

        return exactSeason
            ?? CreateSeason(startYear);
    }
}
