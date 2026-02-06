using Howl.ECS;
using Howl.Math;

namespace Howl.Physics;

public readonly struct Collision
{
    /// <summary>
    /// Gets the owning collider of this collision.
    /// </summary>
    public readonly GenIndex Owner;

    /// <summary>
    /// Gets the other collider of this collision.
    /// </summary>
    public readonly GenIndex Other;

    /// <summary>
    /// Gets the owner's parameters.
    /// </summary>
    public readonly ColliderParameters OwnerParameters;

    /// <summary>
    /// Gets the other's parameters.
    /// </summary>
    public readonly ColliderParameters OtherParameters;

    /// <summary>
    /// Gets the normal of the collision.
    /// </summary>
    public readonly Vector2 Normal;

    /// <summary>
    /// Gets the depth of the collision.
    /// </summary>
    public readonly float Depth;

    /// <summary>
    /// Constructs a Collision.
    /// </summary>
    /// <param name="owner">The owner of this collision.</param>
    /// <param name="other">The other collider of this collision.</param>
    /// <param name="ownerParameters">the owner's parameters.</param>
    /// <param name="otherParameters">the other's parameters.</param>
    /// <param name="normal">the normal of the collision.</param>
    /// <param name="depth">the depth of the collision.</param>
    public Collision(
        GenIndex owner, 
        GenIndex other, 
        ColliderParameters ownerParameters, 
        ColliderParameters otherParameters, 
        Vector2 normal, 
        float depth)
    {
        Owner = owner;
        Other = other;
        OwnerParameters = ownerParameters;
        OtherParameters = otherParameters;
        Normal = normal;
        Depth = depth;
    }
}