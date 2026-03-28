using Howl.Math;

namespace Howl.Test.Math;

public static class Soa_Vector2SliceAssert
{
    /// <summary>
    /// Asserts the equality of span lengths in a slice instance.
    /// </summary>
    /// <param name="length">the expected length of the backing spans.</param>
    /// <param name="slice">the slice instance.</param>
    public static void LengthEqual(int length, Soa_Vector2Slice slice)
    {
        Assert.Equal(length, slice.X.Length);
        Assert.Equal(length, slice.Y.Length);
        Assert.Equal(length, slice.Length);
    }

    /// <summary>
    /// Asserts the equality of a slice entry and expected values.
    /// </summary>
    /// <param name="x">the expected x value.</param>
    /// <param name="y">the expected y value.</param>
    /// <param name="entryIndex">the index of the entry in the slice to assert equality against.</param>
    /// <param name="slice">the slice containing the entry to assert.</param>
    public static void EntryEqual(float x, float y, int entryIndex, Soa_Vector2Slice slice)
    {
        Assert.Equal(x, slice.X[entryIndex]);
        Assert.Equal(y, slice.Y[entryIndex]);
    }
}