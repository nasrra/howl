using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;
using Howl.Math.Shapes;

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
    public static void MovementStep(ComponentRegistry componentRegistry, RigidBodySystemState state, float deltaTime)
    {        
        GenIndexList<RigidBody> rigidbodies = componentRegistry.Get<RigidBody>();
        Span<DenseEntry<RigidBody>> denseEntries = rigidbodies.GetDenseAsSpan();

        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();

        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        GenIndexList<RectangleCollider> rectangleColliders = componentRegistry.Get<RectangleCollider>();

        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RigidBody> denseEntry = ref denseEntries[i];
            ref RigidBody rigidbody = ref denseEntry.Value;
            rigidbodies.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the rigid body has a transform component.
            if(transforms.GetDenseRef(genIndex, out Ref<Transform> transformRef).Fail())
            {
                System.Diagnostics.Debug.Assert(false);
                continue;
            }

            ref Transform transform = ref transformRef.Value;

            if(circleColliders.GetDenseRef(genIndex, out Ref<CircleCollider> circleCollider).Ok())
            {
                rigidbody.SetShape(circleCollider.Value.Shape.Scale(transform.Scale));
            }
            else if(rectangleColliders.GetDenseRef(genIndex, out Ref<RectangleCollider> rectangleCollider).Ok())
            {
                rigidbody.SetShape(rectangleCollider.Value.Shape.Scale(transform.Scale));   
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "rigidbody must have a collider!");
                continue;
            }

            if(rigidbody.Mode == RigidBodyMode.Dynamic)
            {
                // apply gravity.
                rigidbody.ImpulseLinearForce(state.GravityDirection * state.Gravity * deltaTime);
            }


            // force = mass * acceleration.
            // acceleration = force / mass.

            rigidbody.ImpulseLinearForce(rigidbody.Force / rigidbody.Mass * deltaTime);
            transformRef.Value.Position += rigidbody.LinearVelocity * deltaTime;
            transformRef.Value.Rotation += rigidbody.AngularVelocity * deltaTime;
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
    /// The collision resolution step.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <param name="deltaTime"></param>
    public static void ResolveCollisionsStep(ComponentRegistry componentRegistry, CollisionSystemState state, float deltaTime)
    {        
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<RigidBody> rigidbodies = componentRegistry.Get<RigidBody>();
        ReadOnlySpan<Collision> collisions = state.CollisionManifold.GetCollisionsAsReadOnlySpan();

        for(int i = 0; i < collisions.Length; i+=2) // NOTE: increment by two as collisions are stored as siblings before the collision manifold is sorted.
        {            
            // get the two rigidbodies.
            if(rigidbodies.GetDenseRef(collisions[i].Owner, out Ref<RigidBody> rigidbodyARef).Fail()
            || rigidbodies.GetDenseRef(collisions[i].Other, out Ref<RigidBody> rigidbodyBRef).Fail())
            {
                System.Diagnostics.Debug.Assert(false);
                continue;
            }

            ref readonly Collision collision = ref collisions[i];
            ref RigidBody rigidbodyA = ref rigidbodyARef.Value;
            ref RigidBody rigidbodyB = ref rigidbodyBRef.Value;

            if(rigidbodyA.RotationalPhysics == true || rigidbodyB.RotationalPhysics == true)
            {
                ResolveCollisionRotational(transforms, in collision, ref rigidbodyA, ref rigidbodyB);
            }
            else
            {
                ResolveCollisionBasic(in collision, ref rigidbodyA, ref rigidbodyB);                
            }
        }
    }


    /// <summary>
    /// Resolves a collision between two rigidbodies.
    /// </summary>
    /// <param name="collision">the collision data.</param>
    /// <param name="rigidBodyA">the rigidbody of the collision data's owner.</param>
    /// <param name="rigidBodyB">the rigidbody of the collision data's other.</param>
    private static void ResolveCollisionBasic(ref readonly Collision collision, ref RigidBody rigidBodyA, ref RigidBody rigidBodyB)
    {
        Vector2 relativeVelocity = rigidBodyB.LinearVelocity - rigidBodyA.LinearVelocity;

        // the magnitude of the relative velocity relative to the normal
        float magnitude = Vector2.Dot(relativeVelocity, collision.Normal);

        if(magnitude > 0)
        {
            return;
        }

        float restitution = MathF.Min(rigidBodyA.Restitution, rigidBodyB.Restitution);

        // magnitude of the impulse
        float impulseMagnitude = -(1f + restitution) * magnitude;
        impulseMagnitude /= rigidBodyA.InverseMass + rigidBodyB.InverseMass;

        if(rigidBodyA.Mode == RigidBodyMode.Dynamic)
        {
            rigidBodyA.ImpulseLinearForce(-(impulseMagnitude / rigidBodyA.Mass * collision.Normal));            
        }

        if(rigidBodyB.Mode == RigidBodyMode.Dynamic)
        {
            rigidBodyB.ImpulseLinearForce(impulseMagnitude / rigidBodyB.Mass * collision.Normal);
        }
    } 

    /// <summary>
    /// Resolves a collision between two rigidbodies with rotational physics.
    /// </summary>
    /// <param name="transforms">the transform components in the rigidbodies component registry.</param>
    /// <param name="collision">the collision data.</param>
    /// <param name="rigidBodyA">the rigidbody of the 'owner' in the collision data.</param>
    /// <param name="rigidBodyB">the rigidbody of the 'other' in the collision data.</param>
    public static void ResolveCollisionRotational(
        GenIndexList<Transform> transforms,
        ref readonly Collision collision,
        ref RigidBody rigidBodyA,
        ref RigidBody rigidBodyB
    )
    {

        // ensure they both have transform components.
        if(transforms.GetDenseRef(collision.Owner, out Ref<Transform> transformARef).Fail()
        || transforms.GetDenseRef(collision.Other, out Ref<Transform> transformBRef).Fail())
        {
            System.Diagnostics.Debug.Assert(false);
            return;
        }
        ref Transform transformA = ref transformARef.Value;
        ref Transform transformB = ref transformBRef.Value;                

        float restitution = MathF.Min(rigidBodyA.Restitution, rigidBodyB.Restitution);

        ReadOnlySpan<float> contactPointX = collision.GetXContactPointsAsReadOnlySpan();
        ReadOnlySpan<float> contactPointY = collision.GetYContactPointsAsReadOnlySpan();

        Span<Vector2> impulse = stackalloc Vector2[collision.ContactPointsCount];
        Span<Vector2> distA   = stackalloc Vector2[collision.ContactPointsCount];
        Span<Vector2> distB   = stackalloc Vector2[collision.ContactPointsCount];
        
        for(int j = 0; j < collision.ContactPointsCount; j++)
        {
            Vector2 contactPoint = new Vector2(contactPointX[j], contactPointY[j]);
            
            // get the angular velocity to travel in.
            distA[j] = contactPoint - collision.OwnerColliderShapeCenter.Transform(transformA);
            distB[j] = contactPoint - collision.OtherColliderShapeCenter.Transform(transformB);
            Vector2 perpendicularA = new Vector2(-distA[j].Y, distA[j].X);
            Vector2 perpendicularB = new Vector2(-distB[j].Y, distB[j].X);
            Vector2 angularLinearVelocityA = perpendicularA * rigidBodyA.AngularVelocity; 
            Vector2 angularLinearVelocityB = perpendicularB * rigidBodyB.AngularVelocity; 

            Vector2 relativeVelocity = 
            (rigidBodyB.LinearVelocity + angularLinearVelocityB) - 
            (rigidBodyA.LinearVelocity + angularLinearVelocityA);
            
            // the magnitude of the relative velocity relative to the normal
            float magnitude = Vector2.Dot(relativeVelocity, collision.Normal);

            if(magnitude > 0)
            {
                continue;
            }

            // calculate the denominator.
            float perpADotNormal = perpendicularA.Dot(collision.Normal);
            float perpBDotNormal = perpendicularB.Dot(collision.Normal);
            float denominator = rigidBodyA.InverseMass + rigidBodyB.InverseMass + 
                (perpADotNormal * perpADotNormal) * rigidBodyA.InverseRotationalInertia +
                (perpBDotNormal * perpBDotNormal) * rigidBodyB.InverseRotationalInertia;

            // magnitude of the impulse
            float impulseMagnitude = -(1f + restitution) * magnitude;
            impulseMagnitude /= denominator;

            // divide by the contact point count to ensure that impulse is evenly spread 
            // across all contact points.
            impulseMagnitude /= (float)collision.ContactPointsCount;

            impulse[j] = impulseMagnitude * collision.Normal;
        }

        for(int j = 0; j < collision.ContactPointsCount; j++)
        {                
            // cross producting the dist and impulse gives a value indicating
            // how much angular velocity - in radians - is needed to be applied based on the impulse direction.
            // this is because cross producting two directions that are parallel to eachother, results in zero.
            // which means that there should be no rotation if the collision is head on.
            // but if the closer the two directions come to being perpendicular to one another,
            // the larger the angular impulse will be, causing the body to rotate.
            if(rigidBodyA.Mode == RigidBodyMode.Dynamic)
            {
                // always apply linear force, even if there is no rotational force to apply.
                rigidBodyA.ImpulseLinearForce(-impulse[j] * rigidBodyA.InverseMass);                

                if(rigidBodyA.RotationalPhysics)
                {
                    rigidBodyA.ImpulseAngularForce(-Vector2.Cross(distA[j], impulse[j]) * rigidBodyA.InverseRotationalInertia);                
                }
            }
            if(rigidBodyB.Mode == RigidBodyMode.Dynamic)
            {
                // always apply linear force, even if there is no rotational force to apply.
                rigidBodyB.ImpulseLinearForce(impulse[j] * rigidBodyB.InverseMass);
                
                if(rigidBodyB.RotationalPhysics)
                {
                    rigidBodyB.ImpulseAngularForce(Vector2.Cross(distB[j], impulse[j]) * rigidBodyB.InverseRotationalInertia);                
                }                
            }
        
        }
    }
}