using Howl.Math;

namespace Howl.Physics;

public struct RectangleCollider
{
    public PolygonRectangle Shape;
    public ColliderParameters Parameters;

    public RectangleCollider(PolygonRectangle shape, ColliderParameters parameters)
    {
        Shape = shape;
        Parameters = parameters;
    }
}