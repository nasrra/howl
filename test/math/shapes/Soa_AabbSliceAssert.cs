using Howl.Math.Shapes;

namespace Howl.Test.Math.Shapes;

public static class Soa_AabbSliceAssert
{
    /// <summary>
    /// Asserts the equality of span lengths in a slice instance.
    /// </summary>
    /// <param name="length">the expected length of the backing spans.</param>
    /// <param name="slice">the slice instance.</param>
    public static void LengthEqual(int length, Soa_AabbSlice slice)
    {
        Assert.Equal(length, slice.MinX.Length);
        Assert.Equal(length, slice.MinY.Length);
        Assert.Equal(length, slice.MaxX.Length);
        Assert.Equal(length, slice.MaxY.Length);
        Assert.Equal(length, slice.Length);
    }

    /// <summary>
    /// Asserts the equality of a slice entry and expected values.
    /// </summary>
    /// <param name="minX">the expected minimum x value.</param>
    /// <param name="minY">the expected minimum y value.</param>
    /// <param name="maxX">the expected maximum x value.</param>
    /// <param name="maxY">the expected maximum y value.</param>
    /// <param name="entryIndex">the index of the entry in the slice to assert equality against.</param>
    /// <param name="slice">the slice containing the entry to assert.</param>
    public static void EntryEqual(float minX, float minY, float maxX, float maxY, int entryIndex, Soa_AabbSlice slice)
    {
        Assert.Equal(minX, slice.MinX[entryIndex]);
        Assert.Equal(minY, slice.MinY[entryIndex]);
        Assert.Equal(maxX, slice.MaxX[entryIndex]);
        Assert.Equal(maxY, slice.MaxY[entryIndex]);
    }
}