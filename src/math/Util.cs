using System.Runtime.CompilerServices;

namespace Howl.Math;

public static class Util
{
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float ToRadians(float degrees)
    {
        return (float)((double)degrees * (System.Math.PI / 180.0));
    }

    /// <summary>
    /// Gets the closest point along a line segment towards a given point. 
    /// </summary>
    /// <param name="lineSegmentStart">the beginning of the line-segment.</param>
    /// <param name="lineSegmentEnd">the end of the line-segment.</param>
    /// <param name="queryPoint">the point to find the closest point towards.</param>
    /// <param name="closestPoint">The closest point along the line segment towards the query point.</param>
    /// <param name="distanceSquared">The distance squared from the closest point to the query point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClosestPoint(Vector2 lineSegmentStart, Vector2 lineSegmentEnd, Vector2 queryPoint, out Vector2 closestPoint, out float distanceSquared)
    {
        closestPoint = ClosestPoint(lineSegmentStart, lineSegmentEnd, queryPoint);
        distanceSquared = Vector2.DistanceSquared(queryPoint, closestPoint);
    }

    /// <summary>
    /// Gets the closest point along a line segment towards a given point. 
    /// </summary>
    /// <param name="lineSegmentStart">the beginning of the line-segment.</param>
    /// <param name="lineSegmentEnd">the end of the line-segment.</param>
    /// <param name="queryPoint">the point to find the closest point towards.</param>
    /// <returns>The closest point along the line segment towards the query point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 ClosestPoint(Vector2 lineSegmentStart, Vector2 lineSegmentEnd, Vector2 queryPoint)
    {
        Vector2 lineDistance = lineSegmentEnd - lineSegmentStart;
        Vector2 pointDistance = queryPoint - lineSegmentStart;

        float projection = Vector2.Dot(pointDistance, lineDistance);
        
        // move the point distance along the line segment.
        float delta = projection / lineDistance.LengthSquared();

        if(delta <= 0)
        {
            return lineSegmentStart;
        }
        else if(delta >= 1)
        {
            return lineSegmentEnd;
        }
        else
        {
            return lineSegmentStart + lineDistance * delta;
        }
    }
}