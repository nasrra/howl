namespace Howl.Test.Simd;

public static class VectorFAssert
{
    /// <summary>
    /// Asserts the equality of an expected tail index of a floating-point SIMD operation.
    /// </summary>
    /// <param name="collectionLength">The length of the collection; E.g. the length of an array.</param>
    /// <param name="expectedTailIndex">the expected tail index of the simd operation.</param>
    public static void TailIndexEqual(int collectionLength, int expectedTailIndex)
    {
        Assert.Equal(expectedTailIndex, System.Numerics.Vector<float>.Count * (collectionLength / System.Numerics.Vector<float>.Count));
    }
}