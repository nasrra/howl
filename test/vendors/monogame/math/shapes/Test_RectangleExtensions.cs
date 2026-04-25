using Howl.Vendors.MonoGame.Math.Shapes;

using Xunit;

namespace Howl.Test.Vendors.MonoGame.Math.Shapes;

public class Test_RectangleExtensions
{
    [Fact]
    public void ToHowl_Test()
    {
        int x = 1;
        int y = 2;
        int width = 3;
        int height = 4;
        Microsoft.Xna.Framework.Rectangle rectangle = new(x,y,width,height);
        Howl.Math.Shapes.Rectangle result = RectangleExtensions.ToHowl(rectangle);

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
        Howl.Math.Shapes.Rectangle rectangle = new(x,y,width,height);
        Microsoft.Xna.Framework.Rectangle result = RectangleExtensions.ToMonoGame(rectangle);

        Assert.Equal(x, result.X);
        Assert.Equal(y, result.Y);
        Assert.Equal(width, result.Width);
        Assert.Equal(height, result.Height);        
    }
}