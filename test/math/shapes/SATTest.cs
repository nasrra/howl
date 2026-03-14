using Howl.Math;
using Howl.Math.Shapes;
using Howl.Physics;
using Xunit;
using static Howl.Math.Shapes.PolygonRectangle;
using static Howl.Math.Shapes.Circle;
using static Howl.Math.Shapes.SAT;

namespace Howl.Test.Math.Shapes;

public class SATTest
{
    [Fact]
    public void CirclesIntersect_Test()
    {
        Vector2 normal;
        float depth;

        Circle circleA;
        Circle circleB;

        circleA = new(0,0,12);
        circleB = new(25,25,12);
    
        Assert.False(
            SAT.Intersect(
                circleA,
                circleB,
                out normal,
                out depth
            )
        );
    }

    [Fact]
    public void CircleIntersectSuccess_Test()
    {
        Vector2 normal;
        float depth;

        Circle circleA;
        Circle circleB;

        circleA = new(0,0,12);
        circleB = new(5,5,12);

        Assert.True(
            SAT.Intersect(
                circleA,
                circleB,
                out normal,
                out depth
            )
        );

        Assert.Equal(16.93f, depth, precision: 2);
        Assert.Equal(0.71, normal.X, precision: 2);
        Assert.Equal(0.71, normal.Y, precision: 2);        
    }

    [Fact]
    public void RectangleIntersectFail_Test()
    {
        PolygonRectangle rectangleA;
        PolygonRectangle rectangleB;
        Vector2 normal;
        float depth;
        bool intersects;

        // no intersection.
        rectangleA = new PolygonRectangle(0, 0, 10, 10);
        rectangleB = new PolygonRectangle(10.01f, 10.01f, 10, 10);
        intersects = SAT.Intersect(rectangleA, rectangleB, Centroid(rectangleA), Centroid(rectangleB), out normal, out depth);
        Assert.False(intersects);
    }

    [Fact]
    public void RectangleIntersectSuccess_Test()
    {        
        PolygonRectangle rectangleA;
        PolygonRectangle rectangleB;
        Vector2 normal;
        float depth;
        bool intersects;

        rectangleA = new PolygonRectangle(0,0,10,10);
        rectangleB = new PolygonRectangle(5,5,10,10);
        intersects = SAT.Intersect(rectangleA, rectangleB, Centroid(rectangleA), Centroid(rectangleB), out normal, out depth);
        Assert.True(intersects);        
        Assert.Equal(5, depth);
        Assert.Equal(Vector2.Up, normal);

        // this counts as intersecting as 10,10 is 'touching' eachother.
        // may want to change later down the line, adding a:
        // if(depth > 0) or if(depth >= float.Epsilon)
        // check at the end of the intersect function before returning true.
        rectangleA = new PolygonRectangle(0,0,10,10);
        rectangleB = new PolygonRectangle(10,10,10,10);
        intersects = SAT.Intersect(rectangleA, rectangleB, Centroid(rectangleA), Centroid(rectangleB), out normal, out depth);
        Assert.True(intersects);
        Assert.Equal(0, depth);
        Assert.Equal(Vector2.Up, normal);
    }

    [Fact]
    public void RotationalRectangleIntersect_Test()
    {
        PolygonRectangle rectangleA;
        PolygonRectangle rectangleB;
        Vector2 normal;
        float depth;
        bool intersects;

        // foutry-five degree rotated rectangle.
        rectangleA = new PolygonRectangle(
            [
                new Vector2(-10f,0f),
                new Vector2(0f,10f),
                new Vector2(10f,0f),
                new Vector2(0f,-10f)
            ]
        );

        // axis-aligned rectangle.
        rectangleB = new PolygonRectangle(5,10,10,10);
        intersects = SAT.Intersect(rectangleA, rectangleB, Centroid(rectangleA), Centroid(rectangleB), out normal, out depth);
        Assert.True(intersects);
        Assert.Equal(3.54f, depth, precision: 2);
        Assert.Equal(0.71f, normal.X, precision: 2);
        Assert.Equal(0.71f, normal.Y, precision: 2);
    }

    [Fact]
    public void RectangleCircleIntersectFail_Test()
    {
        PolygonRectangle rectangle;
        Circle circle;
        Vector2 normal;
        float depth;
        bool intersects;

        rectangle = new PolygonRectangle(0,0,10,10);
        circle = new Circle(20,20,5);
        intersects = SAT.Intersect(rectangle, circle, Centroid(rectangle), Center(circle), out normal, out depth);

        Assert.False(intersects);
    }


    [Fact]
    public void RectangleCircleIntersectSuccess_Test()
    {
        PolygonRectangle rectangle;
        Circle circle;
        Vector2 normal;
        float depth;
        bool intersects;

        // postional.

        rectangle = new PolygonRectangle(0,0,10,10);
        circle = new Circle(5f,0f,5);
        intersects = SAT.Intersect(rectangle, circle, Centroid(rectangle), Center(circle), out normal, out depth);

        Assert.True(intersects);
        Assert.Equal(5, depth);
        Assert.Equal(0, normal.X);        
        Assert.Equal(1f, normal.Y);

        // rotational.
        
        // foutry-five degree rotated rectangle.
        rectangle = new PolygonRectangle(
            [
                new Vector2(-10f,0f),
                new Vector2(0f,10f),
                new Vector2(10f,0f),
                new Vector2(0f,-10f)
            ]
        );

        circle = new Circle(0f,10f,5);

        intersects = SAT.Intersect(rectangle, circle, Centroid(rectangle), Center(circle), out normal, out depth);
        Assert.True(intersects);
        // Assert.Equal(0, depth);
        // Assert.Equal(0f, normal.X, precision: 2);        
        // Assert.Equal(0f, normal.Y, precision: 2);


        Assert.Equal(5, depth);
        // note that the result is not Vector2.Up
        // circles will always have a diagonal normal when colliding with
        // corners of a box.
        Assert.Equal(-0.71f, normal.X, precision: 2);        
        Assert.Equal(0.71f, normal.Y, precision: 2);
    }

    [Fact]
    public void FindCircleContactPoints_Test()
    {
        Circle a;
        Circle b;
        Vector2 contactPoint;

        a = new Circle(-1,1,2);
        b = new Circle(-2,1,2);
        FindContactPoints(a,b, out contactPoint);
        Assert.Equal(-3,contactPoint.X);
        Assert.Equal(1,contactPoint.Y,precision:2);

        a = new Circle(3, -2, 3);
        b = new Circle(3, -5, 3);
        FindContactPoints(a,b, out contactPoint);
        Assert.Equal(3,contactPoint.X);
        Assert.Equal(-5f,contactPoint.Y,precision:2);
    }

    [Fact]
    public void FindPolygonContactPoints_Test()
    {
        float cX1;
        float cY1;
        float cX2;
        float cY2;
        int contactCount;

        Span<float> xA = [0.5f, -0.5f, -0.5f, 0.5f];
        Span<float> xB = [0.5f, -1.5f, -1.5f, 0.5f];
        Span<float> yA = [0.5f, 0.5f, -0.5f, -0.5f];
        Span<float> yB = [0.5f, 0.5f, -1.5f, -1.5f];
        FindContactPoints(xA, yA, xB, yB, PolygonContactPointEpsilon, out cX1, out cY1, out cX2, out cY2, out contactCount);
        Assert.Equal(0.5f, cX1);
        Assert.Equal(0.5f, cY1);
        Assert.Equal(0.5f, cX2);
        Assert.Equal(-0.5f, cY2);
        Assert.Equal(2, contactCount);
    }
}