using FreeTypeSharp;
using N_Howl.N_Math;

using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;
using N_Howl.N_Collections;
using Howl;

namespace N_Howl.N_Font.N_FreeTypeSharp;
public unsafe static class Font{


/// <param name = "glyphsOutput">
///     Output for the loaded glyph data; note that the length of the array is the amount 
///     of glyphs that will attempted to be loaded.
/// </param>
/// <param name = "baseGlyphIdx">
///     The the index to start loading glyphs from in the font file bit map; 
///     (look at an ASCII table; 32 is 'SPACE').
/// </param>
/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>Free Type file loading automatically handles utf8 paths on windows, so all is good; there is no need for a conversion.</para>
/// </remarks>
public static bool LoadFont(
    byte* utf8FilePath, ref Array<Glyph> glyphsOutput, ref Array<byte> textureData, uint textureWidth, 
    uint lineHeightInPixels, uint baseGlyphIdx, ref nint fontHeightOutput
){
    // init the font libraray.
    FT_LibraryRec_* lib = default;
    if(IsOk(FT_Init_FreeType(&lib)) != true){
        return false;
    }

    // init a font face.
    FT_FaceRec_* face = default;
    FT_New_Face(lib, utf8FilePath, 0, &face);
    FT_Set_Pixel_Sizes(face, 0, lineHeightInPixels);

    // how many pixels are between eachother (vertically and horizontally).
    uint padding = 2;

    // create a buffer on the stack
    // copy glyphs into atlas
    uint row = 0;
    uint col = padding;
    
    // this will need to be used else where.
    fontHeightOutput = 0;
    // loop through ASCII characters 32 to 127.
    nuint glyphIdx = 0;
    for(int i = 0; i < glyphsOutput.Length; ++i){
        
        uint glpyhIndex = FT_Get_Char_Index(face, glyphIdx);
        
        if(IsOk(FT_Load_Glyph(face, glpyhIndex, FT_LOAD_DEFAULT))!=true){
            return false;
        }
        if(IsOk(FT_Render_Glyph(face->glyph, FT_RENDER_MODE_NORMAL))!=true){
            return false;
        }

        // check if the glyph fits into the current row.
        if(col + face->glyph->bitmap.width + padding >= textureWidth){
            // if the glyp doesnt fit; increase the rows and reset the column.
            col = padding;
            row += lineHeightInPixels;
        }
        
        /**
            Get the heighest glyph in the font file.

            NOTE:
                In order to get the correct values that correspond to actual pixels values you have to bit 
                shift down by 6; as freetype represents sizes in 1/64th of a pixel.
        **/
        fontHeightOutput = Math.Max((face->size->metrics.ascender - face->size->metrics.descender) >> 6, fontHeightOutput);

        /**
            go through the rows and columns of the bitmap (the x and y coordinates) and 
            then copy the glyph pixels one by one into the texture atlas.
        **/
        for(uint y = 0; y < face->glyph->bitmap.rows; ++y){
            for(uint x = 0; x < face->glyph->bitmap.width; ++x){
                textureData[(int)((row+y) * textureWidth + col + x)] = face->glyph->bitmap.buffer[y * face->glyph->bitmap.width + x];
            }
        }

        /**
            retrieve the loaded glyph data.
        **/
        ref Glyph glyph = ref glyph = ref glyphsOutput[i];
        if(i == 0){
            // we've loaded the null terminator '🞎' fallback character; now, we should load the actual fonts we want.
            glyphIdx = baseGlyphIdx;
        }
        else{
            glyphIdx++;
        }

        glyph.Size = new(){X = (int)face->glyph->bitmap.width, Y = (int)face->glyph->bitmap.rows};
        /**
            NOTE:
                In order to get the correct values that correspond to actual pixels values you have to bit 
                shift down by 6; as freetype represents sizes in 1/64th of a pixel.
        **/
        glyph.Advance = new(){X = face->glyph->advance.x >> 6, Y = face->glyph->advance.y >> 6};
        glyph.Offset = new(){X = face->glyph->bitmap_left, Y = face->glyph->bitmap_top};
        glyph.TextureCoords = new(){X = (int)col, Y = (int)row};

        // glyph.TextureCoords = {(int)col, (int)row};

        //  move to the next column to write the next glyph to.
        col += face->glyph->bitmap.width + padding;
    }
    
    // cleanup.
    if(IsOk(FT_Done_Face(face))!=true){
        return false;
    }
    if(IsOk(FT_Done_FreeType(lib))!=true){
        return false;
    }
    return true;
}


// todo: this should crash on every case except for image not found and ok.
public static bool IsOk(FT_Error err){
    Debug.Assert(err == FT_Error.FT_Err_Ok,
        $"Free type sharp failed with error code: {err}"
    );
    return err == FT_Error.FT_Err_Ok;
}

}