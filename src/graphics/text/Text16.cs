
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
    /// 
    /// </summary>
    /// <param name="textParameters">The text parameters used when drawing.</param>
    /// <param name="characters">the chracters to draw.</param>
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
            throw new InvalidOperationException($"Text cannot be constructed with a span of characters of length '{characters.Length}'. Max span length is '{MaxLength}'");
        }
#endif

        length = System.Math.Min(characters.Length, MaxLength);

        fixed (char* dst = Characters)
        {
            characters[..length].CopyTo(new Span<char>(dst, MaxLength));
        }
    }

    public void SetCharacters(ReadOnlySpan<char> characters, int length)
    {
#if DEBUG
        if(characters.Length > MaxLength || length < 0 || length > MaxLength)
        {
            throw new InvalidOperationException($"Text cannot be constructed with a span of characters of length '{characters.Length}'. Max span length is '{MaxLength}' and Min span length is '0'");
        }
#endif
        
        this.length = length;

        fixed (char* dst = Characters)
        {
            characters[..length].CopyTo(new Span<char>(dst, MaxLength));
        }
    }
}