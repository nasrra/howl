using System.Runtime.CompilerServices;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Unmanaged.Collections;

namespace Howl.Text.Bvh;

public struct Soa_Leaf
{
    /// <summary>
    ///     The Axis-Aligned Bounding-Boxes.
    /// </summary>
    public Soa_Aabb Aabbs;

    /// <summary>
    ///     The centroids of the Aabbs.
    /// </summary>
    public Soa_Vector2 Centroids;

    /// <summary>
    ///     The user-defined categories of the leaves (used to filter overlap results).
    /// </summary>
    public Array<int> Categories;

    /// <summary>
    ///     Gets the indices of branches that leaves are parented to.
    /// </summary>
    /// <remarks>
    ///     Elements in this array should be valid after a Bounding Volume Hierarchy has been constructed.
    /// </remarks>
    public Array<int> BranchIndices;

    /// <summary>
    ///     The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;

    /// <summary>
    ///     The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    public bool IsInitialised;

    public static bool Initialise(ref Soa_Leaf soa, ref Memory.Arena arena, int length)
    {
        if (soa.IsInitialised)
        {
            Debug.Panic("Already Initialised.");
            return false;
        }

        Soa_Aabb.Initialise(ref soa.Aabbs, ref arena, length);
        Soa_Vector2.Initialise(ref soa.Centroids, ref arena, length);
        Array.Initialise(ref soa.BranchIndices, ref arena, length);
        Array.Initialise(ref soa.Categories, ref arena, length);
        soa.Length = length;

        soa.IsInitialised = true;
        return true;
    }

    /// <summary>
    ///     Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
    /// </summary>
    /// <param name="soa">the soa instance to append to.</param>
    /// <param name="minX">the the x-component of the aabb minimum vertex.</param>
    /// <param name="minY">the the y-component of the aabb minimum vertex.</param>
    /// <param name="maxX">the the x-component of the aabb maximum vertex.</param>
    /// <param name="maxY">the the y-component of the aabb maximum vertex.</param>
    /// <param name="centroidX">the x-component of the aabb centroid.</param>
    /// <param name="centroidY">the y-component of the aabb centroid.</param>
    /// <param name="category">the category of the leaf.</param>
    /// <returns>the index the entry was appended to in the backing arrays.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int Append(ref Soa_Leaf soa, float minX, float minY, float maxX, float maxY, float centroidX, float centroidY, int category)
    {
        Soa_Aabb.Append(ref soa.Aabbs, minX, minY, maxX, maxY);
        Soa_Vector2.Append(ref soa.Centroids, centroidX, centroidY);
        soa.Categories[soa.AppendCount] = category;
        soa.AppendCount++;
        return soa.AppendCount-1;
    }

    /// <summary>
    /// Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa instance to reset.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResetCount(ref Soa_Leaf soa)
    {
        Soa_Aabb.ResetCount(ref soa.Aabbs);
        Soa_Vector2.ResetCount(ref soa.Centroids);
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
        System.Span<float> aabbsMinX = Array.AsSpan(aabbs.MinX);
        System.Span<float> aabbsMinY = Array.AsSpan(aabbs.MinY);
        System.Span<float> aabbsMaxX = Array.AsSpan(aabbs.MaxX);
        System.Span<float> aabbsMaxY = Array.AsSpan(aabbs.MaxY);

        return Aabb.Intersect(aabbsMinX[leafIndex], minX, aabbsMinY[leafIndex], minY, aabbsMaxX[leafIndex], maxX, aabbsMaxY[leafIndex], maxY);
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
        System.Span<float> aabbsMinX = Array.AsSpan(aabbs.MinX);
        System.Span<float> aabbsMinY = Array.AsSpan(aabbs.MinY);
        System.Span<float> aabbsMaxX = Array.AsSpan(aabbs.MaxX);
        System.Span<float> aabbsMaxY = Array.AsSpan(aabbs.MaxY);

        return Aabb.LineIntersect(aabbsMinX[leafIndex], aabbsMinY[leafIndex], aabbsMaxX[leafIndex], aabbsMaxY[leafIndex], startX, startY, endX, endY);        
    }
}