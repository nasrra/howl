using System.Runtime.CompilerServices;

namespace Howl.Collections;

public static class FixedStrideStackArray
{
    /// <summary>
    ///     Pushes a value onto an entry in a array instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">the value to push.</param>
    /// <param name="values">the array to push onto.</param>
    /// <param name="counts">the counts of each entry in the array.</param>
    /// <param name="stride">the stride of each entry in the array.</param>
    /// <param name="entryIndex">the index of the entry to push onto.</param>
    /// <returns>true, if the value was successfully pushed; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Push<T>(T value, T[] values, int[] counts, int stride, int entryIndex)
    {
        ref int count = ref counts[entryIndex];
        int next = count+1; 
        if(next > stride)
        {
            count--;
            return false;
        }

        values[entryIndex * stride + count] = value;
        count = next;
        return true;
    }

    /// <summary>
    ///     Gets the last pushed value of the entry in the array instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">the array to pop off of.</param>
    /// <param name="counts">the counts of each entry in the array.</param>
    /// <param name="stride">the stride of each entry in the array.</param>
    /// <param name="entryIndex">the index of the entry to pop the data off of.</param>
    /// <param name="isValid">output for if the returned value is valid.</param>
    /// <returns>the last pushed value of the entry in the array instance.</returns>
    public static T Pop<T>(T[] values, int[] counts, int stride, int entryIndex, ref bool isValid)
    {
        ref int count = ref counts[entryIndex];
        count--;

        if(count < 0)
        {
            count++;
            isValid = false;
            return default;
        }

        return values[entryIndex * stride + count];
    }

    /// <summary>
    ///     Sets the count values of an array instance to zero.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="counts">the instance to clear.</param>
    public static void Clear<T>(int[] counts)
    {
        for(int i = 0; i < counts.Length; i++)
        {
            counts[i] = 0;
        }
    }
}