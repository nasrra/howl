using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;

namespace Howl.Physics;

public static class RigidBodySystem
{
    /// <summary>
    /// Registers all necesarry components for this system.
    /// </summary>
    /// <param name="componentRegistry">The component registery to register to.</param>
    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.RegisterComponent<RigidBody>();
    }

    /// <summary>
    /// The movement step for this rigid body system.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <param name="deltaTime"></param>
    /// <exception cref="DenseNotAllocatedException"></exception>
    /// <exception cref="StaleGenIndexException"></exception>
    public static void MovementStep(ComponentRegistry componentRegistry, RigidbodySystemState state, float deltaTime)
    {        
        GenIndexList<RigidBody> rigidbodies = componentRegistry.Get<RigidBody>();
        Span<DenseEntry<RigidBody>> denseEntries = rigidbodies.GetDenseAsSpan();

        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>(); 

        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RigidBody> denseEntry = ref denseEntries[i];
            ref RigidBody rigidbody = ref denseEntry.Value;
            rigidbodies.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the rigid body has a transform component.
            if(transforms.GetDenseRef(genIndex, out Ref<Transform> transformRef).Fail(out var result))
            {
                Debug.Assert(false, $"{result}");
                continue;
            }

            if(rigidbody.Mode == RigidBodyMode.Dynamic)
            {
                // apply gravity.
                rigidbody.ImpulseForce(state.GravityDirection * state.Gravity * deltaTime);
            }


            // force = mass * acceleration.
            // acceleration = force / mass.

            rigidbody.ImpulseForce(rigidbody.Force / rigidbody.Mass * deltaTime);
            transformRef.Value.Position += rigidbody.LinearVelocity * deltaTime;
            transformRef.Value.Rotation += rigidbody.RotationalVelocity * deltaTime;
        } 
    }

    /// <summary>
    /// Clears applied force of all rigidbodies.
    /// </summary>
    /// <param name="componentRegistry"></param>
    public static void ClearForces(ComponentRegistry componentRegistry)
    {
        GenIndexList<RigidBody> rigidbodies = componentRegistry.Get<RigidBody>();
        Span<DenseEntry<RigidBody>> denseEntries = rigidbodies.GetDenseAsSpan();
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RigidBody> denseEntry = ref denseEntries[i];
            ref RigidBody rigidbody = ref denseEntry.Value;
            rigidbody.ClearForces();
        }        
    }

    /// <summary>
    /// The collision resolution step for this rigibody system.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <param name="deltaTime"></param>
    public static void ResolveCollisionsStep(ComponentRegistry componentRegistry, CollisionSystemState state, float deltaTime)
    {        
        GenIndexList<RigidBody> rigidbodies = componentRegistry.Get<RigidBody>();
        ReadOnlySpan<Collision> collisions = state.CollisionManifold.GetCollisionsAsReadOnlySpan();

        for(int i = 0; i < collisions.Length; i+=2) // NOTE: increment by two as collisions are stored as siblings before the collision manifold is sorted.
        {            
            if(rigidbodies.GetDenseRef(collisions[i].Owner, out Ref<RigidBody> rigidbodyARef) != GenIndexResult.Ok)
            {
                continue;
            }

            if(rigidbodies.GetDenseRef(collisions[i].Other, out Ref<RigidBody> rigidbodyBRef) != GenIndexResult.Ok)
            {
                continue;
            }

            ref RigidBody rigidbodyA = ref rigidbodyARef.Value;
            ref RigidBody rigidbodyB = ref rigidbodyBRef.Value;

            Vector2 relativeVelocity = rigidbodyB.LinearVelocity - rigidbodyA.LinearVelocity;

            float relative = Vector2.Dot(relativeVelocity, collisions[i].Normal);

            if(relative > 0)
            {
                continue;
            }

            float e = MathF.Min(rigidbodyA.Restitution, rigidbodyB.Restitution);

            float j = -(1f + e) * relative;
            j /= (1f/rigidbodyA.Mass) + (1f/rigidbodyB.Mass);

            if(rigidbodyA.Mode == RigidBodyMode.Dynamic)
            {
                rigidbodyA.ImpulseForce(-(j / rigidbodyA.Mass * collisions[i].Normal));            
            }

            if(rigidbodyB.Mode == RigidBodyMode.Dynamic)
            {
                rigidbodyB.ImpulseForce(j / rigidbodyB.Mass * collisions[i].Normal);
            }
        }
    }
}