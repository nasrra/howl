using Howl.Math;
using Xunit;

namespace Howl.Test.Math;

public class Vector3Test
{
    public Vector3Test()
    {
        
    }

    [Fact]
    public void Constructor_Test()
    {
        Vector2 vector2;
        Vector3 vector3;
        
        vector3 = new(1,2,3);
        Assert.Equal(1, vector3.X);
        Assert.Equal(2, vector3.Y);
        Assert.Equal(3, vector3.Z);

        vector2 = new(1,2);
        vector3 = new(vector2, 4);
        Assert.Equal(1, vector3.X);
        Assert.Equal(2, vector3.Y);
        Assert.Equal(4, vector3.Z);
    }

    [Fact]
    public void Add_Test()
    {
        Vector3 a = new(1,7,8);
        Vector3 b = new(19,3,12);
        Vector3 r = a + b;
        Assert.Equal(20,r.X);
        Assert.Equal(10,r.Y);
        Assert.Equal(20,r.Z);
    }

    [Fact]
    public void Subtract_Test()
    {
        Vector3 a = new(1,7,2);
        Vector3 b = new(2,5,6);
        Vector3 r = a - b;
        Assert.Equal(-1, r.X);
        Assert.Equal(2, r.Y);
        Assert.Equal(-4, r.Z);
    }

    [Fact]
    public void Divide_Test()
    {
        Vector3 a;
        Vector3 b;
        Vector3 result;

        // divide two vectors.

        a = new(4,8,9);
        b = new(4,2,3);
        result = a/b;
        
        Assert.Equal(1,result.X);
        Assert.Equal(4,result.Y);
        Assert.Equal(3,result.Z);

        // divide by zero.

        a = new(0,3,6);
        b = new(2,3,12);
        result = a/b;

        Assert.Equal(0,result.X);
        Assert.Equal(1,result.Y);
        Assert.Equal(0.5f,result.Z, precision:1);

        // divide by a value.

        a = new(1,2,4);
        float value = 2;
        result = a/value;

        Assert.Equal(0.5,result.X, precision:1);
        Assert.Equal(1,result.Y);
        Assert.Equal(2,result.Z);
    }

    [Fact]  
    public void Multiply_Test()
    {
        Vector3 a;
        Vector3 b;
        Vector3 result;

        // multiply two vectors.

        a = new(2,3,4);
        b = new(1,3,12);
        result = a * b;
        Assert.Equal(2, result.X);
        Assert.Equal(9, result.Y);
        Assert.Equal(48, result.Z);

        // multiply vector by value.
        a = new(1,3,10);
        float value = 2;
        result = a * value;
        Assert.Equal(2, result.X);
        Assert.Equal(6, result.Y);
        Assert.Equal(20, result.Z);
    }

    [Fact]
    public void Dot_Test()
    {
        Vector3 a = new(1,3,6);
        float dot = Vector3.Dot(a,a);
        Assert.Equal(46, dot);
    }

    [Fact]
    public void Length_Test()
    {
        Vector3 a = new(2,6,1);
        float length = a.Length();
        Assert.Equal(6.403, length, precision: 3);
    }

    [Fact]
    public void LengthSquared_Test()
    {
        Vector3 a = new(2,6,3);
        float length = a.LengthSquared();
        Assert.Equal(49, length);        
    }

    [Fact]
    public void Normalise_Test()
    {
        Vector3 a;
        Vector3 result;

        a = new(1,2,3);
        result = a.Normalise();
        Assert.Equal(0.2673f, result.X, precision: 4);
        Assert.Equal(0.5345f, result.Y, precision: 4);
        Assert.Equal(0.8018f, result.Z, precision: 4);
    }

    [Fact]
    public void Equals_Test()
    {
        Vector3 a = new(2,3,6);    
        Vector3 b = new(2,3,6);
        Assert.True(a==b);
        Assert.False(a!=b);
        Assert.True(a.Equals(b));

        Vector3 c = new(2,3,1);    
        Vector3 d = new(3,4,5);
        Assert.False(c==d);
        Assert.True(d!=c);
        Assert.False(c.Equals(d));
    }

    [Fact]
    public void Unary_Test()
    {
        Vector3 a;

        a = new(-1, 2, 4);
        a = -a;

        Assert.Equal(1, a.X);
        Assert.Equal(-2, a.Y);
        Assert.Equal(-4, a.Z);
    }       
}