using System;
using FontStashSharp;

namespace Howl.Vendors.MonoGame.FontStashSharp;

public class Font
{
    /// <summary>
    ///     The font system for registering, loading, and unloading font files.
    /// </summary>
    public FontSystem FontSystem;

    /// <summary>
    ///     The generated sprite font used for interacting with the monogame api.
    /// </summary>
    public SpriteFontBase SpriteFontBase;

    /// <summary>
    ///     Whether this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new empty font instance.
    /// </summary>
    public Font()
    {
        FontSystem = new();
        SpriteFontBase = null;
    }

    /// <summary>
    ///     Disposes of a font instance.
    /// </summary>
    /// <param name="font">the font to dispose of.</param>
    public static void Dispose(Font font)
    {
        if (font.Disposed)
        {
            return;
        }

        font.Disposed = true;

        font.FontSystem.Dispose();
        font.FontSystem = null;
        font.SpriteFontBase = null;
    
        GC.SuppressFinalize(font);
    }

    ~Font()
    {
        Dispose(this);
    }
}