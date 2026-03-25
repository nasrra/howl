using Howl.DataStructures.Bvh;

public static class QueryResultBufferAssert
{
    /// <summary>
    /// Asserts the values of a buffer entry are equal to expected values.
    /// </summary>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the epxected generation value.</param>
    /// <param name="flags">the expected flags value.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to assert equality against.</param>
    /// <param name="buffer">the buffer containing the entry to assert.</param>
    public static void EntryEquals(int index, int generation, int flags, int entryIndex, QueryResultBuffer buffer)
    {
        Assert.Equal(index, buffer.GenIndices.Indices[entryIndex]);
        Assert.Equal(generation, buffer.GenIndices.Generations[entryIndex]);
        Assert.Equal(flags, buffer.Flags[entryIndex]);
    }
}