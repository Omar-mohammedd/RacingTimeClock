namespace RacingTimeClock.Timing;

public interface ITimingInput : IDisposable
{
    event EventHandler<TimingEvent>? TimingEventReceived;

    void Start();
    void Stop();
}
