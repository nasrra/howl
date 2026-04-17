using Howl.Collections;

namespace Howl.Test.Collections;

public static class Assert_SwapBackArray
{
    /// <summary>
    ///     Asserts the eqaulity of array lengths in a SwapBackArray instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="array">the swapback array instance.</param>
    public static void LengthEqual<T>(int length, SwapBackArray<T> array)
    {
        Assert.Equal(length, array.Data.Length);
        Assert.Equal(length, array.Length);
    }

    /// <summary>
    ///     Asserts that a swapback array instance is disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">the array instance.</param>
    public static void Disposed<T>(this SwapBackArray<T> array)
    {
        Assert.Null(array.Data);
        Assert.True(array.Count == 0);
        Assert.True(array.Length == 0);
        Assert.True(array.Disposed);
    }
}