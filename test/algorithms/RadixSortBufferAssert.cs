using Howl.Algorithms.Sorting;

public static class RadixSortBufferAssert
{
    /// <summary>
    /// Asserts the equality of array lengths in a buffer instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="buffer">the buffer instance.</param>
    public static void LengthEqual(int expected, RadixSortBuffer buffer)
    {
        Assert.Equal(256, buffer.ByteCount.Length);
        Assert.Equal(expected, buffer.TranslatedValues.Length);
        Assert.Equal(expected, buffer.TempValues.Length);
        Assert.Equal(expected, buffer.TempIndices.Length);
    }
}