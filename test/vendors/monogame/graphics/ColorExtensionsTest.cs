using Howl.Vendors.MonoGame.Graphics;

namespace Howl.Test.Vendors.MonoGame.Graphics;

public class ColorExtensionsTest
{
    [Fact]
    public void ToMonoGame_Test()
    {
        byte r = 1;
        byte g = 2;
        byte b = 3;
        byte a = 4;
        Howl.Graphics.Color color = new(r,g,b,a);
        Microsoft.Xna.Framework.Color result = color.ToMonoGame();
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
        Assert.Equal(a, color.A);
    }

    [Fact]
    public void ToHowl_Test()
    {
        byte r = 1;
        byte g = 2;
        byte b = 3;
        byte a = 4;
        Microsoft.Xna.Framework.Color color = new(r,g,b,a);
        Howl.Graphics.Color result = color.ToHowl();
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
        Assert.Equal(a, color.A);        
    }
}