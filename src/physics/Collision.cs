using System;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Math;

namespace Howl.Physics;

public unsafe struct Collision
{
    /// <summary>
    /// Gets the maximum amount of contact points for a collision.
    /// </summary>
    /// <remarks>
    /// Note: A 2d collision can have two contact points when two edges are perfectly perpendicular to eachother.
    /// </remarks>
    private const int MaxContactPoints = 2;

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
    /// Gets the center of the owner's collider shape.
    /// </summary>
    /// <remarks>
    /// Note: the shape must not be transformed in any way, it must be the untransformed shape directly from a collider.
    /// </remarks>
    public readonly Vector2 OwnerColliderShapeCenter;

    /// <summary>
    /// Gets the center of the other's collider shape.
    /// </summary>
    /// <remarks>
    /// Note: the shape must not be transformed in any way, it must be the untransformed shape directly from a collider.
    /// </remarks>
    public readonly Vector2 OtherColliderShapeCenter;

    /// <summary>
    /// Gets and sets the x-positional value for the contact points.
    /// </summary>
    private fixed float xContactPoints[MaxContactPoints];

    /// <summary>
    /// Gets and sets the y-positional value for the contact points.
    /// </summary>
    private fixed float yContactPoints[MaxContactPoints];

    /// <summary>
    /// Gets the depth of the collision.
    /// </summary>
    public readonly float Depth;

    /// <summary>
    /// Gets and sets the count of stored contact points. 
    /// </summary>
    private int contactPointsCount;

    /// <summary>
    /// Gets the count of stored contact points. 
    /// </summary>
    public readonly int ContactPointsCount => contactPointsCount;

    /// <summary>
    /// Constructs a Collision.
    /// </summary>
    /// <param name="owner">The owner of this collision.</param>
    /// <param name="other">The other collider of this collision.</param>
    /// <param name="ownerParameters">the owner's parameters.</param>
    /// <param name="otherParameters">the other's parameters.</param>
    /// <param name="xContactPoints">the x-positional value for the contact points.</param>
    /// <param name="yContactPoints">the y-positional value for the contact points.</param>
    /// <param name="ownerColliderShapeCenter">the center of the owner's collider shape</param>
    /// <param name="otherColliderShapeCenter">the center of the other's collider shape</param>
    /// <param name="normal">the normal of the collision.</param>
    /// <param name="depth">the depth of the collision.</param>
    public Collision(
        GenIndex owner, 
        GenIndex other, 
        ColliderParameters ownerParameters, 
        ColliderParameters otherParameters, 
        ReadOnlySpan<float> xContactPoints,
        ReadOnlySpan<float> yContactPoints,
        Vector2 ownerColliderShapeCenter,
        Vector2 otherColliderShapeCenter,
        Vector2 normal,
        float depth)
    {
        Owner = owner;
        Other = other;
        OwnerParameters = ownerParameters;
        OtherParameters = otherParameters;
        Normal = normal;
        Depth = depth;
        SetContactPoints(xContactPoints, yContactPoints);
        OwnerColliderShapeCenter = ownerColliderShapeCenter;
        OtherColliderShapeCenter = otherColliderShapeCenter;
    }

    private void SetContactPoints(ReadOnlySpan<float> xContactPoints, ReadOnlySpan<float> yContactPoints)
    {
        if(xContactPoints.Length != yContactPoints.Length)
        {
            throw new ArgumentException($"xContactPoints length '{xContactPoints.Length}' does not equal yContactPoints length '{yContactPoints.Length}'");
        }

        fixed (float* x = this.xContactPoints)
        {
            for(int i = 0; i < xContactPoints.Length; i++)
            {
                x[i] = xContactPoints[i];
            }
        }

        fixed (float* y = this.yContactPoints)
        {
            for(int i = 0; i < yContactPoints.Length; i++)
            {
                y[i] = yContactPoints[i];
            }
        }

        contactPointsCount = xContactPoints.Length;
    }

    /// <summary>
    /// Gets a readonly span of the x-positional values of the stored contact points.
    /// </summary>
    /// <returns>the readonly span.</returns>
    public ReadOnlySpan<float> GetXContactPointsAsReadOnlySpan()
    {
        ReadOnlySpan<float> span;
        fixed(float* ptr = xContactPoints)
        {
            span = new ReadOnlySpan<float>(ptr, ContactPointsCount);
        }
        return span;
    }

    /// <summary>
    /// Gets a readonly span of the y-positional values of the stored contact points.
    /// </summary>
    /// <returns>the readonly span.</returns>
    public ReadOnlySpan<float> GetYContactPointsAsReadOnlySpan()
    {
        ReadOnlySpan<float> span;
        fixed(float* ptr = yContactPoints)
        {
            span = new ReadOnlySpan<float>(ptr, ContactPointsCount);
        }
        return span;
    }
}