using Xunit;
using Howl.DataStructures.Bvh;
using Howl.Math;
using Howl.Test.Math;
using Howl.Math.Shapes;
using Howl.DataStructures;
using Howl.Ecs;
using Howl.Test.Physics;

namespace Howl.Test.DataStructures.Bvh;

public class Test_BoundingVolumeHierarchy
{
    public const int MaxOverlaps = 65535;
    
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 6; length++)
        {            
            BoundingVolumeHierarchy bvh = new(length, MaxOverlaps);
            int doubleLength = length*2;

            RadixSortBufferAssert.LengthEqual(length, bvh.RadixSortBuffer);
            Assert_Soa_Branch.LengthEqual(doubleLength, bvh.Branches);
            Assert_Soa_Overlap.LengthEqual(MaxOverlaps, bvh.Overlaps);
            Assert_Soa_Leaf.LengthEqual(length, bvh.Leaves);
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
            BoundingVolumeHierarchy bvh = new(length, MaxOverlaps);
            for(int i = 0 ; i < length; i++)
            {
                Soa_Leaf.Append(bvh.Leaves, 0,0,0,0,0,0);
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
        
        BoundingVolumeHierarchy bvh = new(12, MaxOverlaps);
        for(int q = 0; q < 2; q++)
        {
            BoundingVolumeHierarchy.Clear(bvh);

            // leaf 0.
            float minX0 = 0;
            float minY0 = 0;
            float maxX0 = 1;
            float maxY0 = 2;
            Aabb.CalculateCentroid(minX0, minY0, maxX0, maxY0, out float centroidX0, out float centroidY0);
            Soa_Leaf.Append(bvh.Leaves, minX0, minY0, maxX0, maxY0, centroidX0, centroidY0);

            // leaf 1.
            float minX1 = -10;
            float minY1 = -12;
            float maxX1 = 3;
            float maxY1 = 3;
            Aabb.CalculateCentroid(minX1, minY1, maxX1, maxY1, out float centroidX1, out float centroidY1);
            Soa_Leaf.Append(bvh.Leaves, minX1, minY1, maxX1, maxY1, centroidX1, centroidY1);
            
            // leaf 2.
            float minX2 = 10;
            float minY2 = 12;
            float maxX2 = 33;
            float maxY2 = 34;
            Aabb.CalculateCentroid(minX2, minY2, maxX2, maxY2, out float centroidX2, out float centroidY2);
            Soa_Leaf.Append(bvh.Leaves, minX2, minY2, maxX2, maxY2, centroidX2, centroidY2);
            
            // leaf 3.
            float minX3 = 100;
            float minY3 = 102;
            float maxX3 = 123;
            float maxY3 = 124;
            Aabb.CalculateCentroid(minX3, minY3, maxX3, maxY3, out float centroidX3, out float centroidY3);
            Soa_Leaf.Append(bvh.Leaves, minX3, minY3, maxX3, maxY3, centroidX3, centroidY3);

            // leaf 4.
            float minX4 = 200;
            float minY4 = 220;
            float maxX4 = 430;
            float maxY4 = 440;
            Aabb.CalculateCentroid(minX4, minY4, maxX4, maxY4, out float centroidX4, out float centroidY4);
            Soa_Leaf.Append(bvh.Leaves, minX4, minY4, maxX4, maxY4, centroidX4, centroidY4);

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
                Assert_Soa_Branch.EntryEqual(eMinX[i], eMinY[i], eMaxX[i], eMaxY[i], eLeftLeafIndices[i], 
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
    public void ConstructOverlaps_Test()
    {        
        int length = 5;
        BoundingVolumeHierarchy bvh = new(length, MaxOverlaps);
        
        Span<int> eOwnerLeafIndices = [0,1,2,3];
        Span<int> eOtherLeafIndices = [1,2,3,4];

        // == spatial pairs found. ==

        for(int i = 0; i < length; i++)
        {
            float minX = i; 
            float minY = i; 
            float maxX = i+1.5f; 
            float maxY = i+1.5f; 
            Aabb.CalculateCentroid(minX, minY, maxX, maxY, out float centroidX, out float centroidY);
            Soa_Leaf.Append(bvh.Leaves, minX, minY, maxX, maxY, centroidX, centroidY);
        }        

        BoundingVolumeHierarchy.ConstructTree(bvh);
    
        Assert.Equal(4, bvh.Overlaps.AppendCount);

        for(int i = 0; i < bvh.Overlaps.AppendCount; i++)
        {
            Assert_Soa_Overlap.EntryEqual(eOwnerLeafIndices[i], eOtherLeafIndices[i], i, bvh.Overlaps);
        }

        // == no spatial pairs found. ==
        
        BoundingVolumeHierarchy.Clear(bvh);

        for(int i = 0; i < length; i++)
        {
            Soa_Leaf.Append(bvh.Leaves, i, i, i, i, i, i);
        }

        BoundingVolumeHierarchy.ConstructTree(bvh);

        Assert.Equal(0, bvh.Overlaps.AppendCount);
    }

    [Fact]
    public void AreaQuery_Test()
    {
        int[] overlaps = new int[MaxOverlaps];
        int appendedOverlaps = 0;

        int length = 6;
        BoundingVolumeHierarchy bvh = new(length, MaxOverlaps);
        
        for(int i = 0; i < length; i++)
        {
            float minX = i; 
            float minY = i; 
            float maxX = i+1; 
            float maxY = i+1; 
            Aabb.CalculateCentroid(minX, minY, maxX, maxY, out float centroidX, out float centroidY);
            Soa_Leaf.Append(bvh.Leaves, minX, minY, maxX, maxY, centroidX, centroidY);
        }
        
        BoundingVolumeHierarchy.ConstructTree(bvh);

        // bvh query.
        BoundingVolumeHierarchy.AreaQuery(bvh, overlaps, ref appendedOverlaps, 0,0,2,2);
        Assert.Equal(2, appendedOverlaps);
        Assert.Equal(0, overlaps[0]);
        Assert.Equal(1, overlaps[1]);
    
        // decomposed bvh query.
        BoundingVolumeHierarchy.AreaQuery(bvh, overlaps, ref appendedOverlaps, 2,2,5,5);
        Assert.Equal(3, appendedOverlaps);
        Assert.Equal(2, overlaps[0]);
        Assert.Equal(3, overlaps[1]);
        Assert.Equal(4, overlaps[2]);
    }

    [Fact]
    public void RaycastQuery_Test()
    {
        int[] overlaps = new int[MaxOverlaps];
        int appendedOverlaps = 0;
        BoundingVolumeHierarchy bvh = new(100, MaxOverlaps);

        // leaf 1 
        Aabb leaf1AABB = new Aabb(0,0,10,10);
        Vector2 leaf1Centroid = Aabb.CalculateCentroid(leaf1AABB);
        Soa_Leaf.Append(bvh.Leaves, leaf1AABB.MinX, leaf1AABB.MinY, leaf1AABB.MaxX, leaf1AABB.MaxY, leaf1Centroid.X, leaf1Centroid.Y);

        // leaf 2
        Aabb leaf2AABB = new Aabb(10,10,20,20);
        Vector2 leaf2Centroid = Aabb.CalculateCentroid(leaf2AABB);
        Soa_Leaf.Append(bvh.Leaves, leaf2AABB.MinX, leaf2AABB.MinY, leaf2AABB.MaxX, leaf2AABB.MaxY, leaf2Centroid.X, leaf2Centroid.Y);

        BoundingVolumeHierarchy.ConstructTree(bvh);

        // fail to interset.
        BoundingVolumeHierarchy.RaycastQuery(bvh, overlaps, ref appendedOverlaps, -1, -1, -10, -10);
        Assert.Equal(0, appendedOverlaps);

        // find single intersect.
        BoundingVolumeHierarchy.RaycastQuery(bvh, overlaps, ref appendedOverlaps, 5, 0, 5, 30);
        Assert.Equal(1, appendedOverlaps);
        Assert.Equal(0, overlaps[0]);

        // find double intersect.
        BoundingVolumeHierarchy.RaycastQuery(bvh, overlaps, ref appendedOverlaps, 0, 0, 40, 40);
        Assert.Equal(2, appendedOverlaps);
        Assert.Equal(0, overlaps[0]);
        Assert.Equal(1, overlaps[1]);
    }

    [Fact]
    public void Dispose_Test()
    {
        for(int length = 0; length < 6; length++)
        {
            BoundingVolumeHierarchy bvh = new(length, MaxOverlaps);
            for(int i = 0; i < length; i++)
            {
                Soa_Leaf.Append(bvh.Leaves, 0,0,0,0,0,0);
                Soa_Branch.Append(bvh.Branches, 0,0,0,0,0,0,0,0,0);
            }

            BoundingVolumeHierarchy.Dispose(bvh);
            
            Assert.Null(bvh.RadixSortBuffer);                
            Assert.Null(bvh.Overlaps);
            Assert.Null(bvh.Branches);
            Assert.Null(bvh.Leaves);
            Assert.Null(bvh.MortonCentroids);
            Assert.Null(bvh.MortonLeafIds);

            Assert.True(bvh.Disposed);
        }
    }
}