using System;
using Howl.Math;

namespace Howl.Physics;

public static class Collisions
{
    public static bool CirclesIntersect(
        Vector2 centerA, 
        Vector2 centerB, 
        float radiusSqrdA,
        float radiusA, 
        float radiusSqrdB,
        float radiusB,
        out Vector2 normal,
        out float depth
    )
    {
        normal = Vector2.Zero;
        depth = 0;

        float distanceSqrd = centerA.DistanceSquared(centerB);
        float radii = radiusSqrdA + radiusSqrdB;

        if(distanceSqrd >= radii)
        {
            return false;
        } 

        normal = (centerB - centerA).Normalise();
        normal = normal == Vector2.Zero || normal == Vector2.NaN
        ? Vector2.One
        : normal; 
        depth = radiusA + radiusB - MathF.Sqrt(distanceSqrd);

        return true;
    }
}