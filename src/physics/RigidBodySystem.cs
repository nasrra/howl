using System;
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
    /// Creates a new movement step system instance.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static FixedUpdateSystem MovementSystem(ComponentRegistry componentRegistry, RigidbodySystemState state)
    => deltaTime =>
    {
        MovementStep(componentRegistry, state, deltaTime);
        ClearForces(componentRegistry);
    };

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
            switch (transforms.GetDenseRef(genIndex, out Ref<Transform> transformRef))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndex);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(genIndex);
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
    /// Creates a new resolve collision step system instance.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static FixedUpdateSystem ResolveCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    => deltaTime =>
    {
        ResolveCollisionsStep(componentRegistry, state, deltaTime);
    };

    /// <summary>
    /// The collision resolution step for this rigibody system.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <param name="deltaTime"></param>
    public static void ResolveCollisionsStep(ComponentRegistry componentRegistry, CollisionSystemState state, float deltaTime)
    {        
        GenIndexList<RigidBody> rigidbodies = componentRegistry.Get<RigidBody>();
        Span<Collision> collisions = CollectionsMarshal.AsSpan(state.CollisionManifold);

        for(int i = 0; i < collisions.Length; i++)
        {
            ref Collision collision = ref collisions[i];
            
            if(rigidbodies.GetDenseRef(collision.ColliderA, out Ref<RigidBody> rigidbodyARef) != GenIndexResult.Success)
            {
                continue;
            }

            if(rigidbodies.GetDenseRef(collision.ColliderB, out Ref<RigidBody> rigidbodyBRef) != GenIndexResult.Success)
            {
                continue;
            }

            ref RigidBody rigidbodyA = ref rigidbodyARef.Value;
            ref RigidBody rigidbodyB = ref rigidbodyBRef.Value;

            Vector2 relativeVelocity = rigidbodyB.LinearVelocity - rigidbodyA.LinearVelocity;

            float relative = Vector2.Dot(relativeVelocity, collision.Normal);

            float e = MathF.Min(rigidbodyA.Restitution, rigidbodyB.Restitution);

            float j = -(1f + e) * relative;
            j /= (1f/rigidbodyA.Mass) + (1f/rigidbodyB.Mass);

            if(rigidbodyA.Mode == RigidBodyMode.Dynamic)
            {
                rigidbodyA.ImpulseForce(-(j / rigidbodyA.Mass * collision.Normal));            
            }

            if(rigidbodyB.Mode == RigidBodyMode.Dynamic)
            {
                rigidbodyB.ImpulseForce(j / rigidbodyB.Mass * collision.Normal);
            }
        }
    }
}