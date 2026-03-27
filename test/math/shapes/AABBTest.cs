using Howl.Math;
using Howl.Math.Shapes;
using static Howl.Math.Shapes.Aabb;

namespace Howl.Test.Math.Shapes;

public class AABBTest
{

    private const float NearlyEqualEpsilon = 1e-6f;
    
    [Fact]
    public void FloatConstructor_Test()
    {
        Aabb aabb;

        float minX = 0;
        float minY = 0;
        float maxX = 10;
        float maxY = 10;
        
        aabb = new(minX, minY, maxX, maxY);
        Assert.Equal(minX, aabb.MinX);
        Assert.Equal(minY, aabb.MinY);
        Assert.Equal(maxX, aabb.MaxX);
        Assert.Equal(maxY, aabb.MaxY);
    
    }

    [Fact]
    public void VectorConstructor_Test()
    {        
        Aabb aabb;
        Vector2 min = new(10,10);
        Vector2 max = new(30,30);
        aabb = new(min, max);
        Assert.Equal(min.X, aabb.MinX);
        Assert.Equal(min.Y, aabb.MinY);
        Assert.Equal(max.X, aabb.MaxX);
        Assert.Equal(max.Y, aabb.MaxY);
    }

    [Fact]
    public void MinAndMaxVector_Test()
    {
        Aabb aabb;
        Vector2 min = new(10,10);
        Vector2 max = new(30,30);
        aabb = new(min, max);
        Assert.Equal(min, MinVector(aabb));
        Assert.Equal(max, MaxVector(aabb));
    }

    [Fact]
    public void UnionAABBConstructor_Test()
    {
        Aabb a = new(
            new Vector2(-20, 20), 
            new Vector2(30,50)
        );
        
        Aabb b = new(
            new Vector2(33, -90), 
            new Vector2(66,45)
        );
        
        Aabb expected = new(
            new Vector2(-20, -90),
            new Vector2(66, 50)
        );

        Aabb result = Union(a,b);

        Assert.Equal(expected.MinX, result.MinX);
        Assert.Equal(expected.MinY, result.MinY);
        Assert.Equal(expected.MaxX, result.MaxX);
        Assert.Equal(expected.MaxY, result.MaxY);
    }

    [Fact]
    public void Height_Test()
    {
        Aabb aabb;

        Vector2 min;
        Vector2 max;
        float expectedHeight;
        float expectedWidth;

        // test 1.
        min = new(10,10);
        max = new(30,30);
        expectedWidth = 20;
        expectedHeight = 20;
        
        aabb = new (min,max);
        Assert.Equal(expectedHeight, Height(aabb));
        Assert.Equal(expectedWidth, Width(aabb));

        // test 2.
        min = new(-20,-30);
        max = new(30,30);
        expectedWidth = 50;
        expectedHeight = 60;
        
        aabb = new (min,max);
        Assert.Equal(expectedHeight, Height(aabb));
        Assert.Equal(expectedWidth, Width(aabb));
    }

    [Fact]
    public void Center_Test()
    {
        Aabb aabb = new(new Vector2(-20, -30), new Vector2(15, 25));
        Vector2 expected = new Vector2(-2.5f, -2.5f);
        Assert.Equal(expected, CalculateCentroid(aabb));
    }

    [Fact]
    public void SubtractVector_Test()
    {
        Aabb aabb;
        Aabb expected;
        Vector2 vector;

        aabb = new(Vector2.Zero, Vector2.Zero);
        vector = new Vector2(12, -20);
        expected = new Aabb(new Vector2(-12, 20), new Vector2(-12, 20));
        Assert.Equal(expected, aabb - vector);

        aabb = new(new Vector2(-10, -10), new Vector2(10, 10));
        vector = new Vector2(-10, -10);
        expected = new Aabb(Vector2.Zero, new Vector2(20,20));
        Assert.Equal(expected, aabb - vector);

        aabb = new(new Vector2(-13, -2), new Vector2(22, 55));
        vector = new Vector2(-16, 3);
        expected = new Aabb(new Vector2(3, -5), new Vector2(38, 52));
        Assert.Equal(expected, aabb - vector);
    }

    [Fact]
    public void AddVector_Test()
    {
        Aabb aabb;
        Aabb expected;
        Vector2 vector;

        aabb = new(Vector2.Zero, Vector2.Zero);
        vector = new Vector2(10,10);
        expected = new Aabb(vector,vector);
        Assert.Equal(expected, aabb + vector);

        aabb = new(new Vector2(-10,-10), new Vector2(10,10));
        vector = new Vector2(-10,-10);
        expected = new Aabb(new Vector2(-20,-20),new Vector2(0,0));
        Assert.Equal(expected, aabb + vector);

        aabb = new(new Vector2(12,-99), new Vector2(200,-33));
        vector = new Vector2(4,-33);
        expected = new Aabb(new Vector2(16,-132),new Vector2(204,-66));
        Assert.Equal(expected, aabb + vector);
    }

    [Fact]
    public void VectorIntersect_Test()
    {
        Aabb aabb = new Aabb(0,0,10,10);
        Vector2 vector;

        vector = new Vector2(5,5);
        Assert.True(Aabb.Intersect(aabb, vector));
        Assert.True(Aabb.Intersect(vector, aabb));

        vector = new Vector2(10,10);
        Assert.True(Aabb.Intersect(aabb, vector));
        Assert.True(Aabb.Intersect(vector, aabb));

        vector = new Vector2(12,12);
        Assert.False(Aabb.Intersect(aabb, vector));
        Assert.False(Aabb.Intersect(vector, aabb));

        vector = new Vector2(-12,12);
        Assert.False(Aabb.Intersect(aabb, vector));
        Assert.False(Aabb.Intersect(vector, aabb));
    }

    [Fact]
    public void AABBIntersect_Test()
    {
        Aabb aabb = new Aabb(0,0,10,10);
        Aabb query;

        query = new Aabb(0,0,5,5);
        Assert.True(Aabb.Intersect(aabb, query));

        query = new Aabb(10,10,15,15);
        Assert.False(Aabb.Intersect(aabb, query));

        query = new Aabb(11,11,15,15);
        Assert.False(Aabb.Intersect(aabb, query));
    }

    [Fact]
    public void LineSegmentIntersect_Test()
    {
        Aabb aabb = new Aabb(0,0,10,10);

        Vector2 lineSegmentStart;
        Vector2 lineSegmentEnd;

        lineSegmentStart = new Vector2(-1,-1);
        lineSegmentEnd = new Vector2(10,10);

        Assert.True(LineIntersect(aabb, lineSegmentStart, lineSegmentEnd));

        lineSegmentEnd = new Vector2(-1,10);
        Assert.False(LineIntersect(aabb, lineSegmentStart, lineSegmentEnd));
    }

    [Fact]
    public void NearlyEquals_Test()
    {
        Aabb a;
        Aabb b;

        a = new Aabb(33.33333f,33.33333f,33.33333f,33.33333f);
        b = new Aabb(0,0,0,0);

        for(int i = 0; i < 3; i++)
        {
            b += new Vector2(11.11111f,11.11111f);
        }

        Assert.True(Aabb.NearlyEqual(a,b,NearlyEqualEpsilon));

        a = new Aabb(-99.99999f,-99.99999f,-99.99999f,-99.99999f);
        b = new Aabb(0,0,0,0);

        for(int i = 0; i < 9; i++)
        {
            b -= new Vector2(11.11111f,11.11111f);
        }

        // this should be false due to rounding errors with floating point accumulation.
        Assert.False(Aabb.NearlyEqual(a,b,NearlyEqualEpsilon));

        a = new Aabb(99999.99999f, 99999.99999f, 99999.99999f, 99999.99999f);
        b = new Aabb(99999.99998f, 99999.99998f, 99999.99998f, 99999.99998f);
        Assert.True(Aabb.NearlyEqual(a,b,NearlyEqualEpsilon));
    }
}