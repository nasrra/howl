using System;
using Howl.Math.Shapes;

namespace Howl.DataStructures.Bvh;

public class Soa_BoundingVolumeHierarchy : IDisposable
{
    /// <summary>
    /// The leaves to construct branches from.
    /// </summary>
    public LeafBuffer Leaves;   

    /// <summary>
    /// The constructed branches from the inserted leaves.
    /// </summary>
    public BranchBuffer Branches;

    /// <summary>
    /// The spatial pairs of leaves in the constructed tree.
    /// </summary>
    public SpatialPairBuffer SpatialPairs;

    /// <summary>
    /// Whether this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new bounding volume hierarchy instance.
    /// </summary>
    /// <param name="capacity"></param>
    public Soa_BoundingVolumeHierarchy(int capacity)
    {
        Leaves = new(capacity);
        Branches = new(capacity*2);
    }

    /// <summary>
    /// Sets the count of the bounding volume hierarchy's internal arrays to zero.
    /// </summary>
    /// <param name="bvh">the bvh to clear.</param>
    public static void Clear(Soa_BoundingVolumeHierarchy bvh)
    {
        LeafBuffer.Clear(bvh.Leaves);
        BranchBuffer.Clear(bvh.Branches);
    }

    public static void ConstructTree(Soa_BoundingVolumeHierarchy bvh)
    {
        BranchBuffer.Clear(bvh.Branches);
        Soa_Aabb.CalculateCentroids(bvh.Leaves.Aabbs, bvh.Leaves.Centroids.X, bvh.Leaves.Centroids.Y, 0, bvh.Leaves.Length);

    }

    public static void ConstructBranches(
        LeafBuffer leaves,
        BranchBuffer branches,
        Span<float> centroidXSlice,
        Span<float> centroidYSlice,
        int centroidSliceLength,
        ref int writeIndex,
        ref float boundingBoxMinX,
        ref float boundingBoxMinY,
        ref float boundingBoxMaxX,
        ref float boundingBoxMaxY
    )
    {
        // reserve space.
        int branchIndex = writeIndex++;

        // == leaf ==
        if (centroidSliceLength <= 2)
        {
            // build leaf aabb.
            // int leftLeafIndex= 
        }
        else
        {
            
        }

    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_BoundingVolumeHierarchy bvh)
    {
        if(bvh.Disposed)
            return;
        
        bvh.Disposed = true;
        
        bvh.Leaves.Dispose();
        bvh.Leaves = null;
        bvh.Branches.Dispose();
        bvh.Branches = null;

        GC.SuppressFinalize(bvh);
    }

    ~Soa_BoundingVolumeHierarchy()
    {
        Dispose(this);
    }
}