using Xunit;
using Howl.DataStructures.Bvh;
using Howl.Math;

namespace Howl.Test.DataStructures.Bvh;

public class Test_Soa_Leaf
{
    [Fact]
    public void Constructor_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            Soa_Leaf buffer = new(capacity);
            Assert_Soa_Leaf.LengthEqual(capacity, buffer);
            Assert.Equal(0, buffer.AppendCount);
            Assert.False(buffer.Disposed);
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int length = 0; length < 7; length++)
        {            
            Soa_Leaf soa = new(length);
            int j = 0;
            
            // contiguous append test.
            for(int i = 0; i < length; i++)
            {            
                float minX = j++;
                float minY = j++;
                float maxX = j++;
                float maxY = j++;
                int centroidX = j++;
                int centroidY = j++;
                int branchIndex = 0;

                int index = Soa_Leaf.Append(soa, minX, minY, maxX, maxY, centroidX, centroidY);
                Assert.Equal(i, index);
                Assert_Soa_Leaf.EntryEqual(minX, minY, maxX, maxY, centroidX, centroidY, branchIndex, i, soa);
                Assert.Equal(i+1, soa.AppendCount);
            }
        }

        for(int length = 0; length < 7; length++)
        {            
            Soa_Leaf soa = new(length);
            int j = 0;

            // gapped test
            for(int i = 0; i < length; i += 2)
            {
                float minX = j++;
                float minY = j++;
                float maxX = j++;
                float maxY = j++;
                int centroidX = j++;
                int centroidY = j++;
                int branchIndex = 0;

                int index = Soa_Leaf.Append(soa, minX, minY, maxX, maxY, centroidX, centroidY);
                Assert.Equal(i/2, index);
                Assert_Soa_Leaf.EntryEqual(minX, minY, maxX, maxY, centroidX, centroidY, branchIndex, index, soa);
                Assert.Equal((i/2)+1, soa.AppendCount);
            }
        }
    }

    [Fact]
    public void Intersect_Test()
    {
        int length = 12;
        Soa_Leaf leaves = new(length);
        Span<int> queryIndices = [0,1,3];

        // leaf 0.
        float leaf0MinX     = -1;
        float leaf0MinY     = -1;
        float leaf0MaxX     = 2;
        float leaf0MaxY     = 2;
        float leaf0CentroiX = 0.5f;
        float leaf0CentroiY = 0.5f;

        // leaf 1.
        float leaf1MinX     = -2;
        float leaf1MinY     = -2;
        float leaf1MaxX     = 1;
        float leaf1MaxY     = 1;
        float leaf1CentroiX = -0.5f;
        float leaf1CentroiY = -0.5f;

        // leaf 2.
        float leaf2MinX     = -10;
        float leaf2MinY     = -10;
        float leaf2MaxX     = 10;
        float leaf2MaxY     = 10;
        float leaf2CentroiX = 0f;
        float leaf2CentroiY = 0f;

        // leaf 3.
        float leaf3MinX     = 40;
        float leaf3MinY     = 40;
        float leaf3MaxX     = 200;
        float leaf3MaxY     = 200;
        float leaf3CentroiX = 120f;
        float leaf3CentroiY = 120f;

        // apend leaves.
        Soa_Leaf.Append(leaves, leaf0MinX, leaf0MinY, leaf0MaxX, leaf0MaxY, leaf0CentroiX, leaf0CentroiY);
        Soa_Leaf.Append(leaves, leaf1MinX, leaf1MinY, leaf1MaxX, leaf1MaxY, leaf1CentroiX, leaf1CentroiY);
        Soa_Leaf.Append(leaves, leaf2MinX, leaf2MinY, leaf2MaxX, leaf2MaxY, leaf2CentroiX, leaf2CentroiY);
        Soa_Leaf.Append(leaves, leaf3MinX, leaf3MinY, leaf3MaxX, leaf3MaxY, leaf3CentroiX, leaf3CentroiY);


        // query leaves.
        Assert.True(Soa_Leaf.Intersects(leaves, 0, -0.5f, -0.5f, 0.5f, 0.5f));
        Assert.True(Soa_Leaf.Intersects(leaves, 1, -0.5f, -0.5f, 0.5f, 0.5f));
        Assert.True(Soa_Leaf.Intersects(leaves, 2, -0.5f, -0.5f, 0.5f, 0.5f));
        Assert.False(Soa_Leaf.Intersects(leaves, 3, -0.5f, -0.5f, 0.5f, 0.5f));
    }

    [Fact]
    public void Clear_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            Soa_Leaf soa = new(length);
            int j = 0;
            for(int i = 0; i < length; i++)
            {            
                Soa_Leaf.Append(soa, j++, j++, j++, j++, j++, j++);
            }        
            Assert.Equal(length, soa.AppendCount);
            Soa_Leaf.ResetCount(soa);        
            Assert.Equal(0, soa.AppendCount);
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        int length = 12;

        Soa_Leaf soa = new(length);
        
        for(int i = 0; i < length; i++)
        {
            Soa_Leaf.Append(soa, i, i, i, i, i, i);            
        }

        Soa_Leaf.Dispose(soa);
        
        Assert.Null(soa.Aabbs);
        Assert.Null(soa.Centroids);
        Assert.Null(soa.BranchIndices);
        Assert.Equal(0, soa.AppendCount);
        Assert.Equal(0, soa.Length);
        Assert.True(soa.Disposed);
    }
}