using System;
using System.Runtime.CompilerServices;

public class StackArray<T> : IDisposable
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
    public StackArray(int length)
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
    /// Pushes a value to the top of a stack array.
    /// </summary>
    /// <param name="array">the stack array instance to push to.</param>
    /// <param name="value">the value to push.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Push(StackArray<T> array, T value)
    {
        array.Data[array.Count] = value;
        array.Count++;
    }

    /// <summary>
    /// Removes and returns the item at the top of the stack.
    /// </summary>
    /// <param name="array">The stack array instance to pop from.</param>
    /// <returns>The element removed from the top of the stack.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Pop(StackArray<T> array)
    {
        // decrement the count.
        array.Count--;
        // return the now deallocated entry.
        return array.Data[array.Count];
    }

    /// <summary>
    /// Sets the <c>Count</c> of a stack array to zero.
    /// </summary>
    /// <param name="array">the stack array instance to clear.</param>
    public static void ClearCount(StackArray<T> array)
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

    public static void Dispose(StackArray<T> array)
    {
        if (array.Disposed)
        {
            return;
        }

        array.Disposed = true;

        array.Data = null;

        array.Count = 0;

        GC.SuppressFinalize(array);
    }

    ~StackArray()
    {
        Dispose(this);
    }
}

public static class StackArray
{
    /// <summary>
    /// Pushes a value to the top of a stack array.
    /// </summary>
    /// <param name="array">the stack array instance to push to.</param>
    /// <param name="value">the value to push.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Push<T>(this StackArray<T> array, T value)
    {
        StackArray<T>.Push(array, value);
    }

    /// <summary>
    /// Removes and returns the item at the top of the stack.
    /// </summary>
    /// <param name="array">The stack array instance to pop from.</param>
    /// <returns>The element removed from the top of the stack.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Pop<T>(this StackArray<T> array)
    {
        return StackArray<T>.Pop(array);
    }

    /// <summary>
    /// Sets the <c>Count</c> of a stack array to zero.
    /// </summary>
    /// <param name="array">the stack array instance to clear.</param>
    public static void ClearCount<T>(this StackArray<T> array)
    {
        StackArray<T>.ClearCount(array);
    }

    public static void Dispose<T>(this StackArray<T> array)
    {
        StackArray<T>.Dispose(array);
    }
}