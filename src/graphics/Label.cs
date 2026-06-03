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

    public static void Initialise(ref Label label, Colour colour, Vector2 offset, int fontId, DrawSpace drawSpace)
    {
        label.Colour = colour;
        label.Offset = offset;
        label.FontId = fontId;
        label.DrawSpace = drawSpace;
    }
}
