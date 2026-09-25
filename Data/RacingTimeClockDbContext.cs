using Microsoft.EntityFrameworkCore;
using RacingTimeClock.Models;
using System;
using System.IO;

namespace RacingTimeClock.Data;

public class RacingTimeClockDbContext : DbContext
{
    public DbSet<Race> Races => Set<Race>();

    public DbSet<RacerResult> RacerResults => Set<RacerResult>();

    public DbSet<Racer> Racers => Set<Racer>();

    public DbSet<Season> Seasons => Set<Season>();

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        string dataDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data");

        Directory.CreateDirectory(dataDirectory);

        string databasePath = Path.Combine(
            dataDirectory,
            "RacingTimeClock.db");

        optionsBuilder.UseSqlite(
            $"Data Source={databasePath}");
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Race>()
            .HasMany(r => r.Results)
            .WithOne(r => r.Race)
            .HasForeignKey(r => r.RaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RacerResult>()
            .HasOne(r => r.Racer)
            .WithMany()
            .HasForeignKey(r => r.RacerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Race>()
            .HasOne(r => r.Season)
            .WithMany()
            .HasForeignKey(r => r.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RacerResult>()
            .Property(r => r.FinishTime)
            .HasConversion(
                time => time.Ticks,
                ticks => TimeSpan.FromTicks(ticks));

        modelBuilder.Entity<Racer>()
            .Property(r => r.FirstName)
            .HasComputedColumnSql(
                "CASE WHEN instr(trim(Name), ' ') > 0 THEN substr(trim(Name), 1, instr(trim(Name), ' ') - 1) ELSE trim(Name) END",
                stored: false)
            .ValueGeneratedOnAddOrUpdate();
    }
}
