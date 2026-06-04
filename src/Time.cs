using System.Runtime.CompilerServices;

namespace Howl;

public static class Time
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static long GetSystemTick()
    {
        return System.Diagnostics.Stopwatch.GetTimestamp();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static double ElapsedMilliseconds(long startTick, long endTick)
    {
        System.TimeSpan elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(startTick, endTick);
        return elapsed.TotalMilliseconds;
    }
}