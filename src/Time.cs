using System.Runtime.CompilerServices;

namespace Howl;

public static class Time
{
    public readonly static long AppStartTick = GetSystemTick();

    /// <summary>
    ///     Gets the current tick; relative to operating system startup.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static long GetSystemTick()
    {
        return System.Diagnostics.Stopwatch.GetTimestamp();
    }

    /// <summary>
    ///     Gets the current tick; relative to process startup.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static long GetProcessTick(){
        return GetSystemTick() - AppStartTick; 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static double ElapsedMilliseconds(
        long startTick, long endTick
    ){
        System.TimeSpan elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(startTick, endTick);
        return elapsed.TotalMilliseconds;
    }
}