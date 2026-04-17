using System;
using System.Runtime.CompilerServices;
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
    /// The centroids of the Aabbs.
    /// </summary>
    public Soa_Vector2 Centroids;

    /// <summary>
    /// Gets the indices of branches that leaves are parented to.
    /// </summary>
    /// <remarks>
    /// Elements in this array should be valid after a Bounding Volume Hierarchy has been constructed.
    /// </remarks>
    public int[] BranchIndices;

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
        Centroids = new(length);
        BranchIndices = new int[length];
        Length = length;
    }

    /// <summary>
    /// Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
    /// </summary>
    /// <param name="soa">the soa instance to append to.</param>
    /// <param name="minX">the the x-component of the aabb minimum vertex.</param>
    /// <param name="minY">the the y-component of the aabb minimum vertex.</param>
    /// <param name="maxX">the the x-component of the aabb maximum vertex.</param>
    /// <param name="maxY">the the y-component of the aabb maximum vertex.</param>
    /// <param name="centroidX">the x-component of the aabb centroid.</param>
    /// <param name="centroidY">the y-component of the aabb centroid.</param>
    /// <returns>the index the entry was appended to in the backing arrays.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int Append(Soa_Leaf soa, float minX, float minY, float maxX, float maxY, float centroidX, float centroidY)
    {
        Soa_Aabb.Append(soa.Aabbs, minX, minY, maxX, maxY);
        Soa_Vector2.Append(soa.Centroids, centroidX, centroidY);
        soa.AppendCount++;
        return soa.AppendCount-1;
    }

    /// <summary>
    /// Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa instance to reset.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResetCount(Soa_Leaf soa)
    {
        Soa_Aabb.ResetCount(soa.Aabbs);
        Soa_Vector2.ResetCount(soa.Centroids);
        soa.AppendCount = 0;
    }



    /// <summary>
    ///     Checks whether or not a leaf overlaps within a given area.
    /// </summary>
    /// <param name="leaves">the soa instance containing the leaf.</param>
    /// <param name="leafIndex">the index of the leaf in the soa instance.</param>
    /// <param name="minX">the x-component of the query area minimum vertex.</param>
    /// <param name="minY">the y-component of the query area maximum vertex.</param>
    /// <param name="maxX">the x-component of the query area maximum vertex.</param>
    /// <param name="maxY">the y-component of the query area maximum vertex.</param>
    /// <returns>true, if the leaf intersects with the query area; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersects(Soa_Leaf leaves, int leafIndex, float minX, float minY, float maxX, float maxY)
    {
        // hoisting of invariance.
        Soa_Aabb aabbs = leaves.Aabbs;
        Span<float> aabbsMinX = aabbs.MinX;
        Span<float> aabbsMinY = aabbs.MinY;
        Span<float> aabbsMaxX = aabbs.MaxX;
        Span<float> aabbsMaxY = aabbs.MaxY;

        return Aabb.Intersect(aabbsMinX[leafIndex], aabbsMinY[leafIndex], aabbsMaxX[leafIndex], aabbsMaxY[leafIndex], minX, minY, maxX, maxY);
    }

    /// <summary>
    ///     Checks whether or not a leaf overlaps with a given line segment.
    /// </summary>
    /// <param name="leaves">the soa instance containing the leaf.</param>
    /// <param name="leafIndex">the index of the leaf in the soa instance.</param>
    /// <param name="lineStartX">the x-component of the line statrting point.</param>
    /// <param name="lineStartY">the x-component of the line statrting point.</param>
    /// <param name="lineEndX">the x-component of the line end point.</param>
    /// <param name="lineEndY">the x-component of the line end point.</param>
    /// <returns>true, if the leaf intersects with the line segment; otherwise false.</returns>
    public static bool LineIntersects(Soa_Leaf leaves, int leafIndex, float startX, float startY, float endX, float endY)
    {
        // hoisting of invariance.
        Soa_Aabb aabbs = leaves.Aabbs;
        Span<float> aabbsMinX = aabbs.MinX;
        Span<float> aabbsMinY = aabbs.MinY;
        Span<float> aabbsMaxX = aabbs.MaxX;
        Span<float> aabbsMaxY = aabbs.MaxY;

        return Aabb.LineIntersect(aabbsMinX[leafIndex], aabbsMinY[leafIndex], aabbsMaxX[leafIndex], aabbsMaxY[leafIndex], startX, startY, endX, endY);        
    }



    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_Leaf soa)
    {
        if(soa.Disposed)
            return;

        soa.Disposed = true;
        
        Soa_Aabb.Dispose(soa.Aabbs);
        soa.Aabbs = null;
        
        Soa_Vector2.Dispose(soa.Centroids);
        soa.Centroids = null;

        soa.BranchIndices = null;
                                        
        soa.Length = 0;
        soa.AppendCount = 0;

        GC.SuppressFinalize(soa);
    }

    ~Soa_Leaf()
    {
        Dispose(this);
    }
}