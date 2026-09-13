using System;

namespace RacingTimeClock.Models;

public class RacerResult
{
    public int Id { get; set; }

    public int RaceId { get; set; }

    public int? RacerId { get; set; }

    public int RacerNumber { get; set; }

    public int Position { get; set; }

    public TimeSpan FinishTime { get; set; }

    public long FinishTimestamp { get; set; }

    public Race? Race { get; set; }

    public Racer? Racer { get; set; }
}

