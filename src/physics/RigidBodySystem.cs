using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;
using Howl.Math.Shapes;
using static Howl.ECS.GenIndexListProc;
using static Howl.Math.Shapes.Circle;
using static Howl.Math.Shapes.Rectangle;
using static Howl.Physics.RigidBody;

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
        Span<DenseEntry<RigidBody>> denseEntries = GetDenseAsSpan(rigidbodies);

        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();

        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        GenIndexList<RectangleCollider> rectangleColliders = componentRegistry.Get<RectangleCollider>();

        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RigidBody> denseEntry = ref denseEntries[i];
            ref RigidBody rigidbody = ref denseEntry.Value;
            GetGenIndex(rigidbodies, denseEntry.sparseIndex, out GenIndex genIndex);

            if(GetDenseRef(circleColliders, genIndex, out Ref<CircleCollider> circleCollider).Ok())
            {
                SetShape(ref rigidbody, circleCollider.Value.TransformedShape);
            }
            else if(GetDenseRef(rectangleColliders, genIndex, out Ref<RectangleCollider> rectangleCollider).Ok())
            {
                SetShape(ref rigidbody, rectangleCollider.Value.TransformedShape);   
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "rigidbody must have a collider!");
                continue;
            }

            if(rigidbody.Mode == RigidBodyMode.Dynamic)
            {
                // apply gravity.
                ImpulseLinearForce(ref rigidbody, state.GravityDirection * state.Gravity * deltaTime);
            }


            // force = mass * acceleration.
            // acceleration = force / mass.
            Vector2 force = new Vector2(rigidbody.ForceX, rigidbody.ForceY);
            ImpulseLinearForce(ref rigidbody, force / rigidbody.Mass * deltaTime);

            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail())
            {
                System.Diagnostics.Debug.Assert(false, "all rigidbodies must have a transform component.");
                continue;
            }

            transformRef.Value.Position += new Vector2(rigidbody.LinearVelocityX, rigidbody.LinearVelocityY) * deltaTime;
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
        Span<DenseEntry<RigidBody>> denseEntries = GetDenseAsSpan(rigidbodies);
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RigidBody> denseEntry = ref denseEntries[i];
            ref RigidBody rigidbody = ref denseEntry.Value;
            RigidBody.ClearForces(ref rigidbody);
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
        Span<float> collisionImpulseMagnitudes = stackalloc float[Collision.MaxContactPoints];

        for(int i = 0; i < collisions.Length; i+=2) // NOTE: increment by two as collisions are stored as siblings before the collision manifold is sorted.
        {            
            // get the two rigidbodies.
            if(GetDenseRef(rigidbodies, collisions[i].Owner, out Ref<RigidBody> rigidbodyARef).Fail()
            || GetDenseRef(rigidbodies, collisions[i].Other, out Ref<RigidBody> rigidbodyBRef).Fail())
            {
                System.Diagnostics.Debug.Assert(false);
                continue;
            }

            ref readonly Collision collision = ref collisions[i];
            ref RigidBody rigidbodyA = ref rigidbodyARef.Value;
            ref RigidBody rigidbodyB = ref rigidbodyBRef.Value;

            // friction and rotational resolution are tightly coupled with eachother.
            // do not remove them from eachother.
            if(rigidbodyA.PhysicsMaterial.UseFriction == true || rigidbodyB.PhysicsMaterial.UseFriction == true
            || rigidbodyA.RotationalPhysics == true || rigidbodyB.RotationalPhysics == true)
            {
                // note: order matters here, do collision resolution
                //  first do that the impulse magnitudes span is
                // filled with the correct data to perform friction resolution.
                ResolveCollisionRotational(collisionImpulseMagnitudes, in collision, ref rigidbodyA, ref rigidbodyB);
                ResolveFriction(collisionImpulseMagnitudes, in collision, ref rigidbodyA, ref rigidbodyB);
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
        Vector2 linearVelocityA = new Vector2(rigidBodyA.LinearVelocityX, rigidBodyA.LinearVelocityY);
        Vector2 linearVelocityB = new Vector2(rigidBodyB.LinearVelocityX, rigidBodyB.LinearVelocityY);
        Vector2 relativeVelocity = linearVelocityA - linearVelocityB;

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
            ImpulseLinearForce(ref rigidBodyA, -(impulseMagnitude / rigidBodyA.Mass * collision.Normal));            
        }

        if(rigidBodyB.Mode == RigidBodyMode.Dynamic)
        {
            ImpulseLinearForce(ref rigidBodyB, impulseMagnitude / rigidBodyB.Mass * collision.Normal);
        }
    } 

    /// <summary>
    /// Resolves a collision between two rigidbodies with rotational physics and friction.
    /// </summary>
    /// <remarks>
    /// Note: impulseMagnitudes will be written to dring this function call.
    /// This function also assumes that impulseMagnitudes length will be
    /// able to contain a Collision's max number of contact points.
    /// </remarks>
    /// <param name="impulseMagnitudes">a span that will store the impulse magnitudes this function generates.</param>
    /// <param name="transforms">the transform components in the rigidbodies component registry.</param>
    /// <param name="collision">the collision data.</param>
    /// <param name="rigidBodyA">the rigidbody of the 'owner' in the collision data.</param>
    /// <param name="rigidBodyB">the rigidbody of the 'other' in the collision data.</param>
    private static void ResolveCollisionRotational(
        Span<float> impulseMagnitudes,
        ref readonly Collision collision,
        ref RigidBody rigidBodyA,
        ref RigidBody rigidBodyB
    )
    {             
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
            distA[j] = contactPoint - collision.OwnerColliderShapeCenter;
            distB[j] = contactPoint - collision.OtherColliderShapeCenter;
            Vector2 perpendicularA = new Vector2(-distA[j].Y, distA[j].X);
            Vector2 perpendicularB = new Vector2(-distB[j].Y, distB[j].X);
            Vector2 angularLinearVelocityA = perpendicularA * rigidBodyA.AngularVelocity; 
            Vector2 angularLinearVelocityB = perpendicularB * rigidBodyB.AngularVelocity; 

            Vector2 linearVelcoityA = new Vector2(rigidBodyA.LinearVelocityX, rigidBodyA.LinearVelocityY);
            Vector2 linearVelcoityB = new Vector2(rigidBodyB.LinearVelocityX, rigidBodyB.LinearVelocityY);

            Vector2 relativeVelocity = 
            (linearVelcoityB + angularLinearVelocityB) - 
            (linearVelcoityA + angularLinearVelocityA);
            
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
            
            // save the impulse magnitude for later friction resolution.
            impulseMagnitudes[j] = impulseMagnitude;

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
                ImpulseLinearForce(ref rigidBodyA, -impulse[j] * rigidBodyA.InverseMass);                

                if(rigidBodyA.RotationalPhysics)
                {
                    ImpulseAngularForce(ref rigidBodyA, -Vector2.Cross(distA[j], impulse[j]) * rigidBodyA.InverseRotationalInertia);                
                }
            }
            if(rigidBodyB.Mode == RigidBodyMode.Dynamic)
            {
                // always apply linear force, even if there is no rotational force to apply.
                ImpulseLinearForce(ref rigidBodyB, impulse[j] * rigidBodyB.InverseMass);
                
                if(rigidBodyB.RotationalPhysics)
                {
                    ImpulseAngularForce(ref rigidBodyB, Vector2.Cross(distB[j], impulse[j]) * rigidBodyB.InverseRotationalInertia);                
                }                
            }   
        }
    }

    /// <summary>
    /// Resolves and applies friction between two colliding bodies.
    /// </summary>
    /// <remarks>
    /// Note: this function should be called after resolve collisions rotational.
    /// This is so that the collisionResolutionImpulseMagnitudes span will be populated
    /// with relevate data relating to two bodies after collision resolution.
    /// </remarks>
    /// <param name="collisionResolutionImpulseMagnitudes">the impulses applied to the two bodies during a collision resolution step.</param>
    /// <param name="collision">the collision data.</param>
    /// <param name="rigidBodyA">the rigidbody of the 'owner' in the collision data.</param>
    /// <param name="rigidBodyB">the rigidbody of the 'other' in the collision data.</param>
    private static void ResolveFriction(
        Span<float> collisionResolutionImpulseMagnitudes,
        ref readonly Collision collision,
        ref RigidBody rigidBodyA,
        ref RigidBody rigidBodyB
    )
    {
        Span<Vector2> impulse = stackalloc Vector2[collision.ContactPointsCount];
        Span<Vector2> distA   = stackalloc Vector2[collision.ContactPointsCount];
        Span<Vector2> distB   = stackalloc Vector2[collision.ContactPointsCount];

        ReadOnlySpan<float> contactPointX = collision.GetXContactPointsAsReadOnlySpan();
        ReadOnlySpan<float> contactPointY = collision.GetYContactPointsAsReadOnlySpan();

        // get an approximation of the friction values.
        // this is faster than the actual physics way.
        float staticFriction = 0;
        float kineticFriction = 0;
        if(rigidBodyA.PhysicsMaterial.UseFriction && rigidBodyB.PhysicsMaterial.UseFriction)
        {
            staticFriction = (rigidBodyA.PhysicsMaterial.StaticFriction + rigidBodyB.PhysicsMaterial.StaticFriction) * 0.5f;
            kineticFriction = (rigidBodyA.PhysicsMaterial.KineticFriction + rigidBodyB.PhysicsMaterial.StaticFriction) * 0.5f;
        }
        else if (rigidBodyA.PhysicsMaterial.UseFriction)
        {
            staticFriction = rigidBodyA.PhysicsMaterial.StaticFriction;
            kineticFriction = rigidBodyA.PhysicsMaterial.KineticFriction;           
        }
        else if (rigidBodyB.PhysicsMaterial.UseFriction)
        {
            staticFriction = rigidBodyB.PhysicsMaterial.StaticFriction;
            kineticFriction = rigidBodyB.PhysicsMaterial.KineticFriction;            
        }
        
        for(int j = 0; j < collision.ContactPointsCount; j++)
        {
            Vector2 contactPoint = new Vector2(contactPointX[j], contactPointY[j]);
            
            // get the angular velocity to travel in.
            distA[j] = contactPoint - collision.OwnerColliderShapeCenter;
            distB[j] = contactPoint - collision.OtherColliderShapeCenter;
            Vector2 perpendicularA = new Vector2(-distA[j].Y, distA[j].X);
            Vector2 perpendicularB = new Vector2(-distB[j].Y, distB[j].X);
            Vector2 angularLinearVelocityA = perpendicularA * rigidBodyA.AngularVelocity; 
            Vector2 angularLinearVelocityB = perpendicularB * rigidBodyB.AngularVelocity; 

            Vector2 linearVelcoityA = new Vector2(rigidBodyA.LinearVelocityX, rigidBodyA.LinearVelocityY);
            Vector2 linearVelcoityB = new Vector2(rigidBodyB.LinearVelocityX, rigidBodyB.LinearVelocityY);

            Vector2 relativeVelocity = 
            (linearVelcoityB + angularLinearVelocityB) - 
            (linearVelcoityA + angularLinearVelocityA);

            // this is the direction the body is travelling in along the contact point surface.
            Vector2 tangent = relativeVelocity - Vector2.Dot(relativeVelocity, collision.Normal) * collision.Normal;

            if(Vector2.NearlyEqual(tangent, Vector2.Zero, 1e-8f))
            {
                continue;
            }
            tangent = tangent.Normalise();

            // calculate the denominator.
            float perpADotTangent = perpendicularA.Dot(tangent);
            float perpBDotTangent = perpendicularB.Dot(tangent);
            float denominator = rigidBodyA.InverseMass + rigidBodyB.InverseMass + 
                (perpADotTangent * perpADotTangent) * rigidBodyA.InverseRotationalInertia +
                (perpBDotTangent * perpBDotTangent) * rigidBodyB.InverseRotationalInertia;

            // magnitude of the impulse.
            // note: a uniary operate is applied to the dot product so that the friction 
            // impulse is applied in the opposite direction this body is traveling.
            float impulseMagnitude = -Vector2.Dot(relativeVelocity, tangent);
            impulseMagnitude /= denominator;

            // divide by the contact point count to ensure that impulse is evenly spread 
            // across all contact points.
            impulseMagnitude /= (float)collision.ContactPointsCount;

            float collisionImpulseMagnitude = collisionResolutionImpulseMagnitudes[j];

            // Coulomb's law states that fricction is proportional to how hard
            // to objects are being pressed together.
            if(Math.Math.Abs(impulseMagnitude) <= collisionImpulseMagnitude * staticFriction)
            {
                impulse[j] = impulseMagnitude * tangent;
            }
            else
            {
                impulse[j] = -collisionImpulseMagnitude * tangent * kineticFriction; 
            }
        }


        for(int j = 0; j < collision.ContactPointsCount; j++)
        {                
            // cross producting the dist and impulse gives a value indicating
            // how much angular velocity - in radians - is needed to be applied based on the impulse direction.
            // this is because cross producting two directions that are parallel to eachother, results in zero.
            // which means that there should be no rotation if the collision is head on.
            // but if the closer the two directions come to being perpendicular to one another,
            // the larger the angular impulse will be, causing the body to rotate.
            if(rigidBodyA.Mode == RigidBodyMode.Dynamic && rigidBodyA.PhysicsMaterial.UseFriction)
            {
                // always apply linear force, even if there is no rotational force to apply.
                ImpulseLinearForce(ref rigidBodyA, -impulse[j] * rigidBodyA.InverseMass);                

                if(rigidBodyA.RotationalPhysics)
                {
                    ImpulseAngularForce(ref rigidBodyA, -Vector2.Cross(distA[j], impulse[j]) * rigidBodyA.InverseRotationalInertia);                
                }
            }
            if(rigidBodyB.Mode == RigidBodyMode.Dynamic && rigidBodyB.PhysicsMaterial.UseFriction)
            {
                // always apply linear force, even if there is no rotational force to apply.
                ImpulseLinearForce(ref rigidBodyB, impulse[j] * rigidBodyB.InverseMass);
                
                if(rigidBodyB.RotationalPhysics)
                {
                    ImpulseAngularForce(ref rigidBodyB, Vector2.Cross(distB[j], impulse[j]) * rigidBodyB.InverseRotationalInertia);                
                }                
            }   
        }        
    }
}