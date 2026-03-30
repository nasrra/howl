using Xunit;
using Howl.DataStructures.Bvh;
using Howl.Math;

namespace Howl.Test.DataStructures.Bvh;

public class Soa_LeafTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            Soa_Leaf buffer = new(capacity);
            Soa_LeafAssert.LengthEqual(capacity, buffer);
            Assert.Equal(0, buffer.AppendCount);
            Assert.False(buffer.Disposed);
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            Soa_Leaf buffer = new(capacity);
            int j = 0;
            for(int i = 0; i < capacity; i++)
            {            
                float minX = j++;
                float minY = j++;
                float maxX = j++;
                float maxY = j++;
                int index = j++;
                int generation = j++;
                int flags = j++;

                Soa_Leaf.Append(buffer, minX, minY, maxX, maxY, index, generation, flags);
                Soa_LeafAssert.EntryEqual(minX, minY, maxX, maxY, index, generation, flags, i, buffer);
                Assert.Equal(i+1, buffer.AppendCount);
            }
        }
    }

    [Fact]
    public void Query_Test()
    {
        int length = 12;
        Soa_Leaf leaves = new(length);
        QueryResultBuffer results = new(length);
        Span<int> queryIndices = [0,1,3];

        // leaf 0.
        float leaf0MinX     = -1;
        float leaf0MinY     = -1;
        float leaf0MaxX     = 2;
        float leaf0MaxY     = 2;
        int leaf0Index      = 0;
        int leaf0Generation = 1;
        int leaf0Flags      = 2;

        // leaf 1.
        float leaf1MinX     = -2;
        float leaf1MinY     = -2;
        float leaf1MaxX     = 1;
        float leaf1MaxY     = 1;
        int leaf1Index      = 3;
        int leaf1Generation = 4;
        int leaf1Flags      = 5;

        // leaf 2.
        float leaf2MinX     = -10;
        float leaf2MinY     = -10;
        float leaf2MaxX     = 10;
        float leaf2MaxY     = 10;
        int leaf2Index      = 9;
        int leaf2Generation = 10;
        int leaf2Flags      = 11;

        // leaf 3.
        float leaf3MinX     = 40;
        float leaf3MinY     = 40;
        float leaf3MaxX     = 200;
        float leaf3MaxY     = 200;
        int leaf3Index      = 6;
        int leaf3Generation = 7;
        int leaf3Flags      = 8;

        // apend leaves.
        Soa_Leaf.Append(leaves, leaf0MinX, leaf0MinY, leaf0MaxX, leaf0MaxY, leaf0Index, leaf0Generation, leaf0Flags);
        Soa_Leaf.Append(leaves, leaf1MinX, leaf1MinY, leaf1MaxX, leaf1MaxY, leaf1Index, leaf1Generation, leaf1Flags);
        Soa_Leaf.Append(leaves, leaf2MinX, leaf2MinY, leaf2MaxX, leaf2MaxY, leaf2Index, leaf2Generation, leaf2Flags);
        Soa_Leaf.Append(leaves, leaf3MinX, leaf3MinY, leaf3MaxX, leaf3MaxY, leaf3Index, leaf3Generation, leaf3Flags);


        // query leaves.
        Soa_Leaf.Query(leaves, results, queryIndices, -0.5f, -0.5f, 0.5f, 0.5f);

        // only leaf 0 and 1 should be counted as overlapping.
        Assert.Equal(2, results.Count);
        QueryResultBufferAssert.EntryEquals(leaf0Index, leaf0Generation, leaf0Flags, 0, results);
        QueryResultBufferAssert.EntryEquals(leaf1Index, leaf1Generation, leaf1Flags, 1, results);
    }

    [Fact]
    public void Clear_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            Soa_Leaf buffer = new(capacity);
            int j = 0;
            for(int i = 0; i < capacity; i++)
            {            
                Soa_Leaf.Append(buffer, j++, j++, j++, j++, j++, j++, j++);
            }        
            Assert.Equal(capacity, buffer.AppendCount);
            Soa_Leaf.Clear(buffer);        
            Assert.Equal(0, buffer.AppendCount);
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        Soa_Leaf buffer = new(12);
        Soa_Leaf.Dispose(buffer);
        Assert.Null(buffer.Aabbs);
        Assert.Null(buffer.GenIndices);
        Assert.Null(buffer.Flags);
        Assert.Equal(0, buffer.AppendCount);
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Disposed);
    }
}