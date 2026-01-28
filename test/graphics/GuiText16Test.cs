using Howl.ECS;
using Howl.Graphics.Text;
using Howl.Graphics;
using Howl.Math;

namespace Howl.Test.Graphics.Text;

public class GuiText16Test
{
    [Fact]
    public unsafe void Constructor_Test()
    {
        Colour colour = Colour.White;
        Vector2 offset = new(1,2);
        GenIndex fontGenIndex = new(0,1);

        GuiText16 text = new(
            new TextParameters(colour, offset, fontGenIndex), 
            ['H','e','l','l','o',' ','W','o','r','l','d','.']
        );

        // Verify text TextParameters.
        Assert.Equal(colour, text.TextParameters.Colour);
        Assert.Equal(offset, text.TextParameters.Offset);
        Assert.Equal(fontGenIndex, text.TextParameters.FontGenIndex);

        // Verify characters.
        Assert.Equal(12, text.Length);
        string actual;
        actual = new string(text.Characters, 0, text.Length);
        Assert.Equal("Hello World.",actual);
    }
}