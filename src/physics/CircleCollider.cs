using System.Runtime.CompilerServices;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public struct CircleCollider
{
    /// <summary>
    /// Gets and sets the shape.
    /// </summary>
    public Circle Shape;

    /// <summary>
    /// Gets and sets collider parameters.
    /// </summary>
    public ColliderParameters Parameters;

    /// <summary>
    /// Constructs a circle collider.
    /// </summary>
    /// <param name="shape">The shape data.</param>
    /// <param name="parameters">the collider parameters.</param>
    public CircleCollider(Circle shape, ColliderParameters parameters)
    {
        Shape = shape;
        Parameters = parameters;
    }
}