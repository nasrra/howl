using System;
using System.Runtime.CompilerServices;
using Howl.ECS;
using Howl.Math.Shapes;

namespace Howl.DataStructures;

public class BvhLeafBuffer : IDisposable
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
    /// The user-defined flags.
    /// </summary>
    public int[] Flags;

    /// <summary>
    /// The count of allocated leaf entries; starting from index 0.
    /// </summary>
    public int Count;

    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new BvhLeafBuffer instance.
    /// </summary>
    /// <param name="capacity">the capacity of the backing arrays.</param>
    public BvhLeafBuffer(int capacity)
    {
        Aabbs = new(capacity);
        GenIndices = new(capacity);
        Flags = new int[capacity];
    }

    /// <summary>
    /// Appends a leaf to a bvh leaf buffer.
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
    public static void AppendLeaf(BvhLeafBuffer buffer, float minX, float minY, float maxX, float maxY, int index, int generation, int flags)
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
    /// Sets the buffer's count to zero.
    /// </summary>
    /// <param name="buffer">the buffer to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(BvhLeafBuffer buffer)
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

    public static void Dispose(BvhLeafBuffer buffer)
    {
        if(buffer.Disposed)
            return;
        
        buffer.Aabbs.Dispose();
        buffer.Aabbs = null;
        buffer.GenIndices.Dispose();
        buffer.GenIndices = null;
        buffer.Flags = null;
    }
}