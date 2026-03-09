using System;
using System.Collections.Generic;
using System.Diagnostics;
using Howl.Math;

namespace Howl.Physics;

public sealed class SOAPhysicsSystemState : IDisposable
{
    /// <summary>
    /// Gets and sets the type flags of all physics bodies.
    /// </summary>
    public PhysicsBodyFlags[] Flags;

    /// <summary>
    /// Gets and sets the widths of all physics bodies.
    /// </summary>
    public float[] Width;

    /// <summary>
    /// Gets and sets the heights of all physics bodies.
    /// </summary>
    public float[] Height;

    /// <summary>
    /// Gets and sets the radii of all physics bodies.
    /// </summary>
    public float[] Radius;
    
    /// <summary>
    /// Gets and sets the vertices positions of all physics bodies.  
    /// </summary>
    /// <remarks>
    /// Note: this is the base/untransformed value for a physics body shape vertice.
    /// </remarks>
    public SoaVector2 Vertice;
    
    /// <summary>
    /// Gets and sets the transformed vertice positions of all physics bodies.  
    /// </summary>
    public SoaVector2 TransformedVertice;

    /// <summary>
    /// Gets and sets the transforms of all physics bodies.
    /// </summary>
    public SoaTransform Transform;

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
    /// Gets and sets the first index of a physics bodies first vertice of physics body's shape.
    /// </summary>
    public int[] FirstVertice;

    /// <summary>
    /// Gets and sets the count of vertices of a physics body's shape.
    /// </summary>
    public int[] VerticeCount;

    /// <summary>
    /// The index of the next vertice of a given shape's vertex.
    /// </summary>
    public int[] NextVertice;
    
    /// <summary>
    /// The generation of a physics body id.
    /// </summary>
    public int[] Generation;
    
    /// <summary>
    /// Gets and sets the physics body indices available for reuse and allocation.
    /// </summary>
    public Stack<int> FreePhysicsBodyIndex;

    /// <summary>
    /// Gets and sets the vertice indices available for reuse and allocation.
    /// </summary>
    public Stack<int> FreeVertexIndex;

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
    /// Gets and sets the count of allocated physics body stored in this physics system state.
    /// </summary>
    public int AlloctedPhysicsBodyCount = 0;

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
        FixedUpdateStepStopwatch    = new();

        Flags               = new PhysicsBodyFlags[physicsBodyCount];
        Width               = new float[physicsBodyCount];
        Height              = new float[physicsBodyCount];
        Radius              = new float[physicsBodyCount];
        Vertice             = new SoaVector2(physicsBodyCount);
        TransformedVertice  = new SoaVector2(physicsBodyCount);
        Transform           = new SoaTransform(physicsBodyCount);
        StaticFriction      = new float[physicsBodyCount];
        KineticFriction     = new float[physicsBodyCount];
        NextVertice         = new int[maxVertices];
        Generation          = new int[physicsBodyCount];
        FirstVertice        = new int[physicsBodyCount];
        VerticeCount        = new int[physicsBodyCount];

        
        FreePhysicsBodyIndex = new();
        FreeVertexIndex = new();

        for(int i = physicsBodyCount-1; i >= 0; i--)
            FreePhysicsBodyIndex.Push(i); // push all available indices to the stack.

        for(int i = maxVertices-1; i >= 0; i--)
            FreeVertexIndex.Push(i); // push all available indices to the stack.
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