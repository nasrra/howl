using Howl.Algorithms;
using Howl.Math;

namespace Howl.Test.Algorithms;

public class MortonCodeTest
{
    [Fact]
    public void ExpandBits_Test()
    {
        Assert.Equal((uint)256, MortonCode.ExpandBits(16));
        Assert.Equal((uint)5456, MortonCode.ExpandBits(124));
        Assert.Equal((uint)21569, MortonCode.ExpandBits(233));
        Assert.Equal((uint)0, MortonCode.ExpandBits(0));
        Assert.Equal((uint)1, MortonCode.ExpandBits(1));
    }

    [Fact]
    public void CalculateScaleFactor_Test()
    {
        int length = 10;

        float scaleX = 0;
        float scaleY = 0;

        for(int x = 0; x < length; x++)
        {
            int minX = x - length;
            int maxX = x + length;

            int rangeX = maxX - minX;

            for(int y = 0; y < length; y++)
            {
                int minY = y - length;
                int maxY = y + length;
            
                int rangeY = maxY - minY;

                MortonCode.CalculateScaleFactor(rangeX, rangeY, ref scaleX, ref scaleY);

                Assert.Equal(65535.0f / rangeX, scaleX);
                Assert.Equal(65535.0f / rangeY, scaleY);
            }
        }
    }

    [Fact]
    public void CalcualteMortonCode_Test()
    {
        float minX = -11;
        float minY = -32;
        float maxX = 16;
        float maxY = 157;

        float rangeX = Howl.Math.Math.Abs(maxX - minX);
        float rangeY = Howl.Math.Math.Abs(maxY - minY);

        Assert.Equal((uint)3557148913, MortonCode.CalculateMortonCode(13, 63, minX, minY, rangeX, rangeY));
    }

}