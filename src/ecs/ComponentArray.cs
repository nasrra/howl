using System;

public class ComponentArray<T> : IDisposable
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
    ///     The count of slots that have been allocated to.
    /// </summary>
    public int Count;

    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    ///     Whether or not this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new component array instance. 
    /// </summary>
    /// <param name="length">the lengths of the backing arrays.</param>
    public ComponentArray(int length){
        Data = new T[length];
        Flags = new int[length];
        Allocated = new bool[length];
        Length = length;
    }

    /// <summary>
    ///     Allocates data into the backing data array.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="index">the index in the backing array to allocate into.</param>
    /// <param name="data">the data to allocate.</param>
    /// <param name="flags">the user-defined flags to distinguish the entry from others.</param>
    public static void Allocate(ComponentArray<T> array, int index, T data, int flags)
    {
        array.Data[index] = data;
        array.Flags[index] = flags;
        array.Allocated[index] = true;
        array.Count++;
    }

    /// <summary>
    ///     Sets the allocated bool at a given index to false.
    /// </summary>
    /// <param name="array">the component array to deallocate from.</param>
    /// <param name="index">the index of the element to deallocate.</param>
    /// <returns>true, if the entry was successfully deallocated; otherwise false if it is already deallocated.</returns>
    public static bool Deallocate(ComponentArray<T> array, int index)
    {
        if(array.Allocated[index] == false)
        {
            return false;
        }

        array.Allocated[index] = false;        
        array.Count--;
        return true;
    }

    /// <summary>
    ///     Sets the component array count to zero.
    /// </summary>
    /// <param name="array">the component array to clear.</param>
    public static void ClearCount(ComponentArray<T> array)
    {
        array.Count = 0;
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public static void Dispose(ComponentArray<T> array)
    {
        if (array.Disposed)
        {
            return;
        }

        array.Disposed = true;

        array.Allocated = null;
        array.Data = null;
        array.Flags = null;
        array.Count = 0;
        array.Length = 0;

        GC.SuppressFinalize(array);
    }

    ~ComponentArray()
    {
        Dispose(this);
    }
}

public static class ComponentArray
{
    /// <summary>
    ///     Allocates data into the backing data array.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="index">the index in the backing array to allocate into.</param>
    /// <param name="data">the data to allocate.</param>
    /// <param name="flags">the user-defined flags to distinguish the entry from others.</param>
    public static void Allocate<T>(this ComponentArray<T> array, int index, T data, int flags)
    {
        ComponentArray<T>.Allocate(array, index, data, flags);
    }

    /// <summary>
    ///     Sets the allocated bool at a given index to false.
    /// </summary>
    /// <param name="array">the component array to deallocate from.</param>
    /// <param name="index">the index of the element to deallocate.</param>
    /// <returns>true, if the entry was successfully deallocated; otherwise false if it is already deallocated.</returns>
    public static bool Deallocate<T>(this ComponentArray<T> array, int index)
    {
        return ComponentArray<T>.Deallocate(array, index);
    }

    /// <summary>
    ///     Sets the component array count to zero.
    /// </summary>
    /// <param name="array">the component array to clear.</param>
    public static void ClearCount<T>(this ComponentArray<T> array)
    {
        array.Count = 0;
    }

    public static void Dispose<T>(this ComponentArray<T> array)
    {
        ComponentArray<T>.Dispose(array);
    }
}