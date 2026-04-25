using System.Collections.Generic;
using FontStashSharp;

namespace Howl.Vendors.MonoGame.FontStashSharp;

public class FontManagerState
{
    /// <summary>
    ///     Instances of fonts.
    /// </summary>
    public Font[] Fonts;

    /// <summary>
    ///     A mapping of all sprite font file paths to their respective indices within the <c>SpriteFonts</c> array;
    /// </summary>
    public Dictionary<string, int> FilePathToIndex;

    /// <summary>
    ///     The count of sprite fonts that have been registered; starting from index 0.
    /// </summary>
    public int RegisteredCount;

    /// <summary>
    ///     The maximum amount of sprite fonts this state instance can store.
    /// </summary>
    public int MaxRegisteredCount;

    /// <summary>
    ///     Whether this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new sprite font manager state instance.
    /// </summary>
    /// <param name="maxSpriteFonts"></param>
    public FontManagerState(int maxSpriteFonts)
    {
        Fonts = new Font[maxSpriteFonts];
        FilePathToIndex = new();
        MaxRegisteredCount = maxSpriteFonts;
    }
}