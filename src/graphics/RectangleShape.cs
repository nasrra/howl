using Howl.Math;

namespace Howl.Graphics;

public struct RectangleShape
{
    /// <summary>
    /// Gets and sets the Rectangle data.
    /// </summary>
    public Rectangle Rectangle;

    /// <summary>
    /// Gets and sets the colour used when drawing.
    /// </summary>
    public Colour Colour;

    /// <summary>
    /// Gets and sets the origin point of the rectangle.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// Gets the top-left point of this rectangle shape.
    /// </summary>
    public Vector2 TopLeft => Rectangle.TopLeft - Origin;

    /// <summary>
    /// Gets the top-right point of this rectangle shape.
    /// </summary>
    public Vector2 TopRight => Rectangle.TopRight - Origin;

    /// <summary>
    /// Gets the bottom-left point of this rectangle shape.
    /// </summary>
    public Vector2 BottomLeft => Rectangle.BottomLeft - Origin;

    /// <summary>
    /// Gets the bottom-right point of this rectangle shape.
    /// </summary>
    public Vector2 BottomRight => Rectangle.BottomRight - Origin;

    /// <summary>
    /// Constructs a RectangleShape.
    /// </summary>
    /// <param name="rectangle">The rectangle data.</param>
    /// <param name="colour">The colour to draw with.</param>
    /// <param name="origin">The origin point of the rectangle.</param>
    public RectangleShape(Rectangle rectangle, Colour colour, Vector2 origin)
    {
        Rectangle = rectangle;
        Colour = colour;
        Origin = origin;
    }
}