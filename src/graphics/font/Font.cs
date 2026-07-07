using Howl;
using N_Howl.N_Text;
using N_Howl.N_Collections;
using N_Howl.N_Math;
using N_Howl.N_Memory;
using FreeType = N_Howl.N_Font.N_FreeTypeSharp.Font;

namespace N_Howl.N_Font;
public unsafe static class Font{

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>Note that the loaded font data will not have a font height that is exactly equal to the requested font height due to glyph size differences.</para>
/// </remarks>
public static bool LoadFont(
    String filePath, ref FontData fontData, ref Array<byte> textureDataOutput, uint textureWidth, 
    uint fontHeightInPixels
){
    Buffer<byte> utf8Path = default;
    Collections.Init(ref utf8Path, stackalloc byte[Text.GetByteCountUTF8(filePath)]);
    Text.GetBytesUTF8(filePath, ref utf8Path);

    nint retrievedFontHeightInPixels = 0;
    bool success = FreeType.LoadFont(
        utf8Path.Pointer, ref fontData.Glyphs, ref textureDataOutput, textureWidth, fontHeightInPixels, fontData.BaseGlyphIndex, ref retrievedFontHeightInPixels
    );
    fontData.MaxGlyphHeightInPixels = (uint)retrievedFontHeightInPixels;
    return success;
}

public static void InitFontData(
    ref FontData fontData, ref MemoryArena arena, int glyphCount, uint baseGlyphIndex
){
    Debug.Assert(glyphCount > 1, "Font data should be intialised with a count greater than one to account for the Nil element.");
    glyphCount = Math.Clamp(glyphCount, 1, int.MaxValue);
    fontData.BaseGlyphIndex = baseGlyphIndex;
    Collections.Init(ref fontData.Glyphs, ref arena, glyphCount);
}

}