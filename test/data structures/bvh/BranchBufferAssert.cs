using Howl.DataStructures.Bvh;
using Howl.Test.Math.Shapes;

namespace Howl.Test.DataStructures.Bvh;

public static class BranchBufferAssert
{
    /// <summary>
    /// Asserts that the values of a buffer entry are equal to the expected values. 
    /// </summary>
    /// <param name="minX">the expected minimum x value.</param>
    /// <param name="minY">the expected minimum y value.</param>
    /// <param name="maxX">the expected maximum x value.</param>
    /// <param name="maxY">the expected maximum y value.</param>
    /// <param name="leftLeafIndex">the expected left leaf index.</param>
    /// <param name="rightLeafIndex">the expected right leaf index.</param>
    /// <param name="subtreeSize">the expected sub tree size value.</param>
    /// <param name="leafCount">the expected leaf count value.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to assert equality against.</param>
    /// <param name="buffer">the buffer containing the entry to assert.</param>
    public static void EntryEqual(float minX, float minY, float maxX, float maxY, int leftLeafIndex, int rightLeafIndex,
        int subtreeSize, int leafCount, int entryIndex, BranchBuffer buffer
    )
    {
        Soa_AabbAssert.EntryEqual(minX, minY, maxX, maxY, entryIndex, buffer.Aabbs);
        Assert.Equal(leftLeafIndex, buffer.LeftLeafIndices[entryIndex]);
        Assert.Equal(rightLeafIndex, buffer.RightLeafIndices[entryIndex]);
        Assert.Equal(subtreeSize, buffer.SubtreeSizes[entryIndex]);
        Assert.Equal(leafCount, buffer.LeafCounts[entryIndex]);
    } 

    /// <summary>
    /// Asserts the equality of array lengths in a buffer instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="buffer">the buffer instance.</param>
    public static void LengthEqual(int length, BranchBuffer buffer)
    {
        Soa_AabbAssert.LengthEqual(length, buffer.Aabbs);
        Assert.Equal(length, buffer.LeftLeafIndices.Length);
        Assert.Equal(length, buffer.RightLeafIndices.Length);
        Assert.Equal(length, buffer.SubtreeSizes.Length);
        Assert.Equal(length, buffer.LeafCounts.Length);
    }
}