using Howl.Collections;

namespace Howl.Test.Collections;

public class Test_StackArray
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 3; length++)
        {
            StackArray<float> array = new(length);
            Assert_StackArray.LengthEqual(length, array);
            Assert.False(array.Disposed);
        }
    }

    [Fact]
    public void Psuh_Test()
    {
        for(int length = 0; length < 12; length++)
        {
            StackArray<float> nums = new(length);

            for(int i = 0; i < length; i++)
            {
                StackArray.Push(nums, i);
                Assert.Equal(i+1, nums.Count);
            }

            Assert.Equal(length, nums.Count);

            for(int i = 0; i < nums.Count; i++)
            {
                Assert.Equal(i, nums[i]);
            }
        }
    }

    [Fact]
    public void ClearCount_Test()
    {
        for(int length = 0; length < 12; length++)
        {
            StackArray<float> nums = new(length);

            for(int i = 0; i < length; i++)
            {
                StackArray.Push(nums, i);
            }
            
            Assert.Equal(length, nums.Count);
            StackArray.ClearCount(nums);            
            Assert.Equal(0, nums.Count);
        }
    }

    [Fact]
    public void Pop_Test()
    {
        for(int length = 0; length < 12; length++)
        {
            StackArray<float> nums = new(length);

            // populate.
            for(int i = 0; i < length; i++)
            {
                StackArray.Push(nums, i);
            }

            // remove checks.
            for(int i = length - 1; i > 0; i--)
            {
                Assert.Equal(i, StackArray.Pop(nums));
            }
        }
    }

    [Fact]
    public void Dispose_Test()
    {
        for(int length = 0; length < 3; length++)
        {
            StackArray<float> nums = new(length);

            for(int i = 0; i < length; i++)
            {
                StackArray.Push(nums, i);
            }

            Assert.Equal(length, nums.Count);

            StackArray.Dispose(nums);

            Assert.Equal(0, nums.Count);

            Assert_StackArray.Disposed(nums);
        }
    }
}