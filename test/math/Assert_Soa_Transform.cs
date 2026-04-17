using Howl.Math;

namespace Howl.Test.Math;

public static class Assert_Soa_Transform
{
    /// <summary>
    ///     Ensures that a transform entry in a soa transform is equal to the specified transform struct.
    /// </summary>
    /// <param name="soa">the soa transform collection.</param>
    /// <param name="transform">the transform to check equality against.</param>
    /// <param name="entryIndex">the index of the entry in the soa transform collection to check.</param>
    /// <param name="precision">the precision of floating point equality checks.</param>
    public static void EntryEqual(Transform transform, int precision, int entryIndex, Soa_Transform soa)
    {
        Assert.Equal(transform.Position.X, soa.Positions.X[entryIndex],   precision);
        Assert.Equal(transform.Position.Y, soa.Positions.Y[entryIndex],   precision);
        Assert.Equal(transform.Scale.X,    soa.Scales.X[entryIndex],      precision);
        Assert.Equal(transform.Scale.Y,    soa.Scales.Y[entryIndex],      precision);        
        Assert.Equal(transform.Cos,        soa.Coses[entryIndex],          precision);
        Assert.Equal(transform.Sin,        soa.Sins[entryIndex],          precision);
    }

    public static void EntryEqual(float posX, float posY, float scaleX, float scaleY, float cos, float sin, int precision, int entryIndex, 
        Soa_Transform soa
    )
    {
        Assert.Equal(posX, soa.Positions.X[entryIndex], precision);
        Assert.Equal(posY, soa.Positions.Y[entryIndex], precision);
        Assert.Equal(scaleX, soa.Scales.X[entryIndex], precision);
        Assert.Equal(scaleY, soa.Scales.Y[entryIndex], precision);        
        Assert.Equal(cos, soa.Coses[entryIndex], precision);
        Assert.Equal(sin, soa.Sins[entryIndex], precision);
    }
}