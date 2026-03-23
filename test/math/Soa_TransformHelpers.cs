using Howl.Math;

namespace Howl.Test.Math;

public static class Soa_TransformHelpers
{
    /// <summary>
    /// Ensures that a transform entry in a soa transform is equal to the specified transform struct.
    /// </summary>
    /// <param name="soa">the soa transform collection.</param>
    /// <param name="transform">the transform to check equality against.</param>
    /// <param name="index">the index of the entry in the soa transform collection to check.</param>
    /// <param name="precision">the precision of floating point equality checks.</param>
    public static void AssertEqualsSoaTransformEntry(Soa_Transform soa, ref Transform transform, int index, int precision)
    {
        Assert.Equal(transform.Position.X, soa.Position.X[index],   precision);
        Assert.Equal(transform.Position.Y, soa.Position.Y[index],   precision);
        Assert.Equal(transform.Scale.X,    soa.Scale.X[index],      precision);
        Assert.Equal(transform.Scale.Y,    soa.Scale.Y[index],      precision);        
        Assert.Equal(transform.Cos,        soa.Cos[index],          precision);
        Assert.Equal(transform.Sin,        soa.Sin[index],          precision);
    }
}