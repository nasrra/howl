using System;
using System.Runtime.CompilerServices;
using Howl.Ecs;

namespace Howl.DataStructures.Bvh;

public class Soa_QueryResult : IDisposable
{
    /// <summary>
    /// The gen indices.
    /// </summary>
    public Soa_GenIndex GenIndices;

    /// <summary>
    /// The user-defined flags.
    /// </summary>
    public int[] Flags;

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
    /// Creates a soa instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public Soa_QueryResult(int length)
    {
        GenIndices = new(length);
        Flags = new int[length];
        Length = length;
    }

    /// <summary>
    /// Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
    /// </summary>
    /// <param name="soa">the soa instance to append to.</param>
    /// <param name="index">the <c>index</c> of the data associated with the query result.</param>
    /// <param name="generation">the <c>generation</c> of the data associated with the query result.</param>
    /// <param name="flags">the user-defined flags of the data associated with the query result.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(Soa_QueryResult soa, int index, int generation, int flags)
    {
        int count = soa.AppendCount;
        soa.GenIndices.Indices[count] = index;
        soa.GenIndices.Generations[count] = generation;
        soa.Flags[count] = flags;
        soa.AppendCount++;
    }

    /// <summary>
    /// Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="buffer">the buffer to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(Soa_QueryResult buffer)
    {
        buffer.AppendCount = 0; 
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public static void Dispose(Soa_QueryResult buffer)
    {
        if(buffer.Disposed)
            return;
        
        buffer.Disposed = true;

        Soa_GenIndex.Dispose(buffer.GenIndices);
        buffer.GenIndices = null;
        buffer.Flags = null;
        buffer.AppendCount = 0;
        buffer.Length = 0;

        GC.SuppressFinalize(buffer);
    }

    ~Soa_QueryResult()
    {
        Dispose(this);
    }
}