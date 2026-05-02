using System.Runtime.CompilerServices;
using Howl.Collections;

public static class FixedStrideSwapBackArray
{
    /// <summary>
    ///     Appends a value to a destination array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">the value to append to the array.</param>
    /// <param name="destination">the destination to write to.</param>
    /// <param name="counts">the count of valid elements of each entry in the destination array.</param>
    /// <param name="stride">the element stride of each entry.</param>
    /// <param name="entryIndex">the index of the entry to append to.</param>
    /// <returns>true, if the value was successfully appended; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Append<T>(this T value, T[] destination, int[] counts, int stride, int entryIndex)
    {
        ref int count = ref counts[entryIndex];
        int next = count + 1;;
        if(next > stride)
        {
            return false;
        }

        destination[FixedStrideArray.GetElementIndex(entryIndex, stride, count)] = value;
        count = next;
        return true;
    }

    /// <summary>
    ///     Removes an element at a specified index from an array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">the array to remove from.</param>
    /// <param name="counts">the count of valid elements of each entry in the <c><paramref name="values"/></c> array.</param>
    /// <param name="stride">the element stride of each entry.</param>
    /// <param name="entryIndex">the index of the entry to remove from.</param>
    /// <param name="elementIndex">the index - relative to the entry - of the element to remove.</param>
    /// <returns>true, if the value was successfully removed; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool RemoveAt<T>(this T[] values, int[] counts, int stride, int entryIndex, int elementIndex)
    {
        ref int count = ref counts[entryIndex];
        
        if(count == 0 || count < elementIndex)
        {
            return false;
        }
        
        count--;
        
        // set the data to remove with the last entry.
        values[FixedStrideArray.GetElementIndex(entryIndex, stride, elementIndex)] = values[FixedStrideArray.GetElementIndex(entryIndex, stride, count)];
        
        return true;
    }

    /// <summary>
    ///     Sets all elements to zero.
    /// </summary>
    /// <param name="counts">the count array to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearCounts(int[] counts)
    {
        for(int i = 0; i < counts.Length; i++)
        {
            counts[i] = 0;
        }
    }
}