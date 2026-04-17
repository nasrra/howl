using Howl.Vendors.MonoGame.Graphics;

namespace Howl.Test.Vendors.MonoGame.Graphics;

public class ColourExtensionsTest
{
    [Fact]
    public void ToMonoGame_Test()
    {
        byte r = 1;
        byte g = 2;
        byte b = 3;
        byte a = 4;
        Howl.Graphics.Colour color = new(r,g,b,a);
        Microsoft.Xna.Framework.Color result = color.ToMonoGame();
        Assert.Equal(r, result.R);
        Assert.Equal(g, result.G);
        Assert.Equal(b, result.B);
        Assert.Equal(a, result.A);
    }

    [Fact]
    public void ToHowl_Test()
    {
        byte r = 1;
        byte g = 2;
        byte b = 3;
        byte a = 4;
        Microsoft.Xna.Framework.Color color = new(r,g,b,a);
        Howl.Graphics.Colour result = color.ToHowl();
        Assert.Equal(r, result.R);
        Assert.Equal(g, result.G);
        Assert.Equal(b, result.B);
        Assert.Equal(a, result.A);        
    }
}