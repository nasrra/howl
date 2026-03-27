using System;
using System.Runtime.CompilerServices;
using Howl.ECS;

namespace Howl.DataStructures.Bvh;

public class QueryResultBuffer : IDisposable
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
    /// The count of allocated entries; starting from index 0.
    /// </summary>
    public int Count;

    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a query result buffer.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public QueryResultBuffer(int length)
    {
        GenIndices = new(length);
        Flags = new int[length];
        Length = length;
    }

    /// <summary>
    /// Appends a query result to a buffer
    /// </summary>
    /// <param name="buffer">the buffer to append to.</param>
    /// <param name="index">the <c>index</c> of the data associated with the query result.</param>
    /// <param name="generation">the <c>generation</c> of the data associated with the query result.</param>
    /// <param name="flags">the user-defined flags of the data associated with the query result.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendQueryResult(QueryResultBuffer buffer, int index, int generation, int flags)
    {
        int count = buffer.Count;
        buffer.GenIndices.Indices[count] = index;
        buffer.GenIndices.Generations[count] = generation;
        buffer.Flags[count] = flags;
        buffer.Count++;
    }

    /// <summary>
    /// Sets a buffer's count to zero.
    /// </summary>
    /// <param name="buffer">the buffer to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(QueryResultBuffer buffer)
    {
        buffer.Count = 0; 
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public static void Dispose(QueryResultBuffer buffer)
    {
        if(buffer.Disposed)
            return;
        
        buffer.Disposed = true;

        Soa_GenIndex.Dispose(buffer.GenIndices);
        buffer.GenIndices = null;
        buffer.Flags = null;

        GC.SuppressFinalize(buffer);
    }

    ~QueryResultBuffer()
    {
        Dispose(this);
    }
}