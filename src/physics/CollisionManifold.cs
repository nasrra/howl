using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Physics;

public sealed class CollisionManifold : IDisposable
{
    private const int InitialCapacity = 4096;

    /// <summary>
    /// Gets and sets the stored collisions.
    /// </summary>
    private List<Collision> collisions;

    /// <summary>
    /// Gets the stored collision data.
    /// </summary>
    public IReadOnlyList<Collision> Collisions => collisions;

    /// <summary>
    /// Gets and sets whether this instance has been disposed.
    /// </summary>
    private bool disposed;

    /// <summary>
    /// Gets whether or not this instance is disposed.
    /// </summary>
    public bool IsDisposed => disposed;

    /// <summary>
    /// Creates a new CollisionManifold instance.
    /// </summary>
    public CollisionManifold()
    {
        disposed = false;
        collisions = new(InitialCapacity);
    }

    /// <summary>
    /// Clears all stored data.
    /// </summary>
    public void Clear()
    {
        collisions.Clear();
    }

    /// <summary>
    /// Adds a collision to the manifold.
    /// </summary>
    /// <param name="collision">The collision to add.</param>
    public void AddCollision(Collision collision)
    {
        collisions.Add(collision);
    }

    /// <summary>
    /// Sorts this manifold's collisions in a GenIndex index ascending order 
    /// </summary>
    public void Sort()
    {
        collisions.Sort((a,b) => a.Owner.Index.CompareTo(b.Owner.Index));
    }

    /// <summary>
    /// Retrieves all collisions associated with a given GenIndex
    /// </summary>
    /// <param name="genIndex"></param>
    /// <returns></returns>
    public ReadOnlySpan<Collision> RetrieveCollisions(GenIndex genIndex)
    {
        ReadOnlySpan<Collision> collisionSpan = CollectionsMarshal.AsSpan(collisions);
        ReadOnlySpan<Collision> result;

        int index = BinarySearch(collisionSpan, genIndex);

        if(index != -1)
        {
            int length = 1;
            int start = index;
            
            // get the collision entries in the upper span.
            for(int i = index + 1; i < collisionSpan.Length; i++)
            {
                if(collisionSpan[i].Owner == genIndex)
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
                if(collisionSpan[i].Owner == genIndex)
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
            result = collisionSpan.Slice(start, length);
        }
        else
        {
            result = new ReadOnlySpan<Collision>();
        }

        return result;
    }

    /// <summary>
    /// Searches a collision span for the first found entry containing the queried GenIndex.
    /// </summary>
    /// <param name="collisionSpan">The span of collisions to search</param>
    /// <param name="genIndex">The gen index</param>
    /// <returns></returns>
    private int BinarySearch(ReadOnlySpan<Collision> collisionSpan, GenIndex genIndex)
    {
        int left = 0;
        int right = collisionSpan.Length -1;

        while(left <= right)
        {
            int mid = (left + right) / 2;

            if (collisionSpan[mid].Owner == genIndex)
            {
                return mid;
            }
            else if (collisionSpan[mid].Owner.Index < genIndex.Index)
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
    /// Gets the internal collision collection as a read-only span.
    /// </summary>
    /// <returns>The read-only span</returns>
    public ReadOnlySpan<Collision> GetCollisionsAsReadOnlySpan()
    {
        return CollectionsMarshal.AsSpan(collisions);
    }

    /// <summary>
    /// Disposed this instace.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            Clear();
            collisions = null;
        }

        GC.SuppressFinalize(this);
        disposed = true;
    }

    ~CollisionManifold()
    {
        Dispose(false);
    }
}