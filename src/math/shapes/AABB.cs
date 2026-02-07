using System;
using System.Runtime.CompilerServices;

namespace Howl.Math.Shapes;

public struct AABB
{
    public static AABB Zero => new AABB(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Gets and sets the minimum vector.
    /// </summary>
    public Vector2 Min;

    /// <summary>
    /// Gets and sets the maximum vector.
    /// </summary>
    public Vector2 Max;

    /// <summary>
    /// Calculates and gets the height of this AABB.
    /// </summary>
    public float Height => Max.Y - Min.Y;

    /// <summary>
    /// Calculates and gets the width of this AABB.
    /// </summary>
    public float Width => Max.X - Min.X;

    /// <summary>
    /// Constructs a Axis-Aligned-Bounding-Box.
    /// </summary>
    /// <param name="min">The minimum vector.</param>
    /// <param name="max">The maximum vector.</param>
    public AABB(Vector2 min, Vector2 max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Constructs a Axis-Aligned-Bounding-Box.
    /// </summary>
    /// <param name="minX">The x-value of the minimum vector.</param>
    /// <param name="minY">The y-value of the minimum vector.</param>
    /// <param name="maxX">The x-value of the maximum vector.</param>
    /// <param name="maxY">the y-value of the maximum vector.</param>
    public AABB(float minX, float minY, float maxX, float maxY)
    {
        Min = new (minX, minY);
        Max = new (maxX, maxY);
    }

    /// <summary>
    /// Constructs a Axis-Aligned-Bounding-Box from the union of two AABB's
    /// </summary>
    /// <param name="a">aabb-a</param>
    /// <param name="b">aabb-b</param>
    public AABB(AABB a, AABB b)
    {
        Min = a.Min.MinComponent(b.Min);
        Max = a.Max.MaxComponent(b.Max);
    }

    /// <summary>
    /// Checks whether two Axis-Aligned-Bounding-Boxes are intersecting.
    /// </summary>
    /// <param name="a">The first AABB.</param>
    /// <param name="b">The other AABB.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(in AABB a, in AABB b)
    {
        if(a.Max.X <= b.Min.X || b.Max.X <= a.Min.X)
        {
            return false;
        }
        if (a.Max.Y <= b.Min.Y || b.Max.Y <= a.Min.Y)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks whether an Axis-Aligned-Bounding-Box intersects with a vector.
    /// </summary>
    /// <param name="a">the aabb.</param>
    /// <param name="vector">the vector.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(Vector2 vector, in AABB aabb)
    {
        return Intersect(aabb, vector);
    }

    /// <summary>
    /// Checks whether an Axis-Aligned-Bounding-Box intersects with a vector.
    /// </summary>
    /// <param name="a">the aabb.</param>
    /// <param name="vector">the vector.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(in AABB aabb, Vector2 vector)
    {
        return 
        aabb.Min.X <= vector.X &&
        aabb.Min.Y <= vector.Y && 
        aabb.Max.X >= vector.X &&
        aabb.Max.Y >= vector.Y;
    }

    /// <summary>
    /// Checks whether an Axis-Aligned-Bounding-Box intersects with a line segment.
    /// </summary>
    /// <param name="aabb">The aabb.</param>
    /// <param name="lineSegmentStart">the start of the line-segment.</param>
    /// <param name="lineSegmentEnd">the end of the line-segment.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    public static bool Intersect(in AABB aabb, Vector2 lineSegmentStart, Vector2 lineSegmentEnd)
    {
        if(Intersect(aabb,Math.ClosestPoint(lineSegmentStart, lineSegmentEnd, aabb.Min)))
        {
            if(Intersect(aabb,Math.ClosestPoint(lineSegmentStart, lineSegmentEnd, aabb.Max)))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Adds a vector to a AABB.
    /// </summary>
    /// <param name="aabb">The aabb to add to.</param>
    /// <param name="vector">The vector to add.</param>
    /// <returns>The resultant aabb.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static AABB operator +(AABB aabb, Vector2 vector)
    {
        return new(aabb.Min + vector, aabb.Max + vector);
    }

    /// <summary>
    /// Subtracts a vector from an AABB.
    /// </summary>
    /// <param name="aabb">The aabb to remove from.</param>
    /// <param name="vector">The aabb to remove.</param>
    /// <returns>The resultant aabb.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static AABB operator -(AABB aabb, Vector2 vector)
    {
        return new(aabb.Min - vector, aabb.Max - vector);        
    }

    /// <summary>
    /// Gets the center point of this AABB.
    /// </summary>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vector2 GetCentroid()
    {
        return (Max + Min) * 0.5f; 
    }

    /// <summary>
    /// Gets whether or not two AABB are equal.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>true, if both AABB are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator ==(AABB a, AABB b)
    {
        return a.Min == b.Min && a.Max == b.Max;   
    }

    /// <summary>
    /// Gets whether or not two AABB are not equal.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>true, if both are not equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator !=(AABB a, AABB b)
    {
        return a.Min != b.Min || a.Max != b.Max;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>true, if the object is equal to this; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override bool Equals(object obj)
    {
        return obj is AABB other && this == other;
    }

    /// <summary>
    /// Gets whether or not this AABB is equal to another.
    /// </summary>
    /// <param name="other">the other aabb.</param>
    /// <returns>true, if this is nearly equal to the other; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool NearlyEqual(AABB other)
    {
        return NearlyEqual(this, other);
    }

    /// <summary>
    /// Gets whether or not two AABB are nearly equal.
    /// </summary>
    /// <param name="a">aabb a.</param>
    /// <param name="b">aabb b.</param>
    /// <returns>true, if both are nearly equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool NearlyEqual(AABB a, AABB b)
    {
        return Vector2.NearlyEqual(a.Min, b.Min) && Vector2.NearlyEqual(a.Max, b.Max);
    }

    /// <summary>
    /// Gets the hash code.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Min, Max);
    }

    public override string ToString()
    {
        return $"Min: '{Min}', Max: '{Max}'";
    }
}