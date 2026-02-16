using System;
using System.Collections.Generic;
using Howl.ECS;

namespace Howl.Physics;

/// <summary>
/// This struct is intended for passing collision system state information
/// to parallel function calls pretaining to rectangle to circle collisions. 
/// </summary>
public struct RectangleToCircleCollisionContext
{
    /// <summary>
    /// Gets and sets the reference to a rectangle colliders gen index list.
    /// </summary>
    public GenIndexList<RectangleCollider> Rectangles;

    /// <summary>
    /// Gets and sets the reference to a circle colliders gen index list.
    /// </summary>
    public GenIndexList<CircleCollider> Circles;

    /// <summary>
    /// Gets and sets a reference to a collider pairs list, containing the pairs of the colliders that are near eachother.
    /// </summary>
    /// <remarks>
    /// Note: collider pairings should be Collider A as the rectangle and Collider B as the circle.
    /// </remarks>
    public List<ColliderPair> Pairs;

    /// <summary>
    /// Gets and sets a reference to a collision manifold.
    /// </summary>
    public CollisionManifold CollisionManifold;

    /// <summary>
    /// Constructs a rectangle collision context.
    /// </summary>
    /// <remarks>
    /// Note: collider pairings should be Collider A as the rectangle and Collider B as the circle.
    /// </remarks>
    /// <param name="rectangles">the reference to a rectangles colliders gen index list.</param>
    /// <param name="circles">the reference to a circle colliders gen index list.</param>
    /// <param name="pairs">the reference to a collider pairs list, containing the pairs of the colliders that are near eachother.</param>
    /// <param name="collisionManifold">the reference to a collision manifold.</param>
    public RectangleToCircleCollisionContext(
        GenIndexList<RectangleCollider> rectangles,
        GenIndexList<CircleCollider> circles,
        List<ColliderPair> pairs,
        CollisionManifold collisionManifold 
    )
    {
        Rectangles = rectangles;
        Circles = circles;
        Pairs = pairs;
        CollisionManifold = collisionManifold;
    }

    /// <summary>
    /// Clears a collision context of its strong references.
    /// </summary>
    /// <param name="ctx">the context to clear.</param>
    public static void Clear(ref RectangleToCircleCollisionContext ctx)
    {
        ctx.Rectangles          = null;
        ctx.Circles             = null;
        ctx.Pairs               = null;
        ctx.CollisionManifold   = null;
    }
}