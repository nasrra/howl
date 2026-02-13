using System;
using Howl.ECS;

namespace Howl.Graphics.Text;

public interface IFontManager : IDisposable
{
    /// <summary>
    /// Gets whether or not this Font Manager has been disposed.
    /// </summary>
    public bool IsDisposed{get;}

    /// <summary>
    /// Loads a new Font asset into memory.
    /// </summary>
    /// <param name="fontFilePath">The font file path to load.</param>
    public void LoadFont(string fontFilePath, out GenIndex genIndex);

    /// <summary>
    /// Checks is a font has been loaded.
    /// </summary>
    /// <param name="genIndex">the gen index of the font.</param>
    /// <returns><see cref="GenIndexResult.Ok"/> if the font is loaded, otherwise false.</returns>
    public GenIndexResult IsFontLoaded(GenIndex genIndex);
}