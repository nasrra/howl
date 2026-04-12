using System;

namespace Howl.Collections;

public class DopeVector<T>
{
    /// <summary>
    ///     The data of all entries.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             There will be gaps in this data set, use <c>AppendCounts</c> to determine how many elements in this array
    ///             are valid for a given entry. To get the 
    ///         </item>
    ///         <item>
    ///             To get the starting index for an entry, use: <c>entryIndex</c> * <c>EntryDataLength</c>. 
    ///         </item>
    ///     </list>
    /// </remarks>
    public T[] Data;
    
    /// <summary>
    ///     The amount of data appended to all entries.
    /// </summary>
    /// <remarks>
    ///     Elements should be indexed via a <c>entryIndex</c> integer.
    /// </remarks>
    public int[] AppendCounts;

    /// <summary>
    ///     The length of data that an entry can store.
    /// </summary>
    public int EntryDataLength;

    /// <summary>
    ///     The stride of data an entry in this array can store.
    /// </summary>
    public int EntryStride;

    /// <summary>
    ///     Whether or not this instance is disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new DopeVector instance.
    /// </summary>
    /// <param name="entryLength">the length of entries.</param>
    /// <param name="entryDataLength">the length of the data each entry can have.</param>
    public DopeVector(int entryLength, int entryDataLength)
    {
        Data = new T[entryLength*entryDataLength];
        AppendCounts = new int[entryLength];
        EntryStride = entryLength;
        EntryDataLength = entryDataLength;
    }




    /*******************
    
        Disposal.
    
    ********************/




    ~DopeVector()
    {
        DopeVector.Dispose(this);
    }
}

public static class DopeVector
{
    /// <summary>
    ///     Gets a slice of the 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="entryIndex"></param>
    /// <returns></returns>
    public static Span<T> GetAppendedData<T>(this DopeVector<T> array, int entryIndex)
    {
        Span<T> values = array.Data;
        return values.Slice(entryIndex * array.EntryDataLength, array.AppendCounts[entryIndex]);
    }

    /// <summary>
    ///     Appends data to an entry in a DopeVector instance.
    /// </summary>
    /// <param name="array">the DopeVector instance to append to.</param>
    /// <param name="data">the data to append to the entry.</param>
    /// <param name="entryIndex">the index of the entry in the 2d array instance.</param>
    /// <returns>true, if the data was successfully appended; otherwise false if there was no space available.</returns>
    public static bool Append<T>(this DopeVector<T> array, T data, int entryIndex)
    {
        if(array.AppendCounts[entryIndex]+1 >= array.EntryDataLength)
        {
            return false;
        }
        
        array.Data[(entryIndex * array.EntryDataLength) + array.AppendCounts[entryIndex]] = data;
        array.AppendCounts[entryIndex]++; 
        
        return true;
    }

    /// <summary>
    ///     Sets a DopeVector's entry append counts all to zero.
    /// </summary>
    /// <param name="array">the DopeVector instance to clear.</param>
    public static void ClearAppendCounts<T>(this DopeVector<T> array)
    {
        for(int i = 0; i < array.AppendCounts.Length; i++)
        {
            array.AppendCounts[i] = 0;
        }
    }
    



    /*******************
    
        Disposal.
    
    ********************/




    public static void Dispose<T>(this DopeVector<T> array)
    {
        if (array.Disposed)
        {
            return;
        }

        array.Disposed = true;
    
        array.Data = null;
        array.AppendCounts = null;
        array.EntryDataLength = 0;
        array.EntryStride = 0;

        GC.SuppressFinalize(array);
    }
}