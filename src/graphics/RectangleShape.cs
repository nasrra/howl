using Howl.Math;
using Howl.Math.Shapes;


namespace Howl.Graphics;

public struct RectangleShape
{
    /// <summary>
    /// Gets and sets the shape data.
    /// </summary>
    public Rectangle Shape;

    /// <summary>
    /// Gets and sets the colour used when drawing.
    /// </summary>
    public Colour Colour;

    /// <summary>
    /// Gets and sets the draw mode.
    /// </summary>
    public DrawMode DrawMode;

    /// <summary>
    /// Constructs a RectangleShape.
    /// </summary>
    /// <param name="shape">The shape data.</param>
    /// <param name="colour">The colour to draw with.</param>
    /// <param name="drawMode">The draw mode.</param>
    public RectangleShape(Rectangle shape, Colour colour, DrawMode drawMode)
    {
        Shape = shape;
        Colour = colour;
        DrawMode = drawMode;
    }
}