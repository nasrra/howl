namespace Howl.Math;

public struct Circle
{
    /// <summary>
    /// Gets and sets the radius.
    /// </summary>
    public float Radius;

    /// <summary>
    /// Gets and sets the x-coordinate position.
    /// </summary>
    public float X;

    /// <summary>
    /// Gets and sets the y-coordinate position.
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
        Radius = radius;
        X = x;
        Y = y;
    }
}