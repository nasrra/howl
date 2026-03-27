using Xunit;
using Howl.DataStructures.Bvh;
using Howl.Test.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class BranchBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
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
    }

    [Fact]
    public void AppendBranch_Test()
    {
        for(int capacity = 0; capacity < 24; capacity++)
        {            
            BranchBuffer buffer = new(capacity);

            int j = 0;
            for(int i = 0; i < capacity; i++)
            {
                float minX = j++;
                float minY = j++;
                float maxX = j++;
                float maxY = j++;
                int leftLeafIndex = j++;
                int rightLeafIndex = j++;
                int subtreeSize = j++;
                int leafCount = j++;
                BranchBuffer.Append(buffer, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
                BranchBufferAssert.EntryEqual(minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount, i, buffer);
                Assert.Equal(i+1, buffer.Count);
            }
        }
    }

    [Fact]
    public void Clear_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            BranchBuffer buffer = new(capacity);

            int j = 0;
            for(int i = 0; i < capacity; i++)
            {
                float minX = j++;
                float minY = j++;
                float maxX = j++;
                float maxY = j++;
                int leftLeafIndex = j++;
                int rightLeafIndex = j++;
                int subtreeSize = j++;
                int leafCount = j++;
                BranchBuffer.Append(buffer, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
            }        
            Assert.Equal(capacity, buffer.Count);
            BranchBuffer.Clear(buffer);
            Assert.Equal(0, buffer.Count);
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        BranchBuffer buffer = new(12);
        
        BranchBuffer.Dispose(buffer);
        
        Assert.Null(buffer.Aabbs);
        Assert.Null(buffer.LeftLeafIndices);
        Assert.Null(buffer.RightLeafIndices);
        Assert.Null(buffer.SubtreeSizes);
        Assert.Null(buffer.LeafCounts);
        Assert.Equal(0, buffer.Count);
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Disposed);
    }
}