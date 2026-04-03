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
    public void SimdAndSisdMortonCodeMatch_()
    {
        int length = 15;
        
        float[] x = new float[length];
        float[] y = new float[length];
        uint[] mortonCodes = new uint[length];

        for(int i = 0; i < length; i++)
        {
            x[i] = i+(i*0.001f);
            y[i] = i+(i*0.001f);
        }

        float scaleX = 0;
        float scaleY = 0; 
        MortonCode.CalculateScaleFactor(length, length, ref scaleX, ref scaleY);
        MortonCode.CalculateMortonCodes(x, y, mortonCodes, 0, 0, scaleX, scaleY, length);

        for(int i = 0; i < length; i++)
        {
            Assert.Equal(MortonCode.CalculateMortonCode(i+(i*0.001f), i+(i*0.001f) , 0, 0, scaleX, scaleY), mortonCodes[i]);
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
        float scaleX = 0;
        float scaleY = 0;

        MortonCode.CalculateScaleFactor(rangeX, rangeY, ref scaleX, ref scaleY);

        Assert.Equal((uint)3557148913, MortonCode.CalculateMortonCode(13, 63, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)3562602602, MortonCode.CalculateMortonCode(14, 64, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)2320928610, MortonCode.CalculateMortonCode(-9.5f, 99.2f, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)1075270331, MortonCode.CalculateMortonCode(3.32f, -31.17f, minX, minY, scaleX, scaleY));

        Assert.Equal((uint)2079204108, MortonCode.CalculateMortonCode(12, 62, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)2124792561, MortonCode.CalculateMortonCode(13, 60, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)2333435800, MortonCode.CalculateMortonCode(-8.5f, 98.2f, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)358365900, MortonCode.CalculateMortonCode(2.32f, -30.17f, minX, minY, scaleX, scaleY));

        Assert.Equal((uint)2074691649, MortonCode.CalculateMortonCode(11.1f, 61.12f, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)2080289046, MortonCode.CalculateMortonCode(12.45f, 62.32f, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)2359930623, MortonCode.CalculateMortonCode(-7.5f, 97.2f, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)354087543, MortonCode.CalculateMortonCode(1.32f, -29.17f, minX, minY, scaleX, scaleY));

        Assert.Equal((uint)3557110738, MortonCode.CalculateMortonCode(12.98f, 62.76f, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)2363923595, MortonCode.CalculateMortonCode(-6.53f, 96.22f, minX, minY, scaleX, scaleY));
        Assert.Equal((uint)342319152, MortonCode.CalculateMortonCode(0.34f, -28.11f, minX, minY, scaleX, scaleY));
    }

    [Fact]
    public void CalculateMortonCodes_Test()
    {
        float minX = -11;
        float minY = -32;
        float maxX = 16;
        float maxY = 157;

        float rangeX = Howl.Math.Math.Abs(maxX - minX);
        float rangeY = Howl.Math.Math.Abs(maxY - minY);
        float scaleX = 0;
        float scaleY = 0;

        MortonCode.CalculateScaleFactor(rangeX, rangeY, ref scaleX, ref scaleY);

        float[] x = [13, 14, -9.5f, 3.32f, 12, 13, -8.5f, 2.32f, 11.1f, 12.45f, -7.5f, 1.32f, 12.98f, -6.53f, 0.34f];
        float[] y = [63, 64, 99.2f, -31.17f, 62, 60, 98.2f, -30.17f, 61.12f, 62.32f, 97.2f, -29.17f, 62.76f, 96.22f, -28.11f];
        uint[] expected = [
            3557148913,3562602602,2320928610,1075270331,2079204108,2124792561,2333435800,358365900,2074691649,2080289046,2359930623,354087543,
            3557110738,2363923595,342319152
        ];
        uint[] mortonCodes = new uint[15];

        MortonCode.CalculateMortonCodes(x, y, mortonCodes, minX, minY, scaleX, scaleY, mortonCodes.Length);

        for(int i = 0; i < mortonCodes.Length; i++)
        {
            Assert.Equal(expected[i], mortonCodes[i]);
        }
    }
}