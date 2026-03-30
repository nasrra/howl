using Xunit;
using Howl.DataStructures.Bvh;
using Howl.Math;

namespace Howl.Test.DataStructures.Bvh;

public class LeafBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            LeafBuffer buffer = new(capacity);
            LeafBufferAssert.LengthEqual(capacity, buffer);
            Assert.Equal(0, buffer.Count);
            Assert.False(buffer.Disposed);
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            LeafBuffer buffer = new(capacity);
            int j = 0;
            for(int i = 0; i < capacity; i++)
            {            
                float minX = j++;
                float minY = j++;
                float maxX = j++;
                float maxY = j++;
                float centroidX = 0;
                float centroidY = 0;
                int index = j++;
                int generation = j++;
                int flags = j++;

                LeafBuffer.Append(buffer, minX, minY, maxX, maxY, index, generation, flags);
                LeafBufferAssert.EntryEqual(minX, minY, maxX, maxY, centroidX, centroidY, index, generation, flags, i, buffer);
                Assert.Equal(i+1, buffer.Count);
            }
        }
    }

    [Fact]
    public void Clear_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            LeafBuffer buffer = new(capacity);
            int j = 0;
            for(int i = 0; i < capacity; i++)
            {            
                LeafBuffer.Append(buffer, j++, j++, j++, j++, j++, j++, j++);
            }        
            Assert.Equal(capacity, buffer.Count);
            LeafBuffer.Clear(buffer);        
            Assert.Equal(0, buffer.Count);
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        LeafBuffer buffer = new(12);
        LeafBuffer.Dispose(buffer);
        Assert.Null(buffer.Aabbs);
        Assert.Null(buffer.GenIndices);
        Assert.Null(buffer.Centroids);
        Assert.Null(buffer.Flags);
        Assert.Null(buffer.CentroidIds);
        Assert.Equal(0, buffer.Count);
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Disposed);
    }
}