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
        await EnsureRaceSchemaAsync(db);

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

    private async Task EnsureRaceSchemaAsync(
        RacingTimeClockDbContext db)
    {
        DbConnection connection =
            db.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using DbCommand command =
            connection.CreateCommand();

        command.CommandText =
            "PRAGMA table_info(Races);";

        bool hasCompetitionType = false;

        await using DbDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string columnName =
                reader["name"]?.ToString() ?? string.Empty;

            if (string.Equals(
                    columnName,
                    "CompetitionType",
                    StringComparison.OrdinalIgnoreCase))
            {
                hasCompetitionType = true;
                break;
            }
        }

        await reader.CloseAsync();

        if (!hasCompetitionType)
        {
            using DbCommand alterCommand =
                connection.CreateCommand();

            alterCommand.CommandText =
                "ALTER TABLE Races " +
                "ADD COLUMN CompetitionType TEXT NOT NULL DEFAULT '';";

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

    public async Task<int> SeedTestRacersAsync(int targetCount = 500)
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        List<Racer> existing =
            await db.Racers.ToListAsync();

        int needed =
            Math.Max(0, targetCount - existing.Count);

        if (needed == 0)
            return 0;

        string[] maleFirstNames =
        {
            "Adam","Ahmed","Ali","Amr","Andrew","Anthony","Arthur","Ayman",
            "Bassel","Bilal","Carlos","Daniel","David","Ehab","Elias","Emad",
            "Fady","Faris","Felix","George","Hassan","Hatem","Ibrahim","James",
            "Karim","Khaled","Leo","Mahmoud","Malek","Marc","Marco","Martin",
            "Mazen","Michael","Mostafa","Nader","Nabil","Nicolas","Omar","Peter",
            "Rami","Ramy","Samir","Sherif","Tarek","Thomas","Wael","Youssef",
            "Ziad","Ziyad"
        };

        string[] femaleFirstNames =
        {
            "Alaa","Alice","Amal","Aya","Carla","Carmen","Catherine","Dalia",
            "Dina","Elena","Farah","Fatma","Hana","Hannah","Heba","Jana",
            "Joanna","Julia","Kareemah","Laila","Lara","Layla","Leah","Lina",
            "Mariam","Maya","Mira","Nada","Nadine","Nadia","Nour","Olivia",
            "Reem","Rita","Salma","Sara","Sarah","Sofia","Sophia","Talia",
            "Yara","Yasmine","Yasmin","Yasmeen","Zahra","Zeinab","Zoe","Mona",
            "Menna","Mariam"
        };

        string[] lastNames =
        {
            "Adel","Ali","Amer","Ashraf","Bakr","Baker","Brown","Clark",
            "Collins","Cooper","Davis","Dawoud","Elmasry","Fahmy","Farouk",
            "Fawzy","George","Gerges","Ghanem","Gomaa","Hassan","Ibrahim",
            "Ismail","Johnson","Kamel","Khalil","Maher","Mansour","Martin",
            "Mohamed","Morgan","Mostafa","Nassar","Nasser","Osman","Parker",
            "Rahman","Ramadan","Reda","Roberts","Salem","Samy","Sayed","Shawky",
            "Smith","Soliman","Taylor","Williams","Wilson","Youssef"
        };

        HashSet<string> usedIds =
            existing
                .Select(r => r.RacerId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        HashSet<string> usedNames =
            existing
                .Select(r => r.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Random random = new Random(20260924);

        int added = 0;
        int candidate = 10001;

        while (added < needed)
        {
            bool isMale = added % 2 == 0;

            string firstName =
                isMale
                    ? maleFirstNames[random.Next(maleFirstNames.Length)]
                    : femaleFirstNames[random.Next(femaleFirstNames.Length)];

            string lastName =
                lastNames[random.Next(lastNames.Length)];

            string name =
                $"{firstName} {lastName}";

            if (usedNames.Contains(name))
            {
                name =
                    $"{firstName} {lastName} {added + 1}";
            }

            int yearOfBirth =
                1980 + random.Next(0, 39);

            string racerId;

            do
            {
                racerId =
                    (10000 + candidate).ToString();

                candidate++;
            }
            while (usedIds.Contains(racerId));

            string racingNumber =
                (added + 1).ToString();

            db.Racers.Add(
                new Racer
                {
                    RacerId = racerId,
                    Name = name,
                    YearOfBirth = yearOfBirth,
                    IsMale = isMale,
                    RacingNumber = racingNumber,
                    IsActive = true
                });

            usedIds.Add(racerId);
            usedNames.Add(name);
            added++;
        }

        await db.SaveChangesAsync();

        return added;
    }
    public async Task SaveRaceAsync(Race race)
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        db.Races.Add(race);

        await db.SaveChangesAsync();
    }

    public async Task<List<Race>> GetRacesAsync()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        return await db.Races
            .Include(r => r.Results)
                .ThenInclude(rr => rr.Racer)
            .Include(r => r.Season)
            .OrderByDescending(
                r => r.StartDateTime)
            .ToListAsync();
    }

    public async Task<List<Racer>> GetRacersAsync()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

        return await db.Racers.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<List<Season>> GetSeasonsAsync()
    {
        using RacingTimeClockDbContext db =
            new RacingTimeClockDbContext();

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

        return await db.Seasons
            .FirstOrDefaultAsync(
                s => s.Id == seasonId &&
                     s.IsActive);
    }
}





