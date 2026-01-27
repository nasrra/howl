using System;

namespace Howl.Math;

public static class Util
{
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    public static float ToRadians(float degrees)
    {
        return (float)((double)degrees * (System.Math.PI / 180.0));
    }

    public static bool CirclesIntersect(
        Circle circleA,
        Circle circleB,
        Vector2 circleAPosition,
        Vector2 circleBPosition,
        out Vector2 normal,
        out float depth
    )
    {
        normal = Vector2.Zero;
        depth = 0;

        float distanceSqrd = circleAPosition.DistanceSquared(circleBPosition);
        float radii = circleA.RadiusSquared + circleB.RadiusSquared;

        if(distanceSqrd >= radii)
        {
            return false;
        } 

        normal = (circleBPosition - circleAPosition).Normalise();
        normal = normal == Vector2.Zero || normal == Vector2.NaN
        ? Vector2.One
        : normal; 
        depth = circleA.Radius + circleB.Radius - MathF.Sqrt(distanceSqrd);

        return true;
    }
}