namespace RacingTimeClock.Timing;

public sealed record TimingEvent(
    TimingEventType Type,
    int RacerNumber = 0,
    DateTime? OccurredAt = null);
