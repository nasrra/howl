using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public struct RectangleCollider
{
    public Rectangle Shape;
    public ColliderParameters Parameters;

    public RectangleCollider(Rectangle shape, ColliderParameters parameters)
    {
        Shape = shape;
        
        Parameters = parameters;
    }
}