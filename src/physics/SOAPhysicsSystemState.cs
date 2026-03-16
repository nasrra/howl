using System;
using System.Collections.Generic;
using System.Diagnostics;
using Howl.DataStructures;
using Howl.ECS;
using Howl.Graphics;
using Howl.Math;

namespace Howl.Physics;

public sealed class SoaPhysicsSystemState : IDisposable
{




    /*******************
    
        Phsyics Body Data.
    
    ********************/




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
    public Soa_Vector2 Vertices;
    
    /// <summary>
    /// Gets and sets the transformed vertice positions of all physics bodies.  
    /// </summary>
    public Soa_Vector2 TransformedVertices;

    /// <summary>
    /// Gets and sets the transforms of all physics bodies.
    /// </summary>
    public Soa_Transform Transforms;

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
    /// Gets and sets the first index of a physics bodies first vertex of physics body's shape.
    /// </summary>
    public int[] FirstVertexIndices;

    /// <summary>
    /// The index of the next vertex of a given shape's vertex.
    /// </summary>
    /// <remarks>
    /// Note: the next index for a given shape are stored in a circular intrusive linked list; 
    /// meaning that the next vertice index of the final vertex will be the first vertex index. 
    /// </remarks>
    public int[] NextVertexIndices;
    
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




    /*******************
    
        Utility.
    
    ********************/




    /// <summary>
    /// Gets the bounding volume hierarchy for a collision system.
    /// </summary>
    public BoundingVolumeHierarchy Bvh;

    /// <summary>
    /// Gets the collision manifold.
    /// </summary>
    public CollisionManifoldNew CollisionManifold;




    /*******************
    
        Debug Diagnostic Stopwatches.
    
    ********************/




    /// <summary>
    /// Gets and sets the stopwatch for timing a physics system fixed-update substep.
    /// </summary>
    public Stopwatch FixedUpdateSubStepStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for timing a physics system fixed-update step.
    /// </summary>
    public Stopwatch FixedUpdateStepStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for timing a spatial pair filtering step.
    /// </summary>
    public Stopwatch FilterBvhIntoCollisionManifoldStopwatch;

    /// <summary>
    /// Gets and sets the debug stopwatch for timing a collision intersect step.
    /// </summary>
    public Stopwatch FindColliderPairsStopwatch;

    /// <summary>
    /// Gets and sets the debug stopwatch for timing a collision resolution step.
    /// </summary>
    public Stopwatch ColliderCollisionResolutionStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for timing a bvh reconstruction step.
    /// </summary>
    public Stopwatch BvhReconstructionStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for timing a collision manifold sort step.
    /// </summary>
    public Stopwatch CollisionManifoldSortStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for syncing physics bodies to their associated entities.
    /// </summary>
    public Stopwatch SyncPhysicsBodiesToEntitiesStopwatch;

    /// <summary>
    /// Gets and sets the debug stopwatch for timing a collision resolution step.
    /// </summary>
    public Stopwatch RigidBodyCollisionResolutionStepStopwatch;

    /// <summary>
    /// Gets and sets the debug stop watch for timing a movement step.
    /// </summary>
    public Stopwatch MovementStepStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for syncing entities to their associated physics bodies.
    /// </summary>
    public Stopwatch SyncEntitiesToPhysicsBodiesStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for vertices with their associated transform.
    /// </summary>
    public Stopwatch TrasformPhysicsBodyVerticesStopwatch;




    /*******************
    
        Debug Colours.
    
    ********************/




    /// <summary>
    /// Gets and sets the debug draw colour for the solid-colliders.
    /// </summary>
    public Colour SolidColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for the trigger-colliders.
    /// </summary>
    public Colour TriggerColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for kinematic-colliders.
    /// </summary>
    public Colour KinematicColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for trigger colliders when triggered.
    /// </summary>
    public Colour TriggerColliderTriggeredColour;

    /// <summary>
    /// Gets and sets the debug draw colour for AABB's.
    /// </summary>
    public Colour AABBColour;

    /// <summary>
    /// Gets and sets the fallback debug draw colour for colliders.
    /// </summary>
    public Colour FallbackColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for inactive colliders.
    /// </summary>
    public Colour InactiveColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for bvh-tree leaf aabb's.
    /// </summary>
    public Colour BvhLeafAABBColour;

    /// <summary>
    /// Gets and sets the debug draw colour for bvh-treee branch aabb's
    /// </summary>
    public Colour BvhBranchAABBColour;

    /// <summary>
    /// Gets and sets the debug draw colour for contact-points;
    /// </summary>
    public Colour ContactPointColour;




    /*******************
    
        Counters.
    
    ********************/




    /// <summary>
    /// Gets and sets the count of allocated physics body stored in this physics system state.
    /// </summary>
    public int AlloctedPhysicsBodyCount = 0;

    /// <summary>
    /// Gets and sets the maximum amount of vertices a physics body can have.
    /// </summary>
    /// <remarks>
    /// Note: this value should never shrink - only enlargen;
    /// undefined behaviour may occur when the value is set to
    /// lower then its stored value.
    /// </remarks>
    public int MaxPhysicsBodyVertexCount;




    /*******************
    
        Debug Draw Flags.
    
    ********************/




    /// <summary>
    /// Gets and sets whether or not to draw collider wireframes.
    /// </summary>
    public bool DrawColliderWireframes;

    /// <summary>
    /// Gets and sets whether or not to draw collider AABB wireframes.
    /// </summary>
    public bool DrawAABBWireframes;

    /// <summary>
    /// Gets and sets whether or not to draw bvh branches.
    /// </summary>
    public bool DrawBvhBranches;

    /// <summary>
    /// Gets and sets whether or not to draw contact points.
    /// </summary>
    public bool DrawContactPoints;




    /*******************
    
        Header.
    
    ********************/




    /// <summary>
    /// Gets and sets the gravity force.
    /// </summary>
    public float Gravity = 9.81f;

    /// <summary>
    /// Gets and sets the direction of gravity.
    /// </summary>
    public Vector2 GravityDirection = Vector2.Down;




    /*******************
    
        Disposal.
    
    ********************/




    /// <summary>
    /// Gets and sets whether or not this instance has been disposed.
    /// </summary>
    public bool IsDisposed;





    public SoaPhysicsSystemState(int physicsBodyCount, int physicsBodyVerticesCount, int maxPhysicsBodyVerticeCount, int maxCollisions)
    {
        // Utility.
        Bvh = new();
        CollisionManifold = new(maxCollisions);

        // Physics body data.
        Flags                   = new PhysicsBodyFlags[physicsBodyCount];
        Widths                  = new float[physicsBodyCount];
        Heights                 = new float[physicsBodyCount];
        Radii                   = new float[physicsBodyCount];
        Vertices                = new Soa_Vector2(physicsBodyCount);
        TransformedVertices     = new Soa_Vector2(physicsBodyCount);
        Transforms              = new Soa_Transform(physicsBodyCount);
        StaticFrictions         = new float[physicsBodyCount];
        KineticFrictions        = new float[physicsBodyCount];
        TransformedRadii        = new float[physicsBodyCount];
        NextVertexIndices        = new int[physicsBodyVerticesCount];
        Generations             = new int[physicsBodyCount];
        FirstVertexIndices       = new int[physicsBodyCount];
        FreePhysicsBodyIndex    = new();
        FreeVertexIndex         = new();

        // Debug diagnostic stopwatches.
        FixedUpdateSubStepStopwatch = new();
        FixedUpdateStepStopwatch = new();
        FindColliderPairsStopwatch = new();
        ColliderCollisionResolutionStopwatch = new();
        BvhReconstructionStopwatch = new();
        CollisionManifoldSortStopwatch = new();
        SyncPhysicsBodiesToEntitiesStopwatch = new();
        SyncEntitiesToPhysicsBodiesStopwatch = new();
        MovementStepStopwatch = new();
        FilterBvhIntoCollisionManifoldStopwatch = new();
        RigidBodyCollisionResolutionStepStopwatch = new();
        TrasformPhysicsBodyVerticesStopwatch = new();

        // debug colours
        SolidColliderColour             = Colour.Green;
        KinematicColliderColour         = Colour.Orange;
        TriggerColliderColour           = Colour.LightBlue;
        TriggerColliderTriggeredColour  = Colour.Red;
        AABBColour                      = new Colour(Colour.Pink.R, Colour.Pink.G, Colour.Pink.B, 50);
        FallbackColliderColour          = Colour.White;
        InactiveColliderColour          = Colour.Black;
        BvhLeafAABBColour               = Colour.Purple;
        BvhBranchAABBColour             = Colour.Yellow;
        ContactPointColour              = Colour.Red;

        // Counters.
        MaxPhysicsBodyVertexCount = maxPhysicsBodyVerticeCount;

        for(int i = physicsBodyCount-1; i >= 0; i--)
            FreePhysicsBodyIndex.Push(i); // push all available indices to the stack.

        for(int i = physicsBodyVerticesCount-1; i >= 0; i--)
            FreeVertexIndex.Push(i); // push all available indices to the stack.
    }




    /*******************
    
        Disposal.
    
    ********************/




    /// <summary>
    /// Throws an exception if an physics system state instance is disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public static void ThrowIfDisposed(SoaPhysicsSystemState state)
    {
        if (state.IsDisposed)
            throw new ObjectDisposedException($"{nameof(SoaPhysicsSystemState)}");
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(this);
    }

    /// <summary>
    /// Disposes an physics system instance. instance.
    /// </summary>
    /// <param name="state">the physics system state to dispose.</param>
    public static void Dispose(SoaPhysicsSystemState state)
    {
        if (state.IsDisposed)
            return;
        
        state.IsDisposed = true;
        GC.SuppressFinalize(state);        
    }

    ~SoaPhysicsSystemState()
    {
        Dispose(this);
    }
}