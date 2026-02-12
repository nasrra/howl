using System;
using System.Runtime.CompilerServices;

namespace Howl.Math.Shapes;

public struct Circle
{
    /// <summary>
    /// Gets and sets the radius.
    /// </summary>
    public float Radius;

    /// <summary>
    /// The x-positional origin value.
    /// </summary>
    public float X;

    /// <summary>
    /// The y-positional origin value.
    /// </summary>
    public float Y;

    /// <summary>
    /// Gets the X and Y origin values as a vector.
    /// </summary>
    public Vector2 Origin => new Vector2(X,Y);

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
        Vector2 origin = Vector2.Transform(circle.X, circle.Y, transform);
        float radius = circle.Radius * MathF.Max(transform.Scale.X, transform.Scale.Y); 
        return new Circle(origin.X, origin.Y, radius);
    }

    /// <summary>
    /// Constructs a circle based on this circle, with the radius scaled by a vector.
    /// </summary>
    /// <remarks>
    /// Note: radius is scaled by the largest component in the scaling vector.
    /// </remarks>
    /// <param name="scale">the scaling vector.</param>
    /// <returns>the resultant circle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Circle Scale(Vector2 scale)
    {
        return Scale(in this, scale);
    }

    /// <summary>
    /// Constructs a circle based on a circle, with its radius scaled by a vector.
    /// </summary>
    /// <remarks>
    /// Note: radius is scaled by the largest component in the scaling vector.
    /// </remarks>
    /// <param name="circle">the circle to scale.</param>
    /// <param name="scale">the scaling vector.</param>
    /// <returns>the resultant circle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Circle Scale(in Circle circle, Vector2 scale)
    {
        return new Circle(circle.X, circle.Y, circle.Radius * MathF.Max(scale.X, scale.Y));
    }

    /// <summary>
    /// Constructs a circle based on this circle, with the radius scaled by a factor.
    /// </summary>
    /// <param name="scale">the scaling factor.</param>
    /// <returns>the resultant circle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Circle Scale(float scale)
    {
        return Scale(in this, scale);
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
    /// Gets the Axis-Aligned-Bounding-Box of this circle.
    /// </summary>
    /// <returns>The calculated AABB.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public AABB GetAABB()
    {
        return new(
            X - Radius,
            Y - Radius,
            X + Radius,
            Y + Radius
        );
    }
}