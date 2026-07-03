using System.Runtime.CompilerServices;

namespace Howl.Algorithms.Sorting;

/******************

    NOTE:
    the procedures of in this file will need to eventually swap away
    from using spans in favour of custom arrays.

*******************/

/// <summary>
/// The 'under-the-hood' sorting algorithms for radix sort.
/// </summary>
public static class RadixSort
{




    /*******************
    
        Ascending Sort.
    
    ********************/




    /// <summary>
    /// Sorts a System.Span of uints in ascending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 32-bit integers in four 8-bit (1 byte) passes .
    /// The following System.Spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="values"/></item>
    /// <item><paramref name="tempValues"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="values">the System.Span of uints to be sorted. Contains the final sorted values.</param>
    /// <param name="tempValues">temporary System.Span for reordering values during each pass.</param>
    /// <param name="byteCount">a histogram System.Span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Ascend(System.Span<uint> values, System.Span<uint> tempValues, System.Span<int> byteCount, int length)
    {
        // Use pointers or references to swap which buffer is "source" and "destination"
        System.Span<uint> src = values;
        System.Span<uint> dst = tempValues;

        // perform the radix sort on the units (LSD approach)
        // Use 8-bit chunks (buckets of 256) for efficiency.
        for (int shift = 0; shift < 32; shift += 8)
        {
            // reset the frequency of counts for this 8-bit chunk.
            byteCount.Clear();

            // count the occurences of each 8-value (0-255).
            for (int i = 0; i < length; i++)
            {
                // Shift the target 8-bit chunk (byte) to the far right of the 32-bit integer.
                //  'shift' moves in increments of 8 (0, 8, 16, 24) to isolate each byte in the uint.
                // Apply a bit mask of 0xFF (binary 11111111) to zero out everythin except
                //  those bottom 8 bits.
                // This results in a 'bucket' index between 0 and 255, matching our count array.                
                byteCount[(int)((src[i] >> shift) & 0xFF)]++;
            }

            // compute prefix sum (cumulative count) - ascending order.
            // this tells us exactly which index each bucket starts at in the temp array.
            int startIdx = 0;
            for (int i = 0; i < 256; i++)
            {
                int c = byteCount[i];
                byteCount[i] = startIdx;
                startIdx += c;
            }

            // Shuffle from src to dst
            for (int i = 0; i < length; i++)
            {
                int bucket = (int)((src[i] >> shift) & 0xFF);
                dst[byteCount[bucket]++] = src[i];
            }

            // TOGGLE: Swap src and dst for the next pass
            // Pass 0: val -> tmp
            // Pass 1: tmp -> val
            // Pass 2: val -> tmp
            // Pass 3: tmp -> val
            // Because we swapped 4 times (a 32-bit uint is an even number), the final result 
            // is already back in the 'values' System.Span! No CopyTo needed.
            var swap = src;
            src = dst;
            dst = swap;
        }
    }

    /// <summary>
    /// Sorts a System.Span of uints in ascending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 32-bit integers in four 8-bit (1 byte) passes .
    /// The following System.Spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="tempIndices"/></item>
    /// <item><paramref name="values"/></item>
    /// <item><paramref name="tempValues"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="values">the System.Span of uints to be sorted. Contains the final sorted values.</param>
    /// <param name="tempValues">temporary System.Span for reordering values during each pass.</param>
    /// <param name="indices">the associated index System.Span to be reordered alongside the values.</param>
    /// <param name="tempIndices">temporary System.Span for reordering indices during each pass.</param>
    /// <param name="byteCount">a histogram System.Span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    public static void IndexedAscend(System.Span<uint> values, System.Span<uint> tempValues, System.Span<int> indices, System.Span<int> tempIndices, System.Span<int> byteCount, int length)
    {
        // Use pointers or references to swap which buffer is "source" and "destination"
        System.Span<uint> srcValues = values;
        System.Span<uint> dstValues = tempValues;
        System.Span<int> srcIndices = indices;
        System.Span<int> dstIndices = tempIndices;

        // perform the radix sort on the units (LSD approach)
        // Use 8-bit chunks (buckets of 256) for efficiency.
        for (int shift = 0; shift < 32; shift += 8)
        {
            // reset the frequency of counts for this 8-bit chunk.
            byteCount.Clear();

            // count the occurences of each 8-value (0-255).
            for (int i = 0; i < length; i++)
            {
                // Shift the target 8-bit chunk (byte) to the far right of the 32-bit integer.
                //  'shift' moves in increments of 8 (0, 8, 16, 24) to isolate each byte in the uint.
                // Apply a bit mask of 0xFF (binary 11111111) to zero out everythin except
                //  those bottom 8 bits.
                // This results in a 'bucket' index between 0 and 255, matching our count array.                
                byteCount[(int)((srcValues[i] >> shift) & 0xFF)]++;
            }

            // compute prefix sum (cumulative count) - ascending order.
            // this tells us exactly which index each bucket starts at in the temp array.
            int startIdx = 0;
            for (int i = 0; i < 256; i++)
            {
                int c = byteCount[i];
                byteCount[i] = startIdx;
                startIdx += c;
            }

            // Shuffle from src to dst
            for (int i = 0; i < length; i++)
            {
                int bucket = (int)((srcValues[i] >> shift) & 0xFF);
                int swapIndex = byteCount[bucket]++; 
                dstValues[swapIndex] = srcValues[i];
                dstIndices[swapIndex] = srcIndices[i];
            }

            // TOGGLE: Swap src and dst for the next pass
            // Pass 0: val -> tmp
            // Pass 1: tmp -> val
            // Pass 2: val -> tmp
            // Pass 3: tmp -> val
            // Because we swapped 4 times (a 32-bit uint is an even number), the final result 
            // is already back in the 'values' System.Span! No CopyTo needed.
            var valueSwap = srcValues;
            srcValues = dstValues;
            dstValues = valueSwap;
            var indicesSwap = srcIndices;
            srcIndices = dstIndices;
            dstIndices = indicesSwap;
        }
    }

    /// <summary>
    /// Sorts a System.Span of uints in ascending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 8-bit chunks (bytes) per 'step', requiring 4 'steps' for a 32-bit integer.
    /// The following System.Spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="tempIndices"/></item>
    /// <item><paramref name="values"/></item>
    /// <item><paramref name="tempValues"/></item> 
    /// <item><paramref name="translated"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. Contains the final sorted values.</param>
    /// <param name="translated">a System.Span to contain the floating-point to uint converted values for sorting.</param>
    /// <param name="tempValues">temporary System.Span for reordering values during each pass.</param>
    /// <param name="indices">the associated index System.Span to be reordered alongside the values.</param>
    /// <param name="tempIndices">temporary System.Span for reordering indices during each pass.</param>
    /// <param name="byteCount">a histogram System.Span, must be at least 256 elements long.</param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="length">the total number of elements after '<paramref name="start"/>' to process.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void IndexedAscend(System.Span<uint> values, System.Span<uint> tempValues, System.Span<int> indices, System.Span<int> tempIndices, System.Span<int> byteCount, 
        int start, int length
    )
    {
        System.Span<uint> valuesSlice = values.Slice(start, length);
        System.Span<uint> tempValuesSlice = tempValues.Slice(start, length);
        System.Span<int> indicesSlice = indices.Slice(start, length);
        System.Span<int> tempIndicesSlice = tempIndices.Slice(start, length);
        IndexedAscend(valuesSlice, tempValuesSlice, indicesSlice, tempIndicesSlice, byteCount, length);
    }

    public static void IndexedAscend(System.Span<uint> values, System.Span<int> indices, ref RadixSortBuffer buffer, int start, int length
    )
    {
        IndexedAscend(values, 
            N_Howl.N_Collections.Collections.AsSpan(buffer.TempValues), indices, 
            N_Howl.N_Collections.Collections.AsSpan(buffer.TempIndices), 
            N_Howl.N_Collections.Collections.AsSpan(buffer.ByteCount), start, length
        );
    }
    




    /*******************
    
        Descending Sort.
    
    ********************/




    /// <summary>
    /// Sorts a System.Span of uints in descending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 32-bit integers in four 8-bit (1 byte) passes .
    /// The following System.Spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="values"/></item>
    /// <item><paramref name="temp"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="values">the System.Span of uints to be sorted. Contains the final sorted values.</param>
    /// <param name="temp">temporary System.Span for reordering values during each pass.</param>
    /// <param name="byteCount">a histogram System.Span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Descend(System.Span<uint> values, System.Span<uint> temp, System.Span<int> byteCount, int length)
    {
        // Use pointers or references to swap which buffer is "source" and "destination"
        System.Span<uint> src = values;
        System.Span<uint> dst = temp;

        // perform the radix sort on the units (LSD approach)
        // Use 8-bit chunks (buckets of 256) for efficiency.
        for (int shift = 0; shift < 32; shift += 8)
        {
            // reset the frequency of counts for this 8-bit chunk.
            byteCount.Clear();

            // count the occurences of each 8-value (0-255).
            for (int i = 0; i < length; i++)
            {
                // Shift the target 8-bit chunk (byte) to the far right of the 32-bit integer.
                //  'shift' moves in increments of 8 (0, 8, 16, 24) to isolate each byte in the uint.
                // Apply a bit mask of 0xFF (binary 11111111) to zero out everythin except
                //  those bottom 8 bits.
                // This results in a 'bucket' index between 0 and 255, matching our count array.                
                byteCount[(int)((src[i] >> shift) & 0xFF)]++;
            }

            // compute prefix sum (cumulative count) - descending order.
            // this tells us exactly which index each bucket starts at in the temp array.
            int startIdx = 0;
            for (int i = 255; i >= 0; i--)
            {
                int c = byteCount[i];
                byteCount[i] = startIdx;
                startIdx += c;
            }

            // Shuffle from src to dst
            for (int i = 0; i < length; i++)
            {
                int bucket = (int)((src[i] >> shift) & 0xFF);
                dst[byteCount[bucket]++] = src[i];
            }

            // TOGGLE: Swap src and dst for the next pass
            // Pass 0: val -> tmp
            // Pass 1: tmp -> val
            // Pass 2: val -> tmp
            // Pass 3: tmp -> val
            // Because we swapped 4 times (a 32-bit uint is an even number), the final result 
            // is already back in the 'values' System.Span! No CopyTo needed.
            var swap = src;
            src = dst;
            dst = swap;
        }
    }

    /// <summary>
    /// Sorts a System.Span of uints in descending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 32-bit integers in four 8-bit (1 byte) passes .
    /// The following System.Spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="tempIndices"/></item>
    /// <item><paramref name="values"/></item>
    /// <item><paramref name="tempValues"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="values">the System.Span of uints to be sorted. Contains the final sorted values.</param>
    /// <param name="tempValues">temporary System.Span for reordering values during each pass.</param>
    /// <param name="indices">the associated index System.Span to be reordered alongside the values.</param>
    /// <param name="tempIndices">temporary System.Span for reordering indices during each pass.</param>
    /// <param name="byteCount">a histogram System.Span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    public static void IndexedDescend(System.Span<uint> values, System.Span<uint> tempValues, System.Span<int> indices, System.Span<int> tempIndices, System.Span<int> byteCount, int length)
    {
        // Use pointers or references to swap which buffer is "source" and "destination"
        System.Span<uint> srcValues = values;
        System.Span<uint> dstValues = tempValues;
        System.Span<int> srcIndices = indices;
        System.Span<int> dstIndices = tempIndices;

        // perform the radix sort on the units (LSD approach)
        // Use 8-bit chunks (buckets of 256) for efficiency.
        for (int shift = 0; shift < 32; shift += 8)
        {
            // reset the frequency of counts for this 8-bit chunk.
            byteCount.Clear();

            // count the occurences of each 8-value (0-255).
            for (int i = 0; i < length; i++)
            {
                // Shift the target 8-bit chunk (byte) to the far right of the 32-bit integer.
                //  'shift' moves in increments of 8 (0, 8, 16, 24) to isolate each byte in the uint.
                // Apply a bit mask of 0xFF (binary 11111111) to zero out everythin except
                //  those bottom 8 bits.
                // This results in a 'bucket' index between 0 and 255, matching our count array.                
                byteCount[(int)((srcValues[i] >> shift) & 0xFF)]++;
            }

            // compute prefix sum (cumulative count) - ascending order.
            // this tells us exactly which index each bucket starts at in the temp array.
            int startIdx = 0;
            for (int i = 255; i >= 0; i--)
            {
                int c = byteCount[i];
                byteCount[i] = startIdx;
                startIdx += c;
            }

            // Shuffle from src to dst
            for (int i = 0; i < length; i++)
            {
                int bucket = (int)((srcValues[i] >> shift) & 0xFF);
                int swapIndex = byteCount[bucket]++; 
                dstValues[swapIndex] = srcValues[i];
                dstIndices[swapIndex] = srcIndices[i];
            }

            // TOGGLE: Swap src and dst for the next pass
            // Pass 0: val -> tmp
            // Pass 1: tmp -> val
            // Pass 2: val -> tmp
            // Pass 3: tmp -> val
            // Because we swapped 4 times (a 32-bit uint is an even number), the final result 
            // is already back in the 'values' System.Span! No CopyTo needed.
            var valueSwap = srcValues;
            srcValues = dstValues;
            dstValues = valueSwap;
            var indicesSwap = srcIndices;
            srcIndices = dstIndices;
            dstIndices = indicesSwap;
        }
    }
}