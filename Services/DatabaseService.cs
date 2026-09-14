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
                .ThenInclude(rr => rr.Racer)
            .OrderByDescending(r => r.StartDateTime)
            .ToListAsync();
    }

    public async Task<List<Season>> GetSeasonsAsync()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        await db.Database.EnsureCreatedAsync();

        return await db.Seasons
            .OrderBy(s => s.StartYear)
            .ToListAsync();
    }

    public async Task<Season?> GetActiveSeasonAsync()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        await db.Database.EnsureCreatedAsync();

        return await db.Seasons
            .FirstOrDefaultAsync(s => s.IsActive);
    }
}
