using Howl.Math;
using Xunit;

namespace Howl.Test.Math;

public class MatrixTest
{
    [Fact]
    public void Contructor_Test()
    {
        Matrix matrix = new(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16);
        Assert.Equal(1, matrix.M11);
        Assert.Equal(2, matrix.M12);
        Assert.Equal(3, matrix.M13);
        Assert.Equal(4, matrix.M14);
        Assert.Equal(5, matrix.M21);
        Assert.Equal(6, matrix.M22);
        Assert.Equal(7, matrix.M23);
        Assert.Equal(8, matrix.M24);
        Assert.Equal(9, matrix.M31);
        Assert.Equal(10, matrix.M32);
        Assert.Equal(11, matrix.M33);
        Assert.Equal(12, matrix.M34);
        Assert.Equal(13, matrix.M41);
        Assert.Equal(14, matrix.M42);
        Assert.Equal(15, matrix.M43);
        Assert.Equal(16, matrix.M44);
    }
}