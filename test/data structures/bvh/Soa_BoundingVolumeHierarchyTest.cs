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
            Soa_BoundingVolumeHierarchy bvh = new(length, 1024);

            RadixSortBufferAssert.LengthEqual(length, bvh.RadixSortBuffer);
            Soa_SpatialPairAssert.LengthEqual(1024, bvh.SpatialPairs);
            Soa_BranchAssert.LengthEqual(length*2, bvh.Branches);
            Soa_LeafAssert.LengthEqual(length, bvh.Leaves);
            Soa_QueryResultAssert.LengthEqual(1024, bvh.SpatialPairQueryBuffer);
            Soa_Vector2Assert.LengthEqual(length, bvh.LeafCentroids);
            Assert.Equal(length, bvh.MortonCentroids.Length);
            Assert.Equal(length, bvh.CentroidLeafIds.Length);
            
            Assert.False(bvh.Disposed);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            Soa_BoundingVolumeHierarchy bvh = new(length, 1024);
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
    public void AreaQuery_Test()
    {
        int length = 6;
        Soa_QueryResult results = new(length*2);
        Soa_BoundingVolumeHierarchy bvh = new(length, 1024);
        
        for(int i = 0; i < length; i++)
        {
            Soa_Leaf.Append(bvh.Leaves, i, i, i+1f, i+1f, i+1, i+2, i+3);
        }
        
        Soa_BoundingVolumeHierarchy.ConstructTree(bvh);

        // bvh query.
        Soa_BoundingVolumeHierarchy.AreaQuery(bvh, results, 0,0,2,2);
        Assert.Equal(2,results.AppendCount);
        Soa_QueryResultAssert.EntryEquals(1,2,3,0,results);
        Soa_QueryResultAssert.EntryEquals(2,3,4,1,results);
    
        // decomposed bvh query.
        Soa_BoundingVolumeHierarchy.AreaQuery(bvh.Branches, bvh.Leaves, results, 2,2,5,5);
        Assert.Equal(3,results.AppendCount);
        Soa_QueryResultAssert.EntryEquals(3,4,5,0,results);
        Soa_QueryResultAssert.EntryEquals(4,5,6,1,results);
        Soa_QueryResultAssert.EntryEquals(5,6,7,2,results);
    }

    [Fact]
    public void ConstructSpatialPairs_Test()
    {        
        int length = 5;
        Soa_QueryResult results = new(length*2);
        Soa_BoundingVolumeHierarchy bvh = new(length, 1024);
        
        Span<int> eOwnerIndices     = [1,2,3,4];
        Span<int> eOwnerGenerations = [2,3,4,5];
        Span<int> eOwnerFlags       = [3,4,5,6];
        Span<int> eOtherIndices     = [2,3,4,5];
        Span<int> eOtherGenerations = [3,4,5,6];
        Span<int> eOtherFlags       = [4,5,6,7];

        // == spatial pairs found. ==

        for(int i = 0; i < length; i++)
        {
            Soa_Leaf.Append(bvh.Leaves, i, i, i+1.5f, i+1.5f, i+1, i+2, i+3);
        }        

        Soa_BoundingVolumeHierarchy.ConstructTree(bvh);

        Soa_BoundingVolumeHierarchy.ConstructSpatialPairs(bvh.Branches, bvh.Leaves, bvh.SpatialPairs, results);
    
        Assert.Equal(4, bvh.SpatialPairs.AppendCount);
        for(int i = 0; i < bvh.SpatialPairs.AppendCount; i++)
        {
            Assert.Equal(eOwnerIndices[i], bvh.SpatialPairs.OwnerGenIndices.Indices[i]);
            Assert.Equal(eOwnerGenerations[i], bvh.SpatialPairs.OwnerGenIndices.Generations[i]);
            Assert.Equal(eOwnerFlags[i], bvh.SpatialPairs.OwnerFlags[i]);
            Assert.Equal(eOtherIndices[i], bvh.SpatialPairs.OtherGenIndices.Indices[i]);
            Assert.Equal(eOtherGenerations[i], bvh.SpatialPairs.OtherGenIndices.Generations[i]);
            Assert.Equal(eOtherFlags[i], bvh.SpatialPairs.OtherFlags[i]);
        }

        // == no spatial pairs found. ==
        
        Soa_BoundingVolumeHierarchy.Clear(bvh);

        for(int i = 0; i < length; i++)
        {
            Soa_Leaf.Append(bvh.Leaves, i, i, i, i, i+1, i+2, i+3);
        }

        Soa_BoundingVolumeHierarchy.ConstructSpatialPairs(bvh.Branches, bvh.Leaves, bvh.SpatialPairs, results);

        Assert.Equal(0, bvh.SpatialPairs.AppendCount);
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
        
        Soa_BoundingVolumeHierarchy bvh = new(12, 1024);
        int j = 0;

        for(int q = 0; q < 2; q++)
        {
            Soa_BoundingVolumeHierarchy.Clear(bvh);

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
            int[] eSubtreeSize      = [5,1,3,1,1];
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

    }

    [Fact]
    public void Dispose_Test()
    {
        for(int length = 0; length < 6; length++)
        {
            Soa_BoundingVolumeHierarchy bvh = new(length, 1024);
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
            Assert.Null(bvh.SpatialPairQueryBuffer);
            Assert.Null(bvh.LeafCentroids);
            Assert.Null(bvh.MortonCentroids);
            Assert.Null(bvh.CentroidLeafIds);

            Assert.True(bvh.Disposed);
        }
    }
}