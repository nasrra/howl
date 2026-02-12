using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public static class Math
{
    public const float Pi = 3.1415926535897932384626433f;

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

    /// <summary>
    /// Gets the absolute value of a floating point number.
    /// </summary>
    /// <param name="value">The floating point value.</param>
    /// <returns>The absolute value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Abs(float value)
    {
        const uint mask = 0x7FFFFFFF;
        uint raw = System.BitConverter.SingleToUInt32Bits(value);
        return System.BitConverter.UInt32BitsToSingle(raw & mask);
    }

    /// <summary>
    /// Gets the maximum value between two numbers.
    /// </summary>
    /// <param name="a">value a.</param>
    /// <param name="b">value b.</param>
    /// <returns>the maximum value between the two.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Max(float a, float b)
    {
        return b > a? b : a;
    }

    /// <summary>
    /// Gets the minimum value between two numbers.
    /// </summary>
    /// <param name="a">value a.</param>
    /// <param name="b">value b.</param>
    /// <returns>the minimum number between the two.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Min(float a, float b)
    {
        return b < a? b : a;
    }

    /// <summary>
    /// returns if two floating point numbers are nearly equal to eachother.
    /// </summary>
    /// <param name="a">value a.</param>
    /// <param name="b">value b.</param>
    /// <param name="epsilon">The threshold for equality.</param>
    /// <returns>true, if both values are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool NearlyEqual(float a, float b, float epsilon)
    {
        // // Note: norm-based comparison enurses
        // // the epsilon comparison doesnt return false negatives
        // // at large floating point values.        
        float diff = Abs(a-b);
        // float norm = Max(Abs(a),Abs(b));
        // return diff <= epsilon * Max(1f, norm);

        return diff <= epsilon;
    }

    /// <summary>
    /// Clamps a value between a min and max
    /// </summary>
    /// <remarks>
    /// Note: Min and max are both invlusive.
    /// </remarks>
    /// <param name="value">the value to clamp.</param>
    /// <param name="min">the min value.</param>
    /// <param name="max">the max value.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Clamp(float value, float min, float max)
    {
        if(min > max)
        {
            ThrowMinMaxException(min, max);
        }

        if(value <= min)
        {
            return min;
        }
        else if(value >= max)
        {
            return max;
        }

        return value;
    }

    private static void ThrowMinMaxException<T>(T min, T max)
    {
        throw new ArgumentException($"cannot Clamp when min '{min}' is greater than max '{max}'");
    }
}