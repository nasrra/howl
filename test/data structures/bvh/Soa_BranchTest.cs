using Xunit;
using Howl.DataStructures.Bvh;
using Howl.Test.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class Soa_BranchTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            Soa_Branch buffer = new(capacity);
            Assert.Equal(capacity, buffer.Aabbs.MinX.Length);
            Assert.Equal(capacity, buffer.Aabbs.MinY.Length);
            Assert.Equal(capacity, buffer.Aabbs.MaxX.Length);
            Assert.Equal(capacity, buffer.Aabbs.MaxY.Length);
            Assert.Equal(capacity, buffer.LeftLeafIndices.Length);
            Assert.Equal(capacity, buffer.RightLeafIndices.Length);
            Assert.Equal(capacity, buffer.SubtreeSizes.Length);
            Assert.Equal(capacity, buffer.LeafCounts.Length);
            Assert.Equal(0, buffer.AppendCount);
            Assert.False(buffer.Disposed);
        }
    }

    [Fact]
    public void Insert_Test()
    {
        for(int length = 0; length < 6; length++)
        {            
            Soa_Branch soa = new(length);

            int j = 0;
            for(int i = 0; i < length; i++)
            {
                float minX = j++;
                float minY = j++;
                float maxX = j++;
                float maxY = j++;
                int leftLeafIndex = j++;
                int rightLeafIndex = j++;
                int subtreeSize = j++;
                int leafCount = j++;
                Soa_Branch.Insert(soa, i, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
                Soa_BranchAssert.EntryEqual(minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount, i, soa);
                Assert.Equal(0, soa.AppendCount);
            }
        }        
    }

    [Fact]
    public void Append_Test()
    {
        for(int length = 0; length < 24; length++)
        {            
            Soa_Branch soa = new(length);

            int j = 0;
            for(int i = 0; i < length; i++)
            {
                float minX = j++;
                float minY = j++;
                float maxX = j++;
                float maxY = j++;
                int leftLeafIndex = j++;
                int rightLeafIndex = j++;
                int subtreeSize = j++;
                int leafCount = j++;
                Soa_Branch.Append(soa, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
                Soa_BranchAssert.EntryEqual(minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount, i, soa);
                Assert.Equal(i+1, soa.AppendCount);
            }
        }
    }

    [Fact]
    public void Clear_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            Soa_Branch buffer = new(capacity);

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
                Soa_Branch.Append(buffer, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
            }        
            Assert.Equal(capacity, buffer.AppendCount);
            Soa_Branch.Clear(buffer);
            Assert.Equal(0, buffer.AppendCount);
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        Soa_Branch buffer = new(12);
        
        Soa_Branch.Dispose(buffer);
        
        Assert.Null(buffer.Aabbs);
        Assert.Null(buffer.LeftLeafIndices);
        Assert.Null(buffer.RightLeafIndices);
        Assert.Null(buffer.SubtreeSizes);
        Assert.Null(buffer.LeafCounts);
        Assert.Equal(0, buffer.AppendCount);
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Disposed);
    }
}