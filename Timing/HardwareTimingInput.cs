namespace RacingTimeClock.Timing;

public sealed class HardwareTimingInput : ITimingInput
{
    public event EventHandler<TimingEvent>? TimingEventReceived;

    public void Start()
    {
    }

    public void Stop()
    {
    }

    public void TriggerStart()
    {
        Raise(TimingEventType.Start);
    }

    public void TriggerFinish(int racerNumber)
    {
        if (racerNumber < 1 || racerNumber > 9)
            return;

        Raise(TimingEventType.Finish, racerNumber);
    }

    public void TriggerLap(int racerNumber)
    {
        if (racerNumber < 1 || racerNumber > 9)
            return;

        Raise(TimingEventType.Lap, racerNumber);
    }

    public void TriggerFalseStart()
    {
        Raise(TimingEventType.FalseStart);
    }

    public void Dispose()
    {
        Stop();
    }

    private void Raise(
        TimingEventType type,
        int racerNumber = 0)
    {
        TimingEventReceived?.Invoke(
            this,
            new TimingEvent(
                type,
                racerNumber,
                DateTime.Now));
    }
}
