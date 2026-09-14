using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Data;
using RacingTimeClock.Models;
using System.Data.Common;

namespace RacingTimeClock.Services;

public class DatabaseService
{
    public async Task InitializeAsync()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        await db.Database.EnsureCreatedAsync();

        await EnsureRacerSchemaAsync(db);

        await SeedSeasonsAsync(db);
    }

    private async Task EnsureRacerSchemaAsync(
        RacingTimeClockDbContext db)
    {
        DbConnection connection =
            db.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using DbCommand command =
            connection.CreateCommand();

        command.CommandText =
            "PRAGMA table_info(Racers);";

        bool hasRacingNumber = false;

        await using DbDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string columnName =
                reader["name"]?.ToString() ?? string.Empty;

            if (string.Equals(
                    columnName,
                    "RacingNumber",
                    StringComparison.OrdinalIgnoreCase))
            {
                hasRacingNumber = true;
                break;
            }
        }

        await reader.CloseAsync();

        if (!hasRacingNumber)
        {
            using DbCommand alterCommand =
                connection.CreateCommand();

            alterCommand.CommandText =
                "ALTER TABLE Racers " +
                "ADD COLUMN RacingNumber TEXT NOT NULL DEFAULT '';";

            await alterCommand.ExecuteNonQueryAsync();
        }
    }

    private async Task SeedSeasonsAsync(
        RacingTimeClockDbContext db)
    {
        int automaticStartYear =
            SeasonService.GetAutomaticSeasonStartYear(
                DateTime.Now);

        int firstYear =
            automaticStartYear - 5;

        int lastYear =
            automaticStartYear + 5;

        List<Season> existingSeasons =
            await db.Seasons.ToListAsync();

        for (int year = firstYear;
             year <= lastYear;
             year++)
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
                .ThenInclude(rr => rr.Racer)
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

