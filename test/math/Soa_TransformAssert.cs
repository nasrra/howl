using Howl.Math;

namespace Howl.Test.Math;

public static class Soa_TransformAssert
{
    /// <summary>
    /// Ensures that a transform entry in a soa transform is equal to the specified transform struct.
    /// </summary>
    /// <param name="soa">the soa transform collection.</param>
    /// <param name="transform">the transform to check equality against.</param>
    /// <param name="index">the index of the entry in the soa transform collection to check.</param>
    /// <param name="precision">the precision of floating point equality checks.</param>
    public static void EntryEquals(Soa_Transform soa, ref Transform transform, int index, int precision)
    {
        Assert.Equal(transform.Position.X, soa.Positions.X[index],   precision);
        Assert.Equal(transform.Position.Y, soa.Positions.Y[index],   precision);
        Assert.Equal(transform.Scale.X,    soa.Scales.X[index],      precision);
        Assert.Equal(transform.Scale.Y,    soa.Scales.Y[index],      precision);        
        Assert.Equal(transform.Cos,        soa.Coses[index],          precision);
        Assert.Equal(transform.Sin,        soa.Sins[index],          precision);
    }
}