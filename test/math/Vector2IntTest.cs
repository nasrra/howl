using Howl.Math;
using Xunit;

namespace Howl.Test.Math;

public class Vector2IntTest
{
    public Vector2IntTest()
    {
        
    }

    [Fact]
    public void Vector2Constructor()
    {
        Vector2Int vector2 = new(1,3);
        Assert.Equal(1, vector2.X);
        Assert.Equal(3, vector2.Y);
    }

    [Fact]
    public void Vector2Add()
    {
        Vector2Int a = new(1,7);
        Vector2Int b = new(19,3);
        Vector2Int r = a + b;
        Assert.Equal(20,r.X);
        Assert.Equal(10,r.Y);
    }

    [Fact]
    public void Vector2Subtract()
    {
        Vector2Int a = new(1,7);
        Vector2Int b = new(2,5);
        Vector2Int r = a - b;
        Assert.Equal(-1, r.X);
        Assert.Equal(2, r.Y);
    }

    [Fact]
    public void Vector2Divide()
    {
        Vector2Int a = new(4,8);
        Vector2Int b = new(4,2);
        Vector2Int r = a/b;
        Assert.Equal(1,r.X);
        Assert.Equal(4,r.Y);

        // check dividde by zero.

        Vector2Int c = new(0,3);
        Vector2Int d = new(2,3);
        r = c/d;
        Assert.Equal(0,r.X);
        Assert.Equal(1,r.Y);
    }

    [Fact]  
    public void Vector2Multiply()
    {
        Vector2Int a = new(2,3);
        Vector2Int b = new(1,3);
        Vector2Int r = a * b;
        Assert.Equal(2, r.X);
        Assert.Equal(9, r.Y);
    }

    [Fact]
    public void Vector2Equals()
    {
        Vector2Int a = new(2,3);    
        Vector2Int b = new(2,3);
        Assert.True(a==b);
        Assert.False(a!=b);
        Assert.True(a.Equals(b));

        Vector2Int c = new(2,3);    
        Vector2Int d = new(3,4);
        Assert.False(c==d);
        Assert.True(d!=c);
        Assert.False(c.Equals(d));
    }       
}