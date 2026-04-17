using Howl.Collections;

namespace Howl.Test.Collections;

public static class Assert_DopeVector
{
    /// <summary>
    ///     Asserts the lengths of all backing arrays in array instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entryLength">the expected length of entries.</param>
    /// <param name="entryDataLength">the expected length of data an entry can store.</param>
    /// <param name="array">the array instance to assert.</param>
    public static void LengthEqual<T>(int entryDataLength, int entryLength, DopeVector<T> array)
    {
        Assert.Equal(entryLength, array.EntryStride);
        Assert.Equal(entryDataLength, array.EntryDataLength);
        Assert.Equal(entryLength * entryDataLength, array.Data.Length);
        Assert.Equal(entryLength, array.AppendCounts.Length);
    }

    /// <summary>
    ///     Asserts the equality of a entry in an array instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">the expected data value.</param>
    /// <param name="appendCount">the expected <c>AppendCount</c> value.</param>
    /// <param name="entryDataLength">the length of data an entry an have.</param>
    /// <param name="entryIndex">the entry index to assert against.</param>
    /// <param name="array">the array instance that contains the entry.</param>
    public static void EntryEqual<T>(T data, int appendCount, int entryDataLength, int entryIndex, DopeVector<T> array)
    {
        int index = entryIndex * entryDataLength;
        
        Assert.Equal(appendCount, array.AppendCounts[entryIndex]);
        
        // adjust by one as the array is zero indexed.
        index += array.AppendCounts[entryIndex]-1;
        
        Assert.Equal(data, array.Data[index]);
    }
}