
using Xunit;
using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class SpatialPairBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 7;
        SpatialPairBuffer buffer = new(capacity);
        SpatialPairBufferAssert.LengthEqual(capacity, buffer);
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
            SpatialPairBuffer.Append(buffer, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
            SpatialPairBufferAssert.EntryEqual(buffer, i, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
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
            SpatialPairBuffer.Append(buffer, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
            SpatialPairBufferAssert.EntryEqual(buffer, i, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
        }        
        Assert.Equal(2, buffer.Count);
        SpatialPairBuffer.Clear(buffer);
        Assert.Equal(0, buffer.Count);
    }

    [Fact]
    public void Disposal_Test()
    {
        SpatialPairBuffer buffer = new(12);
        
        SpatialPairBuffer.Dispose(buffer);

        Assert.Null(buffer.OwnerGenIndices);
        Assert.Null(buffer.OtherGenIndices);
        Assert.Null(buffer.OwnerFlags);
        Assert.Null(buffer.OtherFlags);
        Assert.Equal(0, buffer.Count);
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Disposed);
    }
}