using System.Windows.Input;

namespace RacingTimeClock.Timing;

public sealed class KeyboardTimingInput : ITimingInput
{
    public event EventHandler<TimingEvent>? TimingEventReceived;

    public void Start()
    {
    }

    public void Stop()
    {
    }

    public void ProcessKey(Key key)
    {
        if (key == Key.Space)
        {
            Raise(TimingEventType.Start);
            return;
        }

        int racerNumber = key switch
        {
            Key.D1 or Key.NumPad1 => 1,
            Key.D2 or Key.NumPad2 => 2,
            Key.D3 or Key.NumPad3 => 3,
            Key.D4 or Key.NumPad4 => 4,
            Key.D5 or Key.NumPad5 => 5,
            Key.D6 or Key.NumPad6 => 6,
            Key.D7 or Key.NumPad7 => 7,
            Key.D8 or Key.NumPad8 => 8,
            Key.D9 or Key.NumPad9 => 9,
            _ => 0
        };

        if (racerNumber > 0)
            Raise(TimingEventType.Finish, racerNumber);
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
