using Howl.Vendors.MonoGame.Math;

using Xunit;

namespace Howl.Test.Vendors.MonoGame.Math;

public class Test_Vector3Extensions{
    
    [Fact]
    public void ToHowl_Test()
    {
        Microsoft.Xna.Framework.Vector3 monogameVector = new(33,44, 55);
        Howl.Math.Vector3 howlVector = monogameVector.ToHowl();
        Assert.Equal(new Howl.Math.Vector3(33,44,55), howlVector);
    }

    [Fact]
    public void ToMonoGame_Test()
    {
        Howl.Math.Vector3 howlVector = new(55, 123, 256);
        Microsoft.Xna.Framework.Vector3 monogameVector = howlVector.ToMonoGame();
        Assert.Equal(new Microsoft.Xna.Framework.Vector3(55, 123, 256), monogameVector);
    }
}