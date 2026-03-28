using Howl.Algorithms;

namespace Howl.Test.Algorithms;

public class SortTest
{
    [Fact]
    public void FloatToUintSortable_Test(){
        
        float additive = 1.12397f;             
        
        for(int i = 0; i < 10; i++)
        {
            additive += additive;
            
            // the floating values to convert.
            float fA = -123.34f + additive;
            float fB = -3.123f + additive;
            float fC = 2.1239f + additive;
            float fD = 98.98f + additive;
            
            // convert to uint.
            uint uA = Sort.FloatToUintSortable(fA);
            uint uB = Sort.FloatToUintSortable(fB);
            uint uC = Sort.FloatToUintSortable(fC);
            uint uD = Sort.FloatToUintSortable(fD);

            // test uints.
            Assert.True((uA < uB) && (uA < uC) && (uA < uD));
            Assert.True((uB > uA) && (uB < uC) && (uB < uD));
            Assert.True((uC > uA) && (uC > uB) && (uC < uD));
            Assert.True((uD > uA) && (uD > uB) && (uD > uC));

            // convert back to float.
            Assert.Equal(fA, Sort.UintSortableToFloat(uA));
            Assert.Equal(fB, Sort.UintSortableToFloat(uB));
            Assert.Equal(fC, Sort.UintSortableToFloat(uC));
            Assert.Equal(fD, Sort.UintSortableToFloat(uD));
        }
    }

    [Fact]
    public void Radix_Test()
    {
        float a = 3498.0192f;
        float b = 123.987f;
        float c = 90.1234f;
        float d = 8.123f;
        float e = 0.0f;
        float f = -0.0f;
        float g = -9.129387f;
        float h = -12.123f;
        float i = -345.213123f;
        float j = -8975.98f;
        
        float[] nums = [a,j,b,i,c,h,g,d,e,f];
        float[] expected = [j,i,h,g,f,e,d,c,b,a];
        uint[] buffer = new uint[nums.Length];
        uint[] temp = new uint[buffer.Length];
        int[] count = new int[256];

        Sort.Radix(nums, buffer, temp, count, nums.Length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }
    }
}