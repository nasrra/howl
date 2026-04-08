using System;
using Howl.Collections;
using Howl.DataStructures;
using Howl.Ecs;
using Howl.Physics;
using static Howl.Physics.Soa_Collision;

public class CollisionManifold : IDisposable
{



    /*******************
    
        Spatial Pairs.
    
    ********************/




    public SpatialPairBuffer CircleSpatialPairs;

    public SpatialPairBuffer PolygonSpatialPairs;

    public SpatialPairBuffer PolygonToCircleSpatialPairs;




    /*******************
    
        Soa Collisions.
    
    ********************/




    /// <summary>
    /// Gets and sets the circle to circle collisions to resolve.
    /// </summary>
    public Soa_Collision CircleCollisionsToResolve;

    /// <summary>
    /// Gets and sets the polygon to polygon collisions to resolve.
    /// </summary>
    public Soa_Collision PolygonCollisionsToResolve;

    /// <summary>
    /// Gets and sets the polyon to circle collisions to resolve.
    /// </summary>
    public Soa_Collision PolygonToCircleCollisionsToResolve;

    /// <summary>
    /// Gets and sets all found collisions.
    /// </summary>
    public Soa_Collision Collisions;

    /// <summary>
    /// Gets and sets whetheror not this instance has been disposed.
    /// </summary>
    public bool IsDisposed;

    /// <summary>
    /// Creates a new Collision Manifold instance.
    /// </summary>
    /// <param name="maxCollisions"></param>
    public CollisionManifold(int totalColliderCount)
    {
        // mapping each collider (within a matrix structure) onto one another gives a squared length/count.
        int maxCollisions = totalColliderCount * totalColliderCount;

        CircleSpatialPairs          = new SpatialPairBuffer(maxCollisions);
        PolygonSpatialPairs         = new SpatialPairBuffer(maxCollisions);
        PolygonToCircleSpatialPairs = new SpatialPairBuffer(maxCollisions);

        CircleCollisionsToResolve           = new Soa_Collision(maxCollisions);
        PolygonCollisionsToResolve          = new Soa_Collision(maxCollisions);
        PolygonToCircleCollisionsToResolve  = new Soa_Collision(maxCollisions);
        Collisions                          = new Soa_Collision(maxCollisions);
    }

    // public static void Update(CollisionManifold manifold)
    // {
    //     Span<CollisionType> span = manifold.Collisions;

    //     for(int i = 0; i < span.Length; i++)
    //     {
    //         ref CollisionType collision = ref span[i];
    //         if((collision & CollisionType.SetThisStep) == 0)
    //         {
    //             collision = default;
    //             continue;
    //         }

    //         // remove the set this step flag for switch statement.
    //         collision &= ~CollisionType.SetThisStep;

    //         switch (collision)
    //         {
    //             case CollisionType.CollisionEnter:
    //                 collision = CollisionType.Colliding;
    //                 break;
    //             case CollisionType.CollisionExit:
    //                 collision = default;
    //                 break;
    //             case CollisionType.TriggerEnter:
    //                 collision = CollisionType.Triggering;
    //                 break;
    //             case CollisionType.TriggerExit:
    //                 collision = default;
    //                 break;
    //         }
    //     }
    // }





    /*******************
    
        Disposal.
    
    ********************/




    public static void Dispose(CollisionManifold manifold)
    {
        if(manifold.IsDisposed)
            return;
        
        GC.SuppressFinalize(manifold);
        manifold.IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(this);
    }

    ~CollisionManifold()
    {
        Dispose(this);
    }
}