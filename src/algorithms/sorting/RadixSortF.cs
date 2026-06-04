using System.Runtime.CompilerServices;
using Howl.Unmanaged.Collections;

namespace Howl.Algorithms.Sorting;

/******************

    NOTE:
    All the functions in the class should eventually shift away
    from using System.System.Spans and instead use custom Arrays.

*******************/

/// <summary>
/// Radix sorting porocedures for floating-point numbers.
/// </summary>
public static class RadixSortF
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
    public static uint ToSortableUint(float value)
    {
        // In our sortable format:
        // - Values starting with '0' (0 to 0x7FFFFFFF) are transformed NEGATIVES.
        // - Values starting with '1' (0x80000000 to 0xFFFFFFFF) are transformed POSITIVES.

        // Get the raw IEEE 754 bits of the float.
        // Format: [1-bit Sign] [8-bit Exponent] [23-bit Mantissa]
        // -    if 1.2345 is the Mantissa, with 3 as the exponent, the exponent tells us that the decimal is moved 3 places to the right
        //      giving the result of 1234.5, which is what the floating point number is conveying.
        uint bits = Unsafe.As<float, uint>(ref value);
        
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
    /// Converts a System.Span of floating point numbers to sortable uint representations.
    /// </summary>
    /// <remarks>
    /// The '<paramref name="input"/>' and '<paramref name="output"/>' System.Spans must be of equal length.
    /// </remarks>
    /// <param name="input">the input floating-point values to convert into their sortable uint representations.</param>
    /// <param name="output">output for the newly converted sortable uint values.</param>
    public static void ToSortableUint_Sisd(System.Span<float> input, System.Span<uint> output, int start)
    {
        for(int i = start; i < input.Length; i++)
        {
            output[i] = ToSortableUint(input[i]);
        }
    }

    public static void ToSortableUint_Simd(System.Span<float> input, System.Span<uint> output, ref int tailIndex)
    {
        int simdSize = System.Numerics.Vector<float>.Count;

        int i = 0;
        for (; i <= input.Length - simdSize; i += simdSize)
        {            
            // Load the data as ints to enable Arithmetic Shifting
            System.Numerics.Vector<int> bits = System.Numerics.Vector.LoadUnsafe(ref Unsafe.As<float, int>(ref input[i]));

            System.Numerics.Vector<uint> signMask = new System.Numerics.Vector<uint>(0x80000000);

            // Generate the mask: 
            // Negative numbers become 0xFFFFFFFF (-1), Positives become 0x00000000 (0)
            System.Numerics.Vector<int> mask = System.Numerics.Vector.ShiftRightArithmetic(bits, 31);

            // REINTERPRET: Note the capital 'U' and 'I' in UInt32
            System.Numerics.Vector<uint> uBits = System.Numerics.Vector.AsVectorUInt32(bits);
            System.Numerics.Vector<uint> uMask = System.Numerics.Vector.AsVectorUInt32(mask);

            // Transform Logic: 
            // If Neg: bits ^ (0xFFFFFFFF | 0x80000000) => bits ^ 0xFFFFFFFF => ~bits
            // If Pos: bits ^ (0x00000000 | 0x80000000) => bits ^ 0x80000000 => flip sign bit
            System.Numerics.Vector<uint> transformer = System.Numerics.Vector.BitwiseOr(uMask, signMask);
            System.Numerics.Vector<uint> result = System.Numerics.Vector.Xor(uBits, transformer);

            // Store the result
            System.Numerics.Vector.StoreUnsafe(result, ref output[i]);
        }
        tailIndex = i;
    }

    public static void ToSortableUint(System.Span<float> input, System.Span<uint> output)
    {
        int tailIndex = 0;
        ToSortableUint_Simd(input, output, ref tailIndex);
        ToSortableUint_Sisd(input, output, tailIndex);
    }

    /// <summary>
    /// Converts a sorted uint-converted floating point number back into its original float point representation.
    /// </summary>
    /// <param name="value">the uint value to convert into a floating point number.</param>
    /// <returns>the underlying floating point number.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float FromSortableUint(uint value)
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
        return Unsafe.As<uint, float>(ref value);
    }

    /// <summary>
    /// Converts a System.Span of sortable uints back into their floating-point representations.
    /// </summary>
    /// <remarks>
    /// The '<paramref name="input"/>' and '<paramref name="output"/>' System.Spans must be of equal length.
    /// </remarks>
    /// <param name="input">the sortable uints to convert into their floating-point representations.</param>
    /// <param name="output">output for the newly converted floating-point values.</param>
    public static void FromSortableUint(System.Span<uint> input, System.Span<float> output)
    {
        for(int i = 0; i < input.Length; i++)
        {
            output[i] = FromSortableUint(input[i]);
        }
    }




    /*******************
    
        Ascending Sorting.
    
    ********************/





    /// <summary>
    /// Sorts a System.Span of floating point values in ascending order using Radix Sort.
    /// </summary>
    /// <remarks>
    /// System.Spans must be of equal length:
    /// <list type="bullet">
    /// <item><paramref name="values"/></item>
    /// <item><paramref name="translated"/></item>
    /// <item><paramref name="temp"/></item>
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. this System.Span will contain the sorted result.</param>
    /// <param name="translated">a System.Span to contain the floating-point to uint converted values for sorting.</param>
    /// <param name="temp">temporary System.Span for reordering values during each pass.</param>
    /// <param name="byteCount">a histogram System.Span, must be at least 256 elements long.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Ascend(System.Span<float> values, System.Span<uint> translated, System.Span<uint> temp, System.Span<int> byteCount)
    {
        ToSortableUint(values, translated);
        RadixSort.Ascend(translated, temp, byteCount, translated.Length);
        FromSortableUint(translated, values);
    }

    /// <summary>
    /// Sorts a System.Span of floating point values in ascending order using Radix Sort.
    /// </summary>
    /// <remarks>
    /// System.Spans must be of equal length:
    /// <list type="bullet">
    /// <item><paramref name="values"/></item>
    /// <item><paramref name="translated"/></item>
    /// <item><paramref name="temp"/></item>
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. this System.Span will contain the sorted result.</param>
    /// <param name="translated">a System.Span to contain the floating-point to uint converted values for sorting.</param>
    /// <param name="temp">temporary System.Span for reordering values during each pass.</param>
    /// <param name="byteCount">a histogram System.Span, must be at least 256 elements long.</param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="length">the total number of elements after '<paramref name="start"/>' to process.</param>
    public static void Ascend(System.Span<float> values, System.Span<uint> translated, System.Span<uint> temp, System.Span<int> byteCount, int start, int length)
    {
        System.Span<float> valuesSlice = values.Slice(start, length);
        System.Span<uint> transSlice = translated.Slice(start, length);
        System.Span<uint> tempSlice = temp.Slice(start, length);        
        Ascend(valuesSlice, transSlice, tempSlice, byteCount);
    }
    
    /// <summary>
    /// Sorts a System.Span of floating point values in ascending order using Radix Sort.
    /// </summary>
    /// <remarks>
    /// The length of the '<paramref name="buffer"/>' and '<paramref name="values"/>' System.Span should be the same.
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. this System.Span will contain the sorted result.</param>
    /// <param name="buffer">A radix sort buffer </param>
    public static void Ascend(System.Span<float> values, ref RadixSortBuffer buffer)
    {
        Ascend(values, Array.AsSpan(buffer.TranslatedValues), Array.AsSpan(buffer.TempValues), Array.AsSpan(buffer.ByteCount));
    }

    /// <summary>
    /// Sorts a System.Span of floating point values in ascending order using Radix Sort.
    /// </summary>
    /// <remarks>
    /// The length of the '<paramref name="buffer"/>' and '<paramref name="values"/>' System.Span should be the same.
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. this System.Span will contain the sorted result.</param>
    /// <param name="buffer">A radix sort buffer </param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="length">the total number of elements after '<paramref name="start"/>' to process.</param>
    public static void Ascend(System.Span<float> values, ref RadixSortBuffer buffer, int start, int length)
    {
        Ascend(values, Array.AsSpan(buffer.TranslatedValues), Array.AsSpan(buffer.TempValues), Array.AsSpan(buffer.ByteCount), start, length);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void IndexedAscend(System.Span<float> values, System.Span<uint> translated, System.Span<uint> tempValues, 
        System.Span<int> indices, System.Span<int> tempIndices, System.Span<int> byteCount
    )
    {
        ToSortableUint(values, translated);
        RadixSort.IndexedAscend(translated, tempValues, indices, tempIndices, byteCount, translated.Length);
        FromSortableUint(translated, values);
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
    public static void IndexedAscend(System.Span<float> values, System.Span<uint> translated, System.Span<uint> tempValues, 
        System.Span<int> indices, System.Span<int> tempIndices, System.Span<int> byteCount, int start, int length
    )
    {
        System.Span<float> valuesSlice = values.Slice(start, length);
        System.Span<uint> transSlice = translated.Slice(start, length);
        System.Span<uint> tempValuesSlice = tempValues.Slice(start, length);
        System.Span<int> indicesSlice = indices.Slice(start, length);
        System.Span<int> tempIndicesSlice = tempIndices.Slice(start, length);
        IndexedAscend(valuesSlice, transSlice, tempValuesSlice, indicesSlice, tempIndicesSlice, byteCount);
    }

    /// <summary>
    /// Sorts a System.Span of uints in ascending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 8-bit chunks (bytes) per 'step', requiring 4 'steps' for a 32-bit integer.
    /// The following System.Spans and buffers must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="indices"/></item> 
    /// <item><paramref name="buffer"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. Contains the final sorted values.</param>
    /// <param name="indices">the associated index System.Span to be reordered alongside the values.</param>
    /// <param name="buffer">A radix sorting buffer for all temporary arrays reused during sorting.</param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="length">the total number of elements after '<paramref name="start"/>' to process.</param>
    public static void IndexedAscend(System.Span<float> values, System.Span<int> indices, ref RadixSortBuffer buffer, int start, int length)
    {
        IndexedAscend(values, Array.AsSpan(buffer.TranslatedValues), Array.AsSpan(buffer.TempValues), 
            indices, Array.AsSpan(buffer.TempIndices), Array.AsSpan(buffer.ByteCount), start, length
        );
    }

    /// <summary>
    /// Sorts a System.Span of uints in ascending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 8-bit chunks (bytes) per 'step', requiring 4 'steps' for a 32-bit integer.
    /// The following System.Spans and buffers must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="indices"/></item> 
    /// <item><paramref name="buffer"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. Contains the final sorted values.</param>
    /// <param name="indices">the associated index System.Span to be reordered alongside the values.</param>
    /// <param name="buffer">A radix sorting buffer for all temporary arrays reused during sorting.</param>
    public static void IndexedAscend(System.Span<float> values, System.Span<int> indices, ref RadixSortBuffer buffer)
    {
        IndexedAscend(values, Array.AsSpan(buffer.TranslatedValues), Array.AsSpan(buffer.TempValues), 
            indices, Array.AsSpan(buffer.TempIndices), Array.AsSpan(buffer.ByteCount)
        );
    }




    /*******************
    
        Descending Sorting.
    
    ********************/




    /// <summary>
    /// Sorts a System.Span of floating point values in descending order using Radix Sort.
    /// </summary>
    /// <remarks>
    /// System.Spans must be of equal length:
    /// <list type="bullet">
    /// <item><paramref name="values"/></item>
    /// <item><paramref name="translated"/></item>
    /// <item><paramref name="temp"/></item>
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. this System.Span will contain the sorted result.</param>
    /// <param name="translated">a System.Span to contain the floating-point to uint converted values for sorting.</param>
    /// <param name="temp">temporary System.Span for reordering values during each pass.</param>
    /// <param name="byteCount">a histogram System.Span, must be at least 256 elements long.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Descend(System.Span<float> values, System.Span<uint> translated, System.Span<uint> temp, System.Span<int> byteCount)
    {
        ToSortableUint(values, translated);
        RadixSort.Descend(translated, temp, byteCount, translated.Length);
        FromSortableUint(translated, values);
    }

    /// <summary>
    /// Sorts a System.Span of floating point values in descending order using Radix Sort.
    /// </summary>
    /// <remarks>
    /// System.Spans must be of equal length:
    /// <list type="bullet">
    /// <item><paramref name="values"/></item>
    /// <item><paramref name="translated"/></item>
    /// <item><paramref name="temp"/></item>
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. this System.Span will contain the sorted result.</param>
    /// <param name="translated">a System.Span to contain the floating-point to uint converted values for sorting.</param>
    /// <param name="temp">temporary System.Span for reordering values during each pass.</param>
    /// <param name="byteCount">a histogram System.Span, must be at least 256 elements long.</param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="length">the total number of elements after '<paramref name="start"/>' to process.</param>
    public static void Descend(System.Span<float> values, System.Span<uint> translated, System.Span<uint> temp, System.Span<int> byteCount, int start, int length)
    {
        System.Span<float> valuesSlice = values.Slice(start, length);
        System.Span<uint> transSlice = translated.Slice(start, length);
        System.Span<uint> tempSlice = temp.Slice(start, length);        
        Descend(valuesSlice, transSlice, tempSlice, byteCount);
    }

    /// <summary>
    /// Sorts a System.Span of floating point values in descending order using Radix Sort.
    /// </summary>
    /// <remarks>
    /// The length of the '<paramref name="buffer"/>' and '<paramref name="values"/>' System.Span should be the same.
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. this System.Span will contain the sorted result.</param>
    /// <param name="buffer">A radix sort buffer </param>
    public static void Descend(System.Span<float> values, ref RadixSortBuffer buffer)
    {
        Descend(values, Array.AsSpan(buffer.TranslatedValues), Array.AsSpan(buffer.TempValues), Array.AsSpan(buffer.ByteCount));
    }

    /// <summary>
    /// Sorts a System.Span of floating point values in descending order using Radix Sort.
    /// </summary>
    /// <remarks>
    /// The length of the '<paramref name="buffer"/>' and '<paramref name="values"/>' System.Span should be the same.
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. this System.Span will contain the sorted result.</param>
    /// <param name="buffer">A radix sort buffer </param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="length">the total number of elements after '<paramref name="start"/>' to process.</param>
    public static void Descend(System.Span<float> values, ref RadixSortBuffer buffer, int start, int length)
    {
        Descend(values, Array.AsSpan(buffer.TranslatedValues), Array.AsSpan(buffer.TempValues), Array.AsSpan(buffer.ByteCount), start, length);
    }

    /// <summary>
    /// Sorts a System.Span of uints in descending order using the Radix Sort Algorithm.
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
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void IndexedDescend(System.Span<float> values, System.Span<uint> translated, System.Span<uint> tempValues, 
        System.Span<int> indices, System.Span<int> tempIndices, System.Span<int> byteCount
    )
    {
        ToSortableUint(values, translated);
        RadixSort.IndexedDescend(translated, tempValues, indices, tempIndices, byteCount, translated.Length);
        FromSortableUint(translated, values);
    }

    /// <summary>
    /// Sorts a System.Span of uints in descending order using the Radix Sort Algorithm.
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
    public static void IndexedDescend(System.Span<float> values, System.Span<uint> translated, System.Span<uint> tempValues, 
        System.Span<int> indices, System.Span<int> tempIndices, System.Span<int> byteCount, int start, int length
    )
    {
        System.Span<float> valuesSlice = values.Slice(start, length);
        System.Span<uint> transSlice = translated.Slice(start, length);
        System.Span<uint> tempValuesSlice = tempValues.Slice(start, length);        
        System.Span<int> indicesSlice = indices.Slice(start, length);
        System.Span<int> tempIndicesSlice = tempIndices.Slice(start, length);
        IndexedDescend(valuesSlice, transSlice, tempValuesSlice, indicesSlice, tempIndicesSlice, byteCount);
    }

    /// <summary>
    /// Sorts a System.Span of uints in descending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 8-bit chunks (bytes) per 'step', requiring 4 'steps' for a 32-bit integer.
    /// The following System.Spans and buffers must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="indices"/></item> 
    /// <item><paramref name="buffer"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. Contains the final sorted values.</param>
    /// <param name="indices">the associated index System.Span to be reordered alongside the values.</param>
    /// <param name="buffer">A radix sorting buffer for all temporary arrays reused during sorting.</param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="length">the total number of elements after '<paramref name="start"/>' to process.</param>
    public static void IndexedDescend(System.Span<float> values, System.Span<int> indices, ref RadixSortBuffer buffer, int start, int length)
    {
        IndexedDescend(values, Array.AsSpan(buffer.TranslatedValues), Array.AsSpan(buffer.TempValues), 
            indices, Array.AsSpan(buffer.TempIndices), Array.AsSpan(buffer.ByteCount), start, length
        );
    }

    /// <summary>
    /// Sorts a System.Span of uints in descending order using the Radix Sort Algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation processes 8-bit chunks (bytes) per 'step', requiring 4 'steps' for a 32-bit integer.
    /// The following System.Spans and buffers must have a length at least equal to <paramref name="length"/>:
    /// <list type="bullet">
    /// <item><paramref name="indices"/></item>
    /// <item><paramref name="indices"/></item> 
    /// <item><paramref name="buffer"/></item> 
    /// </list>
    /// </remarks>
    /// <param name="values">the System.Span of floats to be sorted. Contains the final sorted values.</param>
    /// <param name="indices">the associated index System.Span to be reordered alongside the values.</param>
    /// <param name="buffer">A radix sorting buffer for all temporary arrays reused during sorting.</param>
    public static void IndexedDescend(System.Span<float> values, System.Span<int> indices, ref RadixSortBuffer buffer)
    {
        IndexedDescend(values, Array.AsSpan(buffer.TranslatedValues), Array.AsSpan(buffer.TempValues), 
            indices, Array.AsSpan(buffer.TempIndices), Array.AsSpan(buffer.ByteCount)
        );
    }
}