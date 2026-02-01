using Howl.ECS;
using Howl.Math;

namespace Howl.Physics;

public readonly struct Collision
{
    public readonly GenIndex ColliderA;
    public readonly GenIndex ColliderB;
    public readonly ColliderParameters ColliderAParameters;
    public readonly ColliderParameters ColliderBParameters;
    public readonly Vector2 Normal;
    public readonly float Depth;

    public Collision(
        GenIndex colliderA, 
        GenIndex colliderB, 
        ColliderParameters colliderAParameters, 
        ColliderParameters colliderBParameters, 
        Vector2 normal, 
        float depth)
    {
        ColliderA = colliderA;
        ColliderB = colliderB;
        ColliderAParameters = colliderAParameters;
        ColliderBParameters = colliderBParameters;
        Normal = normal;
        Depth = depth;
    }
}