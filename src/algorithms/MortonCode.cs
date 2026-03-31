using System.Runtime.CompilerServices;

namespace Howl.Algorithms;

public static class MortonCode
{
    /// <summary>
    /// Spreads 16 bits into 32 bits by inserting a 0 between each bit.
    /// </summary>
    /// <remarks>
    /// Passing a <paramref name="value"/> that is above 65535 will result in overflow and undefined behaviour; there are no checks in this function.
    /// </remarks>
    /// <param name="value">the value to expand</param>
    /// <returns>the value with expanded bits.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static uint ExpandBits(uint value)
    {
        // ----------------------------------------------
        // Example:
        // ----------------------------------------------
        // value    = 00000000 00000000 00000000 01010101
        // expanded = 00000000 00000000 00010001 00010001
        // ----------------------------------------------

        value = (value | (value << 8)) & 0x00FF00FF;
        value = (value | (value << 4)) & 0x0F0F0F0F;
        value = (value | (value << 2)) & 0x33333333;
        value = (value | (value << 1)) & 0x55555555;
        return value;
    }  

    /// <summary>
    /// Calculates the normalization factors required to map a pair of 32-bit values (x,y) 
    /// into the 16-bit integer range [0, 65535] for Morton encoding.
    /// </summary>
    /// <remarks>
    /// This calculates the scale values used to transform a floating-point value
    /// into a discrete 65536 x 65536 grid. This ensures that the bit-interleaving 
    /// process utilizes the maximum precision available in a 32-bit Morton code.
    /// </remarks>
    /// <param name="rangeX">The range of the highest to lowest value in the 'x' dataset/collection.</param>
    /// <param name="rangeY">The range of the highest to lowest value in the 'y' dataset/collection.</param>
    /// <param name="scaleX">output for the calculated x-scaling factor for calculating a morton code.</param>
    /// <param name="scaleY">output for the calculated y-scaling factor for calcualting a morton code.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CalculateScaleFactor(float rangeX, float rangeY, ref float scaleX, ref float scaleY)
    {
        // calculate the range.
        // float width = maxX - minX; 
        // float height = maxY - minY;

        // calculate the scales to fit in a 16 bit-range (2^16 - 1 = 65535)
        // this is done as a morton code is two 16-bit numbers interleaved together to form a 32 bit number.
        scaleX = rangeX > 0 ? 65535.0f / rangeX : 0;
        scaleY = rangeY > 0 ? 65535.0f / rangeY : 0; 
    }

    /// <summary>
    /// Interleaves the bits of a pair of 32-bit values (x,y) into a 1-dimensional 32-bit Morton code.
    /// </summary>
    /// <remarks>
    /// <c><paramref name="x"/></c> should never be lower than <c><paramref name="minX"/></c>.
    /// <c><paramref name="y"/></c> should never be lower than <c><paramref name="minY"/></c>
    /// </remarks>
    /// <param name="x">the x-value of the 32-bit pair.</param>
    /// <param name="y">the y-value of the 32-bit pair.</param>
    /// <param name="minX">the minimum x-value in the pair's dataset/collection.</param>
    /// <param name="minY">the minimum x-value in the pair's dataset/collection.</param>
    /// <param name="scaleX">the x-component of the morton code scaling factor.</param>
    /// <param name="scaleY">the y-component of the morton code scaling factor.</param>
    /// <returns></returns>
    public static uint CalculateMortonCode(float x, float y, float minX, float minY, float scaleX, float scaleY)
    {
        // normalise coordinates to [0, 65535] (16 bit range.)
        // this is done as a morton code is two 16-bit numbers interleaved together to form a 32 bit number.
        uint ux = (uint)((x - minX) * scaleX);
        uint uy = (uint)((y - minY) * scaleY);

        // shift 'y' coordinate bit values to the left; so that the 'x' coordinate bit values dont overwrite it.
        // this creates the morton code by interleaving the two 16 bit numbers.
        //
        // ------------------------------------------------------------------
        // Example:
        // ------------------------------------------------------------------
        // ux                           = 00000000 00000000 00000000 01010101
        // uy                           = 00000000 00000000 00000000 01010101
        // ------------------------------------------------------------------
        // expanded ux                  = 00000000 00000000 00010001 00010001
        // expanded uy                  = 00000000 00000000 00010001 00010001
        // ------------------------------------------------------------------
        // expanded uy << 1             = 00000000 00000000 00100010 00100010
        // ------------------------------------------------------------------
        // (expanded uy << 1) | exp ux  = 00000000 00000000 00110010 00110011
        // ------------------------------------------------------------------
        return ExpandBits(uy) << 1 | ExpandBits(ux);
    }  
}
