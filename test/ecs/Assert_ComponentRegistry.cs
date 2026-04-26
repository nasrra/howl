using Howl.Ecs;

namespace Howl.Test.Ecs;

public static class Assert_ComponentRegistry
{
    /// <summary>
    ///     Asserts the equality of array lengths in a component registry instance. 
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="registry">the component registry instance to assert.</param>
    public static void LengthEqual(int length, ComponentRegistry registry)
    {
        Assert.Equal(length, registry.TotalComponentArrayLength);
    }

    /// <summary>
    ///     Asserts that a component registry instance is disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="registry">the component registry instance to assert.</param>
    public static void Disposed(ComponentRegistry registry)
    {
        Assert.Null(registry.Components);
        Assert.True(registry.Disposed);
        Assert.True(registry.TotalComponentArrayLength == 0);
    }
}