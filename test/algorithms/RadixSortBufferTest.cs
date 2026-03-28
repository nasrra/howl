using Howl.Algorithms.Sorting;

namespace Howl.Test.Algorithms.Sorting;

public class RadixSortBufferTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 12; length++)
        {
            RadixSortBuffer buffer = new(length);
            RadixSortBufferAssert.LengthEqual(length, buffer);
            buffer.Disposed = false;
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        for(int length = 0; length < 12; length++)
        {
            RadixSortBuffer buffer = new(length);
            RadixSortBuffer.Dispose(buffer);
            Assert.Null(buffer.TempValues);
            Assert.Null(buffer.TranslatedValues);
            Assert.Null(buffer.ByteCount);
            Assert.Null(buffer.TempIndices);
            Assert.True(buffer.Disposed);
        }
    }
}