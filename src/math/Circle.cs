namespace Howl.Math;

public struct Circle
{
    private float radius;

    public readonly float Radius => radius;

    private float radiusSquared;

    public readonly float RadiusSquared => radiusSquared;

    public float X;

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
        Y =y;
        this.radius = radius;
        this.radiusSquared = Radius * Radius;
    }

    public void SetRadius(float radius)
    {
        this.radius = radius;
        this.radiusSquared = Radius * Radius;
    }
}