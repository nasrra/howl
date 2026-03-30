using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class Soa_QueryResultTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            Soa_QueryResult buffer = new(length);
            Soa_QueryResultAssert.LengthEqual(length, buffer);
            Assert.Equal(0, buffer.AppendCount);
            Assert.False(buffer.Disposed);
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            Soa_QueryResult buffer = new(length);
            int j = 0;
            for(int i = 0; i < length; i++)
            {
                int index = j++;
                int generation = j++;
                int flags = j++;
                Soa_QueryResult.Append(buffer, index, generation, flags);
                Soa_QueryResultAssert.EntryEquals(index, generation, flags, i, buffer);
                Assert.Equal(i+1, buffer.AppendCount);
            }
            Assert.Equal(length, buffer.AppendCount);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            Soa_QueryResult buffer = new(length);
            int j = 0;
            for(int i = 0; i < length; i++)
            {
                int index = j++;
                int generation = j++;
                int flags = j++;
                Soa_QueryResult.Append(buffer, index, generation, flags);
                Soa_QueryResultAssert.EntryEquals(index, generation, flags, i, buffer);
            }
            Assert.Equal(length,buffer.AppendCount);
            Soa_QueryResult.Clear(buffer);
            Assert.Equal(0,buffer.AppendCount);
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        Soa_QueryResult buffer = new(12);
        Soa_QueryResult.Dispose(buffer);
        Assert.Null(buffer.GenIndices);
        Assert.Null(buffer.Flags);
        Assert.Equal(0, buffer.AppendCount);
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Disposed);
    }
}