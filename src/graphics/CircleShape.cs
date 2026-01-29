using Howl.Math;

namespace Howl.Graphics;

public struct CircleShape
{
    /// <summary>
    /// Gets and sets the Shape data.
    /// </summary>
    public Circle Shape;

    /// <summary>
    /// Gets and sets the colour used when drawing.
    /// </summary>
    public Colour Colour;

    /// <summary>
    /// Gets and sets the draw mode.
    /// </summary>
    public DrawMode DrawMode;

    /// <summary>
    /// Constructs a CircleShape.
    /// </summary>
    /// <param name="shape">The shape data.</param>
    /// <param name="colour">The colour to draw with.</param>
    /// <param name="drawMode">The draw mode.</param>
    public CircleShape(Circle shape, Colour colour, DrawMode drawMode)
    {
        Shape = shape;
        Colour = colour;
        DrawMode = drawMode;
    }
}
