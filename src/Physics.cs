using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Howl.Collections;
using Howl.DataStructures;
using Howl.DataStructures.Bvh;
using Howl.Graphics;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Physics;
using Howl.Systems;

namespace Howl;

public static class PhysicsNew
{




    /******************
    
        Constants.
    
    *******************/




    public const float RectangleRotationalInertia = 0.0833333333333f;
    public const float CircleRotationalInertia = 0.5f;
    public const float MinBodySize = float.Epsilon;
    public const float MaxBodySize = float.MaxValue;
    public const int MaxCollisionContactPoints = 2;

    public static readonly System.Numerics.Vector<float> VectorRectangleRotationalInertia = new(RectangleRotationalInertia);
    public static readonly System.Numerics.Vector<float> VectorCircleRotationalInertia = new(CircleRotationalInertia);




    /*******************
    
        Debug Colours.
    
    ********************/




    public static Colour DynamicShapeColour = Colour.Green;
    public static Colour PassiveTriggerShapeColour = Colour.LightBlue;
    public static Colour KinematicShapeColour = Colour.Orange;
    public static Colour ActiveTriggerShapeColour = Colour.Red;
    public static Colour AABBColour = Colour.Pink;
    public static Colour FallbackShapeColour = Colour.White;
    public static Colour InactivePhysicsBodyColour = Colour.Black;
    public static Colour BvhLeafAABBColour = Colour.Green;
    public static Colour BvhBranchAABBColour = Colour.White;
    public static Colour ContactPointColour = Colour.Red;
    public static Colour LinearVelocityColour = Colour.White;
    public static Colour PositionColour = Colour.White;
    public static Colour CentroidColour = Colour.Yellow;
    public static Colour CollisionOwnerColour = Colour.Blue;
    public static Colour CollisionNormalColour = Colour.Purple;




    /******************
    
        Header
    
    *******************/




    public enum MovementStepConfig : byte
    {
        LinearVelocityOnly,
        DisplacementOnly,
        Full
    }




    /******************
    
        Entity Type.
    
    *******************/




    public enum EntityType : int
    {
        /**
            -------------------------
            |       Connections:    |
            |-----------------------|
            | Shape | Body  | Joint |
            |-------|-------|-------|
            |   0   |   1   |   0   |
            |-----------------------| 
        **/
        Shape,


        /**
            -------------------------
            |       Connections:    |
            |-----------------------|
            | Shape | Body  | Joint |
            |-------|-------|-------|
            |   N   |   0   |   N   |
            |-----------------------| 
        **/
        Body,

        /**
            -------------------------
            |       Connections:    |
            |-----------------------|
            | Shape | Body  | Joint |
            |-------|-------|-------|
            |   0   |   2   |   0   |
            |-----------------------| 
        **/
        Joint    
    }




    /******************
    
        State
    
    *******************/




    public class State
    {




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




        /******************
        
            Entity Data.
        
        *******************/




        /// <summary>
        ///     The base vertices for all shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>vertexIndex</c>.</para>
        /// </remarks>
        public FsSoa_Vector2 BaseVertices;

        /// <summary>
        ///     The world-space vertices for all shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>vertexIndex</c>.</para>
        /// </remarks>    
        public FsSoa_Vector2 WorldVertices;

        /// <summary>
        ///     The local-space transforms for all entities.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Soa_Transform LocalTransforms;

        /// <summary>
        ///     The global-space transforms for all entities.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Soa_Transform GlobalTransforms;

        /// <summary>
        ///     The positions of entities from the previous step.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c></para>
        /// </remarks>
        public Soa_Vector2 PreviousStepPositions;

        /// <summary>
        ///     The force values that will be applied in to rigidbodies.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via<c>entityIndex</c>.</para>
        /// </remarks>
        public Soa_Vector2 Forces;

        /// <summary>
        ///     The linear velocity values for all rigidbodies.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para></para>
        /// </remarks>
        public Soa_Vector2 LinearVelocities;

        /// <summary>
        ///     The centroids of all shapes; in global-space.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Soa_Vector2 Centroids;

        /// <summary>
        ///     The center of masses of all bodies; relative to their global position.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Soa_Vector2 LocalCentersOfMass;

        /// <summary>
        ///     The Axis Aligned Bounding Boxes of all shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Soa_Aabb Aabbs;

        /// <summary>
        ///     The physics materials for all shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Soa_PhysicsMaterial PhysicsMaterials;

        /// <summary>
        ///     the angular velocities for all rigidbodies.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] AngularVelocities;

        /// <summary>
        ///     The mass values of all rigid bodies and their associated shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] Masses;

        /// <summary>
        ///     The inverse mass values of all rigid bodies and their associated shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] InverseMasses;

        /// <summary>
        ///     The base width values of all rectangle shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] BaseWidths;

        /// <summary>
        ///     The base height values of all rectangle shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] BaseHeights;

        /// <summary>
        ///     The base radii values of all circle shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] BaseRadii;

        /// <summary>
        ///     The world-space radii values of all circle shapes.
        /// </summary>
        /// <remarks>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] WorldRadii;

        /// <summary>
        ///     The rotational inertia values of all rigidbodies.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] RotationalInertia;

        /// <summary>
        ///     The inverse rotational inertia values of all rigidbodies.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] InverseRotationalInertia;

        /// <summary>
        ///     The generations of all bodies.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public int[] Generations;

        /// <summary>
        ///     The categories of all shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public int[] Categories;

        /// <summary>
        ///     The bvh indices of all shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public int[] BvhLeafIndices;

        /// <summary>
        ///     The padding of all shapes to apply to their AABB when inserted into the bvh..
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] BvhLeafPaddings;

        /// <summary>
        ///     The shape value of all rigid shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Shape.Rigid.ShapeType[] ShapeTypes;

        /// <summary>
        ///     Whether a rigidbody uses rotational response.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public bool[] RotationalResponses;

        /// <summary>
        ///     The scratch buffer for retrieving overlap data from the bvh. 
        /// </summary>
        public CategorisedLeafOverlaps OverlapsScratchBuffer;

        /// <summary>
        ///     The indices in the <c>CollisionManifoldState</c> of collider collisions to resolve in the current substep.
        /// </summary>
        public CategorisedOverlapArray<int> SubStepShapeCollisionsToResolve;

        /// <summary>
        ///     The indices in the <c>CollisionManifoldState</c> of rigidbody collisions to resolve in the current substep.
        /// </summary>
        public CategorisedOverlapArray<int> SubStepRigidShapeCollisionsToResolve;

        /// <summary>
        ///     The types of all entities.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public EntityType[] EntityTypes;

        /// <summary>
        ///     Whether or not an entity is active.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public bool[] Active;

        /// <summary>
        ///     Whether or not a body is gravity affected.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public bool[] GravityAffected;

        /// <summary>
        ///     All bodies shape collision displacement vectors. 
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Soa_Vector2 ShapeCollisionDisplacements;

        /// <summary>
        ///     The indices of bodies that have been displaced this substep.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para></para>
        /// </remarks>
        public StackArray<int> DisplacedThisSubStep;




        /*******************
        
            Utility.
        
        ********************/




        /// <summary>
        ///     The gen-id allocator for all phsyics bodies.
        /// </summary>
        public EntityRegistry Entities;

        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Roots are the indices of <c>bodies</c>.</para>
        ///    <para>All subsequent children are the indices of <c>shapes</c>.</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public IntrusiveList.State BodyHierarchy;

        /// <summary>
        ///     Gets the bounding volume hierarchy for a collision system.
        /// </summary>
        public BoundingVolumeHierarchy Bvh;

        /// <summary>
        ///     The collision manifold.
        /// </summary>
        public CollisionManifoldState CollisionManifoldState;

        /// <summary>
        ///     Gets and sets the direction of gravity.
        /// </summary>
        public Vector2 GravityDirection = Vector2.Down;

        /// <summary>
        ///     Gets and sets the gravity force.
        /// </summary>
        public float Gravity = 9.81f;




        /******************
        
            Counts
        
        *******************/




        public int SolidPolygonColliderCount;
        public int TriggerPolygonColliderCount;
        public int KinematicPolygonColliderCount;
        public int SolidPolygonRigidBodyCount;
        public int TriggerPolygonRigidBodyCount;
        public int KinematicPolygonRigidBodyCount;
        public int SolidCircleColliderCount;
        public int TriggerCircleColliderCount;
        public int KinematicCircleColliderCount;
        public int SolidCircleRigidBodyCount;
        public int TriggerCircleRigidBodyCount;
        public int KinematicCircleRigidBodyCount;
        public int SolidCapsuleRigidBodyCount;
        public int KinematicCapsuleRigidBodyCount;
        public int TriggerCapsuleRigidBodyCount;
        public int SolidCapsuleColliderCount;
        public int KinematicCapsuleColliderCount;
        public int TriggerCapsuleColliderCount;




        public State(int maxEntities, int verticesPerShape)
        {
            int maxCollisions = maxEntities*maxEntities;

            {   // Debug diagnostic stopwatches.               

                FixedUpdateStepStopwatch = new();
                FixedUpdateSubStepStopwatch = new();
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
            }

            {   // Entity Data.
                
                BaseVertices = new FsSoa_Vector2(verticesPerShape, maxEntities);
                WorldVertices = new FsSoa_Vector2(verticesPerShape, maxEntities);
                LocalTransforms = new(maxEntities);
                GlobalTransforms = new(maxEntities);
                PreviousStepPositions = new(maxEntities);
                Forces = new(maxEntities);
                LinearVelocities = new(maxEntities);
                Centroids = new(maxEntities);
                Aabbs = new(maxEntities);
                PhysicsMaterials = new(maxEntities);
                AngularVelocities = new float[maxEntities];
                Masses = new float[maxEntities];
                InverseMasses = new float[maxEntities];
                BaseWidths = new float[maxEntities];
                BaseHeights = new float[maxEntities];
                BaseRadii = new float[maxEntities];
                WorldRadii = new float[maxEntities];
                RotationalInertia = new float[maxEntities];
                InverseRotationalInertia = new float[maxEntities];
                Generations = new int[maxEntities];
                Categories = new int[maxEntities];
                BvhLeafIndices = new int[maxEntities];
                BvhLeafPaddings = new float[maxEntities];
                ShapeTypes = new Shape.Rigid.ShapeType[maxEntities];
                RotationalResponses = new bool[maxEntities];
                EntityTypes = new EntityType[maxEntities];
                Active = new bool[maxEntities];
                ShapeCollisionDisplacements = new(maxEntities);
                GravityAffected = new bool[maxEntities];
                LocalCentersOfMass = new(maxEntities);
            }

            {   // Utility.
                
                Entities = new(maxEntities);
                Bvh = new(maxEntities);
                OverlapsScratchBuffer = new(Shape.Category.Count, maxCollisions);
                CollisionManifoldState = new(maxEntities);
                SubStepShapeCollisionsToResolve  = new(CollisionResolutionCategory.Count, maxCollisions);
                SubStepRigidShapeCollisionsToResolve = new(CollisionResolutionCategory.Count, maxCollisions);
                BodyHierarchy = new (maxEntities);
            }
        }
    }

    /// <summary>
    ///     Enforces a <c>Nil</c> entry for all underling arrays of a physics system state instance.
    /// </summary>
    /// <param name="state">the physics state instance.</param>
    public static void EnforceNil(State state)
    {
        throw new NotImplementedException();
    }



    /******************
    
        Node Type
    
    *******************/




    public enum NodeType
    {
        Body,
        CollisionShape
    }




    /******************
    
        
    
    *******************/




    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIdResult SetActive(State state, GenId entityId, bool isActive)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
        {
            return GenIdResult.StaleGenId;
        }
        SetActiveUnsafe(state, entityId, isActive);
        return GenIdResult.Ok;
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetActiveUnsafe(State state, GenId entityId, bool isActive)
    {
        SetActiveUnsafe(state, GenId.GetIndex(entityId), isActive);
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetActiveUnsafe(State state, int entityIndex, bool isActive)
    {
        state.Active[entityIndex] = isActive; 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsActive(State state, GenId entityId, ref GenIdResult resultOutput)
    {
        if (EntityRegistry.IsGenIdStale(state.Entities, entityId))
        {
            resultOutput = GenIdResult.StaleGenId;
            return false;
        }
        resultOutput = GenIdResult.Ok;
        return IsActiveUnsafe(state, entityId);
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsActiveUnsafe(State state, GenId genId)
    {
        return IsActiveUnsafe(state, GenId.GetIndex(genId));
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsActiveUnsafe(State state, int entityIndex)
    {
        return state.Active[entityIndex];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIdResult SetLocalTransform(State state, GenId genId, Transform transform)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetLocalTransformUnsafe(state, genId, transform);

        return GenIdResult.Ok;
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetLocalTransformUnsafe(State state, GenId entityId, Transform newTransform)
    {
        SetLocalTransformUnsafe(state, GenId.GetIndex(entityId), newTransform);    
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetLocalTransformUnsafe(State state, int entityIndex, Transform newTransform)
    {
        state.LocalTransforms.Positions.X[entityIndex] = newTransform.Position.X;
        state.LocalTransforms.Positions.Y[entityIndex] = newTransform.Position.Y;
        state.LocalTransforms.Scales.X[entityIndex] = newTransform.Scale.X;
        state.LocalTransforms.Scales.Y[entityIndex] = newTransform.Scale.Y;
        state.LocalTransforms.Cosines[entityIndex] = newTransform.Cosine;
        state.LocalTransforms.Sines[entityIndex] = newTransform.Sine;        
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    public static void SetGlobalTransformUnsafe(State state, int entityIndex, Transform newTransform)
    {
        state.GlobalTransforms.Positions.X[entityIndex] = newTransform.Position.X;
        state.GlobalTransforms.Positions.Y[entityIndex] = newTransform.Position.Y;
        state.GlobalTransforms.Scales.X[entityIndex] = newTransform.Scale.X;
        state.GlobalTransforms.Scales.Y[entityIndex] = newTransform.Scale.Y;
        state.GlobalTransforms.Cosines[entityIndex] = newTransform.Cosine;
        state.GlobalTransforms.Sines[entityIndex] = newTransform.Sine;                
    }

    public static Vector2 GetLinearVelocity(State state, GenId entityId, ref GenIdResult resultOutput)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
        {
            resultOutput = GenIdResult.StaleGenId;
            return default;
        }

        int index = GenId.GetIndex(entityId);
                
        if(IsActiveUnsafe(state, index) == false)
        {
            resultOutput = GenIdResult.NotActive;
            return default;
        }

        return GetLinearVelocityUnsafe(state, index);
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id will always be returned. 
    /// </remarks>
    public static Vector2 GetLinearVelocityUnsafe(State state, GenId entityId)
    {
        return GetLinearVelocityUnsafe(state, GenId.GetIndex(entityId));
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id will always be returned. 
    /// </remarks>
    public static Vector2 GetLinearVelocityUnsafe(State state, int entityIndex)
    {
        Soa_Vector2 linearVelocities = state.LinearVelocities;
        return new(linearVelocities.X[entityIndex], linearVelocities.Y[entityIndex]);
    }

    public static void Translate(State state, float xDisplacement, float yDisplacement, int entityIndex)
    {            
        ref float tX = ref state.ShapeCollisionDisplacements.X[entityIndex];
        ref float tY = ref state.ShapeCollisionDisplacements.Y[entityIndex];

        if(tX == 0 || tY == 0)
        {
            StackArray.Push(state.DisplacedThisSubStep, entityIndex);
        }

        tX += xDisplacement;
        tY += yDisplacement;
    }

    public static void ApplyTranslations(State state)
    {
        float[] dXS = state.ShapeCollisionDisplacements.X;
        float[] dYS = state.ShapeCollisionDisplacements.Y;
        for(int i = 1; i < state.DisplacedThisSubStep.Count; i++) // skip Nil.
        {
            int bodyIndex = state.DisplacedThisSubStep[i];

            ref float dX = ref dXS[bodyIndex];
            ref float dY = ref dYS[bodyIndex];

            // translate the body and its shapes...

            // clear the translation for next step.
            dX = 0;
            dY = 0;
        }
    }




    /******************
    
        Main Loop.
    
    *******************/




    public static void FixedUpdate(HowlAppState app, State state, float deltaTime, int subSteps)
    {
        state.FixedUpdateStepStopwatch.Restart();

        // == hoisting invariance. ==.
        
        int[] bvhLeafIndices = state.BvhLeafIndices;
        CollisionManifoldState collisions = state.CollisionManifoldState;
        float[] collisionNormalsX = collisions.Normals.X;
        float[] collisionNormalsY = collisions.Normals.Y;
        float[] collisionDepths = collisions.Depths;
        float[] collisionFirstContactPointsX = collisions.FirstContactPoints.X;
        float[] collisionFirstContactPointsY = collisions.FirstContactPoints.Y;
        float[] collisionSecondContactPointsX = collisions.SecondContactPoints.X;
        float[] collisionSecondContactPointsY = collisions.SecondContactPoints.Y;
        bool[] collisionTwoContactPoints = collisions.TwoContactPoints;
        int collisionsStride = collisions.Stride;
        Soa_Vector2 centroids = state.Centroids;
        FsSoa_Vector2 worldVertices = state.WorldVertices;
        CategorisedOverlapArray<int> shapeCollisionsToResolve = state.SubStepShapeCollisionsToResolve;
        CategorisedOverlapArray<int> rigidShapeCollisionsToResolve = state.SubStepRigidShapeCollisionsToResolve;
        float[] worldRadii = state.WorldRadii;
        CategorisedLeafOverlaps overlaps = state.OverlapsScratchBuffer;
        BoundingVolumeHierarchy bvh = state.Bvh;
        float[] bvhLeafPaddings = state.BvhLeafPaddings;
        int[] categories = state.Categories;
        FsSoa_Vector2 localVertices = state.BaseVertices;
        float[] baseRadii = state.BaseRadii;
        float[] baseWidths = state.BaseWidths;
        float[] baseHeights = state.BaseHeights;
        Soa_Transform localTransforms = state.LocalTransforms;
        Soa_Transform globalTransforms = state.GlobalTransforms;
        float[] globalPositionsX = globalTransforms.Positions.X;
        float[] globalPositionsY = globalTransforms.Positions.Y;  
        float[] globalScalesX = globalTransforms.Scales.X;
        float[] globalScalesY = globalTransforms.Scales.Y;
        float[] globalCosines = globalTransforms.Cosines;
        float[] globalSines = globalTransforms.Sines;
        float[] globalRotationRadians = globalTransforms.RotationRadians;
        float[] masses = state.Masses;
        float[] inverseMasses = state.InverseMasses;
        float[] rotationalInertia = state.RotationalInertia;
        float[] inverseRotationalInertia = state.InverseRotationalInertia;
        float[] previousPositionsX = state.PreviousStepPositions.X;
        float[] previousPositionsY = state.PreviousStepPositions.Y;
        float[] densities = state.PhysicsMaterials.Density;
        float[] staticFrictions = state.PhysicsMaterials.StaticFriction;
        float[] kineticFrictions = state.PhysicsMaterials.KineticFriction;
        float[] restitutions = state.PhysicsMaterials.Restitution;
        bool[] gravityAffected = state.GravityAffected;
        float[] minAabbsX = state.Aabbs.MinX;
        float[] minAabbsY = state.Aabbs.MinY;
        float[] maxAabbsX = state.Aabbs.MaxX;
        float[] maxAabbsY = state.Aabbs.MaxY;
        float[] centroidsX = state.Centroids.X;
        float[] centroidsY = state.Centroids.Y;
        float[] linearVelocitiesX = state.LinearVelocities.X;
        float[] linearVelocitiesY = state.LinearVelocities.Y;
        float[] forcesX = state.Forces.X;
        float[] forcesY = state.Forces.Y;
        float[] angularVelocities = state.AngularVelocities;
        float[] collisionDisplacementsX = state.ShapeCollisionDisplacements.X;
        float[] collisionDisplacementsY = state.ShapeCollisionDisplacements.Y;
        float[] localCentersOfMassX = state.LocalCentersOfMass.X;
        float[] localCentersOfMassY = state.LocalCentersOfMass.Y;
        float gravity = state.Gravity;
        float gravityDirectionX = state.GravityDirection.X;
        float gravityDirectionY = state.GravityDirection.Y;
        bool[] rotationalResponses = state.RotationalResponses;
        Shape.Rigid.ShapeType[] shapes = state.ShapeTypes;
        SwapBackArray<int> activeBodies = state.BodyHierarchy.RootIndices;
        IntrusiveList.Node[] nodes = state.BodyHierarchy.Nodes;

        // scratch buffers for rigid body reslution.
        Span<float> impulseMagnitudes = stackalloc float[MaxCollisionContactPoints]; 
        Span<float> contactPointsX = stackalloc float[MaxCollisionContactPoints];
        Span<float> contactPointsY = stackalloc float[MaxCollisionContactPoints];
        Span<float> impulsesX = stackalloc float[MaxCollisionContactPoints];
        Span<float> impulsesY = stackalloc float[MaxCollisionContactPoints];
        Span<float> distsAX = stackalloc float[MaxCollisionContactPoints];
        Span<float> distsAY = stackalloc float[MaxCollisionContactPoints];
        Span<float> distsBX = stackalloc float[MaxCollisionContactPoints];
        Span<float> distsBY = stackalloc float[MaxCollisionContactPoints];

        // scale delta time by the substeps.
        deltaTime /= (float)subSteps;
        
        {   // Prepare Substep Collisions
            
            CollisionManifold.PrepareForNextStep(collisions);

            int solidCount = state.SolidPolygonColliderCount + state.SolidCircleColliderCount + state.SolidPolygonRigidBodyCount + state.SolidCircleRigidBodyCount;
            int kinematicCount = state.KinematicPolygonColliderCount + state.KinematicCircleColliderCount + state.KinematicPolygonRigidBodyCount + state.KinematicCircleRigidBodyCount;

            // prepare sub step collision resolution collection.
            shapeCollisionsToResolve.CategoryLengths[CollisionResolutionCategory.Solid] = solidCount;
            shapeCollisionsToResolve.CategoryLengths[CollisionResolutionCategory.Kinematic] = kinematicCount;
            CategorisedOverlapArray.BuildChunks(shapeCollisionsToResolve);

            rigidShapeCollisionsToResolve.CategoryLengths[CollisionResolutionCategory.Solid] = solidCount;
            rigidShapeCollisionsToResolve.CategoryLengths[CollisionResolutionCategory.Kinematic] = kinematicCount;
            CategorisedOverlapArray.BuildChunks(rigidShapeCollisionsToResolve);
        }

        {   // Bvh
            
            state.BvhStopwatch.Restart();
            
            CalculateBvhLeafPadding(globalPositionsX, globalPositionsY, previousPositionsX, previousPositionsY, activeBodies, bvhLeafPaddings, deltaTime);

            // Update Overlap Scratch Buffer Category Length.       
            {                
            
                CategorisedLeafOverlaps.ClearCounts(overlaps);
                overlaps.CategoryLengths[Shape.Category.SolidCircleCollider]       = state.SolidCircleColliderCount;
                overlaps.CategoryLengths[Shape.Category.TriggerCircleCollider]     = state.TriggerCircleColliderCount;
                overlaps.CategoryLengths[Shape.Category.KinematicCircleCollider]   = state.KinematicCircleColliderCount;
                
                overlaps.CategoryLengths[Shape.Category.SolidCircleRigidBody]      = state.SolidCircleRigidBodyCount;
                overlaps.CategoryLengths[Shape.Category.TriggerCircleRigidBody]    = state.TriggerCircleRigidBodyCount;
                overlaps.CategoryLengths[Shape.Category.KinematicCircleRigidBody]  = state.KinematicCircleRigidBodyCount;

                overlaps.CategoryLengths[Shape.Category.SolidPolygonCollider]      = state.SolidPolygonColliderCount;
                overlaps.CategoryLengths[Shape.Category.TriggerPolygonCollider]    = state.TriggerPolygonColliderCount;
                overlaps.CategoryLengths[Shape.Category.KinematicPolygonCollider]  = state.KinematicPolygonColliderCount;
                
                overlaps.CategoryLengths[Shape.Category.SolidPolygonRigidBody]     = state.SolidPolygonRigidBodyCount;
                overlaps.CategoryLengths[Shape.Category.TriggerPolygonRigidBody]   = state.TriggerPolygonRigidBodyCount;
                overlaps.CategoryLengths[Shape.Category.KinematicPolygonRigidBody] = state.KinematicPolygonRigidBodyCount;
                
                CategorisedLeafOverlaps.BuildChunks(overlaps);
            }

            // Reconstruct Bvh.
            ConstructBvhTree(activeBodies, nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, centroidsX, centroidsY, categories, bvhLeafPaddings, 
                bvhLeafIndices, bvh
            );

            BoundingVolumeHierarchy.FindOverlaps(bvh.Branches, bvh.Leaves, overlaps);
            FormatCategorisedOverlaps(overlaps, bvhLeafIndices, categories);
            
            state.BvhStopwatch.Stop();
        }
        
        // note: ordering matters here; keep this below the bvh section always.
        SetPreviousPositions(globalPositionsX, globalPositionsY, previousPositionsX, previousPositionsY);      

        // == retrieve overlap info.

        // solid polygon rigidbody.        
        OverlapInfo overlaps_SolPolRig_To_SolPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.SolidPolygonRigidBody);
        OverlapInfo overlaps_SolPolRig_To_SolCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.SolidCircleRigidBody);
        OverlapInfo overlaps_SolPolRig_To_KinPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.KinematicPolygonRigidBody);
        OverlapInfo overlaps_SolPolRig_To_KinCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.KinematicCircleRigidBody);
        OverlapInfo overlaps_SolPolRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.TriggerPolygonRigidBody);
        OverlapInfo overlaps_SolPolRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.TriggerCircleRigidBody);
        OverlapInfo overlaps_SolPolRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.SolidPolygonCollider);
        OverlapInfo overlaps_SolPolRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.SolidCircleCollider);
        OverlapInfo overlaps_SolPolRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.KinematicPolygonCollider);
        OverlapInfo overlaps_SolPolRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_SolPolRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_SolPolRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonRigidBody, Shape.Category.TriggerCircleCollider);
        
        // solid circle rigid body.
        OverlapInfo overlaps_SolCirRig_To_SolCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.SolidCircleRigidBody);
        OverlapInfo overlaps_SolCirRig_To_KinPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.KinematicPolygonRigidBody);
        OverlapInfo overlaps_SolCirRig_To_KinCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.KinematicCircleRigidBody);
        OverlapInfo overlaps_SolCirRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.TriggerPolygonRigidBody);
        OverlapInfo overlaps_SolCirRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.TriggerCircleRigidBody);
        OverlapInfo overlaps_SolCirRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.SolidPolygonCollider);
        OverlapInfo overlaps_SolCirRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.SolidCircleCollider);
        OverlapInfo overlaps_SolCirRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.KinematicPolygonCollider);
        OverlapInfo overlaps_SolCirRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_SolCirRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_SolCirRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleRigidBody, Shape.Category.TriggerCircleCollider);

        // kinematic polygon rigid body.
        OverlapInfo overlaps_KinPolRig_To_KinPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.KinematicPolygonRigidBody);
        OverlapInfo overlaps_KinPolRig_To_KinCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.KinematicCircleRigidBody);
        OverlapInfo overlaps_KinPolRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.TriggerPolygonRigidBody);
        OverlapInfo overlaps_KinPolRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.TriggerCircleRigidBody);
        OverlapInfo overlaps_KinPolRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.SolidPolygonCollider);
        OverlapInfo overlaps_KinPolRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.SolidCircleCollider);
        OverlapInfo overlaps_KinPolRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.KinematicPolygonCollider);
        OverlapInfo overlaps_KinPolRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_KinPolRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_KinPolRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonRigidBody, Shape.Category.TriggerCircleCollider);
        
        // kinematic circle rigid body.
        OverlapInfo overlaps_KinCirRig_To_KinCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleRigidBody, Shape.Category.KinematicCircleRigidBody);
        OverlapInfo overlaps_KinCirRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleRigidBody, Shape.Category.TriggerPolygonRigidBody);
        OverlapInfo overlaps_KinCirRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleRigidBody, Shape.Category.TriggerCircleRigidBody);
        OverlapInfo overlaps_KinCirRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleRigidBody, Shape.Category.SolidPolygonCollider);
        OverlapInfo overlaps_KinCirRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleRigidBody, Shape.Category.SolidCircleCollider);
        OverlapInfo overlaps_KinCirRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleRigidBody, Shape.Category.KinematicPolygonCollider);
        OverlapInfo overlaps_KinCirRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleRigidBody, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_KinCirRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleRigidBody, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_KinCirRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleRigidBody, Shape.Category.TriggerCircleCollider);
        
        // trigger polygon rigid body.
        OverlapInfo overlaps_TriPolRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonRigidBody, Shape.Category.TriggerPolygonRigidBody);    
        OverlapInfo overlaps_TriPolRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonRigidBody, Shape.Category.TriggerCircleRigidBody);
        OverlapInfo overlaps_TriPolRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonRigidBody, Shape.Category.SolidPolygonCollider);
        OverlapInfo overlaps_TriPolRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonRigidBody, Shape.Category.SolidCircleCollider);
        OverlapInfo overlaps_TriPolRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonRigidBody, Shape.Category.KinematicPolygonCollider);
        OverlapInfo overlaps_TriPolRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonRigidBody, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_TriPolRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonRigidBody, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_TriPolRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonRigidBody, Shape.Category.TriggerCircleCollider);
        
        // trigger circle rigidbody.
        OverlapInfo overlaps_TriCirRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerCircleRigidBody, Shape.Category.TriggerCircleRigidBody);
        OverlapInfo overlaps_TriCirRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerCircleRigidBody, Shape.Category.SolidPolygonCollider);
        OverlapInfo overlaps_TriCirRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerCircleRigidBody, Shape.Category.SolidCircleCollider);
        OverlapInfo overlaps_TriCirRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerCircleRigidBody, Shape.Category.KinematicPolygonCollider);
        OverlapInfo overlaps_TriCirRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerCircleRigidBody, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_TriCirRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerCircleRigidBody, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_TriCirRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerCircleRigidBody, Shape.Category.TriggerCircleCollider);
        
        // solid polygon collider.
        OverlapInfo overlaps_SolPolCol_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonCollider, Shape.Category.SolidPolygonCollider);
        OverlapInfo overlaps_SolPolCol_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonCollider, Shape.Category.SolidCircleCollider);
        OverlapInfo overlaps_SolPolCol_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonCollider, Shape.Category.KinematicPolygonCollider);
        OverlapInfo overlaps_SolPolCol_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonCollider, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_SolPolCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonCollider, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_SolPolCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidPolygonCollider, Shape.Category.TriggerCircleCollider);
        
        // solid circle collider.
        OverlapInfo overlaps_SolCirCol_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleCollider, Shape.Category.SolidCircleCollider);
        OverlapInfo overlaps_SolCirCol_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleCollider, Shape.Category.KinematicPolygonCollider);
        OverlapInfo overlaps_SolCirCol_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleCollider, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_SolCirCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleCollider, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_SolCirCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.SolidCircleCollider, Shape.Category.TriggerCircleCollider);
        
        // kinematic polygon collider.
        OverlapInfo overlaps_KinPolCol_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonCollider, Shape.Category.KinematicPolygonCollider);
        OverlapInfo overlaps_KinPolCol_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonCollider, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_KinPolCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonCollider, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_KinPolCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicPolygonCollider, Shape.Category.TriggerCircleCollider);
        
        // kinematic circle collider.
        OverlapInfo overlaps_KinCirCol_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleCollider, Shape.Category.KinematicCircleCollider);
        OverlapInfo overlaps_KinCirCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleCollider, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_KinCirCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinematicCircleCollider, Shape.Category.TriggerCircleCollider);
        
        // trigger polygon collider.
        OverlapInfo overlaps_TriPolCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonCollider, Shape.Category.TriggerPolygonCollider);
        OverlapInfo overlaps_TriPolCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerPolygonCollider, Shape.Category.TriggerCircleCollider);
        
        // trigger circle collider.
        OverlapInfo overlaps_TriCirCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriggerCircleCollider, Shape.Category.TriggerCircleCollider);

        for(int i = 0; i < subSteps; i++)
        {
            // clear any grabage collisions that were resolved last sub step.
            CategorisedOverlapArray.ClearCounts(shapeCollisionsToResolve);
            CategorisedOverlapArray.ClearCounts(rigidShapeCollisionsToResolve);

            state.FixedUpdateSubStepStopwatch.Restart();

            // RigidBody Movement Step.
            state.RigidBodyMovementStepStopwatch.Restart();
            BodyMovementStep(activeBodies, nodes, localTransforms, globalTransforms, linearVelocitiesX, linearVelocitiesY, 
                forcesX, forcesY, masses, angularVelocities, collisionDisplacementsX, collisionDisplacementsY, localCentersOfMassX, localCentersOfMassY, 
                globalRotationRadians, categories, gravityAffected, gravityDirectionX, gravityDirectionY, gravity, deltaTime, 
                MovementStepConfig.Full
            );
            state.RigidBodyMovementStepStopwatch.Stop();

            // transform physics bodies
            state.TransformPhysicsBodiesStopwatch.Restart();
            TransformAllShapesVertices(activeBodies, nodes, worldVertices, localVertices, shapes, globalScalesX, globalScalesY, 
                globalPositionsX, globalPositionsY, globalSines, globalCosines, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, 
                centroidsX, centroidsY, baseRadii, worldRadii
            );
            state.TransformPhysicsBodiesStopwatch.Stop();


            // Find collisions.
            state.FindCollisionsStopwatch.Restart();
                        
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_SolidPolygonRigidBody(    overlaps_SolPolRig_To_SolPolRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_SolidCircleRigidBody(     overlaps_SolPolRig_To_SolCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_KinematicPolygonRigidBody(overlaps_SolPolRig_To_KinPolRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_KinematicCircleRigidBody( overlaps_SolPolRig_To_KinCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_TriggerPolygonRigidBody(  overlaps_SolPolRig_To_TriPolRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_TriggerCircleRigidBody(   overlaps_SolPolRig_To_TriCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_SolidPolygonCollider(     overlaps_SolPolRig_To_SolPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_SolidCircleCollider(      overlaps_SolPolRig_To_SolCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_KinematicPolygonCollider( overlaps_SolPolRig_To_KinPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_KinematicCircleCollider(  overlaps_SolPolRig_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_TriggerPolygonCollider(   overlaps_SolPolRig_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.SolidPolygonRigidBody_To_TriggerCircleCollider(    overlaps_SolPolRig_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);

            Physics.Collisions.Detection.SolidCircleRigidBody_To_SolidCircleRigidBody(      overlaps_SolCirRig_To_SolCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_KinematicPolygonRigidBody( overlaps_SolCirRig_To_KinPolRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_KinematicCircleRigidBody(  overlaps_SolCirRig_To_KinCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_TriggerPolygonRigidBody(   overlaps_SolCirRig_To_TriPolRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_TriggerCircleRigidBody(    overlaps_SolCirRig_To_TriCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_SolidPolygonCollider(      overlaps_SolCirRig_To_SolPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_SolidCircleCollider(       overlaps_SolCirRig_To_SolCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_KinematicPolygonCollider(  overlaps_SolCirRig_To_KinPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_KinematicCircleCollider(   overlaps_SolCirRig_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_TriggerPolygonCollider(    overlaps_SolCirRig_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.SolidCircleRigidBody_To_TriggerCircleCollider(     overlaps_SolCirRig_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);

            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_KinematicPolygonRigidBody(overlaps_KinPolRig_To_KinPolRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);            
            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_KinematicCircleRigidBody( overlaps_KinPolRig_To_KinCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_TriggerPolygonRigidBody(  overlaps_KinPolRig_To_TriPolRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_TriggerCircleRigidBody(   overlaps_KinPolRig_To_TriCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_SolidPolygonCollider(     overlaps_KinPolRig_To_SolPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, shapeCollisionsToResolve);
            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_SolidCircleCollider(      overlaps_KinPolRig_To_SolCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_KinematicPolygonCollider( overlaps_KinPolRig_To_KinPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_KinematicCircleCollider(  overlaps_KinPolRig_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_TriggerPolygonCollider(   overlaps_KinPolRig_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.KinematicPolygonRigidBody_To_TriggerCircleCollider(    overlaps_KinPolRig_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);

            Physics.Collisions.Detection.KinematicCircleRigidBody_To_KinematicCircleRigidBody(  overlaps_KinCirRig_To_KinCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
            Physics.Collisions.Detection.KinematicCircleRigidBody_To_TriggerPolygonRigidBody(   overlaps_KinCirRig_To_TriPolRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.KinematicCircleRigidBody_To_TriggerCircleRigidBody(    overlaps_KinCirRig_To_TriCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
            Physics.Collisions.Detection.KinematicCircleRigidBody_To_SolidPolygonCollider(      overlaps_KinCirRig_To_SolPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.KinematicCircleRigidBody_To_SolidCircleCollider(       overlaps_KinCirRig_To_SolCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.KinematicCircleRigidBody_To_KinematicPolygonCollider(  overlaps_KinCirRig_To_KinPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.KinematicCircleRigidBody_To_KinematicCircleCollider(   overlaps_KinCirRig_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
            Physics.Collisions.Detection.KinematicCircleRigidBody_To_TriggerPolygonCollider(    overlaps_KinCirRig_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.KinematicCircleRigidBody_To_TriggerCircleCollider(     overlaps_KinCirRig_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
        
            Physics.Collisions.Detection.TriggerPolygonRigidBody_To_TriggerPolygonRigidBody(  overlaps_TriPolRig_To_TriPolRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.TriggerPolygonRigidBody_To_TriggerCircleRigidBody(   overlaps_TriPolRig_To_TriCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.TriggerPolygonRigidBody_To_SolidPolygonCollider(     overlaps_TriPolRig_To_SolPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.TriggerPolygonRigidBody_To_SolidCircleCollider(      overlaps_TriPolRig_To_SolCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.TriggerPolygonRigidBody_To_KinematicPolygonCollider( overlaps_TriPolRig_To_KinPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.TriggerPolygonRigidBody_To_KinematicCircleCollider(  overlaps_TriPolRig_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.TriggerPolygonRigidBody_To_TriggerPolygonCollider(   overlaps_TriPolRig_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.TriggerPolygonRigidBody_To_TriggerCircleCollider(    overlaps_TriPolRig_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);

            Physics.Collisions.Detection.TriggerCircleRigidBody_To_TriggerCircleRigidBody(    overlaps_TriCirRig_To_TriCirRig, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
            Physics.Collisions.Detection.TriggerCircleRigidBody_To_SolidPolygonCollider(      overlaps_TriCirRig_To_SolPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.TriggerCircleRigidBody_To_SolidCircleCollider(       overlaps_TriCirRig_To_SolCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
            Physics.Collisions.Detection.TriggerCircleRigidBody_To_KinematicPolygonCollider(  overlaps_TriCirRig_To_KinPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.TriggerCircleRigidBody_To_KinematicCircleCollider(   overlaps_TriCirRig_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
            Physics.Collisions.Detection.TriggerCircleRigidBody_To_TriggerPolygonCollider(    overlaps_TriCirRig_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.TriggerCircleRigidBody_To_TriggerCircleCollider(     overlaps_TriCirRig_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);

            Physics.Collisions.Detection.SolidPolygonCollider_To_SolidPolygonCollider(     overlaps_SolPolCol_To_SolPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonCollider_To_SolidCircleCollider(      overlaps_SolPolCol_To_SolCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonCollider_To_KinematicPolygonCollider( overlaps_SolPolCol_To_KinPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonCollider_To_KinematicCircleCollider(  overlaps_SolPolCol_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidPolygonCollider_To_TriggerPolygonCollider(   overlaps_SolPolCol_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.SolidPolygonCollider_To_TriggerCircleCollider(    overlaps_SolPolCol_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);

            Physics.Collisions.Detection.SolidCircleCollider_To_SolidCircleCollider(       overlaps_SolCirCol_To_SolCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleCollider_To_KinematicPolygonCollider(  overlaps_SolCirCol_To_KinPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleCollider_To_KinematicCircleCollider(   overlaps_SolCirCol_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii, shapeCollisionsToResolve);
            Physics.Collisions.Detection.SolidCircleCollider_To_TriggerPolygonCollider(    overlaps_SolCirCol_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.SolidCircleCollider_To_TriggerCircleCollider(     overlaps_SolCirCol_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
        
            Physics.Collisions.Detection.KinematicPolygonCollider_To_KinematicPolygonCollider( overlaps_KinPolCol_To_KinPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.KinematicPolygonCollider_To_KinematicCircleCollider(  overlaps_KinPolCol_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.KinematicPolygonCollider_To_TriggerPolygonCollider(   overlaps_KinPolCol_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.KinematicPolygonCollider_To_TriggerCircleCollider(    overlaps_KinPolCol_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
        
            Physics.Collisions.Detection.KinematicCircleCollider_To_KinematicCircleCollider(  overlaps_KinCirCol_To_KinCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);
            Physics.Collisions.Detection.KinematicCircleCollider_To_TriggerPolygonCollider(   overlaps_KinCirCol_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);
            Physics.Collisions.Detection.KinematicCircleCollider_To_TriggerCircleCollider(    overlaps_KinCirCol_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);

            Physics.Collisions.Detection.TriggerPolygonCollider_To_TriggerPolygonCollider(  overlaps_TriPolCol_To_TriPolCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices);
            Physics.Collisions.Detection.TriggerPolygonCollider_To_TriggerCircleCollider(   overlaps_TriPolCol_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldVertices, worldRadii);

            Physics.Collisions.Detection.TriggerCircleCollider_To_TriggerCircleCollider(overlaps_TriCirCol_To_TriCirCol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, worldRadii);

            state.FindCollisionsStopwatch.Stop();

            // Resolve Collider Collisions.
            // NOTE: ordering matters here, make sure to this is above rigidbody collision resolution.
            state.ColliderCollisionResolutionStopwatch.Restart();
            ResolveColliderCollisions(nodes, shapeCollisionsToResolve, collisionDepths, collisionNormalsX, collisionNormalsY, 
                collisionDisplacementsX, collisionDisplacementsY, collisionsStride
            );
            state.ColliderCollisionResolutionStopwatch.Stop();

            // Resolve RigidBody Collisions.
            // NOTE: ordering matters here, make sure this is below collision resolution.
            state.RigidBodyCollisionResolutionStepStopwatch.Restart();
            ResolveRigidShapeCollisions(rigidShapeCollisionsToResolve, nodes,
                collisionNormalsX, collisionNormalsY, collisionFirstContactPointsX, collisionFirstContactPointsY,
                globalPositionsX, globalPositionsY, localCentersOfMassX, localCentersOfMassY, 
                collisionSecondContactPointsX, collisionSecondContactPointsY, linearVelocitiesX, linearVelocitiesY, 
                restitutions, kineticFrictions, staticFrictions, angularVelocities, masses, inverseMasses, inverseRotationalInertia, 
                collisionTwoContactPoints, rotationalResponses, contactPointsX, contactPointsY, distsAX, distsAY, 
                distsBX, distsBY, impulseMagnitudes, impulsesX, impulsesY, 
                collisionsStride
            );
            state.RigidBodyCollisionResolutionStepStopwatch.Stop();

            // Sort Collision Manifold.
            // sort the collision manifold after resolution step.
            // this is to ensure that binary searching for collisions
            // using a GenIndex work outside of this function.
            state.CollisionManifoldSortStopwatch.Restart();
            state.CollisionManifoldSortStopwatch.Stop();
            state.FixedUpdateSubStepStopwatch.Stop();
        }

        CollisionManifold.CompleteStep(state.CollisionManifoldState);

        // Transform bodies by collision resolution.
        // NOTE: this is needed at the end as the final
        // sub-step iteration does not transform the bodies
        // at the end of it's loop; meaning the final collision
        // resolution wouldn't be applied.
        TransformAllShapesVertices(activeBodies, nodes, worldVertices, localVertices, shapes, globalScalesX, globalScalesY, 
            globalPositionsX, globalPositionsY, globalSines, globalCosines, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, 
            centroidsX, centroidsY, baseRadii, worldRadii
        );
        state.FixedUpdateStepStopwatch.Stop();
    }

    /// <summary>
    ///     Performs a movement step for all bodies.
    /// </summary>
    /// <remarks>
    ///     Remarks: All provided spans must be indexed by a integer <c>physicsBodyIndex</c>:
    /// </remarks>
    public static void BodyMovementStep(SwapBackArray<int> activeBodies, IntrusiveList.Node[] nodes, 
        Soa_Transform localTransforms, Soa_Transform globalTransforms, float[] linearVelocitiesX, float[] linearVelocitiesY, 
        float[] forcesX, float[] forcesY, float[] masses, float[] angularVelocities, float[] collisionDisplacementsX, 
        float[] collisionDisplacementsY, float[] localCentersOfMassX, float[] localCentersOfMassY, float[] rotationRadians, int[] categories, 
        bool[] gravityAffected, float gravityDirectionX, float gravityDirectionY, float gravity, float deltaTime, MovementStepConfig config
    )
    {   
        float gravityLinearForceX = gravityDirectionX * gravity * deltaTime;
        float gravityLinearForceY = gravityDirectionY * gravity * deltaTime;
        
        for(int i = 1; i < activeBodies.Count; i++) // skip the Nil.
        {
            int bodyIndex = activeBodies[i];

            ref float bodyPosX = ref globalTransforms.Positions.X[bodyIndex];
            ref float bodyPosY = ref globalTransforms.Positions.Y[bodyIndex];
            ref float bodySine = ref globalTransforms.Sines[bodyIndex];
            ref float bodyCosine = ref globalTransforms.Cosines[bodyIndex];
            ref float bodyRotationRadians = ref globalTransforms.RotationRadians[bodyIndex];
            ref float bodyScaleX = ref globalTransforms.Scales.X[bodyIndex];
            ref float bodyScaleY = ref globalTransforms.Scales.Y[bodyIndex];            

            if(config == MovementStepConfig.Full || config == MovementStepConfig.LinearVelocityOnly)
            {
                ref float linearVelocityX = ref linearVelocitiesX[bodyIndex];
                ref float linearVelocityY = ref linearVelocitiesY[bodyIndex];
                ref float mass = ref masses[bodyIndex];

                if(categories[bodyIndex] < Shape.Category.KinematicPolygonRigidBody && gravityAffected[bodyIndex])
                {                
                    // apply gravity.
                    linearVelocityX += gravityLinearForceX;
                    linearVelocityY += gravityLinearForceY;
                }

                // force = mass * acceleration.
                // acceleration = force / mass.
                if(mass > 0)
                {
                    linearVelocityX += forcesX[bodyIndex] / mass * deltaTime;
                    linearVelocityY += forcesY[bodyIndex] / mass * deltaTime;
                }

                // get the local center of mass of the body.
                float localComX = localCentersOfMassX[bodyIndex];
                float localComY = localCentersOfMassY[bodyIndex];

                // calculate the world position of the center of mass before rotation.
                float worldComX = bodyPosX + (localComX * bodyCosine - localComY * bodySine);
                float worldComY = bodyPosY + (localComX * bodySine + localComY * bodyCosine);

                // rotate the body by the anfular velocity.
                Math.Math.RotorMultiply(bodySine, bodyCosine, angularVelocities[bodyIndex] * deltaTime, 
                    ref bodySine, ref bodyCosine
                );
                rotationRadians[bodyIndex] = MathF.Atan2(bodySine, bodyCosine);

                // reverse the calculation using the new rotation values.
                // keeping the center of mass as the point of rotation rather than the body's global position.
                bodyPosX = worldComX - (localComX * bodyCosine - localComY * bodySine);
                bodyPosY = worldComY - (localComX * bodySine + localComY * bodyCosine);

                // apply the linear velocity translation.
                bodyPosX += linearVelocityX * deltaTime;
                bodyPosY += linearVelocityY * deltaTime;         
            }

            if(config == MovementStepConfig.Full || config == MovementStepConfig.DisplacementOnly)
            {
                ref float dX = ref collisionDisplacementsX[bodyIndex];
                bodyPosX += dX;
                dX = 0;

                ref float dY = ref collisionDisplacementsY[bodyIndex];
                bodyPosY += dY;
                dY = 0;
            }

            // move and rotate the body's shapes.
            ref IntrusiveList.Node node = ref nodes[bodyIndex];
            int bodyFirstShapeIndex = node.FirstChild;
            if(bodyFirstShapeIndex != 0)
            {
                int shapeIndex = bodyFirstShapeIndex;
                
                while (true)
                {
                    Soa_Transform.TransformRelative(localTransforms, globalTransforms, shapeIndex, shapeIndex,
                        bodyPosX, bodyPosY, bodyScaleX, bodyScaleY, bodySine, bodyCosine, bodyRotationRadians
                    );

                    shapeIndex = nodes[shapeIndex].NextSibling;
                    if(shapeIndex == bodyFirstShapeIndex)
                    {
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Transforms <c>InUse<c/> physics bodies local-space vertices by their world-space transforms.
    /// </summary>
    /// <remarks>
    ///     All arrays must be of the same length and elements should be vertivally accessible via <c>physicsBodyIndex</c>. 
    /// </remarks>
    public static void TransformAllShapesVertices(SwapBackArray<int> activeBodies, IntrusiveList.Node[] nodes, FsSoa_Vector2 worldVertices, 
        FsSoa_Vector2 localVertices, Shape.Rigid.ShapeType[] shapes, float[] globalScalesX, float[] globalScalesY, float[] globalPositionsX, 
        float[] globalPositionsY, float[] globalSines, float[] globalCosines, float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, 
        float[] maxAabbsY, float[] centroidsX, float[] centroidsY, float[] localRadii, float[] globalRadii
    )
    {
        FsSoa_Vector2.ClearAppendCounts(worldVertices);
        int length = activeBodies.Count;

        for(int i = 1; i < length; i++) // start at one to avoid Nil.
        {
            int bodyIndex = activeBodies[i];

            int bodyFirstShapeIndex = nodes[bodyIndex].FirstChild;

            if(bodyFirstShapeIndex == 0)
            {
                return;
            }
            else
            {
                int shapeIndex = bodyFirstShapeIndex;
                while (true)
                {
                    TransformShapeVertices(worldVertices, localVertices, globalPositionsX, globalPositionsY, globalScalesX, globalScalesY, 
                        globalCosines, globalSines, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, localRadii, globalRadii, centroidsX, 
                        centroidsY, shapes, shapeIndex
                    );

                    shapeIndex = nodes[shapeIndex].NextSibling;
                    if(shapeIndex == bodyFirstShapeIndex)
                    {
                        break;
                    } 
                }
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para><paramref name="vertsX"/> and <paramref name="vertsY"/> are empty scratch buffers to write the gathered verts of the shape to.</para>
    /// </remarks>
    public static void TransformShapeVertices(FsSoa_Vector2 worldVertices, FsSoa_Vector2 localVertices, float[] globalPositionsX, 
        float[] globalPositionsY, float[] globalScalesX, float[] globalScalesY, float[] globalCosines, float[] globalSines, 
        float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY, float[] localRadii, float[] globalRadii, 
        float[] centroidsX, float[] centroidsY, Shape.Rigid.ShapeType[] shapes, int shapeIndex
    )
    {
        Shape.Rigid.ShapeType shapeType = shapes[shapeIndex];
        ref float scaleX = ref globalScalesX[shapeIndex];
        ref float scaleY = ref globalScalesY[shapeIndex];
        Span<float> vertsX = default; 
        Span<float> vertsY = default;

        int vertexCount = localVertices.AppendCounts[shapeIndex];
        int startIndex = FixedStrideArray.GetElementIndex(shapeIndex, localVertices.Stride, 0);                        
        for(int vertex = 0; vertex < vertexCount; vertex++){
            int currentIndex = vertex + startIndex;

            // transform the base/un-transformed vertice.
            Math.Math.TransformVector(localVertices.X[currentIndex], localVertices.Y[currentIndex], scaleX, scaleY,
                globalCosines[shapeIndex], globalSines[shapeIndex], globalPositionsX[shapeIndex], globalPositionsY[shapeIndex], 
                out float x, out float y
            );

            // store the newly transformed vertex into the world vertices array.
            // (TODO): this will need to be changed so that you can append directly to an entry element index
            // if you already know the element index. Create a new unsafe function for it.
            FsSoa_Vector2.Append(worldVertices, shapeIndex, x, y);
        }

        // set the new centroid.
        Shape.GetVerticesUnsafe(worldVertices, shapeIndex, ref vertsX, ref vertsY);
        ShapeUtils.CalculateCentroid(vertsX, vertsY, ref centroidsX[shapeIndex], ref centroidsY[shapeIndex]);

        switch (shapeType)
        {
            case Shape.Rigid.ShapeType.Rectangle:
                // set the new min and max vectors.
                Math.Math.GetMinMaxVectors(vertsX, vertsY, out minAabbsX[shapeIndex], out minAabbsY[shapeIndex], 
                    out maxAabbsX[shapeIndex], out maxAabbsY[shapeIndex]
                );
            break;

            case Shape.Rigid.ShapeType.Circle:
                globalRadii[shapeIndex] = Circle.ScaleRadius(localRadii[shapeIndex], scaleX, scaleY);
                // set the new min and max vectors. 
                Circle.GetMinMaxVectors(vertsX[0], vertsY[0], globalRadii[shapeIndex], 
                    out minAabbsX[shapeIndex], out minAabbsY[shapeIndex], out maxAabbsX[shapeIndex], out maxAabbsY[shapeIndex]
                );
            break;
            
            default:
                System.Diagnostics.Debug.Assert(false, "shape not implemented.");
            break;
        }
    }

    /// <summary>
    ///     constructs the bvh tree in relation to physics body data.
    /// </summary>
    /// <remarks>
    ///     All arrays must be of equal length and elements should be accessed via a <c>physicsBodyIndex</c> integer.
    /// </remarks>
    /// <param name="minAabbsX"></param>
    /// <param name="minAabbsY"></param>
    /// <param name="maxAabbsX"></param>
    /// <param name="maxAabbsY"></param>
    /// <param name="centroidsX"></param>
    /// <param name="centroidsY"></param>
    /// <param name="flags"></param>
    /// <param name="bvhCategories"></param>
    /// <param name="bvhLeafPaddings"></param>
    /// <param name="bvhLeafIndices"></param>
    /// <param name="bvh"></param>
    public static void ConstructBvhTree(SwapBackArray<int> activeBodies, IntrusiveList.Node[] nodes, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, float[] centroidsX, float[] centroidsY, int[] bvhCategories, float[] bvhLeafPaddings, 
        int[] bvhLeafIndices, BoundingVolumeHierarchy bvh
    )
    {
        // clear the previous bvh data.
        BoundingVolumeHierarchy.Clear(bvh);

        int count = activeBodies.Count;
        for(int i = 1; i < count; i++) // start at one to avoid Nil.
        {
            int bodyIndex = activeBodies[i];
            ref IntrusiveList.Node bodyNode = ref nodes[bodyIndex];
            int firstShapeIndex = bodyNode.FirstChild;
            if(firstShapeIndex == 0)
            {
                continue;
            }

            float minX;
            float minY;
            float maxX;
            float maxY;
            int shapeIndex = firstShapeIndex;
            while (true)
            {
                ref IntrusiveList.Node shapeNode = ref nodes[shapeIndex]; 

                ref float padding = ref bvhLeafPaddings[shapeIndex];
                minX = minAabbsX[shapeIndex] - padding;
                minY = minAabbsY[shapeIndex] - padding;
                maxX = maxAabbsX[shapeIndex] + padding;
                maxY = maxAabbsY[shapeIndex] + padding;

                // insert into the bvh.
                bvhLeafIndices[
                    Soa_Leaf.Append(bvh.Leaves, minX, minY, maxX, maxY, centroidsX[shapeIndex], centroidsY[shapeIndex], 
                        bvhCategories[shapeIndex]
                    )
                ] = shapeIndex;

                shapeIndex = shapeNode.NextSibling;
                if(shapeIndex == firstShapeIndex)
                {
                    break;
                }
            }
        }

        // construct the bvh with the new data.
        BoundingVolumeHierarchy.ConstructTree(bvh);
    }




    /******************
    
        Collider Collision Resolution.
    
    *******************/




    public static void ResolveColliderCollisions(IntrusiveList.Node[] nodes, CategorisedOverlapArray<int> subStepCollisionsToResolve, 
        float[] collisionDepths, float[] collisionNormalsX, float[] collisionNormalsY, float[] displacementsX, float[] displacementsY, 
        int collisionsStride
    )
    {
        // hoisting invariance.
        float depth;
        float displacementX;
        float displacementY;
        int ownerIndex; // always the solid collider.
        int otherIndex; // always the kinematic or other solid collider.

        Span<int> collisionsToResolve;

        // == resolve solid to solid collisions ==.
        collisionsToResolve = CategorisedOverlapArray.GetOverlaps(subStepCollisionsToResolve,
            CollisionResolutionCategory.Solid,
            CollisionResolutionCategory.Solid
        );

        for(int i = 0; i < collisionsToResolve.Length; i++)
        {
            int collisionIndex = collisionsToResolve[i];
            ownerIndex = collisionIndex / collisionsStride; // int div truncates the remainder, always giving the owner index.
            otherIndex = collisionIndex % collisionsStride;

            depth = collisionDepths[collisionIndex];
            displacementX = collisionNormalsX[collisionIndex] * depth * 0.5f;
            displacementY = collisionNormalsY[collisionIndex] * depth * 0.5f;

            ref IntrusiveList.Node ownerNode = ref nodes[ownerIndex]; 
            ref IntrusiveList.Node otherNode = ref nodes[otherIndex]; 
            
            // apply the displacement to the bodies of the shape.            
            displacementsX[otherNode.Parent] -= displacementX;
            displacementsY[otherNode.Parent] -= displacementY;
            displacementsX[ownerNode.Parent] += displacementX;
            displacementsY[ownerNode.Parent] += displacementY; 
        }

        // == resolve solid to kinematic collisions ==.

        collisionsToResolve = CategorisedOverlapArray.GetOverlaps(subStepCollisionsToResolve,
            CollisionResolutionCategory.Solid,
            CollisionResolutionCategory.Kinematic
        );

        for(int i = 0; i < collisionsToResolve.Length; i++)
        {            
            int collisionIndex = collisionsToResolve[i];
            ownerIndex = collisionIndex / collisionsStride; // int div truncates the remainder, always giving the owner index.
            depth = collisionDepths[collisionIndex];
            displacementX = collisionNormalsX[collisionIndex] * depth;
            displacementY = collisionNormalsY[collisionIndex] * depth;

            ref IntrusiveList.Node ownerNode = ref nodes[ownerIndex]; 

            // apply the displacement to the body of the shape.
            displacementsX[ownerNode.Parent] += displacementX;
            displacementsY[ownerNode.Parent] += displacementY; 
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para>
    ///     <para>Elements accesible by <c>collisionIndex</c>:</para>
    ///     <list type="bullet">
    ///         <item><paramref name="collisionNormalsX"/></item>
    ///         <item><paramref name="collisionNormalsY"/></item>
    ///         <item><paramref name="firstContactPointsX"/></item>
    ///         <item><paramref name="firstContactPointsY"/></item>
    ///         <item><paramref name="secondContactPointsX"/></item>
    ///         <item><paramref name="secondContactPointsY"/></item>
    ///         <item><paramref name="twoContactPoints"/></item>
    ///     </list>
    ///     <para>Elements accesible by <c>physicsBodyIndex</c>:</para>
    ///     <list type="bullet">
    ///         <item><paramref name="centroidsX"/></item>
    ///         <item><paramref name="centroidsY"/></item>
    ///         <item><paramref name="linearVelocitiesX"/></item>
    ///         <item><paramref name="linearVelocitiesY"/></item>
    ///         <item><paramref name="restitutions"/></item>
    ///         <item><paramref name="kineticFrictions"/></item>
    ///         <item><paramref name="staticFrictions"/></item>
    ///         <item><paramref name="angularVelocities"/></item>
    ///         <item><paramref name="masses"/></item>
    ///         <item><paramref name="inverseMasses"/></item>
    ///         <item><paramref name="inverseRotationalInertia"/></item>
    ///         <item><paramref name="flags"/></item>
    ///     </list>
    ///     <para><c>NOTE:</c> All spans are scratch buffers and should have a length of <see cref="MaxCollisionContactPoints"/></para>
    /// </remarks>
    /// <param name="contactPointsX">scratch buffer</param>
    /// <param name="contactPointsY">scratch buffer</param>
    /// <param name="distsAX">scratch buffer</param>
    /// <param name="distsAY">scratch buffer</param>
    /// <param name="distsBX">scratch buffer</param>
    /// <param name="distsBY">scratch buffer</param>
    /// <param name="impulseMagnitudes">scratch buffer</param>
    /// <param name="impulsesX">scratch buffer</param>
    /// <param name="impulsesY">scratch buffer</param>
    /// <param name="collisionsStride">the stride of elements in a collision entry.</param>
    public static void ResolveRigidShapeCollisions(CategorisedOverlapArray<int> collisionsToResolve, IntrusiveList.Node[] nodes,
        float[] collisionNormalsX, float[] collisionNormalsY, float[] firstContactPointsX, float[] firstContactPointsY,
        float[] globalPositionsX, float[] globalPositionsY, float[] localCentersOfMassX, float[] localCentersOfMassY, 
        float[] secondContactPointsX, float[] secondContactPointsY, float[] linearVelocitiesX, float[] linearVelocitiesY, 
        float[] restitutions, float[] kineticFrictions, float[] staticFrictions, float[] angularVelocities,
        float[] masses, float[] inverseMasses, float[] inverseRotationalInertia, bool[] twoContactPoints, bool[] rotationalResponses,
        Span<float> contactPointsX, Span<float> contactPointsY, Span<float> distsAX, Span<float> distsAY, 
        Span<float> distsBX, Span<float> distsBY, Span<float> impulseMagnitudes, Span<float> impulsesX, Span<float> impulsesY, 
        int collisionsStride
    )
    {
        Span<int> collisions;
        bool otherIsKinematic = false;

        collisions = CategorisedOverlapArray.GetOverlaps(
            collisionsToResolve, CollisionResolutionCategory.Solid, CollisionResolutionCategory.Solid
        );

        ResolveRigidBodyCollisions(collisions, nodes, collisionNormalsX, collisionNormalsY, firstContactPointsX, firstContactPointsY, 
            secondContactPointsX, secondContactPointsY, linearVelocitiesX, linearVelocitiesY, restitutions, kineticFrictions, 
            staticFrictions, angularVelocities, masses, inverseMasses, inverseRotationalInertia, localCentersOfMassX, 
            localCentersOfMassY, globalPositionsX, globalPositionsY, twoContactPoints, rotationalResponses, contactPointsX, 
            contactPointsY, distsAX, distsAY, distsBX, distsBY, impulseMagnitudes, impulsesX, impulsesY, collisionsStride, otherIsKinematic
        );

        collisions = CategorisedOverlapArray.GetOverlaps(
            collisionsToResolve, CollisionResolutionCategory.Solid, CollisionResolutionCategory.Kinematic
        );

        otherIsKinematic = true;

        ResolveRigidBodyCollisions(collisions, nodes, collisionNormalsX, collisionNormalsY, firstContactPointsX, firstContactPointsY, 
            secondContactPointsX, secondContactPointsY, linearVelocitiesX, linearVelocitiesY, restitutions, kineticFrictions, 
            staticFrictions, angularVelocities, masses, inverseMasses, inverseRotationalInertia, localCentersOfMassX, 
            localCentersOfMassY, globalPositionsX, globalPositionsY, twoContactPoints, rotationalResponses, contactPointsX, 
            contactPointsY, distsAX, distsAY, distsBX, distsBY, impulseMagnitudes, impulsesX, impulsesY, collisionsStride, otherIsKinematic
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResolveRigidBodyCollisions(Span<int> collisionsToResolve, IntrusiveList.Node[] nodes, 
        float[] normalsX, float[] normalsY, float[] firstContactPointsX, float[] firstContactPointsY, 
        float[] secondContactPointsX, float[] secondContactPointsY, float[] linearVelocitiesX, float[] linearVelocitiesY, 
        float[] restitutions, float[] kineticFrictions, float[] staticFrictions, float[] angularVelocities,
        float[] masses, float[] inverseMasses, float[] inverseRotationalInertia, float[] localCentersOfMassX, float[] localCentersOfMassY,
        float[] globalPositionsX, float[] globalPositionsY, bool[] twoContactPoints, bool[] rotationalResponses,
        Span<float> contactPointsX, Span<float> contactPointsY, Span<float> distsAX, Span<float> distsAY, 
        Span<float> distsBX, Span<float> distsBY, Span<float> impulseMagnitudes, Span<float> impulsesX, Span<float> impulsesY, 
        int collisionsStride, bool otherIsKinematic
    )
    {
        int contactPointsCount;
        float revNormalX = 0;
        float revNormalY = 0;

        for(int i = 0; i < collisionsToResolve.Length; i++)
        {
            int collisionIndex = collisionsToResolve[i];
            int ownerShapeIndex = collisionIndex / collisionsStride; // int div truncates the remainder, always giving the owner index.
            int otherShapeIndex = collisionIndex % collisionsStride;
            int ownerBodyIndex = nodes[ownerShapeIndex].Parent;
            int otherBodyIndex = nodes[otherShapeIndex].Parent;

            ref float normalX = ref normalsX[collisionIndex];
            ref float normalY = ref normalsY[collisionIndex];
            
            float ownerShapeCentroidX = globalPositionsX[ownerBodyIndex] + localCentersOfMassX[ownerBodyIndex];
            float otherShapeCentroidX = globalPositionsX[otherBodyIndex] + localCentersOfMassX[otherBodyIndex];
            
            float ownerShapeCentroidY = globalPositionsY[ownerBodyIndex] + localCentersOfMassY[ownerBodyIndex];
            float otherShapeCentroidY = globalPositionsY[otherBodyIndex] + localCentersOfMassY[otherBodyIndex];
            
            ref float ownerShapeRestitution = ref restitutions[ownerShapeIndex];
            ref float otherShapeRestitution = ref restitutions[otherShapeIndex];
            
            ref float ownerBodyAngVel = ref angularVelocities[ownerBodyIndex];
            ref float otherBodyAngVel = ref angularVelocities[otherBodyIndex];
            
            ref float ownerBodyLinVelX = ref linearVelocitiesX[ownerBodyIndex];
            ref float otherBodyLinVelX = ref linearVelocitiesX[otherBodyIndex];
            
            ref float ownerBodyLinVelY = ref linearVelocitiesY[ownerBodyIndex];
            ref float otherBodyLinVelY = ref linearVelocitiesY[otherBodyIndex];
            
            ref float ownerBodyInvMass = ref inverseMasses[ownerBodyIndex];
            ref float otherBodyInvMass = ref inverseMasses[otherBodyIndex];
            
            ref float ownerBodyInvRotInertia = ref inverseRotationalInertia[ownerBodyIndex];
            ref float otherBodyInvRotInertia = ref inverseRotationalInertia[otherBodyIndex];
            
            ref float ownerShapeStaticFriction = ref staticFrictions[ownerShapeIndex];
            ref float otherShapeStaticFriction = ref staticFrictions[otherShapeIndex];
            
            ref float ownerShapeKineticFriction = ref kineticFrictions[ownerShapeIndex];
            ref float otherShapeKineticFriction = ref kineticFrictions[otherShapeIndex];
            
            ref float ownerBodyMass = ref masses[ownerBodyIndex];
            ref float otherBodyMass = ref masses[otherBodyIndex];

            ref bool ownerShapeRotationalResponse = ref rotationalResponses[ownerShapeIndex];
            ref bool otherShapeRotationalResponse = ref rotationalResponses[otherShapeIndex];

            revNormalX = normalX * -1;
            revNormalY = normalY * -1;

            // note: these have to be set to zero.
            // this function resuses these stack allocated spans
            // so without this, the loop could operate on garbage data from the previous step.
            impulsesX.Clear();
            impulsesY.Clear();
            impulseMagnitudes.Clear();
            contactPointsX.Clear();
            contactPointsY.Clear();

            if (twoContactPoints[collisionIndex])
            {
                contactPointsCount = 2;
                contactPointsX[0] = firstContactPointsX[collisionIndex];
                contactPointsX[1] = secondContactPointsX[collisionIndex];
                contactPointsY[0] = firstContactPointsY[collisionIndex];
                contactPointsY[1] = secondContactPointsY[collisionIndex];
            }
            else
            {
                contactPointsCount = 1;  
                contactPointsX[0] = firstContactPointsX[collisionIndex];
                contactPointsY[0] = firstContactPointsY[collisionIndex];
            }

            if(ownerShapeRotationalResponse || otherShapeRotationalResponse)
            {
                ResolveRigidShapeCollision_Rotational(impulseMagnitudes, contactPointsX,
                    impulsesX, impulsesY, distsAX, distsAY, distsBX, distsBY,
                    contactPointsY, ref ownerBodyLinVelX, ref otherBodyLinVelX, ref ownerBodyLinVelY, 
                    ref otherBodyLinVelY, ref revNormalX, ref revNormalY, ref ownerShapeRestitution, ref otherShapeRestitution,
                    ref ownerShapeCentroidX, ref otherShapeCentroidX, ref ownerShapeCentroidY, ref otherShapeCentroidY, ref ownerBodyInvMass, 
                    ref otherBodyInvMass, ref ownerBodyAngVel, ref otherBodyAngVel, ref ownerBodyInvRotInertia, 
                    ref otherBodyInvRotInertia, ref ownerShapeRotationalResponse, ref otherShapeRotationalResponse, contactPointsCount, 
                    otherIsKinematic
                );
            }
            else
            {
                ResolveRigidBodyCollision_Basic(impulseMagnitudes, ref ownerBodyLinVelX, 
                    ref otherBodyLinVelX, ref ownerBodyLinVelX, ref otherBodyLinVelY, ref revNormalX, 
                    ref revNormalY, ref ownerShapeRestitution, ref otherShapeRestitution, ref ownerBodyInvMass, ref otherBodyInvMass,
                    ref ownerBodyMass, ref otherBodyMass, contactPointsCount, otherIsKinematic
                );
            }

            ResolveRigidBodyFrictionCollision(impulseMagnitudes, contactPointsX, impulsesX, impulsesY, distsAX, distsAY, distsBX, distsBY,
                contactPointsY, ref ownerBodyLinVelX, ref otherBodyLinVelX, ref ownerBodyLinVelY, ref otherBodyLinVelY, 
                ref revNormalX, ref revNormalY, ref ownerShapeStaticFriction, ref otherShapeStaticFriction, ref ownerShapeKineticFriction, 
                ref otherShapeKineticFriction, ref ownerShapeCentroidX, ref otherShapeCentroidX, ref ownerShapeCentroidY, ref otherShapeCentroidY, 
                ref ownerBodyInvMass, ref ownerBodyInvRotInertia, ref otherBodyInvRotInertia, ref otherBodyInvMass, 
                ref ownerBodyAngVel, ref otherBodyAngVel, ref ownerShapeRotationalResponse, ref otherShapeRotationalResponse, 
                contactPointsCount, otherIsKinematic
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResolveRigidBodyCollision_Basic(Span<float> impulseMagnitudes, ref float ownerBodyLinVelX, 
        ref float otherBodyLinVelX, ref float ownerBodyLinVelY, ref float otherBodyLinVelY, ref float revNormalX, 
        ref float revNormalY, ref float ownerShapeRestitution, ref float otherShapeRestitution, ref float ownerBodyInvMass, 
        ref float otherBodyInvMass, ref float ownerBodyMass, ref float otherBodyMass, int contactPointsCount, bool otherShapeIsKinematic
    )
    {
        for(int j = 0; j < contactPointsCount; j++)
        {                    
            float relativeVelocityX = otherBodyLinVelX - ownerBodyLinVelX;
            float relativeVelocityY = otherBodyLinVelY - ownerBodyLinVelY;

            // the magnitude of the relative velocity relative to the normal
            float magnitude = Math.Math.Dot(relativeVelocityX, relativeVelocityY, revNormalX, revNormalY);

            if(magnitude > 0)
            {
                continue;
            }

            float restitution = MathF.Min(ownerShapeRestitution, otherShapeRestitution);

            // magnitude of the impulse
            float impulseMagnitude = -(1f + restitution) * magnitude;
            impulseMagnitude /= ownerBodyInvMass + otherBodyInvMass;
            
            // divide by the contact point count to ensure that impulse is evenly spread 
            // across all contact points.
            impulseMagnitude /= (float)contactPointsCount;

            impulseMagnitudes[j] = impulseMagnitude;
        }

        float impulseForceX;
        float impulseForceY;

        for(int j = 0; j < contactPointsCount; j++)
        {                    
            float mag = impulseMagnitudes[j];
            impulseForceX = -(mag / ownerBodyMass * revNormalX);
            impulseForceY = -(mag / ownerBodyMass * revNormalY);
            ownerBodyLinVelX += impulseForceX;
            ownerBodyLinVelY += impulseForceY;

            if(otherShapeIsKinematic)
            {
                continue;
            }

            impulseForceX = mag / otherBodyMass * revNormalX;
            impulseForceY = mag / otherBodyMass * revNormalY;
            otherBodyLinVelX += impulseForceX;
            otherBodyLinVelY += impulseForceY;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResolveRigidShapeCollision_Rotational(Span<float> impulseMagnitudes, Span<float> contactPointsX,
        Span<float> impulsesX, Span<float> impulsesY, Span<float> distsAX, Span<float> distsAY, Span<float> distsBX, Span<float> distsBY,
        Span<float> contactPointsY, ref float ownerBodyLinVelX, ref float otherBodyLinVelX, ref float ownerBodyLinVelY, 
        ref float otherBodyLinVelY, ref float revNormalX, ref float revNormalY, ref float ownerShapeRestitution, 
        ref float otherShapeRestitution, ref float ownerShapeCentroidX, ref float otherShapeCentroidX, ref float ownerShapeCentroidY, 
        ref float otherShapeCentroidY, ref float ownerBodyInvMass, ref float otherBodyInvMass, ref float ownerBodyAngVel, 
        ref float otherBodyAngVel, ref float ownerBodyInvRotInertia, ref float otherBodyInvRotInertia, ref bool ownerRotationalResponse, 
        ref bool otherRotationalResponse, int contactPointsCount, bool otherShapeIsKinematic
    )
    {
        float restitution = MathF.Min(ownerShapeRestitution, otherShapeRestitution);
                
        for(int j = 0; j < contactPointsCount; j++)
        {
            float contactPointX = contactPointsX[j];
            float contactPointY = contactPointsY[j];

            // get the angular velocity to travel in.
            distsAX[j] = contactPointX - ownerShapeCentroidX;
            distsAY[j] = contactPointY - ownerShapeCentroidY;
            distsBX[j] = contactPointX - otherShapeCentroidX;
            distsBY[j] = contactPointY - otherShapeCentroidY;            
            
            float perpendicularAX = -distsAY[j];
            float perpendicularAY = distsAX[j];
            float perpendicularBX = -distsBY[j];
            float perpendicularBY = distsBX[j];

            float angularVelocityAX = perpendicularAX * ownerBodyAngVel;
            float angularVelocityAY = perpendicularAY * ownerBodyAngVel;
            float angularVelocityBX = perpendicularBX * otherBodyAngVel;
            float angularVelocityBY = perpendicularBY * otherBodyAngVel;

            float relativeVelocityX = (otherBodyLinVelX + angularVelocityBX) - (ownerBodyLinVelX + angularVelocityAX);
            float relativeVelocityY = (otherBodyLinVelY + angularVelocityBY) - (ownerBodyLinVelY + angularVelocityAY);
            
            // the magnitude of the relative velocity relative to the normal
            float magnitude = Math.Math.Dot(relativeVelocityX, relativeVelocityY, revNormalX, revNormalY);

            if(magnitude > 0)
            {
                continue;
            }

            // calculate the denominator.
            float perpADotNormal = Math.Math.Dot(perpendicularAX, perpendicularAY, revNormalX, revNormalY);
            float perpBDotNormal = Math.Math.Dot(perpendicularBX, perpendicularBY, revNormalX, revNormalY);
            float denominator = ownerBodyInvMass + otherBodyInvMass + 
                (perpADotNormal * perpADotNormal) * ownerBodyInvRotInertia +
                (perpBDotNormal * perpBDotNormal) * otherBodyInvRotInertia;

            // magnitude of the impulse
            float impulseMagnitude = -(1f + restitution) * magnitude;
            impulseMagnitude /= denominator;

            // divide by the contact point count to ensure that impulse is evenly spread 
            // across all contact points.
            impulseMagnitude /= (float)contactPointsCount;
            
            // save the impulse magnitude for later friction resolution.
            impulseMagnitudes[j] = impulseMagnitude;
            impulsesX[j] = impulseMagnitude * revNormalX;
            impulsesY[j] = impulseMagnitude * revNormalY;

        }

        // keep these outside the for loop so they dont allocate each time.
        float impulseX;
        float impulseY;
        float distAX;
        float distAY;
        float distBX;
        float distBY;

        for(int i = 0; i < contactPointsCount; i++)
        {                
            impulseX = impulsesX[i];
            impulseY = impulsesY[i];

            // cross producting the dist and impulse gives a value indicating
            // how much angular velocity - in radians - is needed to be applied based on the impulse direction.
            // this is because cross producting two directions that are parallel to eachother, results in zero.
            // which means that there should be no rotation if the collision is head on.
            // but if the closer the two directions come to being perpendicular to one another,
            // the larger the angular impulse will be, causing the body to rotate.

            ownerBodyLinVelX += -impulseX * ownerBodyInvMass;
            ownerBodyLinVelY += -impulseY * ownerBodyInvMass;

            if(ownerRotationalResponse)
            {
                distAX = distsAX[i];
                distAY = distsAY[i];
                ownerBodyAngVel += -Math.Math.Cross(distAX, distAY, impulseX, impulseY) * ownerBodyInvRotInertia;
            }

            if (otherShapeIsKinematic)
            {
                continue;
            }

            otherBodyLinVelX += impulseX * otherBodyInvMass;
            otherBodyLinVelY += impulseY * otherBodyInvMass;

            if(otherRotationalResponse)
            {
                distBX = distsBX[i];
                distBY = distsBY[i];
                otherBodyAngVel += Math.Math.Cross(distBX, distBY, impulseX, impulseY) * otherBodyInvRotInertia;
            }
        }        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResolveRigidBodyFrictionCollision(Span<float> impulseMagnitudes, Span<float> contactPointsX,
        Span<float> impulsesX, Span<float> impulsesY, Span<float> distsAX, Span<float> distsAY, Span<float> distsBX, Span<float> distsBY,
        Span<float> contactPointsY, ref float ownerLinearVelocityX, ref float otherLinearVelocityX, ref float ownerLinearVelocityY, 
        ref float otherLinearVelocityY, ref float revNormalX, ref float revNormalY, ref float ownerStaticFriction, ref float otherStaticFriction,
        ref float ownerKineticFriction, ref float otherKineticFriction, ref float ownerCentroidX, ref float otherCentroidX, ref float ownerCentroidY, ref float otherCentroidY, ref float ownerInverseMass, 
        ref float ownerInverseRotationalInertia, ref float otherInverseRotationalInertia, ref float otherInverseMass, 
        ref float ownerAngularVelocity, ref float otherAngularVelocity, ref bool ownerRotationalResponse, ref bool otherRotationalResponse, 
        int contactPointsCount, bool otherIsKinematic
    )
    {
        // get an approximation of the friction values.
        // this is faster than the actual physics way.
        float staticFriction = 0;
        float kineticFriction = 0;

        staticFriction = (ownerStaticFriction + otherStaticFriction) * 0.5f;
        kineticFriction = (ownerKineticFriction + otherKineticFriction) * 0.5f;
        
        for(int j = 0; j < contactPointsCount; j++)
        {
            float contactPointX = contactPointsX[j];
            float contactPointY = contactPointsY[j];

            // get the angular velocity to travel in.
            distsAX[j] = contactPointX - ownerCentroidX;
            distsAY[j] = contactPointY - ownerCentroidY;
            distsBX[j] = contactPointX - otherCentroidX;
            distsBY[j] = contactPointY - otherCentroidY;            
            
            float perpendicularAX = -distsAY[j];
            float perpendicularAY = distsAX[j];
            float perpendicularBX = -distsBY[j];
            float perpendicularBY = distsBX[j];

            float angularVelocityAX = perpendicularAX * ownerAngularVelocity;
            float angularVelocityAY = perpendicularAY * ownerAngularVelocity;
            float angularVelocityBX = perpendicularBX * otherAngularVelocity;
            float angularVelocityBY = perpendicularBY * otherAngularVelocity;

            float relativeVelocityX = (otherLinearVelocityX + angularVelocityBX) - (ownerLinearVelocityX + angularVelocityAX);
            float relativeVelocityY = (otherLinearVelocityY + angularVelocityBY) - (ownerLinearVelocityY + angularVelocityAY);

            // this is the direction the body is travelling in along the contact point surface.
            float relativeDotNormal = Math.Math.Dot(relativeVelocityX, relativeVelocityY, revNormalX, revNormalY);
            float tangentX = relativeVelocityX - relativeDotNormal * revNormalX;
            float tangentY = relativeVelocityY - relativeDotNormal * revNormalY;

            if(Math.Math.NearlyEqual((tangentX * tangentX) + (tangentY * tangentY), 0, 1e-12f))
            {
                continue;
            }

            Math.Math.Normalise(tangentX, tangentY, out tangentX, out tangentY);

            // calculate the denominator.
            float perpADotTangent = Math.Math.Dot(perpendicularAX, perpendicularAY, tangentX, tangentY);
            float perpBDotTangent = Math.Math.Dot(perpendicularBX, perpendicularBY, tangentX, tangentY);
            float denominator = ownerInverseMass + otherInverseMass + 
                (perpADotTangent * perpADotTangent) * ownerInverseRotationalInertia +
                (perpBDotTangent * perpBDotTangent) * otherInverseRotationalInertia;

            // Calculate the DESIRED friction magnitude to stop all sliding.
            float frictionImpulseMag = -Math.Math.Dot(relativeVelocityX, relativeVelocityY, tangentX, tangentY) / denominator;

            // Coulomb's Law:
            // Limit that desire by the static friction. 
            float maxFriction = impulseMagnitudes[j] * staticFriction;

            // the the desired friction amount is greater than static friction
            // that means that the object should be sliding with kinetic friction.
            if (Math.Math.Abs(frictionImpulseMag) > maxFriction)
            {
                // Note: We multiply by the SIGN of frictionImpulseMag to keep the direction correct.
                frictionImpulseMag = (impulseMagnitudes[j] * kineticFriction) * MathF.Sign(frictionImpulseMag);
            }

            // Apply the capped magnitude to the tangent vector
            impulsesX[j] = frictionImpulseMag * tangentX;
            impulsesY[j] = frictionImpulseMag * tangentY;
        }

        // keep these outside the for loop so they dont allocate each time.
        float impulseX;
        float impulseY;
        float distAX;
        float distAY;
        float distBX;
        float distBY;

        for(int j = 0; j < contactPointsCount; j++)
        {                
            impulseX = impulsesX[j];
            impulseY = impulsesY[j];

            // cross producting the dist and impulse gives a value indicating
            // how much angular velocity - in radians - is needed to be applied based on the impulse direction.
            // this is because cross producting two directions that are parallel to eachother, results in zero.
            // which means that there should be no rotation if the collision is head on.
            // but if the closer the two directions come to being perpendicular to one another,
            // the larger the angular impulse will be, causing the body to rotate.

            ownerLinearVelocityX += -impulseX * ownerInverseMass;
            ownerLinearVelocityY += -impulseY * ownerInverseMass;

            if(ownerRotationalResponse)
            {
                distAX = distsAX[j];
                distAY = distsAY[j];
                ownerAngularVelocity += -Math.Math.Cross(distAX, distAY, impulseX, impulseY) * ownerInverseRotationalInertia;
            }

            if (otherIsKinematic)
            {
                continue;
            }

            otherLinearVelocityX += impulseX * otherInverseMass;
            otherLinearVelocityY += impulseY * otherInverseMass;

            if(otherRotationalResponse)
            {
                distBX = distsBX[j];
                distBY = distsBY[j];
                otherAngularVelocity += Math.Math.Cross(distBX, distBY, impulseX, impulseY) * otherInverseRotationalInertia;
            }       
        } 
    }

    // / <summary>
    // / Clears all forces and velocities being applied to a rigidbody.
    // / </summary>
    /// <param name="state">the phsysics system state.</param>
    /// <param name="bodyIndex">the index of the physics body to stop.</param>
    public static void ClearForcesAndVelocities(PhysicsSystemState state, int bodyIndex)
    {
        state.LinearVelocities.X[bodyIndex] = 0;
        state.LinearVelocities.Y[bodyIndex] = 0;
        state.AngularVelocities[bodyIndex] = 0;
        state.Forces.X[bodyIndex] = 0;
        state.Forces.Y[bodyIndex] = 0;
    }


    /// <summary>
    ///     Formats overlap data so that the <c>owner</c> of an overlap is always the <c>solid</c> collider.
    /// </summary>
    /// <param name="overlaps">the overlap instance to format.</param>
    /// <param name="bvhLeafIndices">the mapping of bvh leaf indices onto a physics body.</param>
    /// <param name="bvhCategories">the categories of all physics bodies when being put into the bvh.</param>
    /// <exception cref="Exception"></exception>
    public static void FormatCategorisedOverlaps(CategorisedLeafOverlaps overlaps, Span<int> bvhLeafIndices, Span<int> bvhCategories)
    {
        // hoisting invariance.
        int temp;
        int otherCategory;
        int ownerCategory;

        for(int i = 0; i < PhysicsBody.Category.Count; i++)
        {
            for(int j = i; j < PhysicsBody.Category.Count; j++)
            {    
                OverlapInfo info = CategorisedLeafOverlaps.GetOverlaps(overlaps, i, j);
                for(int w = 0; w < info.Length; w++)
                {
                    // get the data about the owner and other.
                    ref int ownerLeaf = ref info.OwnerLeafIndices[w];
                    ref int otherLeaf = ref info.OtherLeafIndices[w];
                    ref int ownerIndex = ref bvhLeafIndices[ownerLeaf];
                    ref int otherIndex = ref bvhLeafIndices[otherLeaf];
                    ownerCategory = bvhCategories[ownerIndex];
                    otherCategory = bvhCategories[otherIndex];

                    // order the leaves in ascending order.
                    if(ownerCategory > otherCategory)
                    {                
                        temp = ownerLeaf;
                        ownerLeaf = otherLeaf;
                        otherLeaf = temp;
                    }
                }
            }
        }
    }




    /******************
    
        Step Preparation
    
    *******************/



    /// <summary>
    ///     Deep copies the current position to the previous position arrays.
    /// </summary>
    /// <remarks>
    ///     All passed arrays should be the same length.
    /// </remarks>
    /// <param name="currentPosX"></param>
    /// <param name="currentPosY"></param>
    /// <param name="previousPosX"></param>
    /// <param name="previousPosY"></param>
    public static void SetPreviousPositions(float[] currentPosX, float[] currentPosY, float[] previousPosX, float[] previousPosY)
    {
        int simdSize = System.Numerics.Vector<float>.Count;
        int i = 0;
        
        float length = currentPosX.Length;

        // bulk.
        for(; i <= length - simdSize; i += simdSize)
        {
            System.Numerics.Vector<float> cX = System.Numerics.Vector.LoadUnsafe(ref currentPosX[i]);
            System.Numerics.Vector<float> cY = System.Numerics.Vector.LoadUnsafe(ref currentPosY[i]);
            System.Numerics.Vector.StoreUnsafe(cX, ref previousPosX[i]);
            System.Numerics.Vector.StoreUnsafe(cY, ref previousPosY[i]);
        }

        // tail end.
        for(int j = i; j < length; j++)
        {
            previousPosX[j] = currentPosX[j];
            previousPosY[j] = currentPosY[j];
        }
    }

    public static void CalculateBvhLeafPadding(float[] currentPositionX, float[] currentPositionY, 
        float[] previousPositionX, float[] previousPositionY, SwapBackArray<int> active, 
        float[] bvhLeafPadding, float deltaTime
    )
    {
        for(int i = 0; i < active.Count; i++)
        {            
            int index = active[i];
            float deltaMovementX = currentPositionX[index] - previousPositionX[index];
            float deltaMovementY = currentPositionY[index] - previousPositionY[index];
            float deltaMovement = Math.Math.Max(Math.Math.Abs(deltaMovementX),Math.Math.Abs(deltaMovementY));   
            float timeFactor = 1 + deltaTime;
            bvhLeafPadding[index] = deltaMovement * timeFactor;
        }
    }




    /******************
    
        Body
    
    *******************/




    public static class Body
    {
        
        public static GenIdResult Allocate(State state, Transform globalTransform, bool gravityAffected, ref GenId entityId)
        {
            GenIdResult result = EntityRegistry.Allocate(state.Entities, ref entityId);

            if(result != GenIdResult.Ok)
            {
                return result;
            }

            int bodyIndex = GenId.GetIndex(entityId);
            SetActiveUnsafe(state, bodyIndex, true);

            Soa_Transform.CopyTransformToSoa(state.GlobalTransforms, ref globalTransform, bodyIndex);
            
            // clear any garbage data from the previously allocated body.
            state.ShapeCollisionDisplacements.X[bodyIndex] = 0;
            state.ShapeCollisionDisplacements.Y[bodyIndex] = 0;
            state.Masses[bodyIndex] = 0;
            state.InverseMasses[bodyIndex] = 0; 
            state.EntityTypes[bodyIndex] = EntityType.Body;
            state.GravityAffected[bodyIndex] = gravityAffected;
            ClearForcesAndVelocities(state, bodyIndex);

            IntrusiveList.AddToTree(state.BodyHierarchy, bodyIndex);
            return result;
        }

        public static GenIdResult Deallocate(State state, GenId genId)
        {
            GenIdResult result = EntityRegistry.Deallocate(state.Entities, genId);

            if(result == GenIdResult.Ok)
            {            
                int index = GenId.GetIndex(genId);

                // this is here temporarily and SHOULD be removed.
                FsSoa_Vector2.ClearEntryAppendCount(state.BaseVertices, index);

                Shape.DecrementCategoryCounter(state, state.Categories[index]);

                state.GravityAffected[index] = false;

                SetActiveUnsafe(state, GenId.GetIndex(genId), false);
            }
            
            return result;
        }

        public static GenIdResult SetActive(State state, GenId entityId, bool isActive)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Body)
            {
                return GenIdResult.NotAllocated;
            }

            return SetActive(state, entityId, isActive);
        }

        public static bool IsActive(State state, GenId entityId, ref GenIdResult resultOutput)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Body)
            {
                resultOutput = GenIdResult.NotAllocated;
                return false;
            }

            return IsActive(state, entityId, ref resultOutput);
        }

        public static GenIdResult SetLocalTransform(State state, GenId entityId, Transform newTransform)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Body)
            {
                return GenIdResult.NotAllocated;
            }

            return SetLocalTransform(state, entityId, newTransform);
        }

        public static Vector2 GetLinearVelocity(State state, GenId entityId, ref GenIdResult resultOutput)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Body)
            {
                resultOutput = GenIdResult.NotAllocated;
                return default;
            }

            return GetLinearVelocity(state, entityId, ref resultOutput);
        }

        public static GenIdResult ImpulseForce(State state, Vector2 force, GenId entityId)
        {
            if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
            {
                return GenIdResult.StaleGenId;
            }

            int index = GenId.GetIndex(entityId);
            
            if(IsActiveUnsafe(state, index) == false)
            {
                return GenIdResult.NotActive;
            }

            state.LinearVelocities.X[index] += force.X;
            state.LinearVelocities.Y[index] += force.Y;

            return GenIdResult.Ok;
        }

        public static void ClearForcesAndVelocities(State state, int entityIndex)
        {
            state.LinearVelocities.X[entityIndex] = 0;
            state.LinearVelocities.Y[entityIndex] = 0;
            state.AngularVelocities[entityIndex] = 0;
            state.Forces.X[entityIndex] = 0;
            state.Forces.Y[entityIndex] = 0;
        }

        /// <summary>
        ///     Calculates the inverse/mass, inverse/rotational inertia, and center of mass for a rigidbody. 
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para></para>
        /// </remarks>
        public static void IntegrateShapePropertiesUnsafe(State state, int bodyIndex)
        {
            // fallback to the global position if there are not valid rigid shapes associated with the body.
            float centerOfMassX = 0;
            float centerOfMassY = 0;
            float totalMass = 0;
            float totalInverseMass = 0;
            float totalRotationalInertia = 0;
            float totalInverseRotationalInertia = 0;

            IntrusiveList.Node[] nodes = state.BodyHierarchy.Nodes;

            ref IntrusiveList.Node bodyNode = ref nodes[bodyIndex];
            int firstShapeIndex = bodyNode.FirstChild;

            if(firstShapeIndex == 0)
            {
                goto WriteData;
            }

            {   // calculate center of mass & total mass.
                
                int shapeIndex = firstShapeIndex;
                centerOfMassX = 0;
                centerOfMassY = 0;
                while (true)
                {
                    float mass = state.Masses[shapeIndex];
                    centerOfMassX += mass * state.Centroids.X[shapeIndex];
                    centerOfMassY += mass * state.Centroids.Y[shapeIndex]; 
                    totalMass += mass;

                    ref IntrusiveList.Node shapeNode = ref nodes[shapeIndex];
                    int nextShapeIndex = shapeNode.NextSibling;
                    if(nextShapeIndex == firstShapeIndex)
                    {
                        // center of mass in world space.
                        // note: use world-sapce coordinates first as shape centroids are in world space.
                        centerOfMassX /= totalMass;
                        centerOfMassY /= totalMass;

                        totalInverseMass = 1f/totalMass; 
                        
                        break;
                    }

                    shapeIndex = nextShapeIndex; 
                } 
            }

            {   // calculate total rotational inertia.
                
                int shapeIndex = firstShapeIndex;
                while (true)
                {
                    float distSqrd = Math.Math.DistanceSquared(state.Centroids.X[shapeIndex], state.Centroids.Y[shapeIndex], 
                        centerOfMassX, centerOfMassY
                    );

                    totalRotationalInertia += state.RotationalInertia[shapeIndex] + (state.Masses[shapeIndex] * distSqrd); 

                    ref IntrusiveList.Node shapeNode = ref nodes[shapeIndex];
                    int nextShapeIndex = shapeNode.NextSibling;
                    if(nextShapeIndex == firstShapeIndex)
                    {
                        // move center of mass into local space.
                        centerOfMassX -= state.GlobalTransforms.Positions.X[shapeIndex];
                        centerOfMassY -= state.GlobalTransforms.Positions.Y[shapeIndex];

                        totalInverseRotationalInertia = 1f/totalRotationalInertia;

                        break;
                    }

                    shapeIndex = nextShapeIndex;
                }
            }

            WriteData:
            {   
                state.Masses[bodyIndex] = totalMass;
                state.InverseMasses[bodyIndex] = totalInverseMass;
                state.LocalCentersOfMass.X[bodyIndex] = centerOfMassX;
                state.LocalCentersOfMass.Y[bodyIndex] = centerOfMassY;
                state.RotationalInertia[bodyIndex] = totalRotationalInertia;
                state.InverseRotationalInertia[bodyIndex] = totalInverseRotationalInertia;
            }
        }
    }




    /******************
    
        class CollisionShapes
    
    *******************/




    public class Shape
    {




        /******************
        
            Category Management.
        
        *******************/




        /// <summary>
        ///         Note: ordering matters here, value 0 is highest precedence in 
        ///         <see cref="PhysicsSystem.FormatCategorisedOverlaps(Howl.CategorisedLeafOverlaps, System.Span{int}, System.Span{int})"/>.
        /// </summary>
        public static class Category
        {

            // padding to allowing for zero intialisation,
            public const int Padding0 = 0;
            public const int Padding1 = 1;
            public const int Padding2 = 2;

            public const int SolidPolygonRigidBody      = 0;
            public const int SolidCircleRigidBody       = 1;
            public const int SolidCapsuleRigidBody      = 2;
            
            public const int TriggerPolygonRigidBody    = 3;
            public const int TriggerCircleRigidBody     = 4;
            public const int TriggerCapsuleRigidBody    = 5;

            // note: everything greater than KinematicPolygonRigidBody
            // is not apart of the rigid body movement step.

            public const int KinematicPolygonRigidBody  = 6;
            public const int KinematicCircleRigidBody   = 7;
            public const int KinematicCapsuleRigidBody  = 8;
            
            public const int SolidPolygonCollider       = 9;
            public const int SolidCircleCollider        = 10;
            public const int SolidCapsuleCollider       = 11;
            
            public const int TriggerPolygonCollider     = 12;
            public const int TriggerCircleCollider      = 13;
            public const int TriggerCapsuleCollider     = 14;

            public const int KinematicPolygonCollider   = 15;
            public const int KinematicCircleCollider    = 16;
            public const int KinematicCapsuleCollider   = 17;




            /******************
            
                Util
            
            *******************/




            public const int Count = 18;

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsPolygon(int category)
            {
                return category < Count && category % 3 == 0;    
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsCircle(int category)
            {
                return category < Count && category % 3 == 1;    
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsCapsule(int category)
            {
                return category < Count && category % 3 == 2;    
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsTrigger(int category)
            {
                return 
                (category >= TriggerPolygonRigidBody && category <= TriggerCapsuleRigidBody) || 
                (category >= TriggerPolygonCollider && category <=  TriggerCapsuleCollider);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsSolid(int category)
            {
                return 
                (category >= SolidPolygonRigidBody && category <= SolidCapsuleRigidBody) || 
                (category >= SolidPolygonCollider && category <= SolidCapsuleCollider);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsKinematic(int category)
            {
                return 
                (category >= KinematicPolygonRigidBody && category <= KinematicCapsuleRigidBody) || 
                (category >= KinematicPolygonCollider && category <=  KinematicCapsuleCollider);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsRigidBody(int category)
            {
                return category >= SolidPolygonRigidBody && category <= KinematicCapsuleRigidBody;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetToRigidBody(ref int category)
            {
                category = category switch
                {
                    SolidPolygonRigidBody   => SolidPolygonRigidBody,
                    SolidCircleRigidBody    => SolidCircleRigidBody ,
                    SolidCapsuleRigidBody   => SolidCapsuleRigidBody,

                    TriggerPolygonRigidBody => TriggerPolygonRigidBody,
                    TriggerCircleRigidBody  => TriggerCircleRigidBody ,
                    TriggerCapsuleRigidBody => TriggerCapsuleRigidBody,

                    KinematicPolygonRigidBody  => KinematicPolygonRigidBody,
                    KinematicCircleRigidBody   => KinematicCircleRigidBody ,
                    KinematicCapsuleRigidBody  => KinematicCapsuleRigidBody,
                    
                    SolidPolygonCollider => SolidPolygonRigidBody,
                    SolidCircleCollider  => SolidCircleRigidBody ,
                    SolidCapsuleCollider => SolidCapsuleRigidBody,

                    TriggerPolygonCollider  => TriggerPolygonRigidBody,
                    TriggerCircleCollider   => TriggerCircleRigidBody ,
                    TriggerCapsuleCollider  => TriggerCapsuleRigidBody,

                    KinematicPolygonCollider   => KinematicPolygonRigidBody,
                    KinematicCircleCollider    => KinematicCircleRigidBody ,
                    KinematicCapsuleCollider   => KinematicCapsuleRigidBody,

                    _ => throw new Exception()
                };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetToCollider(ref int category)
            {
                category = category switch
                {
                    SolidPolygonRigidBody   => SolidPolygonCollider,
                    SolidCircleRigidBody    => SolidCircleCollider ,
                    SolidCapsuleRigidBody   => SolidCapsuleCollider,

                    TriggerPolygonRigidBody => TriggerPolygonCollider,
                    TriggerCircleRigidBody  => TriggerCircleCollider ,
                    TriggerCapsuleRigidBody => TriggerCapsuleCollider,

                    KinematicPolygonRigidBody  => KinematicPolygonCollider,
                    KinematicCircleRigidBody   => KinematicCircleCollider ,
                    KinematicCapsuleRigidBody  => KinematicCapsuleCollider,
                    
                    SolidPolygonCollider => SolidPolygonCollider,
                    SolidCircleCollider  => SolidCircleCollider ,
                    SolidCapsuleCollider => SolidCapsuleCollider,

                    TriggerPolygonCollider  => TriggerPolygonCollider,
                    TriggerCircleCollider   => TriggerCircleCollider ,
                    TriggerCapsuleCollider  => TriggerCapsuleCollider,

                    KinematicPolygonCollider   => KinematicPolygonCollider,
                    KinematicCircleCollider    => KinematicCircleCollider ,
                    KinematicCapsuleCollider   => KinematicCapsuleCollider,

                    _ => throw new Exception()
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void IncrementCategoryCounter(State state, int category)
        {
            switch (category)
            {
                case Category.SolidPolygonRigidBody: state.SolidPolygonRigidBodyCount++; break;
                case Category.SolidCircleRigidBody: state.SolidCircleRigidBodyCount++; break;
                case Category.SolidCapsuleRigidBody: state.SolidCapsuleRigidBodyCount++; break;
                
                case Category.TriggerPolygonRigidBody: state.TriggerPolygonRigidBodyCount++; break;
                case Category.TriggerCircleRigidBody: state.TriggerCircleRigidBodyCount++; break;
                case Category.TriggerCapsuleRigidBody: state.TriggerCapsuleRigidBodyCount++; break;

                case Category.KinematicPolygonRigidBody: state.KinematicPolygonRigidBodyCount++; break;
                case Category.KinematicCircleRigidBody: state.KinematicCircleRigidBodyCount++; break;
                case Category.KinematicCapsuleRigidBody: state.KinematicCapsuleRigidBodyCount++; break;
                
                case Category.SolidPolygonCollider: state.SolidPolygonColliderCount++; break;
                case Category.SolidCircleCollider: state.SolidCircleColliderCount++; break;
                case Category.SolidCapsuleCollider: state.SolidCapsuleColliderCount++; break;
                
                case Category.TriggerPolygonCollider: state.TriggerPolygonColliderCount++; break;
                case Category.TriggerCircleCollider: state.TriggerCircleColliderCount++; break;
                case Category.TriggerCapsuleCollider: state.TriggerCapsuleColliderCount++; break;

                case Category.KinematicPolygonCollider: state.KinematicPolygonColliderCount++; break;
                case Category.KinematicCircleCollider: state.KinematicCircleColliderCount++; break;
                case Category.KinematicCapsuleCollider: state.KinematicCapsuleColliderCount++; break;

                default:
                    throw new Exception();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void DecrementCategoryCounter(State state, int category)
        {
            switch (category)
            {
                case Category.SolidPolygonRigidBody: state.SolidPolygonRigidBodyCount--; break;
                case Category.SolidCircleRigidBody: state.SolidCircleRigidBodyCount--; break;
                case Category.SolidCapsuleRigidBody: state.SolidCapsuleRigidBodyCount--; break;
                
                case Category.TriggerPolygonRigidBody: state.TriggerPolygonRigidBodyCount--; break;
                case Category.TriggerCircleRigidBody: state.TriggerCircleRigidBodyCount--; break;
                case Category.TriggerCapsuleRigidBody: state.TriggerCapsuleRigidBodyCount--; break;

                case Category.KinematicPolygonRigidBody: state.KinematicPolygonRigidBodyCount--; break;
                case Category.KinematicCircleRigidBody: state.KinematicCircleRigidBodyCount--; break;
                case Category.KinematicCapsuleRigidBody: state.KinematicCapsuleRigidBodyCount--; break;
                
                case Category.SolidPolygonCollider: state.SolidPolygonColliderCount--; break;
                case Category.SolidCircleCollider: state.SolidCircleColliderCount--; break;
                case Category.SolidCapsuleCollider: state.SolidCapsuleColliderCount--; break;
                
                case Category.TriggerPolygonCollider: state.TriggerPolygonColliderCount--; break;
                case Category.TriggerCircleCollider: state.TriggerCircleColliderCount--; break;
                case Category.TriggerCapsuleCollider: state.TriggerCapsuleColliderCount--; break;

                case Category.KinematicPolygonCollider: state.KinematicPolygonColliderCount--; break;
                case Category.KinematicCircleCollider: state.KinematicCircleColliderCount--; break;
                case Category.KinematicCapsuleCollider: state.KinematicCapsuleColliderCount--; break;

                default:
                    throw new Exception();
            }
        }




        /******************
        
            Setters & Getters.
        
        *******************/




        public static GenIdResult SetActive(State state, GenId entityId, bool isActive)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Shape)
            {
                return GenIdResult.NotAllocated;
            }
            return SetActive(state, entityId, isActive);
        }

        public static bool IsActive(State state, GenId entityId, ref GenIdResult resultOutput)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Shape)
            {
                resultOutput = GenIdResult.NotAllocated;
                return false;
            }

            return IsActive(state, entityId, ref resultOutput);
        }

        public static GenIdResult SetLocalTransform(State state, GenId entityId, Transform newTransform)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Shape)
            {
                return GenIdResult.NotAllocated;
            }
            return SetLocalTransform(state, entityId, newTransform);
        }




        /******************
        
            Rigid.
        
        *******************/




        public static class Rigid{
            public enum ShapeType : int
            {
                Circle,
                Rectangle
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetStaticFriction(State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if (EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;
                    
                    // return a ref to the nil.
                    return ref state.PhysicsMaterials.StaticFriction[0];
                }

                if(IsRigidBodyUnsafe(state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.PhysicsMaterials.StaticFriction[0];            
                }

                resultOutput = GenIdResult.Ok;
                return ref GetStaticFrictionUnsafe(state, entityId);
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetStaticFrictionUnsafe(State state, GenId entityId)
            {
                return ref GetStaticFrictionUnsafe(state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetStaticFrictionUnsafe(State state, int body)
            {
                return ref state.PhysicsMaterials.StaticFriction[body];
            }

            /// <summary>
            ///     Gets a reference to the kinetic friction value of a body.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetKineticFriction(State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;

                    // return a ref to the nil.
                    return ref state.PhysicsMaterials.KineticFriction[0];
                }

                if(IsRigidBodyUnsafe(state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.PhysicsMaterials.KineticFriction[0];            
                }
                
                resultOutput = GenIdResult.Ok;
                return ref GetKineticFrictionUnsafe(state, entityId);
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetKineticFrictionUnsafe(State state, GenId entityId)
            {
                return ref GetKineticFrictionUnsafe(state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetKineticFrictionUnsafe(State state, int entityIndex)
            {
                return ref state.PhysicsMaterials.KineticFriction[entityIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetDensity(State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;

                    // return a ref to the nil.
                    return ref state.PhysicsMaterials.Density[0];
                }

                if(IsRigidBodyUnsafe(state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.PhysicsMaterials.Density[0];            
                }

                resultOutput = GenIdResult.Ok;
                return ref GetDensityUnsafe(state, entityId);
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetDensityUnsafe(State state, GenId entityId)
            {
                return ref GetDensityUnsafe(state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetDensityUnsafe(State state, int entityIndex)
            {
                return ref state.PhysicsMaterials.Density[entityIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetRestitution(State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;

                    // return a ref to the nil.
                    return ref state.PhysicsMaterials.Restitution[0];
                }

                if(IsRigidBodyUnsafe(state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.PhysicsMaterials.Restitution[0];            
                }

                resultOutput = GenIdResult.Ok;
                return ref GetRestitutionUnsafe(state, entityId);
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetRestitutionUnsafe(State state, GenId entityId)
            {
                return ref GetRestitutionUnsafe(state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetRestitutionUnsafe(State state, int entityIndex)
            {
                return ref state.PhysicsMaterials.Restitution[entityIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static GenIdResult SetRotationalResponse(State state, GenId entityId, bool enabled)
            {
                if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    return GenIdResult.StaleGenId;
                }

                SetRotationalResponseUnsafe(state, entityId, enabled);
                return GenIdResult.Ok;
            }

            /// <remarks>
            ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetRotationalResponseUnsafe(State state, GenId entityId, bool enabled)
            {
                SetRotationalResponseUnsafe(state, GenId.GetIndex(entityId), enabled);
            }

            /// <remarks>
            ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetRotationalResponseUnsafe(State state, int entityIndex, bool enabled)
            {
                state.RotationalResponses[entityIndex] = enabled;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool UsesRotationalResponse(State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if (EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;
                    return false;
                }

                resultOutput = GenIdResult.Ok;
                return UsesRotationalResponseUnsafe(state, entityId);
            }

            /// <remarks>
            ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool UsesRotationalResponseUnsafe(State state, GenId entityId)
            {
                return UsesRotationalResponseUnsafe(state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool UsesRotationalResponseUnsafe(State state, int body)
            {
                return state.RotationalResponses[body];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static GenIdResult SetRigidBody(State state, GenId entityId, bool enabled)
            {
                if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    return GenIdResult.StaleGenId;
                }

                SetRigidBodyUnsafe(state, GenId.GetIndex(entityId), enabled);
                return GenIdResult.Ok;
            }

            /// <remarks>
            ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetRigidBodyUnsafe(State state, int entityIndex, bool enabled)
            {
                switch (enabled)
                {
                    case true:
                        Category.SetToRigidBody(ref state.Categories[entityIndex]);
                    break;
                    case false:
                        Category.SetToCollider(ref state.Categories[entityIndex]);
                    break;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsRigidBody(State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;
                    return false;
                }
                
                resultOutput = GenIdResult.Ok;
                return IsRigidBodyUnsafe(state, entityId);
            }

            /// <remarks>
            ///     GenId checks are not enforced; the retrieved data at the given gen id slot will always be returned.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsRigidBodyUnsafe(State state, GenId entityId)
            {
                return IsRigidBodyUnsafe(state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///     GenId checks are not enforced; the retrieved data at the given gen id slot will always be returned.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsRigidBodyUnsafe(State state, int entityIndex)
            {        
                return Category.IsRigidBody(state.Categories[entityIndex]);
            }
        }



        /******************
        
            Allocation & Deallocation.
        
        *******************/


        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetCategory(State state, Rigid.ShapeType shape, ColliderBehaviour behaviour, bool rigidbodyEnabled, 
            int bodyIndex
        )
        {
            state.ShapeTypes[bodyIndex] = shape;
            ref int category = ref state.Categories[bodyIndex];

            switch (shape)
            {
                case Rigid.ShapeType.Rectangle:
                    category = behaviour switch 
                    {
                        ColliderBehaviour.Solid 
                            => rigidbodyEnabled? Category.SolidPolygonRigidBody : Category.SolidPolygonCollider,
                        ColliderBehaviour.Kinematic
                            => rigidbodyEnabled? Category.KinematicPolygonRigidBody : Category.KinematicPolygonCollider,
                        ColliderBehaviour.Trigger
                            => rigidbodyEnabled? Category.TriggerPolygonRigidBody : Category.TriggerPolygonCollider,
                        _ => throw new Exception()
                    };
                break;

                case Rigid.ShapeType.Circle:
                    category = behaviour switch 
                    {
                        ColliderBehaviour.Solid 
                            => rigidbodyEnabled? Category.SolidCircleRigidBody : Category.SolidCircleCollider,
                        ColliderBehaviour.Kinematic
                            => rigidbodyEnabled? Category.KinematicCircleRigidBody : Category.KinematicCircleCollider,
                        ColliderBehaviour.Trigger
                            => rigidbodyEnabled? Category.TriggerCircleRigidBody : Category.TriggerCircleCollider,
                        _ => throw new Exception()
                    };
                break;

                default:
                    throw new Exception();
            }

            return category;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void PrepareCollisionShapeAllocation(State state, ColliderBehaviour colliderBehaviour, int shapeIndex, int bodyIndex, 
            Rigid.ShapeType shape, bool IsRigid
        )
        {
            state.EntityTypes[shapeIndex] = EntityType.Shape;

            // clear any garbage data from previous allocations.
            FsSoa_Vector2.ClearEntryAppendCount(state.BaseVertices, shapeIndex);

            // set this so that the previous position isnt garbage from previous steps.
            state.PreviousStepPositions.X[bodyIndex] = state.GlobalTransforms.Positions.X[bodyIndex];
            state.PreviousStepPositions.Y[bodyIndex] = state.GlobalTransforms.Positions.Y[bodyIndex];

            // set the new data.
            SetActiveUnsafe(state, shapeIndex, true);
            int category = SetCategory(state, shape, colliderBehaviour, IsRigid, shapeIndex);            
            IncrementCategoryCounter(state, category);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void FinaliseCollisionShapeAllocation(State state, Span<float> shapeBaseVertsX,
            Span<float> shapeBaseVertsY, Transform transform, int shapeIndex, int bodyIndex, bool IsRigid)
        {        
            // note:
            // order matters here (from top to bottom):
            // - set transform data
            // - set vertce data
            // - transform vertice data (getting centroid as well).
            // - add shape to tree.
            // - integrate the now intialised shape into the body (if it is a rigid shape.)

            Soa_Transform globalTransforms = state.GlobalTransforms;
            
            Transform globalTransform = Transform.TransformRelative(transform, globalTransforms.Positions.X[bodyIndex], 
                globalTransforms.Positions.Y[bodyIndex], globalTransforms.Scales.X[bodyIndex], globalTransforms.Scales.Y[bodyIndex], 
                globalTransforms.Sines[bodyIndex], globalTransforms.Cosines[bodyIndex], globalTransforms.RotationRadians[bodyIndex]
            );            

            SetLocalTransformUnsafe(state, shapeIndex, transform);
            SetGlobalTransformUnsafe(state, shapeIndex, globalTransform);

            for(int i = 0; i < shapeBaseVertsX.Length; i++)
            {
                FsSoa_Vector2.Append(state.BaseVertices, shapeIndex, shapeBaseVertsX[i], shapeBaseVertsY[i]);
            }

            TransformShapeVertices(state.WorldVertices, state.BaseVertices, globalTransforms.Positions.X, globalTransforms.Positions.Y, 
                globalTransforms.Scales.X, globalTransforms.Scales.Y, globalTransforms.Cosines, globalTransforms.Sines, 
                state.Aabbs.MinX, state.Aabbs.MinY, state.Aabbs.MaxX, state.Aabbs.MaxY, state.BaseRadii, state.WorldRadii, 
                state.Centroids.X, state.Centroids.Y, state.ShapeTypes, shapeIndex
            );

            IntrusiveList.AddToTree(state.BodyHierarchy, shapeIndex, bodyIndex);

            if (IsRigid)
            {
                Body.IntegrateShapePropertiesUnsafe(state, bodyIndex);
            }
        }




        /******************
        
            Circle
        
        *******************/




        public static class Circle
        {
            public static class Collider
            {                
                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static GenIdResult Allocate(State state, Math.Shapes.Circle shape, Transform transform, 
                    ColliderBehaviour colliderBehaviour, GenId bodyId, ref GenId colliderId
                )
                {
                    GenIdResult result = EntityRegistry.Allocate(state.Entities, ref colliderId); 
                    if(result != GenIdResult.Ok)
                    {
                        return result;
                    }
                    
                    if (EntityRegistry.IsGenIdStale(state.Entities, bodyId))
                    {
                        Debug.LogError("cannot allocate collision shape into a stale body", stackDepth: 2);
                        return GenIdResult.StaleGenId;
                    }

                    int shapeIndex = GenId.GetIndex(colliderId);
                    int bodyIndex = GenId.GetIndex(bodyId);

                    PrepareCollisionShapeAllocation(state, colliderBehaviour, shapeIndex, bodyIndex, Shape.Rigid.ShapeType.Circle, false);
                    
                    { // Set specific data.

                        // rigidbodies should respond to this like a kinematic rigidbody if it is solid or kinematic. 
                        state.Masses[shapeIndex] = 0 ;
                        state.InverseMasses[shapeIndex] = 0;
                        state.BaseRadii[shapeIndex] = shape.Radius;
                    }

                    FinaliseCollisionShapeAllocation(state, [shape.X], [shape.Y], transform, shapeIndex, bodyIndex, 
                        false
                    );
                
                    return GenIdResult.Ok;
                }
            }
            
            public static class Rigid
            {                
                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static GenIdResult Allocate(State state, Math.Shapes.Circle shape, Transform transform, 
                    PhysicsMaterial material, ColliderBehaviour colliderBehaviour, bool rotationalResponse, GenId bodyId, ref GenId genId
                )
                {

                    GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId);
                    if(result != GenIdResult.Ok)
                    {
                        return result;
                    }

                    if (EntityRegistry.IsGenIdStale(state.Entities, bodyId))
                    {
                        Debug.LogError("cannot allocate collision shape into a stale body", stackDepth: 2);
                        return GenIdResult.StaleGenId;
                    }

                    int shapeIndex = GenId.GetIndex(genId);
                    int bodyIndex = GenId.GetIndex(bodyId);

                    PrepareCollisionShapeAllocation(state, colliderBehaviour, shapeIndex, bodyIndex, Shape.Rigid.ShapeType.Circle, true);

                    {   // Set specific data. 
                        
                        Shape.Rigid.SetRotationalResponseUnsafe(state, shapeIndex, rotationalResponse);
                        Soa_PhysicsMaterial.Insert(state.PhysicsMaterials, material.StaticFriction, material.KineticFriction, 
                            material.Density, material.Restitution, shapeIndex
                        );
                        state.BaseRadii[shapeIndex] = shape.Radius;

                        IntegrateProperties(state, transform.Scale.X, transform.Scale.Y, shape.Radius, shapeIndex);
                    }

                    FinaliseCollisionShapeAllocation(state, [shape.X], [shape.Y], transform, shapeIndex, bodyIndex, 
                        true
                    );

                    return GenIdResult.Ok;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static float CalculateRotationalInertia(float radius, float mass)
                {
                    return PhysicsSystem.CircleRotationalInertia * mass * (radius * radius);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static System.Numerics.Vector<float> CalculateRotationalInertia(System.Numerics.Vector<float> radius, 
                    System.Numerics.Vector<float> mass
                )
                {
                    return PhysicsSystem.VectorCircleRotationalInertia * mass * (radius * radius);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static float CalculateMass(float radius, float density)
                {
                    return density * Math.Shapes.Circle.GetArea(radius);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static System.Numerics.Vector<float> CalculateMass(System.Numerics.Vector<float> radius, 
                    System.Numerics.Vector<float> density
                )
                {
                    return density * Math.Shapes.Circle.GetArea(radius);
                }

                public static void IntegrateProperties(State state, float scaleX, float scaleY, float baseRadii,
                    int shapeIndex
                )
                {
                    float radius = Math.Shapes.Circle.ScaleRadius(baseRadii, scaleX, scaleY);
                    state.WorldRadii[shapeIndex] = radius;

                    float mass = CalculateMass(radius, state.PhysicsMaterials.Density[shapeIndex]); 
                    state.Masses[shapeIndex] = mass;
                    state.InverseMasses[shapeIndex] = mass == 0? 0 : 1f/mass;

                    float inertia = CalculateRotationalInertia(radius, mass);
                    state.RotationalInertia[shapeIndex] = inertia;
                    state.InverseRotationalInertia[shapeIndex] = inertia == 0? 0f : 1f/inertia;                        
                }
            }
        }




        /******************
        
            Rectangle.
        
        *******************/




        public static class Rectangle
        {
            public static class Collider
            {                
                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static GenIdResult Allocate(State state, Math.Shapes.Rectangle shape, Transform transform, 
                    ColliderBehaviour colliderBehaviour, GenId bodyId, ref GenId genId
                )
                {

                    GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId);
                    if(result != GenIdResult.Ok)
                    {
                        return result;
                    }

                    PolygonRectangle polyRect = new(shape);

                    if (EntityRegistry.IsGenIdStale(state.Entities, bodyId))
                    {
                        Debug.LogError("cannot allocate collision shape into a stale body", stackDepth: 2);
                        return GenIdResult.StaleGenId;
                    }

                    int shapeIndex = GenId.GetIndex(genId);
                    int bodyIndex = GenId.GetIndex(bodyId);

                    PrepareCollisionShapeAllocation(state, colliderBehaviour, shapeIndex, bodyIndex, Shape.Rigid.ShapeType.Rectangle, false);

                    {   // set specific data.
                        
                        // apply data.                        
                        state.BaseHeights[shapeIndex] = shape.Height;
                        state.BaseWidths[shapeIndex] = shape.Width;

                        // rigidbodies should respond to this like a kinematic rigidbody if it is solid or kinematic. 
                        state.Masses[shapeIndex] = 0;
                        state.InverseMasses[shapeIndex] = 0;
                    }

                    FinaliseCollisionShapeAllocation(state, PolygonRectangle.VerticesXAsSpan(polyRect), PolygonRectangle.VerticesYAsSpan(polyRect), 
                        transform, shapeIndex, bodyIndex, false
                    );

                    return GenIdResult.Ok;
                }
            }

            public static class Rigid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static GenIdResult Allocate(State state, Math.Shapes.Rectangle shape, Transform transform,
                    PhysicsMaterial material, ColliderBehaviour colliderBehaviour, bool rotationalResponse, GenId bodyId, ref GenId genId
                )
                {
                    GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId);
                    if(result != GenIdResult.Ok)
                    {
                        return result;
                    }

                    PolygonRectangle polyRect = new(shape);
                    
                    if (EntityRegistry.IsGenIdStale(state.Entities, bodyId))
                    {
                        Debug.LogError("cannot allocate collision shape into a stale body", stackDepth: 2);
                        return GenIdResult.StaleGenId;
                    }

                    int shapeIndex = GenId.GetIndex(genId);
                    int bodyIndex = GenId.GetIndex(bodyId);

                    PrepareCollisionShapeAllocation(state, colliderBehaviour, shapeIndex, bodyIndex, Shape.Rigid.ShapeType.Rectangle, true);

                    {   // specific data.
                        
                        Shape.Rigid.SetRotationalResponseUnsafe(state, shapeIndex, rotationalResponse);
                        state.BaseHeights[shapeIndex] = shape.Height;
                        state.BaseWidths[shapeIndex] = shape.Width;
                        Soa_PhysicsMaterial.Insert(state.PhysicsMaterials, material.StaticFriction, material.KineticFriction, 
                            material.Density, material.Restitution, shapeIndex
                        );
                        IntegrateProperties(state, transform.Scale.X, transform.Scale.Y, shape.Height, shape.Width, shapeIndex);
                    }

                    FinaliseCollisionShapeAllocation(state, PolygonRectangle.VerticesXAsSpan(polyRect), PolygonRectangle.VerticesYAsSpan(polyRect), 
                        transform, shapeIndex, bodyIndex, true
                    );

                    return GenIdResult.Ok;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static float CalculateMass(float width, float height, float density)
                {
                    return Math.Shapes.Rectangle.GetArea(width, height) * density;
                } 

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static System.Numerics.Vector<float> CalculateMass(System.Numerics.Vector<float> width, 
                    System.Numerics.Vector<float> height, System.Numerics.Vector<float> density
                )
                {
                    return Math.Shapes.Rectangle.GetArea(width, height) * density;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static float CalculateRotationalInertia(float width, float height, float mass)
                {
                    return PhysicsSystem.RectangleRotationalInertia * mass * ((width * width) + (height * height));
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static System.Numerics.Vector<float> CalculateRotationalInertia(System.Numerics.Vector<float> width, 
                    System.Numerics.Vector<float> height, System.Numerics.Vector<float> mass
                )
                {
                    return PhysicsSystem.VectorRectangleRotationalInertia * mass * ((width * width) + (height * height));
                }

                public static void IntegrateProperties(State state, float scaleX, float scaleY, float baseHeight, float baseWidth,
                    int shapeIndex
                )
                {
                    IntrusiveList.Node[] nodes = state.BodyHierarchy.Nodes;
                    ref IntrusiveList.Node shapeNode = ref nodes[shapeIndex];

                    float height = baseHeight * scaleY;
                    float width = baseWidth * scaleX;

                    float mass = CalculateMass(width, height, state.PhysicsMaterials.Density[shapeIndex]); 
                    state.Masses[shapeIndex] = mass;
                    state.InverseMasses[shapeIndex] = mass == 0? 0 : 1f/mass;

                    float inertia = CalculateRotationalInertia(width, height, mass);
                    state.RotationalInertia[shapeIndex] = inertia;
                    state.InverseRotationalInertia[shapeIndex] = inertia == 0? 0 : 1f/inertia;
                }
            }

        }

        public static void GetVerticesUnsafe(FsSoa_Vector2 vertices, int bodyIndex, ref Span<float> xOutput, ref Span<float> yOutput)
        {
            int startIndex = FixedStrideArray.GetElementIndex(bodyIndex, vertices.Stride, 0);
            int appendCount = vertices.AppendCounts[bodyIndex];
            xOutput = vertices.X.AsSpan().Slice(startIndex, appendCount);
            yOutput = vertices.Y.AsSpan().Slice(startIndex, appendCount);
        }
    }




    /******************
    
        Debug Drawing.
    
    *******************/




    public static void Draw(HowlAppState howl, State state, float deltaTime)
    {
        DrawGlobalPositions(howl, state.Active, state.GlobalTransforms.Positions.X, state.GlobalTransforms.Positions.Y);
        DrawShapes(howl, state.CollisionManifoldState, state.WorldVertices, state.Centroids.X, state.Centroids.Y, state.WorldRadii, 
            state.Categories, state.EntityTypes, state.Active
        );

        // DrawAabbs(howl, state.Active, state.Aabbs.MinX, state.Aabbs.MinY, state.Aabbs.MaxX, state.Aabbs.MaxY, AABBColour);
        // BoundingVolumeHierarchy.DrawBranches(howl, state.Bvh, Colour.Yellow);
        // BoundingVolumeHierarchy.DrawLeaves(howl, state.Bvh, Colour.Yellow);
    }

    public static void DrawGlobalPositions(HowlAppState howl, bool[] active, float[] globalPositionsX, float[] globalPositionsY)
    {
        for(int i = 1; i < active.Length; i++)
        {
            if (active[i])
            {
                Debug.DrawWireCircle(howl, new Circle(globalPositionsX[i], globalPositionsY[i], 0.1f), Colour.White, DrawSpace.World);
            }
        }
    }

    public static void DrawShapes(HowlAppState howl, CollisionManifoldState collisions, FsSoa_Vector2 vertices, 
        float[] centroidsX, float[] centroidsY, float[] radii, int[] categories, EntityType[] entityTypes, bool[] active
    )
    {
        Colour colour = default;

        Span<float> polyVertsX = stackalloc float[vertices.Stride];
        Span<float> polyVertsY = stackalloc float[vertices.Stride];

        for(int i = 1; i < active.Length; i++)
        {
            if (active[i] && entityTypes[i] == EntityType.Shape)
            {
                ref int category = ref categories[i];

                if (Shape.Category.IsSolid(category))
                {
                    colour = DynamicShapeColour;
                }
                else if (Shape.Category.IsKinematic(category))
                {
                    colour = KinematicShapeColour;
                }
                else if(Shape.Category.IsTrigger(category))
                {
                    colour = CollisionManifold.HasContacts(collisions, i)
                    ? ActiveTriggerShapeColour
                    : PassiveTriggerShapeColour;            

                }
                else
                {
                    colour = FallbackShapeColour;
                }

                if (Shape.Category.IsPolygon(category))
                {
                    Shape.GetVerticesUnsafe(vertices, i, ref polyVertsX, ref polyVertsY);
                    Debug.DrawWirePoly(howl, polyVertsX, polyVertsY, colour, DrawSpace.World);
                }
                else if (Shape.Category.IsCircle(category))
                {
                    Circle shape = new(centroidsX[i], centroidsY[i], radii[i]);
                    Debug.DrawWireCircle(howl, shape, colour, DrawSpace.World);
                }
            }
        }
    }

    public static void DrawAabbs(HowlAppState app, bool[] active, float[] aabbsMinX, float[] aabbsMinY, float[] aabbsMaxX, float[] aabbsMaxY, 
        Colour colour
    )
    {
        for(int i = 1; i < active.Length; i++) // start at one to skip Nil.
        {
            if (active[i])
            {                
                float minX = aabbsMinX[i];
                float minY = aabbsMinY[i];
                float maxX = aabbsMaxX[i];
                float maxY = aabbsMaxY[i];
                Debug.DrawWirePoly(app, [minX, maxX, maxX, minX], [maxY, maxY, minY, minY], colour, DrawSpace.World);
            }
        }
    }
}