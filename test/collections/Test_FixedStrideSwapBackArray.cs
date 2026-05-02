namespace Howl.Test.Collections;

public class Test_FixedStrideSwapBackArray
{
    [Fact]
    public void Test()
    {
        int[] values;
        int[] counts;
        int[] expectedValues;
        int[] expectedCounts;

        int stride = 3;
        int maxEntries = 3;
        int length = stride * maxEntries;

        values = new int[length];
        counts = new int[maxEntries];
    
        // == append test. ==

        int j = 0;
        for(int entryIndex = 0; entryIndex < maxEntries; entryIndex++)
        {
            for(int elementIndex = 0; elementIndex < stride; elementIndex++)
            {
                j++;
                Assert.True(FixedStrideSwapBackArray.Append(j, values, counts, stride, entryIndex));
            }
        }

        expectedValues = [1,2,3,4,5,6,7,8,9];
        expectedCounts = [3,3,3];

        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedCounts, counts);

        // == fail cases as the array is full ==
        
        Assert.False(FixedStrideSwapBackArray.Append(100, values, counts, stride, 0));
        Assert.False(FixedStrideSwapBackArray.Append(100, values, counts, stride, 1));
        Assert.False(FixedStrideSwapBackArray.Append(100, values, counts, stride, 2));

        // == remove test 1 ==.
        expectedValues = [3,2,3,4,5,6,7,9,9];
        expectedCounts = [2,3,2];

        Assert.True(FixedStrideSwapBackArray.RemoveAt(values, counts, stride, 0, 0));
        Assert.True(FixedStrideSwapBackArray.RemoveAt(values, counts, stride, 2, 1));

        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedCounts, counts);

        // == remove test 2 ==.

        Assert.True(FixedStrideSwapBackArray.RemoveAt(values, counts, stride, 2, 1));

        expectedValues = [3,2,3,4,5,6,7,9,9];
        expectedCounts = [2,3,1];

        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedCounts, counts);

        // == remove test 3 ==.

        Assert.True(FixedStrideSwapBackArray.RemoveAt(values, counts, stride, 2, 0));
    
        expectedValues = [3,2,3,4,5,6,7,9,9];
        expectedCounts = [2,3,0];

        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedCounts, counts);

        // fail remove as array is empty.

        Assert.False(FixedStrideSwapBackArray.RemoveAt(values, counts, stride, 2, 0));
        Assert.False(FixedStrideSwapBackArray.RemoveAt(values, counts, stride, 2, 1));
        Assert.False(FixedStrideSwapBackArray.RemoveAt(values, counts, stride, 2, 2));
    }
}