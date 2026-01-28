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
        Vector2 posA,
        Vector2 posB,
        out Vector2 normal,
        out float depth
    )
    {
        normal = Vector2.Zero;
        depth = 0f;

        float distanceSqrd = posA.DistanceSquared(posB);

        float radiusSum = circleA.Radius + circleB.Radius;
        float radiusSumSq = radiusSum * radiusSum;

        if (distanceSqrd >= radiusSumSq)
            return false;

        // Apply a full up force if the two colliders are in the exact same position.
        // this also stops the whole collision system from exploding.
        if (distanceSqrd < float.Epsilon)
        {
            normal = Vector2.Up;
            depth = radiusSum;
            return true;
        }

        float distance = MathF.Sqrt(distanceSqrd);
        normal = (posB - posA).Normalise();
        depth = radiusSum - distance;

        return true;
    }
}