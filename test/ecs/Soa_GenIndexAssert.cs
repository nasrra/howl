using Howl.Ecs;

namespace Howl.Test.Ecs;

public static class Soa_GenIndexAssert
{   
    /// <summary>
    /// Asserts the equality of array lengths in a soa gen index instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="soa">the soa gen index instance to assert against.</param>
    public static void LengthEqual(int length, Soa_GenIndex soa)
    {
        Assert.Equal(length, soa.Generations.Length);
        Assert.Equal(length, soa.Indices.Length);
    }

    /// <summary>
    /// Asserts the equality of values for a soa entry and expected values.
    /// </summary>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the expected generation value.</param>
    /// <param name="entryIndex">the index of the entry in the soa to assert eqaulity against.</param>
    /// <param name="soa">the soa containing the entry to assert against.</param>
    public static void EntryEqual(int index, int generation, int entryIndex, Soa_GenIndex soa)
    {
        Assert.Equal(index, soa.Indices[entryIndex]);
        Assert.Equal(generation, soa.Generations[entryIndex]);
    }
}