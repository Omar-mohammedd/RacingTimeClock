using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RacingTimeClock.Timing;

public class TimingService
{
    private long startTimestamp;

    private readonly Dictionary<int, TimeSpan> finishTimes = new();

    private readonly Dictionary<int, long> finishTimestamps = new();

    public bool IsRunning { get; private set; }

    public bool IsCompleted { get; private set; }

    public DateTime StartDateTime { get; private set; }

    public long StartTimestamp => startTimestamp;

    public long StopwatchFrequency => Stopwatch.Frequency;

    public void Start()
    {
        if (IsRunning || IsCompleted)
            return;

        finishTimes.Clear();
        finishTimestamps.Clear();

        StartDateTime = DateTime.Now;

        startTimestamp = Stopwatch.GetTimestamp();

        IsRunning = true;
    }

    public TimeSpan GetElapsedTime()
    {
        if (!IsRunning && !IsCompleted)
            return TimeSpan.Zero;

        long currentTimestamp = Stopwatch.GetTimestamp();

        return CalculateElapsedTime(currentTimestamp);
    }

    public TimeSpan? FinishRacer(int racerNumber)
    {
        if (!IsRunning)
            return null;

        if (finishTimes.ContainsKey(racerNumber))
            return null;

        long finishTimestamp = Stopwatch.GetTimestamp();

        TimeSpan finishTime =
            CalculateElapsedTime(finishTimestamp);

        finishTimes[racerNumber] = finishTime;
        finishTimestamps[racerNumber] = finishTimestamp;

        return finishTime;
    }

    public long? GetFinishTimestamp(int racerNumber)
    {
        if (finishTimestamps.TryGetValue(
                racerNumber,
                out long timestamp))
        {
            return timestamp;
        }

        return null;
    }

    public bool HasFinished(int racerNumber)
    {
        return finishTimes.ContainsKey(racerNumber);
    }

    public bool AllRacersFinished(int racerCount)
    {
        return finishTimes.Count >= racerCount;
    }

    public IReadOnlyDictionary<int, TimeSpan> GetResults()
    {
        return finishTimes;
    }

    public IReadOnlyDictionary<int, long> GetFinishTimestamps()
    {
        return finishTimestamps;
    }

    public void Complete()
    {
        if (!IsRunning)
            return;

        IsRunning = false;
        IsCompleted = true;
    }

    private TimeSpan CalculateElapsedTime(long timestamp)
    {
        long elapsedTicks =
            timestamp - startTimestamp;

        double seconds =
            (double)elapsedTicks / Stopwatch.Frequency;

        return TimeSpan.FromSeconds(seconds);
    }
}
