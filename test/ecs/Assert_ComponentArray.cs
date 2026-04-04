namespace Howl.Test.ECS;

public static class Assert_ComponentArray
{
    /// <summary>
    /// Asserts the equality of array lengths in a component array instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="array">the gen index array instance.</param>
    public static void LengthEqual<T>(int length, ComponentArray<T> array)
    {
        Assert.Equal(length, array.Data.Length);
        Assert.Equal(length, array.Flags.Length);
        Assert.Equal(length, array.Allocated.Length);
        Assert.Equal(length, array.Length);
    }

    /// <summary>
    /// Asserts the equality of a entry and expected values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">the expected data value.</param>
    /// <param name="flag">the expected user-defined flags.</param>
    /// <param name="allocated">the expected allocated bool.</param>
    /// <param name="entryIndex">the index of the entry in the array to assert equality against.</param>
    /// <param name="array">the array instance containing the entry to assert.</param>
    public static void EntryEqual<T>(T data, int flag, bool allocated, int entryIndex, ComponentArray<T> array)
    {
        Assert.Equal(data, array.Data[entryIndex]);
        Assert.Equal(flag, array.Flags[entryIndex]);
        Assert.Equal(allocated, array.Allocated[entryIndex]);
    }

    /// <summary>
    /// Asserts that a component array instance is disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">the instance to check for disposed.</param>
    public static void Disposed<T>(ComponentArray<T> array)
    {
        Assert.Null(array.Data);
        Assert.Null(array.Flags);
        Assert.Null(array.Allocated);
        Assert.Equal(0, array.Length);
        Assert.Equal(0, array.Count);
        Assert.True(array.Disposed);
    }
}