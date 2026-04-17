namespace Howl.Test.Math;

using Math = Howl.Math.Math;

public class MathTest
{
    private const float NearlyEqualEpsilon = 1e-6f;

    [Fact]
    public void Abs_Test()
    {
        Assert.Equal(0,Math.Abs(0));
        Assert.Equal(1,Math.Abs(1));
        Assert.Equal(1,Math.Abs(-1));
        Assert.Equal(33.33333f,Math.Abs(-33.33333f));
        Assert.Equal(float.Epsilon,Math.Abs(-float.Epsilon));
    }

    [Fact]
    public void Max_Test()
    {
        Assert.Equal(0,Math.Max(0,0));
        Assert.Equal(2,Math.Max(1,2));
        Assert.Equal(2,Math.Max(2,1));
        Assert.Equal(1,Math.Max(-2,1));
        Assert.Equal(1,Math.Max(1,-2));
        Assert.Equal(float.Epsilon,Math.Max(float.Epsilon,-float.Epsilon));
        Assert.Equal(float.Epsilon,Math.Max(-float.Epsilon, float.Epsilon));
    }

    [Fact]
    public void Min_Test()
    {
        Assert.Equal(0,Math.Min(0,0));
        Assert.Equal(1,Math.Min(1,2));
        Assert.Equal(1,Math.Min(2,1));
        Assert.Equal(-2,Math.Min(-2,1));
        Assert.Equal(-2,Math.Min(1,-2));
        Assert.Equal(-float.Epsilon,Math.Min(float.Epsilon,-float.Epsilon));
        Assert.Equal(-float.Epsilon,Math.Min(-float.Epsilon, float.Epsilon));
    }

    [Fact]
    public void NearlyEqual_Test()
    {
        float value1;
        float value2;

        value1 = 33.33333f;
        value2 = 0;

        for(int i = 0; i < 3; i++)
        {
            value2 += 11.11111f;
        }

        Assert.True(Math.NearlyEqual(value1, value2, NearlyEqualEpsilon));

        value1 = -99.99999f;
        value2 = 0;

        for(int i = 0; i < 9; i++)
        {
            value2 -= 11.11111f;
        }

        // this should be false due to rounding errors with floating point accumulation.
        Assert.False(Math.NearlyEqual(value1, value2, NearlyEqualEpsilon));

        value1 = 99999.9998f;
        value2 = 99999.9999f;
        Assert.True(Math.NearlyEqual(value1, value2, NearlyEqualEpsilon));
    }

    [Fact]
    public void Clamp_Test()
    {
        Assert.Equal(1.0f, Math.Clamp(0.1f, 1.0f, 2.0f));
        Assert.Equal(2.0f, Math.Clamp(2.2f, 1.0f, 2.0f));
        Assert.Equal(1.5f, Math.Clamp(1.5f, 1.0f, 2.0f));
        Assert.Throws<ArgumentException>(() => Math.Clamp(1.5f, 2.0f, 1.0f));
    }
}