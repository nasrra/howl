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
    /// <param name="lineStart">the beginning of the line-segment.</param>
    /// <param name="lineEnd">the end of the line-segment.</param>
    /// <param name="queryPoint">the point to find the closest point towards.</param>
    /// <returns>The closest point along the line segment towards the query point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 ClosestPoint(Vector2 lineStart, Vector2 lineEnd, Vector2 queryPoint)
    {
        ClosestPoint(
            lineStart.X,
            lineStart.Y,
            lineEnd.X,
            lineEnd.Y,
            queryPoint.X,
            queryPoint.Y,
            out float closestPointX,
            out float closestPointY
        );
        return new Vector2(closestPointX, closestPointY);
    }

    /// <summary>
    /// Gets the closest point along a line segment towards a given point.
    /// </summary>
    /// <param name="lineStartX">the x-component of the line segment starting point.</param>
    /// <param name="lineStartY">the y-component of the line segment starting point.</param>
    /// <param name="lineEndX">the x-component of the line segment end point.</param>
    /// <param name="lineEndY">the y-component of the line segment end point.</param>
    /// <param name="queryPointX">the x-component of the query point.</param>
    /// <param name="queryPointY">the y-component of the query point.</param>
    /// <param name="closestPointX">the x-compoennt of the closest point along the line segment towards the query point.</param>
    /// <param name="closestPointY">the y-compoennt of the closest point along the line segment towards the query point.</param>
    /// <param name="distanceSquared">the distance sqaured from the query point to the closest point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClosestPoint(
        float lineStartX, 
        float lineStartY,
        float lineEndX, 
        float lineEndY,
        float queryPointX,
        float queryPointY, 
        out float closestPointX,
        out float closestPointY, 
        out float distanceSquared
    )
    {
        ClosestPoint(
            lineStartX, 
            lineStartY, 
            lineEndX, 
            lineEndY, 
            queryPointX,
            queryPointY,
            out closestPointX,
            out closestPointY
        );
        distanceSquared = DistanceSquared(queryPointX, queryPointY, closestPointX, closestPointY);
    }

    /// <summary>
    /// Gets the closest point along a line segment towards a given point.
    /// </summary>
    /// <param name="lineStartX">the x-component of the line segment starting point.</param>
    /// <param name="lineStartY">the y-component of the line segment starting point.</param>
    /// <param name="lineEndX">the x-component of the line segment end point.</param>
    /// <param name="lineEndY">the y-component of the line segment end point.</param>
    /// <param name="queryPointX">the x-component of the query point.</param>
    /// <param name="queryPointY">the y-component of the query point.</param>
    /// <param name="closestPointX">the x-compoennt of the closest point along the line segment towards the query point.</param>
    /// <param name="closestPointY">the y-compoennt of the closest point along the line segment towards the query point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClosestPoint(
        float lineStartX, 
        float lineStartY,
        float lineEndX, 
        float lineEndY,
        float queryPointX,
        float queryPointY,
        out float closestPointX,
        out float closestPointY
    )
    {
        float lineDistanceX = lineEndX - lineStartX;
        float lineDistanceY = lineEndY - lineStartY;
        float pointDistanceX = queryPointX - lineStartX;
        float pointDistanceY = queryPointY - lineStartY;

        // float projection = Vector2.Dot(pointDistance, lineDistance);
        float projection = Dot(pointDistanceX, pointDistanceY, lineDistanceX, lineDistanceY);

        // move the point distance along the line segment.
        float delta = projection / LengthSquared(lineDistanceX, lineDistanceY);

        if(delta <= 0)
        {
            closestPointX = lineStartX;
            closestPointY = lineStartY;
        }
        else if(delta >= 1)
        {
            closestPointX = lineEndX;
            closestPointY = lineEndY;
        }
        else
        {
            closestPointX = lineStartX + lineDistanceX * delta;
            closestPointY = lineStartY + lineDistanceY * delta;
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
    public static int Clamp(int value, int min, int max)
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

    /// <summary>
    /// Gets the distance squared between two points.
    /// </summary>
    /// <param name="fromX">the x-component of the point to start at.</param>
    /// <param name="fromY">the y-component of the point to start at.</param>
    /// <param name="toX">the x-component of the point to end at</param>
    /// <param name="toY">the y-component of the point to end at</param>
    /// <returns>The distance squared.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float DistanceSquared(float fromX, float fromY, float toX, float toY)
    {
        float dx = fromX - toX;
        float dy = fromY - toY;
        return dx * dx + dy * dy;
    }

    /// <summary>
    /// Noramlises two numbers with eachother.
    /// </summary>
    /// <param name="x">value 1.</param>
    /// <param name="y">value 2.</param>
    /// <param name="nX">normalised value 1.</param>
    /// <param name="nY">normalised value 2.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Normalise(float x, float y, out float nX, out float nY)
    {
        float invLength = 1.0f / MathF.Sqrt(x * x + y * y);
        nX = x * invLength;
        nY = y * invLength;
    }

    /// <summary>
    /// Gets the dot product of two points.
    /// </summary>
    /// <param name="lhsX">The x-component of the left-hand side point.</param>
    /// <param name="lhsY">The y-component of the left-hand side point.</param>
    /// <param name="rhsX">The x-component of the left-hand side point.</param>
    /// <param name="rhsY">The y-component of the left-hand side point.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Dot(float lhsX, float lhsY, float rhsX, float rhsY)
    {
        return (lhsX * rhsX) + (lhsY * rhsY);
    }

    /// <summary>
    /// Gets the squared length of a point.
    /// </summary>
    /// <param name="x">the x-component of the point.</param>
    /// <param name="y">the y-compoennt of the point.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]    
    public static float LengthSquared(float x, float y)
    {
        return Dot(x,y,x,y);
    }

    /// <summary>
    /// Gets the cross product between two points.
    /// </summary>
    /// <param name="lhsX">the x-component of the left-hand side point.</param>
    /// <param name="lhsY">the y-component of the left-hand side point.</param>
    /// <param name="rhsX">the x-component of the right-hand side point.</param>
    /// <param name="rhsY">the y-component of the right-hand side point.</param>
    /// <returns>the cross product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Cross(float lhsX, float lhsY, float rhsX, float rhsY)
    {
        return lhsX * rhsY - lhsY * rhsX;    
    }   

    /// <summary>
    /// Transforms a vector.
    /// </summary>
    /// <param name="valueX">the x-component of the vector.</param>
    /// <param name="valueY">the y-component of the vector.</param>
    /// <param name="scaleX">the x-component of the scaling vector to transform by.</param>
    /// <param name="scaleY">the y-component of the scaling vector to transform by.</param>
    /// <param name="cos">the cos of the rotation.</param>
    /// <param name="sin">the sin of the rotation.</param>
    /// <param name="positionX">the x-component of the position vector to transform by.</param>
    /// <param name="positionY">the y-component of the position vector to transform by.</param>
    /// <param name="tX">the x-component of the transformed vector.</param>
    /// <param name="tY">the y-component of the transformed vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void TransformVector(
        float valueX, 
        float valueY, 
        float scaleX, 
        float scaleY, 
        float cos,
        float sin,
        float positionX,
        float positionY,
        out float tX, 
        out float tY
    )
    {
        // NOTE:
        // This ordering: Scale -> Rotation -> Translation
        // should remain the same. It is pretty much Matrix math.

        // Scale:
        float sx = valueX * scaleX;
        float sy = valueY * scaleY; 

        // Rotation:
        float rx = sx * cos - sy * sin;
        float ry = sx * sin + sy * cos;

        // Translation:
        tX = rx + positionX;
        tY = ry + positionY;
    }

}