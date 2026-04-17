using Howl.Graphics;
using Howl.Math;

namespace Howl.Graphics;

public struct VertexPositionColour
{
    /// <summary>
    /// Gets and sets the position.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// Gets and sets the colour.
    /// </summary>
    public Colour Colour;

    /// <summary>
    /// Constructs a VertexPositionColour.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="colour">The colour.</param>
    public VertexPositionColour(Vector3 position, Colour colour)
    {
        Position = position;
        Colour = colour;
    }
}