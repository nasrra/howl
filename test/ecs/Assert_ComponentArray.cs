using Howl.Collections;
using Howl.Test.Collections;

namespace Howl.Test.ECS;

public static class Assert_ComponentArray
{
    /// <summary>
    ///     Asserts the equality of array lengths in a component array instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="array">the gen index array instance.</param>
    public static void LengthEqual<T>(int length, ComponentArray<T> array)
    {
        Assert.Equal(length, array.Sparse.Length);
        Assert.Equal(length, array.DenseIndices.Length);
        Assert.Equal(length, array.Allocated.Length);
        Assert_SwapBackArray.LengthEqual(length, array.Active);
        Assert.Equal(length, array.Length);
    }

    /// <summary>
    ///     Asserts the equality of a entry and expected values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component">the expected component values.</param>
    /// <param name="allocated">the expected allocated bool.</param>
    /// <param name="entryIndex">the index of the entry in the array to assert equality against.</param>
    /// <param name="array">the array instance containing the entry to assert.</param>
    public static void EntryEqual<T>(T component, bool allocated, int entryIndex, ComponentArray<T> array)
    {
        Assert.Equal(component, array.Sparse[entryIndex]);
        Assert.Equal(allocated, array.Allocated[entryIndex]);
    }

    /// <summary>
    ///     Asserts that an entry in a component array instance is <c>Active</c>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="genId">the gen id of the component entry.</param>
    /// <param name="array">the component array instance.</param>
    public static void EntryIsActive<T>(GenId genId, ComponentArray<T> array)
    {
        int sparseIndex = ComponentArray.GetSparseIndex(genId);
        int denseIndex = array.DenseIndices[sparseIndex];
        Assert.True(denseIndex > 0); // 0 is the Nil value and indicates that the component is not active.        
        Assert.Equal(genId, array.Active[denseIndex]);
    }

    /// <summary>
    ///     Asserts that an entry in a component array instance is <c>Inactive</c>./
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="genId">the gen id of the component entry.</param>
    /// <param name="array">the component array instance.</param>
    public static void EntryIsInactive<T>(GenId genId, ComponentArray<T> array)
    {
        int sparseIndex = ComponentArray.GetSparseIndex(genId);
        int denseIndex = array.DenseIndices[sparseIndex];
        Assert.True(denseIndex == 0); // 0 is the Nil value and indicates that the component is not active.
    }

    /// <summary>
    ///     Asserts that a component array instance is disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">the instance to check for disposed.</param>
    public static void Disposed<T>(ComponentArray<T> array)
    {
        Assert.Null(array.Sparse);
        Assert.Null(array.DenseIndices);
        Assert.Null(array.Allocated);
        Assert.Null(array.Active);
        Assert.Equal(0, array.Length);
        Assert.True(array.Disposed);
    }
}