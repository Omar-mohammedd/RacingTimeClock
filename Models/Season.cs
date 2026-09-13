namespace RacingTimeClock.Models;

public class Season
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SeniorMinimumBirthYear { get; set; }

    public bool IsActive { get; set; } = true;
}
