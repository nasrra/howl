using Howl.Test.Collections;

namespace Howl.Test.Ecs;

public static class Assert_EntityRegistry
{
    /// <summary>
    ///     Asserts the equality of array lengths in a entity registry instance. 
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="registry">the entity registry instance.</param>
    public static void LengthEqual(int length, EntityRegistry registry)
    {
        Assert.Equal(length, registry.GenIds.Length);
        Assert_StackArray.LengthEqual(length, registry.FreeSlots);
        Assert.Equal(length, registry.Allocated.Length);
    }

    /// <summary>
    ///     Asserts that a entity registry instance is disposed.
    /// </summary>
    /// <param name="registry">the entity registry instance to assert.</param>
    public static void Disposed(EntityRegistry registry)
    {
        Assert.Null(registry.GenIds);
        Assert.Null(registry.FreeSlots);
        Assert.Null(registry.Allocated);
        Assert.True(registry.Disposed);
    }
}