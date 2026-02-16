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
    public const int MaxContactPoints = 2;

    /// <summary>
    /// Gets the owning collider of this collision.
    /// </summary>
    public GenIndex Owner;

    /// <summary>
    /// Gets the other collider of this collision.
    /// </summary>
    public GenIndex Other;

    /// <summary>
    /// Gets the owner's parameters.
    /// </summary>
    public ColliderParameters OwnerParameters;

    /// <summary>
    /// Gets the other's parameters.
    /// </summary>
    public ColliderParameters OtherParameters;

    /// <summary>
    /// Gets and sets the x-component of the collision normal.
    /// </summary>
    public float NormalX;

    /// <summary>
    /// Gets and sets the y-component of the collision normal.
    /// </summary>
    public float NormalY;

    /// <summary>
    /// Gets the X-component of the owner collider shape's center.
    /// </summary>
    /// <remarks>
    /// Note: the shape must not be transformed in any way, it must be the untransformed shape directly from a collider.
    /// </remarks>
    public float OwnerColliderShapeCenterX;

    /// <summary>
    /// Gets the y-component of the owner collider shape's center.
    /// </summary>
    /// <remarks>
    /// Note: the shape must not be transformed in any way, it must be the untransformed shape directly from a collider.
    /// </remarks>
    public float OwnerColliderShapeCenterY;

    /// <summary>
    /// Gets the X-component of the other collider shape's center.
    /// </summary>
    /// <remarks>
    /// Note: the shape must not be transformed in any way, it must be the untransformed shape directly from a collider.
    /// </remarks>
    public float OtherColliderShapeCenterX;

    /// <summary>
    /// Gets the y-component of the other collider shape's center.
    /// </summary>
    /// <remarks>
    /// Note: the shape must not be transformed in any way, it must be the untransformed shape directly from a collider.
    /// </remarks>
    public float OtherColliderShapeCenterY;

    /// <summary>
    /// Gets and sets the x-positional value for the contact points.
    /// </summary>
    public fixed float ContactPointsX[MaxContactPoints];

    /// <summary>
    /// Gets and sets the y-positional value for the contact points.
    /// </summary>
    public fixed float ContactPointsY[MaxContactPoints];

    /// <summary>
    /// Gets and sets the depth of the collision.
    /// </summary>
    public float Depth;

    /// <summary>
    /// Gets and sets the count of stored contact points. 
    /// </summary>
    public int ContactPointsCount;

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
        Span<float> xContactPoints,
        Span<float> yContactPoints,
        float normalX,
        float normalY,
        float ownerColliderShapeCenterX,
        float ownerColliderShapeCenterY,
        float otherColliderShapeCenterX,
        float otherColliderShapeCenterY,
        float depth)
    {
        Owner = owner;
        Other = other;
        OwnerParameters = ownerParameters;
        OtherParameters = otherParameters;
        NormalX = normalX;
        NormalY = normalY;
        Depth = depth;
        OwnerColliderShapeCenterX = ownerColliderShapeCenterX;
        OwnerColliderShapeCenterY = ownerColliderShapeCenterY;
        OtherColliderShapeCenterX = otherColliderShapeCenterX;
        OtherColliderShapeCenterY = otherColliderShapeCenterY;
        SetContactPoints(ref this, xContactPoints, yContactPoints);
    }

    public static void SetContactPoints(ref Collision collision, Span<float> xContactPoints, Span<float> yContactPoints)
    {
        if(xContactPoints.Length != yContactPoints.Length)
        {
            throw new ArgumentException($"xContactPoints length '{xContactPoints.Length}' does not equal yContactPoints length '{yContactPoints.Length}'");
        }

        fixed (float* x = collision.ContactPointsX)
        {
            for(int i = 0; i < xContactPoints.Length; i++)
            {
                x[i] = xContactPoints[i];
            }
        }

        fixed (float* y = collision.ContactPointsY)
        {
            for(int i = 0; i < yContactPoints.Length; i++)
            {
                y[i] = yContactPoints[i];
            }
        }

        collision.ContactPointsCount = xContactPoints.Length;
    }

    /// <summary>
    /// Gets a span of the y-components of the stored contact points.
    /// </summary>
    /// <param name="collision">the collision.</param>
    /// <returns>the span.</returns>
    public static Span<float> ContactPointsXAsSpan(in Collision collision)
    {
        Span<float> span;
        fixed(float* ptr = collision.ContactPointsX)
        {
            span = new Span<float>(ptr, collision.ContactPointsCount);
        }
        return span;
    }

    /// <summary>
    /// Gets a span of the y-components of a collision's stored contact points.
    /// </summary>
    /// <param name="collision">the collision.</param>
    /// <returns>the span.</returns>
    public static Span<float> ContactPointsYAsSpan(in Collision collision)
    {
        Span<float> span;
        fixed(float* ptr = collision.ContactPointsY)
        {
            span = new Span<float>(ptr, collision.ContactPointsCount);
        }
        return span;
    }
}