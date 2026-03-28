using System;
using System.Runtime.CompilerServices;

namespace Howl.Algorithms.Sorting;

public static class RadixSort
{




    /*******************
    
        Bit Conversions.
    
    ********************/




    /// <summary>
    /// Converts a floatint point number into a sortable uint representation.
    /// </summary>
    /// <param name="value">the floating-point number to convert into a sortable uint..</param>
    /// <returns>the sortable uint representation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static uint FloatToUintSortable(float value)
    {
        // In our sortable format:
        // - Values starting with '0' (0 to 0x7FFFFFFF) are transformed NEGATIVES.
        // - Values starting with '1' (0x80000000 to 0xFFFFFFFF) are transformed POSITIVES.

        // Get the raw IEEE 754 bits of the float.
        // Format: [1-bit Sign] [8-bit Exponent] [23-bit Mantissa]
        // -    if 1.2345 is the Mantissa, with 3 as the exponent, the exponent tells us that the decimal is moved 3 places to the right
        //      giving the result of 1234.5, which is what the floating point number is conveying.
        uint bits = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);

        if((bits & 0x80000000) != 0)
        {
            // For NEGATIVE numbers:
            // IEEE 754 bits increase as the value gets "more negative" (e.g., -2.0 has higher bits than -1.0).
            // We flip ALL bits to reverse this order and place them at the very bottom of the uint range.
            return ~bits;
        }
        else
        {
            // For POSITIVE numbers:
            // The bits already increase correctly, but the sign bit is 0.
            // We flip ONLY the sign bit (setting it to 1) to "offset" all positives
            // so they appear larger than any transformed negative number.
            return bits | 0x80000000;
        }
    }

    /// <summary>
    /// Converts a sorted uint-converted floating point number back into its original float point representation.
    /// </summary>
    /// <param name="value">the uint value to convert into a floating point number.</param>
    /// <returns>the underlying floating point number.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float UintSortableToFloat(uint value)
    {
        // In our sortable format:
        // - Values starting with '0' (0 to 0x7FFFFFFF) are transformed NEGATIVES.
        // - Values starting with '1' (0x80000000 to 0xFFFFFFFF) are transformed POSITIVES.

        // reverse the transformation
        if((value & 0x80000000) == 0)
        {
            // if the highest bit is 0, this was originally a NEGATIVE number.
            // During FloatToUinty, we flipped all bits to fix the order.
            // We now flip all bits back to restore the original IEEE 754 negative representation.
            value = ~value;
        }
        else
        {
            // If the highest bit is 1, this was originally a POSITIVE number.
            // During FloatToUint, we only flipped the sign bit to 1 to mov it above negatives.
            // We now flip that sign bit back to 0 (using a bitwise AND with the inverse of the sign mask).
            value = value & ~0x80000000;
        }

        //convert the corrected bit pattern back into a 32-bit float.
        return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
    }




    /*******************
    
        Floating Point Sorting.
    
    ********************/




    /// <summary>
    /// Sorts a span of floating point numbers in ascending order using Radix sort.
    /// </summary>
    /// <param name="numbers">the span of floats to be sorted. this span will contain the sorted result.</param>
    /// <param name="translated">a span to contain the floating-point to uint converted numbers for sorting.</param>
    /// <param name="temp">temporary span for reordering numbers during each pass.</param>
    /// <param name="count">a histogram span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    public static void Ascend(Span<float> numbers, Span<uint> translated, Span<uint> temp, Span<int> count, int length)
    {
        // convert float bits to uints that are able to be ordered in ascending/descending order.
        for(int i = 0; i < length; i++)
        {
            translated[i] = FloatToUintSortable(numbers[i]);
        }

        Ascend(translated, temp, count, length);

        // finally, convert the sorted uints back into the original float span
        for(int i = 0; i < length; i++)
        {
            numbers[i] = UintSortableToFloat(translated[i]);
        }
    }

    /// <summary>
    /// Sorts a span of floating point numbers in descending order using Radix sort.
    /// </summary>
    /// <param name="numbers">the span of floats to be sorted. this span will contain the sorted result.</param>
    /// <param name="translated">a span to contain the floating-point to uint converted numbers for sorting.</param>
    /// <param name="temp">temporary span for reordering numbers during each pass.</param>
    /// <param name="count">a histogram span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    public static void Descend(Span<float> numbers, Span<uint> translated, Span<uint> temp, Span<int> count, int length)
    {
        // convert float bits to uints that are able to be ordered in ascending/descending order.
        for(int i = 0; i < length; i++)
        {
            translated[i] = FloatToUintSortable(numbers[i]);
        }

        Descend(translated, temp, count, length);

        // finally, convert the sorted uints back into the original float span
        for(int i = 0; i < length; i++)
        {
            numbers[i] = UintSortableToFloat(translated[i]);
        }
    }

    /// <summary>
    /// Sorts a span of uints in ascending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 8-bit chunks (bytes) per 'step', requiring 4 'steps' for a 32-bit integer.
    /// The following spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="tempIndices"/></item>
    /// <item><paramref name="numbers"/></item>
    /// <item><paramref name="tempNumbers"/></item> 
    /// <item><paramref name="translatedNumbers"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="numbers">the span of floats to be sorted. Contains the final sorted values.</param>
    /// <param name="translated">a span to contain the floating-point to uint converted numbers for sorting.</param>
    /// <param name="tempNumbers">temporary span for reordering numbers during each pass.</param>
    /// <param name="indices">the associated index span to be reordered alongside the numbers.</param>
    /// <param name="tempIndices">temporary span for reordering indices during each pass.</param>
    /// <param name="count">a histogram span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void IndexedAscend(Span<float> numbers, Span<uint> translatedNumbers, Span<uint> tempNumbers, 
        Span<int> indices, Span<int> tempIndices, Span<int> count, int length
    )
    {
        // convert float bits to uints that are able to be ordered in ascending/descending order.
        for(int i = 0; i < length; i++)
        {
            translatedNumbers[i] = FloatToUintSortable(numbers[i]);
        }

        IndexedAscend(translatedNumbers, tempNumbers, indices, tempIndices, count, length);

        // finally, convert the sorted uints into the original flat span.
        for(int i = 0; i < length; i++)
        {
            numbers[i] = UintSortableToFloat(translatedNumbers[i]);
        }
    }

    /// <summary>
    /// Sorts a span of uints in descending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 8-bit chunks (bytes) per 'step', requiring 4 'steps' for a 32-bit integer.
    /// The following spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="tempIndices"/></item>
    /// <item><paramref name="numbers"/></item>
    /// <item><paramref name="tempNumbers"/></item> 
    /// <item><paramref name="translatedNumbers"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="numbers">the span of floats to be sorted. Contains the final sorted values.</param>
    /// <param name="translated">a span to contain the floating-point to uint converted numbers for sorting.</param>
    /// <param name="tempNumbers">temporary span for reordering numbers during each pass.</param>
    /// <param name="indices">the associated index span to be reordered alongside the numbers.</param>
    /// <param name="tempIndices">temporary span for reordering indices during each pass.</param>
    /// <param name="count">a histogram span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void IndexedDescend(Span<float> numbers, Span<uint> translatedNumbers, Span<uint> tempNumbers, 
        Span<int> indices, Span<int> tempIndices, Span<int> count, int length
    )
    {
        // convert float bits to uints that are able to be ordered in ascending/descending order.
        for(int i = 0; i < length; i++)
        {
            translatedNumbers[i] = FloatToUintSortable(numbers[i]);
        }

        IndexedDescend(translatedNumbers, tempNumbers, indices, tempIndices, count, length);

        // finally, convert the sorted uints into the original flat span.
        for(int i = 0; i < length; i++)
        {
            numbers[i] = UintSortableToFloat(translatedNumbers[i]);
        }
    }




    /*******************
    
        Radix Sorting.
    
    ********************/




    /// <summary>
    /// Sorts a span of uints in ascending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 32-bit integers in four 8-bit (1 byte) passes .
    /// The following spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="numbers"/></item>
    /// <item><paramref name="temp"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="numbers">the span of uints to be sorted. Contains the final sorted values.</param>
    /// <param name="temp">temporary span for reordering numbers during each pass.</param>
    /// <param name="count">a histogram span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Ascend(Span<uint> numbers, Span<uint> temp, Span<int> count, int length)
    {        
        // perform the radix sort on the units (LSD approach)
        // Use 8-bit chunks (buckets of 256) for efficiency.
        for(int shift = 0; shift < 32; shift += 8)
        {
            // reset the frequency of counts for this 8-bit chunk.
            count.Clear();

            // count the occurences of each 8-value (0-255).
            for(int i = 0 ; i < length; i++)
            {
                // Shift the target 8-bit chunk (byte) to the far right of the 32-bit integer.
                //  'shift' moves in increments of 8 (0, 8, 16, 24) to isolate each byte in the uint.
                // Apply a bit mask of 0xFF (binary 11111111) to zero out everythin except
                //  those bottom 8 bits.
                // This results in a 'bucket' index between 0 and 255, matching our count array.
                int bucket = (int)((numbers[i] >> shift) & 0xFF);
                count[bucket]++;
            }

            // compute prefix sum (cumulative count) - ascending order.
            // this tells us exactly which index each bucket starts at in the temp array.
            int startIndex = 0;
            int c = 0;
            for(int i = 0; i < 256; i++)
            {
                c = count[i];
                count[i] = startIndex;
                startIndex += c;
            }

            // move data from buffer to temp based on the counts.
            for(int i = 0; i < length; i++)
            {
                int bucket = (int)((numbers[i] >> shift) & 0xFF);
                temp[count[bucket]++] = numbers[i];
            }

            // swap buffer and temp for the next pass.
            temp.CopyTo(numbers);
        }
    }

    /// <summary>
    /// Sorts a span of uints in descending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 32-bit integers in four 8-bit (1 byte) passes .
    /// The following spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="numbers"/></item>
    /// <item><paramref name="temp"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="numbers">the span of uints to be sorted. Contains the final sorted values.</param>
    /// <param name="temp">temporary span for reordering numbers during each pass.</param>
    /// <param name="count">a histogram span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Descend(Span<uint> numbers, Span<uint> temp, Span<int> count, int length)
    {        
        // perform the radix sort on the units (LSD approach)
        // Use 8-bit chunks (buckets of 256) for efficiency.
        for(int shift = 0; shift < 32; shift += 8)
        {
            // reset the frequency of counts for this 8-bit chunk.
            count.Clear();

            // count the occurences of each 8-value (0-255).
            for(int i = 0 ; i < length; i++)
            {
                // Shift the target 8-bit chunk (byte) to the far right of the 32-bit integer.
                //  'shift' moves in increments of 8 (0, 8, 16, 24) to isolate each byte in the uint.
                // Apply a bit mask of 0xFF (binary 11111111) to zero out everythin except
                //  those bottom 8 bits.
                // This results in a 'bucket' index between 0 and 255, matching our count array.
                int bucket = (int)((numbers[i] >> shift) & 0xFF);
                count[bucket]++;
            }

            // compute prefix sum (cumulative count) - descending order
            // this tells us exactly which index each bucket starts at in the temp array.
            int startIndex = 0;
            int c = 0;
            for(int i = 255; i >= 0; i--)
            {
                c = count[i];
                count[i] = startIndex;
                startIndex += c;
            }

            // move data from buffer to temp based on the counts.
            for(int i = 0; i < length; i++)
            {
                int bucket = (int)((numbers[i] >> shift) & 0xFF);
                temp[count[bucket]++] = numbers[i];
            }

            // swap buffer and temp for the next pass.
            temp.CopyTo(numbers);
        }
    }

    /// <summary>
    /// Sorts a span of uints in ascending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 32-bit integers in four 8-bit (1 byte) passes .
    /// The following spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="tempIndices"/></item>
    /// <item><paramref name="numbers"/></item>
    /// <item><paramref name="tempNumbers"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="numbers">the span of uints to be sorted. Contains the final sorted values.</param>
    /// <param name="tempNumbers">temporary span for reordering numbers during each pass.</param>
    /// <param name="indices">the associated index span to be reordered alongside the numbers.</param>
    /// <param name="tempIndices">temporary span for reordering indices during each pass.</param>
    /// <param name="count">a histogram span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    public static void IndexedAscend(Span<uint> numbers, Span<uint> tempNumbers, Span<int> indices, Span<int> tempIndices, Span<int> count, int length)
    {        
        // perform the radix sort on the units (LSD approach)
        // Use 8-bit chunks (buckets of 256) for efficiency.
        for(int shift = 0; shift < 32; shift += 8)
        {
            // reset the frequency of counts for this 8-bit chunk.
            count.Clear();

            // count the occurences of each 8-value (0-255).
            for(int i = 0 ; i < length; i++)
            {
                // Shift the target 8-bit chunk (byte) to the far right of the 32-bit integer.
                //  'shift' moves in increments of 8 (0, 8, 16, 24) to isolate each byte in the uint.
                // Apply a bit mask of 0xFF (binary 11111111) to zero out everythin except
                //  those bottom 8 bits.
                // This results in a 'bucket' index between 0 and 255, matching our count array.
                int bucket = (int)((numbers[i] >> shift) & 0xFF);
                count[bucket]++;
            }

            // compute prefix sum (cumulative count)
            // this tells us exactly which index each bucket starts at in the temp array.
            int startIndex = 0;
            int c = 0;
            for(int i = 0; i < 256; i++)
            {
                c = count[i];
                count[i] = startIndex;
                startIndex += c;
            }

            // move data from buffer to temp based on the counts.
            for(int i = 0; i < length; i++)
            {
                int bucket = (int)((numbers[i] >> shift) & 0xFF);
                int swapIndex = count[bucket]++; 
                tempNumbers[swapIndex] = numbers[i];
                tempIndices[swapIndex] = indices[i];
            }

            // swap buffer and temp for the next pass.
            tempNumbers.CopyTo(numbers);
            tempIndices.CopyTo(indices);
        }
    }

    /// <summary>
    /// Sorts a span of uints in descending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 32-bit integers in four 8-bit (1 byte) passes .
    /// The following spans must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="tempIndices"/></item>
    /// <item><paramref name="numbers"/></item>
    /// <item><paramref name="tempNumbers"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="numbers">the span of uints to be sorted. Contains the final sorted values.</param>
    /// <param name="tempNumbers">temporary span for reordering numbers during each pass.</param>
    /// <param name="indices">the associated index span to be reordered alongside the numbers.</param>
    /// <param name="tempIndices">temporary span for reordering indices during each pass.</param>
    /// <param name="count">a histogram span, must be at least 256 elements long.</param>
    /// <param name="length">the total number of elements to process.</param>
    public static void IndexedDescend(Span<uint> numbers, Span<uint> tempNumbers, Span<int> indices, Span<int> tempIndices, Span<int> count, int length)
    {        
        // perform the radix sort on the units (LSD approach)
        // Use 8-bit chunks (buckets of 256) for efficiency.
        for(int shift = 0; shift < 32; shift += 8)
        {
            // reset the frequency of counts for this 8-bit chunk.
            count.Clear();

            // count the occurences of each 8-value (0-255).
            for(int i = 0 ; i < length; i++)
            {
                // Shift the target 8-bit chunk (byte) to the far right of the 32-bit integer.
                //  'shift' moves in increments of 8 (0, 8, 16, 24) to isolate each byte in the uint.
                // Apply a bit mask of 0xFF (binary 11111111) to zero out everythin except
                //  those bottom 8 bits.
                // This results in a 'bucket' index between 0 and 255, matching our count array.
                int bucket = (int)((numbers[i] >> shift) & 0xFF);
                count[bucket]++;
            }

            // compute prefix sum (cumulative count) - descending order.
            // this tells us exactly which index each bucket starts at in the temp array.
            int startIndex = 0;
            int c = 0;
            for(int i = 255; i >= 0; i--)
            {
                c = count[i];
                count[i] = startIndex;
                startIndex += c;
            }

            // move data from buffer to temp based on the counts.
            for(int i = 0; i < length; i++)
            {
                int bucket = (int)((numbers[i] >> shift) & 0xFF);
                int swapIndex = count[bucket]++; 
                tempNumbers[swapIndex] = numbers[i];
                tempIndices[swapIndex] = indices[i];
            }

            // swap buffer and temp for the next pass.
            tempNumbers.CopyTo(numbers);
            tempIndices.CopyTo(indices);
        }
    }
}