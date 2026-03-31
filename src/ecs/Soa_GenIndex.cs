using System;

namespace Howl.ECS;

public class Soa_GenIndex : IDisposable
{
    /// <summary>
    /// Gets and sets the indices.
    /// </summary>
    public int[] Indices;

    /// <summary>
    /// Gets and sets the generations.
    /// </summary>
    public int[] Generations;

    /// <summary>
    /// The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;

    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new Structure-Of-Array's GenIndex instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public Soa_GenIndex(int length)
    {
        Indices = new int[length];
        Generations = new int[length];
        Length = length;
    }

    /// <summary>
    /// Inserts an entry into a soa instance.
    /// </summary>
    /// <param name="soa">the soa instance.</param>
    /// <param name="insertIndex">the index in the soa to insert into.</param>
    /// <param name="index">the index value to insert.</param>
    /// <param name="generation">the generation value to insert.</param>
    public static void Insert(Soa_GenIndex soa, int insertIndex, int index, int generation)
    {
        soa.Indices[insertIndex] = index;
        soa.Generations[insertIndex] = generation;
    }

    /// <summary>
    /// Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
    /// </summary>
    /// <param name="soa">the soa instance to append to.</param>
    /// <param name="index">the index value.</param>
    /// <param name="generation">the generation value.</param>
    public static void Append(Soa_GenIndex soa, int index, int generation)
    {
        Insert(soa, soa.AppendCount, index, generation);
        soa.AppendCount++;
    }

    /// <summary>
    /// Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa instance to reset.</param>
    public static void ResetCount(Soa_GenIndex soa)
    {
        soa.AppendCount = 0;        
    }



    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_GenIndex soa)
    {
        if(soa.Disposed)
            return;

        soa.Disposed = true;
        
        soa.Indices = null;
        soa.Generations = null;
        soa.Length = 0;

        GC.SuppressFinalize(soa);
    }

    ~Soa_GenIndex()
    {
        Dispose(this);
    }
}