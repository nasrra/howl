using System;

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
    }

    public static void ConstructBranches(
        LeafBuffer leaves,
        BranchBuffer branches,
        ref int writeIndex,
        ref float boundingBoxMinX,
        ref float boundingBoxMinY,
        ref float boundingBoxMaxX,
        ref float boundingBoxMaxY
    )
    {
        // reserve space.
        int branchIndex = writeIndex++;

        // if()
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