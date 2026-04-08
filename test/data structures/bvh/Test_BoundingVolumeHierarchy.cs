using Xunit;
using Howl.DataStructures.Bvh;
using Howl.Math;
using Howl.Test.Math;
using Howl.Math.Shapes;
using Howl.DataStructures;
using Howl.Ecs;

namespace Howl.Test.DataStructures.Bvh;

public class Test_BoundingVolumeHierarchy
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 6; length++)
        {            
            BoundingVolumeHierarchy bvh = new(length);
            int doubleLength = length*2;

            RadixSortBufferAssert.LengthEqual(length, bvh.RadixSortBuffer);
            Soa_SpatialPairAssert.LengthEqual(doubleLength, bvh.SpatialPairs);
            Soa_BranchAssert.LengthEqual(doubleLength, bvh.Branches);
            Soa_LeafAssert.LengthEqual(length, bvh.Leaves);
            Soa_QueryResultAssert.LengthEqual(doubleLength, bvh.SpatialPairQueryBuffer);
            Assert.Equal(length, bvh.MortonCentroids.Length);
            Assert.Equal(length, bvh.MortonLeafIds.Length);
            
            Assert.False(bvh.Disposed);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            BoundingVolumeHierarchy bvh = new(length);
            for(int i = 0 ; i < length; i++)
            {
                Soa_Leaf.Append(bvh.Leaves, 0,0,0,0,0,0,0,0,0);
                Soa_Branch.Append(bvh.Branches, 0,0,0,0,0,0,0,0,0);
            }

            Assert.Equal(length, bvh.Leaves.AppendCount);
            Assert.Equal(length, bvh.Branches.AppendCount);
            BoundingVolumeHierarchy.Clear(bvh);
            Assert.Equal(0, bvh.Leaves.AppendCount);
            Assert.Equal(0, bvh.Branches.AppendCount);
        }
    }

    [Fact]
    public void AreaQuery_Test()
    {
        int length = 6;
        Soa_QueryResult results = new(length*2);
        BoundingVolumeHierarchy bvh = new(length);
        
        for(int i = 0; i < length; i++)
        {
            float minX = i; 
            float minY = i; 
            float maxX = i+1; 
            float maxY = i+1; 
            int index = i+1;
            int generation = i+2;
            int flags = i+3;
            Aabb.CalculateCentroid(minX, minY, maxX, maxY, out float centroidX, out float centroidY);
            Soa_Leaf.Append(bvh.Leaves, minX, minY, maxX, maxY, centroidX, centroidY, index, generation, flags);
        }
        
        BoundingVolumeHierarchy.ConstructTree(bvh);

        // bvh query.
        BoundingVolumeHierarchy.AreaQuery(bvh, results, 0,0,2,2);
        Assert.Equal(2,results.AppendCount);
        Soa_QueryResultAssert.EntryEquals(1,2,3,0,results);
        Soa_QueryResultAssert.EntryEquals(2,3,4,1,results);
    
        // decomposed bvh query.
        BoundingVolumeHierarchy.AreaQuery(bvh.Branches, bvh.Leaves, results, 2,2,5,5);
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
        BoundingVolumeHierarchy bvh = new(length);
        
        Span<int> eOwnerIndices     = [1,2,3,4];
        Span<int> eOwnerGenerations = [2,3,4,5];
        Span<int> eOwnerFlags       = [3,4,5,6];
        Span<int> eOtherIndices     = [2,3,4,5];
        Span<int> eOtherGenerations = [3,4,5,6];
        Span<int> eOtherFlags       = [4,5,6,7];

        // == spatial pairs found. ==

        for(int i = 0; i < length; i++)
        {
            float minX = i; 
            float minY = i; 
            float maxX = i+1.5f; 
            float maxY = i+1.5f; 
            int index = i+1;
            int generation = i+2;
            int flags = i+3;
            Aabb.CalculateCentroid(minX, minY, maxX, maxY, out float centroidX, out float centroidY);
            Soa_Leaf.Append(bvh.Leaves, minX, minY, maxX, maxY, centroidX, centroidY, index, generation, flags);
        }        

        BoundingVolumeHierarchy.ConstructTree(bvh);

        BoundingVolumeHierarchy.ConstructSpatialPairs(bvh.Branches, bvh.Leaves, bvh.SpatialPairs, results);
    
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
        
        BoundingVolumeHierarchy.Clear(bvh);

        for(int i = 0; i < length; i++)
        {
            Soa_Leaf.Append(bvh.Leaves, i, i, i, i, i, i, i+1, i+2, i+3);
        }

        BoundingVolumeHierarchy.ConstructSpatialPairs(bvh.Branches, bvh.Leaves, bvh.SpatialPairs, results);

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
        
        BoundingVolumeHierarchy bvh = new(12);
        int j = 0;

        for(int q = 0; q < 2; q++)
        {
            BoundingVolumeHierarchy.Clear(bvh);

            // leaf 0.
            float minX0 = 0;
            float minY0 = 0;
            float maxX0 = 1;
            float maxY0 = 2;
            int index0 = j++;
            int gen0 = j++;
            int flags0 = j++;
            Aabb.CalculateCentroid(minX0, minY0, maxX0, maxY0, out float centroidX0, out float centroidY0);
            Soa_Leaf.Append(bvh.Leaves, minX0, minY0, maxX0, maxY0, centroidX0, centroidY0, index0, gen0, flags0);

            // leaf 1.
            float minX1 = -10;
            float minY1 = -12;
            float maxX1 = 3;
            float maxY1 = 3;
            int index1 = j++;
            int gen1 = j++;
            int flags1 = j++;
            Aabb.CalculateCentroid(minX1, minY1, maxX1, maxY1, out float centroidX1, out float centroidY1);
            Soa_Leaf.Append(bvh.Leaves, minX1, minY1, maxX1, maxY1, centroidX1, centroidY1, index1, gen1, flags1);
            
            // leaf 2.
            float minX2 = 10;
            float minY2 = 12;
            float maxX2 = 33;
            float maxY2 = 34;
            int index2 = j++;
            int gen2 = j++;
            int flags2 = j++;
            Aabb.CalculateCentroid(minX2, minY2, maxX2, maxY2, out float centroidX2, out float centroidY2);
            Soa_Leaf.Append(bvh.Leaves, minX2, minY2, maxX2, maxY2, centroidX2, centroidY2, index2, gen2, flags2);
            
            // leaf 3.
            float minX3 = 100;
            float minY3 = 102;
            float maxX3 = 123;
            float maxY3 = 124;
            int index3 = j++;
            int gen3 = j++;
            int flags3 = j++;
            Aabb.CalculateCentroid(minX3, minY3, maxX3, maxY3, out float centroidX3, out float centroidY3);
            Soa_Leaf.Append(bvh.Leaves, minX3, minY3, maxX3, maxY3, centroidX3, centroidY3, index3, gen3, flags3);

            // leaf 4.
            float minX4 = 200;
            float minY4 = 220;
            float maxX4 = 430;
            float maxY4 = 440;
            int index4 = j++;
            int gen4 = j++;
            int flags4 = j++;
            Aabb.CalculateCentroid(minX4, minY4, maxX4, maxY4, out float centroidX4, out float centroidY4);
            Soa_Leaf.Append(bvh.Leaves, minX4, minY4, maxX4, maxY4, centroidX4, centroidY4, index4, gen4, flags4);

            // construct.
            BoundingVolumeHierarchy.ConstructTree(bvh);
            
            // expected branch data.
            float[] eMinX           = [minX1,minX1,minX2,minX2,minX3];
            float[] eMinY           = [minY1,minY1,minY2,minY2,minY3];
            float[] eMaxX           = [maxX4,maxX1,maxX4,maxX2,maxX4];
            float[] eMaxY           = [maxY4,maxY1,maxY4,maxY2,maxY4];
            int[] eLeftLeafIndices  = [0,1,0,2,3];
            int[] eRightLeafIndices = [0,0,0,0,4];
            int[] eSubtreeSizes     = [5,1,3,1,1];
            int[] eLeafCounts       = [0,2,0,1,2];
            int[] eParentIndices    = [0,0,1,1,1];

            // check constructed branch output.
            Assert.Equal(5, bvh.Branches.AppendCount);
            for(int i = 0; i < bvh.Branches.AppendCount; i++)
            {
                Soa_BranchAssert.EntryEqual(eMinX[i], eMinY[i], eMaxX[i], eMaxY[i], eLeftLeafIndices[i], 
                    eRightLeafIndices[i], eSubtreeSizes[i], eLeafCounts[i], eParentIndices[i], i, bvh.Branches
                );
            } 

            // expected leaf data.
            int[] eLeafBranchIndices = [1,1,3,4,4];

            // check constructed leaf output.
            for(int i = 0; i < bvh.Leaves.AppendCount; i++)
            {
                Assert.Equal(eLeafBranchIndices[i], bvh.Leaves.BranchIndices[i]);
            }
        }

    }

    [Fact]
    public void RaycastQuery_Test()
    {
        BoundingVolumeHierarchy bvh = new(100);

        // leaf 1 
        Aabb leaf1AABB = new Aabb(0,0,10,10);
        Vector2 leaf1Centroid = Aabb.CalculateCentroid(leaf1AABB);
        GenIndex leaf1GenIndex = new GenIndex(0,0);
        int leaf1Flag = 0;
        Soa_Leaf.Append(bvh.Leaves, leaf1AABB.MinX, leaf1AABB.MinY, leaf1AABB.MaxX, leaf1AABB.MaxY, leaf1Centroid.X, leaf1Centroid.Y, 
            leaf1GenIndex.Index, leaf1GenIndex.Generation, leaf1Flag
        );

        // leaf 2
        Aabb leaf2AABB = new Aabb(10,10,20,20);
        Vector2 leaf2Centroid = Aabb.CalculateCentroid(leaf2AABB);
        GenIndex leaf2GenIndex = new GenIndex(1,0);
        int leaf2Flag = 0;
        Soa_Leaf.Append(bvh.Leaves, leaf2AABB.MinX, leaf2AABB.MinY, leaf2AABB.MaxX, leaf2AABB.MaxY, leaf2Centroid.X, leaf2Centroid.Y, 
            leaf2GenIndex.Index, leaf2GenIndex.Generation, leaf2Flag
        );

        BoundingVolumeHierarchy.ConstructTree(bvh);

        // fail to interset.
        // Span<QueryResult> zeroResult = Soa_BoundingVolumeHierarchy.RaycastQuery(bvh, new Vector2(-1,-1), new Vector2(-10,-10));
        Soa_QueryResult zeroResult = BoundingVolumeHierarchy.RaycastQuery(bvh, new Vector2(-1,-1), new Vector2(-10,-10));
        Assert.Equal(0, zeroResult.AppendCount);

        // find single intersect.
        Soa_QueryResult singleResult = BoundingVolumeHierarchy.RaycastQuery(bvh, new Vector2(5,0), new Vector2(5,30));
        Assert.Equal(1, singleResult.AppendCount);
        Assert.Equal(leaf1GenIndex.Index, singleResult.GenIndices.Indices[0]);
        Assert.Equal(leaf1GenIndex.Generation, singleResult.GenIndices.Generations[0]);
        Assert.Equal(leaf1Flag, singleResult.Flags[0]);

        // find double intersect.
        Soa_QueryResult doubleResult = BoundingVolumeHierarchy.RaycastQuery(bvh, new Vector2(0,0), new Vector2(40,40));
        Assert.Equal(2, doubleResult.AppendCount);
        Assert.Equal(leaf1GenIndex.Index, doubleResult.GenIndices.Indices[0]);
        Assert.Equal(leaf1GenIndex.Generation, doubleResult.GenIndices.Generations[0]);
        Assert.Equal(leaf1Flag, doubleResult.Flags[0]);
        Assert.Equal(leaf2GenIndex.Index, doubleResult.GenIndices.Indices[1]);
        Assert.Equal(leaf2GenIndex.Generation, doubleResult.GenIndices.Generations[1]);
        Assert.Equal(leaf2Flag, doubleResult.Flags[1]);
    }

    [Fact]
    public void Dispose_Test()
    {
        for(int length = 0; length < 6; length++)
        {
            BoundingVolumeHierarchy bvh = new(length);
            for(int i = 0; i < length; i++)
            {
                Soa_Leaf.Append(bvh.Leaves, 0,0,0,0,0,0,0,0,0);
                Soa_Branch.Append(bvh.Branches, 0,0,0,0,0,0,0,0,0);
            }

            BoundingVolumeHierarchy.Dispose(bvh);
            
            Assert.Null(bvh.RadixSortBuffer);                
            Assert.Null(bvh.SpatialPairs);
            Assert.Null(bvh.Branches);
            Assert.Null(bvh.Leaves);
            Assert.Null(bvh.SpatialPairQueryBuffer);
            Assert.Null(bvh.MortonCentroids);
            Assert.Null(bvh.MortonLeafIds);

            Assert.True(bvh.Disposed);
        }
    }
}