using System;
using System.Runtime.CompilerServices;

namespace Howl.Algorithms;

public static class Sort
{
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

    public static void Radix(Span<float> numbers, Span<uint> buffer, Span<uint> temp, Span<int> count, int length)
    {
        // convert float bits to uints that are able to be ordered in ascending/descending order.
        for(int i = 0; i < length; i++)
        {
            buffer[i] = FloatToUintSortable(numbers[i]);
        }

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
                int bucket = (int)((buffer[i] >> shift) & 0xFF);
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
                int bucket = (int)((buffer[i] >> shift) & 0xFF);
                temp[count[bucket]++] = buffer[i];
            }

            // swap buffer and temp for the next pass.
            temp.CopyTo(buffer);
        }

        // finally, convert the sorted uints back into the original float span
        for(int i = 0; i < length; i++)
        {
            numbers[i] = UintSortableToFloat(buffer[i]);
        }
    }
}