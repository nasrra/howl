using Howl.Algorithms.Sorting;

namespace Howl.Test.Algorithms.Sorting;

public class RadixSortFTest
{
    public const float ValueA = 3498.0192f;
    public const float ValueB = 123.987f;
    public const float ValueC = 90.1234f;
    public const float ValueD = 8.123f;
    public const float ValueE = 0.0f;
    public const float ValueF = -0.0f;
    public const float ValueG = -9.129387f;
    public const float ValueH = -12.123f;
    public const float ValueI = -345.213123f;
    public const float ValueJ = -8975.98f;
    public const int IndexA = 9;
    public const int IndexB = 8;
    public const int IndexC = 7;
    public const int IndexD = 6;
    public const int IndexE = 5;
    public const int IndexF = 4;
    public const int IndexG = 3;
    public const int IndexH = 2;
    public const int IndexI = 1;
    public const int IndexJ = 0;




    /*******************
    
        Bit Converions.
    
    ********************/




    [Fact]
    public void SortableUintConversion_Test(){
        
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
            uint uA = RadixSortF.ToSortableUint(fA);
            uint uB = RadixSortF.ToSortableUint(fB);
            uint uC = RadixSortF.ToSortableUint(fC);
            uint uD = RadixSortF.ToSortableUint(fD);

            // test uints.
            Assert.True((uA < uB) && (uA < uC) && (uA < uD));
            Assert.True((uB > uA) && (uB < uC) && (uB < uD));
            Assert.True((uC > uA) && (uC > uB) && (uC < uD));
            Assert.True((uD > uA) && (uD > uB) && (uD > uC));

            // convert back to float.
            Assert.Equal(fA, RadixSortF.FromSortableUint(uA));
            Assert.Equal(fB, RadixSortF.FromSortableUint(uB));
            Assert.Equal(fC, RadixSortF.FromSortableUint(uC));
            Assert.Equal(fD, RadixSortF.FromSortableUint(uD));

            // the spans to convert.
            float[] nums = [fA, fB, fC, fD];
            uint[] sortables = new uint[nums.Length];
            RadixSortF.ToSortableUint(nums, sortables);

            // test uints.
            uA = sortables[0];
            uB = sortables[1];
            uC = sortables[2];
            uD = sortables[3];
            Assert.True((uA < uB) && (uA < uC) && (uA < uD));
            Assert.True((uB > uA) && (uB < uC) && (uB < uD));
            Assert.True((uC > uA) && (uC > uB) && (uC < uD));
            Assert.True((uD > uA) && (uD > uB) && (uD > uC));

            // convert back to float.
            RadixSortF.FromSortableUint(sortables, nums);
            Assert.Equal(fA, nums[0]);
            Assert.Equal(fB, nums[1]);
            Assert.Equal(fC, nums[2]);
            Assert.Equal(fD, nums[3]);
        }
    }




    /*******************
    
        Ascend.
    
    ********************/




    [Fact]
    public void Ascend_Test()
    {
        float[] nums;
        float[] expected;
        uint[] translated;
        uint[] temp;
        int[] count;

        // == full ==

        nums = [ValueA,ValueJ,ValueB,ValueI,ValueC,ValueH,ValueG,ValueD,ValueE,ValueF];
        expected = [ValueJ,ValueI,ValueH,ValueG,ValueF,ValueE,ValueD,ValueC,ValueB,ValueA];
        translated = new uint[nums.Length];
        temp = new uint[nums.Length];
        count = new int[256];

        RadixSortF.Ascend(nums, translated, temp, count);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }

        // == slice ==

        nums = [ValueA,ValueJ,ValueB,ValueI,ValueC,ValueH,ValueG,ValueD,ValueE,ValueF];
        expected = [ValueA,ValueJ,ValueI,ValueH,ValueG,ValueD,ValueC,ValueB,ValueE,ValueF];
        translated = new uint[nums.Length];
        temp = new uint[nums.Length];
        count = new int[256];
        int start = 2;
        int length = nums.Length - 4;

        RadixSortF.Ascend(nums, translated, temp, count, start, length);

        for(int q = start; q < length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }
    }

    [Fact]
    public void BufferedAscend_Test()
    {
        RadixSortBuffer buffer;
        float[] nums;
        float[] expected;

        // == full ==

        nums = [ValueA,ValueJ,ValueB,ValueI,ValueC,ValueH,ValueG,ValueD,ValueE,ValueF];
        expected = [ValueJ,ValueI,ValueH,ValueG,ValueF,ValueE,ValueD,ValueC,ValueB,ValueA];
        buffer = new(nums.Length);

        RadixSortF.Ascend(nums, buffer);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }

        // == slice ==

        nums = [ValueA,ValueJ,ValueB,ValueI,ValueC,ValueH,ValueG,ValueD,ValueE,ValueF];
        expected = [ValueA,ValueJ,ValueI,ValueH,ValueG,ValueD,ValueC,ValueB,ValueE,ValueF];
        int start = 2;
        int length = nums.Length - 4;
        buffer = new(nums.Length);

        RadixSortF.Ascend(nums, buffer, start, length);

        for(int q = start; q < length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }      
    }

    [Fact]
    public void IndexedAscend_Test()
    {
        float[] nums;
        float[] expectedNums;
        int[] indices;
        int[] expectedIndices;
        uint[] translatedNums;
        uint[] tempNums;
        int[] tempIndices;
        int[] count;


        // == full ==

        nums            = [ValueA, ValueJ, ValueB, ValueI, ValueC, ValueH, ValueG, ValueD, ValueE, ValueF];
        indices         = [IndexA, IndexJ, IndexB, IndexI, IndexC, IndexH, IndexG, IndexD, IndexE, IndexF];
        expectedNums    = [ValueJ, ValueI, ValueH, ValueG, ValueF, ValueE, ValueD, ValueC, ValueB, ValueA];
        expectedIndices = [IndexJ, IndexI, IndexH, IndexG, IndexF, IndexE, IndexD, IndexC, IndexB, IndexA];    
        translatedNums = new uint[nums.Length];
        tempNums = new uint[translatedNums.Length];
        tempIndices = new int[indices.Length];
        count = new int[256];

        RadixSortF.IndexedAscend(nums, translatedNums, tempNums, indices, tempIndices, count);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }

        // == slice ==
        nums            = [ValueA, ValueJ, ValueB, ValueI, ValueC, ValueH, ValueG, ValueD, ValueE, ValueF];
        indices         = [IndexA, IndexJ, IndexB, IndexI, IndexC, IndexH, IndexG, IndexD, IndexE, IndexF];
        expectedNums    = [ValueA, ValueJ, ValueI, ValueH, ValueG, ValueD, ValueC, ValueB, ValueE, ValueF]; 
        expectedIndices = [IndexA, IndexJ, IndexI, IndexH, IndexG, IndexD, IndexC, IndexB, IndexE, IndexF];    
        translatedNums = new uint[nums.Length];
        tempNums = new uint[translatedNums.Length];
        tempIndices = new int[indices.Length];
        count = new int[256];
        int start = 2;
        int length = nums.Length - 4;

        
        RadixSortF.IndexedAscend(nums, translatedNums, tempNums, indices, tempIndices, count, start, length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }
    }

    [Fact]
    public void BufferedIndexedAscend_Test()
    {
        RadixSortBuffer buffer;
        float[] nums;
        float[] expectedNums;
        int[] indices;
        int[] expectedIndices;
        int start;
        int length;


        // == full ==

        nums            = [ValueA, ValueJ, ValueB, ValueI, ValueC, ValueH, ValueG, ValueD, ValueE, ValueF];
        indices         = [IndexA, IndexJ, IndexB, IndexI, IndexC, IndexH, IndexG, IndexD, IndexE, IndexF];
        expectedNums    = [ValueJ, ValueI, ValueH, ValueG, ValueF, ValueE, ValueD, ValueC, ValueB, ValueA];
        expectedIndices = [IndexJ, IndexI, IndexH, IndexG, IndexF, IndexE, IndexD, IndexC, IndexB, IndexA];    
        buffer = new(nums.Length);
        start = 0;
        length = nums.Length;

        RadixSortF.IndexedAscend(nums, indices, buffer, start, length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }

        // == slice ==
        
        nums            = [ValueA, ValueJ, ValueB, ValueI, ValueC, ValueH, ValueG, ValueD, ValueE, ValueF];
        indices         = [IndexA, IndexJ, IndexB, IndexI, IndexC, IndexH, IndexG, IndexD, IndexE, IndexF];
        expectedNums    = [ValueA, ValueJ, ValueI, ValueH, ValueG, ValueD, ValueC, ValueB, ValueE, ValueF]; 
        expectedIndices = [IndexA, IndexJ, IndexI, IndexH, IndexG, IndexD, IndexC, IndexB, IndexE, IndexF];    
        buffer = new(nums.Length);
        start = 2;
        length = nums.Length - 4;

        
        RadixSortF.IndexedAscend(nums, indices, buffer, start, length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }
    }




    /*******************
    
        Descend.
    
    ********************/




    [Fact]
    public void Descend_Test()
    {
        float[] nums;
        float[] expected;
        uint[] translated;
        uint[] temp;
        int[] count = new int[256];

        int start;
        int length;

        // == full ==

        nums = [ValueA,ValueJ,ValueB,ValueI,ValueC,ValueH,ValueG,ValueD,ValueE,ValueF];
        expected = [ValueA,ValueB,ValueC,ValueD,ValueE,ValueF,ValueG,ValueH,ValueI,ValueJ];
        translated = new uint[nums.Length];
        temp = new uint[nums.Length];
        start = 0;
        length = nums.Length;

        RadixSortF.Descend(nums, translated, temp, count, start, length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }

        // == slice ==

        nums        = [ValueA, ValueJ, ValueB, ValueI, ValueC, ValueH, ValueG, ValueD, ValueE, ValueF];
        expected    = [ValueA, ValueJ, ValueB, ValueC, ValueD, ValueG, ValueH, ValueI, ValueE, ValueF];
        translated = new uint[nums.Length];
        temp = new uint[nums.Length];
        start = 2;
        length = nums.Length - 4;

        RadixSortF.Descend(nums, translated, temp, count, start, length);

        for(int q = start; q < length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }
    }

    [Fact]
    public void BufferedDescend_Test()
    {
        RadixSortBuffer buffer;
        float[] nums;
        float[] expected;
        int start;
        int length;

        // == full ==

        nums        = [ValueA,ValueJ,ValueB,ValueI,ValueC,ValueH,ValueG,ValueD,ValueE,ValueF];
        expected    = [ValueA,ValueB,ValueC,ValueD,ValueE,ValueF,ValueG,ValueH,ValueI,ValueJ];
        start = 0;
        length = nums.Length;
        buffer = new(nums.Length);

        RadixSortF.Descend(nums, buffer, start, length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }

        // == slice ==

        nums        = [ValueA,ValueJ,ValueB,ValueI,ValueC,ValueH,ValueG,ValueD,ValueE,ValueF];        
        expected    = [ValueA,ValueJ,ValueB,ValueC,ValueD,ValueG,ValueH,ValueI,ValueE,ValueF];
        start = 2;
        length = nums.Length - 4;
        buffer = new(nums.Length);

        RadixSortF.Descend(nums, buffer, start, length);

        for(int q = start; q < length; q++)
        {
            Assert.Equal(expected[q], nums[q]);
        }      
    }

    [Fact]
    public void IndexedDescend_Test()
    {
        float[] nums;
        float[] expectedNums;
        int[] indices;
        int[] expectedIndices;
        uint[] translatedNums;
        uint[] tempNums;
        int[] tempIndices;
        int[] count;


        // == full ==

        nums            = [ValueA, ValueJ, ValueB, ValueI, ValueC, ValueH, ValueG, ValueD, ValueE, ValueF];
        indices         = [IndexA, IndexJ, IndexB, IndexI, IndexC, IndexH, IndexG, IndexD, IndexE, IndexF];
        expectedNums    = [ValueA, ValueB, ValueC, ValueD, ValueE, ValueF, ValueG, ValueH, ValueI, ValueJ];
        expectedIndices = [IndexA, IndexB, IndexC, IndexD, IndexE, IndexF, IndexG, IndexH, IndexI, IndexJ];    
        translatedNums = new uint[nums.Length];
        tempNums = new uint[translatedNums.Length];
        tempIndices = new int[indices.Length];
        count = new int[256];

        RadixSortF.IndexedDescend(nums, translatedNums, tempNums, indices, tempIndices, count);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }

        // == slice ==
        nums            = [ValueA, ValueJ, ValueB, ValueI, ValueC, ValueH, ValueG, ValueD, ValueE, ValueF];
        indices         = [IndexA, IndexJ, IndexB, IndexI, IndexC, IndexH, IndexG, IndexD, IndexE, IndexF];
        expectedNums    = [ValueA, ValueJ, ValueB, ValueC, ValueD, ValueG, ValueH, ValueI, ValueE, ValueF]; 
        expectedIndices = [IndexA, IndexJ, IndexB, IndexC, IndexD, IndexG, IndexH, IndexI, IndexE, IndexF];    
        translatedNums = new uint[nums.Length];
        tempNums = new uint[translatedNums.Length];
        tempIndices = new int[indices.Length];
        count = new int[256];
        int start = 2;
        int length = nums.Length - 4;

        
        RadixSortF.IndexedDescend(nums, translatedNums, tempNums, indices, tempIndices, count, start, length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }
    }

    [Fact]
    public void BufferedIndexedDescend_Test()
    {
        float[] nums;
        float[] expectedNums;
        int[] indices;
        int[] expectedIndices;
        RadixSortBuffer buffer;


        // == full ==

        nums            = [ValueA, ValueJ, ValueB, ValueI, ValueC, ValueH, ValueG, ValueD, ValueE, ValueF];
        indices         = [IndexA, IndexJ, IndexB, IndexI, IndexC, IndexH, IndexG, IndexD, IndexE, IndexF];
        expectedNums    = [ValueA, ValueB, ValueC, ValueD, ValueE, ValueF, ValueG, ValueH, ValueI, ValueJ];
        expectedIndices = [IndexA, IndexB, IndexC, IndexD, IndexE, IndexF, IndexG, IndexH, IndexI, IndexJ];    
        buffer = new(nums.Length);

        RadixSortF.IndexedDescend(nums, indices, buffer);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }

        // == slice ==
        nums            = [ValueA, ValueJ, ValueB, ValueI, ValueC, ValueH, ValueG, ValueD, ValueE, ValueF];
        indices         = [IndexA, IndexJ, IndexB, IndexI, IndexC, IndexH, IndexG, IndexD, IndexE, IndexF];
        expectedNums    = [ValueA, ValueJ, ValueB, ValueC, ValueD, ValueG, ValueH, ValueI, ValueE, ValueF]; 
        expectedIndices = [IndexA, IndexJ, IndexB, IndexC, IndexD, IndexG, IndexH, IndexI, IndexE, IndexF];    
        buffer = new(nums.Length);
        int start = 2;
        int length = nums.Length - 4;

        
        RadixSortF.IndexedDescend(nums, indices, buffer, start, length);

        for(int q = 0; q < nums.Length; q++)
        {
            Assert.Equal(expectedNums[q], nums[q]);
            Assert.Equal(expectedIndices[q], indices[q]);
        }
    }
}