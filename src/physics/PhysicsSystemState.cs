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
    ///     The entity id's for all physics bodies.
    /// </summary>
    /// <remarks>
    ///     Remarks: Use a <c>physicsBodyIndex</c> to access elements.
    /// </remarks>
    public GenId[] EntityIds;

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
    ///     The local to world-space transforms for all physics bodies.
    /// </summary>
    /// <remarks>
    ///     Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Transform Transforms;

    /// <summary>
    ///     The positions of the physics bodies from the previous step.
    /// </summary>
    /// <remarks>
    ///     Use a <c>physicsBodyIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Vector2 PreviousStepPositions;

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
    ///     Categories of all physics bodies when being put into the bvh.
    /// </summary>
    public int[] BvhCategories;

    /// <summary>
    ///     Maps a bvh leaf indice onto a physics body.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed by <c>leafIndex</c>.
    /// <code>
    /// int physicsBodyIndex = BvhLeafIndices[leafIndex];
    /// </code>
    /// </remarks>
    public int[] BvhLeafIndices;

    /// <summary>
    ///     The padding for each pysics body leaf AABB in the bvh.
    /// </summary>
    public float[] BvhLeafPaddings;

    /// <summary>
    ///     The scratch buffer for retrieving overlap data from the bvh. 
    /// </summary>
    public CategorisedLeafOverlaps Overlaps;

    /// <summary>
    ///     The indices in the <c>CollisionManifoldState</c> of collider collisions to resolve in the current substep.
    /// </summary>
    public CategorisedOverlapArray<int> SubStepColliderCollisionsToResolve;

    /// <summary>
    ///     The indices in the <c>CollisionManifoldState</c> of rigidbody collisions to resolve in the current substep.
    /// </summary>
    public StackArray<int> SubStepRigidBodyCollisionsToResolve;

    /// <summary>
    ///     The indices of physics bodies that are currently active.
    /// </summary>
    /// <remarks>
    ///     Remarks: this array contains a <c>Nil</c> element.
    /// </remarks>
    public SwapBackArray<int> ActiveBodies;

    /// <summary>
    ///     The indices of physics bodies that point to an element in the <c>ActiveBodies</c> array.
    /// </summary>
    /// <remarks>
    ///     Remarks: this array contains a <c>Nil</c> element.
    /// </remarks>
    public int[] ActiveBodiesDenseIndices;

    /// <summary>
    ///     Whether a rotational response is enabled for a physics body.
    /// </summary>    
    /// <remarks>
    ///     Remarks: Elements should be accessed by <c>physicsBodyIndex</c>.
    /// </remarks>
    public bool[] RotationalResponses;

    /// <summary>
    ///     The amount of allocated solid polygon colliders.
    /// </summary>
    public int SolidPolygonColliderCount;

    /// <summary>
    ///     The amount of allocated trigger polygon colliders.
    /// </summary>
    public int TriggerPolygonColliderCount;

    /// <summary>
    ///     The amount of allocated kinematic polygon colliders.
    /// </summary>
    public int KinematicPolygonColliderCount;

    /// <summary>
    ///     The amount of allocated solid polygon rigidbodies.
    /// </summary>
    public int SolidPolygonRigidBodyCount;

    /// <summary>
    ///     The amount of allocated trigger polygon rigidbodies.
    /// </summary>
    public int TriggerPolygonRigidBodyCount;

    /// <summary>
    ///     The amount of allocated kinematic polygon rigidbodies.
    /// </summary>
    public int KinematicPolygonRigidBodyCount;

    /// <summary>
    ///     The amount of allocated solid circle colliders.
    /// </summary>
    public int SolidCircleColliderCount;

    /// <summary>
    ///     The amount of allocated trigger circle colliders.
    /// </summary>
    public int TriggerCircleColliderCount;

    /// <summary>
    ///     The amount of allocated kinematic circle colliders.
    /// </summary>
    public int KinematicCircleColliderCount;

    /// <summary>
    ///     The amount of allocated solid circle rigidbodies.
    /// </summary>
    public int SolidCircleRigidBodyCount;
    
    /// <summary>
    ///     The amount of allocated trigger circle rigidbodies.
    /// </summary>
    public int TriggerCircleRigidBodyCount;

    /// <summary>
    ///     The amount of allocated kinematic circle rigidbodies.
    /// </summary>
    public int KinematicCircleRigidBodyCount;

    /// <summary>
    ///     The amount of allocated solid capsule rigidbodies.
    /// </summary>
    public int SolidCapsuleRigidBodyCount;

    /// <summary>
    ///     The amount of allocated kinematic capsule rigidbodies.
    /// </summary>
    public int KinematicCapsuleRigidBodyCount;

    /// <summary>
    ///     The amount of allocated trigger capsule rigidbodies.
    /// </summary>
    public int TriggerCapsuleRigidBodyCount;

    /// <summary>
    ///     The amount of solid capsule colliders.
    /// </summary>
    public int SolidCapsuleColliderCount;

    /// <summary>
    ///     The amount of kinematic capsule colliders.
    /// </summary>
    public int KinematicCapsuleColliderCount;

    /// <summary>
    ///     The amount of trigger capsule colliders.
    /// </summary>
    public int TriggerCapsuleColliderCount;


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
    public CollisionManifoldState CollisionManifoldState;




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
    public Stopwatch BvhStopwatch;

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

    /// <summary>
    ///     Toggle for drawing bvh leaves.
    /// </summary>
    public bool DrawLeaves;




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
        Bvh = new(physicsBodyCount);
        Overlaps = new(PhysicsBodyCategory.Count, maxCollisions);
        CollisionManifoldState = new(physicsBodyCount);
        SubStepColliderCollisionsToResolve  = new(CollisionResolutionCategory.Count, maxCollisions);
        SubStepRigidBodyCollisionsToResolve = new(maxCollisions);
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
        BvhCategories               = new int[physicsBodyCount];
        EntityIds                   = new GenId[physicsBodyCount];
        BvhLeafIndices              = new int[physicsBodyCount];
        PreviousStepPositions       = new(physicsBodyCount);
        BvhLeafPaddings             = new float[physicsBodyCount];
        ActiveBodies                = new(physicsBodyCount);
        ActiveBodiesDenseIndices    = new int[physicsBodyCount];
        RotationalResponses          = new bool[physicsBodyCount];

        // Debug diagnostic stopwatches.
        FixedUpdateStepStopwatch = new();
        FixedUpdateSubStepStopwatch = new();
        SyncTransformsToEntitiesStopwatch = new();
        IntegrateBodyPropertiesStopwatch = new();
        RigidBodyMovementStepStopwatch = new();
        TransformPhysicsBodiesStopwatch = new();
        BvhStopwatch = new();
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

        // append Nil element.
        SwapBackArray.Append(ActiveBodies, 0);
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