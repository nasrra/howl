using System;
using Howl.Math.Shapes;

namespace Howl.DataStructures.Bvh;

public class Soa_Branch : IDisposable
{
    /// <summary>
    /// The Axis-Aligned Bounding-Boxes of all branches.
    /// </summary>
    public Soa_Aabb Aabbs;

    /// <summary>
    /// The left leaf indices of all branches.
    /// </summary>
    public int[] LeftLeafIndices;

    /// <summary>
    /// The right leaf indices of all branches.
    /// </summary>
    public int[] RightLeafIndices;

    /// <summary>
    /// The number of child branches (including the branch itself) of all branches.
    /// </summary>
    /// <remarks>
    /// E.g a branch that has three 4 children will have a subtree size of 5; as the subtree size counts the branch as well.
    /// </remarks>
    public int[] SubtreeSizes;

    /// <summary>
    /// The amount of leaves attatched of all branches.
    /// </summary>
    /// <remarks>
    /// Specifically, the amount of immediate leaves attatched to a branch; not counting children or parents.
    /// </remarks>
    public int[] LeafCounts;

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
    /// Creates a new soa branch instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public Soa_Branch(int length)
    {
        Aabbs = new(length);
        LeftLeafIndices = new int[length];
        RightLeafIndices = new int[length];
        SubtreeSizes = new int[length];
        LeafCounts = new int[length];
        Length = length;
    }

    /// <summary>
    /// Appends an entry into a soa at a soa's <c>AppendCount</c> index.
    /// </summary>
    /// <param name="soa">the soa instance to append to.</param>
    /// <param name="minX">the x-component of the minimum vertex of the aabb.</param>
    /// <param name="minY">the y-component of the minimum vertex of the aabb.</param>
    /// <param name="maxX">the x-component of the minimum vertex of the aabb.</param>
    /// <param name="maxY">the y-component of the minimum vertex of the aabb.</param>
    /// <param name="leftLeafIndex">the index of the left leaf.</param>
    /// <param name="rightLeafIndex">the index of the right leaf.</param>
    /// <param name="subtreeSize">the subtree size.</param>
    /// <param name="leafCount">the amount of leaves attached to the branch.</param>
    public static void Append(Soa_Branch soa, float minX, float minY, float maxX, float maxY, int leftLeafIndex, int rightLeafIndex, 
        int subtreeSize, int leafCount
    )
    {
        Insert(soa, soa.AppendCount, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
        soa.AppendCount++;
    }

    /// <summary>
    /// Inserts an entry into a soa instance.
    /// </summary>
    /// <param name="soa">the soa instance to insert into.</param>
    /// <param name="insertIndex">the index in the soa arrays to insert into.</param>
    /// <param name="minX">the x-component of the minimum vertex of the aabb.</param>
    /// <param name="minY">the y-component of the minimum vertex of the aabb.</param>
    /// <param name="maxX">the x-component of the maximum vertex of the aabb.</param>
    /// <param name="maxY">the y-component of the maximum vertex of the aabb.</param>
    /// <param name="leftLeafIndex">the index of the left leaf.</param>
    /// <param name="rightLeafIndex">the index of the right leaf.</param>
    /// <param name="subtreeSize">the subtree size.</param>
    /// <param name="leafCount">the amount of leaves attached to the branch.</param>
    public static void Insert(Soa_Branch soa, int insertIndex, float minX, float minY, float maxX, float maxY, int leftLeafIndex, int rightLeafIndex, 
        int subtreeSize, int leafCount
    )
    {
        soa.Aabbs.MinX[insertIndex] = minX;
        soa.Aabbs.MinY[insertIndex] = minY;
        soa.Aabbs.MaxX[insertIndex] = maxX;
        soa.Aabbs.MaxY[insertIndex] = maxY;
        soa.LeftLeafIndices[insertIndex] = leftLeafIndex;
        soa.RightLeafIndices[insertIndex] = rightLeafIndex;
        soa.SubtreeSizes[insertIndex] = subtreeSize;
        soa.LeafCounts[insertIndex] = leafCount;        
    }

    /// <summary>
    /// Sets a soa's append count to zero.
    /// </summary>
    /// <param name="soa">the buffer to clear.</param>
    public static void Clear(Soa_Branch soa)
    {
        soa.AppendCount = 0;
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_Branch soa)
    {
        if(soa.Disposed)
            return;
        
        soa.Disposed = true;

        Soa_Aabb.Dispose(soa.Aabbs);
        soa.Aabbs = null;
        soa.LeftLeafIndices = null;
        soa.RightLeafIndices = null;
        soa.SubtreeSizes = null;
        soa.LeafCounts = null;
        soa.AppendCount = 0;
        soa.Length = 0;

        GC.SuppressFinalize(soa);
    }

    ~Soa_Branch()
    {
        Dispose(this);
    }

}