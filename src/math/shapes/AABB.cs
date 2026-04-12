using System;
using System.Runtime.CompilerServices;
using static Howl.Math.Math;

namespace Howl.Math.Shapes;

public struct Aabb
{
    public static Aabb Zero => new Aabb(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Gets and sets the x-component of the minimum vertex.
    /// </summary>
    public float MinX;

    /// <summary>
    /// Gets and sets the y-component of the minimum vertex.
    /// </summary>
    public float MinY;

    /// <summary>
    /// Gets and sets the x-component of the maximum vertex.
    /// </summary>
    public float MaxX;

    /// <summary>
    /// Gets and sets the y-component of the maximum vertex. 
    /// </summary>
    public float MaxY;

    /// <summary>
    /// Constructs a Axis-Aligned-Bounding-Box.
    /// </summary>
    /// <param name="min">The minimum vertex.</param>
    /// <param name="max">The maximum vertex.</param>
    public Aabb(Vector2 min, Vector2 max)
    : this(min.X, min.Y, max.X, max.Y)
    {}

    /// <summary>
    /// Constructs a Axis-Aligned-Bounding-Box.
    /// </summary>
    /// <param name="minX">The x-value of the minimum vertex.</param>
    /// <param name="minY">The y-value of the minimum vertex.</param>
    /// <param name="maxX">The x-value of the maximum vertex.</param>
    /// <param name="maxY">the y-value of the maximum vertex.</param>
    public Aabb(float minX, float minY, float maxX, float maxY)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    /// <summary>
    /// Adds a vector to a AABB.
    /// </summary>
    /// <param name="aabb">The aabb to add to.</param>
    /// <param name="vector">The vector to add.</param>
    /// <returns>The resultant aabb.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Aabb operator +(Aabb aabb, Vector2 vector)
    {
        return new Aabb(
            aabb.MinX + vector.X,
            aabb.MinY + vector.Y,
            aabb.MaxX + vector.X,
            aabb.MaxY + vector.Y
        );
    }

    /// <summary>
    /// Subtracts a vector from an AABB.
    /// </summary>
    /// <param name="aabb">The aabb to remove from.</param>
    /// <param name="vector">The aabb to remove.</param>
    /// <returns>The resultant aabb.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Aabb operator -(Aabb aabb, Vector2 vector)
    {
        return new Aabb(
            aabb.MinX - vector.X,
            aabb.MinY - vector.Y,
            aabb.MaxX - vector.X,
            aabb.MaxY - vector.Y
        );      
    }

    /// <summary>
    /// Gets whether or not two AABB are equal.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>true, if both AABB are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator ==(Aabb a, Aabb b)
    {
        return 
        a.MinX == b.MinX 
        && a.MinY == b.MinY
        && a.MaxX == b.MaxX
        && a.MaxY == b.MaxY;   
    }

    /// <summary>
    /// Gets whether or not two AABB are not equal.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>true, if both are not equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator !=(Aabb a, Aabb b)
    {
        return 
        a.MinX != b.MinX
        || a.MinY != b.MinY 
        || a.MaxX != b.MaxX
        || a.MaxY != b.MaxY;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>true, if the object is equal to this; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override bool Equals(object obj)
    {
        return obj is Aabb other && this == other;
    }

    /// <summary>
    /// Gets whether or not this AABB is equal to another.
    /// </summary>
    /// <param name="other">the other aabb.</param>
    /// <param name="epsilon">the threshold for equality.</param>
    /// <returns>true, if this is nearly equal to the other; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool NearlyEqual(Aabb other, float epsilon)
    {
        return NearlyEqual(this, other, epsilon);
    }
    
    /// <summary>
    /// Gets whether or not two AABB are nearly equal.
    /// </summary>
    /// <param name="a">aabb a.</param>
    /// <param name="b">aabb b.</param>
    /// <param name="epsilon">the threshold for equality.</param>
    /// <returns>true, if both are nearly equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool NearlyEqual(Aabb a, Aabb b, float epsilon)
    {
        return 
        Math.NearlyEqual(a.MinX, b.MinX, epsilon)
        && Math.NearlyEqual(a.MinY, b.MinY, epsilon)
        && Math.NearlyEqual(a.MaxX, b.MaxX, epsilon)
        && Math.NearlyEqual(a.MaxY, b.MaxY, epsilon);

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

    /// <summary>
    /// Gets the height of an AABB.
    /// </summary>
    /// <param name="aabb">the aabb.</param>
    /// <returns>the height of the aabb.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Height(in Aabb aabb)
    {
        return aabb.MaxY - aabb.MinY;
    }

    /// <summary>
    /// Gets the width of an AABB.
    /// </summary>
    /// <param name="aabb">the aabb.</param>
    /// <returns>the width of the aabb.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Width(in Aabb aabb)
    {
        return aabb.MaxX - aabb.MinX;
    }

    /// <summary>
    /// Calculates the center point of a AABB.
    /// </summary>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 CalculateCentroid(in Aabb aabb)
    {
        CalculateCentroid(aabb.MinX, aabb.MinY, aabb.MaxX, aabb.MaxY, out float centerX, out float centerY);
        return new Vector2(centerX, centerY); 
    }

    /// <summary>
    /// Calcuates the center point of an AABB.
    /// </summary>
    /// <param name="minX">the x-component of the minimum vertex.</param>
    /// <param name="minY">the y-component of the minimum vertex.</param>
    /// <param name="maxX">the x-component of the maxiumum vertex.</param>
    /// <param name="maxY">the y-component of the maxiumum vertex.</param>
    /// <param name="centerX">the x-component of the center point.</param>
    /// <param name="centerY">the y-component of the center point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveInlining)]
    public static void CalculateCentroid(float minX, float minY, float maxX, float maxY, out float centerX, out float centerY)
    {
        centerX = (maxX + minX) * 0.5f;
        centerY = (maxY + minY) * 0.5f;
    }

    /// <summary>
    /// Constructs the minimum vector of an AABB.
    /// </summary>
    /// <param name="aabb">the aabb.</param>
    /// <returns>the minimum vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 MinVector(in Aabb aabb)
    {
        return new Vector2(aabb.MinX, aabb.MinY);
    }

    /// <summary>
    /// Constructs the maximum vector of an AABB.
    /// </summary>
    /// <param name="aabb">the aabb.</param>
    /// <returns>the maximum vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 MaxVector(in Aabb aabb)
    {
        return new Vector2(aabb.MaxX, aabb.MaxY);
    }




    /*******************
    
        Union.
    
    ********************/




    /// <summary>
    /// Constructs a Axis-Aligned-Bounding-Box from the union of two AABB's
    /// </summary>
    /// <param name="a">aabb-a</param>
    /// <param name="b">aabb-b</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Aabb Union(Aabb a, Aabb b)
    {
        Union(
            a.MinX, 
            a.MinY,
            a.MaxX, 
            a.MaxY,
            b.MinX, 
            b.MinY,
            b.MaxX, 
            b.MaxY,
            out float unionMinX,
            out float unionMinY,
            out float unionMaxX,
            out float unionMaxY
        );

        return new Aabb(
            unionMinX,
            unionMinY,
            unionMaxX,
            unionMaxY
        );
    }

    /// <summary>
    /// Gets the min and max components for the union of an AABB.
    /// </summary>
    /// <param name="aMinX">the x-component of the minimum vertex from aabbb a.</param>
    /// <param name="aMinY">the y-component of the minimum vertex from aabbb a.</param>
    /// <param name="aMaxX">the x-component of the maximum vertex from aabbb a.</param>
    /// <param name="aMaxY">the y-component of the maximum vertex from aabbb a.</param>
    /// <param name="bMinX">the x-component of the minimum vertex from aabbb b.</param>
    /// <param name="bMinY">the y-component of the minimum vertex from aabbb b.</param>
    /// <param name="bMaxX">the x-component of the maximum vertex from aabbb b.</param>
    /// <param name="bMaxY">the y-component of the maximum vertex from aabbb b.</param>
    /// <param name="unionMinX">the x-component of the minimum vertex for the unioned aabbb.</param>
    /// <param name="unionMinY">the y-component of the minimum vertex for the unioned aabbb.</param>
    /// <param name="unionMaxX">the x-component of the maximum vertex for the unioned aabbb.</param>
    /// <param name="unionMaxY">the y-component of the maximum vertex for the unioned aabbb.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Union(
        float aMinX, 
        float aMinY,
        float aMaxX, 
        float aMaxY,
        float bMinX, 
        float bMinY,
        float bMaxX, 
        float bMaxY,
        out float unionMinX,
        out float unionMinY,
        out float unionMaxX,
        out float unionMaxY
    )
    {
        unionMinX = Math.Min(aMinX, bMinX);
        unionMinY = Math.Min(aMinY, bMinY);
        unionMaxX = Math.Max(aMaxX, bMaxX);
        unionMaxY = Math.Max(aMaxY, bMaxY);
    }




    /*******************
    
        Intersect.
    
    ********************/




    /// <summary>
    /// Checks whether two Axis-Aligned-Bounding-Boxes are intersecting.
    /// </summary>
    /// <param name="a">The first AABB.</param>
    /// <param name="b">The other AABB.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(in Aabb a, in Aabb b)
    {
        return Intersect(
            a.MinX,
            a.MinY,
            a.MaxX,
            a.MaxY,
            b.MinX,
            b.MinY,
            b.MaxX,
            b.MaxY
        );
    }


    /// <summary>
    /// Checks whether two Axis-Aligned-Bounding-Boxes are intersecting.
    /// </summary>
    /// <param name="aMinX">the x-component of the minimum vertex from aabbb a.</param>
    /// <param name="aMinY">the y-component of the minimum vertex from aabbb a.</param>
    /// <param name="aMaxX">the x-component of the maximum vertex from aabbb a.</param>
    /// <param name="aMaxY">the y-component of the maximum vertex from aabbb a.</param>
    /// <param name="bMinX">the x-component of the minimum vertex from aabbb b.</param>
    /// <param name="bMinY">the y-component of the minimum vertex from aabbb b.</param>
    /// <param name="bMaxX">the x-component of the maximum vertex from aabbb b.</param>
    /// <param name="bMaxY">the y-component of the maximum vertex from aabbb b.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(float aMinX,float aMinY, float aMaxX, float aMaxY, float bMinX, float bMinY, float bMaxX, float bMaxY)
    {        
        if(aMaxX <= bMinX || bMaxX <= aMinX)
        {
            return false;
        }
        if (aMaxY <= bMinY || bMaxY <= aMinY)
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
    public static bool Intersect(Vector2 vector, in Aabb aabb)
    {
        return Intersect(
            aabb.MinX,
            aabb.MinY,
            aabb.MaxX,
            aabb.MaxY,
            vector.X,
            vector.Y
        );
    }

    /// <summary>
    /// Checks whether an Axis-Aligned-Bounding-Box intersects with a vector.
    /// </summary>
    /// <param name="a">the aabb.</param>
    /// <param name="vector">the vector.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(in Aabb aabb, Vector2 vector)
    {
        return Intersect(
            aabb.MinX,
            aabb.MinY,
            aabb.MaxX,
            aabb.MaxY,
            vector.X,
            vector.Y
        );
    }

    /// <summary>
    /// Checks whether an Axis-Aligned-Bounding-Box intersects with a vector.
    /// </summary>
    /// <param name="aabbMinX">the x-component of the minimum vertex in the aabb.</param>
    /// <param name="aabbMinY">the y-component of the minimum vertex in the aabb.</param>
    /// <param name="aabbMaxX">the x-component of the maximum vertex in the aabb.</param>
    /// <param name="aabbMaxY">the y-component of the maximum vertex in the aabb.</param>
    /// <param name="vectorX">the x-component of the vertex.</param>
    /// <param name="vectorY">the y-component of the vertex.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(
        float aabbMinX,
        float aabbMinY,
        float aabbMaxX,
        float aabbMaxY,
        float vectorX,
        float vectorY
    )
    {
        return 
        aabbMinX <= vectorX &&
        aabbMinY <= vectorY && 
        aabbMaxX >= vectorX &&
        aabbMaxY >= vectorY;        
    }

    /// <summary>
    /// Checks whether an Axis-Aligned-Bounding-Box intersects with a line segment.
    /// </summary>
    /// <param name="aabb">The aabb.</param>
    /// <param name="lineStart">the start of the line-segment.</param>
    /// <param name="lineEnd">the end of the line-segment.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    public static bool LineIntersect(in Aabb aabb, Vector2 lineStart, Vector2 lineEnd)
    {
        
        return LineIntersect(
            aabb.MinX,
            aabb.MinY,
            aabb.MaxX,
            aabb.MaxY,
            lineStart.X,
            lineStart.Y,
            lineEnd.X,
            lineEnd.Y
        );
    }

    /// <summary>
    /// Checks whether an Axis-Aligned-Bounding-Box intersects with a line segment.
    /// </summary>
    /// <param name="aabbMinX">the x-component of the aabb minimum vertex.</param>
    /// <param name="aabbMinY">the y-component of the aabb minimum vertex.</param>
    /// <param name="aabbMaxX">the x-component of the aabb maximum vertex.</param>
    /// <param name="aabbMaxY">the y-component of the aabb minimum vertex.</param>
    /// <param name="lineStartX">the x-component of the line statrting point.</param>
    /// <param name="lineStartY">the x-component of the line statrting point.</param>
    /// <param name="lineEndX">the x-component of the line end point.</param>
    /// <param name="lineEndY">the x-component of the line end point.</param>
    /// <returns></returns>
    public static bool LineIntersect(
        float aabbMinX,
        float aabbMinY,
        float aabbMaxX,
        float aabbMaxY,
        float lineStartX,
        float lineStartY,
        float lineEndX,
        float lineEndY
    )
    {       
        float closestPointX;
        float closestPointY;

        ClosestPoint(lineStartX, lineStartY, lineEndX, lineEndY, aabbMinX, aabbMinY, out closestPointX, out closestPointY);
        if(Intersect(aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, closestPointX, closestPointY))
        {
            ClosestPoint(lineStartX, lineStartY, lineEndX, lineEndY, aabbMaxX, aabbMaxY, out closestPointX, out closestPointY);
            if(Intersect(aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, closestPointX, closestPointY))
            {
                return true;
            }
        }
        return false;
    }
}