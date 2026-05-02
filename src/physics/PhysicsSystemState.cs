using System;
using System.Diagnostics;
using Howl.Collections;
using Howl.DataStructures.Bvh;
using Howl.Graphics;
using Howl.Math;

namespace Howl.Physics;

public sealed class PhysicsSystemState
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
    public FsSoa_Vector2 LocalVertices;
    
    /// <summary>
    /// The world-space vertices for all physics bodies.  
    /// </summary>
    /// <remarks>
    /// Use a <c>vertexIndex</c> integer to access elements.
    /// </remarks>
    public FsSoa_Vector2 WorldVertices;

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
    /// The generation of a physics body id.
    /// </summary>
    public int[] Generations;

    /// <summary>
    /// Gets and sets the vertex entry indices available for reuse and allocation in LocalRadii.
    /// </summary>
    public StackArray<int> FreeVertexEntries;

    /// <summary>
    ///     The indices of all physics bodies in a bvh tree.
    /// </summary>
    /// <remarks>
    ///     Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public int[] BvhIndices;




    /*******************
    
        Utility.
    
    ********************/




    /// <summary>
    ///     The gen-id allocator for all phsyics bodies.
    /// </summary>
    public EntityRegistry Entities;

    /// <summary>
    ///     Gets the bounding volume hierarchy for a collision system.
    /// </summary>
    public BoundingVolumeHierarchy Bvh;

    /// <summary>
    ///     The collision manifold.
    /// </summary>
    public CollisionManifoldStateNew CollisionManifoldState;




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





    public PhysicsSystemState(int physicsBodyCount, int maxPhysicsBodyVerticeCount)
    {
        MaxPhysicsBodyCount = physicsBodyCount;

        // Utility.
        int maxCollisions = physicsBodyCount*physicsBodyCount;
        Bvh = new(physicsBodyCount, maxCollisions);
        CollisionManifoldState = new(physicsBodyCount);
        Entities = new(physicsBodyCount);

        // Physics body data.
        Flags                       = new PhysicsBodyFlags[physicsBodyCount];
        LocalVertices               = new FsSoa_Vector2(maxPhysicsBodyVerticeCount, physicsBodyCount);
        WorldVertices               = new FsSoa_Vector2(maxPhysicsBodyVerticeCount, physicsBodyCount);
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
        LocalWidths                 = new float[physicsBodyCount];
        LocalHeights                = new float[physicsBodyCount];
        LocalRadii                  = new float[physicsBodyCount];
        WorldRadii                  = new float[physicsBodyCount];
        RotationalInertia           = new float[physicsBodyCount];
        InverseRotationalInertia    = new float[physicsBodyCount];
        Generations                 = new int[physicsBodyCount];
        FreeVertexEntries           = new(physicsBodyCount);
        BvhIndices                  = new int[physicsBodyCount];

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

        for(int i = physicsBodyCount-1; i > 0; i--) // dont push zero as that is Nil
            StackArray.Push(FreeVertexEntries, i); // push all available indices to the stack.
    }

    /// <summary>
    ///     Enforces a <c>Nil</c> entry for all underling arrays of a physics system state instance.
    /// </summary>
    /// <param name="state">the physics system state instance.</param>
    public static void EnforceNil(PhysicsSystemState state)
    {
        Nil.Enforce(state.Flags);
        FsSoa_Vector2.EnforceNil(state.LocalVertices);
        FsSoa_Vector2.EnforceNil(state.WorldVertices);    
        Soa_Transform.EnforceNil(state.Transforms);
        Soa_Vector2.EnforceNil(state.Forces);
        Soa_Vector2.EnforceNil(state.LinearVelocities);
        Soa_Vector2.EnforceNil(state.Centroids);
        Soa_Vector2.EnforceNil(state.MaxAABBVertices);
        Soa_Vector2.EnforceNil(state.MinAABBVertices);
        Soa_PhysicsMaterial.EnforceNil(state.PhysicsMaterials);
        Nil.Enforce(state.AngularVelocities);
        Nil.Enforce(state.Masses);
        Nil.Enforce(state.InverseMasses);
        Nil.Enforce(state.LocalWidths);
        Nil.Enforce(state.LocalHeights);
        Nil.Enforce(state.LocalRadii);
        Nil.Enforce(state.WorldRadii);
        Nil.Enforce(state.RotationalInertia);
        Nil.Enforce(state.InverseRotationalInertia); 
        Nil.Enforce(state.FreeVertexEntries.Data);
        Nil.Enforce(state.Generations);
    }




    /*******************
    
        Disposal.
    
    ********************/




    /// <summary>
    /// Disposes an physics system instance. instance.
    /// </summary>
    /// <param name="state">the physics system state to dispose.</param>
    public static void Dispose(PhysicsSystemState state)
    {
        if (state.IsDisposed)
            return;
        
        state.IsDisposed = true;
        GC.SuppressFinalize(state);        
    }

    ~PhysicsSystemState()
    {
        Dispose(this);
    }
}