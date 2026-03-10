using System;

namespace Howl.Collections;

public class Buffer<T>
{
    /// <summary>
    /// Gets and sets the stored data.
    /// </summary>
    public T[] Data;

    /// <summary>
    /// Gets and sets the count of valid entries - from index zero - in the data array.
    /// </summary>
    public int Count;

    /// <summary>
    /// Creates a new buffer instance.
    /// </summary>
    /// <param name="capacity">the capacity of the backing array.</param>
    public Buffer(int capacity)
    {
        Data = new T[capacity];
    }

    /// <summary>
    /// Appends a value to a buffer.
    /// </summary>
    /// <remarks>
    /// Note: the reference value 'count' will be incremented by 1 after appending.
    /// </remarks>
    /// <param name="buffer">the buffer to append to.</param>
    /// <param name="value">the value to append into the buffer.</param>
    public static void Append(Buffer<T> buffer, T value)
    {
        buffer.Data[buffer.Count] = value;
        buffer.Count++;
    }

    /// <summary>
    /// Clears the buffer by zeroing the count value.
    /// </summary>
    /// <param name="buffer">the buffer to clear.</param>
    public static void Clear(Buffer<T> buffer)
    {
        buffer.Count = 0;
    }
}

public static class Buffer
{
    /// <summary>
    /// Clears the buffer by zeroing the count value.
    /// </summary>
    /// <param name="buffer">the buffer to clear.</param>
    public static void Clear<T>(Buffer<T> buffer)
    {
        buffer.Count = 0;
    }    
}