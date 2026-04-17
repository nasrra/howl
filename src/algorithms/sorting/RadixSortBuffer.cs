using System;

namespace Howl.Algorithms.Sorting;

public class RadixSortBuffer : IDisposable
{
    /// <summary>
    /// The uint converted values for sorting.
    /// </summary>
    public uint[] TranslatedValues;

    /// <summary>
    /// The array for temporary values when reordering the translated values during each radix pass.
    /// </summary>
    public uint[] TempValues;

    /// <summary>
    /// A histogram array.
    /// </summary>
    /// <remarks>
    /// Always 256 elements long.
    /// </remarks>
    public int[] ByteCount;

    /// <summary>
    /// The array for temporary indices when reordering indices alongside the values during each radix pass.
    /// </summary>
    public int[] TempIndices;

    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new radix sort buffer.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public RadixSortBuffer(int length)
    {
        TranslatedValues = new uint[length];
        TempValues = new uint[length];
        TempIndices = new int[length];
        ByteCount = new int[256]; // count must always be 256 as radix operates on 8-bit/byte chunks.
    }



    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(RadixSortBuffer buffer)
    {
        if (buffer.Disposed)
        {
            return;
        }

        buffer.Disposed = true;
        buffer.TranslatedValues = null;
        buffer.TempValues = null;
        buffer.ByteCount = null;
        buffer.TempIndices = null;

        GC.SuppressFinalize(buffer);
    }

    ~RadixSortBuffer()
    {
        Dispose(this);
    }
}