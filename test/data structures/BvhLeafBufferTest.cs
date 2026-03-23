using Xunit;
using Howl.DataStructures;
using static Howl.DataStructures.BvhLeafBuffer;

namespace Howl.Test.DataStructures;

public class BvhLeafBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 23;
        BvhLeafBuffer buffer = new(capacity);
        Assert.Equal(capacity, buffer.Aabbs.MaxX.Length);
        Assert.Equal(capacity, buffer.Aabbs.MaxY.Length);
        Assert.Equal(capacity, buffer.Aabbs.MinX.Length);
        Assert.Equal(capacity, buffer.Aabbs.MinY.Length);
        Assert.Equal(capacity, buffer.GenIndices.Indices.Length);
        Assert.Equal(capacity, buffer.GenIndices.Generations.Length);
        Assert.Equal(capacity, buffer.Flags.Length);
        Assert.Equal(0, buffer.Count);
    }

    [Fact]
    public void Append_Test()
    {
        BvhLeafBuffer buffer = new(10);
        int j = 0;
        for(int i = 0; i < 2; i++)
        {            
            float minX = j += 1;
            float minY = j += 1;
            float maxX = j += 1;
            float maxY = j += 1;
            int index = j += 1;
            int generation = j += 1;
            int flags = j += 1;
            AppendLeaf(buffer, minX, minY, maxX, maxY, index, generation, flags);
            BvhLeafBufferAssert.EntryEquals(buffer, i, minX, minY, maxX, maxY, index, generation, flags);
            Assert.Equal(i+1, buffer.Count);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        BvhLeafBuffer buffer = new(10);
        int j = 0;
        for(int i = 0; i < 2; i++)
        {            
            float minX = j += 1;
            float minY = j += 1;
            float maxX = j += 1;
            float maxY = j += 1;
            int index = j += 1;
            int generation = j += 1;
            int flags = j += 1;
            AppendLeaf(buffer, minX, minY, maxX, maxY, index, generation, flags);
            BvhLeafBufferAssert.EntryEquals(buffer, i, minX, minY, maxX, maxY, index, generation, flags);
        }        
        Assert.Equal(2, buffer.Count);
        Clear(buffer);        
        Assert.Equal(0, buffer.Count);
    }
}