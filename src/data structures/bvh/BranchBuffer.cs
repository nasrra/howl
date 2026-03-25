using System;
using Howl.Math.Shapes;

namespace Howl.DataStructures.Bvh;

public class BranchBuffer : IDisposable
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
    /// the amount of allocated entries; starting from index 0.
    /// </summary>
    public int Count;

    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new branch buffer instance.
    /// </summary>
    /// <param name="capacity">the capcity of the backing arrays.</param>
    public BranchBuffer(int capacity)
    {
        Aabbs = new(capacity);
        LeftLeafIndices = new int[capacity];
        RightLeafIndices = new int[capacity];
        SubtreeSizes = new int[capacity];
        LeafCounts = new int[capacity];
    }

    /// <summary>
    /// Appends a branch to a buffer.
    /// </summary>
    /// <param name="buffer">the buffer to append to.</param>
    /// <param name="minX">the x-component of the minimum vertex.</param>
    /// <param name="minY">the y-component of the minimum vertex.</param>
    /// <param name="maxX">the x-component of the minimum vertex.</param>
    /// <param name="maxY">the y-component of the minimum vertex.</param>
    /// <param name="leftLeafIndex">the index of the left leaf.</param>
    /// <param name="rightLeafIndex">the index of the right leaf.</param>
    /// <param name="subtreeSize">the subtree size.</param>
    /// <param name="leafCount">the amount of leaves attached to the branch.</param>
    public static void AppendBranch(BranchBuffer buffer, float minX, float minY, float maxX, float maxY, int leftLeafIndex, int rightLeafIndex, 
        int subtreeSize, int leafCount
    )
    {
        int count = buffer.Count;
        buffer.Aabbs.MinX[count] = minX;
        buffer.Aabbs.MinY[count] = minY;
        buffer.Aabbs.MaxX[count] = maxX;
        buffer.Aabbs.MaxY[count] = maxY;
        buffer.LeftLeafIndices[count] = leftLeafIndex;
        buffer.RightLeafIndices[count] = rightLeafIndex;
        buffer.SubtreeSizes[count] = subtreeSize;
        buffer.LeafCounts[count] = leafCount;
        buffer.Count++;
    }

    /// <summary>
    /// Sets a buffer's count to zero.
    /// </summary>
    /// <param name="buffer">the buffer to clear.</param>
    public static void Clear(BranchBuffer buffer)
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

    public static void Dispose(BranchBuffer buffer)
    {
        if(buffer.Disposed)
            return;
        
        buffer.Disposed = true;

        Soa_Aabb.Dispose(buffer.Aabbs);
        buffer.Aabbs = null;
        buffer.LeftLeafIndices = null;
        buffer.RightLeafIndices = null;
        buffer.SubtreeSizes = null;
        buffer.LeafCounts = null;

        GC.SuppressFinalize(buffer);
    }

    ~BranchBuffer()
    {
        Dispose(this);
    }

}