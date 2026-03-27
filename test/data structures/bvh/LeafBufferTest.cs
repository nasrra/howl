using Xunit;
using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class LeafBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int capacity = 0; capacity < 25; capacity++)
        {            
            LeafBuffer buffer = new(capacity);
            Assert.Equal(capacity, buffer.Aabbs.MaxX.Length);
            Assert.Equal(capacity, buffer.Aabbs.MaxY.Length);
            Assert.Equal(capacity, buffer.Aabbs.MinX.Length);
            Assert.Equal(capacity, buffer.Aabbs.MinY.Length);
            Assert.Equal(capacity, buffer.GenIndices.Indices.Length);
            Assert.Equal(capacity, buffer.GenIndices.Generations.Length);
            Assert.Equal(capacity, buffer.Flags.Length);
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
                float minX = j += 1;
                float minY = j += 1;
                float maxX = j += 1;
                float maxY = j += 1;
                int index = j += 1;
                int generation = j += 1;
                int flags = j += 1;
                LeafBuffer.Append(buffer, minX, minY, maxX, maxY, index, generation, flags);
                LeafBufferAssert.EntryEqual(minX, minY, maxX, maxY, index, generation, flags, i, buffer);
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
                float minX = j += 1;
                float minY = j += 1;
                float maxX = j += 1;
                float maxY = j += 1;
                int index = j += 1;
                int generation = j += 1;
                int flags = j += 1;
                LeafBuffer.Append(buffer, minX, minY, maxX, maxY, index, generation, flags);
                LeafBufferAssert.EntryEqual(minX, minY, maxX, maxY, index, generation, flags, i, buffer);
            }        
            Assert.Equal(capacity, buffer.Count);
            LeafBuffer.Clear(buffer);        
            Assert.Equal(0, buffer.Count);
        }
    }
}