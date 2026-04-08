
using System;
using System.Runtime.CompilerServices;

namespace Howl.Ecs;

// Static fields in a generic class are per-closed type. That means:
// ComponentType<Position>   // has its own static fields
// ComponentType<Velocity>   // has a separate set of static fields
// Id is a static readonly field, so it is initialized only once, the first time the class is used.
internal static class ComponentType<T>
{
    private static int id = -1;
    private static bool isInitialised = false;
    public static bool IsInitialised => isInitialised;

    /// <summary>
    /// Initialises this component type with an Id.
    /// </summary>
    /// <exception cref="InvalidOperationException">thrown if this ComponentType has already been initialised.</exception>
    public static void Initialise()
    {
        if(isInitialised == false)
        {
            isInitialised = true;
            id = ComponentTypeId.Next();
        }
        else
        {
            throw new InvalidOperationException($"ComponentType '{typeof(T)}' has already been Initialised.");
        }
    }

    /// <summary>
    /// Gets the unique id associated with this component type.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">thrown if this ComponentType has not yet been initialised.</exception>
    public static int GetId()
    {
        if(isInitialised)
        {
            return id;
        }
        throw new InvalidOperationException($"ComponentType '{typeof(T)}' has not been initialised yet.");
    }
}
