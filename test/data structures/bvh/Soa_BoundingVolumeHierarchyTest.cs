using Xunit;
using Howl.DataStructures.Bvh;
using Howl.Math;
using Howl.Test.Math;

namespace Howl.Test.DataStructures.Bvh;

public class Soa_BoundingVolumeHierarchyTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 6; length++)
        {            
            Soa_BoundingVolumeHierarchy bvh = new(length);

            RadixSortBufferAssert.LengthEqual(length, bvh.RadixSortBuffer);
            SpatialPairBufferAssert.LengthEqual(length, bvh.SpatialPairs);
            Soa_BranchAssert.LengthEqual(length*2, bvh.Branches);
            Soa_LeafAssert.LengthEqual(length, bvh.Leaves);
            Soa_Vector2Assert.LengthEqual(length, bvh.LeafCentroids);
            Assert.Equal(length, bvh.LeafCentroidsSwapBuffer.Length);
            Assert.Equal(length, bvh.CentroidLeafIds.Length);
            
            Assert.False(bvh.Disposed);
        }
    }

    [Fact]
    public void ConstructTree_Test()
    {
        // At the end of this tree construction.
        // leaves 0 and 1 should be together.
        // leaf 2 should be in a branch of its own.
        // leaves 3 and 4 should be together.
        // 
        // five branches should have also been created.
        // the structure should look something like this:
        // 
        //  -Root
        //      -Branch (leaf 0-1)
        //      -Branch
        //          -Branch (leaf 2)
        //          - Branch (leaf 3-4)
        //
        
        Soa_BoundingVolumeHierarchy bvh = new(12);
        int j = 0;

        // together.
        Soa_Leaf.Append(bvh.Leaves, 0, 0, 1, 2, j++, j++, j++); // leaf 0.
        Soa_Leaf.Append(bvh.Leaves, -10, -12, 3, 3,  j++, j++, j++); // leaf 1.
        // solo.
        Soa_Leaf.Append(bvh.Leaves, 10, 12, 33, 34, j++, j++, j++); // leaf 2.
        // together.
        Soa_Leaf.Append(bvh.Leaves, 100, 102, 123, 124,  j++, j++, j++); // leaf 3.
        Soa_Leaf.Append(bvh.Leaves, 200, 220, 430, 440,  j++, j++, j++); // leaf 4.


        // expected min and maxes of the constructed aabbs.
        float[] eMinX           = [-10,-10,10,10,100];
        float[] eMinY           = [-12,-12,12,12,102];
        float[] eMaxX           = [430,3,430,33,430];
        float[] eMaxY           = [440,3,440,34,440];
        int[] eLeftLeafIndices  = [0,1,0,2,3];
        int[] eRightLeafIndices = [0,0,0,0,4];
        int[] eSubtreeSize      = [5,0,3,0,0];
        int[] eLeafCount        = [0,2,0,1,2];

        // construct.
        Soa_BoundingVolumeHierarchy.ConstructTree(bvh);
        
        // check construction output.
        Assert.Equal(5, bvh.Branches.AppendCount);
        for(int i = 0; i < bvh.Branches.AppendCount; i++)
        {
            Soa_BranchAssert.EntryEqual(eMinX[i], eMinY[i], eMaxX[i], eMaxY[i], eLeftLeafIndices[i], eRightLeafIndices[i], eSubtreeSize[i], eLeafCount[i], i, bvh.Branches);
        } 
    }

    [Fact]
    public void Clear_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            Soa_BoundingVolumeHierarchy bvh = new(length);
            int j = 0;
            for(int i = 0 ; i < length; i++)
            {
                float minX = j++;
                float minY = j++; 
                float maxX = j++; 
                float maxY = j++; 
                int index = j++; 
                int generation = j++; 
                int flags = j++;
                Soa_Leaf.Append(bvh.Leaves, minX, minY, maxX, maxY, index, generation, flags);

                int leftLeafIndex = j++;
                int rightLeafIndex = j++; 
                int subtreeSize = j++;
                int leafCount = j++;
                Soa_Branch.Append(bvh.Branches, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
            }

            Assert.Equal(length, bvh.Leaves.AppendCount);
            Assert.Equal(length, bvh.Branches.AppendCount);
            Soa_BoundingVolumeHierarchy.Clear(bvh);
            Assert.Equal(0, bvh.Leaves.AppendCount);
            Assert.Equal(0, bvh.Branches.AppendCount);
        }
    }

    [Fact]
    public void Dispose_Test()
    {
        for(int length = 0; length < 6; length++)
        {
            Soa_BoundingVolumeHierarchy bvh = new(length);
            int j = 0;
            for(int i = 0; i < length; i++)
            {
                float minX = j++;
                float minY = j++; 
                float maxX = j++; 
                float maxY = j++; 
                int index = j++; 
                int generation = j++; 
                int flags = j++;
                Soa_Leaf.Append(bvh.Leaves, minX, minY, maxX, maxY, index, generation, flags);

                int leftLeafIndex = j++;
                int rightLeafIndex = j++; 
                int subtreeSize = j++;
                int leafCount = j++;
                Soa_Branch.Append(bvh.Branches, minX, minY, maxX, maxY, leftLeafIndex, rightLeafIndex, subtreeSize, leafCount);
            }

            Soa_BoundingVolumeHierarchy.Dispose(bvh);
            
            Assert.Null(bvh.RadixSortBuffer);                
            Assert.Null(bvh.SpatialPairs);
            Assert.Null(bvh.Branches);
            Assert.Null(bvh.Leaves);
            Assert.Null(bvh.LeafCentroids);
            Assert.Null(bvh.CentroidLeafIds);
            Assert.Null(bvh.LeafCentroidsSwapBuffer);

            Assert.True(bvh.Disposed);
        }
    }
}