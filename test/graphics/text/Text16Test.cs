using Howl.Ecs;
using Howl.Graphics.Text;
using Howl.Graphics;
using Howl.Math;
using System.Text;

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
            new TextParameters(colour, offset, fontGenIndex, DrawSpace.World), 
            ['H','e','l','l','o',' ','W','o','r','l','d','.']
        );

        // Verify text TextParameters.
        Assert.Equal(colour, text.TextParameters.Colour);
        Assert.Equal(offset, text.TextParameters.Offset);
        Assert.Equal(fontGenIndex, text.TextParameters.FontGenIndex);
        Assert.Equal(DrawSpace.World, text.TextParameters.WorldSpace);

        // Verify characters.
        Assert.Equal(12, text.Length);
        string actual;
        actual = new string(text.Characters, 0, text.Length);
        Assert.Equal("Hello World.",actual);
    }

    [Fact]
    public unsafe void SetCharacters_Test()
    {
        Text16 text = new Text16(
            new TextParameters(Colour.White, Vector2.Zero, new GenIndex(0,0), DrawSpace.World),
            ""
        );
        Span<char> characters = stackalloc char[Text16.MaxCharacters];

        TextProc.SetCharacters(ref text, "foo");
        Assert.Equal(3, text.Length);
        Assert.Equal("foo", new string(text.Characters, 0, text.Length));
        TextProc.SetCharacters(ref text, "lorem ipsum");
        Assert.Equal(11, text.Length);
        Assert.Equal("lorem ipsum", new string(text.Characters, 0, text.Length));

        // set characters to floats.
        TextProc.SetCharacters(ref text, 1234567f);
        Assert.Equal(7, text.Length);
        Assert.Equal("1234567", new string(text.Characters, 0, text.Length));
        TextProc.SetCharacters(ref text, 99.99f);
        Assert.Equal(5, text.Length);
        Assert.Equal("99.99", new string(text.Characters, 0, text.Length));
    }

    [Fact]
    public unsafe void AppendCharacters_Test()
    {
        Text16 text = new Text16(
            new TextParameters(Colour.White, Vector2.Zero, new GenIndex(0,0), DrawSpace.World),
            ""
        );

        // append strings.
        TextProc.AppendCharacters(ref text, "Tools");
        Assert.Equal(5, text.Length);
        Assert.Equal("Tools", new string(text.Characters, 0, text.Length));
        TextProc.AppendCharacters(ref text, " are cool.");        
        Assert.Equal(15, text.Length);
        Assert.Equal("Tools are cool.", new string(text.Characters, 0, text.Length));
    
        TextProc.ClearCharacters(ref text);

        // append floats
        TextProc.AppendCharacters(ref text, 123.123f);
        Assert.Equal(7, text.Length);
        Assert.Equal("123.123", new string(text.Characters, 0, text.Length));
        TextProc.AppendCharacters(ref text, 456.456f);        
        Assert.Equal(14, text.Length);
        Assert.Equal("123.123456.456", new string(text.Characters, 0, text.Length));
    }

    [Fact]
    public void AsSpanUsed_Test()
    {
        Text16 text = new Text16(
            new TextParameters(Colour.White, Vector2.Zero, new GenIndex(0,0), DrawSpace.World),
            ""
        );
        TextProc.AppendCharacters(ref text, "Tools");

        StringBuilder stringBuilder = new();
        stringBuilder.Append(text.AsSpanUsed());

        Assert.Equal(5, text.AsSpanUsed().Length);
        Assert.Equal(Text16.MaxCharacters, text.AsSpan().Length);
        Assert.Equal("Tools", stringBuilder.ToString());
    }
}