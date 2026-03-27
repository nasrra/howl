using Howl.Math.Shapes;

public static class Soa_GenIndexSliceAssert
{
    /// <summary>
    /// Asserts the equality os span lengths in a slice instance.
    /// </summary>
    /// <param name="length">the expected length of the backing spans.</param>
    /// <param name="slice">the slice instance.</param>
    public static void LengthEqual(int length, Soa_GenIndexSlice slice)
    {
        Assert.Equal(length, slice.Indices.Length);
        Assert.Equal(length, slice.Generations.Length);
        Assert.Equal(length, slice.Length);
    }

    /// <summary>
    /// Asserts the equality of a slice entry and expected values.
    /// </summary>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the expected generation value.</param>
    /// <param name="entryIndex">the index of the entry in the slice to assert equality against.</param>
    /// <param name="slice">the slice containing the entry to assert.</param>
    public static void EntryEqual(int index, int generation, int entryIndex, Soa_GenIndexSlice slice)
    {
        Assert.Equal(index, slice.Indices[entryIndex]);
        Assert.Equal(generation, slice.Generations[entryIndex]);
    }
}