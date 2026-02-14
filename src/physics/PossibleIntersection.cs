using Howl.ECS;

namespace Howl.Physics;

public readonly struct ColliderPair
{
    public readonly GenIndex ColliderA;
    public readonly GenIndex ColliderB;

    public ColliderPair(GenIndex colliderA, GenIndex colliderB)
    {
        ColliderA = colliderA;
        ColliderB = colliderB;
    }
}