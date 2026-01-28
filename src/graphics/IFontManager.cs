using System;
using Howl.Math;
using Howl.ECS;

namespace Howl.Graphics;

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
}