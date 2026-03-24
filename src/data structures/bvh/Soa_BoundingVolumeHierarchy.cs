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
        Branches = new(capacity);
    }

    /// <summary>
    /// Appends a leaf into a bounding volume hierarchy.
    /// </summary>
    /// <param name="bvh">the bvh to append to.</param>
    /// <param name="minX">the the x-component of the aabb minimum vertex.</param>
    /// <param name="minY">the the y-component of the aabb minimum vertex.</param>
    /// <param name="maxX">the the x-component of the aabb maximum vertex.</param>
    /// <param name="maxY">the the y-component of the aabb maximum vertex.</param>
    /// <param name="index">the index of the data to associate with the leaf.</param>
    /// <param name="generation">the generation of the data to associate with the leaf.</param>
    /// <param name="flags">the user-defined flags to associate with the leaf.</param>
    public static void AppendLeaf(Soa_BoundingVolumeHierarchy bvh, float minX, float minY, float maxX, float maxY, int index, int generation, int flags)
    {
        LeafBuffer.AppendLeaf(bvh.Leaves, minX, minY, maxX, maxY, index, generation, flags);
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