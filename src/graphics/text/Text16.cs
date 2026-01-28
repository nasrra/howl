
using System;

namespace Howl.Graphics.Text;

public unsafe struct Text16 : IText
{
    public const int MaxLength = 16;
    
    /// <summary>
    /// Gets and sets the TextParameters.
    /// </summary>
    public TextParameters TextParameters;

    private int length;

    /// <summary>
    /// Gets the current length of the stored characters/text.
    /// </summary>
    public int Length => length;

    /// <summary>
    /// Gets and sets the text string when drawing.
    /// </summary>
    public fixed char Characters[MaxLength];

    /// <summary>
    /// Constructs a Text.
    /// </summary>
    /// <param name="colour">The draw colour.</param>
    /// <param name="offset">The offset - in pixels - when drawing.</param>
    /// <param name="characters">The span of characters that will render when drawing (max length of 16.)</param>
    public Text16(TextParameters textParameters, ReadOnlySpan<char> characters)
    {
        TextParameters = textParameters;
        SetCharacters(characters);
    }

    public void SetCharacters(ReadOnlySpan<char> characters)
    {
#if DEBUG
        if(characters.Length > MaxLength)
        {
            throw new InvalidOperationException($"Text16 cannot be constructed with a span of characters of length '{characters.Length}'. Max span length is '{MaxLength}'");
        }
#endif

        length = System.Math.Min(characters.Length, MaxLength);

        fixed (char* dst = Characters)
        {
            characters[..length].CopyTo(new Span<char>(dst, MaxLength));
        }
    }
}