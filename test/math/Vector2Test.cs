using Howl.Math;
using Xunit;

namespace Howl.Test.Math;

public class Vector2Test
{
    public Vector2Test()
    {
        
    }

    [Fact]
    public void Vector2Constructor_Test()
    {
        Vector2 vector2 = new(1,3);
        Assert.Equal(1, vector2.X);
        Assert.Equal(3, vector2.Y);
    }

    [Fact]
    public void Add_Test()
    {
        Vector2 a = new(1,7);
        Vector2 b = new(19,3);
        Vector2 r = a + b;
        Assert.Equal(20,r.X);
        Assert.Equal(10,r.Y);
    }

    [Fact]
    public void Subtract_Test()
    {
        Vector2 a = new(1,7);
        Vector2 b = new(2,5);
        Vector2 r = a - b;
        Assert.Equal(-1, r.X);
        Assert.Equal(2, r.Y);
    }

    [Fact]
    public void Divide_Test()
    {
        Vector2 a;
        Vector2 b;
        Vector2 result;

        // divide two vectors.

        a = new(4,8);
        b = new(4,2);
        result = a/b;
        
        Assert.Equal(1,result.X);
        Assert.Equal(4,result.Y);

        // divide by zero.

        a = new(0,3);
        b = new(2,3);
        result = a/b;

        Assert.Equal(0,result.X);
        Assert.Equal(1,result.Y);

        // divide by a value.

        a = new(1,2);
        float value = 2;
        result = a/value;

        Assert.Equal(0.5,result.X);
        Assert.Equal(1,result.Y);
    }

    [Fact]  
    public void Multiply_Test()
    {
        Vector2 a;
        Vector2 b;
        Vector2 result;

        // multiply two vectors.

        a = new(2,3);
        b = new(1,3);
        result = a * b;
        Assert.Equal(2, result.X);
        Assert.Equal(9, result.Y);
    
        // multiply vector by value.
        a = new(1,3);
        float value = 2;
        result = a * value;
        Assert.Equal(2, result.X);
        Assert.Equal(6, result.Y);
    }

    [Fact]
    public void Dot_Test()
    {
        Vector2 a = new(1,3);
        float dot = Vector2.Dot(a,a);
        Assert.Equal(10, dot);
    }

    [Fact]
    public void Length_Test()
    {
        Vector2 a = new(2,6);
        float length = a.Length();
        Assert.Equal(6.325, length, precision: 3);
    }

    [Fact]
    public void LengthSquared_Test()
    {
        Vector2 a = new(2,6);
        float length = a.LengthSquared();
        Assert.Equal(40, length);        
    }

    [Fact]
    public void Normalise_Test()
    {
        Vector2 a;
        Vector2 result;

        a = new(1,2);

        result = a.Normalise();
        Assert.Equal(0.45f, result.X, precision: 2);
        Assert.Equal(0.89f, result.Y, precision: 2);

        result = Vector2.Normalise(a);
        Assert.Equal(0.45f, result.X, precision: 2);
        Assert.Equal(0.89f, result.Y, precision: 2);
    }

    [Fact]
    public void Equals_Test()
    {
        Vector2 a = new(2,3);    
        Vector2 b = new(2,3);
        Assert.True(a==b);
        Assert.False(a!=b);
        Assert.True(a.Equals(b));

        Vector2 c = new(2,3);    
        Vector2 d = new(3,4);
        Assert.False(c==d);
        Assert.True(d!=c);
        Assert.False(c.Equals(d));
    }

    [Fact]
    public void Unary_Test()
    {
        Vector2 a;

        a = new(-1, 2);
        a = -a;

        Assert.Equal(1, a.X);
        Assert.Equal(-2, a.Y);
    }       

    [Fact]
    public void Distance_Test()
    {
        Vector2 a = new(1,1);
        Vector2 b = new(-23,12);
        Assert.Equal(26.4, a.Distance(b), precision: 1);
        Assert.Equal(26.4, Vector2.Distance(a,b), precision: 1);
    } 

    [Fact]
    public void DistanceSquared_Test()
    {
        Vector2 a = new(1,1);
        Vector2 b = new(-23,12);
        Assert.Equal(697, a.DistanceSquared(b), precision: 1);
        Assert.Equal(697, Vector2.DistanceSquared(a,b), precision: 1);
    }

    [Fact]
    public void InverseLength_Test()
    {
        Vector2 a = new(2,6);
        float length = a.Length();
        Assert.Equal(6.325, length, precision: 3);
    }

    [Fact]
    public void InvertX_Test()
    {
        Vector2 vector = new Vector2(1,1);
        Vector2 expected = new Vector2(-1,1);
        Assert.Equal(expected, vector.InvertX());        
        Assert.Equal(expected, Vector2.InvertX(vector));
    }

    [Fact]
    public void InvertY_Test()
    {
        Vector2 vector = new Vector2(1,1);
        Vector2 expected = new Vector2(1,-1);
        Assert.Equal(expected, vector.InvertY());        
        Assert.Equal(expected, Vector2.InvertY(vector));
    }

    [Fact]
    public void Transform_Test()
    {
        Transform transform;
        Vector2 vector;
        Vector2 position;
        Vector2 scale;
        float rotation;

        // translate test.
        vector      = Vector2.Zero;
        position    = new(2,2);
        scale       = new(1,1);
        rotation    = 0;
        transform   = new Transform(position, scale, rotation);
        Assert.Equal(new Vector2(2,2), vector.Transform(transform));

        // scale test.
        vector      = Vector2.Zero;
        position    = new(1,1);
        scale       = new(4,4);
        rotation    = 0;
        transform   = new Transform(position, scale, rotation);
        Assert.Equal(new Vector2(1,1), vector.Transform(transform));
    }

    [Fact]
    public void MinComponent_Test()
    {
        Vector2 a = new(-2,12);
        Vector2 b = new(33,-19);
        Vector2 expected = new(-2,-19);

        Assert.Equal(expected, Vector2.MinComponent(a,b));
        Assert.Equal(expected, Vector2.MinComponent(b,a));
        Assert.Equal(expected, a.MinComponent(b));
        Assert.Equal(expected, b.MinComponent(a));
    }

    [Fact]
    public void Min_Test()
    {
        Vector2 a = new (-33, 12);
        Vector2 b = new (33, 99);
        Vector2 expected = a;

        Assert.Equal(expected, a.Min(b));
        Assert.Equal(expected, b.Min(a));
        Assert.Equal(expected, Vector2.Min(a,b));
        Assert.Equal(expected, Vector2.Min(b,a));
    }

    [Fact]
    public void MaxComponent_Test()
    {
        Vector2 a = new (-120, 120);
        Vector2 b = new (33, 12);
        Vector2 expected = new(33,120);

        Assert.Equal(expected, Vector2.MaxComponent(a,b));
        Assert.Equal(expected, Vector2.MaxComponent(b,a));
        Assert.Equal(expected, a.MaxComponent(b));
        Assert.Equal(expected, b.MaxComponent(a));        
    }

    [Fact]
    public void Max_Test()
    {
        Vector2 a = new(12,-99);
        Vector2 b = new(-10,100);
        Vector2 expected = b;

        Assert.Equal(expected, a.Max(b));
        Assert.Equal(expected, b.Max(a));
        Assert.Equal(expected, Vector2.Max(a,b));
        Assert.Equal(expected, Vector2.Max(b,a));
    }

    [Fact]
    public void NearlyEquals_Test()
    {
        Vector2 a;
        Vector2 b;

        a = new Vector2(33.33333f,33.33333f);
        b = new Vector2(0,0);

        for(int i = 0; i < 3; i++)
        {
            b += new Vector2(11.11111f,11.11111f);
        }

        Assert.True(Vector2.NearlyEqual(a,b));

        a = new Vector2(-99.99999f,-99.99999f);
        b = new Vector2(0,0);

        for(int i = 0; i < 9; i++)
        {
            b -= new Vector2(11.11111f,11.11111f);
        }

        // this should be false due to rounding errors with floating point accumulation.
        Assert.False(Vector2.NearlyEqual(a,b));

        a = new Vector2(99999.99999f, 99999.99999f);
        b = new Vector2(99999.99998f, 99999.99998f);
        Assert.True(Vector2.NearlyEqual(a,b));
    }
}