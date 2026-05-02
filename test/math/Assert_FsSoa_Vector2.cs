using Howl.Collections;
using Howl.Math;

namespace Howl.Test.Math;

public static class Assert_FsSoa_Vector2
{
    /// <summary>
    ///     Asserts the length of the backing arrays of a soa instance.
    /// </summary>
    /// <param name="stride">the expected stride.</param>
    /// <param name="maxEntries">the expected max entries.</param>
    /// <param name="soa">the soa instance to assert against.</param>
    public static void LengthEqual(int stride, int maxEntries, FsSoa_Vector2 soa)
    {
        int dataLength = stride * maxEntries;
        Assert.Equal(stride, soa.Stride);
        Assert.Equal(maxEntries, soa.MaxEntries);
        Assert.Equal(dataLength, soa.X.Length);
        Assert.Equal(dataLength, soa.Y.Length);
        Assert.Equal(maxEntries, soa.AppendCounts.Length);
    }

    /// <summary>
    ///     Asserts the equality of elements in the backing arrays of an soa instance.
    /// </summary>
    /// <param name="x">the epxected x-value.</param>
    /// <param name="y">the expected y-value.</param>
    /// <param name="index">the element index.</param>
    /// <param name="soa">the soa instance containing the elements.</param>
    public static void ElementEqual(float x, float y, int entryElementIndex, int entryIndex, FsSoa_Vector2 soa)
    {
        int index = FixedStrideArray.GetElementIndex(entryIndex, soa.Stride, entryElementIndex);
        Assert.Equal(x, soa.X[index]);
        Assert.Equal(y, soa.Y[index]);
    }

    /// <summary>
    ///     Asserts the equality of entries in the backing arrays of a soa instance.
    /// </summary>
    /// <param name="x">the expected x-values.</param>
    /// <param name="y">the expected y-values.</param>
    /// <param name="entryIndex">the entry index.</param>
    /// <param name="soa">the soa instance containing the entry.</param>
    public static void EntryEqual(Span<float> x, Span<float> y, int entryIndex, FsSoa_Vector2 soa)
    {
        int appendCount = soa.AppendCounts[entryIndex];

        System.Diagnostics.Debug.Assert(x.Length <= appendCount);
        System.Diagnostics.Debug.Assert(y.Length <= appendCount);
        
        for(int i = 0; i < appendCount; i++)
        {
            int elementIndex = FixedStrideArray.GetElementIndex(entryIndex, soa.Stride, i);
            Assert.Equal(x[i], soa.X[elementIndex]);
            Assert.Equal(y[i], soa.Y[elementIndex]);
        }
    }

    /// <summary>
    ///     Asserts that a soa instance is disposed.
    /// </summary>
    /// <param name="soa">the soa instance to assert against.</param>
    public static void Disposed(FsSoa_Vector2 soa)
    {
        Assert.True(soa.Disposed);
        Assert.Null(soa.X);
        Assert.Null(soa.Y);
        Assert.Null(soa.AppendCounts);
        Assert.Equal(0, soa.Stride);
        Assert.Equal(0, soa.MaxEntries);
    }
}