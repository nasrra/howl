using Howl.Math;

namespace Howl.Graphics;

public struct CircleShape
{
    /// <summary>
    /// Gets and sets the Circle data.
    /// </summary>
    public Circle Circle;

    /// <summary>
    /// Gets and sets the colour used when drawing.
    /// </summary>
    public Colour Colour;

    /// <summary>
    /// Gets and sets the origin of the circle.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// Constructs a CircleShape.
    /// </summary>
    /// <param name="circle">The circle data.</param>
    /// <param name="colour">The colour to draw with.</param>
    /// <param name="origin">The origin point of the circle.</param>
    public CircleShape(Circle circle, Colour colour, Vector2 origin)
    {
        Circle = circle;
        Colour = colour;
        Origin = origin;
    }
}
