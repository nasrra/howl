using System;
using System.Runtime.CompilerServices;
using static Howl.Math.Math;

namespace Howl.Math.Shapes;

public struct Circle
{
    /// <summary>
    /// Gets and sets the radius.
    /// </summary>
    public float Radius;

    /// <summary>
    /// The x-positional center value.
    /// </summary>
    public float X;

    /// <summary>
    /// The y-positional center value.
    /// </summary>
    public float Y;

    /// <summary>
    /// Constructs a Circle.
    /// </summary>
    /// <param name="x">The x-coordinate position.</param>
    /// <param name="y">The y-coordinate position.</param>
    /// <param name="radius">The radius</param>
    public Circle(float x, float y, float radius)
    {
        X = x;
        Y = y;
        Radius = radius;
    }
    
    /// <summary>
    /// Constructs a Circle by transforming an existing circle.
    /// </summary>
    /// <remarks>
    /// Note: scale is calculated by the largest component in the transform's scaling vector.
    /// </remarks>
    /// <param name="circle">The circle to transform.</param>
    /// <param name="transform">The transform data.</param>
    /// <returns>The resultant circle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Circle Transform(Circle circle, Transform transform)
    {
        Transform(
            circle.X,
            circle.Y,
            circle.Radius,
            transform.Scale.X,
            transform.Scale.Y,
            transform.Cos,
            transform.Sin,
            transform.Position.X,
            transform.Position.Y,
            out float x,
            out float y,
            out float radius
        );
        return new Circle(x, y, radius);
    }

    /// <summary>
    /// Transforms a circle.
    /// </summary>
    /// <param name="x">the x-component of the circle's origin.</param>
    /// <param name="y">the y-component of the circle's origin.</param>
    /// <param name="radius">the radius of the circle.</param>
    /// <param name="scaleX">the x-component of the scaling vector to transform by.</param>
    /// <param name="scaleY">the y-component of the scaling vector to transform by.</param>
    /// <param name="cos">the cos of the rotation.</param>
    /// <param name="sin">the sin of the rotation.</param>
    /// <param name="posX">the x-component of the position vector to transform by.</param>
    /// <param name="posY">the y-component of the position vector to transform by.</param>
    /// <param name="tX">the x-component of the transformed origin.</param>
    /// <param name="tY">the y-component of the transformed origin.</param>
    /// <param name="tRadius">the scaled radius.</param>
    public static void Transform(
        float x, 
        float y, 
        float radius,
        float scaleX, 
        float scaleY, 
        float cos,
        float sin,
        float posX,
        float posY,
        out float tX, 
        out float tY,
        out float tRadius 
    )
    {
        Math.TransformVector(x,y,scaleX, scaleY, cos, sin, posX, posY, out tX, out tY);
        tRadius = radius * Math.Max(scaleX, scaleY);
    }


    /// <summary>
    /// Constructs a circle based on a circle, with its radius scaled by a vector.
    /// </summary>
    /// <remarks>
    /// The radius is scaled by the largest component in the scaling vector.
    /// </remarks>
    /// <param name="circle">the circle to scale.</param>
    /// <param name="scale">the scaling vector.</param>
    /// <returns>the resultant circle.</returns>    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Circle Scale(in Circle circle, Vector2 scale)
    {
        return new Circle(circle.X, circle.Y, circle.Radius * Math.Max(scale.X, scale.Y));
    }

    /// <summary>
    /// Scales a circle's radius by a scaling vector.
    /// </summary>
    /// <remarks>
    /// The radius is scaled by the largest component in the scaling vector.
    /// </remarks>
    /// <param name="radius">the radius of the circle.</param>
    /// <param name="scale">the scaling vector.</param>
    /// <returns>the scaled radius.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float ScaleRadius(float radius, Vector2 scale)
    {
        return ScaleRadius(radius, scale.X, scale.Y);
    }

    /// <summary>
    /// Scales a circle's radius by a scaling vector.
    /// </summary>
    /// <remarks>
    /// The radius is scaled by the largest component in the scaling vector.
    /// </remarks>
    /// <param name="radius">the radius of the circle.</param>
    /// <param name="scaleX">the x-component of the scaling vector.</param>
    /// <param name="scaleY">the y-component of the scaling vector.</param>
    /// <returns>the scaled radius value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float ScaleRadius(float radius, float scaleX, float scaleY)
    {
        return radius *= Max(scaleX, scaleY);
    }

    /// <summary>
    /// Constructs a circle based on a circle, with its radius scaled by a factor.
    /// </summary>
    /// <param name="circle">the circle to scale.</param>
    /// <param name="scale">the scaling factor.</param>
    /// <returns>the resultant circle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Circle Scale(in Circle circle, float scale)
    {
        return new Circle(circle.X, circle.Y, circle.Radius * scale);        
    }

    /// <summary>
    /// Gets the Axis-Aligned-Bounding-Box of a circle.
    /// </summary>
    /// <returns>The calculated AABB.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Aabb GetAABB(in Circle circle)
    {
        GetMinMaxVectors(circle.X, circle.Y, circle.Radius, out float minX, out float minY, out float maxX, out float maxY);
        return new(minX, minY, maxX, maxY);
    }

    /// <summary>
    /// Gets the min and max vectors of a circle.
    /// </summary>
    /// <param name="x">the positional x-component of the circle.</param>
    /// <param name="y">the positional y-component of the circle.</param>
    /// <param name="radius">the radius of the circle.</param>
    /// <param name="minX">the x-component of the minimum vector.</param>
    /// <param name="minY">the y-component of the minimum vector.</param>
    /// <param name="maxX">the x-component of the maximum vector.</param>
    /// <param name="maxY">the y-component of the maximum vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetMinMaxVectors(float x, float y, float radius, out float minX, out float minY, out float maxX, out float maxY)
    {
        minX = x - radius;
        minY = y - radius;
        maxX = x + radius;
        maxY = y + radius;
    }

    /// <summary>
    /// Gets the center position.
    /// </summary>
    public static Vector2 Center(in Circle circle)
    {
        return new Vector2(circle.X,circle.Y);    
    }

    /// <summary>
    /// Checks whether or not two circles are nearly equal with eachother.
    /// </summary>
    /// <param name="a">circle a.</param>
    /// <param name="b">circle b.</param>
    /// <param name="epsilon">the threshold for equality.</param>
    /// <returns></returns>
    public static bool NearlyEqual(in Circle a, in Circle b, float epsilon)
    {
        return 
            Math.NearlyEqual(a.X, b.X, epsilon) &&
            Math.NearlyEqual(a.Y, b.Y, epsilon) &&
            Math.NearlyEqual(a.Radius, b.Radius, epsilon);
    }
    
    /// <summary>
    /// Gets the area of a circle.
    /// </summary>
    /// <param name="radius">the radius of the circle.</param>
    /// <returns>the area of the circle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float GetArea(float radius)
    {
        return radius * radius * Pi;
    }

    /// <summary>
    /// Gets the area of a circle.
    /// </summary>
    /// <param name="circle">the circle.</param>
    /// <returns>the area of the circle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float GetArea(ref Circle circle)
    {
        return GetArea(circle.Radius);
    }

    /// <summary>
    /// Vectorised calculation of circles radii.
    /// </summary>
    /// <param name="radius">a vector of circle radii.</param>
    /// <returns>the area values of the circles.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static System.Numerics.Vector<float> GetArea(System.Numerics.Vector<float> radius)
    {
        return radius * radius * MathV.Pi;
    }

}