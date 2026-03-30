using System;
using System.Collections.Generic;
using System.Diagnostics;
using Howl.DataStructures;
using Howl.DataStructures.Bvh;
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
    /// The type flags for all physics bodies.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public PhysicsBodyFlags[] Flags;

    /// <summary>
    /// The local-space vertices for all physics bodies.
    /// </summary>
    /// <remarks>
    /// Use a <c>vertexIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Vector2 LocalVertices;
    
    /// <summary>
    /// The world-space vertices for all physics bodies.  
    /// </summary>
    /// <remarks>
    /// Use a <c>vertexIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Vector2 WorldVertices;

    /// <summary>
    /// The local to world-space transforms for all physics bodies.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Transform Transforms;

    /// <summary>
    /// The force values of all all physics bodies that will be applied in the rigidbody movement step.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Vector2 Forces;

    /// <summary>
    /// the linear velocity values for all physics bodies.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Vector2 LinearVelocities;

    /// <summary>
    /// Gets and sets the centroid of a physics body.
    /// </summary>
    public Soa_Vector2 Centroids;

    /// <summary>
    /// Gets the max of vector of a physics body's AABB. 
    /// </summary>
    /// <remarks>
    /// Note: this collection is indexed by the body index; not a body's vertex indices.
    /// </remarks>
    public Soa_Vector2 MaxAABBVertices;

    /// <summary>
    /// Gets the min vector of a physics body's AABB.
    /// </summary>
    /// <remarks>
    /// Note: this collection is indexed by the body index; not a body's vertex indices.
    /// </remarks>
    public Soa_Vector2 MinAABBVertices;

    /// <summary>
    /// the physics material's for all physics bodies.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public Soa_PhysicsMaterial PhysicsMaterials;

    /// <summary>
    /// the angular velocity values for all physics bodies.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public float[] AngularVelocities;

    /// <summary>
    /// The mass values of all physics bodies.
    /// </summary>
    /// <remarks>
    /// Mass is relative to world-space. Use <c>physicsBodyIndex</c> integer to access elements
    /// </remarks>
    public float[] Masses;

    /// <summary>
    /// The inverse mass values of all physics bodies.
    /// </summary>
    /// <remarks>
    /// Inverse mass is relative to world-space. Use <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public float[] InverseMasses;

    /// <summary>
    /// The local-space width values for all physics bodies.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public float[] LocalWidths;

    /// <summary>
    /// The local-space height values for all physics bodies in the system.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public float[] LocalHeights;

    /// <summary>
    /// The local-space radius values for all physics bodies.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public float[] LocalRadii;

    /// <summary>
    /// The world-space radius values for all physics bodies.
    /// </summary>
    /// <remarks>
    /// This value is updated during the integrate physics bodies step of the physics simulation 
    /// to reflect the current scale of te body in world-space.
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public float[] WorldRadii;

    /// <summary>
    /// The rotational inertia  values of all physics bodies.
    /// </summary>
    /// <remarks>
    /// Rotational inertia is relative to world-space. Use <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public float[] RotationalInertia;

    /// <summary>
    /// Gets and sets the inverse rotational inertia of a physics body.
    /// </summary>
    /// <remarks>
    /// Inverse rotational intertia is relative to world-space. Use <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public float[] InverseRotationalInertia;
    
    /// <summary>
    /// The indices in the vertices collection that point to the first vertex of a given physics body.
    /// </summary>
    /// <remarks>
    /// Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public int[] FirstVertexIndices;

    /// <summary>
    /// the indices in the vertices vertices collection that point to the next vertex of a given vertex index..
    /// </summary>
    /// <remarks>
    /// Use a <c>vertexIndex</c> integer to access elements.
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
    public Soa_BoundingVolumeHierarchy Bvh;

    /// <summary>
    /// Gets the collision manifold.
    /// </summary>
    public CollisionManifoldNew CollisionManifold;




    /*******************
    
        Debug Diagnostic Stopwatches.
    
    ********************/



    /// <summary>
    /// Gets and sets the stopwatch for timing a physics system fixed-update step.
    /// </summary>
    public Stopwatch FixedUpdateStepStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for timing a physics system fixed-update substep.
    /// </summary>
    public Stopwatch FixedUpdateSubStepStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for syncing physics bodies to their associated entities.
    /// </summary>
    public Stopwatch SyncTransformsToEntitiesStopwatch;

    /// <summary>
    /// The diagnostic stopwatch for the IntegrateBodyProperties step.
    /// </summary>
    public Stopwatch IntegrateBodyPropertiesStopwatch;

    /// <summary>
    /// Gets and sets the debug stop watch for timing a rigidbody movement step.
    /// </summary>
    public Stopwatch RigidBodyMovementStepStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for transforming physics bodies.
    /// </summary>
    public Stopwatch TransformPhysicsBodiesStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for timing a bvh reconstruction step.
    /// </summary>
    public Stopwatch BvhReconstructionStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for timing a spatial pair filtering step.
    /// </summary>
    public Stopwatch FilterBvhIntoCollisionManifoldStopwatch;

    /// <summary>
    /// Gets and sets the debug stopwatch for timing a collision intersect step.
    /// </summary>
    public Stopwatch FindCollisionsStopwatch;

    /// <summary>
    /// Gets and sets the debug stopwatch for timing a collision resolution step.
    /// </summary>
    public Stopwatch ColliderCollisionResolutionStopwatch;

    /// <summary>
    /// Gets and sets the debug stopwatch for timing a collision resolution step.
    /// </summary>
    public Stopwatch RigidBodyCollisionResolutionStepStopwatch;

    /// <summary>
    /// Gets and sets the stopwatch for timing a collision manifold sort step.
    /// </summary>
    public Stopwatch CollisionManifoldSortStopwatch;


    /// <summary>
    /// Gets and sets the stopwatch for syncing entities to their associated physics bodies.
    /// </summary>
    public Stopwatch SyncEntitiesToPhysicsBodiesStopwatch;





    /*******************
    
        Debug Colours.
    
    ********************/




    /// <summary>
    /// Gets and sets the debug draw colour for the dynamic-bodies.
    /// </summary>
    public Colour DynamicPhysicsBodyColour;

    /// <summary>
    /// Gets and sets the debug draw colour for the trigger-bodies.
    /// </summary>
    public Colour TriggerPhysicsBodyColour;

    /// <summary>
    /// Gets and sets the debug draw colour for kinematic-bodies.
    /// </summary>
    public Colour KinematicPhysicsBodyColour;

    /// <summary>
    /// Gets and sets the debug draw colour for trigger-bodies when triggered.
    /// </summary>
    public Colour TriggeredPhysicsBodyColour;

    /// <summary>
    /// Gets and sets the debug draw colour for AABB's.
    /// </summary>
    public Colour AABBColour;

    /// <summary>
    /// Gets and sets the fallback debug draw colour for colliders.
    /// </summary>
    public Colour FallbackPhysicsBodyColour;

    /// <summary>
    /// Gets and sets the debug draw colour for inactive colliders.
    /// </summary>
    public Colour InactivePhysicsBodyColour;

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

    /// <summary>
    /// Gets and sets the debug draw colour for linear velocities.
    /// </summary>
    public Colour LinearVelocityColour;

    /// <summary>
    /// Gets and sets the debug draw colour for positions.
    /// </summary>
    public Colour PositionColour;

    /// <summary>
    /// Gets and sets the debug draw colour for centroids. 
    /// </summary>
    public Colour CentroidColour;

    /// <summary>
    /// Gets and sets the debug draw colour for a collision owner.
    /// </summary>
    public Colour CollisionOwnerColour;

    /// <summary>
    /// Gets and sets the debug draw colour for a collision other.
    /// </summary>
    public Colour CollisionOtherColour;

    /// Gets and sets the debug draw colour for a normal vector.
    public Colour NormalColour;

    /// <summary>
    /// The debug draw colour of the boudning volume hierarchy's branches.
    /// </summary>
    public Colour BvhBranchColour;




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

    /// <summary>
    /// Gets and sets the max physics body count of this physics system state.
    /// </summary>
    public int MaxPhysicsBodyCount;




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
    /// Gets and sets whether or not to draw collision information.
    /// </summary>
    public bool DrawCollisionInformation;

    /// <summary>
    /// Gets and sets whether or not to draw linear velocities for each body.
    /// </summary>
    public bool DrawLinearVelocities;

    /// <summary>
    /// Gets and sets whether or not to draw positions for each body.
    /// </summary>
    public bool DrawPositions;

    /// <summary>
    /// Gets and sets whether or not to draw centroids for each body.
    /// </summary>
    public bool DrawCentroids;




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
        MaxPhysicsBodyCount = physicsBodyCount;

        // Utility.
        Bvh = new(physicsBodyCount, maxCollisions);
        CollisionManifold = new(maxCollisions);

        // Physics body data.
        Flags                       = new PhysicsBodyFlags[physicsBodyCount];
        LocalVertices                    = new Soa_Vector2(physicsBodyVerticesCount);
        WorldVertices         = new Soa_Vector2(physicsBodyVerticesCount);
        Transforms                  = new Soa_Transform(physicsBodyCount);
        Forces                      = new Soa_Vector2(physicsBodyCount);
        LinearVelocities            = new Soa_Vector2(physicsBodyCount);
        Centroids                   = new Soa_Vector2(physicsBodyCount);
        MaxAABBVertices             = new Soa_Vector2(physicsBodyCount);
        MinAABBVertices             = new Soa_Vector2(physicsBodyCount);
        PhysicsMaterials            = new Soa_PhysicsMaterial(physicsBodyCount);
        AngularVelocities           = new float[physicsBodyCount];
        Masses                      = new float[physicsBodyCount];
        InverseMasses               = new float[physicsBodyCount];
        LocalWidths                      = new float[physicsBodyCount];
        LocalHeights                     = new float[physicsBodyCount];
        LocalRadii                       = new float[physicsBodyCount];
        WorldRadii            = new float[physicsBodyCount];
        RotationalInertia           = new float[physicsBodyCount];
        InverseRotationalInertia    = new float[physicsBodyCount];
        FirstVertexIndices          = new int[physicsBodyCount];
        NextVertexIndices           = new int[physicsBodyVerticesCount];
        Generations                 = new int[physicsBodyCount];
        FreePhysicsBodyIndex        = new();
        FreeVertexIndex             = new();

        // Debug diagnostic stopwatches.
        FixedUpdateStepStopwatch = new();
        FixedUpdateSubStepStopwatch = new();
        SyncTransformsToEntitiesStopwatch = new();
        IntegrateBodyPropertiesStopwatch = new();
        RigidBodyMovementStepStopwatch = new();
        TransformPhysicsBodiesStopwatch = new();
        BvhReconstructionStopwatch = new();
        FilterBvhIntoCollisionManifoldStopwatch = new();
        FindCollisionsStopwatch = new();
        ColliderCollisionResolutionStopwatch = new();
        RigidBodyCollisionResolutionStepStopwatch = new();
        CollisionManifoldSortStopwatch = new();
        SyncEntitiesToPhysicsBodiesStopwatch = new();

        // debug colours
        DynamicPhysicsBodyColour        = Colour.Green;
        KinematicPhysicsBodyColour      = Colour.Orange;
        TriggerPhysicsBodyColour        = Colour.LightBlue;
        TriggeredPhysicsBodyColour      = Colour.Red;
        AABBColour                      = new Colour(Colour.Pink.R, Colour.Pink.G, Colour.Pink.B, 50);
        FallbackPhysicsBodyColour       = Colour.White;
        InactivePhysicsBodyColour       = Colour.Black;
        BvhLeafAABBColour               = Colour.Purple;
        BvhBranchAABBColour             = Colour.Yellow;
        ContactPointColour              = Colour.Red;
        LinearVelocityColour            = Colour.White;
        PositionColour                  = Colour.White;
        CentroidColour                  = Colour.White;
        CollisionOwnerColour            = Colour.Green;
        CollisionOtherColour            = Colour.LightBlue;
        NormalColour                    = Colour.Red;
        BvhBranchColour                 = Colour.Yellow;


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