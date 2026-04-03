using System;
using System.Runtime.CompilerServices;

namespace Howl.Collections;

public class SwapBackArray<T> : IDisposable
{
    /// <summary>
    /// The data of type stored by this 
    /// </summary>
    public T[] Data;

    /// <summary>
    /// The total number of allocated entries after index zero.  
    /// </summary>
    public int Count;

    /// <summary>
    /// Whether or not this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new SwapbackArray instance.
    /// </summary>
    /// <param name="length">the length of the backing array.</param>
    public SwapBackArray(int length)
    {
        Data = new T[length];
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

    /// <summary>
    /// Appends a value to a swapback array.
    /// </summary>
    /// <param name="array">the swapback array instance to append to.</param>
    /// <param name="value">the value to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(SwapBackArray<T> array, T value)
    {
        array.Data[array.Count] = value;
        array.Count++;
    }

    /// <summary>
    /// Removes an entry at a given index from a swapback array.
    /// </summary>
    /// <param name="array">the swapback array instance.</param>
    /// <param name="index">the index to remove at.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void RemoveAt(SwapBackArray<T> array, int index)
    {
        if(index >= array.Count)
        {
            throw new IndexOutOfRangeException();
        }

        // set the data to remove with the last entry.
        array.Data[index] = array.Data[array.Count-1];
        // decrement the count.
        array.Count--;
    }

    /// <summary>
    /// Sets the <c>Count</c> of a swap back array to zero.
    /// </summary>
    /// <param name="array">the swap back array instance to clear.</param>
    public static void ClearCount(SwapBackArray<T> array)
    {
        array.Count = 0;
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
    /// Appends a value to a swapback array.
    /// </summary>
    /// <param name="array">the swapback array instance to append to.</param>
    /// <param name="value">the value to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append<T>(this SwapBackArray<T> array, T value)
    {
        SwapBackArray<T>.Append(array, value);
    }

    /// <summary>
    /// Removes an entry at a given index from a swapback array.
    /// </summary>
    /// <param name="array">the swapback array instance.</param>
    /// <param name="index">the index to remove at.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void RemoveAt<T>(this SwapBackArray<T> array, int index)
    {
        SwapBackArray<T>.RemoveAt(array, index);
    }

    /// <summary>
    /// Sets the <c>Count</c> of a swap back array to zero.
    /// </summary>
    /// <param name="array">the swap back array instance to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearCount<T>(this SwapBackArray<T> array)
    {
        SwapBackArray<T>.ClearCount(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Dispose<T>(this SwapBackArray<T> array)
    {
        SwapBackArray<T>.Dispose(array);
    }
}