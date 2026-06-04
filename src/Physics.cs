using System.Runtime.CompilerServices;
using Howl.DataStructures;
using Howl.DataStructures.Bvh;
using Howl.Graphics;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Unmanaged.Collections;
using Howl.Unmanaged.Ecs;

namespace Howl;

public static class Physics
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
        [System.Diagnostics.Conditional("DEBUG")]
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
        [System.Diagnostics.Conditional("DEBUG")]
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
        [System.Diagnostics.Conditional("DEBUG")]
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
        [System.Diagnostics.Conditional("DEBUG")]
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




    public struct Soa_Material
    {
        public Array<float> StaticFriction;
        public Array<float> KineticFriction;
        public Array<float> Density;
        public Array<float> Restitution;
        public int Length;
        public bool IsIntialised;

        public static bool Initialise(ref Soa_Material soa, ref Memory.Arena arena, int length)
        {
            if (soa.IsIntialised)
            {
                Debug.Panic("Already Initialised.");
                return false;
            }
            
            Array.Initialise(ref soa.StaticFriction, ref arena, length);
            Array.Initialise(ref soa.KineticFriction, ref arena, length);
            Array.Initialise(ref soa.Density, ref arena, length);
            Array.Initialise(ref soa.Restitution, ref arena, length);

            soa.IsIntialised = true;
            return true;
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
        public static void Insert(ref Array<float> staticFrictions, ref Array<float> kineticFrictions, 
            ref Array<float> densities, ref Array<float> restitutions, float staticFriction, float kineticFriction, 
            float density, float restitution, int insertIndex
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
        public static void Insert(ref Soa_Material soa, int insertIndex, float staticFriction, float kineticFriction, float density, 
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
        public static void Insert(ref Soa_Material soa, Material material, int insertIndex)
        {
            Insert(ref soa.StaticFriction, ref soa.KineticFriction, ref soa.Density, ref soa.Restitution,
                material.StaticFriction, material.KineticFriction, material.Density, material.Restitution, insertIndex
            );  
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Insert(ref Soa_Material soa, float staticFriction, float kineticFriction, float density, float restitution, int index
        )
        {
            Insert(ref soa.StaticFriction, ref soa.KineticFriction, ref soa.Density, ref soa.Restitution,
                staticFriction, kineticFriction, density, restitution, index
            );  
        }
    }




    /******************
    
        State
    
    *******************/




    public struct State
    {




        /*******************
        
            Debug Diagnostic Stopwatches.
        
        ********************/




        public double StepTimeInMs;
        public double SubStepTimeInMs;
        public double BodyMovementStepInMs;
        public double TransformVerticesStepInMs;
        public double BvhConstructionStepInMs;
        public double FindCollisionsStepInMs;
        public double ColliderResolutionStepInMs;
        public double RigidResolutionStepInMs;




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
        public Array<float> AngularVelocities;

        /// <summary>
        ///     The mass values of all rigid bodies and their associated shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<float> Masses;

        /// <summary>
        ///     The inverse mass values of all rigid bodies and their associated shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<float> InverseMasses;

        /// <summary>
        ///     The base width values of all rectangle shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<float> BaseWidths;

        /// <summary>
        ///     The base height values of all rectangle shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<float> BaseHeights;

        /// <summary>
        ///     The base radii values of all circle shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<float> BaseRadii;

        /// <summary>
        ///     The global-space radii values of all circle shapes.
        /// </summary>
        /// <remarks>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<float> GlobalRadii;

        /// <summary>
        ///     The rotational inertia values of all rigidbodies.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<float> RotationalInertia;

        /// <summary>
        ///     The inverse rotational inertia values of all rigidbodies.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<float> InverseRotationalInertia;

        /// <summary>
        ///     The generations of all bodies.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<int> Generations;

        /// <summary>
        ///     The categories of all shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<int> Categories;

        /// <summary>
        ///     The bvh indices of all shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<int> BvhLeafIndices;

        /// <summary>
        ///     The padding of all shapes to apply to their AABB when inserted into the bvh..
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<float> BvhLeafPaddings;

        /// <summary>
        ///     The shape value of all rigid shapes.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<Shape.Rigid.ShapeType> ShapeTypes;

        /// <summary>
        ///     Whether a rigidbody uses rotational response.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<bool> RotationalResponses;

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
        public Array<EntityType> EntityTypes;

        /// <summary>
        ///     Whether or not an entity is active.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<bool> Active;

        /// <summary>
        ///     Whether or not a body is gravity affected.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public Array<bool> GravityAffected;

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
        public Howl.Unmanaged.Collections.StackArray<int> DisplacedThisSubStep;




        /*******************
        
            Utility.
        
        ********************/




        /// <summary>
        ///     The gen-id allocator for all phsyics bodies.
        /// </summary>
        public Howl.Unmanaged.Ecs.GenIdAllocator GenIdAllocator;

        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Roots are the indices of <c>bodies</c>.</para>
        ///    <para>All subsequent children are the indices of <c>shapes</c>.</para>
        ///    <para>Elements are accessed via <c>entityIndex</c>.</para>
        /// </remarks>
        public IntrusiveList BodyHierarchy;

        /// <summary>
        ///     Gets the bounding volume hierarchy for a collision system.
        /// </summary>
        public BoundingVolumeHierarchy Bvh;

        /// <summary>
        ///     The collision manifold.
        /// </summary>
        public Collisions.Manifold CollisionManifold;

        /// <summary>
        ///     Gets and sets the direction of gravity.
        /// </summary>
        public Vector2 GravityDirection;

        /// <summary>
        ///     Gets and sets the gravity force.
        /// </summary>
        public float Gravity;




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




        public bool IsInitialised;


        public static bool Initialise(ref State state, ref Memory.Arena arena, int maxEntities, int verticesPerShape)
        {
            if (state.IsInitialised)
            {
                Debug.Panic("Already Initialised.");
                return false;
            }

            int maxCollisions = maxEntities*maxEntities;

            {   // Entity Data.
                FsSoa_Vector2.Initialise(ref state.BaseVertices, ref arena, verticesPerShape, maxEntities);
                FsSoa_Vector2.Initialise(ref state.GlobalVertices, ref arena, verticesPerShape, maxEntities);
                Soa_Transform.Initialise(ref state.LocalTransforms, ref arena, maxEntities);
                Soa_Transform.Initialise(ref state.GlobalTransforms, ref arena, maxEntities);
                Soa_Vector2.Initialise(ref state.PreviousStepPositions, ref arena, maxEntities);
                Soa_Vector2.Initialise(ref state.Forces, ref arena, maxEntities);
                Soa_Vector2.Initialise(ref state.LinearVelocities, ref arena, maxEntities);
                Soa_Vector2.Initialise(ref state.Centroids, ref arena, maxEntities);
                Soa_Vector2.Initialise(ref state.ShapeCollisionDisplacements, ref arena, maxEntities);
                Soa_Vector2.Initialise(ref state.LocalCentersOfMass, ref arena, maxEntities);
                Soa_Aabb.Initialise(ref state.Aabbs, ref arena, maxEntities);
                Soa_Material.Initialise(ref state.Materials, ref arena, maxEntities);
                Array.Initialise(ref state.AngularVelocities, ref arena, maxEntities);
                Array.Initialise(ref state.Masses, ref arena, maxEntities);
                Array.Initialise(ref state.InverseMasses, ref arena, maxEntities);
                Array.Initialise(ref state.BaseWidths, ref arena, maxEntities);
                Array.Initialise(ref state.BaseHeights, ref arena, maxEntities);
                Array.Initialise(ref state.BaseRadii, ref arena, maxEntities);
                Array.Initialise(ref state.GlobalRadii, ref arena, maxEntities);
                Array.Initialise(ref state.RotationalInertia, ref arena, maxEntities);
                Array.Initialise(ref state.InverseRotationalInertia, ref arena, maxEntities);
                Array.Initialise(ref state.Generations, ref arena, maxEntities);
                Array.Initialise(ref state.Categories, ref arena, maxEntities);
                Array.Initialise(ref state.BvhLeafIndices, ref arena, maxEntities);
                Array.Initialise(ref state.BvhLeafPaddings, ref arena, maxEntities);
                Array.Initialise(ref state.ShapeTypes, ref arena, maxEntities);
                Array.Initialise(ref state.RotationalResponses, ref arena, maxEntities);
                Array.Initialise(ref state.EntityTypes, ref arena, maxEntities);
                Array.Initialise(ref state.Active, ref arena, maxEntities);
                Array.Initialise(ref state.GravityAffected, ref arena, maxEntities);
            }

            {   // Utility.
                
                Howl.Unmanaged.Ecs.GenIdAllocator.Intialise(ref state.GenIdAllocator, ref arena, maxEntities);
                BoundingVolumeHierarchy.Initialise(ref state.Bvh, ref arena, maxEntities);
                CategorisedLeafOverlaps.Initialise(ref state.OverlapsScratchBuffer, ref arena, Shape.Category.Count, maxCollisions);
                Collisions.Manifold.Initialise(ref state.CollisionManifold, ref arena, maxEntities);
                CategorisedOverlapArray.Initialise(ref state.SubStepShapeCollisionsToResolve, ref arena, Collisions.ResolutionCategory.Count, maxCollisions);
                CategorisedOverlapArray.Initialise(ref state.SubStepRigidShapeCollisionsToResolve, ref arena, Collisions.ResolutionCategory.Count, maxCollisions);
                IntrusiveList.Initialise(ref state.BodyHierarchy, ref arena, maxEntities);
            }

            state.GravityDirection = Vector2.Down;
            state.Gravity = 9.81f;

            state.IsInitialised = true;
            return true;
        }
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
    public static GenIdResult SetActive(ref State state, GenId entityId, bool isActive)
    {
        if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
        {
            return GenIdResult.StaleGenId;
        }
        SetActiveUnsafe(ref state, entityId, isActive);
        return GenIdResult.Ok;
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetActiveUnsafe(ref State state, GenId entityId, bool isActive)
    {
        SetActiveUnsafe(ref state, GenId.GetIndex(entityId), isActive);
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetActiveUnsafe(ref State state, int entityIndex, bool isActive)
    {
        state.Active[entityIndex] = isActive; 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsActive(ref State state, GenId entityId, ref GenIdResult resultOutput)
    {
        if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
        {
            resultOutput = GenIdResult.StaleGenId;
            return false;
        }
        resultOutput = GenIdResult.Ok;
        return IsActiveUnsafe(ref state, entityId);
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsActiveUnsafe(ref State state, GenId genId)
    {
        return IsActiveUnsafe(ref state, GenId.GetIndex(genId));
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsActiveUnsafe(ref State state, int entityIndex)
    {
        return state.Active[entityIndex];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIdResult SetLocalTransform(ref State state, GenId genId, Transform transform)
    {
        if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetLocalTransformUnsafe(ref state, genId, transform);

        return GenIdResult.Ok;
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetLocalTransformUnsafe(ref State state, GenId entityId, Transform newTransform)
    {
        SetLocalTransformUnsafe(ref state, GenId.GetIndex(entityId), newTransform);    
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetLocalTransformUnsafe(ref State state, int entityIndex, Transform newTransform)
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
    public static void SetGlobalTransformUnsafe(ref State state, int entityIndex, Transform newTransform)
    {
        state.GlobalTransforms.Positions.X[entityIndex] = newTransform.Position.X;
        state.GlobalTransforms.Positions.Y[entityIndex] = newTransform.Position.Y;
        state.GlobalTransforms.Scales.X[entityIndex] = newTransform.Scale.X;
        state.GlobalTransforms.Scales.Y[entityIndex] = newTransform.Scale.Y;
        state.GlobalTransforms.Cosines[entityIndex] = newTransform.Cosine;
        state.GlobalTransforms.Sines[entityIndex] = newTransform.Sine;                
    }

    public static Vector2 GetLinearVelocity(ref State state, GenId entityId, ref GenIdResult resultOutput)
    {
        if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
        {
            resultOutput = GenIdResult.StaleGenId;
            return default;
        }

        int index = GenId.GetIndex(entityId);
                
        if(IsActiveUnsafe(ref state, index) == false)
        {
            resultOutput = GenIdResult.NotActive;
            return default;
        }

        return GetLinearVelocityUnsafe(ref state, index);
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id will always be returned. 
    /// </remarks>
    public static Vector2 GetLinearVelocityUnsafe(ref State state, GenId entityId)
    {
        return GetLinearVelocityUnsafe(ref state, GenId.GetIndex(entityId));
    }

    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id will always be returned. 
    /// </remarks>
    public static Vector2 GetLinearVelocityUnsafe(ref State state, int entityIndex)
    {
        Soa_Vector2 linearVelocities = state.LinearVelocities;
        return new(linearVelocities.X[entityIndex], linearVelocities.Y[entityIndex]);
    }

    public static void Translate(ref State state, float xDisplacement, float yDisplacement, int entityIndex)
    {            
        ref float tX = ref state.ShapeCollisionDisplacements.X[entityIndex];
        ref float tY = ref state.ShapeCollisionDisplacements.Y[entityIndex];

        if(tX == 0 || tY == 0)
        {
            StackArray.Push(ref state.DisplacedThisSubStep, entityIndex);
        }

        tX += xDisplacement;
        tY += yDisplacement;
    }

    public static void ApplyTranslations(ref State state)
    {
        ref Array<float> dXS = ref state.ShapeCollisionDisplacements.X;
        ref Array<float> dYS = ref state.ShapeCollisionDisplacements.Y;
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




    public static void FixedUpdate(HowlAppState app, ref State state, float deltaTime, int subSteps)
    {
        long startStepTime = Time.GetSystemTick();

        // == hoisting invariance. ==.
        
        ref Array<int> bvhLeafIndices = ref state.BvhLeafIndices;
        ref Collisions.Manifold collisions = ref state.CollisionManifold;
        ref Array<float> collisionNormalsX = ref collisions.Normals.X;
        ref Array<float> collisionNormalsY = ref collisions.Normals.Y;
        ref Array<float> collisionDepths = ref collisions.Depths;
        ref Array<float> collisionFirstContactPointsX = ref collisions.FirstContactPoints.X;
        ref Array<float> collisionFirstContactPointsY = ref collisions.FirstContactPoints.Y;
        ref Array<float> collisionSecondContactPointsX = ref collisions.SecondContactPoints.X;
        ref Array<float> collisionSecondContactPointsY = ref collisions.SecondContactPoints.Y;
        ref Array<bool> collisionTwoContactPoints = ref collisions.TwoContactPoints;
        int collisionsStride = collisions.Stride;
        ref Soa_Vector2 centroids = ref state.Centroids;
        ref FsSoa_Vector2 globalVertices = ref state.GlobalVertices;
        ref CategorisedOverlapArray<int> shapeCollisionsToResolve = ref state.SubStepShapeCollisionsToResolve;
        ref CategorisedOverlapArray<int> rigidShapeCollisionsToResolve = ref state.SubStepRigidShapeCollisionsToResolve;
        ref Array<float> globalRadii = ref state.GlobalRadii;
        ref CategorisedLeafOverlaps overlaps = ref state.OverlapsScratchBuffer;
        ref BoundingVolumeHierarchy bvh = ref state.Bvh;
        ref Array<float> bvhLeafPaddings = ref state.BvhLeafPaddings;
        ref Array<int> categories = ref state.Categories;
        ref FsSoa_Vector2 localVertices = ref state.BaseVertices;
        ref Array<float> baseRadii = ref state.BaseRadii;
        ref Array<float> baseWidths = ref state.BaseWidths;
        ref Array<float> baseHeights = ref state.BaseHeights;
        ref Soa_Transform localTransforms = ref state.LocalTransforms;
        ref Soa_Transform globalTransforms = ref state.GlobalTransforms;
        ref Array<float> globalPositionsX = ref globalTransforms.Positions.X;
        ref Array<float> globalPositionsY = ref globalTransforms.Positions.Y;  
        ref Array<float> globalScalesX = ref globalTransforms.Scales.X;
        ref Array<float> globalScalesY = ref globalTransforms.Scales.Y;
        ref Array<float> globalCosines = ref globalTransforms.Cosines;
        ref Array<float> globalSines = ref globalTransforms.Sines;
        ref Array<float> globalRotationRadians = ref globalTransforms.RotationRadians;
        ref Array<float> masses = ref state.Masses;
        ref Array<float> inverseMasses = ref state.InverseMasses;
        ref Array<float> rotationalInertia = ref state.RotationalInertia;
        ref Array<float> inverseRotationalInertia = ref state.InverseRotationalInertia;
        ref Array<float> previousPositionsX = ref state.PreviousStepPositions.X;
        ref Array<float> previousPositionsY = ref state.PreviousStepPositions.Y;
        ref Array<float> densities = ref state.Materials.Density;
        ref Array<float> staticFrictions = ref state.Materials.StaticFriction;
        ref Array<float> kineticFrictions = ref state.Materials.KineticFriction;
        ref Array<float> restitutions = ref state.Materials.Restitution;
        ref Array<bool> gravityAffected = ref state.GravityAffected;
        ref Array<float> minAabbsX = ref state.Aabbs.MinX;
        ref Array<float> minAabbsY = ref state.Aabbs.MinY;
        ref Array<float> maxAabbsX = ref state.Aabbs.MaxX;
        ref Array<float> maxAabbsY = ref state.Aabbs.MaxY;
        ref Array<float> centroidsX = ref state.Centroids.X;
        ref Array<float> centroidsY = ref state.Centroids.Y;
        ref Array<float> linearVelocitiesX = ref state.LinearVelocities.X;
        ref Array<float> linearVelocitiesY = ref state.LinearVelocities.Y;
        ref Array<float> forcesX = ref state.Forces.X;
        ref Array<float> forcesY = ref state.Forces.Y;
        ref Array<float> angularVelocities = ref state.AngularVelocities;
        ref Array<float> collisionDisplacementsX = ref state.ShapeCollisionDisplacements.X;
        ref Array<float> collisionDisplacementsY = ref state.ShapeCollisionDisplacements.Y;
        ref Array<float> localCentersOfMassX = ref state.LocalCentersOfMass.X;
        ref Array<float> localCentersOfMassY = ref state.LocalCentersOfMass.Y;
        float gravity = state.Gravity;
        float gravityDirectionX = state.GravityDirection.X;
        float gravityDirectionY = state.GravityDirection.Y;
        ref Array<bool> rotationalResponses = ref state.RotationalResponses;
        ref Array<Shape.Rigid.ShapeType> shapes = ref state.ShapeTypes;
        ref SwapBackArray<int> activeBodies = ref state.BodyHierarchy.RootIndices;
        ref Array<IntrusiveList.Node> nodes = ref state.BodyHierarchy.Nodes;

        // scratch buffers for rigid body reslution.
        System.Span<float> impulseMagnitudes = stackalloc float[MaxCollisionContactPoints]; 
        System.Span<float> contactPointsX = stackalloc float[MaxCollisionContactPoints];
        System.Span<float> contactPointsY = stackalloc float[MaxCollisionContactPoints];
        System.Span<float> impulsesX = stackalloc float[MaxCollisionContactPoints];
        System.Span<float> impulsesY = stackalloc float[MaxCollisionContactPoints];
        System.Span<float> distsAX = stackalloc float[MaxCollisionContactPoints];
        System.Span<float> distsAY = stackalloc float[MaxCollisionContactPoints];
        System.Span<float> distsBX = stackalloc float[MaxCollisionContactPoints];
        System.Span<float> distsBY = stackalloc float[MaxCollisionContactPoints];

        // scale delta time by the substeps.
        deltaTime /= (float)subSteps;
        
        {   // Prepare Substep Collisions
            
            Collisions.Manifold.PrepareForNextStep(ref collisions);

            int solidCount = state.DynamicColliderPolygonCount + state.DynamicColliderCircleCount + state.DynamicRigidPolygonCount + state.DynamicRigidCircleCount;
            int kinematicCount = state.KinematicColliderPolygonCount + state.KinematicColliderCircleCount + state.KinematicRigidPolygonCount + state.KinematicRigidCircleCount;

            // prepare sub step collision resolution collection.
            shapeCollisionsToResolve.CategoryLengths[Collisions.ResolutionCategory.Dynamic] = solidCount;
            shapeCollisionsToResolve.CategoryLengths[Collisions.ResolutionCategory.Kinematic] = kinematicCount;
            CategorisedOverlapArray.BuildChunks(ref shapeCollisionsToResolve);

            rigidShapeCollisionsToResolve.CategoryLengths[Collisions.ResolutionCategory.Dynamic] = solidCount;
            rigidShapeCollisionsToResolve.CategoryLengths[Collisions.ResolutionCategory.Kinematic] = kinematicCount;
            CategorisedOverlapArray.BuildChunks(ref rigidShapeCollisionsToResolve);
        }

        {   // Bvh
            
            long startBvhStepTime = Time.GetSystemTick();
            
            CalculateBvhLeafPadding(globalPositionsX, globalPositionsY, previousPositionsX, previousPositionsY, activeBodies, 
                ref bvhLeafPaddings, deltaTime
            );

            // Update Overlap Scratch Buffer Category Length.       
            {                
            
                CategorisedLeafOverlaps.ClearCounts(ref overlaps);
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
            ConstructBvhTree(activeBodies, nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, centroidsX, centroidsY, categories, 
                bvhLeafPaddings, ref bvhLeafIndices, ref bvh
            );

            BoundingVolumeHierarchy.FindOverlaps(bvh.Branches, bvh.Leaves, overlaps);
            FormatCategorisedOverlaps(overlaps, ref bvhLeafIndices, categories);
            
            long endBvhStepTime = Time.GetSystemTick();
            state.BvhConstructionStepInMs = Time.ElapsedMilliseconds(startBvhStepTime, endBvhStepTime);
        }
        
        // note: ordering matters here; keep this below the bvh section always.
        SetPreviousPositions(globalPositionsX, globalPositionsY, ref previousPositionsX, ref previousPositionsY);      

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
            long startSubStepTime = Time.GetSystemTick();

            // clear any grabage collisions that were resolved last sub step.
            CategorisedOverlapArray.ClearCounts(ref shapeCollisionsToResolve);
            CategorisedOverlapArray.ClearCounts(ref rigidShapeCollisionsToResolve);

            // RigidBody Movement Step.
            long startMovementStepTime = Time.GetSystemTick();
            BodyMovementStep(activeBodies, ref nodes, localTransforms, ref globalTransforms, ref linearVelocitiesX, ref linearVelocitiesY, 
                forcesX, forcesY, masses, angularVelocities, ref collisionDisplacementsX, ref collisionDisplacementsY, localCentersOfMassX, 
                localCentersOfMassY, ref globalRotationRadians, categories, gravityAffected, gravityDirectionX, gravityDirectionY, gravity, 
                deltaTime, MovementStepConfig.Full
            );
            long endMovementStepTime = Time.GetSystemTick();
            state.BodyMovementStepInMs = Time.ElapsedMilliseconds(startMovementStepTime, endMovementStepTime);

            // transform physics bodies
            long startTransformVertsTime = Time.GetSystemTick();
            TransformAllShapesVertices(activeBodies, nodes, ref globalVertices, localVertices, shapes, ref globalScalesX, ref globalScalesY, 
                ref globalPositionsX, ref globalPositionsY, ref globalSines, ref globalCosines, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, 
                ref centroidsX, ref centroidsY, baseRadii, ref globalRadii
            );
            long endTransformVertsTime = Time.GetSystemTick();
            state.TransformVerticesStepInMs = Time.ElapsedMilliseconds(startTransformVertsTime, endTransformVertsTime);

            // Find collisions.
            long startFindCollisionsTime = Time.GetSystemTick();
                        
            Collisions.Detection.DynamicRigidPolygon_To_DynamicRigidPolygon(    overlaps_DynRigPol_To_DynRigPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_DynamicRigidCircle(     overlaps_DynRigPol_To_DynRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_KinematicRigidPolygon(overlaps_DynRigPol_To_KinRigPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_KinematicRigidCircle( overlaps_DynRigPol_To_KinRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_TriggerRigidPolygon(  overlaps_DynRigPol_To_TriRigPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.DynamicRigidPolygon_To_TriggerRigidCircle(   overlaps_DynRigPol_To_TriRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.DynamicRigidPolygon_To_DynamicColliderPolygon(     overlaps_DynRigPol_To_DynColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_DynamicColliderCircle(      overlaps_DynRigPol_To_DynColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_KinematicColliderPolygon( overlaps_DynRigPol_To_KinColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_KinematicColliderCircle(  overlaps_DynRigPol_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidPolygon_To_TriggerColliderPolygon(   overlaps_DynRigPol_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.DynamicRigidPolygon_To_TriggerColliderCircle(    overlaps_DynRigPol_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.DynamicRigidCircle_To_DynamicRigidCircle(      overlaps_DynRigCir_To_DynRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_KinematicRigidPolygon( overlaps_DynRigCir_To_KinRigPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_KinematicRigidCircle(  overlaps_DynRigCir_To_KinRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve, rigidShapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_TriggerRigidPolygon(   overlaps_DynRigCir_To_TriRigPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.DynamicRigidCircle_To_TriggerRigidCircle(    overlaps_DynRigCir_To_TriRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.DynamicRigidCircle_To_DynamicColliderPolygon(      overlaps_DynRigCir_To_DynColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_DynamicColliderCircle(       overlaps_DynRigCir_To_DynColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_KinematicColliderPolygon(  overlaps_DynRigCir_To_KinColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_KinematicColliderCircle(   overlaps_DynRigCir_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicRigidCircle_To_TriggerColliderPolygon(    overlaps_DynRigCir_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.DynamicRigidCircle_To_TriggerColliderCircle(     overlaps_DynRigCir_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);

            Collisions.Detection.KinematicRigidPolygon_To_KinematicRigidPolygon(overlaps_KinRigPol_To_KinRigPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);            
            Collisions.Detection.KinematicRigidPolygon_To_KinematicRigidCircle( overlaps_KinRigPol_To_KinRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidPolygon_To_TriggerRigidPolygon(  overlaps_KinRigPol_To_TriRigPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicRigidPolygon_To_TriggerRigidCircle(   overlaps_KinRigPol_To_TriRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidPolygon_To_DynamicColliderPolygon(     overlaps_KinRigPol_To_DynColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.KinematicRigidPolygon_To_DynamicColliderCircle(      overlaps_KinRigPol_To_DynColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.KinematicRigidPolygon_To_KinematicColliderPolygon( overlaps_KinRigPol_To_KinColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicRigidPolygon_To_KinematicColliderCircle(  overlaps_KinRigPol_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidPolygon_To_TriggerColliderPolygon(   overlaps_KinRigPol_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicRigidPolygon_To_TriggerColliderCircle(    overlaps_KinRigPol_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.KinematicRigidCircle_To_KinematicRigidCircle(  overlaps_KinRigCir_To_KinRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_TriggerRigidPolygon(   overlaps_KinRigCir_To_TriRigPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_TriggerRigidCircle(    overlaps_KinRigCir_To_TriRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_DynamicColliderPolygon(      overlaps_KinRigCir_To_DynColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.KinematicRigidCircle_To_DynamicColliderCircle(       overlaps_KinRigCir_To_DynColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.KinematicRigidCircle_To_KinematicColliderPolygon(  overlaps_KinRigCir_To_KinColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_KinematicColliderCircle(   overlaps_KinRigCir_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_TriggerColliderPolygon(    overlaps_KinRigCir_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicRigidCircle_To_TriggerColliderCircle(     overlaps_KinRigCir_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
        
            Collisions.Detection.TriggerRigidPolygon_To_TriggerRigidPolygon(  overlaps_TriRigPol_To_TriRigPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerRigidPolygon_To_TriggerRigidCircle(   overlaps_TriRigPol_To_TriRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidPolygon_To_DynamicColliderPolygon(     overlaps_TriRigPol_To_DynColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerRigidPolygon_To_DynamicColliderCircle(      overlaps_TriRigPol_To_DynColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidPolygon_To_KinematicColliderPolygon( overlaps_TriRigPol_To_KinColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerRigidPolygon_To_KinematicColliderCircle(  overlaps_TriRigPol_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidPolygon_To_TriggerColliderPolygon(   overlaps_TriRigPol_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerRigidPolygon_To_TriggerColliderCircle(    overlaps_TriRigPol_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.TriggerRigidCircle_To_TriggerRigidCircle(    overlaps_TriRigCir_To_TriRigCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_DynamicColliderPolygon(      overlaps_TriRigCir_To_DynColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_DynamicColliderCircle(       overlaps_TriRigCir_To_DynColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_KinematicColliderPolygon(  overlaps_TriRigCir_To_KinColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_KinematicColliderCircle(   overlaps_TriRigCir_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_TriggerColliderPolygon(    overlaps_TriRigCir_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.TriggerRigidCircle_To_TriggerColliderCircle(     overlaps_TriRigCir_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);

            Collisions.Detection.DynamicColliderPolygon_To_DynamicColliderPolygon(     overlaps_DynColPol_To_DynColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderPolygon_To_DynamicColliderCircle(      overlaps_DynColPol_To_DynColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderPolygon_To_KinematicColliderPolygon( overlaps_DynColPol_To_KinColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderPolygon_To_KinematicColliderCircle(  overlaps_DynColPol_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderPolygon_To_TriggerColliderPolygon(   overlaps_DynColPol_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.DynamicColliderPolygon_To_TriggerColliderCircle(    overlaps_DynColPol_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.DynamicColliderCircle_To_DynamicColliderCircle(       overlaps_DynColCir_To_DynColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderCircle_To_KinematicColliderPolygon(  overlaps_DynColCir_To_KinColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderCircle_To_KinematicColliderCircle(   overlaps_DynColCir_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii, shapeCollisionsToResolve);
            Collisions.Detection.DynamicColliderCircle_To_TriggerColliderPolygon(    overlaps_DynColCir_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.DynamicColliderCircle_To_TriggerColliderCircle(     overlaps_DynColCir_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
        
            Collisions.Detection.KinematicColliderPolygon_To_KinematicColliderPolygon( overlaps_KinColPol_To_KinColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicColliderPolygon_To_KinematicColliderCircle(  overlaps_KinColPol_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicColliderPolygon_To_TriggerColliderPolygon(   overlaps_KinColPol_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.KinematicColliderPolygon_To_TriggerColliderCircle(    overlaps_KinColPol_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
        
            Collisions.Detection.KinematicColliderCircle_To_KinematicColliderCircle(  overlaps_KinColCir_To_KinColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);
            Collisions.Detection.KinematicColliderCircle_To_TriggerColliderPolygon(   overlaps_KinColCir_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);
            Collisions.Detection.KinematicColliderCircle_To_TriggerColliderCircle(    overlaps_KinColCir_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);

            Collisions.Detection.TriggerColliderPolygon_To_TriggerColliderPolygon(  overlaps_TriColPol_To_TriColPol, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices);
            Collisions.Detection.TriggerColliderPolygon_To_TriggerColliderCircle(   overlaps_TriColPol_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalVertices, globalRadii);

            Collisions.Detection.TriggerColliderCircle_To_TriggerColliderCircle(overlaps_TriColCir_To_TriColCir, bvhLeafIndices, ref collisions, nodes, centroidsX, centroidsY, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, globalRadii);

            long endFindCollisionsTime = Time.GetSystemTick();
            state.FindCollisionsStepInMs = Time.ElapsedMilliseconds(startFindCollisionsTime, endFindCollisionsTime);

            // Resolve Collider Collisions.
            // NOTE: ordering matters here, make sure to this is above rigidbody collision resolution.
            long startColliderResolutionTime = Time.GetSystemTick();
            ResolveColliderCollisions(nodes, shapeCollisionsToResolve, collisionDepths, collisionNormalsX, collisionNormalsY, 
                ref collisionDisplacementsX, ref collisionDisplacementsY, collisionsStride
            );
            long endColliderResolutionTime = Time.GetSystemTick();
            state.ColliderResolutionStepInMs = Time.ElapsedMilliseconds(startColliderResolutionTime, endColliderResolutionTime);

            // Resolve RigidBody Collisions.
            // NOTE: ordering matters here, make sure this is below collision resolution.
            long startRigidResolutionTime = Time.GetSystemTick();
            ResolveRigidShapeCollisions(rigidShapeCollisionsToResolve, nodes,
                collisionNormalsX, collisionNormalsY, collisionFirstContactPointsX, collisionFirstContactPointsY,
                globalPositionsX, globalPositionsY, localCentersOfMassX, localCentersOfMassY, 
                collisionSecondContactPointsX, collisionSecondContactPointsY, ref linearVelocitiesX, ref linearVelocitiesY, 
                restitutions, kineticFrictions, staticFrictions, ref angularVelocities, masses, inverseMasses, inverseRotationalInertia, 
                collisionTwoContactPoints, rotationalResponses, contactPointsX, contactPointsY, distsAX, distsAY, 
                distsBX, distsBY, impulseMagnitudes, impulsesX, impulsesY, 
                collisionsStride
            );
            long endRigidResolutionTime = Time.GetSystemTick();
            state.RigidResolutionStepInMs = Time.ElapsedMilliseconds(startRigidResolutionTime, endRigidResolutionTime);

            long endSubStepTime = Time.GetSystemTick();
            state.SubStepTimeInMs = Time.ElapsedMilliseconds(startSubStepTime, endSubStepTime);
        }

        Collisions.Manifold.CompleteStep(ref state.CollisionManifold);

        // Transform bodies by collision resolution.
        // NOTE: this is needed at the end as the final
        // sub-step iteration does not transform the bodies
        // at the end of it's loop; meaning the final collision
        // resolution wouldn't be applied.
        TransformAllShapesVertices(activeBodies, nodes, ref globalVertices, localVertices, shapes, ref globalScalesX, ref globalScalesY, 
            ref globalPositionsX, ref globalPositionsY, ref globalSines, ref globalCosines, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, 
            ref centroidsX, ref centroidsY, baseRadii, ref globalRadii
        );

        long endStepTime = Time.GetSystemTick();
        state.StepTimeInMs = Time.ElapsedMilliseconds(startStepTime, endStepTime);
    }

    /// <summary>
    ///     Performs a movement step for all bodies.
    /// </summary>
    /// <remarks>
    ///     Remarks: All provided System.Spans must be indexed by a integer <c>physicsBodyIndex</c>:
    /// </remarks>
    public static void BodyMovementStep(SwapBackArray<int> activeBodies, ref Array<IntrusiveList.Node> nodes, 
        Soa_Transform localTransforms, ref Soa_Transform globalTransforms, ref Array<float> linearVelocitiesX, 
        ref Array<float> linearVelocitiesY, Array<float> forcesX, Array<float> forcesY, Array<float> masses, 
        Array<float> angularVelocities, ref Array<float> collisionDisplacementsX, ref Array<float> collisionDisplacementsY, 
        Array<float> localCentersOfMassX, Array<float> localCentersOfMassY, ref Array<float> rotationRadians, Array<int> categories, 
        Array<bool> gravityAffected, float gravityDirectionX, float gravityDirectionY, float gravity, float deltaTime, 
        MovementStepConfig config
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
                rotationRadians[bodyIndex] = System.MathF.Atan2(bodySine, bodyCosine);

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
    public static void TransformAllShapesVertices(
        SwapBackArray<int> activeBodies, Array<IntrusiveList.Node> nodes, ref FsSoa_Vector2 globalVertices, FsSoa_Vector2 localVertices, 
        Array<Shape.Rigid.ShapeType> shapes, ref Array<float> globalScalesX, ref Array<float> globalScalesY, ref Array<float> globalPositionsX, 
        ref Array<float> globalPositionsY, ref Array<float> globalSines, ref Array<float> globalCosines, Array<float> minAabbsX, 
        Array<float> minAabbsY, Array<float> maxAabbsX, Array<float> maxAabbsY, ref Array<float> centroidsX, ref Array<float> centroidsY, 
        Array<float> localRadii, ref Array<float> globalRadii
    )
    {
        FsSoa_Vector2.ClearAppendCounts(ref globalVertices);
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
                    TransformShapeVertices(ref globalVertices, localVertices, ref globalPositionsX, ref globalPositionsY, ref globalScalesX, 
                        ref globalScalesY, ref globalCosines, ref globalSines, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, localRadii, 
                        ref globalRadii, ref centroidsX, ref centroidsY, shapes, shapeIndex
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
    public static void TransformShapeVertices(
        ref FsSoa_Vector2 globalVertices, FsSoa_Vector2 localVertices, ref Array<float> globalPositionsX, ref Array<float> globalPositionsY, 
        ref Array<float> globalScalesX, ref Array<float> globalScalesY, ref Array<float> globalCosines, ref Array<float> globalSines, 
        Array<float> minAabbsX, Array<float> minAabbsY, Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> localRadii, 
        ref Array<float> globalRadii, ref Array<float> centroidsX, ref Array<float> centroidsY, Array<Shape.Rigid.ShapeType> shapes, 
        int shapeIndex
    )
    {
        Shape.Rigid.ShapeType shapeType = shapes[shapeIndex];
        ref float scaleX = ref globalScalesX[shapeIndex];
        ref float scaleY = ref globalScalesY[shapeIndex];
        System.Span<float> vertsX = default; 
        System.Span<float> vertsY = default;

        int vertexCount = localVertices.AppendCounts[shapeIndex];
        int startIndex = Collections.FixedStrideArray.GetElementIndex(shapeIndex, localVertices.EntryStride, 0);                        
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
            FsSoa_Vector2.Append(ref globalVertices, shapeIndex, x, y);
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
    public static void ConstructBvhTree(SwapBackArray<int> activeBodies, Array<IntrusiveList.Node> nodes, Array<float> minAabbsX, 
        Array<float> minAabbsY, Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> centroidsX, Array<float> centroidsY, 
        Array<int> bvhCategories, Array<float> bvhLeafPaddings, ref Array<int> bvhLeafIndices, ref BoundingVolumeHierarchy bvh
    )
    {
        // clear the previous bvh data.
        BoundingVolumeHierarchy.Clear(ref bvh);

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
                    Soa_Leaf.Append(ref bvh.Leaves, minX, minY, maxX, maxY, centroidsX[shapeIndex], centroidsY[shapeIndex], 
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
        BoundingVolumeHierarchy.ConstructTree(ref bvh);
    }




    /******************
    
        Collider Collision Resolution.
    
    *******************/




    public static void ResolveColliderCollisions(
        Array<IntrusiveList.Node> nodes, CategorisedOverlapArray<int> subStepCollisionsToResolve, Array<float> collisionDepths, 
        Array<float> collisionNormalsX, Array<float> collisionNormalsY, ref Array<float> displacementsX, ref Array<float> displacementsY, 
        int collisionsStride
    )
    {
        // hoisting invariance.
        float depth;
        float displacementX;
        float displacementY;
        int ownerIndex; // always the solid collider.
        int otherIndex; // always the kinematic or other solid collider.

        System.Span<int> collisionsToResolve;

        // == resolve solid to solid collisions ==.
        collisionsToResolve = CategorisedOverlapArray.GetOverlaps(subStepCollisionsToResolve,
            Collisions.ResolutionCategory.Dynamic,
            Collisions.ResolutionCategory.Dynamic
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
            Collisions.ResolutionCategory.Dynamic,
            Collisions.ResolutionCategory.Kinematic
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
    ///     <para><c>NOTE:</c> All System.Spans are scratch buffers and should have a length of <see cref="MaxCollisionContactPoints"/></para>
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
    public static void ResolveRigidShapeCollisions(CategorisedOverlapArray<int> collisionsToResolve, Array<IntrusiveList.Node> nodes,
        Array<float> collisionNormalsX, Array<float> collisionNormalsY, Array<float> firstContactPointsX, Array<float> firstContactPointsY,
        Array<float> globalPositionsX, Array<float> globalPositionsY, Array<float> localCentersOfMassX, Array<float> localCentersOfMassY, 
        Array<float> secondContactPointsX, Array<float> secondContactPointsY, ref Array<float> linearVelocitiesX, 
        ref Array<float> linearVelocitiesY, Array<float> restitutions, Array<float> kineticFrictions, Array<float> staticFrictions, 
        ref Array<float> angularVelocities, Array<float> masses, Array<float> inverseMasses, Array<float> inverseRotationalInertia, 
        Array<bool> twoContactPoints, Array<bool> rotationalResponses, System.Span<float> contactPointsX, System.Span<float> contactPointsY, 
        System.Span<float> distsAX, System.Span<float> distsAY, System.Span<float> distsBX, System.Span<float> distsBY, 
        System.Span<float> impulseMagnitudes, System.Span<float> impulsesX, System.Span<float> impulsesY, int collisionsStride
    )
    {
        System.Span<int> collisions;
        bool otherIsKinematic = false;

        collisions = CategorisedOverlapArray.GetOverlaps(
            collisionsToResolve, Collisions.ResolutionCategory.Dynamic, Collisions.ResolutionCategory.Dynamic
        );

        ResolveRigidBodyCollisions(collisions, nodes, collisionNormalsX, collisionNormalsY, firstContactPointsX, firstContactPointsY, 
            secondContactPointsX, secondContactPointsY, ref linearVelocitiesX, ref linearVelocitiesY, restitutions, kineticFrictions, 
            staticFrictions, ref angularVelocities, masses, inverseMasses, inverseRotationalInertia, localCentersOfMassX, 
            localCentersOfMassY, globalPositionsX, globalPositionsY, twoContactPoints, rotationalResponses, contactPointsX, 
            contactPointsY, distsAX, distsAY, distsBX, distsBY, impulseMagnitudes, impulsesX, impulsesY, collisionsStride, otherIsKinematic
        );

        collisions = CategorisedOverlapArray.GetOverlaps(
            collisionsToResolve, Collisions.ResolutionCategory.Dynamic, Collisions.ResolutionCategory.Kinematic
        );

        otherIsKinematic = true;

        ResolveRigidBodyCollisions(collisions, nodes, collisionNormalsX, collisionNormalsY, firstContactPointsX, firstContactPointsY, 
            secondContactPointsX, secondContactPointsY, ref linearVelocitiesX, ref linearVelocitiesY, restitutions, kineticFrictions, 
            staticFrictions, ref angularVelocities, masses, inverseMasses, inverseRotationalInertia, localCentersOfMassX, 
            localCentersOfMassY, globalPositionsX, globalPositionsY, twoContactPoints, rotationalResponses, contactPointsX, 
            contactPointsY, distsAX, distsAY, distsBX, distsBY, impulseMagnitudes, impulsesX, impulsesY, collisionsStride, otherIsKinematic
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResolveRigidBodyCollisions(
        System.Span<int> collisionsToResolve, Array<IntrusiveList.Node> nodes, Array<float> normalsX, Array<float> normalsY, 
        Array<float> firstContactPointsX, Array<float> firstContactPointsY, Array<float> secondContactPointsX, 
        Array<float> secondContactPointsY, ref Array<float> linearVelocitiesX, ref Array<float> linearVelocitiesY, 
        Array<float> restitutions, Array<float> kineticFrictions, Array<float> staticFrictions, ref Array<float> angularVelocities,
        Array<float> masses, Array<float> inverseMasses, Array<float> inverseRotationalInertia, Array<float> localCentersOfMassX, 
        Array<float> localCentersOfMassY, Array<float> globalPositionsX, Array<float> globalPositionsY, Array<bool> twoContactPoints, 
        Array<bool> rotationalResponses, System.Span<float> contactPointsX, System.Span<float> contactPointsY, System.Span<float> distsAX, 
        System.Span<float> distsAY, System.Span<float> distsBX, System.Span<float> distsBY, System.Span<float> impulseMagnitudes, 
        System.Span<float> impulsesX, System.Span<float> impulsesY, int collisionsStride, bool otherIsKinematic
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
            // this function resuses these stack allocated System.Spans
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
                    ref otherBodyLinVelY, revNormalX, revNormalY, ownerShapeRestitution, otherShapeRestitution,
                    ownerShapeCentroidX, otherShapeCentroidX, ownerShapeCentroidY, otherShapeCentroidY, ownerBodyInvMass, 
                    otherBodyInvMass, ref ownerBodyAngVel, ref otherBodyAngVel, ownerBodyInvRotInertia, 
                    otherBodyInvRotInertia, ownerShapeRotationalResponse, otherShapeRotationalResponse, contactPointsCount, 
                    otherIsKinematic
                );
            }
            else
            {
                ResolveRigidBodyCollision_Basic(impulseMagnitudes, ref ownerBodyLinVelX, 
                    ref otherBodyLinVelX, ref ownerBodyLinVelX, ref otherBodyLinVelY, revNormalX, 
                    revNormalY, ownerShapeRestitution, otherShapeRestitution, ownerBodyInvMass, otherBodyInvMass,
                    ownerBodyMass, otherBodyMass, contactPointsCount, otherIsKinematic
                );
            }

            ResolveRigidBodyFrictionCollision(impulseMagnitudes, contactPointsX, impulsesX, impulsesY, distsAX, distsAY, distsBX, distsBY,
                contactPointsY, ref ownerBodyLinVelX, ref otherBodyLinVelX, ref ownerBodyLinVelY, ref otherBodyLinVelY, 
                revNormalX, revNormalY, ownerShapeStaticFriction, otherShapeStaticFriction, ownerShapeKineticFriction, 
                otherShapeKineticFriction, ownerShapeCentroidX, otherShapeCentroidX, ownerShapeCentroidY, otherShapeCentroidY, 
                ownerBodyInvMass, ownerBodyInvRotInertia, otherBodyInvRotInertia, otherBodyInvMass, 
                ref ownerBodyAngVel, ref otherBodyAngVel, ownerShapeRotationalResponse, otherShapeRotationalResponse, 
                contactPointsCount, otherIsKinematic
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResolveRigidBodyCollision_Basic(System.Span<float> impulseMagnitudes, ref float ownerBodyLinVelX, 
        ref float otherBodyLinVelX, ref float ownerBodyLinVelY, ref float otherBodyLinVelY, float revNormalX, 
        float revNormalY, float ownerShapeRestitution, float otherShapeRestitution, float ownerBodyInvMass, 
        float otherBodyInvMass, float ownerBodyMass, float otherBodyMass, int contactPointsCount, bool otherShapeIsKinematic
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

            float restitution = Math.Math.Min(ownerShapeRestitution, otherShapeRestitution);

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
    public static void ResolveRigidShapeCollision_Rotational(
        System.Span<float> impulseMagnitudes, System.Span<float> contactPointsX, System.Span<float> impulsesX, System.Span<float> impulsesY, 
        System.Span<float> distsAX, System.Span<float> distsAY, System.Span<float> distsBX, System.Span<float> distsBY, 
        System.Span<float> contactPointsY, ref float ownerBodyLinVelX, ref float otherBodyLinVelX, ref float ownerBodyLinVelY, 
        ref float otherBodyLinVelY, float revNormalX, float revNormalY, float ownerShapeRestitution, float otherShapeRestitution, 
        float ownerShapeCentroidX, float otherShapeCentroidX, float ownerShapeCentroidY, float otherShapeCentroidY, float ownerBodyInvMass, 
        float otherBodyInvMass, ref float ownerBodyAngVel, ref float otherBodyAngVel, float ownerBodyInvRotInertia, float otherBodyInvRotInertia, 
        bool ownerRotationalResponse, bool otherRotationalResponse, int contactPointsCount, bool otherShapeIsKinematic
    )
    {
        float restitution = Math.Math.Min(ownerShapeRestitution, otherShapeRestitution);
                
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
    public static void ResolveRigidBodyFrictionCollision(
        System.Span<float> impulseMagnitudes, System.Span<float> contactPointsX, System.Span<float> impulsesX, System.Span<float> impulsesY, 
        System.Span<float> distsAX, System.Span<float> distsAY, System.Span<float> distsBX, System.Span<float> distsBY, 
        System.Span<float> contactPointsY, ref float ownerLinearVelocityX, ref float otherLinearVelocityX, ref float ownerLinearVelocityY, 
        ref float otherLinearVelocityY, float revNormalX, float revNormalY, float ownerStaticFriction, float otherStaticFriction,
        float ownerKineticFriction, float otherKineticFriction, float ownerCentroidX, float otherCentroidX, float ownerCentroidY, 
        float otherCentroidY, float ownerInverseMass, float ownerInverseRotationalInertia, float otherInverseRotationalInertia, 
        float otherInverseMass, ref float ownerAngularVelocity, ref float otherAngularVelocity, bool ownerRotationalResponse, bool otherRotationalResponse, 
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
                frictionImpulseMag = (impulseMagnitudes[j] * kineticFriction) * System.MathF.Sign(frictionImpulseMag);
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
    public static void ClearForcesAndVelocities(ref State state, int bodyIndex)
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
    public static void FormatCategorisedOverlaps(CategorisedLeafOverlaps overlaps, ref Array<int> bvhLeafIndices, Array<int> bvhCategories)
    {
        // hoisting invariance.
        int temp;
        int otherCategory;
        int ownerCategory;

        for(int i = 0; i < Shape.Category.Count; i++)
        {
            for(int j = i; j < Shape.Category.Count; j++)
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
    public static void SetPreviousPositions(
        Array<float> currentPosX, Array<float> currentPosY, 
        ref Array<float> previousPosX, ref Array<float> previousPosY
    )
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

    public static void CalculateBvhLeafPadding(Array<float> currentPositionX, Array<float> currentPositionY, 
        Array<float> previousPositionX, Array<float> previousPositionY, SwapBackArray<int> active, 
        ref Array<float> bvhLeafPadding, float deltaTime
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
        public static GenIdResult Allocate(ref State state, Transform globalTransform, bool gravityAffected, ref GenId entityId)
        {
            GenIdResult result = GenIdAllocator.Allocate(ref state.GenIdAllocator, ref entityId);

            if(result != GenIdResult.Ok)
            {
                return result;
            }

            int bodyIndex = GenId.GetIndex(entityId);
            SetActiveUnsafe(ref state, bodyIndex, true);

            Soa_Transform.CopyTransformToSoa(state.GlobalTransforms, ref globalTransform, bodyIndex);
            
            // clear any garbage data from the previously allocated body.
            state.ShapeCollisionDisplacements.X[bodyIndex] = 0;
            state.ShapeCollisionDisplacements.Y[bodyIndex] = 0;
            state.Masses[bodyIndex] = 0;
            state.InverseMasses[bodyIndex] = 0; 
            state.EntityTypes[bodyIndex] = EntityType.Body;
            state.GravityAffected[bodyIndex] = gravityAffected;
            ClearForcesAndVelocities(ref state, bodyIndex);

            bool inserted = IntrusiveList.AddToTree(ref state.BodyHierarchy, bodyIndex);
            System.Diagnostics.Debug.Assert(inserted, "failed to insert into transform hierarchy.");
            return result;
        }

        public static GenIdResult Deallocate(ref State state, GenId genId)
        {
            if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, genId))
            {
                return GenIdResult.StaleGenId;
            }
            
            int entityIndex = GenId.GetIndex(genId);
            if (state.EntityTypes[entityIndex] != EntityType.Body)
            {
                return GenIdResult.NotAllocated;
            }

            DeallocateUnsafe(ref state, entityIndex);

            return GenIdResult.Ok;
        }

        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>stale id and entity type checks are not enforced; the entity index will always go through the deallocation procedure.</para>
        /// </remarks>
        public static void DeallocateUnsafe(ref State state, int entityIndex)
        {            
            GenIdAllocator.DeallocateUnsafe(ref state.GenIdAllocator, entityIndex);
            
            // deallocate all shapes.
            // note the reverse order and starting deallocation at the last child.
            // this is so first shape is preserved until the end of the loop, ensuring the loop knows when to stop.
            ref Array<IntrusiveList.Node> nodes = ref state.BodyHierarchy.Nodes;
            int lastShapeIndex = nodes[nodes[entityIndex].FirstChild].PreviousSibling;
            if(lastShapeIndex != 0)
            {
                int shapeIndex = lastShapeIndex;
                int previousShapeIndex = 0;
                while (true)
                {
                    if(shapeIndex == previousShapeIndex)
                    {
                        break;
                    }
                    Shape.DeallocateUnsafe(ref state, shapeIndex, false);
                    previousShapeIndex = shapeIndex;
                    shapeIndex = nodes[shapeIndex].PreviousSibling;
                }
            }

            IntrusiveList.RemoveFromTree(ref state.BodyHierarchy, entityIndex);
            state.GravityAffected[entityIndex] = false;
            SetActiveUnsafe(ref state, entityIndex, false);            
        }

        public static GenIdResult SetActive(ref State state, GenId entityId, bool isActive)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Body)
            {
                return GenIdResult.NotAllocated;
            }

            return SetActive(ref state, entityId, isActive);
        }

        public static bool IsActive(ref State state, GenId entityId, ref GenIdResult resultOutput)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Body)
            {
                resultOutput = GenIdResult.NotAllocated;
                return false;
            }

            return IsActive(ref state, entityId, ref resultOutput);
        }

        public static GenIdResult SetLocalTransform(ref State state, GenId entityId, Transform newTransform)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Body)
            {
                return GenIdResult.NotAllocated;
            }

            return SetLocalTransform(ref state, entityId, newTransform);
        }

        public static Vector2 GetLinearVelocity(ref State state, GenId entityId, ref GenIdResult resultOutput)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Body)
            {
                resultOutput = GenIdResult.NotAllocated;
                return default;
            }

            return GetLinearVelocity(ref state, entityId, ref resultOutput);
        }

        public static GenIdResult ImpulseForce(ref State state, Vector2 force, GenId entityId)
        {
            if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
            {
                return GenIdResult.StaleGenId;
            }

            int index = GenId.GetIndex(entityId);
            
            if(IsActiveUnsafe(ref state, index) == false)
            {
                return GenIdResult.NotActive;
            }

            state.LinearVelocities.X[index] += force.X;
            state.LinearVelocities.Y[index] += force.Y;

            return GenIdResult.Ok;
        }

        public static void ClearForcesAndVelocities(ref State state, int entityIndex)
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
        public static void IntegrateShapePropertiesUnsafe(ref State state, int bodyIndex)
        {
            // fallback to the global position if there are not valid rigid shapes associated with the body.
            float centerOfMassX = 0;
            float centerOfMassY = 0;
            float totalMass = 0;
            float totalInverseMass = 0;
            float totalRotationalInertia = 0;
            float totalInverseRotationalInertia = 0;

            ref Array<IntrusiveList.Node> nodes = ref state.BodyHierarchy.Nodes;

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

                    _ => throw new System.Exception()
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

                    _ => throw new System.Exception()
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void IncrementCategoryCounter(ref State state, int category)
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
                    throw new System.Exception();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void DecrementCategoryCounter(ref State state, int category)
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
                    throw new System.Exception();
            }
        }




        /******************
        
            Setters & Getters.
        
        *******************/




        public static GenIdResult SetActive(ref State state, GenId entityId, bool isActive)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Shape)
            {
                return GenIdResult.NotAllocated;
            }
            return SetActive(ref state, entityId, isActive);
        }

        public static bool IsActive(ref State state, GenId entityId, ref GenIdResult resultOutput)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Shape)
            {
                resultOutput = GenIdResult.NotAllocated;
                return false;
            }

            return IsActive(ref state, entityId, ref resultOutput);
        }

        public static GenIdResult SetLocalTransform(ref State state, GenId entityId, Transform newTransform)
        {
            if(state.EntityTypes[GenId.GetIndex(entityId)] != EntityType.Shape)
            {
                return GenIdResult.NotAllocated;
            }
            return SetLocalTransform(ref state, entityId, newTransform);
        }

        /// <summary>
        ///     Gets whether a physics body has collided with other bodies.
        /// </summary>
        /// <param name="state">the state that contacinsthe physics body.</param>
        /// <param name="physicsBodyId">the id of the physics body.</param>
        /// <param name="result">output for the genid result.</param>
        /// <returns>true, if the physics body has collided with another; otherwise false.</returns>
        public static bool HasCollisions(ref State state, GenId physicsBodyId, ref GenIdResult result)
        {
            if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, physicsBodyId))
            {
                result = GenIdResult.StaleGenId;
                return false;
            }

            result = GenIdResult.Ok;
            return Collisions.Manifold.HasContacts(state.CollisionManifold, GenId.GetIndex(physicsBodyId));
        }

        /// <summary>
        ///     Executes a collision callback for a given physics body.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callbackPacket">the data packet to route to the callback function.</param>
        /// <param name="callbacks">the callbacks for all physics bodies in the physics state.</param>
        /// <param name="state">the physics state that contains the physics bodies.</param>
        /// <param name="physicsBodyId">the id of the physics body to execute callbacks for.</param>
        public static unsafe void ExecuteCollisionCallbacks<T>(ref T callbackPacket, Collisions.Callbacks<T> callbacks, State state, GenId physicsBodyId) where T : unmanaged
        {        
            if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, physicsBodyId))
            {
                return;
            }
            
            // hoisting invariance.
            Collisions.Manifold manifold = state.CollisionManifold;
            System.Span<float> normalsX = Array.AsSpan(manifold.Normals.X);
            System.Span<float> normalsY = Array.AsSpan(manifold.Normals.Y);
            System.Span<float> firstContactPointsX = Array.AsSpan(manifold.FirstContactPoints.X);
            System.Span<float> firstContactPointsY = Array.AsSpan(manifold.FirstContactPoints.Y);
            System.Span<float> secondContactPointsX = Array.AsSpan(manifold.SecondContactPoints.X);
            System.Span<float> secondContactPointsY = Array.AsSpan(manifold.SecondContactPoints.Y);
            System.Span<float> depths = Array.AsSpan(manifold.Depths);
            System.Span<bool> twoContactPoints = Array.AsSpan(manifold.TwoContactPoints);
            System.Span<Collisions.ContactState> contactStates = Array.AsSpan(manifold.ContactStates);

            // get the collision indices of the physics body.
            int bodyIndex = GenId.GetIndex(physicsBodyId);
            int start = Collections.FixedStrideArray.GetElementIndex(bodyIndex, manifold.Stride, 0);
            int collisionCount = manifold.ActiveIndicesCount[bodyIndex];
            System.Span<int> collisionIndices = Array.AsSpan(manifold.ActiveIndices, start, collisionCount);

            StackArray<Collisions.Callbacks<T>.Callback> callbackStack;

            // process each collision in each callback.
            for(int i = 0; i < collisionCount; i++)
            {
                // get the next collision to process.
                int collisionIndex = collisionIndices[i];
                
                // get the callbacks to iterate over.
                switch (contactStates[collisionIndex])
                {
                    case Collisions.ContactState.Enter:
                        callbackStack = callbacks.OnEnterCallbacks[bodyIndex];;
                    break;
                    case Collisions.ContactState.Exit:
                        callbackStack = callbacks.OnExitCallbacks[bodyIndex];;
                    break;
                    case Collisions.ContactState.Sustain:
                        callbackStack = callbacks.OnSustainCallbacks[bodyIndex];;
                    break;
                    case Collisions.ContactState.None:
                        continue;
                    default:
                        continue;
                }

                // read the data.
                Collisions.CollisionInfo info = new(ref normalsX[collisionIndex], ref normalsY[collisionIndex], ref firstContactPointsX[collisionIndex], 
                    ref firstContactPointsY[collisionIndex], ref secondContactPointsX[collisionIndex], ref secondContactPointsY[collisionIndex], 
                    ref depths[collisionIndex], ref twoContactPoints[collisionIndex], default, default
                );

                // callback/process data.
                for(int j = 0; j < callbackStack.Count; j++)
                {
                    callbackStack[j].Pointer(callbackPacket, info);
                }
            }
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
            public static ref float GetStaticFriction(ref State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;
                    
                    // return a ref to the nil.
                    return ref state.Materials.StaticFriction[0];
                }

                if(IsRigidBodyUnsafe(ref state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.Materials.StaticFriction[0];            
                }

                resultOutput = GenIdResult.Ok;
                return ref GetStaticFrictionUnsafe(ref state, entityId);
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetStaticFrictionUnsafe(ref State state, GenId entityId)
            {
                return ref GetStaticFrictionUnsafe(ref state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetStaticFrictionUnsafe(ref State state, int body)
            {
                return ref state.Materials.StaticFriction[body];
            }

            /// <summary>
            ///     Gets a reference to the kinetic friction value of a body.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetKineticFriction(ref State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;

                    // return a ref to the nil.
                    return ref state.Materials.KineticFriction[0];
                }

                if(IsRigidBodyUnsafe(ref state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.Materials.KineticFriction[0];            
                }
                
                resultOutput = GenIdResult.Ok;
                return ref GetKineticFrictionUnsafe(ref state, entityId);
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetKineticFrictionUnsafe(ref State state, GenId entityId)
            {
                return ref GetKineticFrictionUnsafe(ref state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetKineticFrictionUnsafe(ref State state, int entityIndex)
            {
                return ref state.Materials.KineticFriction[entityIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetDensity(ref State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;

                    // return a ref to the nil.
                    return ref state.Materials.Density[0];
                }

                if(IsRigidBodyUnsafe(ref state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.Materials.Density[0];            
                }

                resultOutput = GenIdResult.Ok;
                return ref GetDensityUnsafe(ref state, entityId);
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetDensityUnsafe(ref State state, GenId entityId)
            {
                return ref GetDensityUnsafe(ref state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetDensityUnsafe(ref State state, int entityIndex)
            {
                return ref state.Materials.Density[entityIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetRestitution(ref State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;

                    // return a ref to the nil.
                    return ref state.Materials.Restitution[0];
                }

                if(IsRigidBodyUnsafe(ref state, entityId) != true)
                {
                    // return not allocated as only a rigidbody is meant to have this property. 
                    resultOutput = GenIdResult.NotAllocated;

                    // return a ref to the nil.
                    return ref state.Materials.Restitution[0];            
                }

                resultOutput = GenIdResult.Ok;
                return ref GetRestitutionUnsafe(ref state, entityId);
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetRestitutionUnsafe(ref State state, GenId entityId)
            {
                return ref GetRestitutionUnsafe(ref state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static ref float GetRestitutionUnsafe(ref State state, int entityIndex)
            {
                return ref state.Materials.Restitution[entityIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static GenIdResult SetRotationalResponse(ref State state, GenId entityId, bool enabled)
            {
                if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
                {
                    return GenIdResult.StaleGenId;
                }

                SetRotationalResponseUnsafe(ref state, entityId, enabled);
                return GenIdResult.Ok;
            }

            /// <remarks>
            ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetRotationalResponseUnsafe(ref State state, GenId entityId, bool enabled)
            {
                SetRotationalResponseUnsafe(ref state, GenId.GetIndex(entityId), enabled);
            }

            /// <remarks>
            ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetRotationalResponseUnsafe(ref State state, int entityIndex, bool enabled)
            {
                state.RotationalResponses[entityIndex] = enabled;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool UsesRotationalResponse(ref State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;
                    return false;
                }

                resultOutput = GenIdResult.Ok;
                return UsesRotationalResponseUnsafe(ref state, entityId);
            }

            /// <remarks>
            ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool UsesRotationalResponseUnsafe(ref State state, GenId entityId)
            {
                return UsesRotationalResponseUnsafe(ref state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool UsesRotationalResponseUnsafe(ref State state, int body)
            {
                return state.RotationalResponses[body];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static GenIdResult SetRigidBody(ref State state, GenId entityId, bool enabled)
            {
                if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
                {
                    return GenIdResult.StaleGenId;
                }

                SetRigidBodyUnsafe(ref state, GenId.GetIndex(entityId), enabled);
                return GenIdResult.Ok;
            }

            /// <remarks>
            ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void SetRigidBodyUnsafe(ref State state, int entityIndex, bool enabled)
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
            public static bool IsRigidBody(ref State state, GenId entityId, ref GenIdResult resultOutput)
            {
                if(GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, entityId))
                {
                    resultOutput = GenIdResult.StaleGenId;
                    return false;
                }
                
                resultOutput = GenIdResult.Ok;
                return IsRigidBodyUnsafe(ref state, entityId);
            }

            /// <remarks>
            ///     GenId checks are not enforced; the retrieved data at the given gen id slot will always be returned.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsRigidBodyUnsafe(ref State state, GenId entityId)
            {
                return IsRigidBodyUnsafe(ref state, GenId.GetIndex(entityId));
            }

            /// <remarks>
            ///     GenId checks are not enforced; the retrieved data at the given gen id slot will always be returned.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static bool IsRigidBodyUnsafe(ref State state, int entityIndex)
            {        
                return Category.IsRigidBody(state.Categories[entityIndex]);
            }
        }



        /******************
        
            Allocation & Deallocation.
        
        *******************/


        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int SetCategory(ref State state, Rigid.ShapeType shape, Shape.Behaviour behaviour, bool rigidbodyEnabled, 
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
                        _ => throw new System.Exception()
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
                        _ => throw new System.Exception()
                    };
                break;

                default:
                    throw new System.Exception();
            }

            return category;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void PrepareCollisionShapeAllocation(ref State state, Shape.Behaviour colliderBehaviour, int shapeIndex, int bodyIndex, 
            Rigid.ShapeType shape, bool IsRigid
        )
        {
            state.EntityTypes[shapeIndex] = EntityType.Shape;

            // clear any garbage data from previous allocations.
            FsSoa_Vector2.ClearEntryAppendCount(ref state.BaseVertices, shapeIndex);

            // set this so that the previous position isnt garbage from previous steps.
            state.PreviousStepPositions.X[bodyIndex] = state.GlobalTransforms.Positions.X[bodyIndex];
            state.PreviousStepPositions.Y[bodyIndex] = state.GlobalTransforms.Positions.Y[bodyIndex];

            // set the new data.
            SetActiveUnsafe(ref state, shapeIndex, true);
            int category = SetCategory(ref state, shape, colliderBehaviour, IsRigid, shapeIndex);            
            IncrementCategoryCounter(ref state, category);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void FinaliseCollisionShapeAllocation(ref State state, System.Span<float> shapeBaseVertsX,
            System.Span<float> shapeBaseVertsY, Transform transform, int shapeIndex, int bodyIndex, bool IsRigid)
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

            SetLocalTransformUnsafe(ref state, shapeIndex, transform);
            SetGlobalTransformUnsafe(ref state, shapeIndex, globalTransform);

            for(int i = 0; i < shapeBaseVertsX.Length; i++)
            {
                FsSoa_Vector2.Append(ref state.BaseVertices, shapeIndex, shapeBaseVertsX[i], shapeBaseVertsY[i]);
            }

            TransformShapeVertices(ref state.GlobalVertices, state.BaseVertices, ref globalTransforms.Positions.X, 
                ref globalTransforms.Positions.Y, ref globalTransforms.Scales.X, ref globalTransforms.Scales.Y, 
                ref globalTransforms.Cosines, ref globalTransforms.Sines, state.Aabbs.MinX, state.Aabbs.MinY, state.Aabbs.MaxX, 
                state.Aabbs.MaxY, state.BaseRadii, ref state.GlobalRadii, ref state.Centroids.X, ref state.Centroids.Y, 
                state.ShapeTypes, shapeIndex
            );

            IntrusiveList.AddToTree(ref state.BodyHierarchy, shapeIndex, bodyIndex);

            if (IsRigid)
            {
                Body.IntegrateShapePropertiesUnsafe(ref state, bodyIndex);
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
                public static GenIdResult Allocate(ref State state, Math.Shapes.Circle shape, Transform transform, 
                    Shape.Behaviour colliderBehaviour, GenId bodyId, ref GenId colliderId
                )
                {
                    GenIdResult result = GenIdAllocator.Allocate(ref state.GenIdAllocator, ref colliderId); 
                    if(result != GenIdResult.Ok)
                    {
                        return result;
                    }
                    
                    if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, bodyId))
                    {
                        Debug.LogError("cannot allocate collision shape into a stale body", stackDepth: 2);
                        return GenIdResult.StaleGenId;
                    }

                    int shapeIndex = GenId.GetIndex(colliderId);
                    int bodyIndex = GenId.GetIndex(bodyId);

                    PrepareCollisionShapeAllocation(ref state, colliderBehaviour, shapeIndex, bodyIndex, Shape.Rigid.ShapeType.Circle, false);
                    
                    { // Set specific data.

                        // rigidbodies should respond to this like a kinematic rigidbody if it is solid or kinematic. 
                        state.Masses[shapeIndex] = 0 ;
                        state.InverseMasses[shapeIndex] = 0;
                        state.BaseRadii[shapeIndex] = shape.Radius;
                    }

                    FinaliseCollisionShapeAllocation(ref state, [shape.X], [shape.Y], transform, shapeIndex, bodyIndex, 
                        false
                    );
                
                    return GenIdResult.Ok;
                }
            }
            
            public static class Rigid
            {            
                public const float RotationalInertia = 0.5f;
                public static readonly System.Numerics.Vector<float> VectorRotationalInertia = new(RotationalInertia);

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static GenIdResult Allocate(ref State state, Math.Shapes.Circle shape, Transform transform, 
                    Material material, Shape.Behaviour colliderBehaviour, bool rotationalResponse, GenId bodyId, ref GenId genId
                )
                {

                    GenIdResult result = GenIdAllocator.Allocate(ref state.GenIdAllocator, ref genId);
                    if(result != GenIdResult.Ok)
                    {
                        return result;
                    }

                    if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, bodyId))
                    {
                        Debug.LogError("cannot allocate collision shape into a stale body", stackDepth: 2);
                        return GenIdResult.StaleGenId;
                    }

                    int shapeIndex = GenId.GetIndex(genId);
                    int bodyIndex = GenId.GetIndex(bodyId);

                    PrepareCollisionShapeAllocation(ref state, colliderBehaviour, shapeIndex, bodyIndex, Shape.Rigid.ShapeType.Circle, true);

                    {   // Set specific data. 
                        
                        Shape.Rigid.SetRotationalResponseUnsafe(ref state, shapeIndex, rotationalResponse);
                        Soa_Material.Insert(ref state.Materials, material.StaticFriction, material.KineticFriction, 
                            material.Density, material.Restitution, shapeIndex
                        );
                        state.BaseRadii[shapeIndex] = shape.Radius;

                        IntegrateProperties(ref state, transform.Scale.X, transform.Scale.Y, shape.Radius, shapeIndex);
                    }

                    FinaliseCollisionShapeAllocation(ref state, [shape.X], [shape.Y], transform, shapeIndex, bodyIndex, 
                        true
                    );

                    return GenIdResult.Ok;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static float CalculateRotationalInertia(float radius, float mass)
                {
                    return RotationalInertia * mass * (radius * radius);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static System.Numerics.Vector<float> CalculateRotationalInertia(System.Numerics.Vector<float> radius, 
                    System.Numerics.Vector<float> mass
                )
                {
                    return VectorRotationalInertia * mass * (radius * radius);
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

                public static void IntegrateProperties(ref State state, float scaleX, float scaleY, float baseRadii,
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
                public static GenIdResult Allocate(ref State state, Math.Shapes.Rectangle shape, Transform transform, 
                    Shape.Behaviour colliderBehaviour, GenId bodyId, ref GenId genId
                )
                {

                    GenIdResult result = GenIdAllocator.Allocate(ref state.GenIdAllocator, ref genId);
                    if(result != GenIdResult.Ok)
                    {
                        return result;
                    }

                    PolygonRectangle polyRect = new(shape);

                    if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, bodyId))
                    {
                        Debug.LogError("cannot allocate collision shape into a stale body", stackDepth: 2);
                        return GenIdResult.StaleGenId;
                    }

                    int shapeIndex = GenId.GetIndex(genId);
                    int bodyIndex = GenId.GetIndex(bodyId);

                    PrepareCollisionShapeAllocation(ref state, colliderBehaviour, shapeIndex, bodyIndex, Shape.Rigid.ShapeType.Rectangle, false);

                    {   // set specific data.
                        
                        // apply data.                        
                        state.BaseHeights[shapeIndex] = shape.Height;
                        state.BaseWidths[shapeIndex] = shape.Width;

                        // rigidbodies should respond to this like a kinematic rigidbody if it is solid or kinematic. 
                        state.Masses[shapeIndex] = 0;
                        state.InverseMasses[shapeIndex] = 0;
                    }

                    FinaliseCollisionShapeAllocation(ref state, PolygonRectangle.VerticesXAsSpan(polyRect), PolygonRectangle.VerticesYAsSpan(polyRect), 
                        transform, shapeIndex, bodyIndex, false
                    );

                    return GenIdResult.Ok;
                }
            }

            public static class Rigid
            {
                public const float RotationalInertia = 0.0833333333333f;
                public static readonly System.Numerics.Vector<float> VectorRotationalInertia = new(RotationalInertia);

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static GenIdResult Allocate(ref State state, Math.Shapes.Rectangle shape, Transform transform,
                    Material material, Shape.Behaviour colliderBehaviour, bool rotationalResponse, GenId bodyId, ref GenId genId
                )
                {
                    GenIdResult result = GenIdAllocator.Allocate(ref state.GenIdAllocator, ref genId);
                    if(result != GenIdResult.Ok)
                    {
                        return result;
                    }

                    PolygonRectangle polyRect = new(shape);
                    
                    if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, bodyId))
                    {
                        Debug.LogError("cannot allocate collision shape into a stale body", stackDepth: 2);
                        return GenIdResult.StaleGenId;
                    }

                    int shapeIndex = GenId.GetIndex(genId);
                    int bodyIndex = GenId.GetIndex(bodyId);

                    PrepareCollisionShapeAllocation(ref state, colliderBehaviour, shapeIndex, bodyIndex, Shape.Rigid.ShapeType.Rectangle, true);

                    {   // specific data.
                        
                        Shape.Rigid.SetRotationalResponseUnsafe(ref state, shapeIndex, rotationalResponse);
                        state.BaseHeights[shapeIndex] = shape.Height;
                        state.BaseWidths[shapeIndex] = shape.Width;
                        Soa_Material.Insert(ref state.Materials, material.StaticFriction, material.KineticFriction, 
                            material.Density, material.Restitution, shapeIndex
                        );
                        IntegrateProperties(ref state, transform.Scale.X, transform.Scale.Y, shape.Height, shape.Width, shapeIndex);
                    }

                    FinaliseCollisionShapeAllocation(ref state, PolygonRectangle.VerticesXAsSpan(polyRect), PolygonRectangle.VerticesYAsSpan(polyRect), 
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
                    return RotationalInertia * mass * ((width * width) + (height * height));
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
                public static System.Numerics.Vector<float> CalculateRotationalInertia(System.Numerics.Vector<float> width, 
                    System.Numerics.Vector<float> height, System.Numerics.Vector<float> mass
                )
                {
                    return VectorRotationalInertia * mass * ((width * width) + (height * height));
                }

                public static void IntegrateProperties(ref State state, float scaleX, float scaleY, float baseHeight, float baseWidth,
                    int shapeIndex
                )
                {
                    ref Array<IntrusiveList.Node> nodes = ref state.BodyHierarchy.Nodes;
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

        public static void GetVerticesUnsafe(FsSoa_Vector2 vertices, int bodyIndex, ref System.Span<float> xOutput, ref System.Span<float> yOutput)
        {
            int startIndex = Collections.FixedStrideArray.GetElementIndex(bodyIndex, vertices.EntryStride, 0);
            int appendCount = vertices.AppendCounts[bodyIndex];
            xOutput = Array.AsSpan(vertices.X, startIndex, appendCount);
            yOutput = Array.AsSpan(vertices.Y, startIndex, appendCount);
        }

        public static GenIdResult Deallocate(ref State state, GenId genId, bool recalculateBodyCenterOfMass)
        {
            if (GenIdAllocator.IsGenIdStale(ref state.GenIdAllocator, genId))
            {
                return GenIdResult.StaleGenId;
            }

            int entityIndex = GenId.GetIndex(genId);

            if(state.EntityTypes[entityIndex] != EntityType.Shape)
            {
                System.Diagnostics.Debug.Assert(false);
                return GenIdResult.NotAllocated;                
            }

            DeallocateUnsafe(ref state, entityIndex, recalculateBodyCenterOfMass);

            return GenIdResult.Ok;
        }

        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>stale id and entity type checks are not enforced; the entity index will always go through the deallocation procedure.</para>
        /// </remarks>
        public static void DeallocateUnsafe(ref State state, int entityIndex, bool recalculateBodyCenterOfMass = true)
        {   
            GenIdAllocator.DeallocateUnsafe(ref state.GenIdAllocator, entityIndex);         
            DecrementCategoryCounter(ref state, state.Categories[entityIndex]);
            SetActiveUnsafe(ref state, entityIndex, false);
            int bodyIndex = state.BodyHierarchy.Nodes[entityIndex].Parent;
            IntrusiveList.RemoveFromTree(ref state.BodyHierarchy, entityIndex);
            SetActiveUnsafe(ref state, entityIndex, false);
            
            if (recalculateBodyCenterOfMass)
            {
                Body.IntegrateShapePropertiesUnsafe(ref state, bodyIndex);
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
            DrawShapes(howl, state.CollisionManifold, state.GlobalVertices, state.BodyHierarchy.RootIndices, state.BodyHierarchy.Nodes, 
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
            DrawCollisionInformation(howl, state.CollisionManifold);
        }

        DrawCentersOfMass(howl, state.BodyHierarchy.RootIndices, state.GlobalTransforms.Positions.X, state.GlobalTransforms.Positions.Y,
            state.GlobalTransforms.Sines, state.GlobalTransforms.Cosines, state.LocalCentersOfMass.X, state.LocalCentersOfMass.Y
        );
    }

    public static void DrawGlobalPositions(HowlAppState howl, SwapBackArray<int> activeBodies, Array<IntrusiveList.Node> nodes, 
        Array<float> globalPositionsX, Array<float> globalPositionsY
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
        Array<float> globalPositionsX, Array<float> globalPositionsY, Array<float> globalSines, Array<float> globalCosines, 
        Array<float> localCentersOfMassX, Array<float> localCentersOfMassY
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

    public static void DrawShapes(HowlAppState howl, Collisions.Manifold collisions, FsSoa_Vector2 vertices,
        SwapBackArray<int> activeBodies, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> radii, 
        Array<int> categories
    )
    {
        Colour colour = default;

        System.Span<float> polyVertsX = stackalloc float[vertices.EntryStride];
        System.Span<float> polyVertsY = stackalloc float[vertices.EntryStride];

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

    public static void DrawCentroids(HowlAppState app, Soa_Vector2 centroids, SwapBackArray<int> activeBodies, Array<IntrusiveList.Node> nodes)
    {
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
                Debug.DrawWireCircle(app, new Circle(centroids.X[shapeIndex], centroids.Y[shapeIndex], 0.1f), CentroidColour, DrawSpace.World);
                
                shapeIndex = nodes[shapeIndex].NextSibling;
                if(shapeIndex == firstShapeIndex)
                {
                    break;
                }
            }
        }
    }

    public static void DrawLinearVelocities(HowlAppState app, SwapBackArray<int> activeBodies,
        Soa_Vector2 linearVelocities, Array<float> globalPositionsX, Array<float> globalPositionsY
    )
    {
        int count = activeBodies.Count;
        for(int i = 1; i < count; i++) // start at one to skip Nil.
        {
            int bodyIndex = activeBodies[i];

            float startX = globalPositionsX[bodyIndex];
            float startY = globalPositionsY[bodyIndex];
            float endX = startX + linearVelocities.X[bodyIndex];
            float endY = startY + linearVelocities.Y[bodyIndex];

            Debug.DrawLine(app, LinearVelocityColour, new Vector2(startX, startY), new Vector2(endX, endY), DrawSpace.World);
        }
    }

    public static void DrawAabbs(HowlAppState app, SwapBackArray<int> activeBodies, Array<IntrusiveList.Node> nodes, Array<float> aabbsMinX, 
        Array<float> aabbsMinY, Array<float> aabbsMaxX, Array<float> aabbsMaxY
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

    public static void DrawCollisionInformation(HowlAppState app, Collisions.Manifold collisions)
    {
        // hoisitng invariance.
        System.Span<float> firstContactPointsX = Array.AsSpan(collisions.FirstContactPoints.X);
        System.Span<float> firstContactPointsY = Array.AsSpan(collisions.FirstContactPoints.Y);
        System.Span<float> secondContactPointsX = Array.AsSpan(collisions.SecondContactPoints.X);
        System.Span<float> secondContactPointsY = Array.AsSpan(collisions.SecondContactPoints.Y);
        System.Span<float> normalsX = Array.AsSpan(collisions.Normals.X);
        System.Span<float> normalsY = Array.AsSpan(collisions.Normals.Y);
        System.Span<float> otherCentroidsX = Array.AsSpan(collisions.ColliderCentroids.X);
        System.Span<float> otherCentroidsY = Array.AsSpan(collisions.ColliderCentroids.Y);
        System.Span<bool> twoContactPoints = Array.AsSpan(collisions.TwoContactPoints);


        float contactPointX;
        float contactPointY;
        float normalX;
        float normalY;
        float otherCentroidX;
        float otherCentroidY;
        
        Vector2 normalStart;
        Vector2 normalEnd;

        Array<int> active = collisions.ActiveIndices;
        Array<int> activeCounts = collisions.ActiveIndicesCount;

        for(int i = 0; i < activeCounts.Length; i++)
        {
            int count = activeCounts[i];
            if(count<= 0)
            {
                continue;
            }
            int entryElementIndex = Collections.FixedStrideArray.GetElementIndex(i, collisions.Stride, 0);
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
        public static class Constants
        {
            /// <summary>
            ///     The maximum amount of colliders 
            /// </summary>
            /// <remarks>
            ///     Remarks: This is because colliders are stored in a one dimensional array, meaning anything higher than 46340 * 46340 will cause an integer overflow.
            /// </remarks>
            public const int MaxColliders = 46340;

            /// <summary>
            ///     The dense index of a physics body if it is inactive.
            /// </summary>
            public const int InactiveDenseIndex = 0;
        }

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

        public unsafe struct Callbacks<T> where T : allows ref struct
        {
            public struct Callback
            {
                public delegate* <T, CollisionInfo, void> Pointer;
            }

            public Array<StackArray<Callback>> OnEnterCallbacks;
            public Array<StackArray<Callback>> OnSustainCallbacks;
            public Array<StackArray<Callback>> OnExitCallbacks;

            public bool IsInitialised;
        }

        public static class Callbacks
        {
            public static bool Initialise<T>(ref Callbacks<T> callbacks, ref Memory.Arena arena, int maxPhysicsEntities, int maxCallbacks)
            {

                if (callbacks.IsInitialised)
                {
                    Debug.Panic("Already Initialised.");
                    return false;
                }

                Array.Initialise(ref callbacks.OnEnterCallbacks, ref arena, maxPhysicsEntities);
                for(int i = 0; i < maxPhysicsEntities; i++)
                {
                    StackArray.Initialise(ref callbacks.OnEnterCallbacks[i], ref arena, maxCallbacks);
                }

                Array.Initialise(ref callbacks.OnExitCallbacks, ref arena, maxPhysicsEntities);
                for(int i = 0; i < maxPhysicsEntities; i++)
                {
                    StackArray.Initialise(ref callbacks.OnExitCallbacks[i], ref arena, maxCallbacks);
                }

                Array.Initialise(ref callbacks.OnSustainCallbacks, ref arena, maxPhysicsEntities);
                for(int i = 0; i < maxPhysicsEntities; i++)
                {
                    StackArray.Initialise(ref callbacks.OnSustainCallbacks[i], ref arena, maxCallbacks);
                }

                callbacks.IsInitialised = true;
                return true;
            }

            /// <summary>
            ///     Pushes a callback onto the <c>OnEnter</c> callback stack at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection to push into.</param>
            /// <param name="callback">the call back to push.</param>
            /// <param name="index">the index of the callback stack to push onto.</param>
            public static void PushOnEnterCallback<T>(ref Callbacks<T> callbacks, Callbacks<T>.Callback callback, int index) where T : unmanaged
            {
                StackArray.Push(ref callbacks.OnEnterCallbacks[index], callback);
            }

            /// <summary>
            ///     Clears a stack of <c>OnEnter</c> callbacks stored at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection that contains the stack to clear.</param>
            /// <param name="index">the index of the stack to clear.</param>
            public static void ClearOnEnterCallbacks<T>(ref Callbacks<T> collisionCallbacks, int index)
            {
                StackArray.Clear(ref collisionCallbacks.OnEnterCallbacks[index]);
            }

            /// <summary>
            ///     Pushes a callback onto the <c>OnSustain</c> callback stack at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection to push into.</param>
            /// <param name="callback">the call back to push.</param>
            /// <param name="index">the index of the callback stack to push onto.</param>
            public static void PushOnSustainCallback<T>(ref Callbacks<T> callbacks, Callbacks<T>.Callback callback, int index)
            {
                StackArray.Push(ref callbacks.OnSustainCallbacks[index], callback);
            }

            /// <summary>
            ///     Clears a stack of <c>OnSustain</c> callbacks stored at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection that contains the stack to clear.</param>
            /// <param name="index">the index of the stack to clear.</param>
            public static void ClearOnSustainCallbacks<T>(Callbacks<T> callbacks, int index)
            {
                StackArray.Clear(ref callbacks.OnSustainCallbacks[index]);
            }

            /// <summary>
            ///     Pushes a callback onto the <c>OnExit</c> callback stack at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection to push into.</param>
            /// <param name="callback">the call back to push.</param>
            /// <param name="index">the index of the callback stack to push onto.</param>
            public static void PushOnExitCallback<T>(ref Callbacks<T> callbacks, Callbacks<T>.Callback callback, int index)
            {
                StackArray.Push(ref callbacks.OnExitCallbacks[index], callback);
            }

            /// <summary>
            ///     Clears a stack of <c>OnExit</c> callbacks stored at a given index.
            /// </summary>
            /// <param name="callbacks">the callback collection that contains the stack to clear.</param>
            /// <param name="index">the index of the stack to clear.</param>
            public static void ClearOnExitCallbacks<T>(ref Callbacks<T> callbacks, int index)
            {
                StackArray.Clear(ref callbacks.OnExitCallbacks[index]);
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

        public struct Manifold
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
            public Array<float> Depths;

            /// <summary>
            ///     Whether or not a collision has a second contact point.
            /// </summary>
            public Array<bool> TwoContactPoints;

            /// <summary>
            ///     The indices of <c>active</c> collision elements separated by <c>entry</c> in the current step.
            /// </summary>
            /// <remarks>
            ///     Remarks: this array is a fixed-stride swapback array.
            /// </remarks>
            public Array<int> ActiveIndices;

            /// <summary>
            ///     The count of indices an entry has in the <c>ActiveIndices</c> fixed stride swapwback array.
            /// </summary>
            /// <remarks>
            ///     Remarks: Elements should be accessed via <c>entryIndex</c>.
            /// </remarks>
            public Array<int> ActiveIndicesCount;

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
            public Array<int> ActivePhase;

            /// <summary>
            ///     The state of all collisions this step.
            /// </summary>
            public Array<ContactState> ContactStates;

            /// <summary>
            ///     The state of all collisions in the previous step.
            /// </summary>
            public Array<ContactState> PreviousContactStates;

            /// <summary>
            ///     The fixed stride of each entry.
            /// </summary>
            public int Stride;

            /// <summary>
            ///     The amount of entries this collection can hold.
            /// </summary>
            public int MaxEntries;

            public bool IsInitialised;

            public static bool Initialise(ref Manifold manifold, ref Memory.Arena arena, int totalColliders)
            {
                if (manifold.IsInitialised)
                {
                    Debug.Panic("Already Initialised.");
                    return false;
                }

                Debug.Assert(totalColliders <= Constants.MaxColliders, 
                    $"Collision Manifold total colliders '{totalColliders}' exceeds max collisions colliders  '{Constants.MaxColliders}'"
                );

                Math.Math.Clamp(totalColliders, 0, Constants.MaxColliders);

                manifold.Stride = totalColliders;
                manifold.MaxEntries = totalColliders;
                int dataLength = manifold.Stride * manifold.MaxEntries;

                Soa_Vector2.Initialise(ref manifold.Normals, ref arena, dataLength);
                Soa_Vector2.Initialise(ref manifold.ColliderCentroids, ref arena, dataLength);
                Soa_Vector2.Initialise(ref manifold.FirstContactPoints, ref arena, dataLength);
                Soa_Vector2.Initialise(ref manifold.SecondContactPoints, ref arena, dataLength);
                Array.Initialise(ref manifold.Depths, ref arena, dataLength);
                Array.Initialise(ref manifold.TwoContactPoints, ref arena, dataLength);
                Array.Initialise(ref manifold.ContactStates, ref arena, dataLength);
                Array.Initialise(ref manifold.PreviousContactStates, ref arena, dataLength);
                Array.Initialise(ref manifold.ActivePhase, ref arena, dataLength);
                Array.Initialise(ref manifold.ActiveIndices, ref arena, dataLength);
                Array.Initialise(ref manifold.ActiveIndicesCount, ref arena, totalColliders);

                manifold.IsInitialised = true;
                return true;
            }



            /******************
            
                Setters.
            
            *******************/




            /// <summary>
            ///     Sets a one-way collision data entry at a given index.
            /// </summary>
            /// <param name="manifold">the state instance to set.</param>
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
            public static int SetDataOneWay(ref Manifold manifold, int recipientIndex, int colliderIndex, 
                float colliderCentroidX, float colliderCentroidY, float normalX, float normalY, float contactPointX, float contactPointY, float depth
            )
            {
                int elementIndex = Collections.FixedStrideArray.GetElementIndex(recipientIndex, manifold.Stride, colliderIndex);

                ref int phase = ref manifold.ActivePhase[elementIndex];
                if(phase <= 0)
                {
                    FixedStrideSwapBackArray.Append(ref manifold.ActiveIndices, ref manifold.ActiveIndicesCount, 
                        manifold.Stride, recipientIndex, elementIndex
                    );
                }
                phase = 1;

                // write data.
                manifold.Normals.X[elementIndex]              = normalX;
                manifold.Normals.Y[elementIndex]              = normalY;
                manifold.ColliderCentroids.X[elementIndex]    = colliderCentroidX;
                manifold.ColliderCentroids.Y[elementIndex]    = colliderCentroidY;
                manifold.FirstContactPoints.X[elementIndex]   = contactPointX;
                manifold.FirstContactPoints.Y[elementIndex]   = contactPointY;
                manifold.Depths[elementIndex]                 = depth;
                manifold.TwoContactPoints[elementIndex]       = false;

                return elementIndex;
            }

            /// <summary>
            ///     Sets a one-way collision data entry at a given index.
            /// </summary>
            /// <param name="manifold">the state instance to set.</param>
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
            public static int SetDataOneWay(ref Manifold manifold, int recipientIndex, int colliderIndex, 
                float colliderCentroidX, float colliderCentroidY, float normalX, float normalY, float firstContactPointX, float firstContactPointY, 
                float secondContactPointX, float secondContactPointY, float depth
            )
            {
                int elementIndex = Collections.FixedStrideArray.GetElementIndex(recipientIndex, manifold.Stride, colliderIndex);

                ref int phase = ref manifold.ActivePhase[elementIndex];
                if(phase <= 0)
                {
                    FixedStrideSwapBackArray.Append(ref manifold.ActiveIndices, ref manifold.ActiveIndicesCount, 
                        manifold.Stride, recipientIndex, elementIndex
                    );
                }
                phase = 1;

                manifold.Normals.X[elementIndex]              = normalX;
                manifold.Normals.Y[elementIndex]              = normalY;
                manifold.ColliderCentroids.X[elementIndex]    = colliderCentroidX;
                manifold.ColliderCentroids.Y[elementIndex]    = colliderCentroidY;
                manifold.FirstContactPoints.X[elementIndex]   = firstContactPointX;
                manifold.FirstContactPoints.Y[elementIndex]   = firstContactPointY;
                manifold.SecondContactPoints.X[elementIndex]  = secondContactPointX;
                manifold.SecondContactPoints.Y[elementIndex]  = secondContactPointY;
                manifold.Depths[elementIndex]                 = depth;
                manifold.TwoContactPoints[elementIndex]       = true;

                return elementIndex;
            }

            /// <summary>
            ///     Sets a two-way collision data entry at a given entry.
            /// </summary>
            /// <param name="manifold">the state instance to append to.</param>
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
            public static IndexPair SetDataTwoWay(ref Manifold manifold, int indexA, int indexB, float centroidXA, float centroidYA, 
                float centroidXB, float centroidYB, float normalX, float normalY, float firstContactPointX, float firstContactPointY, 
                float secondContactPointX, float secondContactPointY, float depth
            )
            {
                int a = SetDataOneWay(ref manifold, indexA, indexB, centroidXB, centroidYB, normalX, normalY, firstContactPointX, firstContactPointY, 
                    secondContactPointX, secondContactPointY, depth
                );

                // note: the normal reversing.
                int b = SetDataOneWay(ref manifold, indexB, indexA, centroidXA, centroidYA, -normalX, -normalY, firstContactPointX, firstContactPointY, 
                    secondContactPointX, secondContactPointY, depth
                );

                return new (a,b);  
            }

            /// <summary>
            ///     Sets a two-way collision data entry at a given entry.
            /// </summary>
            /// <param name="manifold">the state instance to append to.</param>
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
            public static IndexPair SetDataTwoWay(ref Manifold manifold, int indexA, int indexB, float centroidXA, float centroidYA, 
                float centroidXB, float centroidYB, float normalX, float normalY, float contactPointX, float contactPointY, 
                float depth
            )
            {
                int a = SetDataOneWay(ref manifold, indexA, indexB, centroidXB, centroidYB, normalX, normalY, contactPointX, contactPointY, depth);

                // note: the normal reversing.
                int b = SetDataOneWay(ref manifold, indexB, indexA, centroidXA, centroidYA, -normalX, -normalY, contactPointX, contactPointY, depth);
                
                return new (a,b);
            }




            /******************
            
                State Handling.
            
            *******************/




            /// <summary>
            ///     Swaps the previous and current contact state context pointers.
            /// </summary>
            /// <param name="state">the state instance to swap.</param>
            public static void SwapContactStateContexts(ref Manifold manifold)
            {
                Array<ContactState> tempContactStates = manifold.PreviousContactStates;
                manifold.PreviousContactStates = manifold.ContactStates;
                manifold.ContactStates = tempContactStates;
            }

            /// <summary>
            ///     Prepares a state instance for the next step.
            /// </summary>
            /// <parsam name="state">the state instance to prepare.</param>
            public static void PrepareForNextStep(ref Manifold manifold)
            {
                SwapContactStateContexts(ref manifold);
            }

            /// <summary>
            ///     Completes the update step for a state instance.
            /// </summary>
            /// <param name="manifold">the state instance to complete the step for.</param>
            public static void CompleteStep(ref Manifold manifold)
            {        
                Array<ContactState> contactStates = manifold.ContactStates;
                Array<ContactState> previousContactStates = manifold.PreviousContactStates;
                ref Array<int> activeIndicesCounts = ref manifold.ActiveIndicesCount;
                ref Array<int> activeIndices = ref manifold.ActiveIndices;
                ref Array<int> active = ref manifold.ActivePhase;
                int stride = manifold.Stride;
                int maxEntries = manifold.MaxEntries;

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

                        int elementIndex = Collections.FixedStrideArray.GetElementIndex(entryIndex, stride, entryElementIndex);;
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
                            FixedStrideSwapBackArray.RemoveAt(ref activeIndices, ref activeIndicesCounts, stride, entryIndex, entryElementIndex);
                        }
                    }
                }
            }

            /// <summary>
            ///     Gets whether a collider is in contact with another.
            /// </summary>
            /// <param name="manifold">the state instance that contains the collider.</param>
            /// <param name="index">the index of the collider in the state instance.</param>
            /// <returns>true, if the collider is in contact with another; otherwise false.</returns>
            public static bool HasContacts(Manifold manifold, int index)
            {
                return manifold.ActiveIndicesCount[index] > 0;
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
            public static IndexPair Polygon_To_Polygon(ref Manifold collisions, FsSoa_Vector2 vertices,
                Array<float> centroidsX, Array<float> centroidsY, int ownerIndex, int otherIndex, ref bool collided
            )
            {
                ref float ownerPosX = ref centroidsX[ownerIndex]; 
                ref float otherPosX = ref centroidsX[otherIndex]; 
                ref float ownerPosY = ref centroidsY[ownerIndex]; 
                ref float otherPosY = ref centroidsY[otherIndex];

                System.Span<float> ownerVertsX = default;
                System.Span<float> ownerVertsY = default;
                System.Span<float> otherVertsX = default;
                System.Span<float> otherVertsY = default;

                // gather polygon a vertices.
                Shape.GetVerticesUnsafe(vertices, ownerIndex, ref ownerVertsX, ref ownerVertsY);
                Shape.GetVerticesUnsafe(vertices, otherIndex, ref otherVertsX, ref otherVertsY);

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
                            return Manifold.SetDataTwoWay(ref collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                                normalX, normalY, firstContactPointX, firstContactPointY, depth
                            );
                        case 2:
                            return Manifold.SetDataTwoWay(ref collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
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
            public static IndexPair Polygon_To_Circle(ref Manifold collisions, FsSoa_Vector2 vertices, 
                Array<float> centroidsX, Array<float> centroidsY, Array<float> radii, int polyIndex, int circIndex, ref bool collided
            )
            {
                ref float polyPosX = ref centroidsX[polyIndex]; 
                ref float circPosX = ref centroidsX[circIndex]; 
                ref float polyPosY = ref centroidsY[polyIndex]; 
                ref float circPosY = ref centroidsY[circIndex];

                System.Span<float> polyVertsX = default;
                System.Span<float> polyVertsY = default;

                // gather polygon a vertices.
                Shape.GetVerticesUnsafe(vertices, polyIndex, ref polyVertsX, ref polyVertsY);

                bool intersect = SAT.PolygonAndCircleIntersect(polyVertsX, polyVertsY, polyPosX, polyPosY, circPosX, circPosY, radii[circIndex], 
                    circPosX, circPosY, out float normalX, out float normalY, out float depth
                );
                // narrow phase intersect check.
                if(intersect)
                {            
                    SAT.FindContactPoints(polyVertsX, polyVertsY, circPosX, circPosY, out float contactPointX, out float contactPointY);
                    
                    collided = true;

                    return Manifold.SetDataTwoWay(ref collisions, polyIndex, circIndex, polyPosX, polyPosY, circPosX, circPosY, 
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
            public static IndexPair Circle_To_Circle(ref Manifold collisions, Array<float> centroidsX, Array<float> centroidsY, 
                Array<float> radii, int ownerIndex, int otherIndex, ref bool collided
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
                    
                    return Manifold.SetDataTwoWay(ref collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                        normalX, normalY, contactPointX, contactPointY, depth
                    );
                }   

                collided = false;
                return default;
            }

            public static bool BroadPhase(Array<IntrusiveList.Node> nodes, Array<float> minAabbsX, Array<float> minAabbsY, Array<float> maxAabbsX, Array<float> maxAabbsY,
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
            public static void DynamicRigidPolygon_To_DynamicRigidPolygon(OverlapInfo info, Array<int> bvhIndices, ref Manifold collisions, Array<IntrusiveList.Node> nodes, 
                Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, Array<float> maxAabbsX, Array<float> maxAabbsY,
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
                    IndexPair collisionIndices = Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );

                        CategorisedOverlapArray.Append(ref rigidBodyCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }  
                }
            }

            public static void DynamicRigidPolygon_To_DynamicRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, Array<float> maxAabbsX, Array<float> maxAabbsY, 
                FsSoa_Vector2 vertices, Array<float> radii, CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );

                        CategorisedOverlapArray.Append(ref rigidBodyCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_DynamicRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_KinematicRigidPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> colliderCollisionsToResolve, 
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
                    IndexPair collisionIndices = Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );

                        CategorisedOverlapArray.Append(ref rigidBodyCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_KinematicRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii,
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );

                        CategorisedOverlapArray.Append(ref rigidBodyCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_KinematicRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_TriggerRigidPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }
            }

            public static void DynamicRigidPolygon_To_TriggerRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void DynamicRigidPolygon_To_TriggerRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_DynamicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> subStepCollisionsToResolve 
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
                    IndexPair collisionIndices = Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref subStepCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_DynamicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii,
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicRigidPolygon_To_DynamicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_KinematicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> subStepCollisionsToResolve 
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
                    IndexPair collisionIndices = Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref subStepCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }        
            }

            public static void DynamicRigidPolygon_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii,
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }    
            }

            public static void DynamicRigidPolygon_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicRigidPolygon_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices 
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }   
            }

            public static void DynamicRigidPolygon_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }     
            }

            public static void DynamicRigidPolygon_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }




            /******************
            
                Dynamic Circle RigidBody.
            
            *******************/


            public static void DynamicRigidCircle_To_DynamicRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii, CategorisedOverlapArray<int> colliderCollisionsToResolve, 
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
                    IndexPair collisionIndices = Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );

                        CategorisedOverlapArray.Append(ref rigidBodyCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }        
            }

            public static void DynamicRigidCircle_To_DynamicRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicRigidCircle_To_KinematicRigidPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii,
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.BToA, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );

                        CategorisedOverlapArray.Append(ref rigidBodyCollisionsToResolve, collisionIndices.BToA, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidCircle_To_KinematicRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii, 
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
                    IndexPair collisionIndices = Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );

                        CategorisedOverlapArray.Append(ref rigidBodyCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }                
            } 

            public static void DynamicRigidCircle_To_KinematicRigidCapsule()
            {
                throw new System.NotImplementedException();        
            }

            public static void DynamicRigidCircle_To_TriggerRigidPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }
            
            public static void DynamicRigidCircle_To_TriggerRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }                
            } 

            public static void DynamicRigidCircle_To_TriggerRigidCapsule()
            {
                throw new System.NotImplementedException();
            } 

            public static void DynamicRigidCircle_To_DynamicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii, 
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicRigidCircle_To_DynamicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii, 
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
                    IndexPair collisionIndices = Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }          
            }

            public static void DynamicRigidCircle_To_DynamicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicRigidCircle_To_KinematicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii, 
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.BToA, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidCircle_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii, 
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
                    IndexPair collisionIndices = Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicRigidCircle_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicRigidCircle_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void DynamicRigidCircle_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void DynamicRigidCircle_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }




            /******************
            
                Kinematic Polygon RigidBody
            
            *******************/




            public static void KinematicRigidPolygon_To_KinematicRigidPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices 
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }           
            }

            public static void KinematicRigidPolygon_To_KinematicRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
                }
            }


            public static void KinematicRigidPolygon_To_KinematicRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void KinematicRigidPolygon_To_TriggerRigidPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices 
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }
            }
            
            public static void KinematicRigidPolygon_To_TriggerRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
                }    
            }
            
            public static void KinematicRigidPolygon_To_TriggerRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void KinematicRigidPolygon_To_DynamicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, 
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
                    IndexPair collisionIndices = Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref subStepCollisionsToResolve, collisionIndices.BToA, 
                            ResolutionCategory.Kinematic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }  
            }

            public static void KinematicRigidPolygon_To_DynamicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii, 
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.BToA, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void KinematicRigidPolygon_To_DynamicColliderCapsule()
            {
                throw new System.NotImplementedException();        
            }

            public static void KinematicRigidPolygon_To_KinematicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }          
            } 

            public static void KinematicRigidPolygon_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }


            public static void KinematicRigidPolygon_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void KinematicRigidPolygon_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void KinematicRigidPolygon_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicRigidPolygon_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            } 




            /******************
            
                Kinematic Circle RigidBody.
            
            *******************/




            public static void KinematicRigidCircle_To_KinematicRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicRigidCircle_To_KinematicRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void KinematicRigidCircle_To_TriggerRigidPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicRigidCircle_To_TriggerRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicRigidCircle_To_TriggerRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void KinematicRigidCircle_To_DynamicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii, 
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void KinematicRigidCircle_To_DynamicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii, 
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
                    IndexPair collisionIndices = Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);

                    if (collided)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.BToA, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void KinematicRigidCircle_To_DynamicColliderCapsule()
            {
                throw new System.NotImplementedException();        
            }

            public static void KinematicRigidCircle_To_KinematicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicRigidCircle_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicRigidCircle_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void KinematicRigidCircle_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicRigidCircle_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicRigidCircle_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }




            /******************
            
                Trigger Polygon Rigidbody.   
            
            *******************/



            public static void TriggerRigidPolygon_To_TriggerRigidPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerRigidPolygon_To_TriggerRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void TriggerRigidPolygon_To_DynamicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerRigidPolygon_To_DynamicColliderCircle (OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidPolygon_To_DynamicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void TriggerRigidPolygon_To_KinematicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerRigidPolygon_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidPolygon_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void TriggerRigidPolygon_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerRigidPolygon_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidPolygon_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }




            /******************
            
                Trigger Circle RigidBody.
            
            *******************/




            public static void TriggerRigidCircle_To_TriggerRigidCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerRigidCircle_To_TriggerRigidCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void TriggerRigidCircle_To_DynamicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidCircle_To_DynamicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerRigidCircle_To_DynamicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void TriggerRigidCircle_To_KinematicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidCircle_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerRigidCircle_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void TriggerRigidCircle_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerRigidCircle_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerRigidCircle_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }




            /******************
            
                Dynamic Polygon Collider.
            
            *******************/





            public static void DynamicColliderPolygon_To_DynamicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, 
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
                    IndexPair collisionIndices = Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicColliderPolygon_To_DynamicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii,
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }
            }

            public static void DynamicColliderPolygon_To_DynamicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicColliderPolygon_To_KinematicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, 
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
                    IndexPair collisionIndices = Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicColliderPolygon_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii,
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicColliderPolygon_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicColliderPolygon_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }
            }

            public static void DynamicColliderPolygon_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
                }
            }

            public static void DynamicColliderPolygon_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }




            /******************
            
                Dynamic Circle Collider
            
            *******************/




            public static void DynamicColliderCircle_To_DynamicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii, 
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
                    IndexPair collisionIndices = Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Dynamic
                        );
                    }
                }        
            }

            public static void DynamicColliderCircle_To_DynamicColliderCapsule()
            {
                throw new System.NotImplementedException(); 
            }

            public static void DynamicColliderCircle_To_KinematicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii,
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
                    IndexPair collisionIndices = Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.BToA, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }
            }

            public static void DynamicColliderCircle_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii, 
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
                    IndexPair collisionIndices = Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
            
                    // resolve the collision.
                    if (collided == true)
                    {
                        CategorisedOverlapArray.Append(ref colliderCollisionsToResolve, collisionIndices.AToB, 
                            ResolutionCategory.Dynamic,
                            ResolutionCategory.Kinematic
                        );
                    }
                }        
            }

            public static void DynamicColliderCircle_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void DynamicColliderCircle_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void DynamicColliderCircle_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }        
            }

            public static void DynamicColliderCircle_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            /******************
            
                Kinematic Polygon Collider
            
            *******************/

            public static void KinematicColliderPolygon_To_KinematicColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void KinematicColliderPolygon_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicColliderPolygon_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void KinematicColliderPolygon_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void KinematicColliderPolygon_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicColliderPolygon_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }




            /******************
            
                Kinematic Circle Collider.
            
            *******************/




            public static void KinematicColliderCircle_To_KinematicColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicColliderCircle_To_KinematicColliderCapsule()
            {
                throw new System.NotImplementedException();
            }

            public static void KinematicColliderCircle_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void KinematicColliderCircle_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void KinematicColliderCircle_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }




            /******************
            
                Trigger Polygon Collider.
            
            *******************/




            public static void TriggerColliderPolygon_To_TriggerColliderPolygon(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices
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
                    Polygon_To_Polygon(ref collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
                }        
            }

            public static void TriggerColliderPolygon_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, FsSoa_Vector2 vertices, Array<float> radii
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
                    Polygon_To_Circle(ref collisions, vertices, 
                        centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
                    );    
                }
            }

            public static void TriggerColliderPolygon_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }




            /******************
            
                Trigger Circle Collider.
            
            *******************/




            public static void TriggerColliderCircle_To_TriggerColliderCircle(OverlapInfo info, Array<int> bvhIndices, 
                ref Manifold collisions, Array<IntrusiveList.Node> nodes, Array<float> centroidsX, Array<float> centroidsY, Array<float> minAabbsX, Array<float> minAabbsY, 
                Array<float> maxAabbsX, Array<float> maxAabbsY, Array<float> radii
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
                    Circle_To_Circle(ref collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
                }
            }

            public static void TriggerColliderCircle_To_TriggerColliderCapsule()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}