using System;
using Howl.Collections;
using Howl.ECS;

public class GenIndexArray<T> : IDisposable
{
    public T[] Data;
    public int[] Flags;
    public int[] Generations;
    public bool[] Allocated;
    public SwapBackArray<int> FreeSlots;
    public bool Disposed = false;

    /// <summary>
    /// Creates a new GenIndexArray instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public GenIndexArray(int length){
        Data        = new T[length];
        Flags       = new int[length];
        Generations = new int[length];
        Allocated   = new bool[length];
        FreeSlots   = new (length);
        FreeSlots.Append(1); // append entry 1 as the next free slot available; not zero as zero is Nil.
    }

    public static void Allocate(GenIndexArray<T> array, ref T data, ref GenIndex genIndex)
    {
    }

    public static void Deallocate(GenIndex genIndex)
    {
        
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(GenIndexArray<T> array)
    {
        if (array.Disposed)
        {
            return;
        }

        array.Disposed = true;

        array.Data = null;
        
        array.Flags = null;
        
        array.Generations = null;
        
        array.Allocated = null;

        SwapBackArray.Dispose(array.FreeSlots);
        array.FreeSlots = null;

        GC.SuppressFinalize(array);
    }

    ~GenIndexArray()
    {
        Dispose(this);
    }
}