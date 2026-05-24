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
    public static Colour AabbColour = Colour.Pink;
    public static Colour FallbackShapeColour = Colour.White;
    public static Colour InactivePhysicsBodyColour = Colour.Black;
    public static Colour BvhLeafAABBColour = Colour.Green;
    public static Colour BvhBranchAABBColour = Colour.White;
    public static Colour ContactPointColour = Colour.Red;
    public static Colour LinearVelocityColour = Colour.White;
    public static Colour PositionColour = Colour.White;
    public static Colour CentroidColour = Colour.Yellow;
    public static Colour CollisionOtherColour = Colour.Blue;
    public static Colour CollisionNormalColour = Colour.Purple;
    public static Colour CenterOfMassColour = Colour.Pink;




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
    
        Material.
    
    *******************/



    public struct Material
    {
        public const float MinFriction = 0;
        public const float MaxFriction = 1;

        public const float MinDensity = float.Epsilon;
        public const float MaxDensity = 22.6f; // osmium density.

        public const float MinRestitution = 0f;
        public const float MaxRestitution = 1f;

        // Friction has two values: kinetic and static.
        // this is because - in the real world - objects require much more
        // initial force to start moving compared to when they would already be
        // in motion. This is simulated as two friction values.

        public float StaticFriction;
        public float KineticFriction;
        public float Density;
        public float Restitution;

        /// <summary>
        ///     Constructs a Physics Material.
        /// </summary>
        public Material(){}

        public Material(float staticFriction, float kineticFriction, float density, float restitution)
        {
            SetKineticFriction(ref KineticFriction, kineticFriction);
            SetStaticFriction(ref StaticFriction, ref KineticFriction, staticFriction);
            SetDensity(ref Density, density);
            Restitution = restitution;
        }

        /// <summary>
        ///     Asserts that a kinetic friction value is betwen <c><see cref="MinFriction"/></c> and <c><see cref="MaxFriction"/></c>
        /// </summary>
        /// <remarks>
        ///     Calls to this function are compiled out entirely when not in <c>DEBUG</c> builds.
        /// </remarks>
        /// <param name="value">the kinetic friction value.</param>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void AssertKineticFrictionInRange(float value)
        {
            System.Diagnostics.Debug.Assert(value >= MinFriction && value <= MaxDensity, 
                $"Kinetic Friction '{value}' is not within the range of '{MinFriction}' and '{MaxFriction}'"
            );
        }

        /// <remarks>
        ///     Note: this function clamps kinetic friction within physics material's min and max friction values.
        /// </remarks>
        /// <param name="kineticFriction">the kinetic friction value to mutate.</param>
        /// <param name="value">the new kinetic friction value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetKineticFriction(ref float kineticFriction, float value)
        {
            AssertKineticFrictionInRange(value);
            kineticFriction = Math.Math.Clamp(value, MinFriction, MaxFriction);
        }

        /// <remarks>
        ///     Note: this function clamps kinetic friction within physics material's min and max friction values.
        /// </remarks>
        /// <param name="material">the physics material to mutate.</param>
        /// <param name="value">the new kinetic friction value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetKineticFriction(ref Material material, float value)
        {
            SetKineticFriction(ref material.KineticFriction, value);
        }

        /// <summary>
        ///     Asserts that a static friction value is between <c><paramref name="kineticFriction"/></c> and <c><see cref="MaxDensity"/></c>.
        /// </summary>
        /// <remarks>
        ///     Calls to this function are compiled out entirely when not in <c>DEBUG</c> builds.
        /// </remarks>
        /// <param name="value">the static friction value.</param>
        /// <param name="kineticFriction">the kinetic friction value.</param>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void AssertStaticFrictionInRange(float value, float kineticFriction)
        {
            System.Diagnostics.Debug.Assert(value >=  kineticFriction && value <= MaxFriction, 
                $"Static Friction '{value}' is not within the range of '{kineticFriction}' and '{MaxFriction}'"
            );
        }

        /// <remarks>
        ///     Note: this function clamps static friction within the kinetic friction value and physic material's max friction value.
        /// </remarks>
        /// <param name="staticFriction">the static friction value to mutate.</param>
        /// <param name="kineticFriction">the kinetic friction value used as the minimum static friction value.</param>
        /// <param name="value">the new static friction value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetStaticFriction(ref float staticFriction, ref float kineticFriction, float value)
        {
            AssertStaticFrictionInRange(value, kineticFriction);
            staticFriction = Math.Math.Clamp(value, kineticFriction, MaxFriction);
        }

        /// <remarks>
        ///     Note: this function clamps static friction within the kinetic friction value and physic material's max friction value.
        /// </remarks>
        /// <param name="material">the physics material to mutate.</param>
        /// <param name="value">the new static friction value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetStaticFriction(ref Material material, float value)
        {
            SetStaticFriction(ref material.StaticFriction, ref material.KineticFriction, value);
        }

        /// <summary>
        ///     Asserts that a density value is between <c><see cref="MinDensity"/></c> and <c><see cref="MaxDensity"/></c>.
        /// </summary>
        /// <remarks>
        ///     Calls to this function are compiled out entirely when not in <c>DEBUG</c> builds.
        /// </remarks>
        /// <param name="value">the density value.</param>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void AssertDensityInRange(float value)
        {
            System.Diagnostics.Debug.Assert(value >= MinDensity && value <= MaxDensity, 
                $"Density '{value}' is not within the range of '{MinDensity}' and '{MaxDensity}'"
            );
        }

        /// <remarks>
        ///     Note: this function clamps density within physics material's min and max density values.
        /// </remarks>
        /// <param name="density">the density value to mutate.</param>
        /// <param name="value">the new density value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetDensity(ref float density, float value)
        {
            AssertDensityInRange(value);
            density = Math.Math.Clamp(value, MinDensity, MaxDensity);
        }

        /// <param name="material">the physics material to mutate.</param>
        /// <param name="value">the new density value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetDensity(ref Material material, float value)
        {
            SetDensity(ref material.Density, value);
        }

        /// <remarks>
        ///     Calls to this function are compiled out entirely when not in <c>DEBUG</c> builds.
        /// </remarks>
        /// <param name="value">the restitution value.</param>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void AssertRestitutionInRange(float value)
        {        
            System.Diagnostics.Debug.Assert(value >= MinRestitution && value <= MaxRestitution, 
                $"Restitution '{value}' is not within the range of '{MinRestitution}' and '{MaxRestitution}'"
            );
        }

        /// <remarks>
        ///     Note: this function clamps restitution within physics material's min and max restitution values.
        /// </remarks>
        /// <param name="restitution">the restitution value to mutate.</param>
        /// <param name="value">the new restitution value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetRestitution(ref float restitution, float value)
        {
            AssertRestitutionInRange(value);
            restitution = Math.Math.Clamp(value, MinRestitution, MaxRestitution);
        }

        /// <remarks>
        ///     Note: this function clamps restitution within physics material's min and max restitution values.
        /// </remarks>
        /// <param name="material">the physics material to mutate.</param>
        /// <param name="value">the new restitution value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetRestitution(ref Material material, float value)
        {
            SetRestitution(ref material.Restitution, value);
        }
    }


    

    /******************
    
        Soa_Material
    
    *******************/




    public class Soa_Material
    {
        public float[] StaticFriction;
        public float[] KineticFriction;
        public float[] Density;
        public float[] Restitution;
        public int Length;
        public bool Disposed;

        /// <summary>
        ///     Creates a new Structure-Of-Arrays Physics Material instance.
        /// </summary>
        /// <param name="length">the length of the backing arrays.</param>
        public Soa_Material(int length)
        {
            StaticFriction = new float[length];
            KineticFriction = new float[length];
            Density = new float[length];
            Restitution = new float[length];
            Length = length;
        }

        /// <summary>
        ///     Inserts a physics material's values into a soa instance.
        /// </summary>
        /// <remarks>
        ///     <para>Remarks:</para>
        ///     <para>All arrays will be mutated with the newly set values.</para>
        ///     <para>All arrays must be the same length; as entries are associated via <paramref name="insertIndex"/>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Insert(float[] staticFrictions, float[] kineticFrictions, float[] densities, float[] restitutions, 
            float staticFriction, float kineticFriction, float density, float restitution,
            int insertIndex
        )
        {
            Material.AssertKineticFrictionInRange(kineticFriction);
            Material.AssertStaticFrictionInRange(staticFriction, kineticFriction);
            Material.AssertRestitutionInRange(restitution);
            Material.AssertDensityInRange(density);

            staticFrictions[insertIndex] = staticFriction;
            kineticFrictions[insertIndex] = kineticFriction;
            densities[insertIndex] = density;
            restitutions[insertIndex] = restitution;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Insert(Soa_Material soa, int insertIndex, float staticFriction, float kineticFriction, float density, 
            float restitution
        )
        {
            Material.AssertKineticFrictionInRange(kineticFriction);
            Material.AssertStaticFrictionInRange(staticFriction, kineticFriction);
            Material.AssertRestitutionInRange(restitution);
            Material.AssertDensityInRange(density);

            soa.StaticFriction[insertIndex] = staticFriction;
            soa.KineticFriction[insertIndex] = kineticFriction;
            soa.Density[insertIndex] = density;
            soa.Restitution[insertIndex] = restitution;
        }

        /// <summary>
        ///     Inserts a physics material's values into a soa instance.
        /// </summary>
        /// <param name="soa">the soa instance to insert into.</param>
        /// <param name="material">the material value to set to.</param>
        /// <param name="insertIndex">the index of the entry to modify.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Insert(Soa_Material soa, Material material, int insertIndex)
        {
            Insert(soa.StaticFriction, soa.KineticFriction, soa.Density, soa.Restitution,
                material.StaticFriction, material.KineticFriction, material.Density, material.Restitution, insertIndex
            );  
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Insert(Soa_Material soa, float staticFriction, float kineticFriction, float density, float restitution, int index
        )
        {
            Insert(soa.StaticFriction, soa.KineticFriction, soa.Density, soa.Restitution,
                staticFriction, kineticFriction, density, restitution, index
            );  
        }

        /// <summary>
        ///     Enforces a <c>Nil</c> entry for all underlying arrays in the soa instance.
        /// </summary>
        /// <param name="soa"></param>
        public static void EnforceNil(Soa_Material soa)
        {
            Nil.Enforce(soa.StaticFriction);
            Nil.Enforce(soa.KineticFriction);
            Nil.Enforce(soa.Density);
            Nil.Enforce(soa.Restitution);
        }




        /*******************
        
            Disposal.
        
        ********************/




        public static void Dispose(Soa_Material soa)
        {
            if(soa.Disposed)
                return;
            
            soa.Disposed = true;
            soa.StaticFriction = null;
            soa.KineticFriction = null;
            soa.Density = null;
            soa.Restitution = null;
            soa.Length = 0;

            GC.SuppressFinalize(soa);
        }

        ~Soa_Material()
        {
            Dispose(this);       
        }
    }




    /******************
    
        State
    
    *******************/




    public class State
    {




        /*******************
        
            Debug Diagnostic Stopwatches.
        
        ********************/




        public Stopwatch FixedUpdateStepStopwatch;
        public Stopwatch FixedUpdateSubStepStopwatch;
        public Stopwatch IntegrateBodyPropertiesStopwatch;
        public Stopwatch RigidBodyMovementStepStopwatch;
        public Stopwatch TransformPhysicsBodiesStopwatch;
        public Stopwatch BvhStopwatch;
        public Stopwatch FilterBvhIntoCollisionManifoldStopwatch;
        public Stopwatch FindCollisionsStopwatch;
        public Stopwatch ColliderCollisionResolutionStopwatch;
        public Stopwatch RigidBodyCollisionResolutionStepStopwatch;
        public Stopwatch CollisionManifoldSortStopwatch;
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
        ///     The global-space vertices for all shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>vertexIndex</c>.</para>
        /// </remarks>    
        public FsSoa_Vector2 GlobalVertices;

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
        public Soa_Material Materials;

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
        ///     The global-space radii values of all circle shapes.
        /// </summary>
        /// <remarks>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public float[] GlobalRadii;

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
        ///     The indices in the <c>CollisionManifold.State</c> of collider collisions to resolve in the current substep.
        /// </summary>
        public CategorisedOverlapArray<int> SubStepShapeCollisionsToResolve;

        /// <summary>
        ///     The indices in the <c>CollisionManifold.State</c> of rigidbody collisions to resolve in the current substep.
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
        public Collisions.Manifold.State CollisionManifoldState;

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




        public int DynamicColliderPolygonCount;
        public int TriggerColliderPolygonCount;
        public int KinematicColliderPolygonCount;
        public int DynamicRigidPolygonCount;
        public int TriggerRigidPolygonCount;
        public int KinematicRigidPolygonCount;

        public int DynamicColliderCircleCount;
        public int TriggerColliderCircleCount;
        public int KinematicColliderCircleCount;
        public int DynamicRigidCircleCount;
        public int TriggerRigidCircleCount;
        public int KinematicRigidCircleCount;
        
        public int DynamicRigidCapsuleCount;
        public int KinematicRigidCapsuleCount;
        public int TriggerRigidCapsuleCount;
        public int DynamicColliderCapsuleCount;
        public int KinematicColliderCapsuleCount;
        public int TriggerColliderCapsuleCount;




        /******************
        
            Draw Flags 
        
        *******************/




        public bool DrawBodyGlobalPositions;
        public bool DrawShapes;
        public bool DrawAabbs;
        public bool DrawBvhBranches;
        public bool DrawCollisionInformation;
        public bool DrawLinearVelocities;
        public bool DrawCentroids;
        public bool DrawLeaves;




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
                GlobalVertices = new FsSoa_Vector2(verticesPerShape, maxEntities);
                LocalTransforms = new(maxEntities);
                GlobalTransforms = new(maxEntities);
                PreviousStepPositions = new(maxEntities);
                Forces = new(maxEntities);
                LinearVelocities = new(maxEntities);
                Centroids = new(maxEntities);
                Aabbs = new(maxEntities);
                Materials = new(maxEntities);
                AngularVelocities = new float[maxEntities];
                Masses = new float[maxEntities];
                InverseMasses = new float[maxEntities];
                BaseWidths = new float[maxEntities];
                BaseHeights = new float[maxEntities];
                BaseRadii = new float[maxEntities];
                GlobalRadii = new float[maxEntities];
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
        Collisions.Manifold.State collisions = state.CollisionManifoldState;
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
        FsSoa_Vector2 globalVertices = state.GlobalVertices;
        CategorisedOverlapArray<int> shapeCollisionsToResolve = state.SubStepShapeCollisionsToResolve;
        CategorisedOverlapArray<int> rigidShapeCollisionsToResolve = state.SubStepRigidShapeCollisionsToResolve;
        float[] globalRadii = state.GlobalRadii;
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
        float[] densities = state.Materials.Density;
        float[] staticFrictions = state.Materials.StaticFriction;
        float[] kineticFrictions = state.Materials.KineticFriction;
        float[] restitutions = state.Materials.Restitution;
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
            
            Collisions.Manifold.PrepareForNextStep(collisions);

            int solidCount = state.DynamicColliderPolygonCount + state.DynamicColliderCircleCount + state.DynamicRigidPolygonCount + state.DynamicRigidCircleCount;
            int kinematicCount = state.KinematicColliderPolygonCount + state.KinematicColliderCircleCount + state.KinematicRigidPolygonCount + state.KinematicRigidCircleCount;

            // prepare sub step collision resolution collection.
            shapeCollisionsToResolve.CategoryLengths[CollisionResolutionCategory.Dynamic] = solidCount;
            shapeCollisionsToResolve.CategoryLengths[CollisionResolutionCategory.Kinematic] = kinematicCount;
            CategorisedOverlapArray.BuildChunks(shapeCollisionsToResolve);

            rigidShapeCollisionsToResolve.CategoryLengths[CollisionResolutionCategory.Dynamic] = solidCount;
            rigidShapeCollisionsToResolve.CategoryLengths[CollisionResolutionCategory.Kinematic] = kinematicCount;
            CategorisedOverlapArray.BuildChunks(rigidShapeCollisionsToResolve);
        }

        {   // Bvh
            
            state.BvhStopwatch.Restart();
            
            CalculateBvhLeafPadding(globalPositionsX, globalPositionsY, previousPositionsX, previousPositionsY, activeBodies, bvhLeafPaddings, deltaTime);

            // Update Overlap Scratch Buffer Category Length.       
            {                
            
                CategorisedLeafOverlaps.ClearCounts(overlaps);
                overlaps.CategoryLengths[Shape.Category.DynColCircle]       = state.DynamicColliderCircleCount;
                overlaps.CategoryLengths[Shape.Category.TriColCircle]     = state.TriggerColliderCircleCount;
                overlaps.CategoryLengths[Shape.Category.KinColCircle]   = state.KinematicColliderCircleCount;
                
                overlaps.CategoryLengths[Shape.Category.DynRigCircle]      = state.DynamicRigidCircleCount;
                overlaps.CategoryLengths[Shape.Category.TriRigCircle]    = state.TriggerRigidCircleCount;
                overlaps.CategoryLengths[Shape.Category.KinRigCircle]  = state.KinematicRigidCircleCount;

                overlaps.CategoryLengths[Shape.Category.DynColPolygon]      = state.DynamicColliderPolygonCount;
                overlaps.CategoryLengths[Shape.Category.TriColPolygon]    = state.TriggerColliderPolygonCount;
                overlaps.CategoryLengths[Shape.Category.KinColPolygon]  = state.KinematicColliderPolygonCount;
                
                overlaps.CategoryLengths[Shape.Category.DynRigPolygon]     = state.DynamicRigidPolygonCount;
                overlaps.CategoryLengths[Shape.Category.TriRigPolygon]   = state.TriggerRigidPolygonCount;
                overlaps.CategoryLengths[Shape.Category.KinRigPolygon] = state.KinematicRigidPolygonCount;
                
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
        OverlapInfo overlaps_DynRigPol_To_DynRigPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.DynRigPolygon);
        OverlapInfo overlaps_DynRigPol_To_DynRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.DynRigCircle);
        OverlapInfo overlaps_DynRigPol_To_KinRigPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.KinRigPolygon);
        OverlapInfo overlaps_DynRigPol_To_KinRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.KinRigCircle);
        OverlapInfo overlaps_DynRigPol_To_TriRigPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.TriRigPolygon);
        OverlapInfo overlaps_DynRigPol_To_TriRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.TriRigCircle);
        OverlapInfo overlaps_DynRigPol_To_DynColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.DynColPolygon);
        OverlapInfo overlaps_DynRigPol_To_DynColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.DynColCircle);
        OverlapInfo overlaps_DynRigPol_To_KinColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.KinColPolygon);
        OverlapInfo overlaps_DynRigPol_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.KinColCircle);
        OverlapInfo overlaps_DynRigPol_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_DynRigPol_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigPolygon, Shape.Category.TriColCircle);
        
        // solid circle rigid body.
        OverlapInfo overlaps_DynRigCir_To_DynRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.DynRigCircle);
        OverlapInfo overlaps_DynRigCir_To_KinRigPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.KinRigPolygon);
        OverlapInfo overlaps_DynRigCir_To_KinRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.KinRigCircle);
        OverlapInfo overlaps_DynRigCir_To_TriRigPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.TriRigPolygon);
        OverlapInfo overlaps_DynRigCir_To_TriRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.TriRigCircle);
        OverlapInfo overlaps_DynRigCir_To_DynColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.DynColPolygon);
        OverlapInfo overlaps_DynRigCir_To_DynColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.DynColCircle);
        OverlapInfo overlaps_DynRigCir_To_KinColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.KinColPolygon);
        OverlapInfo overlaps_DynRigCir_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.KinColCircle);
        OverlapInfo overlaps_DynRigCir_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_DynRigCir_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynRigCircle, Shape.Category.TriColCircle);

        // kinematic polygon rigid body.
        OverlapInfo overlaps_KinRigPol_To_KinRigPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.KinRigPolygon);
        OverlapInfo overlaps_KinRigPol_To_KinRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.KinRigCircle);
        OverlapInfo overlaps_KinRigPol_To_TriRigPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.TriRigPolygon);
        OverlapInfo overlaps_KinRigPol_To_TriRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.TriRigCircle);
        OverlapInfo overlaps_KinRigPol_To_DynColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.DynColPolygon);
        OverlapInfo overlaps_KinRigPol_To_DynColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.DynColCircle);
        OverlapInfo overlaps_KinRigPol_To_KinColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.KinColPolygon);
        OverlapInfo overlaps_KinRigPol_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.KinColCircle);
        OverlapInfo overlaps_KinRigPol_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_KinRigPol_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigPolygon, Shape.Category.TriColCircle);
        
        // kinematic circle rigid body.
        OverlapInfo overlaps_KinRigCir_To_KinRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigCircle, Shape.Category.KinRigCircle);
        OverlapInfo overlaps_KinRigCir_To_TriRigPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigCircle, Shape.Category.TriRigPolygon);
        OverlapInfo overlaps_KinRigCir_To_TriRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigCircle, Shape.Category.TriRigCircle);
        OverlapInfo overlaps_KinRigCir_To_DynColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigCircle, Shape.Category.DynColPolygon);
        OverlapInfo overlaps_KinRigCir_To_DynColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigCircle, Shape.Category.DynColCircle);
        OverlapInfo overlaps_KinRigCir_To_KinColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigCircle, Shape.Category.KinColPolygon);
        OverlapInfo overlaps_KinRigCir_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigCircle, Shape.Category.KinColCircle);
        OverlapInfo overlaps_KinRigCir_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigCircle, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_KinRigCir_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinRigCircle, Shape.Category.TriColCircle);
        
        // trigger polygon rigid body.
        OverlapInfo overlaps_TriRigPol_To_TriRigPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigPolygon, Shape.Category.TriRigPolygon);    
        OverlapInfo overlaps_TriRigPol_To_TriRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigPolygon, Shape.Category.TriRigCircle);
        OverlapInfo overlaps_TriRigPol_To_DynColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigPolygon, Shape.Category.DynColPolygon);
        OverlapInfo overlaps_TriRigPol_To_DynColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigPolygon, Shape.Category.DynColCircle);
        OverlapInfo overlaps_TriRigPol_To_KinColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigPolygon, Shape.Category.KinColPolygon);
        OverlapInfo overlaps_TriRigPol_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigPolygon, Shape.Category.KinColCircle);
        OverlapInfo overlaps_TriRigPol_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigPolygon, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_TriRigPol_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigPolygon, Shape.Category.TriColCircle);
        
        // trigger circle rigidbody.
        OverlapInfo overlaps_TriRigCir_To_TriRigCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigCircle, Shape.Category.TriRigCircle);
        OverlapInfo overlaps_TriRigCir_To_DynColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigCircle, Shape.Category.DynColPolygon);
        OverlapInfo overlaps_TriRigCir_To_DynColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigCircle, Shape.Category.DynColCircle);
        OverlapInfo overlaps_TriRigCir_To_KinColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigCircle, Shape.Category.KinColPolygon);
        OverlapInfo overlaps_TriRigCir_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigCircle, Shape.Category.KinColCircle);
        OverlapInfo overlaps_TriRigCir_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigCircle, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_TriRigCir_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriRigCircle, Shape.Category.TriColCircle);
        
        // solid polygon collider.
        OverlapInfo overlaps_DynColPol_To_DynColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColPolygon, Shape.Category.DynColPolygon);
        OverlapInfo overlaps_DynColPol_To_DynColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColPolygon, Shape.Category.DynColCircle);
        OverlapInfo overlaps_DynColPol_To_KinColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColPolygon, Shape.Category.KinColPolygon);
        OverlapInfo overlaps_DynColPol_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColPolygon, Shape.Category.KinColCircle);
        OverlapInfo overlaps_DynColPol_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColPolygon, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_DynColPol_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColPolygon, Shape.Category.TriColCircle);
        
        // solid circle collider.
        OverlapInfo overlaps_DynColCir_To_DynColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColCircle, Shape.Category.DynColCircle);
        OverlapInfo overlaps_DynColCir_To_KinColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColCircle, Shape.Category.KinColPolygon);
        OverlapInfo overlaps_DynColCir_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColCircle, Shape.Category.KinColCircle);
        OverlapInfo overlaps_DynColCir_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColCircle, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_DynColCir_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.DynColCircle, Shape.Category.TriColCircle);
        
        // kinematic polygon collider.
        OverlapInfo overlaps_KinColPol_To_KinColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinColPolygon, Shape.Category.KinColPolygon);
        OverlapInfo overlaps_KinColPol_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinColPolygon, Shape.Category.KinColCircle);
        OverlapInfo overlaps_KinColPol_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinColPolygon, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_KinColPol_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinColPolygon, Shape.Category.TriColCircle);
        
        // kinematic circle collider.
        OverlapInfo overlaps_KinColCir_To_KinColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinColCircle, Shape.Category.KinColCircle);
        OverlapInfo overlaps_KinColCir_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinColCircle, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_KinColCir_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.KinColCircle, Shape.Category.TriColCircle);
        
        // trigger polygon collider.
        OverlapInfo overlaps_TriColPol_To_TriColPol = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriColPolygon, Shape.Category.TriColPolygon);
        OverlapInfo overlaps_TriColPol_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriColPolygon, Shape.Category.TriColCircle);
        
        // trigger circle collider.
        OverlapInfo overlaps_TriColCir_To_TriColCir = CategorisedLeafOverlaps.GetOverlaps(overlaps, Shape.Category.TriColCircle, Shape.Category.TriColCircle);

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
            TransformAllShapesVertices(activeBodies, nodes, globalVertices, localVertices, shapes, globalScalesX, globalScalesY, 
                globalPositionsX, globalPositionsY, globalSines, globalCosines, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, 
                centroidsX, centroidsY, baseRadii, globalRadii
            );
            state.TransformPhysicsBodiesStopwatch.Stop();


            // Find collisions.
            state.FindCollisionsStopwatch.Restart();
                        
            Collisions.Detection.DynamicRigidPolygon_To_DynamicRigidPolygon(    overlaps_DynRigPol_To_DynRigPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_DynamicRigidCircle(     overlaps_DynRigPol_To_DynRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_KinematicRigidPolygon(overlaps_DynRigPol_To_KinRigPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_KinematicRigidCircle( overlaps_DynRigPol_To_KinRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_TriggerRigidPolygon(  overlaps_DynRigPol_To_TriRigPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.DynamicRigidPolygon_To_TriggerRigidCircle(   overlaps_DynRigPol_To_TriRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.DynamicRigidPolygon_To_DynamicColliderPolygon(     overlaps_DynRigPol_To_DynColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_DynamicColliderCircle(      overlaps_DynRigPol_To_DynColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_KinematicColliderPolygon( overlaps_DynRigPol_To_KinColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_KinematicColliderCircle(  overlaps_DynRigPol_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_TriggerColliderPolygon(   overlaps_DynRigPol_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.DynamicRigidPolygon_To_TriggerColliderCircle(    overlaps_DynRigPol_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.DynamicRigidCircle_To_DynamicRigidCircle(      overlaps_DynRigCir_To_DynRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_KinematicRigidPolygon( overlaps_DynRigCir_To_KinRigPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_KinematicRigidCircle(  overlaps_DynRigCir_To_KinRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_TriggerRigidPolygon(   overlaps_DynRigCir_To_TriRigPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.DynamicRigidCircle_To_TriggerRigidCircle(    overlaps_DynRigCir_To_TriRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.DynamicRigidCircle_To_DynamicColliderPolygon(      overlaps_DynRigCir_To_DynColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_DynamicColliderCircle(       overlaps_DynRigCir_To_DynColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_KinematicColliderPolygon(  overlaps_DynRigCir_To_KinColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_KinematicColliderCircle(   overlaps_DynRigCir_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_TriggerColliderPolygon(    overlaps_DynRigCir_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.DynamicRigidCircle_To_TriggerColliderCircle(     overlaps_DynRigCir_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);

            Collisions.Detection.KinematicRigidPolygon_To_KinematicRigidPolygon(overlaps_KinRigPol_To_KinRigPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);            
            Collisions.Detection.KinematicRigidPolygon_To_KinematicRigidCircle( overlaps_KinRigPol_To_KinRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidPolygon_To_TriggerRigidPolygon(  overlaps_KinRigPol_To_TriRigPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicRigidPolygon_To_TriggerRigidCircle(   overlaps_KinRigPol_To_TriRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidPolygon_To_DynamicColliderPolygon(     overlaps_KinRigPol_To_DynColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.KinematicRigidPolygon_To_DynamicColliderCircle(      overlaps_KinRigPol_To_DynColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.KinematicRigidPolygon_To_KinematicColliderPolygon( overlaps_KinRigPol_To_KinColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicRigidPolygon_To_KinematicColliderCircle(  overlaps_KinRigPol_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidPolygon_To_TriggerColliderPolygon(   overlaps_KinRigPol_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicRigidPolygon_To_TriggerColliderCircle(    overlaps_KinRigPol_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.KinematicRigidCircle_To_KinematicRigidCircle(  overlaps_KinRigCir_To_KinRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_TriggerRigidPolygon(   overlaps_KinRigCir_To_TriRigPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_TriggerRigidCircle(    overlaps_KinRigCir_To_TriRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_DynamicColliderPolygon(      overlaps_KinRigCir_To_DynColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.KinematicRigidCircle_To_DynamicColliderCircle(       overlaps_KinRigCir_To_DynColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.KinematicRigidCircle_To_KinematicColliderPolygon(  overlaps_KinRigCir_To_KinColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_KinematicColliderCircle(   overlaps_KinRigCir_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_TriggerColliderPolygon(    overlaps_KinRigCir_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_TriggerColliderCircle(     overlaps_KinRigCir_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
        
            Collisions.Detection.TriggerRigidPolygon_To_TriggerRigidPolygon(  overlaps_TriRigPol_To_TriRigPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerRigidPolygon_To_TriggerRigidCircle(   overlaps_TriRigPol_To_TriRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidPolygon_To_DynamicColliderPolygon(     overlaps_TriRigPol_To_DynColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerRigidPolygon_To_DynamicColliderCircle(      overlaps_TriRigPol_To_DynColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidPolygon_To_KinematicColliderPolygon( overlaps_TriRigPol_To_KinColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerRigidPolygon_To_KinematicColliderCircle(  overlaps_TriRigPol_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidPolygon_To_TriggerColliderPolygon(   overlaps_TriRigPol_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerRigidPolygon_To_TriggerColliderCircle(    overlaps_TriRigPol_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.TriggerRigidCircle_To_TriggerRigidCircle(    overlaps_TriRigCir_To_TriRigCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_DynamicColliderPolygon(      overlaps_TriRigCir_To_DynColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_DynamicColliderCircle(       overlaps_TriRigCir_To_DynColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_KinematicColliderPolygon(  overlaps_TriRigCir_To_KinColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_KinematicColliderCircle(   overlaps_TriRigCir_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_TriggerColliderPolygon(    overlaps_TriRigCir_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_TriggerColliderCircle(     overlaps_TriRigCir_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);

            Collisions.Detection.DynamicColliderPolygon_To_DynamicColliderPolygon(     overlaps_DynColPol_To_DynColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderPolygon_To_DynamicColliderCircle(      overlaps_DynColPol_To_DynColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderPolygon_To_KinematicColliderPolygon( overlaps_DynColPol_To_KinColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderPolygon_To_KinematicColliderCircle(  overlaps_DynColPol_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderPolygon_To_TriggerColliderPolygon(   overlaps_DynColPol_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.DynamicColliderPolygon_To_TriggerColliderCircle(    overlaps_DynColPol_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.DynamicColliderCircle_To_DynamicColliderCircle(       overlaps_DynColCir_To_DynColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderCircle_To_KinematicColliderPolygon(  overlaps_DynColCir_To_KinColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderCircle_To_KinematicColliderCircle(   overlaps_DynColCir_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderCircle_To_TriggerColliderPolygon(    overlaps_DynColCir_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.DynamicColliderCircle_To_TriggerColliderCircle(     overlaps_DynColCir_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
        
            Collisions.Detection.KinematicColliderPolygon_To_KinematicColliderPolygon( overlaps_KinColPol_To_KinColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicColliderPolygon_To_KinematicColliderCircle(  overlaps_KinColPol_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicColliderPolygon_To_TriggerColliderPolygon(   overlaps_KinColPol_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicColliderPolygon_To_TriggerColliderCircle(    overlaps_KinColPol_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
        
            Collisions.Detection.KinematicColliderCircle_To_KinematicColliderCircle(  overlaps_KinColCir_To_KinColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.KinematicColliderCircle_To_TriggerColliderPolygon(   overlaps_KinColCir_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicColliderCircle_To_TriggerColliderCircle(    overlaps_KinColCir_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);

            Collisions.Detection.TriggerColliderPolygon_To_TriggerColliderPolygon(  overlaps_TriColPol_To_TriColPol, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerColliderPolygon_To_TriggerColliderCircle(   overlaps_TriColPol_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.TriggerColliderCircle_To_TriggerColliderCircle(overlaps_TriColCir_To_TriColCir, bvhLeafIndices, collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);

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

        Collisions.Manifold.CompleteStep(state.CollisionManifoldState);

        // Transform bodies by collision resolution.
        // NOTE: this is needed at the end as the final
        // sub-step iteration does not transform the bodies
        // at the end of it's loop; meaning the final collision
        // resolution wouldn't be applied.
        TransformAllShapesVertices(activeBodies, nodes, globalVertices, localVertices, shapes, globalScalesX, globalScalesY, 
            globalPositionsX, globalPositionsY, globalSines, globalCosines, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, 
            centroidsX, centroidsY, baseRadii, globalRadii
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

                if(gravityAffected[bodyIndex])
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

                // calculate the global position of the center of mass before rotation.
                float localComX = localCentersOfMassX[bodyIndex];
                float localComY = localCentersOfMassY[bodyIndex];
                float globalComX = 0;
                float globalComY = 0;
                Body.CalculateGlobalCenterOfMass(bodyPosX, bodyPosY, bodySine, bodyCosine, localComX, localComY, 
                    ref globalComX, ref globalComY
                );

                // rotate the body by the anfular velocity.
                Math.Math.RotorMultiply(bodySine, bodyCosine, angularVelocities[bodyIndex] * deltaTime, 
                    ref bodySine, ref bodyCosine
                );
                rotationRadians[bodyIndex] = MathF.Atan2(bodySine, bodyCosine);

                // reverse the calculation using the new rotation values.
                // keeping the center of mass as the point of rotation rather than the body's global position.
                bodyPosX = globalComX - (localComX * bodyCosine - localComY * bodySine);
                bodyPosY = globalComY - (localComX * bodySine + localComY * bodyCosine);

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
    ///     Transforms <c>InUse<c/> bodies local-space vertices by their global-space transforms.
    /// </summary>
    /// <remarks>
    ///     All arrays must be of the same length and elements should be vertivally accessible via <c>physicsBodyIndex</c>. 
    /// </remarks>
    public static void TransformAllShapesVertices(SwapBackArray<int> activeBodies, IntrusiveList.Node[] nodes, FsSoa_Vector2 globalVertices, 
        FsSoa_Vector2 localVertices, Shape.Rigid.ShapeType[] shapes, float[] globalScalesX, float[] globalScalesY, float[] globalPositionsX, 
        float[] globalPositionsY, float[] globalSines, float[] globalCosines, float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, 
        float[] maxAabbsY, float[] centroidsX, float[] centroidsY, float[] localRadii, float[] globalRadii
    )
    {
        FsSoa_Vector2.ClearAppendCounts(globalVertices);
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
                    TransformShapeVertices(globalVertices, localVertices, globalPositionsX, globalPositionsY, globalScalesX, globalScalesY, 
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
    public static void TransformShapeVertices(FsSoa_Vector2 globalVertices, FsSoa_Vector2 localVertices, float[] globalPositionsX, 
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

            // store the newly transformed vertex into the global vertices array.
            // (TODO): this will need to be changed so that you can append directly to an entry element index
            // if you already know the element index. Create a new unsafe function for it.
            FsSoa_Vector2.Append(globalVertices, shapeIndex, x, y);
        }

        // set the new centroid.
        Shape.GetVerticesUnsafe(globalVertices, shapeIndex, ref vertsX, ref vertsY);
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
            CollisionResolutionCategory.Dynamic,
            CollisionResolutionCategory.Dynamic
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
            CollisionResolutionCategory.Dynamic,
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
            collisionsToResolve, CollisionResolutionCategory.Dynamic, CollisionResolutionCategory.Dynamic
        );

        ResolveRigidBodyCollisions(collisions, nodes, collisionNormalsX, collisionNormalsY, firstContactPointsX, firstContactPointsY, 
            secondContactPointsX, secondContactPointsY, linearVelocitiesX, linearVelocitiesY, restitutions, kineticFrictions, 
            staticFrictions, angularVelocities, masses, inverseMasses, inverseRotationalInertia, localCentersOfMassX, 
            localCentersOfMassY, globalPositionsX, globalPositionsY, twoContactPoints, rotationalResponses, contactPointsX, 
            contactPointsY, distsAX, distsAY, distsBX, distsBY, impulseMagnitudes, impulsesX, impulsesY, collisionsStride, otherIsKinematic
        );

        collisions = CategorisedOverlapArray.GetOverlaps(
            collisionsToResolve, CollisionResolutionCategory.Dynamic, CollisionResolutionCategory.Kinematic
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

        // public static GenIdResult Deallocate(State state, GenId genId)
        // {
        //     GenIdResult result = EntityRegistry.Deallocate(state.Entities, genId);

        //     if(result == GenIdResult.Ok)
        //     {            
        //         int index = GenId.GetIndex(genId);

        //         // this is here temporarily and SHOULD be removed.
        //         FsSoa_Vector2.ClearEntryAppendCount(state.BaseVertices, index);

        //         Shape.DecrementCategoryCounter(state, state.Categories[index]);

        //         state.GravityAffected[index] = false;

        //         SetActiveUnsafe(state, GenId.GetIndex(genId), false);
        //     }
            
        //     return result;
        // }

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
                        // center of mass in global space.
                        // note: use global-sapce coordinates first as shape centroids are in global space.
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

        /// <summary>
        ///     Projects a local center of mass onto a global body transform.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void CalculateGlobalCenterOfMass(float bodyGlobalPositionX, float bodyGlobalPositionY,
            float bodyGlobalSine, float bodyGlobalCosine, float localCenterOfMassX, float localCenterOfMassY,
            ref float globalCenterOfMassXOutput, ref float globalCenterOfMassYOutput
        )
        {
            // calculate the global position of the center of mass before rotation.
            globalCenterOfMassXOutput = bodyGlobalPositionX + (localCenterOfMassX * bodyGlobalCosine - localCenterOfMassY * bodyGlobalSine);
            globalCenterOfMassYOutput = bodyGlobalPositionY + (localCenterOfMassX * bodyGlobalSine + localCenterOfMassY * bodyGlobalCosine);
        }
    }




    /******************
    
        class CollisionShapes
    
    *******************/




    public class Shape
    {


        

        /******************
        
            Behaviour
        
        *******************/
        



        public enum Behaviour : int
        {
            Dynamic,
            Kinematic,
            Trigger
        }




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

            public const int DynRigPolygon      = 0;
            public const int DynRigCircle       = 1;
            public const int DynRigCapsule      = 2;
            
            public const int TriRigPolygon    = 3;
            public const int TriRigCircle    = 4;
            public const int TriRigCapsule    = 5;

            // note: everything greater than KinematicRigidPolygon
            // is not apart of the rigid body movement step.

            public const int KinRigPolygon  = 6;
            public const int KinRigCircle   = 7;
            public const int KinRigCapsule  = 8;
            
            public const int DynColPolygon       = 9;
            public const int DynColCircle        = 10;
            public const int DynColCapsule       = 11;
            
            public const int TriColPolygon     = 12;
            public const int TriColCircle      = 13;
            public const int TriColCapsule     = 14;

            public const int KinColPolygon   = 15;
            public const int KinColCircle    = 16;
            public const int KinColCapsule   = 17;




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
                (category >= TriRigPolygon && category <= TriRigCapsule) || 
                (category >= TriColPolygon && category <=  TriColCapsule);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsSolid(int category)
            {
                return 
                (category >= DynRigPolygon && category <= DynRigCapsule) || 
                (category >= DynColPolygon && category <= DynColCapsule);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsKinematic(int category)
            {
                return 
                (category >= KinRigPolygon && category <= KinRigCapsule) || 
                (category >= KinColPolygon && category <=  KinColCapsule);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsRigidBody(int category)
            {
                return category >= DynRigPolygon && category <= KinRigCapsule;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetToRigidBody(ref int category)
            {
                category = category switch
                {
                    DynRigPolygon   => DynRigPolygon,
                    DynRigCircle    => DynRigCircle ,
                    DynRigCapsule   => DynRigCapsule,

                    TriRigPolygon => TriRigPolygon,
                    TriRigCircle  => TriRigCircle ,
                    TriRigCapsule => TriRigCapsule,

                    KinRigPolygon  => KinRigPolygon,
                    KinRigCircle   => KinRigCircle ,
                    KinRigCapsule  => KinRigCapsule,
                    
                    DynColPolygon => DynRigPolygon,
                    DynColCircle  => DynRigCircle ,
                    DynColCapsule => DynRigCapsule,

                    TriColPolygon  => TriRigPolygon,
                    TriColCircle   => TriRigCircle ,
                    TriColCapsule  => TriRigCapsule,

                    KinColPolygon   => KinRigPolygon,
                    KinColCircle    => KinRigCircle ,
                    KinColCapsule   => KinRigCapsule,

                    _ => throw new Exception()
                };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetToCollider(ref int category)
            {
                category = category switch
                {
                    DynRigPolygon   => DynColPolygon,
                    DynRigCircle    => DynColCircle ,
                    DynRigCapsule   => DynColCapsule,

                    TriRigPolygon => TriColPolygon,
                    TriRigCircle  => TriColCircle ,
                    TriRigCapsule => TriColCapsule,

                    KinRigPolygon  => KinColPolygon,
                    KinRigCircle   => KinColCircle ,
                    KinRigCapsule  => KinColCapsule,
                    
                    DynColPolygon => DynColPolygon,
                    DynColCircle  => DynColCircle ,
                    DynColCapsule => DynColCapsule,

                    TriColPolygon  => TriColPolygon,
                    TriColCircle   => TriColCircle ,
                    TriColCapsule  => TriColCapsule,

                    KinColPolygon   => KinColPolygon,
                    KinColCircle    => KinColCircle ,
                    KinColCapsule   => KinColCapsule,

                    _ => throw new Exception()
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void IncrementCategoryCounter(State state, int category)
        {
            switch (category)
            {
                case Category.DynRigPolygon: state.DynamicRigidPolygonCount++; break;
                case Category.DynRigCircle: state.DynamicRigidCircleCount++; break;
                case Category.DynRigCapsule: state.DynamicRigidCapsuleCount++; break;
                
                case Category.TriRigPolygon: state.TriggerRigidPolygonCount++; break;
                case Category.TriRigCircle: state.TriggerRigidCircleCount++; break;
                case Category.TriRigCapsule: state.TriggerRigidCapsuleCount++; break;

                case Category.KinRigPolygon: state.KinematicRigidPolygonCount++; break;
                case Category.KinRigCircle: state.KinematicRigidCircleCount++; break;
                case Category.KinRigCapsule: state.KinematicRigidCapsuleCount++; break;
                
                case Category.DynColPolygon: state.DynamicColliderPolygonCount++; break;
                case Category.DynColCircle: state.DynamicColliderCircleCount++; break;
                case Category.DynColCapsule: state.DynamicColliderCapsuleCount++; break;
                
                case Category.TriColPolygon: state.TriggerColliderPolygonCount++; break;
                case Category.TriColCircle: state.TriggerColliderCircleCount++; break;
                case Category.TriColCapsule: state.TriggerColliderCapsuleCount++; break;

                case Category.KinColPolygon: state.KinematicColliderPolygonCount++; break;
                case Category.KinColCircle: state.KinematicColliderCircleCount++; break;
                case Category.KinColCapsule: state.KinematicColliderCapsuleCount++; break;

                default:
                    throw new Exception();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void DecrementCategoryCounter(State state, int category)
        {
            switch (category)
            {
                case Category.DynRigPolygon: state.DynamicRigidPolygonCount--; break;
                case Category.DynRigCircle: state.DynamicRigidCircleCount--; break;
                case Category.DynRigCapsule: state.DynamicRigidCapsuleCount--; break;
                
                case Category.TriRigPolygon: state.TriggerRigidPolygonCount--; break;
                case Category.TriRigCircle: state.TriggerRigidCircleCount--; break;
                case Category.TriRigCapsule: state.TriggerRigidCapsuleCount--; break;

                case Category.KinRigPolygon: state.KinematicRigidPolygonCount--; break;
                case Category.KinRigCircle: state.KinematicRigidCircleCount--; break;
                case Category.KinRigCapsule: state.KinematicRigidCapsuleCount--; break;
                
                case Category.DynColPolygon: state.DynamicColliderPolygonCount--; break;
                case Category.DynColCircle: state.DynamicColliderCircleCount--; break;
                case Category.DynColCapsule: state.DynamicColliderCapsuleCount--; break;
                
                case Category.TriColPolygon: state.TriggerColliderPolygonCount--; break;
                case Category.TriColCircle: state.TriggerColliderCircleCount--; break;
                case Category.TriColCapsule: state.TriggerColliderCapsuleCount--; break;

                case Category.KinColPolygon: state.KinematicColliderPolygonCount--; break;
                case Category.KinColCircle: state.KinematicColliderCircleCount--; break;
                case Category.KinColCapsule: state.KinematicColliderCapsuleCount--; break;

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
                    return ref state.Materials.StaticFriction[0];
                }

                if(IsRigidBodyUnsafe(state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.Materials.StaticFriction[0];            
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
                return ref state.Materials.StaticFriction[body];
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
                    return ref state.Materials.KineticFriction[0];
                }

                if(IsRigidBodyUnsafe(state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.Materials.KineticFriction[0];            
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
                return ref state.Materials.KineticFriction[entityIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetDensity(State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;

                    // return a ref to the nil.
                    return ref state.Materials.Density[0];
                }

                if(IsRigidBodyUnsafe(state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.Materials.Density[0];            
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
                return ref state.Materials.Density[entityIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetRestitution(State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(EntityRegistry.IsGenIdStale(state.Entities, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;

                    // return a ref to the nil.
                    return ref state.Materials.Restitution[0];
                }

                if(IsRigidBodyUnsafe(state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.Materials.Restitution[0];            
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
                return ref state.Materials.Restitution[entityIndex];
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
        public static int SetCategory(State state, Rigid.ShapeType shape, Shape.Behaviour behaviour, bool rigidbodyEnabled, 
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
                        Shape.Behaviour.Dynamic 
                            => rigidbodyEnabled? Category.DynRigPolygon : Category.DynColPolygon,
                        Shape.Behaviour.Kinematic
                            => rigidbodyEnabled? Category.KinRigPolygon : Category.KinColPolygon,
                        Shape.Behaviour.Trigger
                            => rigidbodyEnabled? Category.TriRigPolygon : Category.TriColPolygon,
                        _ => throw new Exception()
                    };
                break;

                case Rigid.ShapeType.Circle:
                    category = behaviour switch 
                    {
                        Shape.Behaviour.Dynamic 
                            => rigidbodyEnabled? Category.DynRigCircle : Category.DynColCircle,
                        Shape.Behaviour.Kinematic
                            => rigidbodyEnabled? Category.KinRigCircle : Category.KinColCircle,
                        Shape.Behaviour.Trigger
                            => rigidbodyEnabled? Category.TriRigCircle : Category.TriColCircle,
                        _ => throw new Exception()
                    };
                break;

                default:
                    throw new Exception();
            }

            return category;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void PrepareCollisionShapeAllocation(State state, Shape.Behaviour colliderBehaviour, int shapeIndex, int bodyIndex, 
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

            TransformShapeVertices(state.GlobalVertices, state.BaseVertices, globalTransforms.Positions.X, globalTransforms.Positions.Y, 
                globalTransforms.Scales.X, globalTransforms.Scales.Y, globalTransforms.Cosines, globalTransforms.Sines, 
                state.Aabbs.MinX, state.Aabbs.MinY, state.Aabbs.MaxX, state.Aabbs.MaxY, state.BaseRadii, state.GlobalRadii, 
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
                    Shape.Behaviour colliderBehaviour, GenId bodyId, ref GenId colliderId
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
                    Material material, Shape.Behaviour colliderBehaviour, bool rotationalResponse, GenId bodyId, ref GenId genId
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
                        Soa_Material.Insert(state.Materials, material.StaticFriction, material.KineticFriction, 
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
                    state.GlobalRadii[shapeIndex] = radius;

                    float mass = CalculateMass(radius, state.Materials.Density[shapeIndex]); 
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
                    Shape.Behaviour colliderBehaviour, GenId bodyId, ref GenId genId
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
                    Material material, Shape.Behaviour colliderBehaviour, bool rotationalResponse, GenId bodyId, ref GenId genId
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
                        Soa_Material.Insert(state.Materials, material.StaticFriction, material.KineticFriction, 
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

                    float mass = CalculateMass(width, height, state.Materials.Density[shapeIndex]); 
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

        public static GenIdResult Deallocate(State state, GenId genId, bool recalculateBodyCenterOfMass)
        {
            if (EntityRegistry.IsGenIdStale(state.Entities, genId))
            {
                return GenIdResult.StaleGenId;
            }

            int entityIndex = GenId.GetIndex(genId);

            if(state.EntityTypes[entityIndex] != EntityType.Shape)
            {
                System.Diagnostics.Debug.Assert(false);
                return GenIdResult.NotAllocated;                
            }

            DeallocateUnsafe(state, entityIndex, recalculateBodyCenterOfMass);

            return GenIdResult.Ok;
        }

        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>stale id checks are not enforced; the entity index will always go through the deallocation procedure.</para>
        /// </remarks>
        public static void DeallocateUnsafe(State state, int entityIndex, bool recalculateBodyCenterOfMass)
        {   
            EntityRegistry.DeallocateUnsafe(state.Entities, entityIndex);         
            DecrementCategoryCounter(state, state.Categories[entityIndex]);
            SetActiveUnsafe(state, entityIndex, false);
            int bodyIndex = state.BodyHierarchy.Nodes[entityIndex].Parent;
            IntrusiveList.RemoveFromTree(state.BodyHierarchy, entityIndex);
            SetActiveUnsafe(state, entityIndex, false);
            
            if (recalculateBodyCenterOfMass)
            {
                Body.IntegrateShapePropertiesUnsafe(state, bodyIndex);
            }
        }
    }




    /******************
    
        Debug Drawing.
    
    *******************/




    public static void Draw(HowlAppState howl, State state, float deltaTime)
    {
        if (state.DrawBodyGlobalPositions)
        {
            DrawGlobalPositions(howl, state.BodyHierarchy.RootIndices, state.BodyHierarchy.Nodes, 
                state.GlobalTransforms.Positions.X, state.GlobalTransforms.Positions.Y
            );            
        }

        if (state.DrawShapes)
        {            
            DrawShapes(howl, state.CollisionManifoldState, state.GlobalVertices, state.BodyHierarchy.RootIndices, state.BodyHierarchy.Nodes, 
                state.Centroids.X, state.Centroids.Y, state.GlobalRadii, state.Categories
            );
        }

        if (state.DrawCentroids)
        {
            DrawCentroids(howl, state.Centroids, state.BodyHierarchy.RootIndices, state.BodyHierarchy.Nodes);
        }

        if (state.DrawLinearVelocities)
        {            
            DrawLinearVelocities(howl, state.BodyHierarchy.RootIndices, state.LinearVelocities, 
                state.GlobalTransforms.Positions.X, state.GlobalTransforms.Positions.Y
            );
        }

        if (state.DrawAabbs)
        {
            DrawAabbs(howl, state.BodyHierarchy.RootIndices, state.BodyHierarchy.Nodes, state.Aabbs.MinX, state.Aabbs.MinY, 
                state.Aabbs.MaxX, state.Aabbs.MaxY
            );
        }

        if (state.DrawBvhBranches)
        {
            BoundingVolumeHierarchy.DrawBranches(howl, state.Bvh, Colour.Yellow);
        }

        if (state.DrawLeaves)
        {    
            BoundingVolumeHierarchy.DrawLeaves(howl, state.Bvh, Colour.Yellow);
        }

        if (state.DrawCollisionInformation)
        {
            DrawCollisionInformation(howl, state.CollisionManifoldState);
        }

        DrawCentersOfMass(howl, state.BodyHierarchy.RootIndices, state.GlobalTransforms.Positions.X, state.GlobalTransforms.Positions.Y,
            state.GlobalTransforms.Sines, state.GlobalTransforms.Cosines, state.LocalCentersOfMass.X, state.LocalCentersOfMass.Y
        );
    }

    public static void DrawGlobalPositions(HowlAppState howl, SwapBackArray<int> activeBodies, IntrusiveList.Node[] nodes, 
        float[] globalPositionsX, float[] globalPositionsY
    )
    {
        for(int i = 1; i < activeBodies.Count; i++) // skip nil.
        {
            int bodyIndex = activeBodies[i];
            ref IntrusiveList.Node bodyNode = ref nodes[bodyIndex];
            int firstShapeIndex = bodyNode.FirstChild;

            if(firstShapeIndex == 0)
            {
                continue;
            }

            int shapeIndex = firstShapeIndex;

            while (true)
            {
                Debug.DrawWireCircle(howl, new Circle(globalPositionsX[shapeIndex], globalPositionsY[shapeIndex], 0.1f), Colour.White, 
                    DrawSpace.World
                );

                shapeIndex = nodes[shapeIndex].NextSibling;

                if(shapeIndex == firstShapeIndex)
                {
                    break;
                }
            }
        }
    }

    public static void DrawCentersOfMass(HowlAppState howl, SwapBackArray<int> activeBodies, 
        float[] globalPositionsX, float[] globalPositionsY, float[] globalSines, float[] globalCosines, 
        float[] localCentersOfMassX, float[] localCentersOfMassY
    )
    {
        for(int i = 1; i < activeBodies.Count; i++)
        {
            int bodyIndex = activeBodies[i];

            float comX = 0;
            float comY = 0;

            Body.CalculateGlobalCenterOfMass(globalPositionsX[bodyIndex], globalPositionsY[bodyIndex],
                globalSines[bodyIndex], globalCosines[bodyIndex], localCentersOfMassX[bodyIndex], localCentersOfMassY[bodyIndex],
                ref comX, ref comY
            );

            Debug.DrawWireCircle(howl, new Circle(comX, comY, 0.1f), CenterOfMassColour, DrawSpace.World);
        }
    }

    public static void DrawShapes(HowlAppState howl, Collisions.Manifold.State collisions, FsSoa_Vector2 vertices,
        SwapBackArray<int> activeBodies, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] radii, 
        int[] categories
    )
    {
        Colour colour = default;

        Span<float> polyVertsX = stackalloc float[vertices.Stride];
        Span<float> polyVertsY = stackalloc float[vertices.Stride];

        for(int i = 1; i < activeBodies.Count; i++) // skip nil.
        {
            int bodyIndex = activeBodies[i];
            ref IntrusiveList.Node bodyNode = ref nodes[bodyIndex];
            int firstShapeIndex = bodyNode.FirstChild;
            if(firstShapeIndex == 0)
            {
                continue;
            }

            int shapeIndex = firstShapeIndex;
            while (true)
            {
                ref int category = ref categories[shapeIndex];

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
                    colour = Collisions.Manifold.HasContacts(collisions, shapeIndex)
                    ? ActiveTriggerShapeColour
                    : PassiveTriggerShapeColour;            

                }
                else
                {
                    colour = FallbackShapeColour;
                }

                if (Shape.Category.IsPolygon(category))
                {
                    Shape.GetVerticesUnsafe(vertices, shapeIndex, ref polyVertsX, ref polyVertsY);
                    Debug.DrawWirePoly(howl, polyVertsX, polyVertsY, colour, DrawSpace.World);
                }
                else if (Shape.Category.IsCircle(category))
                {
                    Circle shape = new(centroidsX[shapeIndex], centroidsY[shapeIndex], radii[shapeIndex]);
                    Debug.DrawWireCircle(howl, shape, colour, DrawSpace.World);
                }

                shapeIndex = nodes[shapeIndex].NextSibling;
                if (shapeIndex == firstShapeIndex)
                {
                    break;
                }
            }
        }
    }

    public static void DrawCentroids(HowlAppState app, Soa_Vector2 centroids, SwapBackArray<int> activeBodies, IntrusiveList.Node[] nodes)
    {
        // hoisting invariance.
        Span<float> centroidsX = centroids.X;
        Span<float> centroidsY = centroids.Y;

        int count = activeBodies.Count;
        for(int i = 1; i < count; i++) // start at one to skip Nil.
        {
            int bodyIndex = activeBodies[i]; 
            ref IntrusiveList.Node bodyNode = ref nodes[bodyIndex];

            if(bodyNode.FirstChild == 0)
            {
                continue;
            }

            int firstShapeIndex = bodyNode.FirstChild;
            int shapeIndex = firstShapeIndex;
            while (true)
            {
                Debug.DrawWireCircle(app, new Circle(centroidsX[shapeIndex], centroidsY[shapeIndex], 0.1f), CentroidColour, DrawSpace.World);
                
                shapeIndex = nodes[shapeIndex].NextSibling;
                if(shapeIndex == firstShapeIndex)
                {
                    break;
                }
            }
        }
    }

    public static void DrawLinearVelocities(HowlAppState app, SwapBackArray<int> activeBodies,
        Soa_Vector2 linearVelocities, float[] globalPositionsX, float[] globalPositionsY
    )
    {
        // hoisting invariance.
        Span<float> linearVelocitiesX = linearVelocities.X;
        Span<float> linearVelocitiesY = linearVelocities.Y;

        int count = activeBodies.Count;
        for(int i = 1; i < count; i++) // start at one to skip Nil.
        {
            int bodyIndex = activeBodies[i];

            float startX = globalPositionsX[bodyIndex];
            float startY = globalPositionsY[bodyIndex];
            float endX = startX + linearVelocitiesX[bodyIndex];
            float endY = startY + linearVelocitiesY[bodyIndex];

            Debug.DrawLine(app, LinearVelocityColour, new Vector2(startX, startY), new Vector2(endX, endY), DrawSpace.World);
        }
    }

    public static void DrawAabbs(HowlAppState app, SwapBackArray<int> activeBodies, IntrusiveList.Node[] nodes, float[] aabbsMinX, 
        float[] aabbsMinY, float[] aabbsMaxX, float[] aabbsMaxY
    )
    {
        for(int i = 1; i < activeBodies.Count; i++) // start at one to skip Nil.
        {
            int bodyIndex = activeBodies[i];
            ref IntrusiveList.Node bodyNode = ref nodes[bodyIndex];
            int firstShapeIndex = bodyNode.FirstChild;
            if(firstShapeIndex == 0)
            {
                continue;
            }

            int shapeIndex = firstShapeIndex;

            while (true)
            {                
                float minX = aabbsMinX[shapeIndex];
                float minY = aabbsMinY[shapeIndex];
                float maxX = aabbsMaxX[shapeIndex];
                float maxY = aabbsMaxY[shapeIndex];
                Debug.DrawWirePoly(app, [minX, maxX, maxX, minX], [maxY, maxY, minY, minY], AabbColour, DrawSpace.World);

                shapeIndex = nodes[shapeIndex].NextSibling;

                if(shapeIndex == firstShapeIndex)
                {
                    break;
                }
            }
        }
    }

    public static void DrawCollisionInformation(HowlAppState app, Collisions.Manifold.State collisions)
    {
        // hoisitng invariance.
        Span<float> firstContactPointsX = collisions.FirstContactPoints.X;
        Span<float> firstContactPointsY = collisions.FirstContactPoints.Y;
        Span<float> secondContactPointsX = collisions.SecondContactPoints.X;
        Span<float> secondContactPointsY = collisions.SecondContactPoints.Y;
        Span<float> normalsX = collisions.Normals.X;
        Span<float> normalsY = collisions.Normals.Y;
        Span<float> otherCentroidsX = collisions.ColliderCentroids.X;
        Span<float> otherCentroidsY = collisions.ColliderCentroids.Y;
        Span<bool> twoContactPoints = collisions.TwoContactPoints;


        float contactPointX;
        float contactPointY;
        float normalX;
        float normalY;
        float otherCentroidX;
        float otherCentroidY;
        
        Math.Vector2 normalStart;
        Math.Vector2 normalEnd;

        int[] active = collisions.ActiveIndices;
        int[] activeCounts = collisions.ActiveIndicesCount;

        for(int i = 0; i < activeCounts.Length; i++)
        {
            int count = activeCounts[i];
            if(count<= 0)
            {
                continue;
            }
            int entryElementIndex = FixedStrideArray.GetElementIndex(i, collisions.Stride, 0);
            for(int j = 0; j < count; j++)
            {
                int elementIndex = entryElementIndex+j;
                int collisionIndex = active[elementIndex];

                int ownerIndex = collisionIndex / collisions.Stride; // int div truncates the remainder, always giving the owner index.
                int otherIndex = collisionIndex % collisions.Stride;

                // avoid duplicate collisions.
                if(ownerIndex > otherIndex)
                {
                    continue;
                }

                // get normal data.
                normalX = normalsX[collisionIndex];
                normalY = normalsY[collisionIndex];
                
                // get contact point 1 data.
                contactPointX = firstContactPointsX[collisionIndex];
                contactPointY = firstContactPointsY[collisionIndex];
                
                // get centroid data.
                otherCentroidX = otherCentroidsX[collisionIndex];
                otherCentroidY = otherCentroidsY[collisionIndex];

                // draw centroids.
                Debug.DrawWireCircle(app, new Circle(otherCentroidX, otherCentroidY, 0.1f), CollisionOtherColour, DrawSpace.World);

                // draw contact point 1.
                Debug.DrawWireCircle(app, new Circle(contactPointX, contactPointY, 0.1f), ContactPointColour, DrawSpace.World);            

                // draw normal from contact point. 
                normalStart = new Vector2(contactPointX, contactPointY);
                normalEnd = normalStart + new Vector2(normalX, normalY);
                Debug.DrawLine(app, CollisionNormalColour, normalStart, normalEnd, DrawSpace.World);

                if (twoContactPoints[collisionIndex])
                {
                    // get contact point 2.
                    contactPointX = secondContactPointsX[collisionIndex];
                    contactPointY = secondContactPointsY[collisionIndex];

                    // draw contact point 2.
                    Debug.DrawWireCircle(app, new Circle(contactPointX, contactPointY, 0.1f), ContactPointColour, DrawSpace.World);            

                    // draw normal from contact point. 
                    normalStart = new Vector2(contactPointX, contactPointY);
                    normalEnd = normalStart + new Vector2(normalX, normalY);
                    Debug.DrawLine(app, CollisionNormalColour, normalStart, normalEnd, DrawSpace.World);
                }
            }
        }
    }




    /******************
    
        Collisions
    
    *******************/




    public static class Collisions
    {
        public enum ContactState : byte
        {
            /// <summary>
            ///     There is no collision between the two bodies.
            /// </summary>
            None,

            /// <summary>
            ///     The two colliders have just started contacting with one another.
            /// </summary>
            Enter,

            /// <summary>
            ///     The two colliders are in sustained contact with one another.
            /// </summary>
            Sustain,

            /// <summary>
            ///     The two colliders have just left contact with one another.
            /// </summary>
            Exit,
        }

        public static class ResolutionCategory
        {
            public const int Dynamic = 0;
            public const int Kinematic = 1;
            public const int Count = 2;
        }

        /// <summary>
        ///     A pair of indices that are registered within a collision manifold.
        /// </summary>
        public struct IndexPair
        {
            public int AToB;
            public int BToA;

            public IndexPair(int aToB, int bToA)
            {
                AToB = aToB;
                BToA = bToA;
            }            
        }

        public unsafe class Callbacks<T> where T : allows ref struct
        {
            public struct Callback
            {
                public delegate* <T, CollisionInfo, void> Pointer;
            }

            public StackArray<Callback>[] OnEnterCallbacks;
            public StackArray<Callback>[] OnSustainCallbacks;
            public StackArray<Callback>[] OnExitCallbacks;

            /// <param name="maxPhysicsBodyCount">the maximum amount of physics bodies.</param>
            /// <param name="maxCallbacks">the maximum amount of callbacks that a physics body can have.</param>
            public Callbacks(int maxPhysicsBodyCount, int maxCallbacks)
            {
                OnEnterCallbacks = new StackArray<Callback>[maxPhysicsBodyCount];
                for(int i = 0; i < maxPhysicsBodyCount; i++)
                {
                    OnEnterCallbacks[i] = new(maxCallbacks);
                }

                OnExitCallbacks = new StackArray<Callback>[maxPhysicsBodyCount];
                for(int i = 0; i < maxPhysicsBodyCount; i++)
                {
                    OnExitCallbacks[i] = new(maxCallbacks);
                }

                OnSustainCallbacks = new StackArray<Callback>[maxPhysicsBodyCount];
                for(int i = 0; i < maxPhysicsBodyCount; i++)
                {
                    OnSustainCallbacks[i] = new(maxCallbacks);
                }
            }
        }

        public static class Callbacks
        {
            /// <summary>
            ///     Pushes a callback onto the <c>OnEnter</c> callback stack at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection to push into.</param>
            /// <param name="callback">the call back to push.</param>
            /// <param name="index">the index of the callback stack to push onto.</param>
            public static void PushOnEnterCallback<T>(CollisionCallbacks<T> callbacks, CollisionCallback<T> callback, int index)
            {
                StackArray.Push(callbacks.OnEnterCallbacks[index], callback);
            }

            /// <summary>
            ///     Clears a stack of <c>OnEnter</c> callbacks stored at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection that contains the stack to clear.</param>
            /// <param name="index">the index of the stack to clear.</param>
            public static void ClearOnEnterCallbacks<T>(CollisionCallbacks<T> collisionCallbacks, int index)
            {
                StackArray.ClearCount(collisionCallbacks.OnEnterCallbacks[index]);
            }

            /// <summary>
            ///     Pushes a callback onto the <c>OnSustain</c> callback stack at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection to push into.</param>
            /// <param name="callback">the call back to push.</param>
            /// <param name="index">the index of the callback stack to push onto.</param>
            public static void PushOnSustainCallback<T>(CollisionCallbacks<T> callbacks, CollisionCallback<T> callback, int index)
            {
                StackArray.Push(callbacks.OnSustainCallbacks[index], callback);
            }

            /// <summary>
            ///     Clears a stack of <c>OnSustain</c> callbacks stored at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection that contains the stack to clear.</param>
            /// <param name="index">the index of the stack to clear.</param>
            public static void ClearOnSustainCallbacks<T>(CollisionCallbacks<T> callbacks, int index)
            {
                StackArray.ClearCount(callbacks.OnSustainCallbacks[index]);
            }

            /// <summary>
            ///     Pushes a callback onto the <c>OnExit</c> callback stack at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection to push into.</param>
            /// <param name="callback">the call back to push.</param>
            /// <param name="index">the index of the callback stack to push onto.</param>
            public static void PushOnExitCallback<T>(CollisionCallbacks<T> callbacks, CollisionCallback<T> callback, int index)
            {
                StackArray.Push(callbacks.OnExitCallbacks[index], callback);
            }

            /// <summary>
            ///     Clears a stack of <c>OnExit</c> callbacks stored at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection that contains the stack to clear.</param>
            /// <param name="index">the index of the stack to clear.</param>
            public static void ClearOnExitCallbacks<T>(CollisionCallbacks<T> callbacks, int index)
            {
                StackArray.ClearCount(callbacks.OnExitCallbacks[index]);
            }
        }

        public ref struct CollisionInfo
        {
            public ref float NormalX;
            public ref float NormalY;
            public ref float FirstContactPointX;
            public ref float FirstContactPointY;
            public ref float SecondContactPointX;
            public ref float SecondContactPointY;
            public ref float Depth;
            public ref bool TwoContactPoints;
            public GenId ColliderPhysicsId;
            public GenId ColliderEntityId;

            public CollisionInfo(ref float normalX, ref float normalY, ref float firstContactPointX, ref float firstContactPointY, 
                ref float secondContactPointX, ref float secondContactPointY, ref float depth, ref bool twoContactPoints, 
                GenId colliderPhysicsId, GenId colliderEntityId
            )
            {
                NormalX = ref normalX;
                NormalY = ref normalY;
                FirstContactPointX = ref firstContactPointX;
                FirstContactPointY = ref firstContactPointY;
                SecondContactPointX = ref secondContactPointX;
                SecondContactPointY = ref secondContactPointY;
                Depth = ref depth;
                TwoContactPoints = ref twoContactPoints;
                ColliderPhysicsId = colliderPhysicsId;
                ColliderPhysicsId = colliderEntityId;
            }
        }

        public static class Manifold
        {
            public class State
            {
                /// <summary>
                ///     The normal vector of a collision.
                /// </summary>
                public Soa_Vector2 Normals;

                /// <summary>
                ///     The centroids of the colliding physics bodys. 
                /// </summary>
                public Soa_Vector2 ColliderCentroids; // this should be removed and use the physics system state centroids instead.

                /// <summary>
                ///     The first contact point of all collisions.
                /// </summary>
                public Soa_Vector2 FirstContactPoints;

                /// <summary>
                ///     The second contact point of all collisions.
                /// </summary>
                public Soa_Vector2 SecondContactPoints;

                /// <summary>
                ///     The depth of the collisions.
                /// </summary>
                public float[] Depths;

                /// <summary>
                ///     Whether or not a collision has a second contact point.
                /// </summary>
                public bool[] TwoContactPoints;

                /// <summary>
                ///     The indices of <c>active</c> collision elements separated by <c>entry</c> in the current step.
                /// </summary>
                /// <remarks>
                ///     Remarks: this array is a fixed-stride swapback array.
                /// </remarks>
                public int[] ActiveIndices;

                /// <summary>
                ///     The count of indices an entry has in the <c>ActiveIndices</c> fixed stride swapwback array.
                /// </summary>
                /// <remarks>
                ///     Remarks: Elements should be accessed via <c>entryIndex</c>.
                /// </remarks>
                public int[] ActiveIndicesCount;

                /// <summary>
                ///     The <c>phase</c> a collision element is of being <c>active</c>.
                /// </summary>
                /// <remarks>
                ///     Value Key:
                ///     <list type = "bullet">
                ///         <item>0: the element has not been active at all.</item>
                ///         <item>1: the element is active in the <c>current</c> step; meaning there is contact between the two colliders.</item>
                ///         <item>2: the element is active in the <c>previous</c> step; meaning the contact between the two colliders has just stopped.</item>
                ///         <item>3: the element is active in the <c>preultimate</c> step; meaning the contact between the two colliders has completely ceased.</item>
                ///     </list>
                /// </remarks>
                public int[] ActivePhase;

                /// <summary>
                ///     The state of all collisions this step.
                /// </summary>
                public ContactState[] ContactStates;

                /// <summary>
                ///     The state of all collisions in the previous step.
                /// </summary>
                public ContactState[] PreviousContactStates;

                /// <summary>
                ///     The fixed stride of each entry.
                /// </summary>
                public int Stride;

                /// <summary>
                ///     The amount of entries this collection can hold.
                /// </summary>
                public int MaxEntries;

                /// <summary>
                ///     Whether this instance has been disposed of.
                /// </summary>
                public bool Disposed;

                /// <summary>
                /// Creates a Structure-Of-Arrays Collision instance.
                /// </summary>
                /// <param name="owner">The owner of this collision.</param>
                /// <param name="other">The other collider of this collision.</param>
                /// <param name="ownerParameters">the owner's parameters.</param>
                /// <param name="otherParameters">the other's parameters.</param>
                /// <param name="xContactPoints">the x-positional value for the contact points.</param>
                /// <param name="yContactPoints">the y-positional value for the contact points.</param>
                /// <param name="ownerColliderShapeCenter">the center of the owner's collider shape</param>
                /// <param name="otherColliderShapeCenter">the center of the other's collider shape</param>
                /// <param name="normalY">the normal of the collision.</param>
                /// <param name="depth">the depth of the collision.</param>
                public State(int totalColliders)
                {
                    System.Diagnostics.Debug.Assert(totalColliders <= Constants.MaxColliders, 
                        $"Collision Manifold total colliders '{totalColliders}' exceeds max collisions colliders  '{Constants.MaxColliders}'"
                    );

                    Math.Math.Clamp(totalColliders, 0, Constants.MaxColliders);

                    Stride = totalColliders;
                    MaxEntries = totalColliders;
                    int dataLength = Stride * MaxEntries;

                    Normals                     = new Soa_Vector2(dataLength);
                    ColliderCentroids           = new Soa_Vector2(dataLength);
                    FirstContactPoints          = new Soa_Vector2(dataLength);
                    SecondContactPoints         = new Soa_Vector2(dataLength);
                    Depths                      = new float[dataLength];
                    TwoContactPoints            = new bool[dataLength];
                    ContactStates               = new ContactState[dataLength];
                    PreviousContactStates       = new ContactState[dataLength];
                    ActivePhase                 = new int[dataLength];
                    ActiveIndices               = new int[dataLength];
                    ActiveIndicesCount          = new int[totalColliders];
                }

                ~State()
                {
                    Manifold.Dispose(this);
                }
            }




            /******************
            
                Setters.
            
            *******************/




            /// <summary>
            ///     Sets a one-way collision data entry at a given index.
            /// </summary>
            /// <param name="state">the state instance to set.</param>
            /// <param name="recipientIndex">the index in the state instance arrays to write to.</param>
            /// <param name="colliderIndex">the physics body index of the of the <c>colliding</c> collider.</param>
            /// <param name="colliderCentroidX">the x-component of the <c>colliding</c> collider's centroid.</param>
            /// <param name="colliderCentroidY">the y-component of the <c>colliding</c> collider's centroid.</param>
            /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
            /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
            /// <param name="contactPointX">the x-component of the contact point.</param>
            /// <param name="contactPointY">the y-component of the contact point.</param>
            /// <param name="depth">the depth of the collision.</param>
            /// <param name="colliderFlags">the physics body flags of the <c>colliding</c> collider.</param>
            /// <returns>the collision index that the data was written to.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static int SetDataOneWay(State state, int recipientIndex, int colliderIndex, 
                float colliderCentroidX, float colliderCentroidY, float normalX, float normalY, float contactPointX, float contactPointY, float depth
            )
            {
                int elementIndex = FixedStrideArray.GetElementIndex(recipientIndex, state.Stride, colliderIndex);

                ref int phase = ref state.ActivePhase[elementIndex];
                if(phase <= 0)
                {
                    FixedStrideSwapBackArray.Append(elementIndex, state.ActiveIndices, state.ActiveIndicesCount, 
                        state.Stride, recipientIndex
                    );
                }
                phase = 1;

                // write data.
                state.Normals.X[elementIndex]              = normalX;
                state.Normals.Y[elementIndex]              = normalY;
                state.ColliderCentroids.X[elementIndex]    = colliderCentroidX;
                state.ColliderCentroids.Y[elementIndex]    = colliderCentroidY;
                state.FirstContactPoints.X[elementIndex]   = contactPointX;
                state.FirstContactPoints.Y[elementIndex]   = contactPointY;
                state.Depths[elementIndex]                 = depth;
                state.TwoContactPoints[elementIndex]       = false;

                return elementIndex;
            }

            /// <summary>
            ///     Sets a one-way collision data entry at a given index.
            /// </summary>
            /// <param name="state">the state instance to set.</param>
            /// <param name="recipientIndex">the index in the state instance arrays to write to.</param>
            /// <param name="colliderIndex">the physics body index of the of the <c>colliding</c> collider.</param>
            /// <param name="colliderCentroidX">the x-component of the <c>colliding</c> collider's centroid.</param>
            /// <param name="colliderCentroidY">the y-component of the <c>colliding</c> collider's centroid.</param>
            /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
            /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
            /// <param name="firstContactPointX">the x-component of the first contact point.</param>
            /// <param name="firstContactPointY">the y-component of the first contact point.</param>
            /// <param name="secondContactPointX">the x-component of the second contact point.</param>
            /// <param name="secondContactPointY">the y-component of the second contact point.</param>
            /// <param name="depth">the depth of the collision.</param>
            /// <param name="colliderFlags">the physics body flags of the <c>colliding</c> collider.</param>
            /// <returns>the collision index the data was written to.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static int SetDataOneWay(State state, int recipientIndex, int colliderIndex, 
                float colliderCentroidX, float colliderCentroidY, float normalX, float normalY, float firstContactPointX, float firstContactPointY, 
                float secondContactPointX, float secondContactPointY, float depth
            )
            {
                int elementIndex = FixedStrideArray.GetElementIndex(recipientIndex, state.Stride, colliderIndex);

                ref int phase = ref state.ActivePhase[elementIndex];
                if(phase <= 0)
                {
                    FixedStrideSwapBackArray.Append(elementIndex, state.ActiveIndices, state.ActiveIndicesCount, 
                        state.Stride, recipientIndex
                    );
                }
                phase = 1;

                state.Normals.X[elementIndex]              = normalX;
                state.Normals.Y[elementIndex]              = normalY;
                state.ColliderCentroids.X[elementIndex]    = colliderCentroidX;
                state.ColliderCentroids.Y[elementIndex]    = colliderCentroidY;
                state.FirstContactPoints.X[elementIndex]   = firstContactPointX;
                state.FirstContactPoints.Y[elementIndex]   = firstContactPointY;
                state.SecondContactPoints.X[elementIndex]  = secondContactPointX;
                state.SecondContactPoints.Y[elementIndex]  = secondContactPointY;
                state.Depths[elementIndex]                 = depth;
                state.TwoContactPoints[elementIndex]       = true;

                return elementIndex;
            }

            /// <summary>
            ///     Sets a two-way collision data entry at a given entry.
            /// </summary>
            /// <param name="state">the state instance to append to.</param>
            /// <param name="indexA">the index of collider A.</param>
            /// <param name="indexB">the index of collider B.</param>
            /// <param name="centroidXA">the x-component of collider A's centroid.</param>
            /// <param name="centroidYA">the y-component of collider A's centroid.</param>
            /// <param name="centroidXB">the x-component of collider B's centroid.</param>
            /// <param name="centroidYB">the y-component of collider B's centroid.</param>
            /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
            /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
            /// <param name="firstContactPointX">the x-component of the first contact point.</param>
            /// <param name="firstContactPointY">the y-component of the first contact point.</param>
            /// <param name="secondContactPointX">the x-component of the second contact point.</param>
            /// <param name="secondContactPointY">the y-component of the second contact point.</param>
            /// <param name="depth">the depth of the collision.</param>
            /// <returns>
            ///     A collision index pair of the collision indices the data was written to.
            /// </returns>
            public static IndexPair SetDataTwoWay(State state, int indexA, int indexB, float centroidXA, float centroidYA, 
                float centroidXB, float centroidYB, float normalX, float normalY, float firstContactPointX, float firstContactPointY, 
                float secondContactPointX, float secondContactPointY, float depth
            )
            {
                int a = SetDataOneWay(state, indexA, indexB, centroidXB, centroidYB, normalX, normalY, firstContactPointX, firstContactPointY, 
                    secondContactPointX, secondContactPointY, depth
                );

                // note: the normal reversing.
                int b = SetDataOneWay(state, indexB, indexA, centroidXA, centroidYA, -normalX, -normalY, firstContactPointX, firstContactPointY, 
                    secondContactPointX, secondContactPointY, depth
                );

                return new (a,b);  
            }

            /// <summary>
            ///     Sets a two-way collision data entry at a given entry.
            /// </summary>
            /// <param name="state">the state instance to append to.</param>
            /// <param name="indexA">the index of collider A.</param>
            /// <param name="indexB">the index of collider B.</param>
            /// <param name="centroidXA">the x-component of collider A's centroid.</param>
            /// <param name="centroidYA">the y-component of collider A's centroid.</param>
            /// <param name="centroidXB">the x-component of collider B's centroid.</param>
            /// <param name="centroidYB">the y-component of collider B's centroid.</param>
            /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
            /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
            /// <param name="contactPointX">the x-component of the contact point.</param>
            /// <param name="contactPointY">the y-component of the contact point.</param>
            /// <param name="depth">the depth of the collision.</param>
            /// <returns>
            ///     A collision index pair of the collision indices the data was written to.
            /// </returns>
            public static IndexPair SetDataTwoWay(State state, int indexA, int indexB, float centroidXA, float centroidYA, 
                float centroidXB, float centroidYB, float normalX, float normalY, float contactPointX, float contactPointY, 
                float depth
            )
            {
                int a = SetDataOneWay(state, indexA, indexB, centroidXB, centroidYB, normalX, normalY, contactPointX, contactPointY, depth);

                // note: the normal reversing.
                int b = SetDataOneWay(state, indexB, indexA, centroidXA, centroidYA, -normalX, -normalY, contactPointX, contactPointY, depth);
                
                return new (a,b);
            }




            /******************
            
                State Handling.
            
            *******************/




            /// <summary>
            ///     Swaps the previous and current contact state context pointers.
            /// </summary>
            /// <param name="state">the state instance to swap.</param>
            public static void SwapContactStateContexts(State state)
            {
                ContactState[] tempContactStates = state.PreviousContactStates;
                state.PreviousContactStates = state.ContactStates;
                state.ContactStates = tempContactStates;
            }

            /// <summary>
            ///     Prepares a state instance for the next step.
            /// </summary>
            /// <parsam name="state">the state instance to prepare.</param>
            public static void PrepareForNextStep(State state)
            {
                SwapContactStateContexts(state);
            }

            /// <summary>
            ///     Completes the update step for a state instance.
            /// </summary>
            /// <param name="state">the state instance to complete the step for.</param>
            public static void CompleteStep(State state)
            {        
                Span<ContactState> contactStates = state.ContactStates;
                Span<ContactState> previousContactStates = state.PreviousContactStates;
                int[] activeIndicesCounts = state.ActiveIndicesCount;
                int[] activeIndices = state.ActiveIndices;
                int[] active = state.ActivePhase;
                int stride = state.Stride;
                int maxEntries = state.MaxEntries;

                // update active counts.
                for(int entryIndex = 0; entryIndex < maxEntries; entryIndex++)
                {
                    int swapBackCount = activeIndicesCounts[entryIndex];
                    if (swapBackCount == 0)
                    {
                        continue;
                    }

                    for(int entryElementIndex = 0; entryElementIndex < swapBackCount; entryElementIndex++)
                    {
                        // get the active phase of the collision.

                        int elementIndex = FixedStrideArray.GetElementIndex(entryIndex, stride, entryElementIndex);;
                        int collisionIndex = activeIndices[elementIndex];
                        ref int phase = ref active[collisionIndex];

                        // update the collision state.
                        ref ContactState previousState = ref previousContactStates[collisionIndex];
                        ref ContactState currentState = ref contactStates[collisionIndex];

                        switch (phase)
                        {
                            case 1:
                                // the collider has began contacting the 
                                switch (previousState)
                                {
                                    case ContactState.Enter:
                                        currentState = ContactState.Sustain;
                                    break;
                                    case ContactState.Exit:
                                        currentState = ContactState.Enter;
                                    break;
                                    case ContactState.None:
                                        currentState = ContactState.Enter;
                                    break;
                                    case ContactState.Sustain:
                                        currentState = ContactState.Sustain;
                                    break;
                                    default:
                                    break;
                                }
                            break;
                            case 2:
                                currentState = ContactState.Exit;
                            break;
                            case 3:
                                currentState = ContactState.None;
                            break;
                            default:
                            break;
                        }

                        // update the active phase of the collision.
                        
                        phase++;
                        phase%=4;
                        if (phase == 0)
                        {
                            FixedStrideSwapBackArray.RemoveAt(activeIndices, activeIndicesCounts, stride, entryIndex, entryElementIndex);
                        }
                    }
                }
            }

            /// <summary>
            ///     Gets whether a collider is in contact with another.
            /// </summary>
            /// <param name="state">the state instance that contains the collider.</param>
            /// <param name="index">the index of the collider in the state instance.</param>
            /// <returns>true, if the collider is in contact with another; otherwise false.</returns>
            public static bool HasContacts(State state, int index)
            {
                return state.ActiveIndicesCount[index] > 0;
            }




            /******************
            
                Disposal.
            
            *******************/




            /// <summary>
            ///     Disposes of a state instance.
            /// </summary>
            /// <param name="state">the state instance to dispose of.</param>
            public static void Dispose(State state)
            {
                if (state.Disposed)
                {
                    return;
                }

                state.Disposed = true;
                
                Soa_Vector2.Dispose(state.Normals);
                state.Normals = null;

                Soa_Vector2.Dispose(state.ColliderCentroids);
                state.ColliderCentroids = null;

                Soa_Vector2.Dispose(state.FirstContactPoints);
                state.FirstContactPoints = null;

                Soa_Vector2.Dispose(state.SecondContactPoints);
                state.SecondContactPoints = null;

                state.Depths = null;

                state.TwoContactPoints = null;

                state.ActiveIndices = null;

                state.ActiveIndicesCount = null;

                state.ActivePhase = null;

                state.ContactStates = null;

                state.PreviousContactStates = null;

                state.Stride = 0;

                state.MaxEntries = 0;

                GC.SuppressFinalize(state);
            }
        }

        public static class Detection
        {




            /******************
            
                Util.
            
            *******************/



            /// <summary>
            /// 
            /// </summary>
            /// <param name="collisions"></param>
            /// <param name="vertices"></param>
            /// <param name="centroidsX"></param>
            /// <param name="centroidsY"></param>
            /// <param name="ownerIndex"></param>
            /// <param name="otherIndex"></param>
            /// <param name="collided"></param>
            /// <returns>
            /// <remarks>
            ///     <list type = "bullet">
            ///         <item><see cref="IndexPair.AToB"/> = owner to other</item>
            ///         <item><see cref="IndexPair.BToA"/> = other to owner</item>
            ///     </list>
            /// </remarks>
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static IndexPair Polygon_To_Polygon(Manifold.State collisions, FsSoa_Vector2 vertices,
                Span<float> centroidsX, Span<float> centroidsY, int ownerIndex, int otherIndex, ref bool collided
            )
            {
                ref float ownerPosX = ref centroidsX[ownerIndex]; 
                ref float otherPosX = ref centroidsX[otherIndex]; 
                ref float ownerPosY = ref centroidsY[ownerIndex]; 
                ref float otherPosY = ref centroidsY[otherIndex];

                Span<float> ownerVertsX = default;
                Span<float> ownerVertsY = default;
                Span<float> otherVertsX = default;
                Span<float> otherVertsY = default;

                // gather polygon a vertices.
                PhysicsBody.GetPolygonVerticesUnsafe(vertices, ownerIndex, ref ownerVertsX, ref ownerVertsY);
                PhysicsBody.GetPolygonVerticesUnsafe(vertices, otherIndex, ref otherVertsX, ref otherVertsY);

                // narrow phase SAT intersect check.
                if(SAT.PolygonsIntersect(ownerVertsX, ownerVertsY, otherVertsX, otherVertsY, ownerPosX, ownerPosY, 
                    otherPosX, otherPosY, out float normalX, out float normalY, out float depth
                ))
                {
                    SAT.FindContactPoints(ownerVertsX, ownerVertsY, otherVertsX, otherVertsY, SAT.PolygonContactPointEpsilon, 
                        out float firstContactPointX, out float firstContactPointY, out float secondContactPointX, out float secondContactPointY, 
                        out int contactCount
                    );

                    collided = true;

                    switch (contactCount)
                    {
                        case 1:
                            return Manifold.SetDataTwoWay(collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                                normalX, normalY, firstContactPointX, firstContactPointY, depth
                            );
                        case 2:
                            return Manifold.SetDataTwoWay(collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                                normalX, normalY, firstContactPointX, firstContactPointY, secondContactPointX, secondContactPointY, depth
                            );
                    }

                }
                
                collided = false;
                return default;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collisions"></param>
            /// <param name="vertices"></param>
            /// <param name="centroidsX"></param>
            /// <param name="centroidsY"></param>
            /// <param name="radii"></param>
            /// <param name="polyIndex"></param>
            /// <param name="circIndex"></param>
            /// <param name="collided"></param>
            /// <returns>
            /// <remarks>
            ///     <list type = "bullet">
            ///         <item><see cref="IndexPair.AToB"/> = poly to circle</item>
            ///         <item><see cref="IndexPair.BToA"/> = circle to poly</item>
            ///     </list>
            /// </remarks>
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static IndexPair Polygon_To_Circle(Manifold.State collisions, FsSoa_Vector2 vertices, 
                Span<float> centroidsX, Span<float> centroidsY, Span<float> radii, int polyIndex, int circIndex, ref bool collided
            )
            {
                ref float polyPosX = ref centroidsX[polyIndex]; 
                ref float circPosX = ref centroidsX[circIndex]; 
                ref float polyPosY = ref centroidsY[polyIndex]; 
                ref float circPosY = ref centroidsY[circIndex];

                Span<float> polyVertsX = default;
                Span<float> polyVertsY = default;

                // gather polygon a vertices.
                PhysicsBody.GetPolygonVerticesUnsafe(vertices, polyIndex, ref polyVertsX, ref polyVertsY);

                bool intersect = SAT.PolygonAndCircleIntersect(polyVertsX, polyVertsY, polyPosX, polyPosY, circPosX, circPosY, radii[circIndex], 
                    circPosX, circPosY, out float normalX, out float normalY, out float depth
                );
                // narrow phase intersect check.
                if(intersect)
                {            
                    SAT.FindContactPoints(polyVertsX, polyVertsY, circPosX, circPosY, out float contactPointX, out float contactPointY);
                    
                    collided = true;

                    return Manifold.SetDataTwoWay(collisions, polyIndex, circIndex, polyPosX, polyPosY, circPosX, circPosY, 
                        normalX, normalY, contactPointX, contactPointY, depth
                    );
                }
                else
                {
                    collided = false;
                    return default;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collisions"></param>
            /// <param name="centroidsX"></param>
            /// <param name="centroidsY"></param>
            /// <param name="radii"></param>
            /// <param name="ownerIndex"></param>
            /// <param name="otherIndex"></param>
            /// <param name="collided"></param>
            /// <returns>
            /// <remarks>
            ///     <list type = "bullet">
            ///         <item><see cref="IndexPair.AToB"/> = owner to other</item>
            ///         <item><see cref="IndexPair.BToA"/> = other to owner</item>
            ///     </list>
            /// </remarks>
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static IndexPair Circle_To_Circle(Manifold.State collisions, Span<float> centroidsX, Span<float> centroidsY, 
                Span<float> radii, int ownerIndex, int otherIndex, ref bool collided
            )
            {
                ref float ownerPosX = ref centroidsX[ownerIndex];
                ref float otherPosX = ref centroidsX[otherIndex];
                ref float ownerPosY = ref centroidsY[ownerIndex];
                ref float otherPosY = ref centroidsY[otherIndex];        
                ref float ownerR = ref radii[ownerIndex];
                ref float otherR = ref radii[otherIndex];

                bool intersects = SAT.CirclesIntersect(ownerPosX, ownerPosY, ownerR, otherPosX, otherPosY, otherR, out float normalX, 
                    out float normalY, out float depth
                );
            
                if(intersects)
                {
                    collided = true;

                    // submit the collision with contact points if one of the colliders needs them.
                    SAT.FindContactPoints(ownerPosX, ownerPosY, ownerR, otherPosX, otherPosY, out float contactPointX, out float contactPointY);
                    
                    return Manifold.SetDataTwoWay(collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                        normalX, normalY, contactPointX, contactPointY, depth
                    );
                }   

                collided = false;
                return default;
            }

            public static bool BroadPhase(IntrusiveList.Node[] nodes, float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY,
                int shapeIndexA, int shapeIndexB
            )
            {
                // skip if the two shapes are apart of the same body.
                if(nodes[shapeIndexA].Parent == nodes[shapeIndexB].Parent)
                {
                    return false;
                }

                return Aabb.Intersect(
                    minAabbsX[shapeIndexA], minAabbsX[shapeIndexB], minAabbsY[shapeIndexA], minAabbsY[shapeIndexB], 
                    maxAabbsX[shapeIndexA], maxAabbsX[shapeIndexB], maxAabbsY[shapeIndexA], maxAabbsY[shapeIndexB]
                );
            }




            /******************
            
                Dynamic Polygon RigidBody.
            
            *******************/




            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void DynamicRigidPolygon_To_DynamicRigidPolygon(OverlapInfo info, Span<int> bvhIndices, Manifold.State collisions, IntrusiveList.Node[] nodes, 
                float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY,
                FsSoa_Vector2 vertices, CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );

                        CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }  
                }
            }

            public static void DynamicRigidPolygon_To_DynamicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY, 
                FsSoa_Vector2 vertices, Span<float> radii, CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );

                        CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_DynamicRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_KinematicRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> colliderCollisionsToResolve, 
                CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );

                        CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_KinematicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
                CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );

                        CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_KinematicRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }
            }

            public static void DynamicRigidPolygon_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void DynamicRigidPolygon_To_TriggerRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> subStepCollisionsToResolve 
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, subStepCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_DynamicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> subStepCollisionsToResolve 
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, subStepCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }        
            }

            public static void DynamicRigidPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }    
            }

            public static void DynamicRigidPolygon_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices 
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }   
            }

            public static void DynamicRigidPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }     
            }

            public static void DynamicRigidPolygon_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }




            /******************
            
                Dynamic Circle RigidBody.
            
            *******************/


            public static void DynamicRigidCircle_To_DynamicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, CategorisedOverlapArray<int> colliderCollisionsToResolve, 
                CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );

                        CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }        
            }

            public static void DynamicRigidCircle_To_DynamicRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicRigidCircle_To_KinematicRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
                CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );

                        CategorisedOverlapArray.Append(collisionIndices.BToA, rigidBodyCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidCircle_To_KinematicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );

                        CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }                
            } 

            public static void DynamicRigidCircle_To_KinematicRigidCapsule()
            {
                throw new NotImplementedException();        
            }

            public static void DynamicRigidCircle_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }
            
            public static void DynamicRigidCircle_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }                
            } 

            public static void DynamicRigidCircle_To_TriggerRigidCapsule()
            {
                throw new NotImplementedException();
            } 

            public static void DynamicRigidCircle_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicRigidCircle_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }          
            }

            public static void DynamicRigidCircle_To_DynamicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicRigidCircle_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidCircle_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicRigidCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void DynamicRigidCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void DynamicRigidCircle_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }




            /******************
            
                Kinematic Polygon RigidBody
            
            *******************/




            public static void KinematicRigidPolygon_To_KinematicRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices 
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }           
            }

            public static void KinematicRigidPolygon_To_KinematicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
                }
            }


            public static void KinematicRigidPolygon_To_KinematicRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void KinematicRigidPolygon_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices 
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }
            }
            
            public static void KinematicRigidPolygon_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
                }    
            }
            
            public static void KinematicRigidPolygon_To_TriggerRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void KinematicRigidPolygon_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, 
                CategorisedOverlapArray<int> subStepCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.BToA, subStepCollisionsToResolve, 
                            CollisionResolutionCategory.Kinematic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }  
            }

            public static void KinematicRigidPolygon_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void KinematicRigidPolygon_To_DynamicColliderCapsule()
            {
                throw new NotImplementedException();        
            }

            public static void KinematicRigidPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }          
            } 

            public static void KinematicRigidPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }


            public static void KinematicRigidPolygon_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void KinematicRigidPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void KinematicRigidPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicRigidPolygon_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            } 




            /******************
            
                Kinematic Circle RigidBody.
            
            *******************/




            public static void KinematicRigidCircle_To_KinematicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicRigidCircle_To_KinematicRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void KinematicRigidCircle_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicRigidCircle_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicRigidCircle_To_TriggerRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void KinematicRigidCircle_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void KinematicRigidCircle_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);

                    if (collided)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void KinematicRigidCircle_To_DynamicColliderCapsule()
            {
                throw new NotImplementedException();        
            }

            public static void KinematicRigidCircle_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicRigidCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicRigidCircle_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void KinematicRigidCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicRigidCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicRigidCircle_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }




            /******************
            
                Trigger Polygon Rigidbody.   
            
            *******************/



            public static void TriggerRigidPolygon_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerRigidPolygon_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void TriggerRigidPolygon_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerRigidPolygon_To_DynamicColliderCircle (OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidPolygon_To_DynamicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void TriggerRigidPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerRigidPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidPolygon_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void TriggerRigidPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerRigidPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidPolygon_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }




            /******************
            
                Trigger Circle RigidBody.
            
            *******************/




            public static void TriggerRigidCircle_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerRigidCircle_To_TriggerRigidCapsule()
            {
                throw new NotImplementedException();
            }

            public static void TriggerRigidCircle_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidCircle_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerRigidCircle_To_DynamicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void TriggerRigidCircle_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerRigidCircle_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void TriggerRigidCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerRigidCircle_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }




            /******************
            
                Dynamic Polygon Collider.
            
            *******************/





            public static void DynamicColliderPolygon_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicColliderPolygon_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicColliderPolygon_To_DynamicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicColliderPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicColliderPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicColliderPolygon_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicColliderPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }
            }

            public static void DynamicColliderPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
                }
            }

            public static void DynamicColliderPolygon_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }




            /******************
            
                Dynamic Circle Collider
            
            *******************/




            public static void DynamicColliderCircle_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Dynamic
                        );
                    }
                }        
            }

            public static void DynamicColliderCircle_To_DynamicColliderCapsule()
            {
                throw new NotImplementedException(); 
            }

            public static void DynamicColliderCircle_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    IndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicColliderCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
                CategorisedOverlapArray<int> colliderCollisionsToResolve
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }
                
                    // detect a collision.
                    IndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                            CollisionResolutionCategory.Dynamic,
                            CollisionResolutionCategory.Kinematic
                        );
                    }
                }        
            }

            public static void DynamicColliderCircle_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void DynamicColliderCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void DynamicColliderCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }        
            }

            public static void DynamicColliderCircle_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }

            /******************
            
                Kinematic Polygon Collider
            
            *******************/

            public static void KinematicColliderPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void KinematicColliderPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicColliderPolygon_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void KinematicColliderPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void KinematicColliderPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicColliderPolygon_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }




            /******************
            
                Kinematic Circle Collider.
            
            *******************/




            public static void KinematicColliderCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicColliderCircle_To_KinematicColliderCapsule()
            {
                throw new NotImplementedException();
            }

            public static void KinematicColliderCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicColliderCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }
                
                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicColliderCircle_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }




            /******************
            
                Trigger Polygon Collider.
            
            *******************/




            public static void TriggerColliderPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {            
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                    
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerColliderPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int circIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Polygon_To_Circle(collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerColliderPolygon_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();
            }




            /******************
            
                Trigger Circle Collider.
            
            *******************/




            public static void TriggerColliderCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
                Manifold.State collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
                float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
            )
            {
                bool collided = false;

                for(int i = 0; i < info.Length; i++)
                {
                    // get the owner and other data.
                    int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
                    int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
                
                    if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
                    {
                        continue;
                    }

                    // detect a collision.
                    Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerColliderCircle_To_TriggerColliderCapsule()
            {
                throw new NotImplementedException();        
            }
        }
    }
}