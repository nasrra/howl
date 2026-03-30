using Howl.DataStructures.Bvh;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Test.ECS;
using Howl.Test.Math;
using Howl.Test.Math.Shapes;

public static class Soa_LeafAssert
{
    /// <summary>
    /// Asserts the equality of a buffer entry and expected values.
    /// </summary>
    /// <param name="minX">the expected minimum x value.</param>
    /// <param name="minY">the expected minimum y value.</param>
    /// <param name="maxX">the expected maximum x value.</param>
    /// <param name="maxY">the expected maximum y value.</param>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the expected generation value.</param>
    /// <param name="flags">the expected flags value.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to assert equality against.</param>
    /// <param name="buffer">the buffer containing the entry to assert.</param>
    public static void EntryEqual(float minX, float minY, float maxX, float maxY, int index, int generation, 
        int flags, int entryIndex, Soa_Leaf buffer
    )
    {
        Soa_AabbAssert.EntryEqual(minX, minY, maxX, maxY, entryIndex, buffer.Aabbs);
        Soa_GenIndexAssert.EntryEqual(index, generation, entryIndex, buffer.GenIndices);
        Assert.Equal(flags, buffer.Flags[entryIndex]);
    }

    /// <summary>
    /// Asserts the equality of array lengths in a buffer instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="buffer">the buffer instance.</param>
    public static void LengthEqual(int length, Soa_Leaf buffer)
    {
        Soa_AabbAssert.LengthEqual(length, buffer.Aabbs);
        Soa_GenIndexAssert.LengthEqual(length, buffer.GenIndices);
        Assert.Equal(length, buffer.Flags.Length);
        Assert.Equal(length, buffer.Length);
    }
}