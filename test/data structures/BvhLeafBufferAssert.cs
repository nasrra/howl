using Howl.DataStructures;

public static class BvhLeafBufferAssert
{
    /// <summary>
    /// Asserts the values of a buffer entry are equal to expected values.
    /// </summary>
    /// <param name="buffer">the buffer to assert.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to check equality against.</param>
    /// <param name="minX">the expected minimum x value.</param>
    /// <param name="minY">the expected minimum y value.</param>
    /// <param name="maxX">the expected maximum x value.</param>
    /// <param name="maxY">the expected maximum y value.</param>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the expected generation value.</param>
    /// <param name="flags">the expected flags value.</param>
    public static void EntryEquals(BvhLeafBuffer buffer, int entryIndex, float minX, float minY, float maxX, float maxY, int index, int generation, 
        int flags
    )
    {
        Assert.Equal(minX, buffer.Aabbs.MinX[entryIndex]);
        Assert.Equal(minY, buffer.Aabbs.MinY[entryIndex]);
        Assert.Equal(maxX, buffer.Aabbs.MaxX[entryIndex]);
        Assert.Equal(maxY, buffer.Aabbs.MaxY[entryIndex]);
        Assert.Equal(index, buffer.GenIndices.Indices[entryIndex]);
        Assert.Equal(generation, buffer.GenIndices.Generations[entryIndex]);
        Assert.Equal(flags, buffer.Flags[entryIndex]);
    }
}