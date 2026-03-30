using System;
using System.Runtime.CompilerServices;
using Howl.Algorithms.Sorting;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.DataStructures.Bvh;

public class LeafBuffer : IDisposable
{

    /// <summary>
    /// The radix sort buffer used when sorting this leaf buffer.
    /// </summary>
    public RadixSortBuffer RadixSortBuffer;

    /// <summary>
    /// The Axis-Aligned Bounding-Boxes.
    /// </summary>
    public Soa_Aabb Aabbs;

    /// <summary>
    /// The gen indices of the data associated with a leaf.
    /// </summary>
    public Soa_GenIndex GenIndices;

    /// <summary>
    /// The centroids of the Aabb's
    /// </summary>
    /// <remarks>
    /// Use an element in <c>CentroidIds</> to get the leaf data associated with this centroid.
    /// Elements in <c>CentroidIds</c> and <c>Centroids</c> are associated via index.
    /// </remarks>
    public Soa_Vector2 Centroids;

    /// <summary>
    /// Used as an index for a centroid element in <c>Centroids</c> to get its leaf data associated witht the centroid.
    /// </summary>
    /// <remarks>
    /// Elements in <c>CentroidIds</c> and <c>Centroids</c> are associated via index.
    /// </remarks>
    public int[] CentroidIds;

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
        Centroids = new(length);
        CentroidIds = new int[length];
        RadixSortBuffer = new(length);
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

    public static void SortCentroidsByAscX(LeafBuffer buffer, int start, int length)
    {
        // set centroid indices.
        for(int i = start; i < length; i++)
        {
            buffer.CentroidIds[i] = i;
        }
    }

    public static void SortCentroidsByAscY(LeafBuffer buffer, int start, int length)
    {
        // set centroid indices.
        for(int i = start; i < length; i++)
        {
            buffer.CentroidIds[i] = i;
        }
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
        
        Soa_Vector2.Dispose(buffer.Centroids);
        buffer.Centroids = null;
                
        buffer.RadixSortBuffer.Dispose();
        buffer.RadixSortBuffer = null;

        buffer.Flags = null;
        
        buffer.CentroidIds = null;
        
        buffer.Length = 0;
        buffer.Count = 0;

        GC.SuppressFinalize(buffer);
    }

    ~LeafBuffer()
    {
        Dispose(this);
    }
}