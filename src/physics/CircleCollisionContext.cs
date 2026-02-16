using System;
using System.Collections.Generic;
using Howl.ECS;

namespace Howl.Physics;

/// <summary>
/// This struct is intended for passing collision system state information 
/// to parallel function calls pretaining to circle to circle collisions.
/// </summary>
public struct CircleCollisionContext
{
    /// <summary>
    /// Gets and sets the reference to a colliders gen index list.
    /// </summary>
    public GenIndexList<CircleCollider> Colliders;

    /// <summary>
    /// Gets and sets a reference to a collider pairs list, containing the pairs of the colliders that are near eachother.
    /// </summary>
    public List<ColliderPair> Pairs;

    /// <summary>
    /// Gets and sets a reference to a collision manifold.
    /// </summary>
    public CollisionManifold CollisionManifold;

    /// <summary>
    /// Constructs a circle collision context.
    /// </summary>
    /// <param name="colliders">the reference to a colliders gen index list.</param>
    /// <param name="pairs">the reference to a collider pairs list, containing the pairs of the colliders that are near eachother.</param>
    /// <param name="collisionManifold">the reference to a collision manifold.</param>
    public CircleCollisionContext(
        GenIndexList<CircleCollider> colliders, 
        List<ColliderPair> pairs,
        CollisionManifold collisionManifold 
    )
    {
        Colliders = colliders;
        Pairs = pairs;
        CollisionManifold = collisionManifold;
    }

    /// <summary>
    /// Clears a collision context of its strong references.
    /// </summary>
    /// <param name="ctx">the context to clear.</param>
    public static void Clear(ref CircleCollisionContext ctx)
    {
        ctx.Colliders         = null;
        ctx.Pairs             = null;
        ctx.CollisionManifold = null;
    }
}