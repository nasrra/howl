namespace Howl.Test.Collections;

public static class StackArray_Assert
{
    /// <summary>
    /// Asserts the equality of array lengths in a StackArray instance. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="array">the stack array instance.</param>
    public static void LengthEqual<T>(int length, StackArray<T> array)
    {
        Assert.Equal(length, array.Data.Length);
    }
}