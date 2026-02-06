using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Test.Math.Shapes;

public class AABBTest
{
    [Fact]
    public void FloatConstructor_Test()
    {
        AABB aabb;

        float minX = 0;
        float minY = 0;
        float maxX = 10;
        float maxY = 10;
        
        aabb = new (minX, minY, maxX, maxY);
        Assert.Equal(minX, aabb.Min.X);
        Assert.Equal(minY, aabb.Min.Y);
        Assert.Equal(maxX, aabb.Max.X);
        Assert.Equal(maxY, aabb.Max.Y);
    
    }

    [Fact]
    public void VectorConstructor_Test()
    {        
        AABB aabb;

        Vector2 min = new(10,10);
        Vector2 max = new(30,30);
        aabb = new(min, max);
        Assert.Equal(min, aabb.Min);
        Assert.Equal(max, aabb.Max);
    }

    [Fact]
    public void UnionAABBConstructor_Test()
    {
        AABB a = new(
            new Vector2(-20, 20), 
            new Vector2(30,50)
        );
        
        AABB b = new(
            new Vector2(33, -90), 
            new Vector2(66,45)
        );
        
        AABB expected = new(
            new Vector2(-20, -90),
            new Vector2(66, 50)
        );

        AABB result = new(a,b);

        Assert.Equal(expected.Min, result.Min);
        Assert.Equal(expected.Max, result.Max);
    }

    [Fact]
    public void Height_Test()
    {
        AABB aabb;

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
        Assert.Equal(expectedHeight, aabb.Height);
        Assert.Equal(expectedWidth, aabb.Width);

        // test 2.
        min = new(-20,-30);
        max = new(30,30);
        expectedWidth = 50;
        expectedHeight = 60;
        
        aabb = new (min,max);
        Assert.Equal(expectedHeight, aabb.Height);
        Assert.Equal(expectedWidth, aabb.Width);
    }

    [Fact]
    public void GetCentroid_Test()
    {
        AABB aabb = new(new Vector2(-20, -30), new Vector2(15, 25));
        Vector2 expected = new Vector2(-2.5f, -2.5f);
        Assert.Equal(expected, aabb.GetCentroid());
    }

    [Fact]
    public void SubtractVector_Test()
    {
        AABB aabb;
        AABB expected;
        Vector2 vector;

        aabb = new(Vector2.Zero, Vector2.Zero);
        vector = new Vector2(12, -20);
        expected = new AABB(new Vector2(-12, 20), new Vector2(-12, 20));
        Assert.Equal(expected, aabb - vector);

        aabb = new(new Vector2(-10, -10), new Vector2(10, 10));
        vector = new Vector2(-10, -10);
        expected = new AABB(Vector2.Zero, new Vector2(20,20));
        Assert.Equal(expected, aabb - vector);

        aabb = new(new Vector2(-13, -2), new Vector2(22, 55));
        vector = new Vector2(-16, 3);
        expected = new AABB(new Vector2(3, -5), new Vector2(38, 52));
        Assert.Equal(expected, aabb - vector);
    }

    [Fact]
    public void AddVector_Test()
    {
        AABB aabb;
        AABB expected;
        Vector2 vector;

        aabb = new(Vector2.Zero, Vector2.Zero);
        vector = new Vector2(10,10);
        expected = new AABB(vector,vector);
        Assert.Equal(expected, aabb + vector);

        aabb = new(new Vector2(-10,-10), new Vector2(10,10));
        vector = new Vector2(-10,-10);
        expected = new AABB(new Vector2(-20,-20),new Vector2(0,0));
        Assert.Equal(expected, aabb + vector);

        aabb = new(new Vector2(12,-99), new Vector2(200,-33));
        vector = new Vector2(4,-33);
        expected = new AABB(new Vector2(16,-132),new Vector2(204,-66));
        Assert.Equal(expected, aabb + vector);
    }

    [Fact]
    public void VectorIntersect_Test()
    {
        AABB aabb = new AABB(0,0,10,10);
        Vector2 vector;

        vector = new Vector2(5,5);
        Assert.True(AABB.Intersect(aabb, vector));
        Assert.True(AABB.Intersect(vector, aabb));

        vector = new Vector2(10,10);
        Assert.True(AABB.Intersect(aabb, vector));
        Assert.True(AABB.Intersect(vector, aabb));

        vector = new Vector2(12,12);
        Assert.False(AABB.Intersect(aabb, vector));
        Assert.False(AABB.Intersect(vector, aabb));

        vector = new Vector2(-12,12);
        Assert.False(AABB.Intersect(aabb, vector));
        Assert.False(AABB.Intersect(vector, aabb));
    }

    [Fact]
    public void AABBIntersect_Test()
    {
        AABB aabb = new AABB(0,0,10,10);
        AABB query;

        query = new AABB(0,0,5,5);
        Assert.True(AABB.Intersect(aabb, query));

        query = new AABB(10,10,15,15);
        Assert.False(AABB.Intersect(aabb, query));

        query = new AABB(11,11,15,15);
        Assert.False(AABB.Intersect(aabb, query));
    }

    [Fact]
    public void LineSegmentIntersect_Test()
    {
        AABB aabb = new AABB(0,0,10,10);

        Vector2 lineSegmentStart;
        Vector2 lineSegmentEnd;

        lineSegmentStart = new Vector2(-1,-1);
        lineSegmentEnd = new Vector2(10,10);

        Assert.True(AABB.Intersect(aabb, lineSegmentStart, lineSegmentEnd));

        lineSegmentEnd = new Vector2(-1,10);
        Assert.False(AABB.Intersect(aabb, lineSegmentStart, lineSegmentEnd));
    }
}