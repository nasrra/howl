using Xunit;
using Howl.Math.Shapes;

namespace Howl.Test.Math.Shapes;

public static class Soa_AabbAssert
{
    /// <summary>
    /// Asserts the equality of array lengths in a soa aabb instance.
    /// </summary>
    /// <param name="soa">the soa aabb instance to assert against.</param>
    /// <param name="length">the expected length of the backing arrays.</param>
    public static void LengthEqual(Soa_Aabb soa, int length)
    {
        Assert.Equal(length, soa.MinX.Length);
        Assert.Equal(length, soa.MinY.Length);
        Assert.Equal(length, soa.MaxX.Length);
        Assert.Equal(length, soa.MaxY.Length);
    }
}