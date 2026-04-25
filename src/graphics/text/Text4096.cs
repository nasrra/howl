
using System;

namespace Howl.Graphics.Text;

public unsafe struct Text4096
{
    public const int MinCharacters = 0;
    public const int MaxCharacters = 4096;

    /// <summary>
    /// Gets and sets the TextParameters.
    /// </summary>
    public TextParameters TextParameters;


    /// <summary>
    /// Gets and sets the current length of the stored characters/text.
    /// </summary>
    public int Length;

    /// <summary>
    /// Gets and sets the text string when drawing.
    /// </summary>
    public fixed char Characters[MaxCharacters];

    public int FontId;

    /// <summary>
    /// Constructs a Text.
    /// </summary>
    /// <param name="colour">The draw colour.</param>
    /// <param name="offset">The offset - in pixels - when drawing.</param>
    /// <param name="characters">The span of characters that will render when drawing (max length of 16.)</param>
    public Text4096(TextParameters textParameters, ReadOnlySpan<char> characters, int fontId)
    {
        TextParameters = textParameters;
        TextProc.SetCharacters(ref this, characters);
        FontId = fontId;
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