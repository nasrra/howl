using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Test.Math.Shapes;

public class RectangleTest
{
    public RectangleTest()
    {
        
    }

    [Fact]
    public void Contructor_Test()
    {
        float x = 1;
        float y = 2;
        float width = 24;
        float height = 90;
        Rectangle rect = new(x,y,width,height);
        Assert.Equal(x, rect.X);
        Assert.Equal(y, rect.Y);
        Assert.Equal(width, rect.Width);
        Assert.Equal(height, rect.Height);
    }

    [Fact]
    public void Add_Test()
    {
        Rectangle lhs = new(1,2,3,4);
        Rectangle rhs = new(5,6,7,8);
        Rectangle result = lhs + rhs;
        Assert.Equal(6,result.X);
        Assert.Equal(8,result.Y);
        Assert.Equal(10,result.Width);
        Assert.Equal(12,result.Height);
    }

    [Fact]
    public void Subtract_Test()
    {
        Rectangle lhs = new(8,7,6,5);
        Rectangle rhs = new(5,6,7,8);
        Rectangle result = lhs - rhs;
        Assert.Equal(3,result.X);
        Assert.Equal(1,result.Y);
        Assert.Equal(-1,result.Width);
        Assert.Equal(-3,result.Height);        
    }

    [Fact]
    public void Multiply_Test()
    {
        Rectangle lhs = new(1,2,3,4);
        Rectangle rhs = new(5,6,7,8);
        Rectangle result = lhs * rhs;
        Assert.Equal(5,result.X);
        Assert.Equal(12,result.Y);
        Assert.Equal(21,result.Width);
        Assert.Equal(32,result.Height);        
    }

    [Fact]
    public void Divide_Test()
    {
        Rectangle lhs = new(0,1,2,3);
        Rectangle rhs = new(2,3,4,5);
        Rectangle result = lhs / rhs;
        Assert.Equal(0,result.X);
        Assert.Equal(0.33,result.Y, precision: 2);
        Assert.Equal(0.5,result.Width);
        Assert.Equal(0.6,result.Height, precision: 2);        
    }

    [Fact]
    public void Equals_Test()
    {   
        Rectangle lhs;
        Rectangle rhs;

        lhs = new(1,2,3,4);
        rhs = new(1,2,3,4);

        Assert.True(lhs == rhs);
        Assert.True(lhs.Equals(rhs));
        Assert.False(lhs != rhs);

        lhs = new(1,2,3,4);
        rhs = new(5,6,7,8);

        Assert.False(lhs == rhs);
        Assert.False(lhs.Equals(rhs));
        Assert.True(lhs != rhs);
    }
}