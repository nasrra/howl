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
        radiusA = 1;
        radiusSqrdA = radiusA * radiusA;

        positionB = new(2,2);
        radiusB = 1;
        radiusSqrdB = radiusA * radiusA;
    
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
        radiusA = 1;
        radiusSqrdA = radiusA * radiusA;

        positionB = new(0.5f,0.5f);
        radiusB = 1;
        radiusSqrdB = radiusA * radiusA;
    
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

        Assert.Equal(1.29f, depth, precision: 2);
        Assert.Equal(0.71, normal.X, precision: 2);
        Assert.Equal(0.71, normal.Y, precision: 2);
    }
}