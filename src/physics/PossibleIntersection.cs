using Howl.ECS;

namespace Howl.Physics;

public readonly struct PossibleIntersection
{
    public readonly GenIndex ColliderA;
    public readonly GenIndex ColliderB;

    public PossibleIntersection(GenIndex colliderA, GenIndex colliderB)
    {
        ColliderA = colliderA;
        ColliderB = colliderB;
    }
}