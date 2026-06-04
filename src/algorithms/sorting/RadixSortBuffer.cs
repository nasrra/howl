
using Howl.Unmanaged.Collections;

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

    public static bool Initialise(ref RadixSortBuffer buffer, ref Memory.Arena arena, int length)
    {
        if (buffer.IsInitialised)
        {
            Debug.Panic("Aleady Initialised.");
            return false;
        }

        Array.Initialise(ref buffer.TranslatedValues, ref arena, length);
        Array.Initialise(ref buffer.TempValues, ref arena, length);
        Array.Initialise(ref buffer.TempIndices, ref arena, length);
        Array.Initialise(ref buffer.ByteCount, ref arena, 256); // count must always be 256 as radix operates on 8-bit/byte chunks.

        buffer.IsInitialised = true;
        return true;
    }
}