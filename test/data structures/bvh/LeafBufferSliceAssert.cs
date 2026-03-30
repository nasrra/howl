
using Howl.DataStructures.Bvh;
using Howl.Math.Shapes;
using Howl.Test.Math.Shapes;

namespace Howl.Test.DataStructures.Bvh;

public static class LeafBufferSliceAssert
{

    /// <summary>
    /// Asserts the equality of span lengths in a slice instance.
    /// </summary>
    /// <param name="length">the expected length of the backing spans.</param>
    /// <param name="slice">the slice instance.</param>
    public static void LengthEqual(int length, LeafBufferSlice slice)
    {
        Assert.Equal(length, slice.Aabbs.Length);
        Assert.Equal(length, slice.GenIndices.Length);
        Assert.Equal(length, slice.Flags.Length);
        Assert.Equal(length, slice.Length);
    }

    /// <summary>
    /// Asserts the equality of a slice entry and expected values.
    /// </summary>
    /// <param name="minX">the expected minimum x value.</param>
    /// <param name="minY">the expected minimum y value.</param>
    /// <param name="maxX">the expected maximum x value.</param>
    /// <param name="maxY">the expected maximum y value.</param>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the expected generation value.</param>
    /// <param name="flags">the expected user-defined flags value.</param>
    /// <param name="entryIndex">the index of the entry in the slice to assert equality against.</param>
    /// <param name="slice">the slice containing the entry to assert.</param>
    public static void EntryEqual(float minX, float minY, float maxX, float maxY, int index, int generation, int flags, int entryIndex, LeafBufferSlice slice)
    {
        Soa_AabbSliceAssert.EntryEqual(minX, minY, maxX, maxY, entryIndex, slice.Aabbs);
        Soa_GenIndexSliceAssert.EntryEqual(index, generation, entryIndex, slice.GenIndices);
        Assert.Equal(flags, slice.Flags[entryIndex]);
    }
}