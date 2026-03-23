using Howl.Math;

namespace Howl.Test.Math;

public static class TransformAssert
{
    /// <summary>
    /// Asserts that two transform structs are equals.
    /// </summary>
    /// <param name="expected">the expected transform.</param>
    /// <param name="result">the resultant transform.</param>
    /// <param name="precision">the preceision to assert floating point values.</param>
    public static void Equals(ref Transform expected, ref Transform result, float precision)
    {
        Assert.Equal(expected.Position.X, result.Position.X, precision);
        Assert.Equal(expected.Position.Y, result.Position.Y, precision);
        Assert.Equal(expected.Scale.X, result.Scale.X, precision);
        Assert.Equal(expected.Scale.Y, result.Scale.Y, precision);
        Assert.Equal(expected.Rotation, result.Rotation, precision);
        Assert.Equal(expected.Sin, result.Sin, precision);
        Assert.Equal(expected.Cos, result.Cos, precision);
    }
}