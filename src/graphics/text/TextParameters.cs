using Howl.Math;
using Howl.ECS;

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
    /// Constructs a FontParamters.
    /// </summary>
    /// <param name="colour">The draw colour.</param>
    /// <param name="offset">The offset - in pixels - when drawing.</param>
    /// <param name="fontGenIndex">The font to use when drawing.</param>
    public TextParameters(Colour colour, Vector2 offset, GenIndex fontGenIndex)
    {
        Colour = colour;
        Offset = offset;
        FontGenIndex = fontGenIndex;
    }
}