using Howl.Math;
using Howl.Ecs;

namespace Howl.Graphics.Text;

public struct TextParameters
{
    /// <summary>
    /// Gets and sets the draw colour of this text component.
    /// </summary>
    public Colour Colour;

    /// <summary>
    /// Gets and sets the offset - in pixels - when drawing.
    /// </summary>
    public Vector2 Offset;

    /// <summary>
    /// Gets and sets the used font.
    /// </summary>
    public GenIndex FontGenIndex; 

    /// <summary>
    /// Gets and sets the world space to draw in.
    /// </summary>
    public WorldSpace WorldSpace;

    /// <summary>
    /// Constructs a FontParamters.
    /// </summary>
    /// <param name="colour">The draw colour.</param>
    /// <param name="offset">The offset - in pixels - when drawing.</param>
    /// <param name="fontGenIndex">The font to use when drawing.</param>
    /// <param name="worldSpace">The world space to draw in.</param>
    public TextParameters(Colour colour, Vector2 offset, GenIndex fontGenIndex, WorldSpace worldSpace)
    {
        Colour = colour;
        Offset = offset;
        FontGenIndex = fontGenIndex;
        WorldSpace = worldSpace;
    }
}