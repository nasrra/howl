using System;
using System.Collections.Generic;
using System.Diagnostics;
using Howl.Math;

namespace Howl.Physics;

public sealed class SoaPhysicsSystemState : IDisposable
{
    /// <summary>
    /// Gets and sets the type flags of all physics bodies.
    /// </summary>
    public PhysicsBodyFlags[] Flags;

    /// <summary>
    /// Gets and sets the vertices positions of all physics bodies.  
    /// </summary>
    /// <remarks>
    /// Note: this is the base/untransformed value for a physics body shape vertice.
    /// </remarks>
    public SoaVector2 Vertices;
    
    /// <summary>
    /// Gets and sets the transformed vertice positions of all physics bodies.  
    /// </summary>
    public SoaVector2 TransformedVertices;

    /// <summary>
    /// Gets and sets the transforms of all physics bodies.
    /// </summary>
    public SoaTransform Transforms;

    /// <summary>
    /// Gets and sets the widths of all physics bodies.
    /// </summary>
    public float[] Widths;

    /// <summary>
    /// Gets and sets the heights of all physics bodies.
    /// </summary>
    public float[] Heights;

    /// <summary>
    /// Gets and sets the radii of all physics bodies.
    /// </summary>
    public float[] Radii;

    /// <summary>
    /// Gets and sets the transformed radii of all physics bodies.
    /// </summary>
    public float[] TransformedRadii;    

    /// <summary>
    /// Gets and sets the static friction values of all physics bodies.
    /// </summary>
    /// <remarks>
    /// Note: static friction resists motion before an object starts sliding.
    /// </remarks>
    public float[] StaticFrictions;

    /// <summary>
    /// Gets and sets the kinetic friction value of all physics bodies.
    /// </summary>
    /// <remarks>
    /// Note: kinetic friction is applied when an object is sliding/currently in motion.
    /// </remarks>
    public float[] KineticFrictions;
    
    /// <summary>
    /// Gets and sets the first index of a physics bodies first vertice of physics body's shape.
    /// </summary>
    public int[] FirstVertices;

    /// <summary>
    /// Gets and sets the count of vertices of a physics body's shape.
    /// </summary>
    public int[] VerticeCounts;

    /// <summary>
    /// The index of the next vertice of a given shape's vertex.
    /// </summary>
    /// <remarks>
    /// Note: the next index for a given shape are stored in a circular intrusive linked list; 
    /// meaning that the next vertice index of the final vertice will be the first vertice index. 
    /// </remarks>
    public int[] NextVertices;
    
    /// <summary>
    /// The generation of a physics body id.
    /// </summary>
    public int[] Generations;
    
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

    public SoaPhysicsSystemState(int physicsBodyCount, int maxVertices, CollisionSystemState collisionSystemState, RigidBodySystemState rigidbodySystemState)
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

        Flags                   = new PhysicsBodyFlags[physicsBodyCount];
        Widths                  = new float[physicsBodyCount];
        Heights                 = new float[physicsBodyCount];
        Radii                   = new float[physicsBodyCount];
        Vertices                = new SoaVector2(physicsBodyCount);
        TransformedVertices     = new SoaVector2(physicsBodyCount);
        Transforms              = new SoaTransform(physicsBodyCount);
        StaticFrictions         = new float[physicsBodyCount];
        KineticFrictions        = new float[physicsBodyCount];
        TransformedRadii        = new float[physicsBodyCount];
        NextVertices            = new int[maxVertices];
        Generations             = new int[physicsBodyCount];
        FirstVertices           = new int[physicsBodyCount];
        VerticeCounts           = new int[physicsBodyCount];

        
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

    ~SoaPhysicsSystemState()
    {
        Dispose(false);    
    }
}