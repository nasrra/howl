using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public static class BranchBufferAssert
{
    /// <summary>
    /// Asserts that the values of a buffer entry are equal to the expected values. 
    /// </summary>
    /// <param name="buffer">the buffer to assert.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to check equality against.</param>
    /// <param name="minX">the expected minimum x value.</param>
    /// <param name="minY">the expected minimum y value.</param>
    /// <param name="maxX">the expected maximum x value.</param>
    /// <param name="maxY">the expected maximum y value.</param>
    /// <param name="leftLeafIndex">the expected left leaf index.</param>
    /// <param name="rightLeafIndex">the expected right leaf index.</param>
    /// <param name="subtreeSize">the expected sub tree size value.</param>
    /// <param name="leafCount">the expected leaf count value.</param>
    public static void EntryEquals(BranchBuffer buffer, int entryIndex, float minX, float minY, float maxX, float maxY, int leftLeafIndex, int rightLeafIndex,
        int subtreeSize, int leafCount
    )
    {
        Assert.Equal(minX, buffer.Aabbs.MinX[entryIndex]);
        Assert.Equal(minY, buffer.Aabbs.MinY[entryIndex]);
        Assert.Equal(maxX, buffer.Aabbs.MaxX[entryIndex]);
        Assert.Equal(maxY, buffer.Aabbs.MaxY[entryIndex]);
        Assert.Equal(leftLeafIndex, buffer.LeftLeafIndices[entryIndex]);
        Assert.Equal(rightLeafIndex, buffer.RightLeafIndices[entryIndex]);
        Assert.Equal(subtreeSize, buffer.SubtreeSizes[entryIndex]);
        Assert.Equal(leafCount, buffer.LeafCounts[entryIndex]);
    } 
}