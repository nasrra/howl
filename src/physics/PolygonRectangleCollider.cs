using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public struct PolygonRectangleCollider
{
    public PolygonRectangle Shape;
    public ColliderParameters Parameters;

    public PolygonRectangleCollider(PolygonRectangle shape, ColliderParameters parameters)
    {
        Shape = shape;
        Parameters = parameters;
    }
}