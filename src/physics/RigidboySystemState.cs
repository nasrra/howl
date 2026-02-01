using System;
using Howl.Math;

namespace Howl.Physics;

public sealed class RigidbodySystemState : IDisposable
{
    private bool disposed;
    public bool IsDisposed => disposed;

    /// <summary>
    /// Gets and sets the gravity force.
    /// </summary>
    public float Gravity = 9.81f;

    /// <summary>
    /// Gets and sets the direction of gravity.
    /// </summary>
    public Vector2 GravityDirection = Vector2.Down;

    /// <summary>
    /// Throws an exception if this instance is disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException($"{nameof(RigidbodySystemState)}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            Gravity = 0;
            GravityDirection = Vector2.Zero;
        }

        disposed = true;
    }

    ~RigidbodySystemState()
    {
        Dispose(false);
    }
}