using Howl.DataStructures.Bvh;
using Howl.Test.Math.Shapes;

namespace Howl.Test.DataStructures.Bvh;

public static class Soa_BranchAssert
{
    /// <summary>
    /// Asserts that the values of a soa entry are equal to the expected values. 
    /// </summary>
    /// <param name="minX">the expected minimum x value.</param>
    /// <param name="minY">the expected minimum y value.</param>
    /// <param name="maxX">the expected maximum x value.</param>
    /// <param name="maxY">the expected maximum y value.</param>
    /// <param name="leftLeafIndex">the expected left leaf index.</param>
    /// <param name="rightLeafIndex">the expected right leaf index.</param>
    /// <param name="subtreeSize">the expected sub tree size value.</param>
    /// <param name="leafCount">the expected leaf count value.</param>
    /// <param name="parentIndex">the expected parent index</param>
    /// <param name="entryIndex">the index of the entry in the buffer to assert equality against.</param>
    /// <param name="soa">the soa instance. containing the entry to assert.</param>
    public static void EntryEqual(float minX, float minY, float maxX, float maxY, int leftLeafIndex, int rightLeafIndex,
        int subtreeSize, int leafCount, int parentIndex, int entryIndex, Soa_Branch soa
    )
    {
        Soa_AabbAssert.EntryEqual(minX, minY, maxX, maxY, entryIndex, soa.Aabbs);
        Assert.Equal(leftLeafIndex, soa.LeftLeafIndices[entryIndex]);
        Assert.Equal(rightLeafIndex, soa.RightLeafIndices[entryIndex]);
        Assert.Equal(subtreeSize, soa.SubtreeSizes[entryIndex]);
        Assert.Equal(leafCount, soa.LeafCounts[entryIndex]);
        Assert.Equal(parentIndex, soa.ParentIndices[entryIndex]);
    } 

    /// <summary>
    /// Asserts the equality of array lengths in a soa instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="soa">the soa instance instance.</param>
    public static void LengthEqual(int length, Soa_Branch soa)
    {
        Soa_AabbAssert.LengthEqual(length, soa.Aabbs);
        Assert.Equal(length, soa.LeftLeafIndices.Length);
        Assert.Equal(length, soa.RightLeafIndices.Length);
        Assert.Equal(length, soa.SubtreeSizes.Length);
        Assert.Equal(length, soa.LeafCounts.Length);
        Assert.Equal(length, soa.ParentIndices.Length);
        Assert.Equal(length, soa.Length);
    }
}