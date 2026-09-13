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

        await db.Database.ExecuteSqlRawAsync("""
            PRAGMA foreign_keys = OFF;

            ALTER TABLE RacerResults
            RENAME TO RacerResults_Old;

            CREATE TABLE RacerResults (
                Id INTEGER NOT NULL CONSTRAINT PK_RacerResults PRIMARY KEY AUTOINCREMENT,
                RaceId INTEGER NOT NULL,
                RacerId INTEGER NULL,
                RacerNumber INTEGER NOT NULL,
                Position INTEGER NOT NULL,
                FinishTime INTEGER NOT NULL,
                FinishTimestamp INTEGER NOT NULL,
                CONSTRAINT FK_RacerResults_Races_RaceId
                    FOREIGN KEY (RaceId) REFERENCES Races (Id) ON DELETE CASCADE,
                CONSTRAINT FK_RacerResults_Racers_RacerId
                    FOREIGN KEY (RacerId) REFERENCES Racers (Id) ON DELETE RESTRICT
            );

            INSERT INTO RacerResults
                (Id, RaceId, RacerId, RacerNumber, Position, FinishTime, FinishTimestamp)
            SELECT
                Id, RaceId, RacerId, RacerNumber, Position, FinishTime, FinishTimestamp
            FROM RacerResults_Old;

            DROP TABLE RacerResults_Old;

            PRAGMA foreign_keys = ON;
        """);

        if (!await db.Seasons.AnyAsync())
        {
            db.Seasons.Add(
                new Season
                {
                    Name = "2026 Season",
                    SeniorMinimumBirthYear = 0,
                    IsActive = true
                });

            await db.SaveChangesAsync();
        }
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
            .OrderByDescending(r => r.StartDateTime)
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

