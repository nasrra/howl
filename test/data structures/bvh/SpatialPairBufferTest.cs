
using Xunit;
using Howl.DataStructures.Bvh;
using static Howl.DataStructures.Bvh.SpatialPairBuffer;

namespace Howl.Test.DataStructures.Bvh;

public class SpatialPairBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 7;
        SpatialPairBuffer buffer = new(capacity);
        Assert.Equal(capacity, buffer.OwnerGenIndices.Indices.Length);
        Assert.Equal(capacity, buffer.OwnerGenIndices.Generations.Length);
        Assert.Equal(capacity, buffer.OtherGenIndices.Indices.Length);
        Assert.Equal(capacity, buffer.OtherGenIndices.Generations.Length);
        Assert.Equal(capacity, buffer.OtherFlags.Length);
        Assert.Equal(capacity, buffer.OtherFlags.Length);
        Assert.False(buffer.Disposed);
        Assert.Equal(0, buffer.Count);
    }

    [Fact]
    public void Append_Test()
    {
        SpatialPairBuffer buffer = new(10);
        int j = 0;
        for(int i = 0; i < 2; i++)
        {            
            int ownerIndex = j += 1;
            int ownerGeneration = j += 1;
            int ownerFlags = j += 1;
            int otherIndex = j += 1;
            int otherGeneration = j += 1;
            int otherFlags = j += 1;
            AppendSpatialPair(buffer, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
            SpatialPairBufferAssert.EntryEquals(buffer, i, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
            Assert.Equal(i+1, buffer.Count);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        SpatialPairBuffer buffer = new(10);
        int j = 0;
        for(int i = 0; i < 2; i++)
        {            
            int ownerIndex = j += 1;
            int ownerGeneration = j += 1;
            int ownerFlags = j += 1;
            int otherIndex = j += 1;
            int otherGeneration = j += 1;
            int otherFlags = j += 1;
            AppendSpatialPair(buffer, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
            SpatialPairBufferAssert.EntryEquals(buffer, i, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
        }        
        Assert.Equal(2, buffer.Count);
        Clear(buffer);
        Assert.Equal(0, buffer.Count);
    }
}