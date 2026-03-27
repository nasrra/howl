using Xunit;
using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class Soa_BoundingVolumeHierarchyTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 9;
        Soa_BoundingVolumeHierarchy bvh = new(capacity);
        LeafBufferAssert.LengthEqual(capacity, bvh.Leaves);
        BranchBufferAssert.LengthEqual(capacity*2, bvh.Branches);
    }

    [Fact]
    public void AppendLeaf_Test()
    {
        Soa_BoundingVolumeHierarchy bvh = new(4);
        int j = 0;
        for(int i = 0 ; i < 2; i++)
        {
            float minX = j++;
            float minY = j++; 
            float maxX = j++; 
            float maxY = j++; 
            int index = j++; 
            int generation = j++; 
            int flags = j++;

            LeafBuffer.AppendLeaf(bvh.Leaves, minX, minY, maxX, maxY, index, generation, flags);
            LeafBufferAssert.EntryEqual(minX, minY, maxX, maxY, index, generation, flags, i, bvh.Leaves);
            Assert.Equal(i+1, bvh.Leaves.Count);
        }
    }

    [Fact]
    public void AppendBranch_Test()
    {
        Soa_BoundingVolumeHierarchy bvh = new(4);
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
            BranchBuffer.AppendBranch(bvh.Branches, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, 
                subtreeSize, leafCount
            );
            BranchBufferAssert.EntryEqual(minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, 
                subtreeSize, leafCount, i, bvh.Branches);
            Assert.Equal(i+1, bvh.Branches.Count);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        int appendCount = 2;
        Soa_BoundingVolumeHierarchy bvh = new(4);
        int j = 0;
        for(int i = 0 ; i < appendCount; i++)
        {
            float minX = j++;
            float minY = j++; 
            float maxX = j++; 
            float maxY = j++; 
            int index = j++; 
            int generation = j++; 
            int flags = j++;
            LeafBuffer.AppendLeaf(bvh.Leaves, minX, minY, maxX, maxY, index, generation, flags);
        }

        j = 0;
        for(int i = 0 ; i < appendCount; i++)
        {
            float minX = j++; 
            float minY = j++; 
            float maxX = j++; 
            float maxY = j++; 
            int leftLeafIndex = j++;
            int rightLeafIndex = j++; 
            int subtreeSize = j++;
            int leafCount = j++;
            BranchBuffer.AppendBranch(bvh.Branches, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
        }

        Assert.Equal(appendCount, bvh.Leaves.Count);
        Assert.Equal(appendCount, bvh.Branches.Count);
        Soa_BoundingVolumeHierarchy.Clear(bvh);
        Assert.Equal(0, bvh.Leaves.Count);
        Assert.Equal(0, bvh.Branches.Count);
    }
}