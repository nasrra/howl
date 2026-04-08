using Howl.Ecs;
using Howl.Test.Collections;

namespace Howl.Test.Ecs;

public class Assert_GenIndexArray{
    
    /// <summary>
    /// Asserts the equality of array lengths in a GenIndexArray instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="array">the gen index array instance.</param>
    public static void LengthEqual<T>(int length, GenIndexArray<T> array)
    {
        Assert.Equal(length, array.Data.Length);
        Assert.Equal(length, array.Flags.Length);
        Assert.Equal(length, array.Generations.Length);
        Assert.Equal(length, array.Allocated.Length);
        Assert_StackArray.LengthEqual(length, array.FreeSlots);
    }

    /// <summary>
    /// Asserts the equality of a entry and expected values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">the expected data value.</param>
    /// <param name="generation">the expected generation.</param>
    /// <param name="flag">the expected user-defined flags.</param>
    /// <param name="allocated">the expected allocated bool.</param>
    /// <param name="entryIndex">the index of the entry in the array to assert equality against.</param>
    /// <param name="array">the array instance containing the entry to assert.</param>
    public static void EntryEqual<T>(T data, int generation, int flag, bool allocated, int entryIndex, GenIndexArray<T> array)
    {
        Assert.Equal(data, array.Data[entryIndex]);
        Assert.Equal(generation, array.Generations[entryIndex]);
        Assert.Equal(flag, array.Flags[entryIndex]);
        Assert.Equal(allocated, array.Allocated[entryIndex]);
    }
}