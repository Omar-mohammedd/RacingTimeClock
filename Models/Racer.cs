namespace RacingTimeClock.Models;

public class Racer
{
    public int Id { get; set; }

    public string RacerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int YearOfBirth { get; set; }

    public bool IsMale { get; set; }

    public string RacingNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<RacerResult> Results { get; set; } = new();
}
