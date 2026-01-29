using System;
using System.Diagnostics;

namespace Howl.Graphics.Text;

public interface IText
{
    /// <summary>
    /// Sets this text's characters to draw.
    /// </summary>
    /// <param name="characters">The specfied span of characters to draw.</param>
    public void SetCharacters(ReadOnlySpan<char> characters);

    /// <summary>
    /// Sets this text's characters to draw and the length of characters in the specified span.
    /// </summary>
    /// <param name="characters">The specified span of characters to draw.</param>
    /// <param name="length">The length of valid characters in the span.</param>
    public void SetCharacters(ReadOnlySpan<char> characters, int length);
}