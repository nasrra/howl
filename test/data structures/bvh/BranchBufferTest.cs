using Xunit;
using Howl.DataStructures.Bvh;
using static Howl.DataStructures.Bvh.BranchBuffer;
using Howl.Test.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class BranchBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 4;
        BranchBuffer buffer = new(capacity);
        Assert.Equal(capacity, buffer.Aabbs.MinX.Length);
        Assert.Equal(capacity, buffer.Aabbs.MinY.Length);
        Assert.Equal(capacity, buffer.Aabbs.MaxX.Length);
        Assert.Equal(capacity, buffer.Aabbs.MaxY.Length);
        Assert.Equal(capacity, buffer.LeftLeafIndices.Length);
        Assert.Equal(capacity, buffer.RightLeafIndices.Length);
        Assert.Equal(capacity, buffer.SubtreeSizes.Length);
        Assert.Equal(capacity, buffer.LeafCounts.Length);
        Assert.Equal(0, buffer.Count);
        Assert.False(buffer.Disposed);
    }

    [Fact]
    public void AppendBranch_Test()
    {
        BranchBuffer buffer = new(10);

        int j = 0;
        for(int i = 0; i < 2; i++)
        {
            float minX = j++;
            float minY = j++;
            float maxX = j++;
            float maxY = j++;
            int leftLeafIndex = j++;
            int rightLeafIndex = j++;
            int subtreeSize = j++;
            int leafCount = j++;
            AppendBranch(buffer, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
            BranchBufferAssert.EntryEquals(buffer, i, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
            Assert.Equal(i+1, buffer.Count);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        BranchBuffer buffer = new(10);

        int j = 0;
        for(int i = 0; i < 2; i++)
        {
            float minX = j++;
            float minY = j++;
            float maxX = j++;
            float maxY = j++;
            int leftLeafIndex = j++;
            int rightLeafIndex = j++;
            int subtreeSize = j++;
            int leafCount = j++;
            AppendBranch(buffer, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
        }        
        Assert.Equal(2, buffer.Count);
        Clear(buffer);
        Assert.Equal(0, buffer.Count);
    }
}