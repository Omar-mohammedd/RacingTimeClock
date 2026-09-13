using System;
using System.Collections.Generic;

namespace RacingTimeClock.Models;

public class Race
{
    public int Id { get; set; }

    public string Distance { get; set; } = string.Empty;

    public string RaceType { get; set; } = string.Empty;

    public DateTime StartDateTime { get; set; }

    public int RacerCount { get; set; }

    public bool IsCompleted { get; set; }

    public int SeasonId { get; set; }

    public Season? Season { get; set; }

    public List<RacerResult> Results { get; set; } = new();
}
