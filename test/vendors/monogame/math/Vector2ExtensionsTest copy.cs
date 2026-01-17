using Howl.Vendors.MonoGame.Math;

using Xunit;

namespace Howl.Test.Vendors.MonoGame.Math;

public class Vector2ExtensionsTest{
    
    [Fact]
    public void ToHowl_Test()
    {
        Microsoft.Xna.Framework.Vector2 monogameVector2 = new(33,44);
        Howl.Math.Vector2 howlVector2 = monogameVector2.ToHowl();
        Assert.Equal(new Howl.Math.Vector2(33,44), howlVector2);
    }

    [Fact]
    public void ToMonoGame_Test()
    {
        Howl.Math.Vector2 howlVector2 = new(55,123);
        Microsoft.Xna.Framework.Vector2 monogameVector2 = howlVector2.ToMonogame();
        Assert.Equal(new Microsoft.Xna.Framework.Vector2(55,123), monogameVector2);
    }
}