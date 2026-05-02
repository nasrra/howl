using System.Runtime.CompilerServices;

namespace Howl.Collections;

public static class FixedStrideArray
{
    /// <summary>
    ///     Gets the element index of an entry's element in a fixed stride array. 
    /// </summary>
    /// <param name="entryIndex">the index of the entry in the array.</param>
    /// <param name="stride">the stride of each entry in the array.</param>
    /// <param name="entryElementIndex">the index of the element in the entry.</param>
    /// <returns>the index in the fixed stride array to the entry's element.</returns>
    public static int GetElementIndex(int entryIndex, int stride, int entryElementIndex)
    {
        return entryIndex * stride + entryElementIndex;
    }

    /// <summary>
    ///     Gets the element index for a value to be appeneded to in a fixed stride array instance.
    /// </summary>
    /// <remarks>
    ///     Remarks: it is assumed that the data array that the element index is calculated for is a one dimensional fixed stride array.
    /// </remarks>
    /// <param name="appendCounts">the count of appended data for all entries in the fixed stride array.</param>
    /// <param name="entryIndex">the desired entry index to append to.</param>
    /// <param name="stride">the stride of each entry in the one-dimensional data array.</param>
    /// <param name="isValid">output, for whether or not the returned append index is valid.</param>
    /// <returns>the index in the one-dimensional data array to append to.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetAppendIndex(int[] appendCounts, int entryIndex, int stride, ref bool isValid)
    {
        // check if a value can be appended to the entry..
        int appendCount = appendCounts[entryIndex];
        if(appendCount >= stride)
        {
            // the entry is full.
            isValid = false;
            return 0;
        }

        // return the element index in the one dimensional data array that the value should be appended to.
        isValid = true;
        return GetElementIndex(entryIndex, stride, appendCount);
    }
}