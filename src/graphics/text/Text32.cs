
using System;

namespace Howl.Graphics.Text;

public unsafe struct Text32
{
    public const int MaxCharacters = 32;
    public const int MinCharacters = 0;
    
    /// <summary>
    /// Gets and sets the TextParameters.
    /// </summary>
    public TextParameters TextParameters;
    
    /// <summary>
    /// Gets the current length of the stored characters/text.
    /// </summary>
    public int Length;

    /// <summary>
    /// Gets and sets the text string when drawing.
    /// </summary>
    public fixed char Characters[MaxCharacters];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textParameters">The text parameters used when drawing.</param>
    /// <param name="characters">the chracters to draw.</param>
    public Text32(TextParameters textParameters, ReadOnlySpan<char> characters)
    {
        TextParameters = textParameters;
        TextProc.SetCharacters(ref this, characters);
    }

    /// <summary>
    /// Gets the full characters array as a span.
    /// </summary>
    /// <returns>the span containging the characters.</returns>
    public Span<char> AsSpan()
    {
        fixed(char* chars = Characters)
        {
            return new Span<char>(chars, MaxCharacters);
        }
    }

    /// <summary>
    /// Gets a span containging only the used characters in the internal characters array.
    /// </summary>
    /// <remarks>
    /// Note: the span length will not be equal to the max characters, rather it will be the length of this text.
    /// </remarks>
    /// <returns>the span containing the characters.</returns>
    public Span<char> AsSpanUsed()
    {
        fixed(char* chars = Characters)
        {
            return new Span<char>(chars, Length);
        }        
    }
}