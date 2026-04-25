using System;
using System.Runtime.CompilerServices;
using Howl.Collections;

namespace Howl.Math;

/// <summary>
///     Fixed-Stride Structure-of-Arrays Vector2.
/// </summary>
public class FsSoa_Vector2{
    /// <summary>
    ///     the x-coordinate values.
    /// </summary>
    /// <remarks>
    ///     Use a <c>entryElementIndex</c> integer to access elements.
    /// </remarks>
    public float[] X;

    /// <summary>
    ///     the y-coordinate values.
    /// </summary>
    /// <remarks>
    ///     Use a <c>entryElementIndex</c> integer to access elements.
    /// </remarks>
    public float[] Y;

    /// <summary>
    ///     the append counts of all entries.
    /// </summary>
    /// <remarks>
    ///     Use a <c>entryIndex</c> integer to access elements.
    /// </remarks>
    public int[] AppendCounts;

    /// <summary>
    ///     The fixed stride of each entry.
    /// </summary>
    public int Stride;

    /// <summary>
    ///     The amount of entries this collection can hold.
    /// </summary>
    public int MaxEntries;

    /// <summary>
    ///     Whether or not this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new fixed stride soa vector instance.
    /// </summary>
    /// <param name="stride">the stride for each entry.</param>
    /// <param name="maxEntries">the maximum amount of entries this collection can store.</param>
    public FsSoa_Vector2(int stride, int maxEntries)
    {
        int dataLength = stride*maxEntries;
        X = new float[dataLength];
        Y = new float[dataLength];
        AppendCounts = new int[maxEntries];
        MaxEntries = maxEntries; 
        Stride = stride;
    }

    /// <summary>
    ///     Appends a vector to a fixed stride soa instance.
    /// </summary>
    /// <param name="soa">the fixed stride soa instance to append to.</param>
    /// <param name="entryIndex">the index of the entry in the fixed stride soa instance to append to.</param>
    /// <param name="x">the x-value to append.</param>
    /// <param name="y">the y-value to append.</param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public static void Append(FsSoa_Vector2 soa, int entryIndex, float x, float y)
    {
        // ensure that the entry slot isnt full.
        int appendCount = soa.AppendCounts[entryIndex];
        if(appendCount >= soa.Stride)
        {
            throw new IndexOutOfRangeException();
        }

        int appendIndex = entryIndex * soa.Stride + appendCount;

        // set the value.
        soa.X[appendIndex] = x;
        soa.Y[appendIndex] = y;

        // increment append index.
        soa.AppendCounts[entryIndex]++;
    }

    /// <summary>
    ///     Sets the append count to zero of an entry in a fixed stride soa instance.
    /// </summary>
    /// <param name="soa">the fixed stride soa instance that contains the entry to clear.</param>
    /// <param name="entryIndex">the index of the entry to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearEntryAppendCount(FsSoa_Vector2 soa, int entryIndex)
    {
        soa.AppendCounts[entryIndex] = 0;
    }

    /// <summary>
    ///     Sets the append count to zero of all entries in a fixed stride soa instance.
    /// </summary>
    /// <param name="soa">the soa instance to clear </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearAppendCounts(FsSoa_Vector2 soa)
    {
        for(int i = 0; i < soa.MaxEntries; i++)
        {
            soa.AppendCounts[i] = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void EnforceNil(FsSoa_Vector2 soa)
    {
        Nil.Enforce(soa.X, soa.Stride);
        Nil.Enforce(soa.Y, soa.Stride);
    }

    public static void Dispose(FsSoa_Vector2 soa)
    {
        if (soa.Disposed)
        {
            return;
        }

        soa.Disposed = true;

        soa.X = null;
        soa.Y = null;
        soa.AppendCounts = null;
        soa.MaxEntries = 0;
        soa.Stride = 0;

        GC.SuppressFinalize(soa);
    }

    ~FsSoa_Vector2()
    {
        Dispose(this);    
    }
}