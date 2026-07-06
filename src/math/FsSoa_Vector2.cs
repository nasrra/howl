using System.Runtime.CompilerServices;
using N_Howl.N_Collections;
using N_Howl.N_Memory;

namespace Howl.Math;

/// <summary>
///     Fixed-Stride Structure-of-Arrays Vector2.
/// </summary>
public struct FsSoa_Vector2{
    
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>Elements are accessed via <c>entryElementIndex</c>.</para>
    /// </remarks>
    public Array<float> X;

    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>Elements are accessed via <c>entryElementIndex</c>.</para>
    /// </remarks>
    public Array<float> Y;

    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>Elements are accessed via <c>entryIndex</c>.</para>
    /// </remarks>
    public Array<int> AppendCounts;

    /// <summary>
    ///     The fixed stride of each entry.
    /// </summary>
    public int EntryStride;

    /// <summary>
    ///     The amount of entries this collection can hold.
    /// </summary>
    public int MaxEntries;

    public bool IsIntialised;

    public static void Initialise(ref FsSoa_Vector2 soa, ref MemoryArena arena, int entryStride, int maxEntries)
    {
        Debug.Assert(soa.IsIntialised==false, "Already Initialised.");
        soa.IsIntialised = true;
        int dataLength = entryStride*maxEntries;
        N_Howl.N_Collections.Collections.Init(ref soa.X, ref arena, dataLength);
        N_Howl.N_Collections.Collections.Init(ref soa.Y, ref arena, dataLength);
        N_Howl.N_Collections.Collections.Init(ref soa.AppendCounts, ref arena, dataLength);
        soa.EntryStride = entryStride;
        soa.MaxEntries = maxEntries;
    }

    /// <summary>
    ///     Appends a vector to a fixed stride soa instance.
    /// </summary>
    /// <returns>
    ///     true, if successfully appended; otherwise false.
    /// </returns>
    public static bool Append(ref FsSoa_Vector2 soa, int entryIndex, float x, float y)
    {
        // ensure that the entry slot isnt full.
        int appendCount = soa.AppendCounts[entryIndex];
        if(appendCount >= soa.EntryStride)
        {
            Debug.Assert(true, "Index Out of Range.");
            return false;
        }
        int appendIndex = entryIndex * soa.EntryStride + appendCount;

        // set the value.
        soa.X[appendIndex] = x;
        soa.Y[appendIndex] = y;

        // increment append index.
        soa.AppendCounts[entryIndex]++;
        return true;
    }

    /// <summary>
    ///     Sets the append count of an entry to zero in a fixed stride soa instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearEntryAppendCount(ref FsSoa_Vector2 soa, int entryIndex)
    {
        soa.AppendCounts[entryIndex] = 0;
    }

    /// <summary>
    ///     Sets the append count to zero of all entries in a fixed stride soa instance.
    /// </summary>
    /// <param name="soa">the soa instance to clear </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearAppendCounts(ref FsSoa_Vector2 soa)
    {
        for(int i = 0; i < soa.MaxEntries; i++)
        {
            soa.AppendCounts[i] = 0;
        }
    }
}