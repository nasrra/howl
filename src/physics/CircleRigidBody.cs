using Howl.Math;

namespace Howl.Physics;

public struct CircleRigidBody
{
    public RigidBody RigidBody {get; private set;}
    public float Radius {get; private set;}
    public float RadiusSquared {get; private set;}

    /// <summary>
    /// Constructs a Circle Rigidbody.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="density"></param>
    /// <param name="restitution"></param>
    /// <param name="radius"></param>
    public CircleRigidBody(
        Vector2 position,
        float rotation,
        float density,
        float restitution,
        float radius
    )
    {

        float area = radius * radius;
        float mass = area * density;

        Radius = radius;
        RadiusSquared = radius * radius;

        RigidBody = new RigidBody(
            position,
            rotation,
            area,
            density,
            mass,
            restitution
        );
    }
}