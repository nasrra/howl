using System.Diagnostics.Metrics;
using System.Threading;

namespace Howl.ECS;

internal static class ComponentTypeId
{
    static int count = 0;

    /// <summary>
    /// Gets the amount of component types that have been registered.
    /// </summary>
    public static int Count => count;

    /// <summary>
    /// Gets the next available Id for a component type.
    /// </summary>
    /// <returns></returns>
    public static int Next()
    {
        // This is thread-safe, so even if multiple threads ask for a new ID simultaneously, each gets a unique one.
        // Interlocked.Increment returns the incremented value, so subtracting 1 gives the current ID before increment.
        return Interlocked.Increment(ref count) - 1;
    } 
}