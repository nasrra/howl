using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;
using static Howl.Physics.SoaPhysicsSystem;

namespace Howl.Physics;

public static class PhysicsBody
{



    
    /*******************
    
        Setters & Getters.
    
    ********************/
    // (todo): write generation checks for getting and setting data.




    /// <summary>
    /// Gets the static friction value of a physics body.
    /// </summary>
    /// <param name="state">the physics system state to query.</param>
    /// <param name="genIndex">the gen index of the physics body.</param>
    /// <returns>a copy of the static friction value.</returns>
    public static float GetStaticFriction(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return state.PhysicsMaterials.StaticFriction[genIndex.Index];
    }

    /// <summary>
    /// Gets the kinematic friction value of a physics body.
    /// </summary>
    /// <param name="state">the physics system state to query.</param>
    /// <param name="genIndex">the gen index of the physics body.</param>
    /// <returns>a copy of the kinematic friction value.</returns>
    public static float GetKinematicFriction(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return state.PhysicsMaterials.KineticFriction[genIndex.Index];
    }

    /// <summary>
    /// Gets the density value of a physics body.
    /// </summary>
    /// <param name="state">the physics system state to query.</param>
    /// <param name="genIndex">the gen index of the physics body.</param>
    /// <returns>a copy of the density value.</returns>
    public static float GetDensity(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return state.PhysicsMaterials.Density[genIndex.Index];
    }

    /// <summary>
    /// Gets the restituion value of a physics body.
    /// </summary>
    /// <param name="state">the physics system state to query.</param>
    /// <param name="genIndex">the gen index of the physics body.</param>
    /// <returns>a copy of the restitution value.</returns>
    public static float GetRestitution(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return state.PhysicsMaterials.Restitution[genIndex.Index];
    }

    /// <summary>
    /// Sets whether or not a physics body is active within a physics simulation.
    /// </summary>
    /// <param name="flags">a reference to the flags to mutate.</param>
    /// <param name="isActive">true, to set to active; otherwise false for inactive.</param>
    public static void SetActive(ref PhysicsBodyFlags flags, bool isActive)
    {
        if (isActive)
        {
            flags |= PhysicsBodyFlags.Active;
        }
        else
        {
            flags &= ~PhysicsBodyFlags.Active;
        }
    }

    /// <summary>
    /// Gets whether or not physics body is active within the physics simulation.
    /// </summary>
    /// <param name="state">the physics state storing the body to check.</param>
    /// <param name="genIndex">the gen index used to look up the body.</param>
    /// <returns>true, if the physics body is active; otherwise false.</returns>
    public static bool IsActive(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.Active) != 0;
    }

    /// <summary>
    /// Sets whether or not a physics body has collision resolution.
    /// </summary>
    /// <param name="flags">a reference to the flags to mutate.</param>
    /// <param name="isKinematic">true, to enable kinematic behaviour; otherwise false.</param>
    public static void SetKinematic(ref PhysicsBodyFlags flags, bool isKinematic)
    {
        if (isKinematic)
        {
            flags |= PhysicsBodyFlags.Kinematic;
        }
        else
        {
            flags &= ~PhysicsBodyFlags.Kinematic;            
        }        
    }

    /// <summary>
    /// Gets whether or not a physics body is of the behavioural mode 'kinematic'.
    /// </summary>
    /// <param name="state">the physics system state storing the body to check.</param>
    /// <param name="genIndex">the gen index used to look up the body.</param>
    /// <returns>true, if the physics body is of the behavioural mode 'kinematic'; otherwise false.</returns>
    public static bool IsKinematic(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.Kinematic) != 0;
    }

    /// <summary>
    /// Sets whether or not a physics body slot has been allcoated in a physics system.
    /// </summary>
    /// <remarks>
    /// Note: this flag indicates whether or not a slot in a physics body array is free and available for reuse.
    /// </remarks>
    /// <param name="flags">a reference to the flags to mutate.</param>
    /// <param name="isAllocated">true, to set to allocated; otherwise false.</param>
    public static void SetAllocated(ref PhysicsBodyFlags flags, bool isAllocated)
    {
        if (isAllocated)
        {
            flags |= PhysicsBodyFlags.Allocated;
        }
        else
        {
            flags &= ~PhysicsBodyFlags.Allocated;
        }                
    }

    /// <summary>
    /// Gets whether or not a physics body slot has been allocated data in the physics system.
    /// </summary>
    /// <param name="state">the physics system state storing the body to check.</param>
    /// <param name="genIndex">the gen index used to look up the body.</param>
    /// <returns>true, if the physics body slot has been allocated data; otherwise false.</returns>
    public static bool IsAllocated(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.Allocated) != 0;
    }

    /// <summary>
    /// Sets whether or not a physics body should respond to collisions by recording the intersection of a colliding object.
    /// </summary>
    /// <param name="flags">a reference to the flags to mutate.</param>
    /// <param name="isTrigger">true, to enable trigger behaviour; otherwise false.</param>
    public static void SetTrigger(ref PhysicsBodyFlags flags, bool isTrigger)
    {
        if (isTrigger)
        {
            flags |= PhysicsBodyFlags.Trigger;
        }
        else
        {
            flags &= ~PhysicsBodyFlags.Trigger;
        }        
    }

    /// <summary>
    /// Gets whether or not a physics body is of the collider mode 'trigger' 
    /// </summary>
    /// <param name="state">the physics system state storing the body to check.</param>
    /// <param name="genIndex">the gen index used to look up the body.</param>
    /// <returns>true, if the collider mode is 'trigger'; otherwise false.</returns>
    public static bool IsTrigger(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.Trigger) != 0;        
    }

    /// <summary>
    /// Gets whether or not a physics body uses rotational physics.
    /// </summary>
    /// <param name="state">the physics system state to query.</param>
    /// <param name="genIndex">the gen index of the physics body.</param>
    /// <returns>true, if the physics body is using rotational physics; otherwise false.</returns>
    public static bool UsesRotationalPhysics(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.RotationalPhysics) != 0;
    }

    /// <summary>
    /// Sets whether or not a physics body is a rigidbody.
    /// </summary>
    /// <param name="flags">a reference to the flags to mutate.</param>
    /// <param name="hasRigidBody">true to enable rigidbody behaviour; otherwise false.</param>
    public static void SetRigidBody(ref PhysicsBodyFlags flags, bool hasRigidBody)
    {
        if (hasRigidBody)
        {
            flags |= PhysicsBodyFlags.RigidBody;
        }
        else
        {
            flags &= ~PhysicsBodyFlags.RigidBody;
        }        
    }

    /// <summary>
    /// Gets whether or not a physics body has a rigidbody.
    /// </summary>
    /// <param name="state">the physics system state storing the body to check.</param>
    /// <param name="genIndex">the gen index used to look up the body.</param>
    /// <returns>true, if the body has a rigidbody; otherwise false.</returns>
    public static bool HasRigidBody(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.RigidBody) != 0;
    }

    public static void SetRotationalPhysics(ref PhysicsBodyFlags flags, bool enabled)
    {
        if (enabled)
        {
            flags |= PhysicsBodyFlags.RotationalPhysics;
        }
        else
        {
            flags &= ~PhysicsBodyFlags.RotationalPhysics;
        }
    }

    /// <summary>
    /// Sets the transform of a body in the physics simulation.
    /// </summary>
    /// <param name="soaTransform">the structure-of-array transforms to mutate and store the specified transform in.</param>
    /// <param name="generation">the generation values of each soa transform entry.</param>
    /// <param name="genIndex">the gen index used to look up the slot in the soa transform collection to insert the specified transform data.</param>
    /// <param name="transform">the specified transform data.</param>
    /// <returns>true; if successfully set; otherwise false.</returns>
    public static bool SetTransform(Soa_Transform soaTransform, Span<int> generation, GenIndex genIndex, Transform transform)
    {
        if(generation[genIndex.Index] != genIndex.Generation)
            return false;

        soaTransform.Position.X[genIndex.Index]  = transform.Position.X;
        soaTransform.Position.Y[genIndex.Index]  = transform.Position.Y;
        soaTransform.Scale.X[genIndex.Index]     = transform.Scale.X;
        soaTransform.Scale.Y[genIndex.Index]     = transform.Scale.Y;
        soaTransform.Cos[genIndex.Index]         = transform.Cos;
        soaTransform.Sin[genIndex.Index]         = transform.Sin;

        return true;
    }




    /*******************
    
        Circle Body.
    
    ********************/




    /// <summary>
    /// Allocates a circle collider into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateCircleCollider(SoaPhysicsSystemState state, Circle shape, Transform transform, 
        bool isKinematic, bool isTrigger, ref GenIndex genIndex
    )
    {
        int index = state.FreePhysicsBodyIndex.Pop();
        state.AlloctedPhysicsBodyCount++;
        genIndex.Index = index;
        genIndex.Generation = state.Generations[index];

        // handle flags.
        // note: no circle shape is needed to be set as it is implied by the system that when a shape is not
        // set, a physics body is a circle.
        PhysicsBodyFlags flag = PhysicsBodyFlags.None; 
        SetActive(ref flag, true);
        SetAllocated(ref flag, true);
        SetRigidBody(ref flag, false);
        SetTrigger(ref flag, isTrigger);
        SetKinematic(ref flag, isKinematic);


        // apply data.
        AddVertices(state, [shape.X], [shape.Y], out int verticesFirstIndex, out int verticeCount);
        SetTransform(state.Transforms, state.Generations, genIndex, transform);
        state.LocalRadii[index]             = shape.Radius;
        state.Flags[index]                  = flag;
        state.FirstVertexIndices[index]     = verticesFirstIndex;
    }

    /// <summary>
    /// Allocates a circle rigidbody into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="physicsMaterial">the physics material to apply to the physics body.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateCircleRigidBody(SoaPhysicsSystemState state, Circle shape, PhysicsMaterial physicsMaterial, Transform transform,
        bool isKinematic, bool isTrigger, bool rotationalPhysics, ref GenIndex genIndex
    )
    {
        int index = state.FreePhysicsBodyIndex.Pop();
        genIndex = new(index, state.Generations[index]);
        state.AlloctedPhysicsBodyCount++;

        // handle flags.
        // note: no circle shape is needed to be set as it is implied by the system that when a shape is not
        // set, a physics body is a circle.
        PhysicsBodyFlags flags = PhysicsBodyFlags.None; 
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);
        SetRotationalPhysics(ref flags, rotationalPhysics);

        // apply data.
        AddVertices(state, [shape.X], [shape.Y], out int verticesFirstIndex, out int verticeCount);
        SetTransform(state.Transforms, state.Generations, genIndex, transform);
        Soa_PhysicsMaterial.SetPhysicsMaterial(state.PhysicsMaterials, physicsMaterial, index);
        state.LocalRadii[index]                      = shape.Radius;
        state.Flags[index]                      = flags;
        state.FirstVertexIndices[index]         = verticesFirstIndex;

        // reset forces
        ClearForcesAndVelocities(state, index);
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
        return CircleRotationalInertia * mass * (radius * radius);
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
        return VectorCircleRotationalInertia * mass * (radius * radius);
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
    /// Allocates a rectangle collider into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateRectangleCollider(SoaPhysicsSystemState state, Rectangle shape, Transform transform, 
        bool isKinematic, bool isTrigger, ref GenIndex genIndex
    )
    {
        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        genIndex = new(bodyIndex, state.Generations[bodyIndex]);    
        state.AlloctedPhysicsBodyCount++;

        // handle flags.
        PhysicsBodyFlags flags = PhysicsBodyFlags.RectangleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        // apply data.

        PolygonRectangle polyRect = new(shape);
        AddVertices(state, PolygonRectangle.VerticesXAsSpan(polyRect), PolygonRectangle.VerticesYAsSpan(polyRect), out int verticesFirstIndex, out int verticeCount);
        SetTransform(state.Transforms, state.Generations, genIndex, transform);
        state.LocalHeights[bodyIndex]            = shape.Height;
        state.LocalWidths[bodyIndex]             = shape.Width;
        state.Flags[bodyIndex]              = flags;
        state.FirstVertexIndices[bodyIndex]  = verticesFirstIndex;

    }

    /// <summary>
    /// Allocates a rectangle rigidbody into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="physicsMaterial">the physics material to apply to the physics body.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateRectangleRigidBody(SoaPhysicsSystemState state, Rectangle shape, PhysicsMaterial physicsMaterial, Transform transform, 
        bool isKinematic, bool isTrigger, bool rotationalPhysics, ref GenIndex genIndex
    )
    {
        int index = state.FreePhysicsBodyIndex.Pop();
        genIndex = new(index, state.Generations[index]);
        state.AlloctedPhysicsBodyCount++;
        
        // handle flags.
        PhysicsBodyFlags flags = PhysicsBodyFlags.RectangleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);
        SetRotationalPhysics(ref flags, rotationalPhysics);

        // apply data.
        PolygonRectangle polyRect = new(shape);
        Soa_PhysicsMaterial.SetPhysicsMaterial(state.PhysicsMaterials, physicsMaterial, index);
        AddVertices(state, PolygonRectangle.VerticesXAsSpan(polyRect), PolygonRectangle.VerticesYAsSpan(polyRect), out int verticesFirstIndex, out int verticeCount);
        SetTransform(state.Transforms, state.Generations, genIndex, transform);
        state.LocalHeights[index]                    = shape.Height;
        state.LocalWidths[index]                     = shape.Width;
        state.Flags[index]                      = flags;
        state.FirstVertexIndices[index]         = verticesFirstIndex;

        // reset forces.
        ClearForcesAndVelocities(state, index);
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
        return RectangleRotationalInertia * mass * ((width * width) + (height * height));
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
        return VectorRectangleRotationalInertia * mass * ((width * width) + (height * height));
    }


}