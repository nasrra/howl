using System.Runtime.CompilerServices;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public struct CircleCollider
{
    /// <summary>
    /// Gets and sets the base shape.
    /// </summary>
    public Circle BaseShape;

    /// <summary>
    /// Gets and sets the transformed shape.
    /// </summary>
    public Circle TransformedShape;

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
        BaseShape = shape;
        TransformedShape = shape;
        Parameters = parameters;
    }
}