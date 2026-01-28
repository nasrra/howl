using Howl.Math;
using Howl.Physics;
using Xunit;

namespace Howl.Test.Physics;

public class CollisionsTest
{
    [Fact]
    public void CirclesIntersect_Test()
    {
        Vector2 normal;
        float depth;

        Vector2 positionA;
        Circle circleA;

        Vector2 positionB;
        Circle circleB;

        // No Collision Test.

        positionA = new(0,0);
        circleA = new(0,0,12);

        positionB = new(25,25);
        circleB = new(0,0,12);
    
        Assert.False(
            Util.CirclesIntersect(
                circleA,
                circleB,
                positionA,
                positionB,
                out normal,
                out depth
            )
        );

        Assert.Equal(0, depth);
        Assert.Equal(Vector2.Zero, normal);
    
        // Collision Test.

        positionA = new(0,0);
        positionB = new(5,5);

        Assert.True(
            Util.CirclesIntersect(
                circleA,
                circleB,
                positionA,
                positionB,
                out normal,
                out depth
            )
        );

        Assert.Equal(16.93f, depth, precision: 2);
        Assert.Equal(0.71, normal.X, precision: 2);
        Assert.Equal(0.71, normal.Y, precision: 2);
    }
}