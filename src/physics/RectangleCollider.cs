using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public struct RectangleCollider
{
    public Rectangle BaseShape;
    public PolygonRectangle TransformedShape;
    
    public ColliderParameters Parameters;

    public RectangleCollider(Rectangle shape, ColliderParameters parameters)
    {
        BaseShape = shape;
        TransformedShape = new PolygonRectangle(shape);
        Parameters = parameters;
    }
}