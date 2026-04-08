using Howl.DataStructures.Bvh;
using Howl.Test.Ecs;

public static class Soa_QueryResultAssert
{
    /// <summary>
    /// Asserts the values of a soa instance's entry are equal to expected values.
    /// </summary>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the epxected generation value.</param>
    /// <param name="flags">the expected flags value.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to assert equality against.</param>
    /// <param name="soa">the soa instance containing the entry to assert.</param>
    public static void EntryEquals(int index, int generation, int flags, int entryIndex, Soa_QueryResult soa)
    {
        Assert.Equal(index, soa.GenIndices.Indices[entryIndex]);
        Assert.Equal(generation, soa.GenIndices.Generations[entryIndex]);
        Assert.Equal(flags, soa.Flags[entryIndex]);
    }

    /// <summary>
    /// Asserts the equality of array lengths in a soa instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="soa">the soa instance.</param>
    public static void LengthEqual(int expectedLength, Soa_QueryResult soa)
    {
        Soa_GenIndexAssert.LengthEqual(expectedLength, soa.GenIndices);
        Assert.Equal(expectedLength, soa.Flags.Length);
        Assert.Equal(expectedLength, soa.Length);
    }
}