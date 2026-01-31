using System;
using System.Runtime.CompilerServices;

namespace Howl.Math.Shapes;

public struct Circle
{
    private float radius;

    public readonly float Radius => radius;

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
        Y =y;
        this.radius = radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetRadius(float radius)
    {
        this.radius = radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Circle Transform(Circle circle, Transform transform)
    {
        Vector2 origin = Vector2.Transform(circle.X, circle.Y, transform);
        float radius = circle.Radius * MathF.Max(transform.Scale.X, transform.Scale.Y); 
        return new Circle(origin.X, origin.Y, radius);
    }
}