using Howl.Collections;

namespace Howl.Test.Collections;

public class Test_SwapBackArray
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 3; length++)
        {
            SwapBackArray<float> array = new(length);
            Assert.Equal(length, array.Data.Length);
            Assert.False(array.Disposed);
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int length = 0; length < 12; length++)
        {
            SwapBackArray<float> nums = new(length);

            for(int i = 0; i < length; i++)
            {
                SwapBackArray.Append(nums, i);
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
            SwapBackArray<float> nums = new(length);

            for(int i = 0; i < length; i++)
            {
                SwapBackArray.Append(nums, i);
            }
            
            Assert.Equal(length, nums.Count);
            SwapBackArray.ClearCount(nums);            
            Assert.Equal(0, nums.Count);
        }
    }

    [Fact]
    public void RemoveAt_Test()
    {
        for(int length = 0; length < 12; length++)
        {
            SwapBackArray<float> nums = new(length);

            // populate.
            for(int i = 0; i < length; i++)
            {
                SwapBackArray.Append(nums, i);
            }

            // remove checks.
            int j = 0;
            for(int i = length - 1; i > 0; i--)
            {
                j++;
                float last = nums[nums.Count-1];
                int removeIndex = j / length;
                SwapBackArray.RemoveAt(nums, removeIndex);
                Assert.Equal(last, nums[removeIndex]);
            }
        }
    }

    [Fact]
    public void AsSpan_Test()
    {
        for(int length = 0; length < 12; length++)
        {
            SwapBackArray<float> nums = new(length);

            // populate.
            for(int i = 0; i < length; i++)
            {
                SwapBackArray.Append(nums, i);
            }

            Span<float> span = SwapBackArray.AsSpan(nums);

            // assert entries.
            for(int i = 0; i < length; i++)
            {
                Assert.Equal(i, span[i]);
            }
        }
    }

    [Fact]
    public void Slice_Test()
    {
        for(int length = 0; length < 12; length++)
        {
            SwapBackArray<float> nums = new(length);

            // populate.
            for(int i = 0; i < length; i++)
            {
                SwapBackArray.Append(nums, i);
            }

            Span<float> span = SwapBackArray.Slice(nums, 0, length / 2);

            // assert entries.
            Assert.Equal(length / 2, span.Length);
            for(int i = 0; i < length / 2; i++)
            {
                Assert.Equal(i, span[i]);                
            }
        }
    }

    [Fact]
    public void Dispose_Test()
    {
        for(int length = 0; length < 3; length++)
        {
            SwapBackArray<float> nums = new(length);

            for(int i = 0; i < length; i++)
            {
                SwapBackArray.Append(nums, i);
            }

            Assert.Equal(length, nums.Count);

            SwapBackArray.Dispose(nums);

            Assert.Equal(0, nums.Count);
            Assert.True(nums.Disposed);
        }
    }
}