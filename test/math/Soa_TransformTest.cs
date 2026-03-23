using static Howl.Math.Soa_Transform;

using Howl.Math;

namespace Howl.Test.Math;

public class Soa_TransformTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 10;
        Soa_Transform soa = new(capacity);
        
        Assert.Equal(capacity, soa.Positions.X.Length);
        Assert.Equal(capacity, soa.Positions.Y.Length);
        Assert.Equal(capacity, soa.Scales.X.Length);        
        Assert.Equal(capacity, soa.Scales.Y.Length);
        Assert.Equal(capacity, soa.Sins.Length);        
        Assert.Equal(capacity, soa.Coses.Length);
        
        for(int i = 0; i < capacity; i++)
        {
            Assert.Equal(0, soa.Positions.X[i]);
            Assert.Equal(0, soa.Positions.Y[i]);
            Assert.Equal(0, soa.Scales.X[i]);
            Assert.Equal(0, soa.Scales.Y[i]);
            Assert.Equal(0, soa.Sins[i]);
            Assert.Equal(0, soa.Coses[i]);
        }
    }

    [Fact]
    public void CopyTransformToSoa_Test()
    {        
        Soa_Transform soa = new(10);
        Transform transform = new Transform(1,2,3,4,5,6,7);
        CopyTransformToSoa(soa, ref transform, 2);
        Soa_TransformAssert.EntryEquals(soa, ref transform, 2, 4);
    }
    
    [Fact]
    public void CopySoaToTransform_Test()
    {
        Soa_Transform soa = new(10);
        Transform expected = new Transform(0,9,8,7,6,5,4);
        Transform result = default;
        CopyTransformToSoa(soa, ref expected, 2);
        CopySoaToTransform(soa, ref result, 2);
        TransformAssert.Equals(ref expected, ref result, 4);
    }
}