using Howl.ECS;
using Howl.Graphics.Text;
using Howl.Graphics;
using Howl.Math;

namespace Howl.Test.Graphics.Text;

public class Text16Test
{
    [Fact]
    public unsafe void Constructor_Test()
    {
        Colour colour = Colour.White;
        Vector2 offset = new(1,2);
        GenIndex fontGenIndex = new(0,1);

        Text16 text = new(
            new TextParameters(colour, offset, fontGenIndex, WorldSpace.World), 
            ['H','e','l','l','o',' ','W','o','r','l','d','.']
        );

        // Verify text TextParameters.
        Assert.Equal(colour, text.TextParameters.Colour);
        Assert.Equal(offset, text.TextParameters.Offset);
        Assert.Equal(fontGenIndex, text.TextParameters.FontGenIndex);
        Assert.Equal(WorldSpace.World, text.TextParameters.WorldSpace);

        // Verify characters.
        Assert.Equal(12, text.Length);
        string actual;
        actual = new string(text.Characters, 0, text.Length);
        Assert.Equal("Hello World.",actual);
    }

    [Fact]
    public void SetCharacters_Test()
    {
        Text16 text = new Text16(
            new TextParameters(Colour.White, Vector2.Zero, new GenIndex(0,0), WorldSpace.World),
            ""
        );

        Span<char> characters = stackalloc char[Text16.MaxLength];
        float num = 123456789.12f;
        num.TryFormat(characters, out int charsWritten, "0.00");

        // set the full span regardless of length.
        text.SetCharacters(characters);
        Assert.Equal(Text16.MaxLength, text.Length);

        // set the span with a specefied length of the valid characters written to it.
        text.SetCharacters(characters, charsWritten);
        Assert.Equal(charsWritten, text.Length);
    }
}