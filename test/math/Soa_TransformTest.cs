using static Howl.Math.Soa_Transform;
using static Howl.Test.Math.Soa_TransformHelpers;
using static Howl.Test.Math.TransformHelpers;

using Howl.Math;

namespace Howl.Test.Math;

public class Soa_TransformTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 10;
        Soa_Transform soa = new(capacity);
        
        Assert.Equal(capacity, soa.Position.X.Length);
        Assert.Equal(capacity, soa.Position.Y.Length);
        Assert.Equal(capacity, soa.Scale.X.Length);        
        Assert.Equal(capacity, soa.Scale.Y.Length);
        Assert.Equal(capacity, soa.Sin.Length);        
        Assert.Equal(capacity, soa.Cos.Length);
        
        for(int i = 0; i < capacity; i++)
        {
            Assert.Equal(0, soa.Position.X[i]);
            Assert.Equal(0, soa.Position.Y[i]);
            Assert.Equal(0, soa.Scale.X[i]);
            Assert.Equal(0, soa.Scale.Y[i]);
            Assert.Equal(0, soa.Sin[i]);
            Assert.Equal(0, soa.Cos[i]);
        }
    }

    [Fact]
    public void CopyTransformToSoa_Test()
    {        
        Soa_Transform soa = new(10);
        Transform transform = new Transform(1,2,3,4,5,6,7);
        CopyTransformToSoa(soa, ref transform, 2);
        AssertEqualsSoaTransformEntry(soa, ref transform, 2, 4);
    }
    [Fact]

    public void CopySoaToTransform_Test()
    {
        Soa_Transform soa = new(10);
        Transform expected = new Transform(0,9,8,7,6,5,4);
        Transform result = default;
        CopyTransformToSoa(soa, ref expected, 2);
        CopySoaToTransform(soa, ref result, 2);
        AssertEqualTransforms(ref expected, ref result, 4);
    }
}