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
        float radiusA;
        float radiusSqrdA;

        Vector2 positionB;
        float radiusB;
        float radiusSqrdB;

        // No Collision

        positionA = new(0,0);
        radiusA = 12;
        radiusSqrdA = radiusA * radiusA;

        positionB = new(25,25);
        radiusB = 12;
        radiusSqrdB = radiusB * radiusB;
    
        Assert.False(
            Collisions.CirclesIntersect(
                positionA,
                positionB,
                radiusSqrdA,
                radiusA,
                radiusSqrdB,
                radiusB,
                out normal,
                out depth
            )
        );

        Assert.Equal(0, depth);
        Assert.Equal(Vector2.Zero, normal);
    
        // Collision

        positionA = new(0,0);
        radiusA = 12;
        radiusSqrdA = radiusA * radiusA;

        positionB = new(5f,5f);
        radiusB = 12;
        radiusSqrdB = radiusB * radiusB;
    
        Assert.True(
            Collisions.CirclesIntersect(
                positionA,
                positionB,
                radiusSqrdA,
                radiusA,
                radiusSqrdB,
                radiusB,
                out normal,
                out depth
            )
        );

        Assert.Equal(16.93f, depth, precision: 2);
        Assert.Equal(0.71, normal.X, precision: 2);
        Assert.Equal(0.71, normal.Y, precision: 2);
    }
}