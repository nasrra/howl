using Howl.Math;
using Howl.Math.Shapes;
using static Howl.Math.Shapes.Circle;
using static Howl.Math.Shapes.AABB;
using static Howl.Math.Math;

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
        circle = Scale(circle, 2);
        Assert.Equal(12, circle.X);
        Assert.Equal(32, circle.Y);
        Assert.Equal(66, circle.Radius, precision: 1);

        circle = new Circle(12, 32, 33);
        circle = Scale(circle, 2);
        Assert.Equal(12, circle.X);
        Assert.Equal(32, circle.Y);
        Assert.Equal(66, circle.Radius, precision: 1);

        circle = new Circle(14, 56, 11);
        circle = Scale(circle, new Vector2(3,1));
        Assert.Equal(14, circle.X);
        Assert.Equal(56, circle.Y);
        Assert.Equal(33, circle.Radius, precision: 1);

        circle = new Circle(14, 56, 11);
        circle = Scale(circle, new Vector2(3,1));
        Assert.Equal(14, circle.X);
        Assert.Equal(56, circle.Y);
        Assert.Equal(33, circle.Radius, precision: 1);
    }

    [Fact]
    public void GetAABB_Test()
    {
        Circle circle = new Circle(0,0,3);
        AABB aabb = GetAABB(circle);
        Assert.Equal(new Vector2(-3,-3), MinVector(aabb));
        Assert.Equal(new Vector2(3,3), MaxVector(aabb));
    }

    [Fact]
    public void NearlyEqual_Test()
    {
        Circle a;
        Circle b;

        a = new Circle(0.1f, 0.1f, 1);
        b = new Circle(0.1f, 0.1f, 1);
        Assert.True(NearlyEqual(a,b,1e-4f));

        a = new Circle(0.1002f, 0.1002f, 1);
        b = new Circle(0.1f, 0.1f, 1);
        Assert.False(NearlyEqual(a,b,1e-4f));
    }

    [Fact]
    public void Transform_Test()
    {
        Circle circle;
        Circle expected;
        Circle tCircle;
        Transform transform;

        // test 1.

        circle = new Circle(0,0,3);
        expected = new Circle(12, 23, 3);
        transform = new Transform(new Vector2(12,23), 1, 0);
        tCircle = Transform(circle, transform);
        Assert.True(NearlyEqual(expected, tCircle, 1e-4f));

        // test 2.

        circle = new Circle(1,1,3);
        expected = new Circle(6, 6, 9);
        transform = new Transform(new Vector2(3,3), 3, 0);
        tCircle = Transform(circle, transform);
        Assert.True(NearlyEqual(expected, tCircle, 1e-4f));
    }
}