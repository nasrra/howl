using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public struct CircleCollider
{
    public Circle Shape;
    public ColliderParameters Parameters;

    public CircleCollider(Circle shape, ColliderParameters parameters)
    {
        Shape = shape;
        Parameters = parameters;
    }
}