using System;

namespace Howl.ECS; 

public class GenIndexArray<T> : IDisposable
{   
    /// <summary>
    ///     The backing storage for actual elements.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             Index 0 is reserved as a <c>Nil</c> sentinel and should not be used for data.
    ///         </item>
    ///         <item>
    ///             This is a parallel array associated with <c>Flags</c>, <c>Generations</c>, and <c>Allocated</c> by index.
    ///         </item>
    ///     </list>
    /// </remarks>
    public T[] Data;

    /// <summary>
    ///     The user-defined flags for each element stored in this collection.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             Index 0 is reserved as a <c>Nil</c> sentinel and should not be used for data.
    ///         </item>
    ///         <item>
    ///             This is a parallel array associated with <c>Data</c>, <c>Generations</c>, and <c>Allocated</c> by index.
    ///         </item>
    ///     </list>
    /// </remarks>
    public int[] Flags;

    /// <summary>
    ///     The generational value for each element stored in this collection.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             Index 0 is reserved as a <c>Nil</c> sentinel and should not be used for data.
    ///         </item>
    ///         <item>
    ///             This is a parallel array associated with <c>Data</c>, <c>Flags</c>, and <c>Allocated</c> by index.
    ///         </item>
    ///     </list>
    /// </remarks>
    public int[] Generations;

    /// <summary>
    ///     Whether or not an element in the collection is valid (has been allocated/is in use). 
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             Index 0 is reserved as a <c>Nil</c> sentinel and should not be used for data.
    ///         </item>
    ///         <item>
    ///             This is a parallel array associated with <c>Data</c>, <c>Flags</c>, and <c>Generations</c> by index.
    ///         </item>
    ///     </list>
    /// </remarks>
    public bool[] Allocated;

    /// <summary>
    ///     A the available slots in this collection that can be allocated to.
    /// </summary>
    public StackArray<int> FreeSlots;

    /// <summary>
    ///     The count of slots that have been allocated to.
    /// </summary>
    public int Count;
    
    /// <summary>
    ///     whether or not this instance has been disposed of.
    /// </summary>
    public bool Disposed = false;

    /// <summary>
    ///     Creates a new GenIndexArray instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public GenIndexArray(int length){
        System.Diagnostics.Debug.Assert(length >= 2, "GenIndexArray length must be 2!");
        length = Math.Math.Clamp(length, 2, int.MaxValue);
        
        Data        = new T[length];
        Flags       = new int[length];
        Generations = new int[length];
        Allocated   = new bool[length];
        FreeSlots   = new(length);
        Count = 0;

        // append entry 1 as the next free slot available; not zero as zero is Nil.
        StackArray.Push(FreeSlots, 1);
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
    ///     Allocates data into the next available slot for allocation a gen index array.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="data">the data to allocate.</param>
    /// <param name="genIndex">the gen index that points to the newly allocated data.</param>
    /// <param name="flags">the user-defined flags to distinguish the entry from others.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <see cref="GenIndexResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIndexResult.MemoryLimitHit"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIndexResult Allocate(GenIndexArray<T> array, T data, ref GenIndex genIndex, int flags)
    {
        if(array.FreeSlots.Count == 0)
        {
            return GenIndexResult.MemoryLimitHit;
        }

        // get the next available slot to allocate in.
        int slot = StackArray.Pop(array.FreeSlots);
        
        // check if its neighbour can be allocated as well.
        int nextSlot = slot + 1;
        if(nextSlot > 0 && nextSlot < array.Data.Length)
        {
            // add to the stack if it is also free.
            if (array.Allocated[nextSlot] == false)
            {
                StackArray.Push(array.FreeSlots, nextSlot);            
            }
        }

        // allocate the data.
        array.Data[slot] = data;
        array.Flags[slot] = flags;
        array.Allocated[slot] = true;

        // update the gen index with the newly allocate data.
        genIndex.Index = slot;
        genIndex.Generation = array.Generations[slot];

        // a new entry has been allocated.
        array.Count++;

        return GenIndexResult.Ok;
    }

    /// <summary>
    ///     Allocates data into the next available slot for allocation a gen index array.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="data">the data to allocate.</param>
    /// <param name="genIndex">the gen index that points to the newly allocated data.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <see cref="GenIndexResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIndexResult.MemoryLimitHit"/>
    ///         </item>
    ///     </list>
    /// </returns>    
    public static GenIndexResult Allocate(GenIndexArray<T> array, T data, ref GenIndex genIndex)
    {
        return Allocate(array, data, ref genIndex, 0);
    }

    /// <summary>
    ///     Deallocates data in a gen index array by setting its allocated flag to false and adding the slot back into the free pool for later reuse.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="genIndex">the gen index that points to the data to deallocate.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <see cref="GenIndexResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIndexResult.StaleGenIndex"/>
    ///         </item>
    ///     </list>
    /// </returns>    
    public static GenIndexResult Deallocate(GenIndexArray<T> array, GenIndex genIndex)
    {
        int index = genIndex.Index;

        // do nothing if the gen index is stale.
        if(array.Generations[index] != genIndex.Generation)
        {
            return GenIndexResult.StaleGenIndex;
        }

        // deallocate the data.
        array.Allocated[index] = false;
        StackArray.Push(array.FreeSlots, index);

        // increment the generation so that any gen indices pointing to this data are invalidated (making them stale pointers).
        genIndex.Generation = array.Generations[index]++;

        // an entry has been deallocated.
        array.Count--;

        return GenIndexResult.Ok;
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

        StackArray.Dispose(array.FreeSlots);
        array.FreeSlots = null;

        array.Count = 0;

        GC.SuppressFinalize(array);
    }

    ~GenIndexArray()
    {
        Dispose(this);
    }
}

public static class GenIndexArray
{

    /// <summary>
    ///     Allocates data into the next available slot for allocation a gen index array.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="data">the data to allocate.</param>
    /// <param name="genIndex">the gen index that points to the newly allocated data.</param>
    /// <param name="flags">the user-defined flags to distinguish the entry from others.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <see cref="GenIndexResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIndexResult.MemoryLimitHit"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIndexResult Allocate<T>(this GenIndexArray<T> array, T data, ref GenIndex genIndex, int flags)
    {
        return GenIndexArray<T>.Allocate(array, data, ref genIndex, flags);
    }

    /// <summary>
    ///     Allocates data into the next available slot for allocation a gen index array.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="data">the data to allocate.</param>
    /// <param name="genIndex">the gen index that points to the newly allocated data.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <see cref="GenIndexResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIndexResult.MemoryLimitHit"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIndexResult Allocate<T>(this GenIndexArray<T> array, T data, ref GenIndex genIndex)
    {
        return Allocate(array, data, ref genIndex, 0);
    }


    /// <summary>
    ///     Deallocates data in a gen index array by setting its allocated flag to false and adding the slot back into the free pool for later reuse.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="genIndex">the gen index that points to the data to deallocate.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <see cref="GenIndexResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIndexResult.StaleGenIndex"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIndexResult Deallocate<T>(this GenIndexArray<T> array, GenIndex genIndex)
    {
        return GenIndexArray<T>.Deallocate(array, genIndex);
    }

    public static void Dispose<T>(this GenIndexArray<T> array)
    {
        GenIndexArray<T>.Dispose(array);
    }
}