using Howl.MonoGame.Math;

using Xunit;

namespace Howl.Test.MonoGame.Math;

public class Vector2TranslatorTest{
    
    [Fact]
    public void ToHowlVector2()
    {
        Microsoft.Xna.Framework.Vector2 monogameVector2 = new(33,44);
        Howl.Math.Vector2 howlVector2 = Vector2Translator.ToHowlVector2(monogameVector2);
        Assert.Equal(new Howl.Math.Vector2(33,44), howlVector2);
    }

    [Fact]
    public void ToMonogameVector2()
    {
        Howl.Math.Vector2 howlVector2 = new(55,123);
        Microsoft.Xna.Framework.Vector2 monogameVector2 = Vector2Translator.ToMonogameVector2(howlVector2);
        Assert.Equal(new Microsoft.Xna.Framework.Vector2(55,123), monogameVector2);
    }
}