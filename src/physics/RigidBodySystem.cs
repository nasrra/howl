using System;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;

namespace Howl.Physics;

public static class RigidBodySystems
{
    public static FixedUpdateSystem MovementSystem(ComponentRegistry componentRegistry, RigidbodySystemState state)
    => deltaTime =>
    {
        GenIndexList<RigidBody> rigidbodies = componentRegistry.Get<RigidBody>();
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>(); 

        Span<DenseEntry<RigidBody>> denseEntries = rigidbodies.GetDenseAsSpan();

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
            rigidbody.ClearForces();
        } 
    };

    public static FixedUpdateSystem ResolveCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    => deltaTime =>
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
    };
}