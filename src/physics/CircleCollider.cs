using Howl.Math;

namespace Howl.Physics;

public struct CircleCollider
{
    public Circle Shape;
    public Collider Collider;

    public CircleCollider(Circle shape, Collider collider)
    {
        Shape = shape;
        Collider = collider;
    }
}