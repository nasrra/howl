using Howl.Unmanaged.Collections;
using N_Howl.N_Math;

namespace N_Howl.N_Font;

public struct Glyph{
    /// <summary>
    ///     The amount of pixels to move the glyph so it is at the expected position along the origin line.
    /// </summary>
    public Vector2 Offset;
    /// <summary>
    ///     The amount of pixels to move before drawing the next glyph.
    /// </summary>
    public Vector2 Advance;
    /// <summary>
    ///     The coordinates of the top left point of this glyph in its generated texture atlas.
    /// </summary>
    public Vector2I TextureCoords;
    /// <summary>
    ///     The size of this glyph's quad in its generated texture atlas.
    /// </summary>
    public Vector2I Size;
}

public struct FontData{
    public uint BaseGlyphIndex;
    public uint MaxGlyphHeightInPixels;
    public Array<Glyph> Glyphs;
}