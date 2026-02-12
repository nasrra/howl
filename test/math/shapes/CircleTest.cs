using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Test.Math.Shapes;

public class CircleTest
{
    [Fact]
    public void Constructor_Test()
    {
        Circle circle = new Circle(12, -23, 123);
        Assert.Equal(12, circle.X);
        Assert.Equal(-23, circle.Y);
        Assert.Equal(123, circle.Radius);
    }

    [Fact]
    public void Scale_Test()
    {
        Circle circle;

        circle = new Circle(12, 32, 33);
        circle = circle.Scale(2);
        Assert.Equal(12, circle.X);
        Assert.Equal(32, circle.Y);
        Assert.Equal(66, circle.Radius, precision: 1);

        circle = new Circle(12, 32, 33);
        circle = Circle.Scale(circle, 2);
        Assert.Equal(12, circle.X);
        Assert.Equal(32, circle.Y);
        Assert.Equal(66, circle.Radius, precision: 1);

        circle = new Circle(14, 56, 11);
        circle = circle.Scale(new Vector2(3,1));
        Assert.Equal(14, circle.X);
        Assert.Equal(56, circle.Y);
        Assert.Equal(33, circle.Radius, precision: 1);

        circle = new Circle(14, 56, 11);
        circle = Circle.Scale(circle, new Vector2(3,1));
        Assert.Equal(14, circle.X);
        Assert.Equal(56, circle.Y);
        Assert.Equal(33, circle.Radius, precision: 1);
    }

    [Fact]
    public void GetAABB_Test()
    {
        Circle circle = new Circle(0,0,3);
        AABB aabb = circle.GetAABB();
        Assert.Equal(new Vector2(-3,-3), aabb.Min);
        Assert.Equal(new Vector2(3,3), aabb.Max);
    }
}