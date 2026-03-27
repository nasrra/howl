using System;
using System.Runtime.CompilerServices;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.DataStructures.Bvh;

public class LeafBuffer : IDisposable
{
    /// <summary>
    /// The Axis-Aligned Bounding-Boxes.
    /// </summary>
    public Soa_Aabb Aabbs;

    /// <summary>
    /// The gen indices.
    /// </summary>
    public Soa_GenIndex GenIndices;

    /// <summary>
    /// The centroids of the Aabb's
    /// </summary>
    public Soa_Vector2 Centroids;

    /// <summary>
    /// The user-defined flags.
    /// </summary>
    public int[] Flags;

    /// <summary>
    /// The count of allocated leaf entries; starting from index 0.
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
    /// Creates a new LeafBuffer instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public LeafBuffer(int length)
    {
        Aabbs = new(length);
        GenIndices = new(length);
        Flags = new int[length];
        Length = length;
    }

    /// <summary>
    /// Appends a leaf to a buffer.
    /// </summary>
    /// <param name="buffer">the buffer to append to.</param>
    /// <param name="minX">the the x-component of the aabb minimum vertex.</param>
    /// <param name="minY">the the y-component of the aabb minimum vertex.</param>
    /// <param name="maxX">the the x-component of the aabb maximum vertex.</param>
    /// <param name="maxY">the the y-component of the aabb maximum vertex.</param>
    /// <param name="index">the index of the data to associate with the leaf.</param>
    /// <param name="generation">the generation of the data to associate with the leaf.</param>
    /// <param name="flags">the user-defined flags to associate with the leaf.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(LeafBuffer buffer, float minX, float minY, float maxX, float maxY, int index, int generation, int flags)
    {
        int count = buffer.Count;
        buffer.Aabbs.MinX[count] = minX;
        buffer.Aabbs.MinY[count] = minY;
        buffer.Aabbs.MaxX[count] = maxX;
        buffer.Aabbs.MaxY[count] = maxY;
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
    public static void Clear(LeafBuffer buffer)
    {
        buffer.Count = 0;
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(LeafBuffer buffer)
    {
        if(buffer.Disposed)
            return;

        buffer.Disposed = true;
        
        Soa_Aabb.Dispose(buffer.Aabbs);
        buffer.Aabbs = null;
        Soa_GenIndex.Dispose(buffer.GenIndices);
        buffer.GenIndices = null;
        buffer.Flags = null;
        buffer.Length = 0;
        buffer.Count = 0;

        GC.SuppressFinalize(buffer);
    }

    ~LeafBuffer()
    {
        Dispose(this);
    }
}