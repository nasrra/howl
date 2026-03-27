using System;
using Howl.Collections;
using Howl.DataStructures;
using Howl.ECS;
using Howl.Physics;
using static Howl.Physics.Soa_Collision;

public class CollisionManifoldNew : IDisposable
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
    public CollisionManifoldNew(int maxCollisions)
    {
        CircleSpatialPairs          = new SpatialPairBuffer(maxCollisions);
        PolygonSpatialPairs         = new SpatialPairBuffer(maxCollisions);
        PolygonToCircleSpatialPairs = new SpatialPairBuffer(maxCollisions);

        CircleCollisionsToResolve           = new Soa_Collision(maxCollisions);
        PolygonCollisionsToResolve          = new Soa_Collision(maxCollisions);
        PolygonToCircleCollisionsToResolve  = new Soa_Collision(maxCollisions);
        Collisions                          = new Soa_Collision(maxCollisions);
    }

    /// <summary>
    /// Sorts a collision manifolds collisions by the owners gen-index index in ascending order.
    /// </summary>
    /// <param name="manifold">the collision manifold to sort.</param>
    public static void SortCollisions(CollisionManifoldNew manifold)
    {
        // Array.Sort(manifold.Collisions.Data, 0, manifold.Collisions.Count, Collision.AscendingOwnerIndexComparer);
        SortByOwnerGenIndex(manifold.Collisions);
    }

    /// <summary>
    /// Searches a collision span for the first found entry containing the queried gen-index.
    /// </summary>
    /// <param name="collisions">the spen of collisions to search.</param>
    /// <param name="genIndex">the gen-index.</param>
    /// <returns>the index in the collision span to the first found entry if the span contains an entry for gen-index; otherwise -1.</returns>
    public static int BinarySearch(Span<Collision> collisions, GenIndex genIndex)
    {
        int left = 0;
        int right = collisions.Length -1;

        while(left <= right)
        {
            int mid = (left + right) / 2;

            if (collisions[mid].Owner == genIndex)
            {
                return mid;
            }
            else if (collisions[mid].Owner.Index < genIndex.Index)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;                
            }
        }
        return -1;
    }

    /// <summary>
    /// Retrieves all collisions associated with a given GenIndex
    /// </summary>
    /// <remarks>
    /// Note: this method uses binary search, ensure that the collisions list has been sorted
    /// using the sort utility function before attempting to retrieve any collisions. 
    /// </remarks>
    /// <param name="collisions">the span of collisions to retrieve all collisions for a given gen-index.</param>
    /// <param name="genIndex">the specified gen-index to retrieve collision for.</param>
    /// <returns>the collisions found associated with the gen-index in the collisions span.</returns>
    public Span<Collision> RetrieveCollisions(Span<Collision> collisions, GenIndex genIndex)
    {
        Span<Collision> result;

        int index = BinarySearch(collisions, genIndex);

        if(index != -1)
        {
            int length = 1;
            int start = index;
            
            // get the collision entries in the upper span.
            for(int i = index + 1; i < collisions.Length; i++)
            {
                if(collisions[i].Owner == genIndex)
                {
                    length++;
                }
                else
                {
                    break;
                }
            }

            // get the collision entries in the lower span.
            for(int i = index - 1; i > 0; i--)
            {
                if(collisions[i].Owner == genIndex)
                {
                    length++;
                    start = i;
                }
                else
                {
                    break;
                }
            }

            // assign a slice of all found collisions as the result.
            result = collisions.Slice(start, length);
        }
        else
        {
            result = new Span<Collision>();
        }

        return result;
    }




    /*******************
    
        Disposal.
    
    ********************/




    public static void Dispose(CollisionManifoldNew manifold)
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

    ~CollisionManifoldNew()
    {
        Dispose(this);
    }
}