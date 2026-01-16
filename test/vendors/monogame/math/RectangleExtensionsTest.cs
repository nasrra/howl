using Howl.Vendors.MonoGame.Math;

using Xunit;

namespace Howl.Test.Vendors.MonoGame.Math;

public class RectangleExtensionsTest
{
    [Fact]
    public void ToHowl_Test()
    {
        int x = 1;
        int y = 2;
        int width = 3;
        int height = 4;
        Microsoft.Xna.Framework.Rectangle rectangle = new(x,y,width,height);
        Howl.Math.Rectangle result = rectangle.ToHowl();

        Assert.Equal(x, result.X);
        Assert.Equal(y, result.Y);
        Assert.Equal(width, result.Width);
        Assert.Equal(height, result.Height);
    }

    [Fact]
    public void ToMonoGame_Test()
    {
        float x = 1;
        float y = 2;
        float width = 3;
        float height = 4;
        Howl.Math.Rectangle rectangle = new(x,y,width,height);
        Microsoft.Xna.Framework.Rectangle result = rectangle.ToMonoGame();

        Assert.Equal(x, result.X);
        Assert.Equal(y, result.Y);
        Assert.Equal(width, result.Width);
        Assert.Equal(height, result.Height);        
    }
}