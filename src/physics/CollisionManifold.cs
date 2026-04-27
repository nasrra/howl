using System;

namespace Howl.Physics;

public class CollisionManifold : IDisposable
{




    /// <summary>
    ///     The maximum amount of colliders 
    /// </summary>
    /// <remarks>
    ///     Remarks: This is because collisions are stored in a one dimensional array, meaning anything higher than 46340 * 46340 will cause an integer overflow.
    /// </remarks>
    public const int MaxColliderCount = 46340;




    /*******************
    
        Soa Collisions.
    
    ********************/




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
    /// <param name="totalColliderCount">the total amount of colliders in the simulation.</param>
    public CollisionManifold(int totalColliderCount)
    {
        // mapping each collider (within a matrix structure) onto one another gives a squared length/count.
        int maxCollisions = totalColliderCount * totalColliderCount;
        Collisions = new Soa_Collision(maxCollisions);
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