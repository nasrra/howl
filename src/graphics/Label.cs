using Howl.Math;
using Howl.Text;

namespace Howl.Graphics;

public struct Label
{ 
    /// <summary>
    ///     The colour used when drawing.
    /// </summary>
    public Colour Colour;
    
    /// <summary>
    ///     The offset when drawing.
    /// </summary>
    public Vector2 Offset;

    /// <summary>
    ///     The string id to this text's characters in a string allocator instance.
    /// </summary>
    public StringId StringId;

    /// <summary>
    ///     The id of the font in a font manager instance to use when drawing.
    /// </summary>
    public int FontId;
    
    /// <summary>
    ///     The space to draw in.
    /// </summary>
    public DrawSpace DrawSpace;

    /// <summary>
    ///     Constructs a Text.
    /// </summary>
    /// <param name="colour">the colour used when drawing.</param>
    /// <param name="offset">the offset when drawing.</param>
    /// <param name="stringId">the string id to this text's characters in a string allocator instance.</param>
    /// <param name="fontId">the id of the font in a font manager instance to use when drawing.</param>
    /// <param name="drawSpace">the space to draw in.</param>
    public Label(Colour colour, Vector2 offset, StringId stringId, int fontId, DrawSpace drawSpace)
    {
        Colour = colour;
        Offset = offset;
        StringId = stringId;
        FontId = fontId;
        DrawSpace = drawSpace;
    }
}
