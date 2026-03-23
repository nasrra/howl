using Howl.DataStructures.Bvh;

public static class QueryResultBufferAssert
{
    /// <summary>
    /// Asserts the values of a buffer entry are equal to expected values.
    /// </summary>
    /// <param name="buffer">the buffer to assert.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to check equality against.</param>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the epxected generation value.</param>
    /// <param name="flags">the expected flags value.</param>
    public static void EntryEquals(QueryResultBuffer buffer, int entryIndex, int index, int generation, int flags)
    {
        Assert.Equal(index, buffer.GenIndices.Indices[entryIndex]);
        Assert.Equal(generation, buffer.GenIndices.Generations[entryIndex]);
        Assert.Equal(flags, buffer.Flags[entryIndex]);
    }
}