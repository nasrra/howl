using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Howl.Physics;

public sealed class SOAPhysicsSystemState : IDisposable
{
    

    /// <summary>
    /// The type flags of all physics bodies.
    /// </summary>
    public PhysicsBodyFlags[] Flags;

    /// <summary>
    /// The widths of all physics bodies.
    /// </summary>
    public float[] Width;

    /// <summary>
    /// The heights of all physics bodies.
    /// </summary>
    public float[] Height;

    /// <summary>
    /// The radii of all physics bodies.
    /// </summary>
    public float[] Radius;

    /// <summary>
    /// The x-component vertex position of all physics bodies.  
    /// </summary>
    /// <remarks>
    /// Note: this is the base/untransformed value for a physics body shape vertice.
    /// </remarks>
    public float[] VerticeX;
    
    /// <summary>
    /// The y-component vertex position of all physics bodies.  
    /// </summary>
    /// <remarks>
    /// Note: this is the base/untransformed value for a physics body shape vertice.
    /// </remarks>
    public float[] VerticeY;
    
    /// <summary>
    /// The transformed x-component vertex position of all physics bodies.  
    /// </summary>
    public float[] TransformedVerticeX;
    
    /// <summary>
    /// The transformed y-component vertex position of all physics bodies.  
    /// </summary>
    public float[] TransformedVerticeY;

    /// <summary>
    /// Gets and sets the static friction values of all physics bodies.
    /// </summary>
    /// <remarks>
    /// Note: static friction resists motion before an object starts sliding.
    /// </remarks>
    public float[] StaticFriction;

    /// <summary>
    /// Gets and sets the kinetic friction value of all physics bodies.
    /// </summary>
    /// <remarks>
    /// Note: kinetic friction is applied when an object is sliding/currently in motion.
    /// </remarks>
    public float[] KineticFriction;
    
    /// <summary>
    /// The index of the next vertice of a given shape's vertex.
    /// </summary>
    public int[] NextVertice;
    
    /// <summary>
    /// The generation of a physics body id.
    /// </summary>
    public int[] Generation;
    
    /// <summary>
    /// the indices available for reuse and allocation.
    /// </summary>
    public Stack<int> Free;

    /// <summary>
    /// Gets and sets the CollisionSystemState.
    /// </summary>
    public CollisionSystemState CollisionSystemState;

    /// <summary>
    /// Gets and sets the RigidbodySystemState.
    /// </summary>
    public RigidBodySystemState RigidbodySystemState;

    /// <summary>
    /// Gets and sets the debug stopwatch for timing a physics system fixed-update substep.
    /// </summary>
    public Stopwatch FixedUpdateSubStepStopwatch;

    /// <summary>
    /// Gets and sets the debug stopwatch for timing a physics system fixed-update step.
    /// </summary>
    public Stopwatch FixedUpdateStepStopwatch;

    /// <summary>
    /// Gets and sets whether or not this instance has been diposed.
    /// </summary>
    private bool disposed;

    /// <summary>
    /// Gets whether or not this instance has been disposed.
    /// </summary>
    public bool IsDisposed => disposed;

    public SOAPhysicsSystemState(int physicsBodyCount, int maxVertices, CollisionSystemState collisionSystemState, RigidBodySystemState rigidbodySystemState)
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
        FixedUpdateSubStepStopwatch = new();
        FixedUpdateStepStopwatch = new();

        Flags               = new PhysicsBodyFlags[physicsBodyCount];
        Width               = new float[physicsBodyCount];
        Height              = new float[physicsBodyCount];
        Radius              = new float[physicsBodyCount];
        VerticeX            = new float[physicsBodyCount];
        VerticeY            = new float[physicsBodyCount];
        TransformedVerticeX = new float[maxVertices];
        TransformedVerticeY = new float[maxVertices];
        StaticFriction      = new float[physicsBodyCount];
        KineticFriction     = new float[physicsBodyCount];
        NextVertice         = new int[maxVertices];
        Generation          = new int[physicsBodyCount];
        Free                = new();

        for(int i = physicsBodyCount-1; i >= 0; i--)
            Free.Push(i); // push all available indices to the stack.
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

    ~SOAPhysicsSystemState()
    {
        Dispose(false);    
    }
}