using Howl.ECS;
using Howl.Math;

namespace Howl.Physics;

public readonly struct Collision
{
    public readonly GenIndex ColliderA;
    public readonly GenIndex ColliderB;
    public readonly Vector2 Normal;
    public readonly float Depth;

    public Collision(GenIndex colliderA, GenIndex colliderB, Vector2 normal, float depth)
    {
        ColliderA = colliderA;
        ColliderB = colliderB;
        Normal = normal;
        Depth = depth;
    }
}