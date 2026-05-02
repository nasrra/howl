using System;
using System.Runtime.CompilerServices;

namespace Howl.Collections;

public class SwapBackArray<T> : IDisposable
{
    /// <summary>
    ///     The data of type stored by this 
    /// </summary>
    public T[] Data;

    /// <summary>
    ///     The total number of allocated entries after index zero.  
    /// </summary>
    public int Count;

    /// <summary>
    ///     Gets the total number of elements in all the dimensions of the Array.
    /// </summary>
    public int Length;

    /// <summary>
    ///     Whether or not this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new SwapbackArray instance.
    /// </summary>
    /// <param name="length">the length of the backing array.</param>
    public SwapBackArray(int length)
    {
        Data = new T[length];
        Length = length;
    }

    public T this[int index]
    {
        get
        {
            return Data[index];
        }
        set
        {
            Data[index] = value;
        }
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Dispose(SwapBackArray<T> array)
    {
        if (array.Disposed)
        {
            return;
        } 

        array.Disposed = true;

        array.Data = null;

        array.Count = 0;

        array.Length = 0;

        GC.SuppressFinalize(array);
    }

    ~SwapBackArray()
    {
        Dispose(this);
    }
}

public static class SwapBackArray
{
    /// <summary>
    ///     Appends a value to a swapback array.
    /// </summary>
    /// <param name="array">the swapback array instance to append to.</param>
    /// <param name="value">the value to append.</param>
    /// <returns>the index the value was written to in the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int Append<T>(this SwapBackArray<T> array, T value)
    {
        array.Data[array.Count] = value;
        return array.Count++;
    }

    /// <summary>
    ///     Removes an entry at a given index from a swapback array.
    /// </summary>
    /// <param name="array">the swapback array instance.</param>
    /// <param name="index">the index to remove at.</param>
    /// <returns>the index of the value that was swapped with the value that was removed.</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int RemoveAt<T>(this SwapBackArray<T> array, int index)
    {
        if(index >= array.Count)
        {
            throw new IndexOutOfRangeException();
        }

        // decrement the count.
        array.Count--;
        
        // set the data to remove with the last entry.
        array.Data[index] = array.Data[array.Count];
    
        return array.Count;
    }

    /// <summary>
    ///     Sets the <c>Count</c> of a swap back array to zero.
    /// </summary>
    /// <param name="array">the swap back array instance to clear.</param>
    public static void ClearCount<T>(this SwapBackArray<T> array)
    {
        array.Count = 0;
    }

    /// <summary>
    ///     Gets the underlying array of a swapback array as a span.
    /// </summary>
    /// <param name="array">the swapback array instance to get as a span.</param>
    /// <returns>The span of the underlying array.</returns>
    public static Span<T> AsSpan<T>(this SwapBackArray<T> array)
    {
        return array.Data;
    }

    /// <summary>
    ///     Gets a span slice of a swapback array's underlying array.
    /// </summary>
    /// <param name="array">the swapback array to get a slice of.</param>
    /// <param name="start">The zero-based index at which to begin this slice.</param>
    /// <param name="length">The desired length for the slice (exclusive).</param>
    /// <returns></returns>
    public static Span<T> Slice<T>(this SwapBackArray<T> array, int start, int length)
    {
        return AsSpan(array).Slice(start, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Dispose<T>(this SwapBackArray<T> array)
    {
        SwapBackArray<T>.Dispose(array);
    }
}