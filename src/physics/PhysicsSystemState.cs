using System;
using Howl.ECS;

namespace Howl.Physics;

public sealed class PhysicsSystemState : IDisposable
{
    /// <summary>
    /// Gets and sets the CollisionSystemState.
    /// </summary>
    public CollisionSystemState CollisionSystemState;

    /// <summary>
    /// Gets and sets the RigidbodySystemState.
    /// </summary>
    public RigidbodySystemState RigidbodySystemState;

    /// <summary>
    /// Gets and sets whether or not this instance has been diposed.
    /// </summary>
    private bool disposed;

    /// <summary>
    /// Gets whether or not this instance has been disposed.
    /// </summary>
    public bool IsDisposed => disposed;

    public PhysicsSystemState(CollisionSystemState collisionSystemState, RigidbodySystemState rigidbodySystemState)
    {
        if (collisionSystemState == null)
        {
            throw new ArgumentNullException($"'{nameof(PhysicsSystemState)}' cannot initialise with a null '{nameof(CollisionSystemState)}'");
        }

        if(rigidbodySystemState == null)
        {
            throw new ArgumentNullException($"'{nameof(PhysicsSystemState)}' cannot initialise with a null '{nameof(RigidbodySystemState)}'");
        }

        CollisionSystemState = collisionSystemState;
        RigidbodySystemState = rigidbodySystemState;
    }

    /// <summary>
    /// Throws an exception if this instance is disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException($"{nameof(PhysicsSystemState)}");
        }        
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            CollisionSystemState?.Dispose();
            CollisionSystemState = null;

            RigidbodySystemState?.Dispose();
            RigidbodySystemState = null;
        }

        disposed = true;
    }

    ~PhysicsSystemState()
    {
        Dispose(false);    
    }
}