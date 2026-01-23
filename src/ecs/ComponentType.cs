
namespace Howl.ECS;

// Static fields in a generic class are per-closed type. That means:
// ComponentType<Position>   // has its own static fields
// ComponentType<Velocity>   // has a separate set of static fields
// Id is a static readonly field, so it is initialized only once, the first time the class is used.

internal static class ComponentType<T>
{
    public static readonly int Id = ComponentTypeId.Next();
}
