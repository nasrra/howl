using System;
using System.Runtime.CompilerServices;
using Howl.Algorithms.Sorting;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.DataStructures.Bvh;

public class Soa_Leaf : IDisposable
{
    /// <summary>
    /// The Axis-Aligned Bounding-Boxes.
    /// </summary>
    public Soa_Aabb Aabbs;

    /// <summary>
    /// The gen indices of the data associated with a leaf.
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
    /// Creates a new soa instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public Soa_Leaf(int length)
    {
        Aabbs = new(length);
        GenIndices = new(length);
        Flags = new int[length];
        Length = length;
    }

    /// <summary>
    /// Appends an entry to the soa instance..
    /// </summary>
    /// <param name="soa">the soa instance to append to.</param>
    /// <param name="minX">the the x-component of the aabb minimum vertex.</param>
    /// <param name="minY">the the y-component of the aabb minimum vertex.</param>
    /// <param name="maxX">the the x-component of the aabb maximum vertex.</param>
    /// <param name="maxY">the the y-component of the aabb maximum vertex.</param>
    /// <param name="index">the index of the data to associate with the leaf.</param>
    /// <param name="generation">the generation of the data to associate with the leaf.</param>
    /// <param name="flags">the user-defined flags to associate with the leaf.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(Soa_Leaf soa, float minX, float minY, float maxX, float maxY, int index, int generation, int flags)
    {
        int count = soa.AppendCount;
        soa.Aabbs.MinX[count] = minX;
        soa.Aabbs.MinY[count] = minY;
        soa.Aabbs.MaxX[count] = maxX;
        soa.Aabbs.MaxY[count] = maxY;
        soa.GenIndices.Indices[count] = index;
        soa.GenIndices.Generations[count] = generation;
        soa.Flags[count] = flags;
        soa.AppendCount++;
    }

    /// <summary>
    /// Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa instance to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(Soa_Leaf soa)
    {
        soa.AppendCount = 0;
    }

    /// <summary>
    /// Queries a soa instance for any leaves that may overlap within a given area.
    /// </summary>
    /// <param name="leaves">the soa instance to query.</param>
    /// <param name="results">the soa instance to append overlapping leaf data to; specifically the gen index and user-defined flags of the data associated with the leaf.</param>
    /// <param name="indices">the indices of the leaves to query for overlap against the query area.</param>
    /// <param name="minX">the x-component of the query area minimum vertex.</param>
    /// <param name="minY">the y-component of the query area maximum vertex.</param>
    /// <param name="maxX">the x-component of the query area maximum vertex.</param>
    /// <param name="maxY">the y-component of the query area maximum vertex.</param>
    public static void Query(Soa_Leaf leaves, QueryResultBuffer results, Span<int> indices, float minX, float minY, float maxX, float maxY)
    {
        // hoisting of invariance.
        Soa_Aabb aabbs = leaves.Aabbs;
        Span<float> aabbsMinX = aabbs.MinX;
        Span<float> aabbsMinY = aabbs.MinY;
        Span<float> aabbsMaxX = aabbs.MaxX;
        Span<float> aabbsMaxY = aabbs.MaxY;
        Soa_GenIndex genIndices = leaves.GenIndices;
        Span<int> leafIndices = genIndices.Indices;
        Span<int> leafGenerations = genIndices.Generations;
        Span<int> leafFags = leaves.Flags;

        for(int i = 0; i < indices.Length; i++)
        {
            int index = indices[i];
            if(Aabb.Intersect(aabbsMinX[index], aabbsMinY[index], aabbsMaxX[index], aabbsMaxY[index], minX, minY, maxX, maxY))
            {
                // add to the results if the leaf intersects with the query area.
                QueryResultBuffer.Append(results, leafIndices[index], leafGenerations[index], leafFags[index]);
            }
        }
    }



    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_Leaf buffer)
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
        buffer.AppendCount = 0;

        GC.SuppressFinalize(buffer);
    }

    ~Soa_Leaf()
    {
        Dispose(this);
    }
}