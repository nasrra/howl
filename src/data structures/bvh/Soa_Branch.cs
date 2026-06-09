using Howl.Math.Shapes;
using Howl.Unmanaged.Collections;

namespace Howl.Text.Bvh;

public struct Soa_Branch
{
    /// <summary>
    ///     The Axis-Aligned Bounding-Boxes of all branches.
    /// </summary>
    public Soa_Aabb Aabbs;

    /// <summary>
    ///     The left leaf indices of all branches.
    /// </summary>
    public Array<int> LeftLeafIndices;

    /// <summary>
    ///     The right leaf indices of all branches.
    /// </summary>
    public Array<int> RightLeafIndices;

    /// <summary>
    ///     The number of child branches (including the branch itself) of all branches.
    /// </summary>
    /// <remarks>
    ///     E.g a branch that has three 4 children will have a subtree size of 5; as the subtree size counts the branch as well.
    /// </remarks>
    public Array<int> SubtreeSizes;

    /// <summary>
    ///     The amount of leaves attatched of all branches.
    /// </summary>
    /// <remarks>
    ///     Specifically, the amount of immediate leaves attatched to a branch; not counting children or parents.
    /// </remarks>
    public Array<int> LeafCounts;

    /// <summary>
    /// The indices for the parent branch of all branches.
    /// </summary>
    public Array<int> ParentIndices;

    /// <summary>
    /// The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;

    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    public bool IsInitialised;

    public static bool Initialise(ref Soa_Branch soa, ref Memory.Arena arena, int length)
    {
        if (soa.IsInitialised)
        {
            Debug.Panic("Already Initialised.");
            return false;
        }

        Soa_Aabb.Initialise(ref soa.Aabbs, ref arena, length);
        Array.Initialise(ref soa.LeftLeafIndices, ref arena, length);
        Array.Initialise(ref soa.RightLeafIndices, ref arena, length);
        Array.Initialise(ref soa.SubtreeSizes, ref arena, length);
        Array.Initialise(ref soa.LeafCounts, ref arena, length);
        Array.Initialise(ref soa.ParentIndices, ref arena, length);
        soa.Length = length;

        soa.IsInitialised = true;
        return true;
    }

    /// <summary>
    /// Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
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
    /// <param name="parentIndex">the index within this soa instance of the branch that this branch is a child of.</param>
    public static void Append(ref Soa_Branch soa, float minX, float minY, float maxX, float maxY, int leftLeafIndex, int rightLeafIndex, 
        int subtreeSize, int leafCount, int parentIndex
    )
    {
        Insert(ref soa, soa.AppendCount, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount, parentIndex);
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
    /// <param name="parentIndex">the index within this soa instance of the branch that this branch is a child of.</param>
    public static void Insert(ref Soa_Branch soa, int insertIndex, float minX, float minY, float maxX, float maxY, int leftLeafIndex, int rightLeafIndex, 
        int subtreeSize, int leafCount, int parentIndex
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
        soa.ParentIndices[insertIndex] = parentIndex;
    }

    /// <summary>
    /// Sets a soa's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa instance to clear.</param>
    public static void ResetCount(ref Soa_Branch soa)
    {
        soa.AppendCount = 0;
    }
}