namespace Howl.Test.Collections;

public static class Assert_StackArray
{
    /// <summary>
    ///     Asserts the equality of array lengths in a StackArray instance. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="array">the stack array instance.</param>
    public static void LengthEqual<T>(int length, StackArray<T> array)
    {
        Assert.Equal(length, array.Data.Length);
        Assert.Equal(length, array.Length);
    }

    /// <summary>
    ///     Asserts that a stack array instance is disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">the array instance.</param>
    public static void Disposed<T>(this StackArray<T> array)
    {
        Assert.Null(array.Data);
        Assert.True(array.Count == 0);
        Assert.True(array.Length == 0);
        Assert.True(array.Disposed);
    }
}