using System;

namespace Howl.Graphics.Text;

public interface IText
{
    /// <summary>
    /// Sets this text's characters to draw.
    /// </summary>
    /// <param name="characters">The specfied span of characters to draw.</param>
    public void SetCharacters(ReadOnlySpan<char> characters);
}