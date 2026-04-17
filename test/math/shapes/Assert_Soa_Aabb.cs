using Xunit;
using Howl.Math.Shapes;

namespace Howl.Test.Math.Shapes;

public static class Assert_Soa_Aabb
{
    /// <summary>
    ///     Asserts the equality of array lengths in a soa aabb instance.
    /// </summary>
    /// <param name="soa">the soa aabb instance to assert against.</param>
    /// <param name="length">the expected length of the backing arrays.</param>
    public static void LengthEqual(int length, Soa_Aabb soa)
    {
        Assert.Equal(length, soa.Length);
        Assert.Equal(length, soa.MinX.Length);
        Assert.Equal(length, soa.MinY.Length);
        Assert.Equal(length, soa.MaxX.Length);
        Assert.Equal(length, soa.MaxY.Length);
    }

    /// <summary>
    ///     Asserts the equality of values for a soa entry and expected values.
    /// </summary>
    /// <param name="minX">the expected minimum x value.</param>
    /// <param name="minY">the epeceted minimum y value.</param>
    /// <param name="maxX">the expected maximum x value.</param>
    /// <param name="maxY">the expected maximum y value.</param>
    /// <param name="entryIndex">the index of the entry in the soa to assert eqaulity against.</param>
    /// <param name="soa">the soa containing the entry to assert against.</param>
    public static void EntryEqual(float minX, float minY, float maxX, float maxY, int entryIndex, Soa_Aabb soa)
    {
        Assert.Equal(minX, soa.MinX[entryIndex]);
        Assert.Equal(minY, soa.MinY[entryIndex]);
        Assert.Equal(maxX, soa.MaxX[entryIndex]);
        Assert.Equal(maxY, soa.MaxY[entryIndex]);
    }
}