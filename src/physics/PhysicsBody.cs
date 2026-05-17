using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Howl.Collections;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public static class PhysicsBody
{




    /// <summary>
    ///     The shape a Physics Body can assume.
    /// </summary>
    public enum Shape : int
    {
        Circle,
        Rectangle
    }





    /// <summary>
    ///         Note: ordering matters here, value 0 is highest precedence in 
    ///         <see cref="PhysicsSystem.FormatCategorisedOverlaps(Howl.CategorisedLeafOverlaps, System.Span{int}, System.Span{int})"/>.
    /// </summary>
    public static class Category
    {

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





    /*******************
    
        Physics Body Setters & Getters.
    
    ********************/




    /// <summary>
    ///     Adds or removes the physics body from the physics simulation.
    /// </summary>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="isActive">whether or not to set the physics body to <c>Active</c></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIdResult SetActive(PhysicsSystemState state, GenId genId, bool isActive)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }
        SetActiveUnsafe(state, genId, isActive);
        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Adds or removes the physics body from the physics simulation.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="isActive">whether or not to set the physics body to <c>Active</c></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetActiveUnsafe(PhysicsSystemState state, GenId genId, bool isActive)
    {
        SetActiveUnsafe(state, GetPhysicsBodyIndex(genId), isActive);
    }

    /// <summary>
    ///     Adds or removes the physics body from the physics simulation.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <param name="isActive">whether or not to set the physics body to <c>Active</c></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetActiveUnsafe(PhysicsSystemState state, int physicsBodyIndex, bool isActive)
    {
        switch (isActive)
        {
            case true:
                state.ActiveBodiesDenseIndices[physicsBodyIndex] = SwapBackArray.Append(state.ActiveBodies, physicsBodyIndex);
            break;
            case false:
                ref int denseIndex = ref state.ActiveBodiesDenseIndices[physicsBodyIndex];
                if(denseIndex != 0) // only set inactive if the body is active.
                {
                    // manual swap back on dense indices.
                    state.ActiveBodiesDenseIndices[state.ActiveBodies.Count-1] = denseIndex;
                    state.ActiveBodiesDenseIndices[physicsBodyIndex] = 0;
                    
                    // perform swap back.
                    SwapBackArray.RemoveAt(state.ActiveBodies, physicsBodyIndex);
                }
            break;
        }
    }

    /// <summary>
    ///     Gets whether or not the physics body is currently being processed by the physics simulation.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     <c>true</c>, if the body is <c>Active</c>; otherwise <c>false</c>. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned value; as <c>false</c> will be returned when <c><paramref name="result"/></c> is not <c><see cref="GenIdResult.Ok"/></c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsActive(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if (EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;
            return false;
        }
        result = GenIdResult.Ok;
        return IsActiveUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets whether or not the physics body is currently being processed by the physics simulation.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>true, if the physics body is active; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsActiveUnsafe(PhysicsSystemState state, GenId genId)
    {
        return IsActiveUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets whether or not the physics body is currently being processed by the physics simulation.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <returns>true, if the physics body is active; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsActiveUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return state.ActiveBodiesDenseIndices[physicsBodyIndex] != 0;
    }

    /// <summary>
    ///     Sets the transform of a body in the physics simulation.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="transform">the new transform data for the physics body.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIdResult SetTransform(PhysicsSystemState state, GenId genId, Transform transform)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetTransformUnsafe(state, genId, transform);

        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Sets the transform of a body in the physics simulation.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="transform">the new transform data for the physics body.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetTransformUnsafe(PhysicsSystemState state, GenId genId, Transform transform)
    {
        SetTransformUnsafe(state, GetPhysicsBodyIndex(genId), transform);    
    }

    /// <summary>
    ///     Sets the transform of a body in the physics simulation.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <param name="transform">the new transform data for the physics body.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetTransformUnsafe(PhysicsSystemState state, int physicsBodyIndex, Transform transform)
    {
        state.Transforms.Positions.X[physicsBodyIndex] = transform.Position.X;
        state.Transforms.Positions.Y[physicsBodyIndex] = transform.Position.Y;
        state.Transforms.Scales.X[physicsBodyIndex] = transform.Scale.X;
        state.Transforms.Scales.Y[physicsBodyIndex] = transform.Scale.Y;
        state.Transforms.Cosines[physicsBodyIndex] = transform.Cos;
        state.Transforms.Sines[physicsBodyIndex] = transform.Sin;        
    }




    /*******************
    
        Collider Setters & Getters.
    
    ********************/





    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int SetCategory(PhysicsSystemState state, Shape shape, ColliderBehaviour behaviour, bool rigidbodyEnabled, 
        int physicsBodyIndex
    )
    {
        state.Shapes[physicsBodyIndex] = shape;
        ref int category = ref state.BvhCategories[physicsBodyIndex];

        switch (shape)
        {
            case Shape.Rectangle:
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

            case Shape.Circle:
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
    public static void IncrementCategoryCounter(PhysicsSystemState state, int category)
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
    public static void DecrementCategoryCounter(PhysicsSystemState state, int category)
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

    /// <summary>
    ///     Gets whether or not a physics body resolves collisions using <c>Trigger</c> behaviour. 
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     <c>true</c>, if the body is <c>Trigger</c>; otherwise <c>false</c>. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned value; as <c>false</c> will be returned when <c><paramref name="result"/></c> is not <c><see cref="GenIdResult.Ok"/></c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsTrigger(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;
            return false;
        }

        result = GenIdResult.Ok;
        return IsTriggerUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets whether or not a physics body resolves collisions using <c>Trigger</c> behaviour. 
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>true, if the body is <c>Trigger</c> otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsTriggerUnsafe(PhysicsSystemState state, GenId genId)
    {
        return IsTriggerUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets whether or not a physics body resolves collisions using <c>Trigger</c> behaviour. 
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <returns>true, if the body is <c>Trigger</c> otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsTriggerUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return Category.IsTrigger(state.BvhCategories[physicsBodyIndex]);
    }

    /// <summary>
    ///     Sets a physics body to resolve collisions using <c>Kinematic</c> behaviour.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIdResult SetKinematic(PhysicsSystemState state, GenId genId, bool isKinematic)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetKinematicUnsafe(state, genId, isKinematic);

        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Sets a physics body to resolve collisions using <c>Kinematic</c> behaviour.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetKinematicUnsafe(PhysicsSystemState state, GenId genId, bool isKinematic)
    {
        SetKinematicUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Sets a physics body to resolve collisions using <c>Kinematic</c> behaviour.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetKinematicUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        ref int category = ref state.BvhCategories[physicsBodyIndex];

        category = category switch
        {
            Category.SolidPolygonRigidBody   => Category.KinematicPolygonRigidBody,
            Category.SolidCircleRigidBody    => Category.KinematicCircleRigidBody,
            Category.SolidCapsuleRigidBody   => Category.KinematicCapsuleRigidBody,

            Category.TriggerPolygonRigidBody => Category.KinematicPolygonRigidBody,
            Category.TriggerCircleRigidBody  => Category.KinematicCircleRigidBody,
            Category.TriggerCapsuleRigidBody => Category.KinematicCapsuleRigidBody,

            Category.KinematicPolygonRigidBody  => Category.KinematicPolygonRigidBody,
            Category.KinematicCircleRigidBody   => Category.KinematicCircleRigidBody,
            Category.KinematicCapsuleRigidBody  => Category.KinematicCapsuleRigidBody,
            
            Category.SolidPolygonCollider    => Category.KinematicPolygonCollider,
            Category.SolidCircleCollider     => Category.KinematicCircleCollider,
            Category.SolidCapsuleCollider    => Category.KinematicCapsuleCollider,

            Category.TriggerPolygonCollider  => Category.KinematicPolygonCollider,
            Category.TriggerCircleCollider   => Category.KinematicCircleCollider,
            Category.TriggerCapsuleCollider  => Category.KinematicCapsuleCollider,

            Category.KinematicPolygonCollider   => Category.KinematicPolygonCollider,
            Category.KinematicCircleCollider    => Category.KinematicCircleCollider ,
            Category.KinematicCapsuleCollider   => Category.KinematicCapsuleCollider,

            _ => throw new Exception()
        };
    }

    /// <summary>
    ///     Gets whether or not a physics body resolves collisions using <c>Kinematic</c> behaviour.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     <c>true</c>, if the body is <c>Kinematic</c>; otherwise <c>false</c>. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned value; as <c>false</c> will be returned when <c><paramref name="result"/></c> is not <c><see cref="GenIdResult.Ok"/></c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsKinematic(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;
            return false;
        }
        result = GenIdResult.Ok;
        return IsKinematicUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets whether or not a physics body resolves collisions using <c>Kinematic</c> behaviour.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>true, if the body is <c>Kinematic</c> otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsKinematicUnsafe(PhysicsSystemState state, GenId genId)
    {
        return IsKinematicUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets whether or not a physics body resolves collisions using <c>Kinematic</c> behaviour.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <returns>true, if the body is <c>Kinematic</c> otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsKinematicUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return Category.IsKinematic(state.BvhCategories[physicsBodyIndex]);        
    }

    /// <summary>
    ///     Gets the vertices of a circle physics body.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="vertices">The source of the vertices; where the polygon's vertices are stored.</param>
    /// <param name="physicsBodyIndex">the indexx of the physics body in the physics system state.</param>
    /// <param name="x">output for the vertex x-component.</param>
    /// <param name="y">output for the vertex y-component.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetCircleWorldVerticesUnsafe(FsSoa_Vector2 vertices, int physicsBodyIndex, ref float x, ref float y)
    {
        int vertexIndex = FixedStrideArray.GetElementIndex(physicsBodyIndex, vertices.Stride, 0);
        x = vertices.X[vertexIndex];
        y = vertices.Y[vertexIndex];
    }

    /// <summary>
    ///     Gets the vertices of a polygon physics body.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="vertices">The source of the vertices; where the polygon's vertices are stored.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <param name="x">output for the vertex x-components.</param>
    /// <param name="y">output for the vertex y-components.</param>
    public static void GetPolygonVerticesUnsafe(FsSoa_Vector2 vertices, int physicsBodyIndex, ref Span<float> x, ref Span<float> y)
    {
        int startIndex = FixedStrideArray.GetElementIndex(physicsBodyIndex, vertices.Stride, 0);
        int appendCount = vertices.AppendCounts[physicsBodyIndex];
        x = vertices.X.AsSpan().Slice(startIndex, appendCount);
        y = vertices.Y.AsSpan().Slice(startIndex, appendCount);
    }




    /*******************
    
        Rigidbody Getters & Setters.
    
    ********************/




    /// <summary>
    ///     Gets a reference to the static friction value of a physics body.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     A reference to the static friction value; note that the data may be
    ///     the Nil value. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned reference.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetStaticFriction(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if (EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;
            
            // return a ref to the nil.
            return ref state.PhysicsMaterials.StaticFriction[0];
        }

        if(IsRigidBodyUnsafe(state, genId) != true)
        {
            // return not allocated as only a rigidbody is meant to have this property. 
            result = GenIdResult.NotAllocated;

            // return a ref to the nil.
            return ref state.PhysicsMaterials.StaticFriction[0];            
        }

        result = GenIdResult.Ok;
        return ref GetStaticFrictionUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets a reference to the static friction value of a physics body.
    /// </summary>
    /// <remarks>
    ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>A reference to the static friction value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetStaticFrictionUnsafe(PhysicsSystemState state, GenId genId)
    {
        return ref GetStaticFrictionUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets a reference to the static friction value of a physics body.
    /// </summary>
    /// <remarks>
    ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <returns>A reference to the static friction value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetStaticFrictionUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return ref state.PhysicsMaterials.StaticFriction[physicsBodyIndex];
    }

    /// <summary>
    ///     Gets a reference to the kinetic friction value of a physics body.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     A reference to the kinetic friction value; note that the data may be
    ///     the Nil value. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned reference.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetKineticFriction(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;

            // return a ref to the nil.
            return ref state.PhysicsMaterials.KineticFriction[0];
        }

        if(IsRigidBodyUnsafe(state, genId) != true)
        {
            // return not allocated as only a rigidbody is meant to have this property. 
            result = GenIdResult.NotAllocated;

            // return a ref to the nil.
            return ref state.PhysicsMaterials.KineticFriction[0];            
        }
        
        result = GenIdResult.Ok;
        return ref GetKineticFrictionUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets a reference to the static friction value of a physics body.
    /// </summary>
    /// <remarks>
    ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>A reference to the static friction value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetKineticFrictionUnsafe(PhysicsSystemState state, GenId genId)
    {
        return ref GetKineticFrictionUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets a reference to the static friction value of a physics body.
    /// </summary>
    /// <remarks>
    ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <returns>A reference to the static friction value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetKineticFrictionUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return ref state.PhysicsMaterials.KineticFriction[physicsBodyIndex];
    }

    /// <summary>
    ///     Gets a reference to the density value of a physics body.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     A reference to the kinetic friction value; note that the data may be
    ///     the Nil value. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned reference.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetDensity(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;

            // return a ref to the nil.
            return ref state.PhysicsMaterials.Density[0];
        }

        if(IsRigidBodyUnsafe(state, genId) != true)
        {
            // return not allocated as only a rigidbody is meant to have this property. 
            result = GenIdResult.NotAllocated;

            // return a ref to the nil.
            return ref state.PhysicsMaterials.Density[0];            
        }

        result = GenIdResult.Ok;
        return ref GetDensityUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets a reference to the density value of a physics body.
    /// </summary>
    /// <remarks>
    ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>A reference to the density value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetDensityUnsafe(PhysicsSystemState state, GenId genId)
    {
        return ref GetDensityUnsafe(state, GetPhysicsBodyIndex(genId));
    }


    /// <summary>
    ///     Gets a reference to the density value of a physics body.
    /// </summary>
    /// <remarks>
    ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <returns>A reference to the density value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetDensityUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return ref state.PhysicsMaterials.Density[physicsBodyIndex];
    }

    /// <summary>
    ///     Gets a reference to the restitution value of a physics body.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     A reference to the restitution value; note that the data may be
    ///     the Nil value. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned reference.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetRestitution(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;

            // return a ref to the nil.
            return ref state.PhysicsMaterials.Restitution[0];
        }

        if(IsRigidBodyUnsafe(state, genId) != true)
        {
            // return not allocated as only a rigidbody is meant to have this property. 
            result = GenIdResult.NotAllocated;

            // return a ref to the nil.
            return ref state.PhysicsMaterials.Restitution[0];            
        }

        result = GenIdResult.Ok;
        return ref GetRestitutionUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets a reference to the restitution value of a physics body.
    /// </summary>
    /// <remarks>
    ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>A reference to the restitution value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetRestitutionUnsafe(PhysicsSystemState state, GenId genId)
    {
        return ref GetRestitutionUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets a reference to the restitution value of a physics body.
    /// </summary>
    /// <remarks>
    ///    <c>Rigidbody</c> and <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <returns>A reference to the restitution value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref float GetRestitutionUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return ref state.PhysicsMaterials.Restitution[physicsBodyIndex];
    }

    /// <summary>
    ///     Sets a physic body to resolve collisions by additionally applying a rotational force in relation to the collision.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="enabled">whether or not to set the body to have <c>RotationalPhysics</c>.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIdResult SetRotationalResponse(PhysicsSystemState state, GenId genId, bool enabled)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetRotationalResponseUnsafe(state, genId, enabled);
        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Sets a physic body to resolve collisions by additionally applying a rotational force in relation to the collision.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="enabled">whether or not to set the body to have <c>RotationalPhysics</c>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRotationalResponseUnsafe(PhysicsSystemState state, GenId genId, bool enabled)
    {
        SetRotationalResponseUnsafe(state, GetPhysicsBodyIndex(genId), enabled);
    }

    /// <summary>
    ///     Sets a physic body to resolve collisions by additionally applying a rotational force in relation to the collision.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <param name="enabled">whether or not to set the body to have <c>RotationalPhysics</c>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRotationalResponseUnsafe(PhysicsSystemState state, int physicsBodyIndex, bool enabled)
    {
        state.RotationalResponses[physicsBodyIndex] = enabled;
    }

    /// <summary>
    ///     Gets whether or not a physics body uses rotational physics.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     <c>true</c>, if the body uses <c>RotationalPhysics</c>; otherwise <c>false</c>. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned value; as <c>false</c> will be returned when <c><paramref name="result"/></c> is not <c><see cref="GenIdResult.Ok"/></c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool UsesRotationalPhysics(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if (EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;
            return false;
        }

        result = GenIdResult.Ok;
        return UsesRotationalResponseUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets whether or not a physics body uses rotational physics resolution.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>true, if the body has <c>RigidBodyPhysics</c>; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool UsesRotationalResponseUnsafe(PhysicsSystemState state, GenId genId)
    {
        return UsesRotationalResponseUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets whether or not a physics body uses rotational physics resolution.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the index of the physics body in the physics system.</param>
    /// <returns>true, if the body has <c>RigidBodyPhysics</c>; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool UsesRotationalResponseUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return state.RotationalResponses[physicsBodyIndex];
    }

    /// <summary>
    ///     Sets whether a physics body exhibits rigidbody behaviour.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIdResult SetRigidBody(PhysicsSystemState state, GenId genId, bool enabled)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetRigidBodyUnsafe(state, GenId.GetIndex(genId), enabled);
        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Sets whether a physics body exhibits rigidbody behaviour.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRigidBodyUnsafe(PhysicsSystemState state, int physicsBodyIndex, bool enabled)
    {
        switch (enabled)
        {
            case true:
                Category.SetToRigidBody(ref state.BvhCategories[physicsBodyIndex]);
            break;
            case false:
                Category.SetToCollider(ref state.BvhCategories[physicsBodyIndex]);
            break;
        }
    }

    /// <summary>
    ///     Gets whether or not a physics body is a <c>RigidBody</c>.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     <c>true</c>, if the body is a <c>RigidBody</c>; otherwise <c>false</c>. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned value; as <c>false</c> will be returned when <c><paramref name="result"/></c> is not <c><see cref="GenIdResult.Ok"/></c>.
    /// </returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsRigidBody(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;
            return false;
        }
        
        result = GenIdResult.Ok;
        return IsRigidBodyUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets whether or not a physics body is a <c>RigidBody</c>.
    /// </summary>
    /// <remarks>
    ///     GenId checks are not enforced; the retrieved data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>true, if the body is a <c>RigidBody</c>; otherwise false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsRigidBodyUnsafe(PhysicsSystemState state, GenId genId)
    {
        return IsRigidBodyUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets whether or not a physics body is a <c>RigidBody</c>.
    /// </summary>
    /// <remarks>
    ///     GenId checks are not enforced; the retrieved data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <returns>true, if the body is a <c>RigidBody</c>; otherwise false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsRigidBodyUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {        
        return Category.IsRigidBody(state.BvhCategories[physicsBodyIndex]);
    }

    /// <summary>
    ///     Gets the linear velocity of a physics body.
    /// </summary>
    /// <param name="state">the state instance that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for the gen id result when retrieving the data.</param>
    /// <returns>a copy of the physics body's linear velocity if successfull; otherwise the default value.</returns>
    public static Math.Vector2 GetLinearVelocity(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;
            return default;
        }

        int index = GenId.GetIndex(genId);
                
        if(Category.IsRigidBody(state.BvhCategories[index]) == false)
        {
            result = GenIdResult.NotAllocated;
            return default;
        }

        if(IsActiveUnsafe(state, index) == false)
        {
            result = GenIdResult.NotActive;
            return default;
        }

        return GetLinearVelocityUnsafe(state, index);
    }

    /// <summary>
    ///     Gets the linear velocity of a physics body.
    /// </summary>
    /// <remarks>
    ///     GenId checks are not enforced; the retrieved data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="state">the state instance that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>a copy of the physics body's linear velocity.</returns>
    public static Math.Vector2 GetLinearVelocityUnsafe(PhysicsSystemState state, GenId genId)
    {
        return GetLinearVelocityUnsafe(state, GenId.GetIndex(genId));
    }

    /// <summary>
    ///     Gets the linear velocity of a physics body.
    /// </summary>
    /// <remarks>
    ///     GenId checks are not enforced; the retrieved data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="state">the state instance that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the state instnce.</param>
    /// <returns>a copy of the physics body's linear velocity.</returns>
    public static Math.Vector2 GetLinearVelocityUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        Soa_Vector2 linearVelocities = state.LinearVelocities;
        return new(linearVelocities.X[physicsBodyIndex], linearVelocities.Y[physicsBodyIndex]);
    }




    /******************
    
        Collision Getters.
    
    *******************/




    /// <summary>
    ///     Gets whether a physics body has collided with other bodies.
    /// </summary>
    /// <param name="state">the state that contacinsthe physics body.</param>
    /// <param name="physicsBodyId">the id of the physics body.</param>
    /// <param name="result">output for the genid result.</param>
    /// <returns>true, if the physics body has collided with another; otherwise false.</returns>
    public static bool HasCollisions(PhysicsSystemState state, GenId physicsBodyId, ref GenIdResult result)
    {
        if (EntityRegistry.IsGenIdStale(state.Entities, physicsBodyId))
        {
            result = GenIdResult.StaleGenId;
            return false;
        }

        result = GenIdResult.Ok;
        return CollisionManifold.HasContacts(state.CollisionManifoldState, GenId.GetIndex(physicsBodyId));
    }

    /// <summary>
    ///     Executes a collision callback for a given physics body.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callbackPacket">the data packet to route to the callback function.</param>
    /// <param name="callbacks">the callbacks for all physics bodies in the physics state.</param>
    /// <param name="state">the physics state that contains the physics bodies.</param>
    /// <param name="physicsBodyId">the id of the physics body to execute callbacks for.</param>
    public static unsafe void ExecuteCollisionCallbacks<T>(this T callbackPacket, CollisionCallbacks<T> callbacks, PhysicsSystemState state, GenId physicsBodyId)
    {        
        if (EntityRegistry.IsGenIdStale(state.Entities, physicsBodyId))
        {
            return;
        }
        
        // hoisting invariance.
        CollisionManifoldState manifold = state.CollisionManifoldState;
        Span<float> normalsX = manifold.Normals.X;
        Span<float> normalsY = manifold.Normals.Y;
        Span<float> firstContactPointsX = manifold.FirstContactPoints.X;
        Span<float> firstContactPointsY = manifold.FirstContactPoints.Y;
        Span<float> secondContactPointsX = manifold.SecondContactPoints.X;
        Span<float> secondContactPointsY = manifold.SecondContactPoints.Y;
        Span<float> depths = manifold.Depths;
        Span<bool> twoContactPoints = manifold.TwoContactPoints;
        Span<ContactState> contactStates = manifold.ContactStates;

        // get the collision indices of the physics body.
        int bodyIndex = GenId.GetIndex(physicsBodyId);
        int start = FixedStrideArray.GetElementIndex(bodyIndex, manifold.Stride, 0);
        int collisionCount = manifold.ActiveIndicesCount[bodyIndex];
        Span<int> collisionIndices = manifold.ActiveIndices.AsSpan(start, collisionCount);

        // get the callbacks of the physics body.
        StackArray<CollisionCallback<T>> callbackStack;

        // process each collision in each callback.
        for(int i = 0; i < collisionCount; i++)
        {
            // get the next collision to process.
            int collisionIndex = collisionIndices[i];
            
            // get the callbacks to iterate over.
            switch (contactStates[collisionIndex])
            {
                case ContactState.Enter:
                    callbackStack = callbacks.OnEnterCallbacks[bodyIndex];;
                break;
                case ContactState.Exit:
                    callbackStack = callbacks.OnExitCallbacks[bodyIndex];;
                break;
                case ContactState.Sustain:
                    callbackStack = callbacks.OnSustainCallbacks[bodyIndex];;
                break;
                case ContactState.None:
                    continue;
                default:
                    continue;
            }

            // read the data.
            CollisionInfo info = new(ref normalsX[collisionIndex], ref normalsY[collisionIndex], ref firstContactPointsX[collisionIndex], 
                ref firstContactPointsY[collisionIndex], ref secondContactPointsX[collisionIndex], ref secondContactPointsY[collisionIndex], 
                ref depths[collisionIndex], ref twoContactPoints[collisionIndex], default, default
            );

            // callback/process data.
            for(int j = 0; j < callbackStack.Count; j++)
            {
                callbackStack.Data[j].Pointer(callbackPacket, info);
            }
        }
    }




    /******************
    
        Util.
    
    *******************/




    /// <summary>
    ///     Applies an impulse force to a rigidbody.
    /// </summary>
    /// <param name="state">the physics state containing the rigidbody.</param>
    /// <param name="force">the force to apply to the rigidbody.</param>
    /// <param name="genId">the gen id of the rigidbody.</param>
    /// <returns>
    ///     <list type = "bullet">
    ///         <item><see cref="GenIdResult.Ok"/></item>
    ///         <item><see cref="GenIdResult.NotAllocated"/></item>
    ///         <item><see cref="GenIdResult.NotActive"/></item>
    ///         <item><see crerf="GenIdResult.StaleGenId"/></item>
    ///     </list>
    /// </returns>
    public static GenIdResult ImpulseForce(PhysicsSystemState state, Math.Vector2 force, GenId genId)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        int index = GenId.GetIndex(genId);
        
        if(Category.IsRigidBody(state.BvhCategories[index]) == false)
        {
            return GenIdResult.NotAllocated;   
        }

        if(IsActiveUnsafe(state, index) == false)
        {
            return GenIdResult.NotActive;
        }

        state.LinearVelocities.X[index] += force.X;
        state.LinearVelocities.Y[index] += force.Y;

        return GenIdResult.Ok;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetPhysicsBodyIndex(GenId genId)
    {
        return GenId.GetIndex(genId);
    }

    public static GenIdResult Deallocate(PhysicsSystemState state, GenId genId)
    {
        GenIdResult result = EntityRegistry.Deallocate(state.Entities, genId);

        int index = GenId.GetIndex(genId);

        // this is here temporarily and SHOULD be removed.
        FsSoa_Vector2.ClearEntryAppendCount(state.LocalVertices, index);

        switch (state.BvhCategories[index])
        {
            case Category.SolidPolygonRigidBody    : state.SolidPolygonRigidBodyCount--;     break;
            case Category.SolidCircleRigidBody     : state.SolidCircleRigidBodyCount--;      break;
            case Category.SolidCapsuleRigidBody    : state.SolidCapsuleRigidBodyCount--;     break;
            
            case Category.KinematicPolygonRigidBody: state.KinematicPolygonRigidBodyCount--; break;
            case Category.KinematicCircleRigidBody : state.KinematicCircleRigidBodyCount--;  break;
            case Category.KinematicCapsuleRigidBody: state.KinematicCapsuleRigidBodyCount--; break;
            
            case Category.TriggerPolygonRigidBody  : state.TriggerPolygonRigidBodyCount--;   break;
            case Category.TriggerCircleRigidBody   : state.TriggerCircleRigidBodyCount--;    break;
            case Category.TriggerCapsuleRigidBody  : state.TriggerCapsuleRigidBodyCount--;   break;
            
            case Category.SolidPolygonCollider     : state.SolidPolygonColliderCount--;      break;
            case Category.SolidCircleCollider      : state.SolidCircleColliderCount--;       break;
            case Category.SolidCapsuleCollider     : state.SolidCapsuleColliderCount--;      break;
            
            case Category.KinematicPolygonCollider : state.KinematicPolygonColliderCount--;  break;
            case Category.KinematicCircleCollider  : state.KinematicCircleColliderCount--;   break;
            case Category.KinematicCapsuleCollider : state.KinematicCapsuleColliderCount--;  break;
            
            case Category.TriggerPolygonCollider   : state.TriggerPolygonColliderCount--;    break;
            case Category.TriggerCircleCollider    : state.TriggerCircleColliderCount--;     break;
            case Category.TriggerCapsuleCollider   : state.TriggerCapsuleColliderCount--;    break;

            default:
                System.Diagnostics.Debug.Assert(false);
                break;
        }

        if(result == GenIdResult.Ok)
        {            
            SetActiveUnsafe(state, GenId.GetIndex(genId), false);
        }
        
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void PreparePhysicsBodyAllocation(PhysicsSystemState state, Transform transform, Span<float> localVertsX, 
        Span<float> localVertsY, ColliderBehaviour colliderBehaviour, GenId entityId, int physicsBodyIndex, bool IsRigidBody, Shape shape
    )
    {
        // clear any garbage data from previous allocations.
        FsSoa_Vector2.ClearEntryAppendCount(state.LocalVertices, physicsBodyIndex);

        // set the new data.
        SetActiveUnsafe(state, physicsBodyIndex, true);
        int category = SetCategory(state, shape, colliderBehaviour, IsRigidBody, physicsBodyIndex);            
        IncrementCategoryCounter(state, category);
        SetTransformUnsafe(state, physicsBodyIndex, transform);

        for(int i = 0; i < localVertsX.Length; i++)
        {
            FsSoa_Vector2.Append(state.LocalVertices, physicsBodyIndex, localVertsX[i], localVertsY[i]);
        }
        
        state.EntityIds[physicsBodyIndex] = entityId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FinalisePhysicsBodyAllocation(PhysicsSystemState state, int physicsBodyIndex)
    {        
        // set this so that the previous position isnt garbage from previous steps.
        state.PreviousStepPositions.X[physicsBodyIndex] = state.Transforms.Positions.X[physicsBodyIndex];
        state.PreviousStepPositions.Y[physicsBodyIndex] = state.Transforms.Positions.Y[physicsBodyIndex];

        // reset any forces that were applied to previously allocated bodies.
        PhysicsSystem.ClearForcesAndVelocities(state, physicsBodyIndex);
    }




    /******************
    
        Circle.
    
    *******************/




    public static class Circle
    {
        /// <summary>
        ///     Allocates a circle collider into a physics system state.
        /// </summary>
        /// <param name="state">the physics system state to allocate into.</param>
        /// <param name="shape">the local-space shape data.</param>
        /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
        /// <param name="entityId">the id of the entity associated with this physics body.</param>
        /// <param name="colliderBehaviour">the behaviour the collider exhibits.</param>
        /// <param name="genId">output for the gen id to the newly allocated body.</param>
        ///<returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="GenIdResult.Ok"/>
        ///         </item>
        ///         <item>
        ///             <see cref="GenIdResult.MemoryLimitHit"/>
        ///         </item>
        ///     </list>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static GenIdResult AllocateCollider(PhysicsSystemState state, Math.Shapes.Circle shape, Transform transform, GenId entityId, 
            ColliderBehaviour colliderBehaviour, ref GenId genId
        )
        {
            GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId); 
            if(result != GenIdResult.Ok)
            {
                return result;
            }
            
            int physicsBodyIndex = GetPhysicsBodyIndex(genId);
            PreparePhysicsBodyAllocation(state, transform, [shape.X], [shape.Y], colliderBehaviour, entityId, physicsBodyIndex, 
                false, Shape.Circle
            );
            
            { // Set specific data.

                // rigidbodies should respond to this like a kinematic rigidbody if it is solid or kinematic. 
                state.Masses[physicsBodyIndex] = 0;
                state.InverseMasses[physicsBodyIndex] = 0;
                state.LocalRadii[physicsBodyIndex] = shape.Radius;            
            }

            FinalisePhysicsBodyAllocation(state, physicsBodyIndex);
        
            return GenIdResult.Ok;
        }
        
        /// <summary>
        ///     Allocates a circle rigidbody into a physics system state.
        /// </summary>
        /// <param name="state">the physics system state to allocate into.</param>
        /// <param name="shape">the local-space shape data.</param>
        /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
        /// <param name="material">the physics material to apply to the body.</param>
        /// <param name="entityId">the gen id of the entity associated with this physics body.</param>
        /// <param name="colliderBehaviour">the behaviour the collider exhibits.</param>
        /// <param name="rotationalResponse">whether rotational collision response is enabled for the rigidbody.</param>
        /// <param name="genId">the associated genId to the newly allocated body.</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="GenIdResult.Ok"/>
        ///         </item>
        ///         <item>
        ///             <see cref="GenIdResult.MemoryLimitHit"/>
        ///         </item>
        ///     </list>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static GenIdResult AllocateRigidBody(PhysicsSystemState state, Math.Shapes.Circle shape, Transform transform, PhysicsMaterial material, 
            GenId entityId, ColliderBehaviour colliderBehaviour, bool rotationalResponse, ref GenId genId
        )
        {
            GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId);
            if(result != GenIdResult.Ok)
            {
                return result;
            }

            int physicsBodyIndex = GetPhysicsBodyIndex(genId);
            PreparePhysicsBodyAllocation(state, transform, [shape.X], [shape.Y], colliderBehaviour, entityId, physicsBodyIndex, 
                true, Shape.Circle
            );

            {   // Set specific data. 
                
                SetRotationalResponseUnsafe(state, physicsBodyIndex, rotationalResponse);
                Soa_PhysicsMaterial.Insert(state.PhysicsMaterials, material.StaticFriction, material.KineticFriction, material.Density, 
                    material.Restitution, physicsBodyIndex
                );
                state.LocalRadii[physicsBodyIndex] = shape.Radius;
                state.EntityIds[physicsBodyIndex] = entityId;
            }

            FinalisePhysicsBodyAllocation(state, physicsBodyIndex);

            return GenIdResult.Ok;
        }

        /// <summary>
        /// Calculates the rotational inertia for a circle.
        /// </summary>
        /// <param name="radius">the radius of the shape.</param>
        /// <param name="mass">the mass of the shape.</param>
        /// <returns>the rotational inertia value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float CalculateRotationalInertia(float radius, float mass)
        {
            return PhysicsSystem.CircleRotationalInertia * mass * (radius * radius);
        }

        /// <summary>
        /// Vectorised radius calculation for circles.
        /// </summary>
        /// <param name="radius">the radii of the shapes.</param>
        /// <param name="mass">the mass of the shapes.</param>
        /// <returns>the rotational inertia values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector<float> CalculateRotationalInertia(Vector<float> radius, Vector<float> mass)
        {
            return PhysicsSystem.VectorCircleRotationalInertia * mass * (radius * radius);
        }

        /// <summary>
        /// Calculates the mass of a circle.
        /// </summary>
        /// <param name="radius">the radius of the shape.</param>
        /// <param name="density">the density of the shape.</param>
        /// <returns>the mass value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float CalculateMass(float radius, float density)
        {
            return density * Math.Shapes.Circle.GetArea(radius);
        }

        /// <summary>
        /// Vectorised mass calculation for circles.
        /// </summary>
        /// <param name="radius">the radii of the shapes.</param>
        /// <param name="density">the densities of the shapes.</param>
        /// <returns>the area values of the shapes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector<float> CalculateMass(Vector<float> radius, Vector<float> density)
        {
            return density * Math.Shapes.Circle.GetArea(radius);
        }
    }




    /******************
    
        Rectangle.
    
    *******************/




    public static class Rectangle
    {
        /// <summary>
        ///     Allocates a rectangle collider into a physics system state.
        /// </summary>
        /// <param name="state">the physics system state to allocate into.</param>
        /// <param name="shape">the local-space shape data.</param>
        /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
        /// <param name="entityId">the gen id of the entity associated with this physics body.</param>
        /// <param name="colliderBehaviour">the behaviour the collider exhibits.</param>
        /// <param name="genId">the associated gen id to the newly allocated body.</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="GenIdResult.Ok"/>
        ///         </item>
        ///         <item>
        ///             <see cref="GenIdResult.MemoryLimitHit"/>
        ///         </item>
        ///     </list>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static GenIdResult AllocateCollider(PhysicsSystemState state, Math.Shapes.Rectangle shape, Transform transform, 
            GenId entityId, ColliderBehaviour colliderBehaviour, ref GenId genId
        )
        {
            GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId);
            if(result != GenIdResult.Ok)
            {
                return result;
            }

            PolygonRectangle polyRect = new(shape);

            int physicsBodyIndex = GetPhysicsBodyIndex(genId);
            PreparePhysicsBodyAllocation(state, transform, PolygonRectangle.VerticesXAsSpan(polyRect), 
                PolygonRectangle.VerticesYAsSpan(polyRect), colliderBehaviour, entityId, physicsBodyIndex, false, Shape.Rectangle
            );

            {   // set specific data.
                
                // apply data.                        
                state.LocalHeights[physicsBodyIndex] = shape.Height;
                state.LocalWidths[physicsBodyIndex] = shape.Width;

                // rigidbodies should respond to this like a kinematic rigidbody if it is solid or kinematic. 
                state.Masses[physicsBodyIndex] = 0;
                state.InverseMasses[physicsBodyIndex] = 0;
            }

            FinalisePhysicsBodyAllocation(state, physicsBodyIndex);

            return GenIdResult.Ok;
        }

        /// <summary>
        ///     Allocates a rectangle rigidbody into a physics system state.
        /// </summary>
        /// <param name="state">the physics system state to allocate into.</param>
        /// <param name="shape">the local-space shape data.</param>
        /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
        /// <param name="material">the physics material to apply to the body.</param>
        /// <param name="entityId">the gen id of the entity associated with this physics body.</param>
        /// <param name="colliderBehaviour">the behaviour the collider exhibits.</param>
        /// <param name="rotationalResponse">Whether rotational collision response is enabled for a rigidbody.</param>
        /// <param name="genId">the associated genId to the newly allocated body.</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="GenIdResult.Ok"/>
        ///         </item>
        ///         <item>
        ///             <see cref="GenIdResult.MemoryLimitHit"/>
        ///         </item>
        ///     </list>
        /// </returns>.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static GenIdResult AllocateRigidBody(PhysicsSystemState state, Math.Shapes.Rectangle shape, Transform transform,
            PhysicsMaterial material, GenId entityId, ColliderBehaviour colliderBehaviour, bool rotationalResponse, ref GenId genId
        )
        {
            GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId);
            if(result != GenIdResult.Ok)
            {
                return result;
            }

            PolygonRectangle polyRect = new(shape);
            
            int physicsBodyIndex = GetPhysicsBodyIndex(genId);
            PreparePhysicsBodyAllocation(state, transform, PolygonRectangle.VerticesXAsSpan(polyRect), 
                PolygonRectangle.VerticesYAsSpan(polyRect), colliderBehaviour, entityId, physicsBodyIndex, true, Shape.Rectangle
            );

            {   // specific data.
                
                SetRotationalResponseUnsafe(state, physicsBodyIndex, rotationalResponse);
                state.LocalHeights[physicsBodyIndex] = shape.Height;
                state.LocalWidths[physicsBodyIndex] = shape.Width;
                Soa_PhysicsMaterial.Insert(state.PhysicsMaterials, material.StaticFriction, material.KineticFriction, material.Density, 
                    material.Restitution, physicsBodyIndex
                );
            }

            FinalisePhysicsBodyAllocation(state, physicsBodyIndex);
            return GenIdResult.Ok;
        }

        /// <summary>
        /// Calculates the mass of a rectangle.
        /// </summary>
        /// <param name="width">the width of the shape.</param>
        /// <param name="height">the height of the shape.</param>
        /// <param name="density">the density of the shape.</param>
        /// <returns>the mass value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float CalculateMass(float width, float height, float density)
        {
            return Math.Shapes.Rectangle.GetArea(width, height) * density;
        } 

        /// <summary>
        /// Vectorised mass calculation for rectangles.
        /// </summary>
        /// <param name="width">the widths of the shapes.</param>
        /// <param name="height">the heights of the shapes.</param>
        /// <param name="density">the densities of the shapes.</param>
        /// <returns>the mass values of the shapes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector<float> CalculateMass(Vector<float> width, Vector<float> height, Vector<float> density)
        {
            return Math.Shapes.Rectangle.GetArea(width, height) * density;
        }

        /// <summary>
        /// Calculates the rotational inertia of a rectangle.
        /// </summary>
        /// <param name="width">the width of the shape.</param>
        /// <param name="height">the height of the shape.</param>
        /// <param name="mass">the mass of the shape.</param>
        /// <returns>the rotational inertia value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float CalculateRotationalInertia(float width, float height, float mass)
        {
            return PhysicsSystem.RectangleRotationalInertia * mass * ((width * width) + (height * height));
        }

        /// <summary>
        /// Vectorized rotational inertia calculation for rectangles.
        /// </summary>
        /// <param name="width">the widths of the shapes.</param>
        /// <param name="height">the heights of the shapes.</param>
        /// <param name="density">the densities of the shapes.</param>
        /// <returns>the inertia values of the shapes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector<float> CalculateRotationalInertia(Vector<float> width, Vector<float> height, Vector<float> mass)
        {
            return PhysicsSystem.VectorRectangleRotationalInertia * mass * ((width * width) + (height * height));
        }
    }
}