
using N_Howl.N_Collections;
using N_Howl.N_Memory;

namespace Howl.Algorithms.Sorting;

public struct RadixSortBuffer
{
    /// <summary>
    ///     The uint converted values for sorting.
    /// </summary>
    public Array<uint> TranslatedValues;

    /// <summary>
    ///     The array for temporary values when reordering the translated values during each radix pass.
    /// </summary>
    public Array<uint> TempValues;

    /// <summary>
    ///     A histogram array.
    /// </summary>
    /// <remarks>
    ///     Always 256 elements long.
    /// </remarks>
    public Array<int> ByteCount;

    /// <summary>
    ///     The array for temporary indices when reordering indices alongside the values during each radix pass.
    /// </summary>
    public Array<int> TempIndices;

    public bool IsInitialised;

    public static bool Init(ref RadixSortBuffer buffer, ref MemoryArena arena, int length)
    {
        if (buffer.IsInitialised)
        {
            Debug.Panic("Aleady Initialised.");
            return false;
        }

        N_Howl.N_Collections.Collections.Init(ref buffer.TranslatedValues, ref arena, length);
        N_Howl.N_Collections.Collections.Init(ref buffer.TempValues, ref arena, length);
        N_Howl.N_Collections.Collections.Init(ref buffer.TempIndices, ref arena, length);
        N_Howl.N_Collections.Collections.Init(ref buffer.ByteCount, ref arena, 256); // count must always be 256 as radix operates on 8-bit/byte chunks.

        buffer.IsInitialised = true;
        return true;
    }
}