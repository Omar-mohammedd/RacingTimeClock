using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Data;
using RacingTimeClock.Models;

namespace RacingTimeClock.Services;

public class DatabaseService
{
    public async Task InitializeAsync()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        await db.Database.EnsureCreatedAsync();

        await SeedSeasonsAsync(db);
    }

    private async Task SeedSeasonsAsync(
        RacingTimeClockDbContext db)
    {
        List<Season> existingSeasons =
            await db.Seasons.ToListAsync();

        for (int year = 2020; year <= 2035; year++)
        {
            bool exists =
                existingSeasons.Any(
                    s => s.StartYear == year);

            if (exists)
                continue;

            db.Seasons.Add(
                SeasonService.CreateSeason(year));
        }

        await db.SaveChangesAsync();
    }

    public async Task SaveRaceAsync(Race race)
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        await db.Database.EnsureCreatedAsync();

        db.Races.Add(race);

        await db.SaveChangesAsync();
    }

    public async Task<List<Race>> GetRacesAsync()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        await db.Database.EnsureCreatedAsync();

        return await db.Races
            .Include(r => r.Results)
                .ThenInclude(r => r.Racer)
            .Include(r => r.Season)
            .OrderByDescending(
                r => r.StartDateTime)
            .ToListAsync();
    }

    public async Task<List<Season>> GetSeasonsAsync()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        await db.Database.EnsureCreatedAsync();

        return await db.Seasons
            .Where(s => s.IsActive)
            .OrderByDescending(
                s => s.StartYear)
            .ToListAsync();
    }

    public async Task<Season?> GetSeasonAsync(
        int seasonId)
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        await db.Database.EnsureCreatedAsync();

        return await db.Seasons
            .FirstOrDefaultAsync(
                s => s.Id == seasonId &&
                     s.IsActive);
    }
}
