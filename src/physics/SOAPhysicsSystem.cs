using System;
using System.Linq;
using Howl.ECS;
using Howl.Math.Shapes;
using static Howl.Math.Shapes.PolygonRectangle;

namespace Howl.Physics;

public static class SOAPhysicsSystem
{




    /*******************
    
        Utility.
    
    ********************/




    public static int AddVertices(SOAPhysicsSystemState state, Span<float> verticesX, Span<float> verticesY, out int firstIndex, out int vertexCount)
    {
        if(verticesX.Length != verticesY.Length)
        {
            throw new ArgumentException($"vertices X length '{verticesX.Length}' must be equalt to vertices Y length '{verticesY.Length}'");
        }

        vertexCount = verticesX.Length;

        // set the first index.
        firstIndex = state.FreeVertexIndex.Pop();
        int previousIndex = -1;
        int index = firstIndex;
        state.VerticeX[index] = verticesX[0];
        state.VerticeY[index] = verticesY[0];

        // add the rest of them.
        for(int i = 1; i < vertexCount; i++)
        {
            previousIndex = index;
            index = state.FreeVertexIndex.Pop();
            state.VerticeX[index] = verticesX[i];
            state.VerticeY[index] = verticesY[i];
            state.NextVertice[previousIndex] = index;
        }

        return firstIndex;
    }




    /*******************
    
        Setters & Getters.
    
    ********************/




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
    public static bool IsActive(SOAPhysicsSystemState state, GenIndex genIndex)
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
    public static bool IsKinematic(SOAPhysicsSystemState state, GenIndex genIndex)
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
    public static bool IsAllocated(SOAPhysicsSystemState state, GenIndex genIndex)
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
    public static bool IsTrigger(SOAPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.Trigger) != 0;        
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
    public static bool HasRigidBody(SOAPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.RigidBody) != 0;
    }

    /// <summary>
    /// Sets whether or not a physics body has a physics material.
    /// </summary>
    /// <param name="flags">a reference to the flags to mutate.</param>
    /// <param name="hasPhysicsMaterial">true, to enable physics material behaviour; otherwise false.</param>
    public static void SetHasPhysicsMaterial(ref PhysicsBodyFlags flags, bool hasPhysicsMaterial)
    {
        if (hasPhysicsMaterial)
        {
            flags |= PhysicsBodyFlags.HasPhysicsMaterial;
        }
        else
        {
            flags &= ~PhysicsBodyFlags.HasPhysicsMaterial;
        }
    }

    /// <summary>
    /// Gets whether or not a physics body has a physics material.
    /// </summary>
    /// <param name="state">the physics system state storing the body to check.</param>
    /// <param name="genIndex">the gen index used to look up the body.</param>
    /// <returns>true, if the body has a physics material; otherwise false.</returns>
    public static bool HasPhysicsMaterial(SOAPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.HasPhysicsMaterial) != 0;
    }




    /*******************
    
        Circle.
    
    ********************/




    /// <summary>
    /// Allocates a circle collider into a phsyics system state.
    /// </summary>
    /// <param name="state">The physics system state to allocate into.</param>
    /// <param name="shape">the shape data of the circle.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateCircleCollider(SOAPhysicsSystemState state, in Circle shape, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.CircleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, false);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        // apply data.
        int index = state.FreePhysicsBodyIndex.Pop();

        state.Radius[index]     = shape.Radius;
        state.VerticeX[index]   = shape.X;
        state.VerticeY[index]   = shape.Y;
        state.Flags[index]      = flags;

        // return gen index.

        genIndex = new(index, state.Generation[index]);

        state.AlloctedPhysicsBodyCount++;
    }

    /// <summary>
    /// Allocates a circle rigidbody - without friction - into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the shape data of the circle.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateCircleRigidBody(SOAPhysicsSystemState state, in Circle shape, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.CircleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        // apply data.
        int index = state.FreePhysicsBodyIndex.Pop();

        state.Radius[index]     = shape.Radius;
        state.VerticeX[index]   = shape.X;
        state.VerticeY[index]   = shape.Y;
        state.Flags[index]      = flags;

        // return gen index.

        genIndex = new(index, state.Generation[index]);

        state.AlloctedPhysicsBodyCount++;
    }

    /// <summary>
    /// Allocates a circle rigidbody into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the shape data of the circle.</param>
    /// <param name="physicsMaterial">the physics material to apply to the physics body.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateCircleRigidBody(SOAPhysicsSystemState state, in Circle shape, in PhysicsMaterial physicsMaterial, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.CircleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, true);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        // apply data.
        int index = state.FreePhysicsBodyIndex.Pop();

        state.Radius[index]             = shape.Radius;
        state.VerticeX[index]           = shape.X;
        state.VerticeY[index]           = shape.Y;
        state.Flags[index]              = flags;
        state.StaticFriction[index]     = physicsMaterial.StaticFriction;
        state.KineticFriction[index]    = physicsMaterial.KineticFriction;

        // return gen index.

        genIndex = new(index, state.Generation[index]);        
        
        state.AlloctedPhysicsBodyCount++;
    }




    /*******************
    
        Rectangle.
    
    ********************/




    /// <summary>
    /// Allocates a rectangle collider into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate a physics body into.</param>
    /// <param name="shape">the rectangle shape data of the physics body.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateRectangleCollider(SOAPhysicsSystemState state, in Rectangle shape, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.PolygonShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, false);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticeFirstIndex, out int verticeCount);

        state.Height[bodyIndex]         = shape.Height;
        state.Width[bodyIndex]          = shape.Width;
        state.Flags[bodyIndex]          = flags;
        state.FirstVertice[bodyIndex]   = verticeFirstIndex;
        state.VerticeCount[bodyIndex]   = verticeCount;

        // return gen index.

        genIndex = new(bodyIndex, state.Generation[bodyIndex]);    

        state.AlloctedPhysicsBodyCount++;
    }

    /// <summary>
    /// Allocates a rectangle rigidbody - without a physics material - into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate a physics body into.</param>
    /// <param name="shape">the rectangle shape data of the physics body.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateRectangleRigidBody(SOAPhysicsSystemState state, in Rectangle shape, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.PolygonShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);
    
        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticeFirstIndex, out int verticeCount);

        state.Height[bodyIndex]         = shape.Height;
        state.Width[bodyIndex]          = shape.Width;
        state.Flags[bodyIndex]          = flags;
        state.FirstVertice[bodyIndex]   = verticeFirstIndex;
        state.VerticeCount[bodyIndex]   = verticeCount;

        // return gen index.

        genIndex = new(bodyIndex, state.Generation[bodyIndex]);
        
        state.AlloctedPhysicsBodyCount++;
    }

    /// <summary>
    /// Allocates a rectangle rigidbody into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate a physics body into.</param>
    /// <param name="shape">the rectangle shape data of the physics body.</param>
    /// <param name="physicsMaterial">the physics material to apply to the physics body.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateRectangleRigidBody(SOAPhysicsSystemState state, in Rectangle shape, PhysicsMaterial physicsMaterial, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.PolygonShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, true);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);
    
        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticeFirstIndex, out int verticeCount);

        state.Height[bodyIndex]             = shape.Height;
        state.Width[bodyIndex]              = shape.Width;
        state.Flags[bodyIndex]              = flags;
        state.FirstVertice[bodyIndex]       = verticeFirstIndex;
        state.VerticeCount[bodyIndex]       = verticeCount;
        state.KineticFriction[bodyIndex]    = physicsMaterial.KineticFriction;
        state.StaticFriction[bodyIndex]     = physicsMaterial.StaticFriction;

        // return gen index.

        genIndex = new(bodyIndex, state.Generation[bodyIndex]);
    
        state.AlloctedPhysicsBodyCount++;
    }
}