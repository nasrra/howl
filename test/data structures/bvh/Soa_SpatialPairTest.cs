
using Xunit;
using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class Soa_SpatialPairTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 7;
        Soa_SpatialPair buffer = new(capacity);
        Soa_SpatialPairAssert.LengthEqual(capacity, buffer);
        Assert.False(buffer.Disposed);
        Assert.Equal(0, buffer.AppendCount);
    }

    [Fact]
    public void Append_Test()
    {
        Soa_SpatialPair buffer = new(10);
        int j = 0;
        for(int i = 0; i < 2; i++)
        {            
            int ownerIndex = j += 1;
            int ownerGeneration = j += 1;
            int ownerFlags = j += 1;
            int otherIndex = j += 1;
            int otherGeneration = j += 1;
            int otherFlags = j += 1;
            Soa_SpatialPair.Append(buffer, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
            Soa_SpatialPairAssert.EntryEqual(buffer, i, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
            Assert.Equal(i+1, buffer.AppendCount);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        Soa_SpatialPair buffer = new(10);
        int j = 0;
        for(int i = 0; i < 2; i++)
        {            
            int ownerIndex = j += 1;
            int ownerGeneration = j += 1;
            int ownerFlags = j += 1;
            int otherIndex = j += 1;
            int otherGeneration = j += 1;
            int otherFlags = j += 1;
            Soa_SpatialPair.Append(buffer, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
            Soa_SpatialPairAssert.EntryEqual(buffer, i, ownerIndex, ownerGeneration, ownerFlags, otherIndex, otherGeneration, otherFlags);
        }        
        Assert.Equal(2, buffer.AppendCount);
        Soa_SpatialPair.Clear(buffer);
        Assert.Equal(0, buffer.AppendCount);
    }

    [Fact]
    public void Disposal_Test()
    {
        Soa_SpatialPair buffer = new(12);
        
        Soa_SpatialPair.Dispose(buffer);

        Assert.Null(buffer.OwnerGenIndices);
        Assert.Null(buffer.OtherGenIndices);
        Assert.Null(buffer.OwnerFlags);
        Assert.Null(buffer.OtherFlags);
        Assert.Equal(0, buffer.AppendCount);
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Disposed);
    }
}