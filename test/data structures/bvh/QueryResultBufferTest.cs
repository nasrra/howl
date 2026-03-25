using Howl.DataStructures.Bvh;
using static Howl.DataStructures.Bvh.QueryResultBuffer;

namespace Howl.Test.DataStructures.Bvh;

public class QueryResultBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 34;
        QueryResultBuffer buffer = new(capacity);
        Assert.Equal(capacity, buffer.GenIndices.Indices.Length);
        Assert.Equal(capacity, buffer.GenIndices.Generations.Length);
        Assert.Equal(capacity, buffer.Flags.Length);
        Assert.Equal(0, buffer.Count);
        Assert.False(buffer.Disposed);
    }

    [Fact]
    public void Append_Test()
    {
        QueryResultBuffer buffer = new(12);
        int j = 0;
        for(int i = 0; i < 2; i++)
        {
            int index = j++;
            int generation = j++;
            int flags = j++;
            AppendQueryResult(buffer, index, generation, flags);
            QueryResultBufferAssert.EntryEquals(index, generation, flags, i, buffer);
            Assert.Equal(i+1, buffer.Count);
        }
    }

    [Fact]
    public void Clear_Test()
    {
        QueryResultBuffer buffer = new(12);
        int j = 0;
        for(int i = 0; i < 2; i++)
        {
            int index = j++;
            int generation = j++;
            int flags = j++;
            AppendQueryResult(buffer, index, generation, flags);
            QueryResultBufferAssert.EntryEquals(index, generation, flags, i, buffer);
        }
        Assert.Equal(2,buffer.Count);
        Clear(buffer);
        Assert.Equal(0,buffer.Count);
    }
}