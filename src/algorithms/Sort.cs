using System;
using System.Runtime.CompilerServices;

namespace Howl.Algorithms;

public static class Sort
{
    public static void RadixSort(Span<float> numbers, Span<uint> buffer, Span<uint> temp, Span<int> count, int length)
    {
    }


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
}