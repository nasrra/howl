using Howl.Vendors.MonoGame.Math;

using Xunit;

namespace Howl.Test.Vendors.MonoGame.Math;

public class Test_Vector2Extensions{
    
    [Fact]
    public void ToHowl_Test()
    {
        Microsoft.Xna.Framework.Vector2 monogameVector2 = new(33,44);
        Howl.Math.Vector2 howlVector2 = Vector2Extensions.ToHowl(monogameVector2);
        Assert.Equal(new Howl.Math.Vector2(33,44), howlVector2);
    }

    [Fact]
    public void ToMonoGame_Test()
    {
        Howl.Math.Vector2 howlVector2 = new(55,123);
        Microsoft.Xna.Framework.Vector2 monogameVector2 = Vector2Extensions.ToMonoGame(howlVector2);
        Assert.Equal(new Microsoft.Xna.Framework.Vector2(55,123), monogameVector2);
    }
}