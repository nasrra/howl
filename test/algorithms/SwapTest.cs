using Howl.Algorithms;

namespace Howl.Test.Algorithms;

public class SwapTest
{
    [Fact]
    public void PermuteTo_Test()
    {

        float[] nums;
        int[] indices;
        float[] swapped;
        float[] expected;

        // == full ==. 

        nums        = [1,2,3,4,5,6,7,8,9,10];
        indices     = [0,9,1,8,2,7,3,6,4,5];
        swapped     = new float[nums.Length];
        expected    = [1,10,2,9,3,8,4,7,5,6];

        Swap.PermuteTo<float>(nums, indices, swapped);
    
        for(int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], swapped[i]);
        } 

        // == slice ==.

        nums        = [1,2,3,4,5,6,7,8,9,10];
        indices     = [0,9,1,8,2,7,3,6,4,5];
        swapped     = new float[nums.Length];
        expected    = [1,2,2,9,3,8,4,7,9,10];

        Swap.PermuteTo<float>(nums, indices, swapped, 2, nums.Length - 4);
    
        for(int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], swapped[i]);
        } 
    }

    [Fact]
    public void PermuteInPlace_Test()
    {
        float[] nums;
        int[] indices;
        float[] temp;
        float[] expected;

        // == full ==. 

        nums        = [1,2,3,4,5,6,7,8,9,10];
        indices     = [0,9,1,8,2,7,3,6,4,5];
        temp        = new float[nums.Length];
        expected    = [1,10,2,9,3,8,4,7,5,6];

        Swap.PermuteInPlace<float>(nums, indices, temp);
    
        for(int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], nums[i]);
        } 

        // == slice ==.

        nums        = [1,2,3,4,5,6,7,8,9,10];
        indices     = [0,9,1,8,2,7,3,6,4,5];
        temp        = new float[nums.Length];
        expected    = [1,2,2,9,3,8,4,7,9,10];

        Swap.PermuteInPlace<float>(nums, indices, temp, 2, nums.Length - 4);
    
        for(int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], nums[i]);
        }        
    }
}