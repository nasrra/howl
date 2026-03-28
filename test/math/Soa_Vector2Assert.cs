using Howl.Math;

namespace Howl.Test.Math;

public static class Soa_Vector2Assert
{
    /// <summary>
    /// Asserts the equality of array lengths in a soa instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="soa">the soa instance.</param>
    public static void LengthEqual(int length, Soa_Vector2 soa)
    {
        Assert.Equal(length, soa.X.Length);
        Assert.Equal(length, soa.Y.Length);
        Assert.Equal(length, soa.Length);
    }

    /// <summary>
    /// Asserts the equality of a soa entry and expected values.
    /// </summary>
    /// <param name="x">the expected x value.</param>
    /// <param name="y">the expected y value.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to assert equality against.</param>
    /// <param name="soa">the soa containing the entry to assert.</param>
    public static void EntryEqual(float x, float y, int entryIndex, Soa_Vector2 soa)
    {
        Assert.Equal(x, soa.X[entryIndex]);
        Assert.Equal(y, soa.Y[entryIndex]);
    }
}