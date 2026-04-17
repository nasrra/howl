using Howl.Math;

namespace Howl.Test.Math;

public class Test_Soa_Transform
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
        Soa_Transform.Insert(soa, 2, transform);
        Assert_Soa_Transform.EntryEqual(transform, 4, 2, soa);
    }
    
    [Fact]
    public void CopySoaToTransform_Test()
    {
        Soa_Transform soa = new(10);
        Transform expected = new Transform(0,9,8,7,6,5,4);
        Transform result = default;
        Soa_Transform.Insert(soa, 2, expected);
        Soa_Transform.CopySoaToTransform(soa, ref result, 2);
        Assert_Transform.Equals(ref expected, ref result, 4);
    }

    [Fact]
    public void EnforeNil_Test()
    {
        Debug.Log.Suppress = true;

        Soa_Transform soa = new(3);
        
        soa.Positions.X[0] = 12;
        soa.Positions.Y[0] = -32;
        soa.Scales.X[0] = 90;
        soa.Scales.Y[0] = -123;
        soa.Sins[0] = 43;
        soa.Coses[0] = 098;

        Soa_Transform.EnforceNil(soa);

        Assert.Equal(0, soa.Positions.X[0]);
        Assert.Equal(0, soa.Positions.Y[0]);
        Assert.Equal(0, soa.Scales.X[0]);
        Assert.Equal(0, soa.Scales.Y[0]);
        Assert.Equal(0, soa.Sins[0]);
        Assert.Equal(0, soa.Coses[0]);

        Debug.Log.Suppress = false;
    }
}