using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;
using static Howl.ECS.GenIndexListProc;
using static Howl.Physics.RigidBody;
using static Howl.Math.Math;
using static Howl.Physics.Collision;

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
    public static void MovementStep(ComponentRegistry componentRegistry, float gravity, float gravityDirectionX, float gravityDirectionY, float deltaTime)
    {        
        GenIndexList<RigidBody> rigidbodies = componentRegistry.Get<RigidBody>();
        Span<DenseEntry<RigidBody>> denseEntries = GetDenseAsSpan(rigidbodies);

        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();

        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        GenIndexList<RectangleCollider> rectangleColliders = componentRegistry.Get<RectangleCollider>();

        float gravityLinearForceX = gravityDirectionX * gravity * deltaTime;
        float gravityLinearForceY = gravityDirectionY * gravity * deltaTime;

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
                ImpulseLinearForce(ref rigidbody, gravityLinearForceX, gravityLinearForceY);
            }


            // force = mass * acceleration.
            // acceleration = force / mass.
            float forceX = rigidbody.ForceX / rigidbody.Mass * deltaTime;
            float forceY = rigidbody.ForceY / rigidbody.Mass * deltaTime;

            ImpulseLinearForce(ref rigidbody, forceX, forceY);

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

        ref Collision start = ref MemoryMarshal.GetReference(collisions);

        for(int i = 0; i < collisions.Length; i+=2) // NOTE: increment by two as collisions are stored as siblings before the collision manifold is sorted.
        {            
            // get the two rigidbodies.
            if(GetDenseRef(rigidbodies, collisions[i].Owner, out Ref<RigidBody> rigidbodyARef).Fail()
            || GetDenseRef(rigidbodies, collisions[i].Other, out Ref<RigidBody> rigidbodyBRef).Fail())
            {
                System.Diagnostics.Debug.Assert(false);
                continue;
            }

            ref readonly Collision collision = ref Unsafe.Add(ref start, i);
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
        float relativeVelocityX = rigidBodyB.LinearVelocityX - rigidBodyA.LinearVelocityX;
        float relativeVelocityY = rigidBodyB.LinearVelocityY - rigidBodyA.LinearVelocityY;

        // the magnitude of the relative velocity relative to the normal
        float magnitude = Dot(relativeVelocityX, relativeVelocityY, collision.NormalX, collision.NormalY);

        if(magnitude > 0)
        {
            return;
        }

        float restitution = MathF.Min(rigidBodyA.Restitution, rigidBodyB.Restitution);

        // magnitude of the impulse
        float impulseMagnitude = -(1f + restitution) * magnitude;
        impulseMagnitude /= rigidBodyA.InverseMass + rigidBodyB.InverseMass;

        float impulseForceX;
        float impulseForceY;

        if(rigidBodyA.Mode == RigidBodyMode.Dynamic)
        {
            impulseForceX = -(impulseMagnitude / rigidBodyA.Mass * collision.NormalX);
            impulseForceY = -(impulseMagnitude / rigidBodyA.Mass * collision.NormalY);
            ImpulseLinearForce(ref rigidBodyA, impulseForceX, impulseForceY);            
        }

        if(rigidBodyB.Mode == RigidBodyMode.Dynamic)
        {
            impulseForceX = impulseMagnitude / rigidBodyB.Mass * collision.NormalX;
            impulseForceY = impulseMagnitude / rigidBodyB.Mass * collision.NormalY;
            ImpulseLinearForce(ref rigidBodyB, impulseForceX, impulseForceY);
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

        Span<float> contactPointsX = ContactPointsXAsSpan(collision);
        Span<float> contactPointsY = ContactPointsYAsSpan(collision);

        int count = collision.ContactPointsCount;
        Span<float> impulsesX   = stackalloc float[count];
        Span<float> impulsesY   = stackalloc float[count];
        Span<float> distsAX     = stackalloc float[count];
        Span<float> distsAY     = stackalloc float[count];
        Span<float> distsBX     = stackalloc float[count];
        Span<float> distsBY     = stackalloc float[count];
        
        for(int j = 0; j < count; j++)
        {
            float contactPointX = contactPointsX[j];
            float contactPointY = contactPointsY[j];

            // get the angular velocity to travel in.
            distsAX[j] = contactPointX - collision.OwnerColliderShapeCenterX;
            distsAY[j] = contactPointY - collision.OwnerColliderShapeCenterY;
            distsBX[j] = contactPointX - collision.OtherColliderShapeCenterX;
            distsBY[j] = contactPointY - collision.OtherColliderShapeCenterY;            
            
            float perpendicularAX = -distsAY[j];
            float perpendicularAY = distsAX[j];
            float perpendicularBX = -distsBY[j];
            float perpendicularBY = distsBX[j];

            float angularVelocityAX = perpendicularAX * rigidBodyA.AngularVelocity;
            float angularVelocityAY = perpendicularAY * rigidBodyA.AngularVelocity;
            float angularVelocityBX = perpendicularBX * rigidBodyB.AngularVelocity;
            float angularVelocityBY = perpendicularBY * rigidBodyB.AngularVelocity;

            float relativeVelocityX = rigidBodyB.LinearVelocityX + angularVelocityBX - (rigidBodyA.LinearVelocityX + angularVelocityAX);
            float relativeVelocityY = rigidBodyB.LinearVelocityY + angularVelocityBY - (rigidBodyA.LinearVelocityY + angularVelocityAY);
            
            // the magnitude of the relative velocity relative to the normal
            float magnitude = Dot(relativeVelocityX, relativeVelocityY, collision.NormalX, collision.NormalY);

            if(magnitude > 0)
            {
                continue;
            }

            // calculate the denominator.
            float perpADotNormal = Dot(perpendicularAX, perpendicularAY, collision.NormalX, collision.NormalY);
            float perpBDotNormal = Dot(perpendicularBX, perpendicularBY, collision.NormalX, collision.NormalY);
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

            impulsesX[j] = impulseMagnitude * collision.NormalX;
            impulsesY[j] = impulseMagnitude * collision.NormalY;
        }

        // keep these outside the for loop so they dont allocate each time.
        float impulseX;
        float impulseY;
        float distAX;
        float distAY;
        float distBX;
        float distBY;

        for(int j = 0; j < count; j++)
        {                
            impulseX = impulsesX[j];
            impulseY = impulsesY[j];

            // cross producting the dist and impulse gives a value indicating
            // how much angular velocity - in radians - is needed to be applied based on the impulse direction.
            // this is because cross producting two directions that are parallel to eachother, results in zero.
            // which means that there should be no rotation if the collision is head on.
            // but if the closer the two directions come to being perpendicular to one another,
            // the larger the angular impulse will be, causing the body to rotate.
            if(rigidBodyA.Mode == RigidBodyMode.Dynamic)
            {
                // always apply linear force, even if there is no rotational force to apply.
                ImpulseLinearForce(ref rigidBodyA, -impulseX * rigidBodyA.InverseMass, -impulseY * rigidBodyA.InverseMass);                

                if(rigidBodyA.RotationalPhysics)
                {
                    distAX = distsAX[j];
                    distAY = distsAY[j];
                    ImpulseAngularForce(ref rigidBodyA, -Cross(distAX, distAY, impulseX, impulseY) * rigidBodyA.InverseRotationalInertia);                
                }
            }
            if(rigidBodyB.Mode == RigidBodyMode.Dynamic)
            {
                // always apply linear force, even if there is no rotational force to apply.
                ImpulseLinearForce(ref rigidBodyB, impulseX * rigidBodyB.InverseMass, impulseY * rigidBodyB.InverseMass);
                
                if(rigidBodyB.RotationalPhysics)
                {
                    distBX = distsBX[j];
                    distBY = distsBY[j];
                    ImpulseAngularForce(ref rigidBodyB, Cross(distBX, distBY, impulseX, impulseY) * rigidBodyB.InverseRotationalInertia);                
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
        Span<float> contactPointsX = ContactPointsXAsSpan(collision);
        Span<float> contactPointsY = ContactPointsYAsSpan(collision);

        int count = collision.ContactPointsCount;
        Span<float> impulsesX   = stackalloc float[count];
        Span<float> impulsesY   = stackalloc float[count];
        Span<float> distsAX     = stackalloc float[count];
        Span<float> distsAY     = stackalloc float[count];
        Span<float> distsBX     = stackalloc float[count];
        Span<float> distsBY     = stackalloc float[count];
        
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
            float contactPointX = contactPointsX[j];
            float contactPointY = contactPointsY[j];

            // get the angular velocity to travel in.
            distsAX[j] = contactPointX - collision.OwnerColliderShapeCenterX;
            distsAY[j] = contactPointY - collision.OwnerColliderShapeCenterY;
            distsBX[j] = contactPointX - collision.OtherColliderShapeCenterX;
            distsBY[j] = contactPointY - collision.OtherColliderShapeCenterY;            
            
            float perpendicularAX = -distsAY[j];
            float perpendicularAY = distsAX[j];
            float perpendicularBX = -distsBY[j];
            float perpendicularBY = distsBX[j];

            float angularVelocityAX = perpendicularAX * rigidBodyA.AngularVelocity;
            float angularVelocityAY = perpendicularAY * rigidBodyA.AngularVelocity;
            float angularVelocityBX = perpendicularBX * rigidBodyB.AngularVelocity;
            float angularVelocityBY = perpendicularBY * rigidBodyB.AngularVelocity;

            float relativeVelocityX = rigidBodyB.LinearVelocityX + angularVelocityBX - (rigidBodyA.LinearVelocityX + angularVelocityAX);
            float relativeVelocityY = rigidBodyB.LinearVelocityY + angularVelocityBY - (rigidBodyA.LinearVelocityY + angularVelocityAY);

            // this is the direction the body is travelling in along the contact point surface.
            float relativeDotNormal = Dot(relativeVelocityX, relativeVelocityY, collision.NormalX, collision.NormalY);
            float tangentX = relativeVelocityX - relativeDotNormal * collision.NormalX;
            float tangentY = relativeVelocityY - relativeDotNormal * collision.NormalY;

            if(NearlyEqual(tangentX, 0, 1e-8f) || NearlyEqual(tangentX, 0, 1e-8f))
            {
                continue;
            }

            Normalise(tangentX, tangentY, out tangentX, out tangentY);

            // calculate the denominator.
            float perpADotTangent = Dot(perpendicularAX, perpendicularAY, tangentX, tangentY);
            float perpBDotTangent = Dot(perpendicularBX, perpendicularBY, tangentX, tangentY);
            float denominator = rigidBodyA.InverseMass + rigidBodyB.InverseMass + 
                (perpADotTangent * perpADotTangent) * rigidBodyA.InverseRotationalInertia +
                (perpBDotTangent * perpBDotTangent) * rigidBodyB.InverseRotationalInertia;

            // magnitude of the impulse.
            // note: a uniary operate is applied to the dot product so that the friction 
            // impulse is applied in the opposite direction this body is traveling.
            float impulseMagnitude = -Dot(relativeVelocityX, relativeVelocityY, tangentX, tangentY);
            impulseMagnitude /= denominator;

            // divide by the contact point count to ensure that impulse is evenly spread 
            // across all contact points.
            impulseMagnitude /= (float)collision.ContactPointsCount;

            float collisionImpulseMagnitude = collisionResolutionImpulseMagnitudes[j];

            // Coulomb's law states that fricction is proportional to how hard
            // to objects are being pressed together.
            if(Abs(impulseMagnitude) <= collisionImpulseMagnitude * staticFriction)
            {
                impulsesX[j] = impulseMagnitude * tangentX;
                impulsesY[j] = impulseMagnitude * tangentY;
            }
            else
            {
                impulsesX[j] = -collisionImpulseMagnitude * tangentX * kineticFriction; 
                impulsesY[j] = -collisionImpulseMagnitude * tangentY * kineticFriction; 
            }
        }

        // keep these outside the for loop so they dont allocate each time.
        float impulseX;
        float impulseY;
        float distAX;
        float distAY;
        float distBX;
        float distBY;

        for(int j = 0; j < count; j++)
        {                
            impulseX = impulsesX[j];
            impulseY = impulsesY[j];

            // cross producting the dist and impulse gives a value indicating
            // how much angular velocity - in radians - is needed to be applied based on the impulse direction.
            // this is because cross producting two directions that are parallel to eachother, results in zero.
            // which means that there should be no rotation if the collision is head on.
            // but if the closer the two directions come to being perpendicular to one another,
            // the larger the angular impulse will be, causing the body to rotate.
            if(rigidBodyA.Mode == RigidBodyMode.Dynamic)
            {
                // always apply linear force, even if there is no rotational force to apply.
                ImpulseLinearForce(ref rigidBodyA, -impulseX * rigidBodyA.InverseMass, -impulseY * rigidBodyA.InverseMass);                

                if(rigidBodyA.RotationalPhysics)
                {
                    distAX = distsAX[j];
                    distAY = distsAY[j];
                    ImpulseAngularForce(ref rigidBodyA, -Cross(distAX, distAY, impulseX, impulseY) * rigidBodyA.InverseRotationalInertia);                
                }
            }
            if(rigidBodyB.Mode == RigidBodyMode.Dynamic)
            {
                // always apply linear force, even if there is no rotational force to apply.
                ImpulseLinearForce(ref rigidBodyB, impulseX * rigidBodyB.InverseMass, impulseY * rigidBodyB.InverseMass);
                
                if(rigidBodyB.RotationalPhysics)
                {
                    distBX = distsBX[j];
                    distBY = distsBY[j];
                    ImpulseAngularForce(ref rigidBodyB, Cross(distBX, distBY, impulseX, impulseY) * rigidBodyB.InverseRotationalInertia);                
                }                
            }   
        }     
    }
}