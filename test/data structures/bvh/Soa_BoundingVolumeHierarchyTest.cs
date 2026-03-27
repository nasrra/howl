using Xunit;
using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class Soa_BoundingVolumeHierarchyTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            Soa_BoundingVolumeHierarchy bvh = new(capacity);
            LeafBufferAssert.LengthEqual(capacity, bvh.Leaves);
            BranchBufferAssert.LengthEqual(capacity*2, bvh.Branches);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            Soa_BoundingVolumeHierarchy bvh = new(capacity);
            int j = 0;
            for(int i = 0 ; i < capacity; i++)
            {
                float minX = j++;
                float minY = j++; 
                float maxX = j++; 
                float maxY = j++; 
                int index = j++; 
                int generation = j++; 
                int flags = j++;
                LeafBuffer.Append(bvh.Leaves, minX, minY, maxX, maxY, index, generation, flags);
            }

            j = 0;
            for(int i = 0 ; i < capacity; i++)
            {
                float minX = j++; 
                float minY = j++; 
                float maxX = j++; 
                float maxY = j++; 
                int leftLeafIndex = j++;
                int rightLeafIndex = j++; 
                int subtreeSize = j++;
                int leafCount = j++;
                BranchBuffer.Append(bvh.Branches, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
            }

            Assert.Equal(capacity, bvh.Leaves.Count);
            Assert.Equal(capacity, bvh.Branches.Count);
            Soa_BoundingVolumeHierarchy.Clear(bvh);
            Assert.Equal(0, bvh.Leaves.Count);
            Assert.Equal(0, bvh.Branches.Count);
        }
    }
}