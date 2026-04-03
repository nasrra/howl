using System;
using Howl.Collections;
using Howl.DataStructures;
using Howl.ECS;
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
    public CollisionManifold(int maxCollisions)
    {
        CircleSpatialPairs          = new SpatialPairBuffer(maxCollisions);
        PolygonSpatialPairs         = new SpatialPairBuffer(maxCollisions);
        PolygonToCircleSpatialPairs = new SpatialPairBuffer(maxCollisions);

        CircleCollisionsToResolve           = new Soa_Collision(maxCollisions);
        PolygonCollisionsToResolve          = new Soa_Collision(maxCollisions);
        PolygonToCircleCollisionsToResolve  = new Soa_Collision(maxCollisions);
        Collisions                          = new Soa_Collision(maxCollisions);
    }


    //     /// <summary>
    // /// Sorts this manifold's collisions in a GenIndex index ascending order 
    // /// </summary>
    // /// <remarks>
    // /// Note: This is done so that the retrieve collision method can binary search for collisions.
    // /// </remarks>
    // public void Sort()
    // {
    //     Collisions.Sort((a,b) => a.Owner.Index.CompareTo(b.Owner.Index));
    // }

    // /// <summary>
    // /// Retrieves all collisions associated with a given GenIndex
    // /// </summary>
    // /// <remarks>
    // /// Note: this method uses binary search, ensure that the collisions list has been sorted
    // /// using the sort utility function before attempting to retrieve any collisions. 
    // /// </remarks>
    // /// <param name="genIndex"></param>
    // /// <returns></returns>
    // public ReadOnlySpan<Collision> RetrieveCollisions(GenIndex genIndex)
    // {
    //     ReadOnlySpan<Collision> collisionSpan = CollectionsMarshal.AsSpan(Collisions);
    //     ReadOnlySpan<Collision> result;

    //     int index = BinarySearch(collisionSpan, genIndex);

    //     if(index != -1)
    //     {
    //         int length = 1;
    //         int start = index;
            
    //         // get the collision entries in the upper span.
    //         for(int i = index + 1; i < collisionSpan.Length; i++)
    //         {
    //             if(collisionSpan[i].Owner == genIndex)
    //             {
    //                 length++;
    //             }
    //             else
    //             {
    //                 break;
    //             }
    //         }

    //         // get the collision entries in the lower span.
    //         for(int i = index - 1; i > 0; i--)
    //         {
    //             if(collisionSpan[i].Owner == genIndex)
    //             {
    //                 length++;
    //                 start = i;
    //             }
    //             else
    //             {
    //                 break;
    //             }
    //         }

    //         // assign a slice of all found collisions as the result.
    //         result = collisionSpan.Slice(start, length);
    //     }
    //     else
    //     {
    //         result = new ReadOnlySpan<Collision>();
    //     }

    //     return result;
    // }

    // /// <summary>
    // /// Searches a collision span for the first found entry containing the queried GenIndex.
    // /// </summary>
    // /// <param name="collisionSpan">The span of collisions to search</param>
    // /// <param name="genIndex">The gen index</param>
    // /// <returns></returns>
    // private int BinarySearch(ReadOnlySpan<Collision> collisionSpan, GenIndex genIndex)
    // {
    //     int left = 0;
    //     int right = collisionSpan.Length -1;

    //     while(left <= right)
    //     {
    //         int mid = (left + right) / 2;

    //         if (collisionSpan[mid].Owner == genIndex)
    //         {
    //             return mid;
    //         }
    //         else if (collisionSpan[mid].Owner.Index < genIndex.Index)
    //         {
    //             left = mid + 1;
    //         }
    //         else
    //         {
    //             right = mid - 1;                
    //         }
    //     }
    //     return -1;
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