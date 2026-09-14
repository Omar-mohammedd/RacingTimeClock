namespace RacingTimeClock.Models;

public class Season
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int StartYear { get; set; }

    // Kept for compatibility with existing SQLite databases.
    // Category calculations use StartYear instead.
    public int SeniorMinimumBirthYear { get; set; } = 2008;

    public bool IsActive { get; set; } = true;
}

