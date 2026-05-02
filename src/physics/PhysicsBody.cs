using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Howl.Collections;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public static class PhysicsBody
{




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
    /// <param name="isActive">whether or not to set the physics body to <c>Active</c>.</param>
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
    /// <param name="isActive">whether or not to set the physics body to <c>Active</c>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetActiveUnsafe(PhysicsSystemState state, int physicsBodyIndex, bool isActive)
    {
        if (isActive)
        {
            state.Flags[physicsBodyIndex] |= PhysicsBodyFlags.Active;
        }
        else
        {
            state.Flags[physicsBodyIndex] &= ~PhysicsBodyFlags.Active;        
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
        return (state.Flags[physicsBodyIndex] & PhysicsBodyFlags.Active) != 0;
    }

    /// <summary>
    ///     Sets whether or not a physics body slot has been allcoated in a physics system state.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="isAllocated">whether or not the slot has been allocated to.</param>
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
    public static GenIdResult SetAllocated(PhysicsSystemState state, GenId genId, bool isAllocated)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetAllocatedUnsafe(state, genId, isAllocated);
        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Sets whether or not a physics body slot has been allcoated in a physics system state.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="isAllocated">whether or not the slot has been allocated to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetAllocatedUnsafe(PhysicsSystemState state, GenId genId, bool isAllocated)
    {
        SetAllocatedUnsafe(state, GetPhysicsBodyIndex(genId), isAllocated);
    }

    /// <summary>
    ///     Sets whether or not a physics body slot has been allcoated in a physics system state.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state..</param>
    /// <param name="isAllocated">whether or not the slot has been allocated to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public  static void SetAllocatedUnsafe(PhysicsSystemState state, int physicsBodyIndex, bool isAllocated)
    {
        if (isAllocated)
        {
            state.Flags[physicsBodyIndex] |= PhysicsBodyFlags.Allocated;
        }
        else
        {
            state.Flags[physicsBodyIndex] &= ~PhysicsBodyFlags.Allocated;
        }        
    }
    
    /// <summary>
    ///     Gets whether or not a physics body slot has been allocated in the physics system state.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="result">output for whether or not the retrieved reference is valid.</param>
    /// <returns>
    ///     <c>true</c>, if the body is <c>Allocated</c>; otherwise <c>false</c>. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned value; as <c>false</c> will be returned when <c><paramref name="result"/></c> is not <c><see cref="GenIdResult.Ok"/></c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsAllocated(PhysicsSystemState state, GenId genId, ref GenIdResult result)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            result = GenIdResult.StaleGenId;
            return false;
        }

        result = GenIdResult.Ok;
        return IsAllocatedUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets whether or not a physics body slot has been allocated in the physics system state.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>true, if the phsyics body has been allocated; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsAllocatedUnsafe(PhysicsSystemState state, GenId genId)
    {
        return IsAllocatedUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets whether or not a physics body slot has been allocated in the physics system state.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body within the physics system state.</param>
    /// <returns>true, if the phsyics body has been allocated; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsAllocatedUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return (state.Flags[physicsBodyIndex] & PhysicsBodyFlags.Allocated) != 0;        
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
        state.Transforms.Coses[physicsBodyIndex] = transform.Cos;
        state.Transforms.Sins[physicsBodyIndex] = transform.Sin;        
    }




    /*******************
    
        Collider Setters & Getters.
    
    ********************/




    /// <summary>
    ///     Sets a physics body to resolve collisions using <c>Trigger</c> behaviour.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="isTrigger">whether or not to set the body to <c>Trigger</c>.</param>
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
    public static GenIdResult SetTrigger(PhysicsSystemState state, GenId genId, bool isTrigger)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetTriggerUnsafe(state, genId, isTrigger);
        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Sets a physics body to resolve collisions using <c>Trigger</c> behaviour.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="isTrigger">whether or not to set the body to <c>Trigger</c>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetTriggerUnsafe(PhysicsSystemState state, GenId genId, bool isTrigger)
    {
        SetTriggerUnsafe(state, GetPhysicsBodyIndex(genId), isTrigger);
    }

    /// <summary>
    ///     Sets a physics body to resolve collisions using <c>Trigger</c> behaviour.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <param name="isTrigger">whether or not to set the body to <c>Trigger</c>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetTriggerUnsafe(PhysicsSystemState state, int physicsBodyIndex, bool isTrigger)
    {        
        if (isTrigger)
        {
            state.Flags[physicsBodyIndex] |= PhysicsBodyFlags.Trigger;
        }
        else
        {
            state.Flags[physicsBodyIndex] &= ~PhysicsBodyFlags.Trigger;
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
        return (state.Flags[physicsBodyIndex] & PhysicsBodyFlags.Trigger) != 0;        
    }

    /// <summary>
    ///     Sets a physics body to resolve collisions using <c>Kinematic</c> behaviour.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="isKinematic">true, if <c>Kinematic</c>; otherwise false.</param>
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
    /// <param name="isKinematic">true, if <c>Kinematic</c>; otherwise false.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetKinematicUnsafe(PhysicsSystemState state, GenId genId, bool isKinematic)
    {
        SetKinematicUnsafe(state, GetPhysicsBodyIndex(genId), isKinematic);
    }

    /// <summary>
    ///     Sets a physics body to resolve collisions using <c>Kinematic</c> behaviour.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <param name="isKinematic">true, if <c>Kinematic</c>; otherwise false.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetKinematicUnsafe(PhysicsSystemState state, int physicsBodyIndex, bool isKinematic)
    {        
        if (isKinematic)
        {
            state.Flags[physicsBodyIndex] |= PhysicsBodyFlags.Kinematic;
        }
        else
        {
            state.Flags[physicsBodyIndex] &= ~PhysicsBodyFlags.Kinematic;
        }        
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
        return (state.Flags[physicsBodyIndex] & PhysicsBodyFlags.Kinematic) != 0;        
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
    /// <param name="hasRotationalPhysics">whether or not to set the body to have <c>RotationalPhysics</c>.</param>
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
    public static GenIdResult SetRotationalPhysics(PhysicsSystemState state, GenId genId, bool hasRotationalPhysics)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetRotationalPhysicsUnsafe(state, genId, hasRotationalPhysics);
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
    /// <param name="hasRotationalPhysics">whether or not to set the body to have <c>RotationalPhysics</c>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRotationalPhysicsUnsafe(PhysicsSystemState state, GenId genId, bool hasRotationalPhysics)
    {
        SetRotationalPhysicsUnsafe(state, GetPhysicsBodyIndex(genId), hasRotationalPhysics);
    }

    /// <summary>
    ///     Sets a physic body to resolve collisions by additionally applying a rotational force in relation to the collision.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <param name="hasRotationalPhysics">whether or not to set the body to have <c>RotationalPhysics</c>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRotationalPhysicsUnsafe(PhysicsSystemState state, int physicsBodyIndex, bool hasRotationalPhysics)
    {        
        if (hasRotationalPhysics)
        {
            state.Flags[physicsBodyIndex] |= PhysicsBodyFlags.RotationalPhysics;
        }
        else
        {
            state.Flags[physicsBodyIndex] &= ~PhysicsBodyFlags.RotationalPhysics;
        }
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
        return UsesRotationalPhysicsUnsafe(state, genId);
    }

    /// <summary>
    ///     Gets whether or not a physics body uses rotational physics.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <returns>true, if the body has <c>RigidBodyPhysics</c>; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool UsesRotationalPhysicsUnsafe(PhysicsSystemState state, GenId genId)
    {
        return UsesRotationalPhysicsUnsafe(state, GetPhysicsBodyIndex(genId));
    }

    /// <summary>
    ///     Gets whether or not a physics body uses rotational physics.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> checks are not enforced; the retrieved data at the given gen id slot will always be returned. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the index of the physics body in the physics system.</param>
    /// <returns>true, if the body has <c>RigidBodyPhysics</c>; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool UsesRotationalPhysicsUnsafe(PhysicsSystemState state, int physicsBodyIndex)
    {
        return (state.Flags[physicsBodyIndex] & PhysicsBodyFlags.RotationalPhysics) != 0;
    }

    /// <summary>
    ///     Sets whether or not a physics body is a rigidbody.
    /// </summary>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <param name="isRigidBody">true, if it is a <c>RigidBody</c>; otherwise false.</param>
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
    public static GenIdResult SetRigidBody(PhysicsSystemState state, GenId genId, bool isRigidBody)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        SetRigidBodyUnsafe(state, genId, isRigidBody);
        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Sets whether or not a physics body is a rigidbody.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="genId">the gen id of the physics body.</param>
    /// <param name="isRigidBody">true, if it is a <c>RigidBody</c>; otherwise false.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRigidBodyUnsafe(PhysicsSystemState state, GenId genId, bool hasRigidBody)
    {
        SetRigidBodyUnsafe(state, GetPhysicsBodyIndex(genId), hasRigidBody);
    }

    /// <summary>
    ///     Sets whether or not a physics body is a rigidbody.
    /// </summary>
    /// <remarks>
    ///    <c>StaleGenId</c> check is not enforced; the retrieved data at the given gen id slot will always mutated. 
    /// </remarks>
    /// <param name="state">the physics system state that contains the physics body.</param>
    /// <param name="physicsBodyIndex">the index of the physics body in the physics system state.</param>
    /// <param name="isRigidBody">true, if it is a <c>RigidBody</c>; otherwise false.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRigidBodyUnsafe(PhysicsSystemState state, int physicsBodyIndex, bool hasRigidBody)
    {
        if (hasRigidBody)
        {
            state.Flags[physicsBodyIndex] |= PhysicsBodyFlags.RigidBody;
        }
        else
        {
            state.Flags[physicsBodyIndex] &= ~PhysicsBodyFlags.RigidBody;
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
        return (state.Flags[physicsBodyIndex] & PhysicsBodyFlags.RigidBody) != 0;
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
        
        ref PhysicsBodyFlags flag = ref state.Flags[index];
        
        if((flag & PhysicsBodyFlags.Allocated) == 0 || (flag & PhysicsBodyFlags.RigidBody) == 0)
        {
            result = GenIdResult.NotAllocated;
            return default;
        }

        if((flag & PhysicsBodyFlags.Active) == 0)
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




    /*******************
    
        Circle Body.
    
    ********************/




    /// <summary>
    ///     Allocates a circle collider into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="entityId">the id of the entity associated with this physics body.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="genId">the associated gen id to the newly allocated body.</param>
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
    public static GenIdResult AllocateCircleCollider(PhysicsSystemState state, Circle shape, Transform transform, GenId entityId, bool isKinematic, bool isTrigger, 
        ref GenId genId
    )
    {
        GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId); 
        if(result != GenIdResult.Ok)
        {
            return result;
        }
        int physicsBodyIndex = GetPhysicsBodyIndex(genId);

        state.AlloctedPhysicsBodyCount++;

        // handle flags.
        // note: no circle shape is needed to be set as it is implied by the system that when a shape is not
        // set, a physics body is a circle.
        SetActiveUnsafe(state, physicsBodyIndex, true);
        SetAllocatedUnsafe(state, physicsBodyIndex, true);
        SetRigidBodyUnsafe(state, physicsBodyIndex, false);
        SetTriggerUnsafe(state, physicsBodyIndex, isTrigger);
        SetKinematicUnsafe(state, physicsBodyIndex, isKinematic);

        // apply data.
        SetTransformUnsafe(state, physicsBodyIndex, transform);
        PhysicsSystem.AddLocalVertices(state, [shape.X], [shape.Y], out int verticesFirstIndex, out int verticeCount);
        state.LocalRadii[physicsBodyIndex] = shape.Radius;
        state.EntityIds[physicsBodyIndex] = entityId;
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
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="rotationalPhysics">whether to enable rotational physics for the physics body.</param>
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
    public static GenIdResult AllocateCircleRigidBody(PhysicsSystemState state, Circle shape, Transform transform, PhysicsMaterial material, 
        GenId entityId, bool isKinematic, bool isTrigger, bool rotationalPhysics, ref GenId genId
    )
    {
        GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId);
        if(result != GenIdResult.Ok)
        {
            return result;
        }
        int physicsBodyIndex = GetPhysicsBodyIndex(genId);

        state.AlloctedPhysicsBodyCount++;

        // handle flags.
        // note: no circle shape is needed to be set as it is implied by the system that when a shape is not
        // set, a physics body is a circle.
        SetActiveUnsafe(state, physicsBodyIndex, true);
        SetAllocatedUnsafe(state, physicsBodyIndex, true);
        SetRigidBodyUnsafe(state, physicsBodyIndex, true);
        SetTriggerUnsafe(state, physicsBodyIndex, isTrigger);
        SetKinematicUnsafe(state, physicsBodyIndex, isKinematic);
        SetRotationalPhysicsUnsafe(state, physicsBodyIndex, rotationalPhysics);

        // apply data.
        PhysicsSystem.AddLocalVertices(state, [shape.X], [shape.Y], out int verticesFirstIndex, out int verticeCount);
        SetTransformUnsafe(state, physicsBodyIndex, transform);
        Soa_PhysicsMaterial.Insert(state.PhysicsMaterials, material.StaticFriction, material.KineticFriction, material.Density, 
            material.Restitution, physicsBodyIndex
        );
        state.LocalRadii[physicsBodyIndex] = shape.Radius;
        state.EntityIds[physicsBodyIndex] = entityId;

        // reset forces
        PhysicsSystem.ClearForcesAndVelocities(state, physicsBodyIndex);

        return GenIdResult.Ok;
    }

    /// <summary>
    /// Calculates the rotational inertia for a circle.
    /// </summary>
    /// <param name="radius">the radius of the shape.</param>
    /// <param name="mass">the mass of the shape.</param>
    /// <returns>the rotational inertia value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float CalculateCircleRotationalInertia(float radius, float mass)
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
    public static Vector<float> CalculateCircleRotationalInertia(Vector<float> radius, Vector<float> mass)
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
    public static float CalculateCircleMass(float radius, float density)
    {
        return density * Circle.GetArea(radius);
    }

    /// <summary>
    /// Vectorised mass calculation for circles.
    /// </summary>
    /// <param name="radius">the radii of the shapes.</param>
    /// <param name="density">the densities of the shapes.</param>
    /// <returns>the area values of the shapes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector<float> CalculateCircleMass(Vector<float> radius, Vector<float> density)
    {
        return density * Circle.GetArea(radius);
    }




    /*******************
    
        Rectangle Body.
    
    ********************/




    /// <summary>
    ///     Allocates a rectangle collider into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="entityId">the gen id of the entity associated with this physics body.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
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
    public static GenIdResult AllocateRectangleCollider(PhysicsSystemState state, Rectangle shape, Transform transform, 
        GenId entityId, bool isKinematic, bool isTrigger, ref GenId genId
    )
    {
        GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId);
        if(result != GenIdResult.Ok)
        {
            return result;
        }
        int physicsBodyIndex = GetPhysicsBodyIndex(genId);

        state.AlloctedPhysicsBodyCount++;

        // handle flags.
        state.Flags[physicsBodyIndex] |= PhysicsBodyFlags.RectangleShape;
        SetActiveUnsafe(state, physicsBodyIndex, true);
        SetAllocatedUnsafe(state, physicsBodyIndex, true);
        SetRigidBodyUnsafe(state, physicsBodyIndex, false);
        SetTriggerUnsafe(state, physicsBodyIndex, isTrigger);
        SetKinematicUnsafe(state, physicsBodyIndex, isKinematic);

        // apply data.
        PolygonRectangle polyRect = new(shape);
        PhysicsSystem.AddLocalVertices(state, PolygonRectangle.VerticesXAsSpan(polyRect), PolygonRectangle.VerticesYAsSpan(polyRect), out int verticesFirstIndex, out int verticeCount);
        SetTransformUnsafe(state, genId, transform);
        state.LocalHeights[physicsBodyIndex] = shape.Height;
        state.LocalWidths[physicsBodyIndex] = shape.Width;
        state.EntityIds[physicsBodyIndex] = entityId;

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
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="rotationalPhysics">whether to enable rotational physics for the physics body.</param>
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
    public static GenIdResult AllocateRectangleRigidBody(PhysicsSystemState state, Rectangle shape, Transform transform,
        PhysicsMaterial material, GenId entityId, bool isKinematic, bool isTrigger, bool rotationalPhysics, ref GenId genId
    )
    {
        GenIdResult result = EntityRegistry.Allocate(state.Entities, ref genId);
        if(result != GenIdResult.Ok)
        {
            return result;
        }

        int physicsBodyIndex = GetPhysicsBodyIndex(genId);
        state.AlloctedPhysicsBodyCount++;
        
        // handle flags.
        state.Flags[physicsBodyIndex] |= PhysicsBodyFlags.RectangleShape;
        SetActiveUnsafe(state, physicsBodyIndex, true);
        SetAllocatedUnsafe(state, physicsBodyIndex, true);
        SetRigidBodyUnsafe(state, physicsBodyIndex, true);
        SetTriggerUnsafe(state, physicsBodyIndex, isTrigger);
        SetKinematicUnsafe(state, physicsBodyIndex, isKinematic);
        SetRotationalPhysicsUnsafe(state, physicsBodyIndex, rotationalPhysics);

        // apply data.
        PolygonRectangle polyRect = new(shape);
        PhysicsSystem.AddLocalVertices(state, PolygonRectangle.VerticesXAsSpan(polyRect), PolygonRectangle.VerticesYAsSpan(polyRect), out int verticesFirstIndex, out int verticeCount);
        SetTransformUnsafe(state, physicsBodyIndex, transform);
        state.LocalHeights[physicsBodyIndex] = shape.Height;
        state.LocalWidths[physicsBodyIndex] = shape.Width;
        state.EntityIds[physicsBodyIndex] = entityId;
        Soa_PhysicsMaterial.Insert(state.PhysicsMaterials, material.StaticFriction, material.KineticFriction, material.Density, 
            material.Restitution, physicsBodyIndex
        );

        // reset forces.
        PhysicsSystem.ClearForcesAndVelocities(state, physicsBodyIndex);

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
    public static float CalculateRectangleMass(float width, float height, float density)
    {
        return Rectangle.GetArea(width, height) * density;
    } 

    /// <summary>
    /// Vectorised mass calculation for rectangles.
    /// </summary>
    /// <param name="width">the widths of the shapes.</param>
    /// <param name="height">the heights of the shapes.</param>
    /// <param name="density">the densities of the shapes.</param>
    /// <returns>the mass values of the shapes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector<float> CalculateRectangleMass(Vector<float> width, Vector<float> height, Vector<float> density)
    {
        return Rectangle.GetArea(width, height) * density;
    }

    /// <summary>
    /// Calculates the rotational inertia of a rectangle.
    /// </summary>
    /// <param name="width">the width of the shape.</param>
    /// <param name="height">the height of the shape.</param>
    /// <param name="mass">the mass of the shape.</param>
    /// <returns>the rotational inertia value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float CalculateRectangleRotationalInertia(float width, float height, float mass)
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
    public static Vector<float> CalculateRectangleRotationalInertia(Vector<float> width, Vector<float> height, Vector<float> mass)
    {
        return PhysicsSystem.VectorRectangleRotationalInertia * mass * ((width * width) + (height * height));
    }

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
    ///         <item><see cref="GenIdResult.StaleGenId"/></item>
    ///     </list>
    /// </returns>
    public static GenIdResult ImpulseForce(PhysicsSystemState state, Math.Vector2 force, GenId genId)
    {
        if(EntityRegistry.IsGenIdStale(state.Entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        int index = GenId.GetIndex(genId);
        ref PhysicsBodyFlags flag = ref state.Flags[index];
        
        if((flag & PhysicsBodyFlags.Allocated) == 0 || (flag & PhysicsBodyFlags.RigidBody) == 0)
        {
            return GenIdResult.NotAllocated;   
        }

        if((flag & PhysicsBodyFlags.Active) == 0)
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

}