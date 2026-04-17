using Howl.Vendors.MonoGame.Math;

using Xunit;

namespace Howl.Test.Vendors.MonoGame.Math;

public class MatrixExtensionsTest
{
    
    [Fact]
    public void ToHowl_Test()
    {
        Microsoft.Xna.Framework.Matrix matrix = new(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16);
        Howl.Math.Matrix result = matrix.ToHowl();
        Assert.Equal(1, result.M11);
        Assert.Equal(2, result.M12);
        Assert.Equal(3, result.M13);
        Assert.Equal(4, result.M14);
        Assert.Equal(5, result.M21);
        Assert.Equal(6, result.M22);
        Assert.Equal(7, result.M23);
        Assert.Equal(8, result.M24);
        Assert.Equal(9, result.M31);
        Assert.Equal(10, result.M32);
        Assert.Equal(11, result.M33);
        Assert.Equal(12, result.M34);
        Assert.Equal(13, result.M41);
        Assert.Equal(14, result.M42);
        Assert.Equal(15, result.M43);
        Assert.Equal(16, result.M44);
    }

    [Fact]
    public void ToMonoGame_Test()
    {
        Howl.Math.Matrix matrix = new(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16);
        Microsoft.Xna.Framework.Matrix result = matrix.ToMonoGame();
        Assert.Equal(1, result.M11);
        Assert.Equal(2, result.M12);
        Assert.Equal(3, result.M13);
        Assert.Equal(4, result.M14);
        Assert.Equal(5, result.M21);
        Assert.Equal(6, result.M22);
        Assert.Equal(7, result.M23);
        Assert.Equal(8, result.M24);
        Assert.Equal(9, result.M31);
        Assert.Equal(10, result.M32);
        Assert.Equal(11, result.M33);
        Assert.Equal(12, result.M34);
        Assert.Equal(13, result.M41);
        Assert.Equal(14, result.M42);
        Assert.Equal(15, result.M43);
        Assert.Equal(16, result.M44);        
    }
}