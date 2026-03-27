using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class QueryResultBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            QueryResultBuffer buffer = new(length);
            QueryResultBufferAssert.LengthEqual(length, buffer);
            Assert.Equal(0, buffer.Count);
            Assert.False(buffer.Disposed);
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            QueryResultBuffer buffer = new(length);
            int j = 0;
            for(int i = 0; i < length; i++)
            {
                int index = j++;
                int generation = j++;
                int flags = j++;
                QueryResultBuffer.Append(buffer, index, generation, flags);
                QueryResultBufferAssert.EntryEquals(index, generation, flags, i, buffer);
                Assert.Equal(i+1, buffer.Count);
            }
            Assert.Equal(length, buffer.Count);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        for(int length = 0; length < 25; length++)
        {            
            QueryResultBuffer buffer = new(length);
            int j = 0;
            for(int i = 0; i < length; i++)
            {
                int index = j++;
                int generation = j++;
                int flags = j++;
                QueryResultBuffer.Append(buffer, index, generation, flags);
                QueryResultBufferAssert.EntryEquals(index, generation, flags, i, buffer);
            }
            Assert.Equal(length,buffer.Count);
            QueryResultBuffer.Clear(buffer);
            Assert.Equal(0,buffer.Count);
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        QueryResultBuffer buffer = new(12);
        QueryResultBuffer.Dispose(buffer);
        Assert.Null(buffer.GenIndices);
        Assert.Null(buffer.Flags);
        Assert.Equal(0, buffer.Count);
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Disposed);
    }
}