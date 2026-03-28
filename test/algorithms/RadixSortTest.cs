using Howl.Algorithms.Sorting;

namespace Howl.Test.Algorithms.Sorting;

public class RadixSortTest
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
            uint uA = RadixSort.FloatToUintSortable(fA);
            uint uB = RadixSort.FloatToUintSortable(fB);
            uint uC = RadixSort.FloatToUintSortable(fC);
            uint uD = RadixSort.FloatToUintSortable(fD);

            // test uints.
            Assert.True((uA < uB) && (uA < uC) && (uA < uD));
            Assert.True((uB > uA) && (uB < uC) && (uB < uD));
            Assert.True((uC > uA) && (uC > uB) && (uC < uD));
            Assert.True((uD > uA) && (uD > uB) && (uD > uC));

            // convert back to float.
            Assert.Equal(fA, RadixSort.UintSortableToFloat(uA));
            Assert.Equal(fB, RadixSort.UintSortableToFloat(uB));
            Assert.Equal(fC, RadixSort.UintSortableToFloat(uC));
            Assert.Equal(fD, RadixSort.UintSortableToFloat(uD));
        }
    }

    [Fact]
    public void FloatAscend_Test()
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
        
        // == without buffer ==

        Span<float> nums = [a,j,b,i,c,h,g,d,e,f];
        Span<float> expected = [j,i,h,g,f,e,d,c,b,a];
        
        uint[] translated = new uint[nums.Length];
        uint[] temp = new uint[translated.Length];
        int[] count = new int[256];

        RadixSort.Ascend(nums, translated, temp, count, nums.Length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }

        // == with buffer ==

        nums = [a,j,b,i,c,h,g,d,e,f];
        expected = [j,i,h,g,f,e,d,c,b,a];

        RadixSortBuffer buffer = new(nums.Length);

        RadixSort.Ascend(nums, buffer, nums.Length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }
    }

    [Fact]
    public void FloatDescend_Test()
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

        // == without buffer ==

        Span<float> nums = [a,j,b,i,c,h,g,d,e,f];
        Span<float> expected = [a,b,c,d,e,f,g,h,i,j];
        
        uint[] translated = new uint[nums.Length];
        uint[] temp = new uint[translated.Length];
        int[] count = new int[256];

        RadixSort.Descend(nums, translated, temp, count, nums.Length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }

        // == with buffer ==

        nums = [a,j,b,i,c,h,g,d,e,f];
        expected = [a,b,c,d,e,f,g,h,i,j];

        RadixSortBuffer buffer = new(nums.Length);

        RadixSort.Descend(nums, buffer, nums.Length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }

    }

    [Fact]
    public void FloatIndexedAscend_Test()
    {
        // numbers.
        float nA = 3498.0192f;
        float nB = 123.987f;
        float nC = 90.1234f;
        float nD = 8.123f;
        float nE = 0.0f;
        float nF = -0.0f;
        float nG = -9.129387f;
        float nH = -12.123f;
        float nI = -345.213123f;
        float nJ = -8975.98f;

        // indices.
        int iA = 9;
        int iB = 8;
        int iC = 7;
        int iD = 6;
        int iE = 5;
        int iF = 4;
        int iG = 3;
        int iH = 2;
        int iI = 1;
        int iJ = 0;

        Span<float> nums            = [nA, nJ, nB, nI, nC, nH, nG, nD, nE, nF];
        Span<int> indices           = [iA, iJ, iB, iI, iC, iH, iG, iD, iE, iF];
        Span<float> expectedNums    = [nJ, nI, nH, nG, nF, nE, nD, nC, nB, nA];
        Span<int> expectedIndices   = [iJ, iI, iH, iG, iF, iE, iD, iC, iB, iA];    

        // == without buffer ==

        // create sorting array's.
        uint[] translatedNums = new uint[nums.Length];
        uint[] tempNums = new uint[translatedNums.Length];
        int[] tempIndices = new int[indices.Length];
        int[] count = new int[256];

        RadixSort.IndexedAscend(nums, translatedNums, tempNums, 
            indices, tempIndices, count, nums.Length
        );

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }

        // == with buffer ==
        
        nums    = [nA, nJ, nB, nI, nC, nH, nG, nD, nE, nF];
        indices = [iA, iJ, iB, iI, iC, iH, iG, iD, iE, iF];
        
        RadixSortBuffer buffer = new(10);
        RadixSort.IndexedAscend(nums, indices, buffer, nums.Length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }
    }

    [Fact]
    public void FloatIndexedDescend_Test()
    {
        // numbers.
        float nA = 3498.0192f;
        float nB = 123.987f;
        float nC = 90.1234f;
        float nD = 8.123f;
        float nE = 0.0f;
        float nF = -0.0f;
        float nG = -9.129387f;
        float nH = -12.123f;
        float nI = -345.213123f;
        float nJ = -8975.98f;

        // indices.
        int iA = 9;
        int iB = 8;
        int iC = 7;
        int iD = 6;
        int iE = 5;
        int iF = 4;
        int iG = 3;
        int iH = 2;
        int iI = 1;
        int iJ = 0;

        Span<float> nums            = [nA, nJ, nB, nI, nC, nH, nG, nD, nE, nF];
        Span<int> indices           = [iA, iJ, iB, iI, iC, iH, iG, iD, iE, iF];
        Span<float> expectedNums    = [nA, nB, nC, nD, nE, nF, nG, nH , nI, nJ];
        Span<int> expectedIndices   = [iA, iB, iC, iD, iE, iF, iG, iH , iI, iJ];

        // == without buffer ==

        // create sorting array's.
        uint[] translatedNums = new uint[nums.Length];
        uint[] tempNums = new uint[translatedNums.Length];
        int[] tempIndices = new int[indices.Length];
        int[] count = new int[256];

        RadixSort.IndexedDescend(nums, translatedNums, tempNums, 
            indices, tempIndices, count, nums.Length
        );

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }

        // == with buffer ==

        nums            = [nA, nJ, nB, nI, nC, nH, nG, nD, nE, nF];
        indices         = [iA, iJ, iB, iI, iC, iH, iG, iD, iE, iF];

        RadixSortBuffer buffer = new(10);        
        RadixSort.IndexedDescend(nums, indices, buffer, nums.Length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }
    }
}