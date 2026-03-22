using System;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Generic;
using Howl.DataStructures;
using static Howl.Math.Shapes.PolygonRectangle;
using static Howl.Math.Math;
using static Howl.ECS.GenIndexListProc;
using static Howl.DataStructures.BoundingVolumeHierarchy;
using static Howl.Math.Shapes.AABB;
using static Howl.Collections.Buffer;
using static Howl.Math.Shapes.ShapeUtils;
using static Howl.Physics.Soa_Collision;
using static Howl.DataStructures.Soa_SpatialPair;
using static Howl.Math.Shapes.SAT;
using static System.Runtime.InteropServices.CollectionsMarshal;
using static Howl.Math.Soa_Transform;
using Howl.Graphics;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Runtime.InteropServices;
using System.Numerics;
using Vector2 = Howl.Math.Vector2;

namespace Howl.Physics;

public static class SoaPhysicsSystem
{
    public const float RectangleRotationalInertia = 0.0833333333333f;
    public const float CircleRotationalInertia = 0.5f;
    public const float MinBodySize = float.Epsilon;
    public const float MaxBodySize = float.MaxValue;
    public const int MaxCollisionContactPoints = 2;
    
    public static void RegisterComponents(ComponentRegistry registry)
    {
        registry.RegisterComponent<Transform>();
        registry.RegisterComponent<PhysicsBodyId>();
    }

    public static void FixedUpdate(ComponentRegistry registry, SoaPhysicsSystemState state, float deltaTime, int subSteps)
    {
        state.FixedUpdateStepStopwatch.Restart();

        // Sync Colliders to Transforms Step.
        state.SyncTransformsToEntitiesStopwatch.Restart();
        SyncTransformsToEntityTransforms(registry.Get<Transform>(), registry.Get<PhysicsBodyId>(), 
            state.Transforms, state.Generations
        );
        state.SyncTransformsToEntitiesStopwatch.Stop();

        state.IntegrateBodyPropertiesStopwatch.Restart();
        IntegrateBodyProperties(state.Transforms.Scale.X, state.Transforms.Scale.Y, state.Masses, state.InverseMasses, 
            state.RotationalInertia, state.InverseRotationalInertia, state.Densities, state.LocalRadii, state.WorldRadii, 
            state.LocalWidths, state.LocalHeights, state.Flags
        );
        state.IntegrateBodyPropertiesStopwatch.Stop();

        // scale delta time by the substeps.
        deltaTime /= (float)subSteps;

        for(int i = 0; i < subSteps; i++)
        {
            state.FixedUpdateSubStepStopwatch.Restart();

            Clear(state.CollisionManifold.CircleCollisionsToResolve);
            Clear(state.CollisionManifold.PolygonCollisionsToResolve);
            Clear(state.CollisionManifold.PolygonToCircleCollisionsToResolve);


            // RigidBody Movement Step.
            state.RigidBodyMovementStepStopwatch.Restart();
            RigidBodyMovementStep(state.Transforms, state.LinearVelocities, state.Forces, 
                state.Masses, state.Flags, state.AngularVelocities, 
                state.GravityDirection.X, state.GravityDirection.Y, state.Gravity, deltaTime, state.MaxPhysicsBodyCount
            );
            state.RigidBodyMovementStepStopwatch.Stop();

            // transform physics bodies
            state.TransformPhysicsBodiesStopwatch.Restart();
            TransformPhysicsBodies(state.Centroids, state.MinAABBVertices, state.MaxAABBVertices,
                state.LocalVertices, state.WorldVertices, state.Transforms, state.Flags, 
                state.LocalRadii, state.WorldRadii, state.LocalWidths, state.LocalHeights, state.FirstVertexIndices, state.NextVertexIndices,
                state.MaxPhysicsBodyVertexCount, state.MaxPhysicsBodyCount, state.AlloctedPhysicsBodyCount
            );
            state.TransformPhysicsBodiesStopwatch.Stop();

            // Reconstruct Bvh.
            state.BvhReconstructionStopwatch.Restart();
            ReconstructBvhTree(state.WorldVertices, state.WorldRadii, state.FirstVertexIndices, 
                state.NextVertexIndices, state.Generations, state.Flags, state.Bvh, state.MaxPhysicsBodyVertexCount
            );
            state.BvhReconstructionStopwatch.Stop();

            // Filter bvh into collision manifold.
            state.FilterBvhIntoCollisionManifoldStopwatch.Restart();
                // clear spatial pairs before filtering.
                Clear(state.CollisionManifold.CircleSpatialPairs);
                Clear(state.CollisionManifold.PolygonSpatialPairs);
                Clear(state.CollisionManifold.PolygonToCircleSpatialPairs);
                FilterBvhIntoCollisionManifold(        
                    state.CollisionManifold.CircleSpatialPairs,
                    state.CollisionManifold.PolygonSpatialPairs,
                    state.CollisionManifold.PolygonToCircleSpatialPairs,
                    AsSpan(state.Bvh.SpatialPairs)
                );
            state.FilterBvhIntoCollisionManifoldStopwatch.Stop();

            // Find collisions.
            state.FindCollisionsStopwatch.Restart();
            FindCircleCollisions(state.CollisionManifold.CircleCollisionsToResolve, state.CollisionManifold.CircleSpatialPairs,
                state.WorldVertices, state.MinAABBVertices, state.MaxAABBVertices, state.WorldRadii,
                state.FirstVertexIndices
            );
            FindPolygonCollisions(state.CollisionManifold.PolygonCollisionsToResolve, state.CollisionManifold.PolygonSpatialPairs,
                state.WorldVertices, state.MinAABBVertices, state.MaxAABBVertices, state.Centroids,
                state.FirstVertexIndices, state.NextVertexIndices, state.MaxPhysicsBodyVertexCount
            );
            FindPolygonToCircleCollisions(state.CollisionManifold.PolygonToCircleCollisionsToResolve, 
                state.CollisionManifold.PolygonToCircleSpatialPairs, state.WorldVertices,
                state.MinAABBVertices, state.MaxAABBVertices, state.Centroids, state.FirstVertexIndices,
                state.NextVertexIndices, state.WorldRadii, state.MaxPhysicsBodyVertexCount
            );
            state.FindCollisionsStopwatch.Stop();

            // Resolve Collider Collisions.
            // NOTE: ordering matters here, make sure to resolve 
            // collisions before sorting the collision manifold.
            // Also make sure that this is above rigidbody collision resolution.
            // this function also moves the transforms of the colliders.
            state.ColliderCollisionResolutionStopwatch.Restart();
            ResolveColliderCollisions(state.CollisionManifold.CircleCollisionsToResolve, state.Transforms);
            ResolveColliderCollisions(state.CollisionManifold.PolygonCollisionsToResolve, state.Transforms);
            ResolveColliderCollisions(state.CollisionManifold.PolygonToCircleCollisionsToResolve, state.Transforms);
            state.ColliderCollisionResolutionStopwatch.Stop();

            // Resolve RigidBody Collisions.
            // NOTE: ordering matters here, make sure to resolve 
            // collisions before sorting the collision manifold.
            // Also make sure that this is below collision resolution.
            // this function also moves the transforms of the colliders.
            state.RigidBodyCollisionResolutionStepStopwatch.Restart();
            for(int j = 0; j < 3; j++)
            {
                Soa_Collision collisions = j switch{
                    0 => state.CollisionManifold.CircleCollisionsToResolve,
                    1 => state.CollisionManifold.PolygonCollisionsToResolve,
                    2 => state.CollisionManifold.PolygonToCircleCollisionsToResolve,
                    _ => null
                };

                if(collisions == null)
                    break;

                ResolveRigidBodyCollisions(collisions, state.LinearVelocities, state.Restitutions, state.AngularVelocities, 
                    state.InverseMasses, state.InverseRotationalInertia, state.KineticFrictions, state.StaticFrictions, state.Masses
                );
            }
            state.RigidBodyCollisionResolutionStepStopwatch.Stop();

            // Sort Collision Manifold.
            // sort the collision manifold after resolution step.
            // this is to ensure that binary searching for collisions
            // using a GenIndex work outside of this function.
            state.CollisionManifoldSortStopwatch.Restart();
            state.CollisionManifoldSortStopwatch.Stop();
            state.FixedUpdateSubStepStopwatch.Stop();
        }

        // Transform bodies by collision resolution.
        // NOTE: this is needed at the end as the final
        // sub-step iteration does not transform the bodies
        // at the end of it's loop; meaning the final collision
        // resolution wouldn't be applied.
        TransformPhysicsBodies(state.Centroids, state.MinAABBVertices, state.MaxAABBVertices,
            state.LocalVertices, state.WorldVertices, state.Transforms, state.Flags, 
            state.LocalRadii, state.WorldRadii, state.LocalWidths, state.LocalHeights, state.FirstVertexIndices, state.NextVertexIndices,
            state.MaxPhysicsBodyVertexCount, state.MaxPhysicsBodyCount, state.AlloctedPhysicsBodyCount
        );
        state.FixedUpdateStepStopwatch.Stop();
    }

    public static void Draw(ComponentRegistry registry, SoaPhysicsSystemState state, float deltaTime)
    {
        GetDenseRef(registry.Get<Camera>(), CameraSystem.MainCameraId, out Ref<Camera> camera);

        if (state.DrawColliderWireframes)
        {
            DrawCirclePhysicsBodies(camera, state.WorldVertices, state.WorldRadii, state.FirstVertexIndices, 
                state.Flags, state.DynamicPhysicsBodyColour, state.KinematicPhysicsBodyColour, state.TriggerPhysicsBodyColour
            );

            DrawPolygonPhysicsBodies(camera, state.WorldVertices, state.FirstVertexIndices, state.NextVertexIndices, 
                state.Flags, state.DynamicPhysicsBodyColour, state.KinematicPhysicsBodyColour, state.TriggerPhysicsBodyColour, 
                state.MaxPhysicsBodyVertexCount
            );
        }

        if (state.DrawCollisionInformation)
        {
            for(int j = 0; j < 3; j++)
            {
                Soa_Collision collisions = j switch{
                    0 => state.CollisionManifold.CircleCollisionsToResolve,
                    1 => state.CollisionManifold.PolygonCollisionsToResolve,
                    2 => state.CollisionManifold.PolygonToCircleCollisionsToResolve,
                    _ => null
                };

                if(collisions == null)
                    break;

                DrawCollisionInformation(camera, collisions, state.CollisionOwnerColour, state.CollisionOtherColour, 
                    state.ContactPointColour, state.NormalColour, collisions.Count
                );
            }
        }

        if (state.DrawLinearVelocities)
        {
            DrawLinearVelocities(camera, state.LinearVelocities, state.Centroids, state.Flags, state.LinearVelocityColour, 
                state.MaxPhysicsBodyCount
            );
        }

        if (state.DrawPositions)
        {
            DrawPositions(camera, state.Transforms.Position, state.Flags, state.PositionColour, state.MaxPhysicsBodyCount);
        }

        if (state.DrawCentroids)
        {
            DrawCentroids(camera, state.Centroids, state.Flags, state.CentroidColour, state.MaxPhysicsBodyCount);
        }
    }

    /// <summary>
    /// Syncs an SoaTransform collection to entities that contain both a transform component and a physics body id component. 
    /// </summary>
    /// <param name="componentRegistry">the component registry housing the entity components.</param>
    /// <param name="soaTransform">the structure-of-array transforms to mutate in relation to the entity data.</param>
    /// <param name="generation">the generations for each entry in the SOA transform's.</param>
    public static void SyncTransformsToEntityTransforms(GenIndexList<Transform> transforms, 
        GenIndexList<PhysicsBodyId> bodyIds, Soa_Transform soaTransform, Span<int> generation
    )
    {
        Span<DenseEntry<PhysicsBodyId>> denseEntries = GetDenseAsSpan(bodyIds);

        // loop through all body id's.
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<PhysicsBodyId> entry = ref denseEntries[i];
            ref PhysicsBodyId bodyId = ref entry.Value;
            GetGenIndex(bodyIds, entry.sparseIndex, out GenIndex genIndex);

            // skip if the physics body id isn't valid.
            if(generation[bodyId.GenIndex.Index] != bodyId.GenIndex.Generation)
                continue;
            
            // sync the transform data to the physics simulation 
            // if it has an associated physics body id.
            
            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Ok())
                SetTransform(soaTransform, generation, genIndex, transformRef);
        } 
    }

    /// <summary>
    /// Syncs a entities that contain both a transform and physics body id component to an soa transform collection.
    /// </summary>
    /// <param name="transforms">the gen index list that stores the entity transform components to mutate..</param>
    /// <param name="bodyIds">the gen index list that stores the entity physics body id components to mutate..</param>
    /// <param name="soaTransform">the soa transforms to copy into the entity transform components.</param>
    /// <param name="generation">the generation of each soa transform entry.</param>
    public static void SyncEntityTransformsToPhysicsBodies(GenIndexList<Transform> transforms,
        GenIndexList<PhysicsBodyId> bodyIds, Soa_Transform soaTransform, Span<int> generation)
    {
        Span<DenseEntry<PhysicsBodyId>> denseEntries = GetDenseAsSpan(bodyIds);
        
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<PhysicsBodyId> entry = ref denseEntries[i];
            ref PhysicsBodyId bodyId = ref entry.Value;
            GetGenIndex(bodyIds, entry.sparseIndex, out GenIndex genIndex);

            // skip if the physics body id isn't valid.
            if(generation[bodyId.GenIndex.Index] != bodyId.GenIndex.Generation)
                continue;

            // sync the transform data to the physics simulation 
            // if it has an associated physics body id.
            
            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Ok())
                CopySoaToTransform(soaTransform, ref transformRef.Value, bodyId.GenIndex.Index);
        }
    }

    /// <summary>
    /// Performs a movement step for all physics bodies with a rigidbody.
    /// </summary>
    /// <param name="transforms">the world-space transforms for all physics bodies.</param>
    /// <param name="linearVelocities">the linear velocity values for all physics bodies.</param>
    /// <param name="forces">the force values for all physics bodies.</param>
    /// <param name="masses">the mass values for all physics bodies.</param>
    /// <param name="flags">the flags for all physics bodies.</param>
    /// <param name="angularVelocities">the angular velocity values for all physics bodies.</param>
    /// <param name="gravityDirectionX">the x-component of the grvity direction vector.</param>
    /// <param name="gravityDirectionY">the y-component of the gravity direction vector.</param>
    /// <param name="gravity">the gravity force.</param>
    /// <param name="deltaTime">delta time.</param>
    /// <param name="maxBodies">the max amount of physics bodies.</param>
    public static void RigidBodyMovementStep(Soa_Transform transforms, Soa_Vector2 linearVelocities, Soa_Vector2 forces, 
        Span<float> masses, Span<PhysicsBodyFlags> flags, Span<float> angularVelocities, 
        float gravityDirectionX, float gravityDirectionY, float gravity, float deltaTime, int maxPhysicsBodies
    )
    {
        RigidBodyMovementStep_Simd(transforms, linearVelocities, forces, masses, flags, angularVelocities, gravityDirectionX, gravityDirectionY, 
            gravity, deltaTime, maxPhysicsBodies
        );
    }

    /// <summary>
    /// Performs a movement step for all physics bodies with a rigidbody.
    /// </summary>
    /// <param name="transforms">the world-space transforms for all physics bodies.</param>
    /// <param name="linearVelocities">the linear velocity values for all physics bodies.</param>
    /// <param name="forces">the force values for all physics bodies.</param>
    /// <param name="masses">the mass values for all physics bodies.</param>
    /// <param name="flags">the flags for all physics bodies.</param>
    /// <param name="angularVelocities">the angular velocity values for all physics bodies.</param>
    /// <param name="gravityDirectionX">the x-component of the grvity direction vector.</param>
    /// <param name="gravityDirectionY">the y-component of the gravity direction vector.</param>
    /// <param name="gravity">the gravity force.</param>
    /// <param name="deltaTime">delta time.</param>
    /// <param name="maxBodies">the max amount of physics bodies.</param>
    public static void RigidBodyMovementStep_Simd(Soa_Transform transforms, Soa_Vector2 linearVelocities, Soa_Vector2 forces, 
        Span<float> masses, Span<PhysicsBodyFlags> flags, Span<float> angularVelocities, 
        float gravityDirectionX, float gravityDirectionY, float gravity, float deltaTime, int maxPhysicsBodies
    )
    {
        int simdSize = Vector<float>.Count;

        PhysicsBodyFlags requiredFlags = PhysicsBodyFlags.Allocated | PhysicsBodyFlags.Active | PhysicsBodyFlags.RigidBody;
        PhysicsBodyFlags forbiddenFlags = PhysicsBodyFlags.Kinematic;

        Vector<int> vRequiredFlags = new Vector<int>((int)requiredFlags);
        Vector<int> vForbiddenFlags = new Vector<int>((int)forbiddenFlags);
        Vector<float> vDeltaTime = new Vector<float>(deltaTime);
        Vector<float> vGravityX = new Vector<float>(gravityDirectionX * gravity * deltaTime);
        Vector<float> vGravityY = new Vector<float>(gravityDirectionY * gravity * deltaTime);
        Vector<float> vZero = new Vector<float>(0);

        int i = 0;
        for(; i <= maxPhysicsBodies - simdSize; i+= simdSize)
        {
            ref int flagsAsInt = ref Unsafe.As<PhysicsBodyFlags, int>(ref flags[i]);
            Vector<int> vFlags = Vector.LoadUnsafe(ref flagsAsInt);
            
            // (flag & required) == required.
            Vector<int> hasRequired = Vector.Equals(vFlags & vRequiredFlags, vRequiredFlags);

            // (flag & forbidden) == 0.
            Vector<int> doesntHaveForbidden = Vector.Equals(vFlags & vForbiddenFlags, Vector<int>.Zero);
        
            // combined mask.
            Vector<int> vMask = hasRequired & doesntHaveForbidden;

            // short circuit if the entire mask is zero 
            // (all bodies in this chunk either dont have the required flags or have the forbidden flags)
            if (vMask.Equals(Vector<int>.Zero))
            {
                continue;
            }

            // load data.
            Vector<float> vLinVelX = Vector.LoadUnsafe(ref linearVelocities.X[i]);
            Vector<float> vLinVelY = Vector.LoadUnsafe(ref linearVelocities.Y[i]);
            Vector<float> vForceX = Vector.LoadUnsafe(ref forces.X[i]);
            Vector<float> vForceY = Vector.LoadUnsafe(ref forces.Y[i]);
            Vector<float> vMass = Vector.LoadUnsafe(ref masses[i]);
            Vector<float> vPosX = Vector.LoadUnsafe(ref transforms.Position.X[i]);
            Vector<float> vPosY = Vector.LoadUnsafe(ref transforms.Position.Y[i]);
            Vector<float> vSin = Vector.LoadUnsafe(ref transforms.Sin[i]);
            Vector<float> vCos = Vector.LoadUnsafe(ref transforms.Cos[i]);
            Vector<float> vAngVel = Vector.LoadUnsafe(ref angularVelocities[i]);

            // apply gravity.
            Vector<float> nextVelX = vLinVelX + vGravityX;
            Vector<float> nextVelY = vLinVelY + vGravityY;

            // use Vector.GreateThan to avoid div by zero if mass is zero.
            Vector<int> massMask = Vector.GreaterThan(vMass, vZero);

            // apply forces: acceleration = (f / m * deltaTime)
            Vector<float> accelX = vForceX / vMass * vDeltaTime;
            Vector<float> accelY = vForceY / vMass * vDeltaTime;

            // only add acceleration where mass > 0 and force > 0.
            nextVelX += Vector.ConditionalSelect(massMask & Vector.GreaterThan(vForceX, vZero), accelX, vZero);
            nextVelY += Vector.ConditionalSelect(massMask & Vector.GreaterThan(vForceY, vZero), accelY, vZero);

            // calculate new positions.
            Vector<float> nextPosX = vPosX + (nextVelX * vDeltaTime);
            Vector<float> nextPosY = vPosY + (nextVelY * vDeltaTime);

            // calculate new rotations.
            Vector<float> newSin = Vector<float>.Zero;
            Vector<float> newCos = Vector<float>.Zero;
            MathV.RotorMultiply(vSin, vCos, vAngVel * vDeltaTime, ref newSin, ref newCos);

            // conditional select (only keep results for valid flags)
            vLinVelX = Vector.ConditionalSelect(vMask, nextVelX, vLinVelX);
            vLinVelY = Vector.ConditionalSelect(vMask, nextVelY, vLinVelY);
            vPosX = Vector.ConditionalSelect(vMask, nextPosX, vPosX);
            vPosY = Vector.ConditionalSelect(vMask, nextPosY, vPosY);
            vCos = Vector.ConditionalSelect(vMask, newCos, vCos);
            vSin = Vector.ConditionalSelect(vMask, newSin, vSin);

            // store results.
            vLinVelX.StoreUnsafe(ref linearVelocities.X[i]);
            vLinVelY.StoreUnsafe(ref linearVelocities.Y[i]);
            vPosX.StoreUnsafe(ref transforms.Position.X[i]);
            vPosY.StoreUnsafe(ref transforms.Position.Y[i]);
            vCos.StoreUnsafe(ref transforms.Cos[i]);
            vSin.StoreUnsafe(ref transforms.Sin[i]);
        }
 
        // tail end.
        RigidBodyMovementStep_Sisd(transforms, linearVelocities, forces, 
            masses, flags, angularVelocities,
            gravityDirectionX, gravityDirectionY, gravity, deltaTime, maxPhysicsBodies, i
        );
    }

    /// <summary>
    /// Performs a movement step for all physics bodies with a rigidbody.
    /// </summary>
    /// <param name="transforms">the world-space transforms for all physics bodies.</param>
    /// <param name="linearVelocities">the linear velocity values for all physics bodies.</param>
    /// <param name="forces">the force values for all physics bodies.</param>
    /// <param name="masses">the mass values for all physics bodies.</param>
    /// <param name="flags">the flags for all physics bodies.</param>
    /// <param name="angularVelocities">the angular velocity values for all physics bodies.</param>
    /// <param name="gravityDirectionX">the x-component of the grvity direction vector.</param>
    /// <param name="gravityDirectionY">the y-component of the gravity direction vector.</param>
    /// <param name="gravity">the gravity force.</param>
    /// <param name="deltaTime">delta time.</param>
    /// <param name="maxBodies">the max amount of physics bodies.</param>
    /// <param name="startIndex">the physics body index to start at in the loop.</param>
    public static void RigidBodyMovementStep_Sisd(Soa_Transform transforms, Soa_Vector2 linearVelocities, Soa_Vector2 forces, 
        Span<float> masses, Span<PhysicsBodyFlags> flags, Span<float> angularVelocities,
        float gravityDirectionX, float gravityDirectionY, float gravity, float deltaTime, int maxBodies, int startIndex
    )
    {
        Span<float> forcesX = forces.X;
        Span<float> forcesY = forces.Y;
        Span<float> linearVelocitiesX = linearVelocities.X;
        Span<float> linearVelocitiesY = linearVelocities.Y;
        Span<float> positionsX = transforms.Position.X;
        Span<float> positionsY = transforms.Position.Y;
        Span<float> sin = transforms.Sin;
        Span<float> cos = transforms.Cos;

        float gravityLinearForceX = gravityDirectionX * gravity;
        float gravityLinearForceY = gravityDirectionY * gravity;

        for(int i = startIndex; i < maxBodies; i++)
        {
            PhysicsBodyFlags flag = (PhysicsBodyFlags)flags[i];
            
            if((flag & PhysicsBodyFlags.Allocated) == 0 ||
                (flag & PhysicsBodyFlags.Active) == 0 ||
                (flag & PhysicsBodyFlags.Kinematic) != 0 ||
                (flag & PhysicsBodyFlags.RigidBody) == 0)
            {
                continue;
            }

            ref float linearVelocityX = ref linearVelocitiesX[i];
            ref float linearVelocityY = ref linearVelocitiesY[i];
            ref float mass = ref masses[i];

            // apply gravity.
            linearVelocityX += gravityLinearForceX * deltaTime;
            linearVelocityY += gravityLinearForceY * deltaTime;

            // force = mass * acceleration.
            // acceleration = force / mass.
            if(mass > 0)
            {
                if (forcesX[i] > 0)
                {
                    linearVelocityX += forcesX[i] / mass * deltaTime;
                }
                if (forcesY[i] > 0)
                {
                    linearVelocityY += forcesY[i] / mass * deltaTime;
                }
            }
            
            // apply linear velocity.
            positionsX[i] += linearVelocityX * deltaTime;
            positionsY[i] += linearVelocityY * deltaTime;
            RotorMultiply(sin[i], cos[i], angularVelocities[i] * deltaTime ,ref sin[i], ref cos[i]);
        }
    }

    /// <summary>
    /// Transforms <c>InUse<c/> physics bodies local-space vertices by their world-space transforms.
    /// </summary>
    /// <param name="centroids">a span to store the generated centroid values of all physics bodies.</param>
    /// <param name="minAABBVertices">a span to store the generated mininum vertice values of all physics bodies AABB's.</param>
    /// <param name="maxAABBVertices">a span to store the generated maximum vertice values of all physics bodies AABB's.</param>
    /// <param name="localVertices">the local-space vertices of all physics bodies.</param>
    /// <param name="worldVertices">a span to store the generated world-space vertex values of all physics bodies.</param>
    /// <param name="transforms">the world-space transforms of all physics bodies.</param>
    /// <param name="flags">the flags of all physics bodies.</param>
    /// <param name="worldRadii">the world-space radius values of all phsyics bodies.</param>
    /// <param name="localWidths">the local-space width values of all physics bodies.</param>
    /// <param name="localHeights">the local-space height values of all physics bodies.</param>
    /// <param name="firstVertexIndices">the indices in the vertices span that point to the first vertex of a given physics body.</param>
    /// <param name="nextVertexIndices">the indices in the vertices span that point to the next vertex of a given vertex index.</param>
    /// <param name="maxPhysicsBodyVertexCount">the max amount of vertices a physics body can have.</param>
    /// <param name="maxPhysicsBodyCount">the max amount of physics bodies that can be stored.</param>
    /// <param name="physicsBodyCount">the current amount of allocated physics bodies.</param>
    public static void TransformPhysicsBodies(Soa_Vector2 centroids, Soa_Vector2 minAABBVertices, Soa_Vector2 maxAABBVertices,
        Soa_Vector2 localVertices, Soa_Vector2 worldVertices, Soa_Transform transforms, Span<PhysicsBodyFlags> flags, 
        Span<float> localRadii, Span<float> worldRadii, Span<float> localWidths, Span<float> localHeights, Span<int> firstVertexIndices, 
        Span<int> nextVertexIndices, int polygonMaxVertices, int maxPhysicsBodyCount, int physicsBodyCount
    )
    {
        // hoisting invariance.
        Span<float> verticesX = localVertices.X;
        Span<float> verticesY = localVertices.Y;
        Span<float> transformedVerticesX = worldVertices.X;
        Span<float> transformedVerticesY = worldVertices.Y;
        Span<float> centroidsX = centroids.X;
        Span<float> centroidsY = centroids.Y;
        Span<float> minAABBVectorsX = minAABBVertices.X;
        Span<float> minAABBVectorsY = minAABBVertices.Y;
        Span<float> maxAABBVectorsX = maxAABBVertices.X;
        Span<float> maxAABBVectorsY = maxAABBVertices.Y;
        Span<float> scalesX = transforms.Scale.X;
        Span<float> scalesY = transforms.Scale.Y;
        Span<float> cos = transforms.Cos;
        Span<float> sin = transforms.Sin;
        Span<float> positionsX = transforms.Position.X;
        Span<float> positionsY = transforms.Position.Y;

        Span<float> polygonTransformedVerticesX = stackalloc float[polygonMaxVertices];
        Span<float> polygonTransformedVerticesY = stackalloc float[polygonMaxVertices];
        int polygonTransformedVerticesCount = 0;

        int physicsBodiesProcessed = 0;

        for(int i = 0; i < maxPhysicsBodyCount; i++)
        {
            PhysicsBodyFlags flag = flags[i];
            
            // if the physics body had been allocated and is active.
            if((flag & PhysicsBodyFlags.Allocated) == 0)
            {
                continue;
            }

            physicsBodiesProcessed++;

            if((flag & PhysicsBodyFlags.Active) == 0)
            {
                continue;
            }

            // hoisting in variance.
            ref float scaleX = ref scalesX[i];
            ref float scaleY = ref scalesY[i];

            if((flag & PhysicsBodyFlags.RectangleShape) != 0)
            {
                int first = firstVertexIndices[i]; 
                int verticeIndex = first;
                while (true)
                {

                    // transform the base/un-transformed vertice.
                    TransformVector(verticesX[verticeIndex], verticesY[verticeIndex], scaleX, scaleY,
                        cos[i], sin[i], positionsX[i], positionsY[i], out float x, out float y
                    );

                    // mutate the transformed vertices array.
                    transformedVerticesX[verticeIndex] = x;
                    transformedVerticesY[verticeIndex] = y;

                    // mutate local cache of vertices.
                    polygonTransformedVerticesX[polygonTransformedVerticesCount] = x;
                    polygonTransformedVerticesY[polygonTransformedVerticesCount] = y;
                    polygonTransformedVerticesCount++;

                    verticeIndex = nextVertexIndices[verticeIndex];

                    if (verticeIndex == first)
                        break;
                }

                // set the new centroid.
                GetCentroid(polygonTransformedVerticesX, polygonTransformedVerticesY, out centroidsX[i], out centroidsY[i]);

                // set the new min and max vectors.
                GetMinMaxVectors(polygonTransformedVerticesX, polygonTransformedVerticesY, 
                    out minAABBVectorsX[i], out minAABBVectorsY[i], out maxAABBVectorsX[i], out maxAABBVectorsY[i]
                );

                // reset for next iteration.
                polygonTransformedVerticesCount = 0; 
            }
            else // circle shape.
            {
                int vertexIndex = firstVertexIndices[i];
                TransformVector(verticesX[vertexIndex],verticesY[vertexIndex],scaleX, scaleY, cos[i], sin[i], positionsX[i], positionsY[i], out float x, out float y);
                transformedVerticesX[vertexIndex] = x;
                transformedVerticesY[vertexIndex] = y;

                // set the new centroid.
                centroidsX[i] = x;
                centroidsY[i] = y;

                // set the new min and max vectors. 
                Circle.GetMinMaxVectors(x, y, worldRadii[i], 
                    out minAABBVectorsX[i], out minAABBVectorsY[i], out maxAABBVectorsX[i], out maxAABBVectorsY[i]
                );
            }

            if(physicsBodiesProcessed >= physicsBodyCount)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Re-calculates rigidbody data and world-space dimensions based on current scales.    
    /// </summary>
    /// <remarks>
    /// All provided spans must be indexed by a valid <c>PhysicsBodyId</c>:
    /// <list type="bullet">
    /// <item><description><paramref name="scalesX"/> / <paramref name="scalesY"/></description></item>
    /// <item><description><paramref name="masses"/> / <paramref name="inverseMasses"/></description></item>
    /// <item><description><paramref name="rotationalInertia"/> / <paramref name="inverseRotationalInertia"/></description></item>
    /// <item><description><paramref name="flags"/></description></item>
    /// </list>
    /// </remarks>
    /// <param name="scalesX">the x-component's of all physics bodies scaling vectors.</param>
    /// <param name="scalesY">the y-component's of all physics bodies scaling vectors.</param>
    /// <param name="masses">a span to store the generated mass values of all physics bodies.</param>
    /// <param name="inverseMasses">a span to store the generated inverse mass values of all physics bodies.</param>
    /// <param name="rotationalInertia">a span to store the generated rotational intertia values of all physics bodies.</param>
    /// <param name="inverseRotationalInertia">a span to store the generated inverse rotational inertia values of all physics bodies.</param>
    /// <param name="densities">the densities of all physics bodies.</param>
    /// <param name="radii">the local-space radii of all physics bodies.</param>
    /// <param name="transformedRadii">a span to store the generated transformed radius values of all physics bodies.</param>
    /// <param name="widths">the local-space width values of all physics bodies.</param>
    /// <param name="heights">the local-space height values of all physics bodies.</param>
    /// <param name="flags">the flags of all physics bodies</param>
    public static void IntegrateBodyProperties(Span<float> scalesX, Span<float> scalesY, Span<float> masses, Span<float> inverseMasses, 
        Span<float> rotationalInertia, Span<float> inverseRotationalInertia, Span<float> densities, Span<float> radii, Span<float> transformedRadii, 
        Span<float> widths, Span<float> heights, Span<PhysicsBodyFlags> flags
    )
    {
        float width;
        float height;
        float radius;
        float scaleX;
        float scaleY;
        bool isRigid;

        for(int i = 0; i < flags.Length; i++)
        {
            ref PhysicsBodyFlags flag = ref flags[i];
            if((flag & PhysicsBodyFlags.InUse) == 0)
            {
                continue;
            }

            scaleX = scalesX[i];
            scaleY = scalesY[i];
            isRigid = (flag & PhysicsBodyFlags.RigidBody) != 0; 

            if((flag & PhysicsBodyFlags.RectangleShape) != 0)
            {
                // set rigidbody data if it is enabled.
                if(isRigid)
                {                    
                    height = heights[i] * scaleY;
                    width = widths[i] * scaleX;

                    float mass = CalculateRectangleMass(width, height, densities[i]); 
                    masses[i] = mass;
                    inverseMasses[i] = mass == 0? 0 : 1f/mass;

                    float inertia = CalculateRectangleRotationalInertia(width, height, mass);
                    rotationalInertia[i] = inertia;
                    inverseRotationalInertia[i] = inertia == 0? 0 : 1f/inertia;
                }
            }
            else // circle shape
            {
                radius = Circle.ScaleRadius(radii[i], scaleX, scaleY);
                transformedRadii[i] = radius;

                // set rigidbody data if it is enabled.
                if(isRigid)
                {                    
                    float mass = CalculateCircleMass(radius, densities[i]);
                    masses[i] = mass;
                    inverseMasses[i] = mass==0? 0 : 1f/mass;

                    float rI = CalculateCircleRotationalInertia(radius, mass);
                    rotationalInertia[i] = rI;
                    inverseRotationalInertia[i] = rI == 0? 0f : 1f/rI;
                }
            }

        }
    }

    /// <summary>
    /// Reconstructs a bounding volume hierarchy tree with physics body data.
    /// </summary>
    /// <remarks>
    /// Note: 
    /// Matching lengths are required as data is associated via index (SOA).
    /// - 'next vertex indices' and 'transformed vertices' must be the same length.
    /// - 'transformed radii', 'generations', 'first vertex indices' and 'flags' must be the same length.
    /// </remarks>
    /// <param name="vertices">the vertices of all physics bodies to insert into the bounding volume hierarchy.</param>
    /// <param name="radii">the radii of circle physics bodies to calculate the bounding box necessary for insertion in the bounding volume hierarchy.</param>
    /// <param name="firstVertexIndices">the first vertex indices for each physics body.</param>
    /// <param name="nextVertexIndices">the next vertex indices for each vertex in transform vertices.</param>
    /// <param name="generations">the generation for each physics body.</param>
    /// <param name="flags">the flags for each physics body.</param>
    /// <param name="bvh">the bounding volume hierarchy.</param>
    /// <param name="maxPhysicsBodyVertexCount">the max amount of vertices that a physics body shape can have.</param>
    public static void ReconstructBvhTree(
        Soa_Vector2 vertices, 
        Span<float> radii,
        Span<int> firstVertexIndices, 
        Span<int> nextVertexIndices, 
        Span<int> generations,
        Span<PhysicsBodyFlags> flags, 
        BoundingVolumeHierarchy bvh,
        int maxPhysicsBodyVertexCount
    )
    {   
        // clear the previous bvh data.
        Clear(bvh);

        // create spans of the maximum amount of vertices a given 
        // body shape can store.
        Span<float> x = stackalloc float[maxPhysicsBodyVertexCount];
        Span<float> y = stackalloc float[maxPhysicsBodyVertexCount];

        for(int i = 0; i < flags.Length; i++)
        {
            ref PhysicsBodyFlags flag = ref flags[i];
            ref int firstVerticeIndex = ref firstVertexIndices[i];
            if((flag & PhysicsBodyFlags.Allocated) != 0 && (flag & PhysicsBodyFlags.Active) != 0)
            {
                float minX;
                float minY;
                float maxX;
                float maxY;

                if((flag & PhysicsBodyFlags.RectangleShape) != 0)
                {

                    // get the body's shape vertices.
                    int verticeCount = 0;
                    int verticeIndex = firstVerticeIndex;
                    
                    while (true)
                    {
                        // store the vertice data.
                        x[verticeCount] = vertices.X[verticeIndex];
                        y[verticeCount] = vertices.Y[verticeIndex];
                        
                        // go to the next vertice.
                        verticeCount++;
                        verticeIndex = nextVertexIndices[verticeIndex];
                        
                        // break when looping back to the start.
                        if(verticeIndex == firstVerticeIndex)
                            break;    
                    }

                    // get the min and max vertices of the current body.
                    GetMinMaxVectors(
                        x.Slice(0, verticeCount), // only read the valid vertices data.
                        y.Slice(0, verticeCount), // only read the valid vertices data.
                        out minX,
                        out minY,
                        out maxX,
                        out maxY
                    );
                    
                }
                else // circle
                {
                    Circle.GetMinMaxVectors(
                        vertices.X[firstVerticeIndex],
                        vertices.Y[firstVerticeIndex],
                        radii[i],
                        out minX,
                        out minY,
                        out maxX,
                        out maxY
                    );
                }

                // insert into the bvh.
                InsertLeaf(
                    bvh,
                    new Leaf(
                        minX,
                        minY,
                        maxX,
                        maxY,
                        i,
                        generations[i],
                        (int)flag // this is okay as PhysicsBodyFlags is a int under the hood.
                    )
                );
            }
        }

        // construct the bvh with the new data.
        ConstructTree(bvh);
    }

    /// <summary>
    /// Filters a given span of spatial pairs into three soa spatial pair categories.
    /// </summary>
    /// <remark>
    /// Note: this functions assumes that spatial pairs have user-defined flags that
    /// can be statically casted to PhysicsBodyFlag.
    /// </remark>
    /// <param name="circleSpatialPairs">the soa spatial pair to append any found pairs that contain only circle physics bodies.</param>
    /// <param name="polygonSpatialPairs">the soa spatial pair to append any found pairs that contain only polygon physics bodies.</param>
    /// <param name="polygonToCircleSpatialPairs">the so spatial pair to append any found pairs that contain a polygon and circle body.</param>
    /// <param name="spatialPairs">the spatial pairs to filter.</param>
    public static void FilterBvhIntoCollisionManifold(
        Soa_SpatialPair circleSpatialPairs,
        Soa_SpatialPair polygonSpatialPairs,
        Soa_SpatialPair polygonToCircleSpatialPairs,
        Span<SpatialPair> spatialPairs
    )
    {
        for(int i = 0; i < spatialPairs.Length; i++)
        {
            ref SpatialPair pair = ref spatialPairs[i];
            PhysicsBodyFlags ownerFlag = (PhysicsBodyFlags)pair.Owner.Flag;
            PhysicsBodyFlags otherFlag = (PhysicsBodyFlags)pair.Other.Flag;
            if((ownerFlag & PhysicsBodyFlags.RectangleShape) != 0)
            {
                if((otherFlag & PhysicsBodyFlags.RectangleShape) != 0)
                {
                    // polygon to polygon.
                    AppendSpatialPair(
                        polygonSpatialPairs, 
                        pair.Owner.GenIndex.Index, 
                        pair.Owner.GenIndex.Generation, 
                        pair.Other.GenIndex.Index, 
                        pair.Other.GenIndex.Generation,
                        pair.Owner.Flag,
                        pair.Other.Flag 
                    );
                }
                else // other is a circle.
                {
                    // polygon to circle.
                    AppendSpatialPair(
                        polygonToCircleSpatialPairs, 
                        pair.Owner.GenIndex.Index, 
                        pair.Owner.GenIndex.Generation, 
                        pair.Other.GenIndex.Index, 
                        pair.Other.GenIndex.Generation,
                        pair.Owner.Flag,
                        pair.Other.Flag 
                    );
                }
            }
            else // owner is circle.
            {
                if((otherFlag & PhysicsBodyFlags.RectangleShape) != 0)
                {
                    // circle to polygon.
                    // Note: append other first instead of owner as
                    // other is the polygon.
                    AppendSpatialPair(
                        polygonToCircleSpatialPairs, 
                        pair.Other.GenIndex.Index, 
                        pair.Other.GenIndex.Generation,
                        pair.Owner.GenIndex.Index, 
                        pair.Owner.GenIndex.Generation, 
                        pair.Other.Flag, 
                        pair.Owner.Flag
                    );
                }
                else // other is circle.
                {
                    AppendSpatialPair(
                        circleSpatialPairs, 
                        pair.Owner.GenIndex.Index, 
                        pair.Owner.GenIndex.Generation, 
                        pair.Other.GenIndex.Index, 
                        pair.Other.GenIndex.Generation,
                        pair.Owner.Flag,
                        pair.Other.Flag 
                    );
                }
            }
        }
    }

    /// <summary>
    /// Finds any circle collisions to resolve from a given span of spatial pairs.
    /// </summary>
    /// <param name="collisionsToResolve">the soa collision to store any found collisions.</param>
    /// <param name="spatialPairs">the spatial pairs containing only circle to circle body pairs.</param>
    /// <param name="vertices">the soa vector containing all circle body vertices.</param>
    /// <param name="minAABBVectors">the soa vector containing the circles AABB's minimum vector.</param>
    /// <param name="maxAABBVectors">the soa vector containing the circles AABB's maximum vector.</param>
    /// <param name="radii">the radii of the circle bodies.</param>
    /// <param name="firstVertexIndices">the index of the first vertex of a body in the vertices collection.</param>
    public static void FindCircleCollisions(Soa_Collision collisionsToResolve, Soa_SpatialPair spatialPairs,
        Soa_Vector2 vertices, Soa_Vector2 minAABBVectors, Soa_Vector2 maxAABBVectors, Span<float> radii,
        Span<int> firstVertexIndices
    )
    {
        // hoisting invariance.
        Span<int> ownerIndices      = spatialPairs.OwnerGenIndices.Indices;
        Span<int> ownerGenerations  = spatialPairs.OwnerGenIndices.Generations;
        Span<int> otherIndices      = spatialPairs.OtherGenIndices.Indices;
        Span<int> otherGenerations  = spatialPairs.OtherGenIndices.Generations;
        Span<int> ownerFlags       = spatialPairs.OwnerFlags;
        Span<int> otherFlags       = spatialPairs.OtherFlags;
        Span<float> minAABBVectorX  = minAABBVectors.X;
        Span<float> minAABBVectorY  = minAABBVectors.Y;
        Span<float> maxAABBVectorX  = maxAABBVectors.X;
        Span<float> maxAABBVectorY  = maxAABBVectors.Y;
        Span<float> x = vertices.X;
        Span<float> y = vertices.Y;
        float normalX = 0;
        float normalY = 0;
        float depth = 0;
        float contactPointX = 0;
        float contactPointY = 0;

        for(int i = 0; i < spatialPairs.Count; i++)
        {
            // retirieve data.
            ref int ownerIndex              = ref ownerIndices[i];
            ref int ownerGeneration         = ref ownerGenerations[i];
            ref int otherIndex              = ref otherIndices[i];
            ref int otherGeneration         = ref otherGenerations[i];
            PhysicsBodyFlags ownerFlag = (PhysicsBodyFlags)ownerFlags[i];
            PhysicsBodyFlags otherFlag = (PhysicsBodyFlags)otherFlags[i];
            ref float ownerX    = ref x[firstVertexIndices[ownerIndex]];
            ref float otherX    = ref x[firstVertexIndices[otherIndex]];
            ref float ownerY    = ref y[firstVertexIndices[ownerIndex]];
            ref float otherY    = ref y[firstVertexIndices[otherIndex]];
            ref float ownerR    = ref radii[ownerIndex];
            ref float otherR    = ref radii[otherIndex];
            ref float ownerMinX = ref minAABBVectorX[ownerIndex];
            ref float otherMinX = ref minAABBVectorX[otherIndex];
            ref float ownerMinY = ref minAABBVectorY[ownerIndex];
            ref float otherMinY = ref minAABBVectorY[otherIndex];
            ref float ownerMaxX = ref maxAABBVectorX[ownerIndex];
            ref float otherMaxX = ref maxAABBVectorX[otherIndex];
            ref float ownerMaxY = ref maxAABBVectorY[ownerIndex];
            ref float otherMaxY = ref maxAABBVectorY[otherIndex];

            // Broad Phase:
            if (Intersect(ownerMinX, ownerMinY, ownerMaxX, ownerMaxY, otherMinX, otherMinY, otherMaxX, otherMaxY))
            {
                // Narrow Phase:
                // perform an SAT check.
                if(CirclesIntersect(ownerX, ownerY, ownerR, otherX, otherY, otherR, out normalX, out normalY, out depth))
                {
                    // submit the collision with contact points if one of the colliders needs them.
                    FindContactPoints(ownerX, ownerY, ownerR, otherX, otherY, out contactPointX, out contactPointY);

                    AppendCollision(collisionsToResolve, ownerIndex, ownerGeneration, otherIndex, otherGeneration,
                        normalX, normalY, ownerX, ownerY, otherX, otherY,
                        contactPointX, contactPointY, depth, ownerFlag, otherFlag
                    );
                }   
            }
        }
    }

    public static void FindPolygonCollisions(Soa_Collision collisionsToResolve, Soa_SpatialPair spatialPairs,
        Soa_Vector2 vertices, Soa_Vector2 minAABBVectors, Soa_Vector2 maxAABBVectors, Soa_Vector2 centroids,
        Span<int> firstVertexIndices, Span<int> nextVertexIndices, int maxPolygonVerticeCount
    )
    {
        // hoisting of invariance.
        Span<int> ownerIndices      = spatialPairs.OwnerGenIndices.Indices;
        Span<int> ownerGenerations  = spatialPairs.OwnerGenIndices.Generations;
        Span<int> otherIndices      = spatialPairs.OtherGenIndices.Indices;
        Span<int> otherGenerations  = spatialPairs.OtherGenIndices.Generations;
        Span<int> ownerFlags       = spatialPairs.OwnerFlags;
        Span<int> otherFlags       = spatialPairs.OtherFlags;
        Span<float> vertsX          = vertices.X;
        Span<float> vertsY          = vertices.Y;
        Span<float> minAABBVectorsX = minAABBVectors.X;
        Span<float> minAABBVectorsY = minAABBVectors.Y;
        Span<float> maxAABBVectorsX = maxAABBVectors.X;
        Span<float> maxAABBVectorsY = maxAABBVectors.Y;
        Span<float> centroidsX      = centroids.X;
        Span<float> centroidsY      = centroids.Y;
        Span<float> ownerVertsX = stackalloc float[maxPolygonVerticeCount];
        Span<float> ownerVertsY = stackalloc float[maxPolygonVerticeCount];
        Span<float> otherVertsX = stackalloc float[maxPolygonVerticeCount];
        Span<float> otherVertsY = stackalloc float[maxPolygonVerticeCount];
        float normalX = 0;
        float normalY = 0;
        float depth = 0;        
        float firstContactPointX = 0;
        float firstContactPointY = 0;
        float secondContactPointX = 0;
        float secondContactPointY = 0;
        
        int vertexCountA = 0;
        int vertexCountB = 0;
        int contactCount = 0;

        for(int i = 0; i < spatialPairs.Count; i++)
        {            
            ref int ownerIndex              = ref ownerIndices[i];
            ref int ownerGeneration         = ref ownerGenerations[i];
            ref int otherIndex              = ref otherIndices[i];
            ref int otherGeneration         = ref otherGenerations[i];
            PhysicsBodyFlags ownerFlag  = (PhysicsBodyFlags)ownerFlags[i];
            PhysicsBodyFlags otherFlag  = (PhysicsBodyFlags)otherFlags[i];

            // gather polygon a vertices.
            GetPolygonVertices(vertsX, vertsY, firstVertexIndices, nextVertexIndices, ownerVertsX, ownerVertsY, ownerIndex, ref vertexCountA);
            GetPolygonVertices(vertsX, vertsY, firstVertexIndices, nextVertexIndices, otherVertsX, otherVertsY, otherIndex, ref vertexCountB);

            ref float ownerCentroidX = ref centroidsX[ownerIndex];
            ref float otherCentroidX = ref centroidsX[otherIndex];
            ref float ownerCentroidY = ref centroidsY[ownerIndex];
            ref float otherCentroidY = ref centroidsY[otherIndex];
            ref float ownerMinX = ref minAABBVectorsX[ownerIndex];
            ref float otherMinX = ref minAABBVectorsX[otherIndex];
            ref float ownerMinY = ref minAABBVectorsY[ownerIndex];
            ref float otherMinY = ref minAABBVectorsY[otherIndex];
            ref float ownerMaxX = ref maxAABBVectorsX[ownerIndex];
            ref float otherMaxX = ref maxAABBVectorsX[otherIndex];
            ref float ownerMaxY = ref maxAABBVectorsY[ownerIndex];
            ref float otherMaxY = ref maxAABBVectorsY[otherIndex];            

            // broad phase AABB intersect check.
            if(Intersect(ownerMinX, ownerMinY, ownerMaxX, ownerMaxY, ownerMinX, ownerMinY, ownerMaxX, ownerMaxY))
            {                
                // narrow phase SAT intersect check.
                if(PolygonsIntersect(ownerVertsX, ownerVertsY, otherVertsX, otherVertsY, ownerCentroidX, ownerCentroidY, 
                    otherCentroidX, otherCentroidY, out normalX, out normalY, out depth
                ))
                {
                    FindContactPoints(ownerVertsX, ownerVertsY, otherVertsX, otherVertsY, PolygonContactPointEpsilon, 
                        out firstContactPointX, out firstContactPointY, out secondContactPointX, out secondContactPointY, 
                        out contactCount
                    );
             
                    switch (contactCount)
                    {
                        case 1:
                            AppendCollision(collisionsToResolve, ownerIndex, ownerGeneration, otherIndex, otherGeneration,
                                normalX, normalY, ownerCentroidX, ownerCentroidY, otherCentroidX, otherCentroidY,
                                firstContactPointX, firstContactPointY, depth, ownerFlag, otherFlag
                            );
                            break;
                        case 2:
                            AppendCollision(collisionsToResolve, ownerIndex, ownerGeneration, otherIndex, otherGeneration,
                                normalX, normalY, ownerCentroidX, ownerCentroidY, otherCentroidX, otherCentroidY,
                                firstContactPointX, firstContactPointY,secondContactPointX, secondContactPointY, depth, ownerFlag, otherFlag
                            );
                            break;
                    }                
                }
            }
        }

    }

    public static void FindPolygonToCircleCollisions(Soa_Collision collisionsToResolve, Soa_SpatialPair spatialPairs, 
        Soa_Vector2 vertices, Soa_Vector2 minAABBVectors, Soa_Vector2 maxAABBVectors, Soa_Vector2 centroids,
        Span<int> firstVertexIndices, Span<int> nextVertexIndices, Span<float> radii, 
        int maxPolygonVerticeCount
    )
    {
        // hoisting of invariance.
        Span<int> ownerIndices      = spatialPairs.OwnerGenIndices.Indices;
        Span<int> ownerGenerations  = spatialPairs.OwnerGenIndices.Generations;
        Span<int> otherIndices      = spatialPairs.OtherGenIndices.Indices;
        Span<int> otherGenerations  = spatialPairs.OtherGenIndices.Generations;
        Span<float> verticesX       = vertices.X;
        Span<float> verticesY       = vertices.Y;
        Span<float> minAABBVectorsX = minAABBVectors.X;
        Span<float> minAABBVectorsY = minAABBVectors.Y;
        Span<float> maxAABBVectorsX = maxAABBVectors.X;
        Span<float> maxAABBVectorsY = maxAABBVectors.Y;
        Span<float> centroidsX      = centroids.X;
        Span<float> centroidsY      = centroids.Y;
        Span<int> ownerFlags       = spatialPairs.OwnerFlags;
        Span<int> otherFlags       = spatialPairs.OtherFlags;

        // declarations:
        float normalX = 0;
        float normalY = 0;
        float depth = 0;
        float contactPointX = 0;
        float contactPointY = 0;
        int polygonVertexCount = 0;
        Span<float> polygonX = stackalloc float[maxPolygonVerticeCount];
        Span<float> polygonY = stackalloc float[maxPolygonVerticeCount];

        for(int i = 0; i < spatialPairs.Count; i++)
        {            
            ref int polygonIndex        = ref ownerIndices[i];
            ref int polygonGeneration   = ref ownerGenerations[i];
            ref int circleIndex         = ref otherIndices[i];
            ref int circleGeneration    = ref otherGenerations[i];
            PhysicsBodyFlags ownerFlag = (PhysicsBodyFlags)ownerFlags[i];
            PhysicsBodyFlags otherFlag = (PhysicsBodyFlags)otherFlags[i];

            // get circle data.
            ref float circleX = ref verticesX[firstVertexIndices[circleIndex]];
            ref float circleY = ref verticesY[firstVertexIndices[circleIndex]];
            ref float circleR = ref radii[circleIndex]; 
            ref float polygonMinX = ref minAABBVectorsX[polygonIndex];
            ref float circleMinX = ref minAABBVectorsX[circleIndex];
            ref float polygonMinY = ref minAABBVectorsY[polygonIndex];
            ref float circleMinY = ref minAABBVectorsY[circleIndex];
            ref float polygonMaxX = ref maxAABBVectorsX[polygonIndex];
            ref float circleMaxX = ref maxAABBVectorsX[circleIndex];
            ref float polygonMaxY = ref maxAABBVectorsY[polygonIndex];
            ref float circleMaxY = ref maxAABBVectorsY[circleIndex];
            ref float polygonCentroidX = ref centroidsX[polygonIndex];
            ref float polygonCentroidY = ref centroidsY[polygonIndex];

            // get polygon data.
            GetPolygonVertices(vertices, firstVertexIndices, nextVertexIndices, polygonX, polygonY, polygonIndex, 
                ref polygonVertexCount
            );

            // broad phase intersect check.
            if(Intersect(polygonMinX, polygonMinY, polygonMaxX, polygonMaxY, circleMinX, circleMinY, circleMaxX, circleMaxY))
            {                
                // narrow phase intersect check.
                if(PolygonAndCircleIntersect(polygonX, polygonY, polygonCentroidX, polygonCentroidY, circleX, 
                    circleY, circleR, circleX, circleY, out normalX, out normalY, out depth
                ))
                {
                    FindContactPoints(polygonX, polygonY, circleX, circleY, out contactPointX, out contactPointY);
                    AppendCollision(collisionsToResolve, polygonIndex, polygonGeneration, circleIndex, circleGeneration,
                        normalX, normalY, polygonCentroidX, polygonCentroidY, circleX, circleY,
                        contactPointX, contactPointY, depth, ownerFlag, otherFlag
                    );
                }
            }
        }
    }

    /// <summary>
    /// Gets whether or not two circle bodies are colliding with eachother.
    /// </summary>
    /// <param name="xA">the x-component of circle a's position vector.</param>
    /// <param name="yA">the y-component of circle a's position vector.</param>
    /// <param name="rA">the radius of circle a.</param>
    /// <param name="xB">the x-component of circle b's position vector.</param>
    /// <param name="yB">the y-component of circle b's position vector.</param>
    /// <param name="rB">the radius of circle b.</param>
    /// <param name="minXA">the x-component of the minimum AABB vector of circle A.</param>
    /// <param name="minYA">the y-component of the minimum AABB vector of circle A.</param>
    /// <param name="maxXA">the x-component of the maximum AABB vector of circle A.</param>
    /// <param name="maxYA">the y-component of the maximum AABB vector of circle A.</param>
    /// <param name="minXB">the x-component of the minimum AABB vector of circle B.</param>
    /// <param name="minYB">the y-component of the minimum AABB vector of circle B.</param>
    /// <param name="maxXB">the x-component of the maximum AABB vector of circle B.</param>
    /// <param name="maxYB">the y-component of the maximum AABB vector of circle B.</param>
    /// <param name="nX">a float to store the x-component of the normal vector.</param>
    /// <param name="nY">a float to store the y-component of the normal vector.</param>
    /// <param name="d">a float to the store the depth of the collision.</param>
    /// <param name="cX">a float to store the x-component of the contact point vector.</param>
    /// <param name="cY">a float to store the y-component of the contact point vector.</param>
    /// <returns>true, if the two bodies are colliding; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool CircleBodiesAreColliding(float xA, float yA, float rA, float xB, float yB, float rB,
        float minXA, float minYA, float maxXA, float maxYA,
        float minXB, float minYB, float maxXB, float maxYB,
        ref float nX, ref float nY, ref float d, ref float cX, ref float cY
    )
    {
        // Broad Phase:
        if (Intersect(minXA, minYA, maxXA, maxYA, minXB, minYB, maxXB, maxYB) == false)
        {
            return false;
        }

        // Narrow Phase:
        // perform an SAT check.
        if(CirclesIntersect(xA, yA, rA, xB, yB, rB, out nX, out nY, out d))
        {
            // submit the collision with contact points if one of the colliders needs them.
            FindContactPoints(xA, yA, rA, xB, yB, out cX, out cY);
            return true;
        }   
        return false;
    }




    /*******************
    
        Utility.
    
    ********************/




    /// <summary>
    /// Adds un-transformed vertices into a physics system state.
    /// </summary>
    /// <remarks>
    /// Note: the next index for a given shape is inserted as a circular intrusive linked list; 
    /// meaning that the next vertice index of the final vertice will be the first vertice index. 
    /// </remarks>
    /// <param name="state">the physics system state to insert into.</param>
    /// <param name="verticesX">the x-component values of the vertices to insert.</param>
    /// <param name="verticesY">the y-component values of the vertices to insert.</param>
    /// <param name="firstIndex">the index in the physics system state's vertice array that contains the first vertice index in the state's vertice array.</param>
    /// <param name="vertexCount">the amount of vertices added.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">throws if verticesX is not of the same length as verticesY.</exception>
    public static int AddVertices(SoaPhysicsSystemState state, Span<float> verticesX, Span<float> verticesY, out int firstIndex, out int vertexCount)
    {
        if(verticesX.Length != verticesY.Length)
            throw new ArgumentException($"vertices X length '{verticesX.Length}' must be equalt to vertices Y length '{verticesY.Length}'");

        vertexCount = verticesX.Length;

        if(vertexCount > state.MaxPhysicsBodyVertexCount)
            throw new ArgumentException($"vertices cannot have a length greater than the state's set max physics body vertice count '{state.MaxPhysicsBodyVertexCount}'");

        // set the first index.
        firstIndex = state.FreeVertexIndex.Pop();
        int previousIndex;
        int index = firstIndex;
        state.LocalVertices.X[index] = verticesX[0];
        state.LocalVertices.Y[index] = verticesY[0];

        // add the rest of them.
        for(int i = 1; i < vertexCount; i++)
        {
            previousIndex = index;
            index = state.FreeVertexIndex.Pop();
            state.LocalVertices.X[index] = verticesX[i];
            state.LocalVertices.Y[index] = verticesY[i];
            state.NextVertexIndices[previousIndex] = index;
        }

        // loop back to the beginning.
        // note: this is very important, do not remove this.
        state.NextVertexIndices[index] = firstIndex;

        return firstIndex;
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
    
        Circle Body Allocation.
    
    ********************/




    /// <summary>
    /// Allocates a circle collider into a phsyics system state.
    /// </summary>
    /// <param name="state">The physics system state to allocate into.</param>
    /// <param name="shape">the shape data of the circle.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateCircleCollider(SoaPhysicsSystemState state, Circle shape, 
        bool isKinematic, bool isTrigger, ref GenIndex genIndex
    )
    {
        // handle flags.
        // note: no circle shape is needed to be set as it is implied by the system that when a shape is not
        // set, a physics body is a circle.
        PhysicsBodyFlags flag = PhysicsBodyFlags.None; 

        SetActive(ref flag, true);
        SetAllocated(ref flag, true);
        SetRigidBody(ref flag, false);
        SetTrigger(ref flag, isTrigger);
        SetKinematic(ref flag, isKinematic);

        int index = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, [shape.X], [shape.Y], out int verticesFirstIndex, out int verticeCount);

        // apply data.
        state.LocalRadii[index]              = shape.Radius;
        state.Flags[index]              = flag;
        state.FirstVertexIndices[index]  = verticesFirstIndex;

        state.AlloctedPhysicsBodyCount++;

        genIndex.Index = index;
        genIndex.Generation = state.Generations[index];
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
    public static void AllocateCircleRigidBody(SoaPhysicsSystemState state, Circle shape, PhysicsMaterial physicsMaterial, bool isKinematic, bool isTrigger, bool rotationalPhysics, ref GenIndex genIndex)
    {
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
        AddVertices(state, [shape.X], [shape.Y], out int verticesFirstIndex, out int verticeCount);
        int index = state.FreePhysicsBodyIndex.Pop();

        // apply data.
        state.LocalRadii[index]                      = shape.Radius;
        state.Flags[index]                      = flags;
        state.FirstVertexIndices[index]         = verticesFirstIndex;
        state.StaticFrictions[index]            = physicsMaterial.StaticFriction;
        state.KineticFrictions[index]           = physicsMaterial.KineticFriction;
        state.Densities[index]                  = physicsMaterial.Density;
        
        // reset forces
        ClearForcesAndVelocities(state, index);

        // return gen index.

        genIndex = new(index, state.Generations[index]);        
        
        state.AlloctedPhysicsBodyCount++;
    }




    /*******************
    
        Rectangle Body Allocation.
    
    ********************/




    /// <summary>
    /// Allocates a rectangle collider into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate a physics body into.</param>
    /// <param name="shape">the rectangle shape data of the physics body.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateRectangleCollider(SoaPhysicsSystemState state, Rectangle shape, bool isKinematic, bool isTrigger, ref GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.RectangleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticesFirstIndex, out int verticeCount);

        state.LocalHeights[bodyIndex]            = shape.Height;
        state.LocalWidths[bodyIndex]             = shape.Width;
        state.Flags[bodyIndex]              = flags;
        state.FirstVertexIndices[bodyIndex]  = verticesFirstIndex;

        // return gen index.

        genIndex = new(bodyIndex, state.Generations[bodyIndex]);    

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
    public static void AllocateRectangleRigidBody(SoaPhysicsSystemState state, Rectangle shape, PhysicsMaterial physicsMaterial, bool isKinematic, bool isTrigger, bool rotationalPhysics, ref GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.RectangleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);
        SetRotationalPhysics(ref flags, rotationalPhysics);

        PolygonRectangle polyRect = new(shape);

        int index = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticesFirstIndex, out int verticeCount);

        // apply data.
        state.LocalHeights[index]                    = shape.Height;
        state.LocalWidths[index]                     = shape.Width;
        state.Flags[index]                      = flags;
        state.FirstVertexIndices[index]         = verticesFirstIndex;
        state.KineticFrictions[index]           = physicsMaterial.KineticFriction;
        state.StaticFrictions[index]            = physicsMaterial.StaticFriction;
        state.Densities[index]                  = physicsMaterial.Density;
        state.Restitutions[index]               = physicsMaterial.Restitution;

        // reset forces.
        ClearForcesAndVelocities(state, index);

        // return gen index.

        genIndex = new(index, state.Generations[index]);
    
        state.AlloctedPhysicsBodyCount++;
    }

    public static void ResolveColliderCollisions(Soa_Collision collisions, Soa_Transform transforms)
    {
        // hoisting invariance.
        float depth;
        float displacementX;
        float displacementY;
        Span<float> positionX = transforms.Position.X;
        Span<float> positionY = transforms.Position.Y;
        Span<float> normalX = collisions.Normals.X;
        Span<float> normalY = collisions.Normals.Y;
        Span<float> depths = collisions.Depths;
        Span<int> ownerIndices = collisions.OwnerGenIndices.Indices;
        Span<int> otherIndices = collisions.OtherGenIndices.Indices;
        Span<PhysicsBodyFlags> ownerFlags = collisions.OwnerFlags;
        Span<PhysicsBodyFlags> otherFlags = collisions.OtherFlags;

        for(int i = 0; i < collisions.Count; i++)
        {
            ref int ownerIndex = ref ownerIndices[i];
            ref int otherIndex = ref otherIndices[i];
            ref PhysicsBodyFlags ownerFlag = ref ownerFlags[i];
            ref PhysicsBodyFlags otherFlag = ref otherFlags[i];

            if((ownerFlag & PhysicsBodyFlags.Kinematic) != 0 && (otherFlag & PhysicsBodyFlags.Kinematic) != 0)
            {
                // two kinematic bodies wont collide with eachother.
                continue;
            }
            else
            {
                depth = depths[i];
                displacementX = normalX[i] * depth;
                displacementY = normalY[i] * depth;

                if((ownerFlag & PhysicsBodyFlags.Kinematic) != 0)
                {
                    // only move away the 'other' if the 'owner' is kinematic.
                    positionX[otherIndex] += displacementX;
                    positionY[otherIndex] += displacementY;
                }
                else if((otherFlag & PhysicsBodyFlags.Kinematic) != 0)
                {
                    // only move away the 'owner' if the other is kinematic.
                    positionX[ownerIndex] -= displacementX;
                    positionY[ownerIndex] -= displacementY;
                }
                else
                {
                    // move 'owner' and 'other' away from eachother.
                    displacementX *= 0.5f;
                    displacementY *= 0.5f;
                    positionX[otherIndex] += displacementX;
                    positionY[otherIndex] += displacementY;
                    positionX[ownerIndex] -= displacementX;
                    positionY[ownerIndex] -= displacementY;
                }

                // // add the resolved collisions to the collisions list.
                // state.CollisionManifold.Collisions.AddRange(state.CollisionManifold.CollisionsToResolve);
                // state.CollisionManifold.CollisionsToResolve.Clear();
            }
        }
    }

    public static void ResolveRigidBodyCollisions(Soa_Collision collisions, Soa_Vector2 linearVelocities, Span<float> restitutions, 
        Span<float> angularVelocities, Span<float> inverseMasses, Span<float> inverseRotationalInertia, Span<float> kineticFriction, 
        Span<float> staticFriction, Span<float> mass
    )
    {
        // hoisting invariance.
        Span<float> normalsX = collisions.Normals.X;
        Span<float> normalsY = collisions.Normals.Y;
        Span<float> depths = collisions.Depths;
        Span<float> firstContactPointsX = collisions.FirstContactPoints.X;
        Span<float> firstContactPointsY = collisions.FirstContactPoints.Y;
        Span<float> secondContactPointsX = collisions.SecondContactPoints.X;
        Span<float> secondContactPointsY = collisions.SecondContactPoints.Y;
        Span<float> ownerCentroidsX = collisions.OwnerCentroids.X;
        Span<float> ownerCentroidsY = collisions.OwnerCentroids.Y;
        Span<float> otherCentroidsX = collisions.OtherCentroids.X;
        Span<float> otherCentroidsY = collisions.OtherCentroids.Y;
        Span<float> linearVelocitiesX = linearVelocities.X;
        Span<float> linearVelocitiesY = linearVelocities.Y;
        Span<int> ownerIndices = collisions.OwnerGenIndices.Indices;
        Span<int> otherIndices = collisions.OtherGenIndices.Indices;
        Span<bool> twoContactPoints = collisions.TwoContactPoints;
        Span<PhysicsBodyFlags> ownerFlags = collisions.OwnerFlags;
        Span<PhysicsBodyFlags> otherFlags = collisions.OtherFlags;

        Span<float> impulseMagnitudes = stackalloc float[MaxCollisionContactPoints]; 
        Span<float> contactPointsX = stackalloc float[MaxCollisionContactPoints];
        Span<float> contactPointsY = stackalloc float[MaxCollisionContactPoints];

        for(int i = 0; i < collisions.Count; i++)
        {
            ref PhysicsBodyFlags ownerFlag = ref ownerFlags[i];
            ref PhysicsBodyFlags otherFlag = ref otherFlags[i];

            if((ownerFlag & PhysicsBodyFlags.RigidBody) == 0 && (otherFlag & PhysicsBodyFlags.RigidBody) == 0)
                continue;

            ref int ownerIndex = ref ownerIndices[i];
            ref int otherIndex = ref otherIndices[i];
            ref float normalX = ref normalsX[i];
            ref float normalY = ref normalsY[i];
            
            ref float ownerCentroidX = ref ownerCentroidsX[i];
            ref float ownerCentroidY = ref ownerCentroidsY[i];
            ref float ownerRestitution = ref restitutions[ownerIndex];
            ref float ownerAngularVelocity = ref angularVelocities[ownerIndex];
            ref float ownerLinearVelocityX = ref linearVelocitiesX[ownerIndex];
            ref float ownerLinearVelocityY = ref linearVelocitiesY[ownerIndex];
            ref float ownerInverseMass = ref inverseMasses[ownerIndex];
            ref float ownerInverseRotationalInertia = ref inverseRotationalInertia[ownerIndex];
            ref float ownerStaticFriction = ref staticFriction[ownerIndex];
            ref float ownerKineticFriction = ref kineticFriction[ownerIndex];
            ref float ownerMass = ref mass[ownerIndex];

            ref float otherCentroidX = ref otherCentroidsX[i];
            ref float otherCentroidY = ref otherCentroidsY[i];
            ref float otherRestitution = ref restitutions[otherIndex];
            ref float otherAngularVelocity = ref angularVelocities[otherIndex];
            ref float otherLinearVelocityX = ref linearVelocitiesX[otherIndex];
            ref float otherLinearVelocityY = ref linearVelocitiesY[otherIndex];
            ref float otherInverseMass = ref inverseMasses[otherIndex];
            ref float otherInverseRotationalInertia = ref inverseRotationalInertia[otherIndex];
            ref float otherStaticFriction = ref staticFriction[otherIndex];
            ref float otherKineticFriction = ref kineticFriction[otherIndex];
            ref float otherMass = ref mass[otherIndex];

            int contactPointsCount;
            if (twoContactPoints[i])
            {
                contactPointsCount = 2;
                contactPointsX[0] = firstContactPointsX[i];
                contactPointsX[1] = secondContactPointsX[i];
                contactPointsY[0] = firstContactPointsY[i];
                contactPointsY[1] = secondContactPointsY[i];
            }
            else
            {
                contactPointsCount = 1;  
                contactPointsX[0] = firstContactPointsX[i];
                contactPointsY[0] = firstContactPointsY[i];
            }

            // friction and rotational resolution are tightly coupled with eachother.
            // do not remove them from eachother.
            // if((ownerFlag & PhysicsBodyFlags.RotationalPhysics) != 0 || (otherFlag & PhysicsBodyFlags.RotationalPhysics) != 0)
            // {
                // note: order matters here, do collision resolution
                //  first do that the impulse magnitudes span is
                // filled with the correct data to perform friction resolution.

            // ResolveRigidBodyCollisionBasic(ref ownerLinearVelocityX, ref ownerLinearVelocityY, ref otherLinearVelocityX,
            //     ref otherLinearVelocityY, ref normalX, ref normalY, ref ownerRestitution, ref otherRestitution,
            //     ref ownerInverseMass, ref otherInverseMass, ref ownerMass, ref otherMass,
            //     ref ownerFlag, ref otherFlag
            // );       

            ResolveRigidBodyCollisionRotational(
                impulseMagnitudes, 
                contactPointsX, 
                contactPointsY, 
                ref ownerRestitution, 
                ref otherRestitution, 
                ref ownerCentroidX, 
                ref ownerCentroidY, 
                ref otherCentroidX, 
                ref otherCentroidY, 
                ref ownerAngularVelocity,
                ref otherAngularVelocity, 
                ref ownerLinearVelocityX, 
                ref ownerLinearVelocityY, 
                ref otherLinearVelocityX,
                ref otherLinearVelocityY, 
                ref normalX, 
                ref normalY, 
                ref ownerInverseMass, 
                ref otherInverseMass,
                ref ownerInverseRotationalInertia, 
                ref otherInverseRotationalInertia, 
                ref ownerFlag, 
                ref otherFlag,
                contactPointsCount
            );  

            ResolveRigidBodyFriction(impulseMagnitudes, contactPointsX, contactPointsY, ref ownerStaticFriction, ref otherStaticFriction, ref ownerKineticFriction,
                ref otherKineticFriction, ref ownerCentroidX, ref otherCentroidX, ref ownerCentroidY, ref otherCentroidY, ref ownerAngularVelocity, 
                ref otherAngularVelocity, ref ownerLinearVelocityX, ref otherLinearVelocityX, ref ownerLinearVelocityY, ref otherLinearVelocityY, 
                ref ownerInverseMass, ref otherInverseMass, ref ownerInverseRotationalInertia, ref otherInverseRotationalInertia, ref normalX, 
                ref normalY, ref ownerFlag, ref otherFlag, contactPointsCount
            );
        }
    }

    public static void ResolveRigidBodyCollisionBasic(ref float ownerLinearVelocityX, ref float ownerLinearVelocityY, ref float otherLinearVelocityX,
        ref float otherLinearVelocityY, ref float normalX, ref float normalY, ref float ownerRestitution, ref float otherRestitution,
        ref float ownerInverseMass, ref float otherInverseMass, ref float ownerMass, ref float otherMass,
        ref PhysicsBodyFlags ownerFlag, ref PhysicsBodyFlags otherFlag
    )
    {
        float relativeVelocityX = otherLinearVelocityX - ownerLinearVelocityX;
        float relativeVelocityY = otherLinearVelocityY - ownerLinearVelocityY;

        // the magnitude of the relative velocity relative to the normal
        float magnitude = Dot(relativeVelocityX, relativeVelocityY, normalX, normalY);

        if(magnitude > 0)
        {
            return;
        }

        float restitution = MathF.Min(ownerRestitution, otherRestitution);

        // magnitude of the impulse
        float impulseMagnitude = -(1f + restitution) * magnitude;
        impulseMagnitude /= ownerInverseMass + otherInverseMass;

        float impulseForceX;
        float impulseForceY;

        if((ownerFlag & PhysicsBodyFlags.Kinematic) == 0 && (ownerFlag & PhysicsBodyFlags.Trigger) == 0)
        {
            impulseForceX = -(impulseMagnitude / ownerMass * normalX);
            impulseForceY = -(impulseMagnitude / ownerMass * normalY);
            ownerLinearVelocityX += impulseForceX;
            ownerLinearVelocityY += impulseForceY;
        }

        if((otherFlag & PhysicsBodyFlags.Kinematic) == 0 && (otherFlag & PhysicsBodyFlags.Trigger) == 0)
        {
            impulseForceX = impulseMagnitude / otherMass * normalX;
            impulseForceY = impulseMagnitude / otherMass * normalY;
            otherLinearVelocityX += impulseForceX;
            otherLinearVelocityY += impulseForceY;
        }
    } 

    public static void ResolveRigidBodyCollisionRotational(
        Span<float> impulseMagnitudes, Span<float> contactPointsX, Span<float> contactPointsY, ref float ownerRestitution, ref float otherRestitution, 
        ref float ownerCentroidX, ref float ownerCentroidY, ref float otherCentroidX, ref float otherCentroidY, ref float ownerAngularVelocity,
        ref float otherAngularVelocity, ref float ownerLinearVelocityX, ref float ownerLinearVelocityY, ref float otherLinearVelocityX,
        ref float otherLinearVelocityY, ref float normalX, ref float normalY, ref float ownerInverseMass, ref float otherInverseMass,
        ref float ownerInverseRotationalInertia, ref float otherInverseRotationalInertia, ref PhysicsBodyFlags ownerFlag, ref PhysicsBodyFlags otherFlag,
        int contactPointsCount
    )
    {             
        float restitution = MathF.Min(ownerRestitution, otherRestitution);

        int count = contactPointsCount;
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
            distsAX[j] = contactPointX - ownerCentroidX;
            distsAY[j] = contactPointY - ownerCentroidY;
            distsBX[j] = contactPointX - otherCentroidX;
            distsBY[j] = contactPointY - otherCentroidY;            
            
            float perpendicularAX = -distsAY[j];
            float perpendicularAY = distsAX[j];
            float perpendicularBX = -distsBY[j];
            float perpendicularBY = distsBX[j];

            float angularVelocityAX = perpendicularAX * ownerAngularVelocity;
            float angularVelocityAY = perpendicularAY * ownerAngularVelocity;
            float angularVelocityBX = perpendicularBX * otherAngularVelocity;
            float angularVelocityBY = perpendicularBY * otherAngularVelocity;

            float relativeVelocityX = (otherLinearVelocityX + angularVelocityBX) - (ownerLinearVelocityX + angularVelocityAX);
            float relativeVelocityY = (otherLinearVelocityY + angularVelocityBY) - (ownerLinearVelocityY + angularVelocityAY);
            
            // the magnitude of the relative velocity relative to the normal
            float magnitude = Dot(relativeVelocityX, relativeVelocityY, normalX, normalY);

            if(magnitude > 0)
            {
                continue;
            }

            // calculate the denominator.
            float perpADotNormal = Dot(perpendicularAX, perpendicularAY, normalX, normalY);
            float perpBDotNormal = Dot(perpendicularBX, perpendicularBY, normalX, normalY);
            float denominator = ownerInverseMass + otherInverseMass + 
                (perpADotNormal * perpADotNormal) * ownerInverseRotationalInertia +
                (perpBDotNormal * perpBDotNormal) * otherInverseRotationalInertia;

            // magnitude of the impulse
            float impulseMagnitude = -(1f + restitution) * magnitude;
            impulseMagnitude /= denominator;

            // divide by the contact point count to ensure that impulse is evenly spread 
            // across all contact points.
            impulseMagnitude /= (float)contactPointsCount;
            
            // save the impulse magnitude for later friction resolution.
            impulseMagnitudes[j] = impulseMagnitude;

            impulsesX[j] = impulseMagnitude * normalX;
            impulsesY[j] = impulseMagnitude * normalY;
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
            if((ownerFlag & PhysicsBodyFlags.Kinematic) == 0 && (ownerFlag & PhysicsBodyFlags.Trigger) == 0) // is dynamic
            {
                // always apply linear force, even if there is no rotational force to apply.

                ownerLinearVelocityX += -impulseX * ownerInverseMass;
                ownerLinearVelocityY += -impulseY * ownerInverseMass;

                if((ownerFlag & PhysicsBodyFlags.RotationalPhysics) != 0)
                {
                    distAX = distsAX[j];
                    distAY = distsAY[j];
                    ownerAngularVelocity += -Cross(distAX, distAY, impulseX, impulseY) * ownerInverseRotationalInertia;
                }
            }   
            if((otherFlag & PhysicsBodyFlags.Kinematic) == 0 && (otherFlag & PhysicsBodyFlags.Trigger) == 0) // is dynamic
            {
                // always apply linear force, even if there is no rotational force to apply.

                otherLinearVelocityX += impulseX * otherInverseMass;
                otherLinearVelocityY += impulseY * otherInverseMass;

                if((otherFlag & PhysicsBodyFlags.RotationalPhysics) != 0)
                {
                    distBX = distsBX[j];
                    distBY = distsBY[j];
                    otherAngularVelocity += Cross(distBX, distBY, impulseX, impulseY) * otherInverseRotationalInertia;

                }
            }
        }
    }

    public static void ResolveRigidBodyFriction(Span<float> collisionResolutionImpulseMagnitudes, Span<float> contactPointsX,
        Span<float> contactPointsY, ref float ownerStaticFriction, ref float otherStaticFriction, ref float ownerKineticFriction,
        ref float otherKineticFriction, ref float ownerCentroidX, ref float otherCentroidX, ref float ownerCentroidY,
        ref float otherCentroidY, ref float ownerAngularVelocity, ref float otherAngularVelocity, 
        ref float ownerLinearVelocityX, ref float otherLinearVelocityX, ref float ownerLinearVelocityY, 
        ref float otherLinearVelocityY, ref float ownerInverseMass, ref float otherInverseMass, 
        ref float ownerInverseRotationalInertia, ref float otherInverseRotationalInertia, ref float normalX, ref float normalY, 
        ref PhysicsBodyFlags ownerFlag, ref PhysicsBodyFlags otherFlag, int contactPointCount
    )
    {
        Span<float> impulsesX   = stackalloc float[contactPointCount];
        Span<float> impulsesY   = stackalloc float[contactPointCount];
        Span<float> distsAX     = stackalloc float[contactPointCount];
        Span<float> distsAY     = stackalloc float[contactPointCount];
        Span<float> distsBX     = stackalloc float[contactPointCount];
        Span<float> distsBY     = stackalloc float[contactPointCount];
        
        // get an approximation of the friction values.
        // this is faster than the actual physics way.
        float staticFriction = 0;
        float kineticFriction = 0;

        staticFriction = (ownerStaticFriction + otherStaticFriction) * 0.5f;
        kineticFriction = (ownerKineticFriction + otherKineticFriction) * 0.5f;
        
        for(int j = 0; j < contactPointCount; j++)
        {
            float contactPointX = contactPointsX[j];
            float contactPointY = contactPointsY[j];

            // get the angular velocity to travel in.
            distsAX[j] = contactPointX - ownerCentroidX;
            distsAY[j] = contactPointY - ownerCentroidY;
            distsBX[j] = contactPointX - otherCentroidX;
            distsBY[j] = contactPointY - otherCentroidY;            
            
            float perpendicularAX = -distsAY[j];
            float perpendicularAY = distsAX[j];
            float perpendicularBX = -distsBY[j];
            float perpendicularBY = distsBX[j];

            float angularVelocityAX = perpendicularAX * ownerAngularVelocity;
            float angularVelocityAY = perpendicularAY * ownerAngularVelocity;
            float angularVelocityBX = perpendicularBX * otherAngularVelocity;
            float angularVelocityBY = perpendicularBY * otherAngularVelocity;

            float relativeVelocityX = (otherLinearVelocityX + angularVelocityBX) - (ownerLinearVelocityX + angularVelocityAX);
            float relativeVelocityY = (otherLinearVelocityY + angularVelocityBY) - (ownerLinearVelocityY + angularVelocityAY);

            // this is the direction the body is travelling in along the contact point surface.
            float relativeDotNormal = Dot(relativeVelocityX, relativeVelocityY, normalX, normalY);
            float tangentX = relativeVelocityX - relativeDotNormal * normalX;
            float tangentY = relativeVelocityY - relativeDotNormal * normalY;

            if(NearlyEqual((tangentX * tangentX) + (tangentY * tangentY), 0, 1e-12f))
                continue;

            Normalise(tangentX, tangentY, out tangentX, out tangentY);

            // calculate the denominator.
            float perpADotTangent = Dot(perpendicularAX, perpendicularAY, tangentX, tangentY);
            float perpBDotTangent = Dot(perpendicularBX, perpendicularBY, tangentX, tangentY);
            float denominator = ownerInverseMass + otherInverseMass + 
                (perpADotTangent * perpADotTangent) * ownerInverseRotationalInertia +
                (perpBDotTangent * perpBDotTangent) * otherInverseRotationalInertia;

            // Calculate the DESIRED friction magnitude to stop all sliding.
            float frictionImpulseMag = -Dot(relativeVelocityX, relativeVelocityY, tangentX, tangentY) / denominator;

            // Coulomb's Law:
            // Limit that desire by the static friction. 
            float maxFriction = collisionResolutionImpulseMagnitudes[j] * staticFriction;

            // the the desired friction amount is greater than static friction
            // that means that the object should be sliding with kinetic friction.
            if (Abs(frictionImpulseMag) > maxFriction)
            {
                // Note: We multiply by the SIGN of frictionImpulseMag to keep the direction correct.
                frictionImpulseMag = (collisionResolutionImpulseMagnitudes[j] * kineticFriction) * MathF.Sign(frictionImpulseMag);
            }

            // Apply the capped magnitude to the tangent vector
            impulsesX[j] = frictionImpulseMag * tangentX;
            impulsesY[j] = frictionImpulseMag * tangentY;
        }

        // keep these outside the for loop so they dont allocate each time.
        float impulseX;
        float impulseY;
        float distAX;
        float distAY;
        float distBX;
        float distBY;

        for(int j = 0; j < contactPointCount; j++)
        {                
            impulseX = impulsesX[j];
            impulseY = impulsesY[j];

            // cross producting the dist and impulse gives a value indicating
            // how much angular velocity - in radians - is needed to be applied based on the impulse direction.
            // this is because cross producting two directions that are parallel to eachother, results in zero.
            // which means that there should be no rotation if the collision is head on.
            // but if the closer the two directions come to being perpendicular to one another,
            // the larger the angular impulse will be, causing the body to rotate.
            if((ownerFlag & PhysicsBodyFlags.Kinematic) == 0 && (ownerFlag & PhysicsBodyFlags.Trigger) == 0) // is dynamic
            {
                // always apply linear force, even if there is no rotational force to apply.
                ownerLinearVelocityX += -impulseX * ownerInverseMass;
                ownerLinearVelocityY += -impulseY * ownerInverseMass;

                if((ownerFlag & PhysicsBodyFlags.RotationalPhysics) != 0)
                {
                    distAX = distsAX[j];
                    distAY = distsAY[j];
                    ownerAngularVelocity += -Cross(distAX, distAY, impulseX, impulseY) * ownerInverseRotationalInertia;
                }
            }
            if((otherFlag & PhysicsBodyFlags.Kinematic) == 0 && (otherFlag & PhysicsBodyFlags.Trigger) == 0) // is dynamic
            {
                // always apply linear force, even if there is no rotational force to apply.
                otherLinearVelocityX += impulseX * otherInverseMass;
                otherLinearVelocityY += impulseY * otherInverseMass;

                if((otherFlag & PhysicsBodyFlags.RotationalPhysics) != 0)
                {
                    distBX = distsBX[j];
                    distBY = distsBY[j];
                    otherAngularVelocity += Cross(distBX, distBY, impulseX, impulseY) * otherInverseRotationalInertia;
                }       
            }   
        } 
    }

    // / <summary>
    // / Clears all forces and velocities being applied to a rigidbody.
    // / </summary>
    /// <param name="state">the phsysics system state.</param>
    /// <param name="bodyIndex">the index of the physics body to stop.</param>
    public static void ClearForcesAndVelocities(SoaPhysicsSystemState state, int bodyIndex)
    {
        state.LinearVelocities.X[bodyIndex] = 0;
        state.LinearVelocities.Y[bodyIndex] = 0;
        state.AngularVelocities[bodyIndex] = 0;
        state.Forces.X[bodyIndex] = 0;
        state.Forces.Y[bodyIndex] = 0;
    }



        
    /*******************
    
        Debug Drawing.
    
    ********************/



    /// <summary>
    /// Draws the branches of a bvh as a wireframe rectangle.
    /// </summary>
    /// <param name="registry">the component registry housing the main camera.</param>
    /// <param name="bvh">the bvh containing the leaves to draw.</param>
    /// <param name="colour">the colour used to draw the wireframes.</param>
    public static void DebugDrawBvhBranches(ComponentRegistry registry, BoundingVolumeHierarchy bvh, Colour colour)
    {
        Span<Branch> branches = AsSpan(bvh.Branches);

        for(int i = 0; i < branches.Length; i++)
        {
            Debug.Draw.Wireframe(
                registry,
                new Transform(Vector2.Zero, Vector2.One, 0),
                new Rectangle(
                    new Vector2(branches[i].BoundingBoxMinX, branches[i].BoundingBoxMinY), 
                    new Vector2(branches[i].BoundingBoxMaxX, branches[i].BoundingBoxMaxY)
                ), 
                colour
            );
        }
    }

    /// <summary>
    /// Draws wireframes for all circle physics bodies.
    /// </summary>
    /// <param name="camera">the camera to draw in relation to.</param>
    /// <param name="vertices">the soa vector containing the vertices for the circles.</param>
    /// <param name="radii">the radii of the circles.</param>
    /// <param name="firstVertexIndices">the index of a circles  positional vertex in the vertices soa vector.</param>
    /// <param name="flags">a span containing the flags of the circles to draw.</param>
    /// <param name="dynamicColour">the colour to draw any 'dynamic' bodies with.</param>
    /// <param name="kinematicColour">the colour to draw any 'kinematic' bodies with.</param>
    /// <param name="triggerColour">the colour to draw any 'trigger' bodies with.</param>
    public static void DrawCirclePhysicsBodies(Camera camera, Soa_Vector2 vertices, Span<float> radii, 
        Span<int> firstVertexIndices, Span<PhysicsBodyFlags> flags, 
        Colour dynamicColour, Colour kinematicColour, Colour triggerColour
    )
    {
        Span<float> verticesX = vertices.X;
        Span<float> verticesY = vertices.Y;
        Colour drawColour;

        for(int i = 0; i < flags.Length; i++)
        {
            ref PhysicsBodyFlags flag = ref flags[i];
            if((flag & PhysicsBodyFlags.Allocated) == 0 || 
                (flag & PhysicsBodyFlags.Active) == 0 || 
                (flag & PhysicsBodyFlags.RectangleShape) != 0
            )
            {
                continue;
            }

            if((flag & PhysicsBodyFlags.Kinematic) != 0)
            {
                drawColour = kinematicColour; 
            }
            else if((flag & PhysicsBodyFlags.Trigger) != 0)
            {
                drawColour = triggerColour;
            }
            else // dynamic body.
            {
                drawColour = dynamicColour;
            }

            Debug.Draw.WireframeCircle(drawColour, camera.Position.X, camera.Position.Y, camera.Zoom,
                verticesX[firstVertexIndices[i]], verticesY[firstVertexIndices[i]], radii[i]
            );
        }    
    }

    /// <summary>
    /// Draws wireframes for all polygon physics bodies.
    /// </summary>
    /// <param name="camera">the camera to draw in relation to.</param>
    /// <param name="vertices">the soa vector containing the vertices for the polygons.</param>
    /// <param name="firstVertexIndices">a span containing the index to the first vertex of a polygon in the vertices soa vector.</param>
    /// <param name="nextVertexIndices">a span containing the index to the next vertex from a given vertex index in the vertices soa vector.</param>
    /// <param name="flags">a span containing the flags of the polygons to draw.</param>
    /// <param name="dynamicColour">the colour to draw 'dynamic' bodies with.</param>
    /// <param name="kinematicColour">the colour to draw 'kinematic' bodies with.</param>
    /// <param name="triggerColour">the colour to draw 'trigger' bodies with.</param>
    /// <param name="maxPolygonVertexCount">the maxmimum amount of vertices a polygon physics body can have.</param>
    public static void DrawPolygonPhysicsBodies(Camera camera, Soa_Vector2 vertices, Span<int> firstVertexIndices, 
        Span<int> nextVertexIndices, Span<PhysicsBodyFlags> flags, 
        Colour dynamicColour, Colour kinematicColour, Colour triggerColour, int maxPolygonVertexCount
    )
    {
        Span<float> verticesX = vertices.X;
        Span<float> verticesY = vertices.Y;
        Colour drawColour;
        Span<float> polygonX = stackalloc float[maxPolygonVertexCount];
        Span<float> polygonY = stackalloc float[maxPolygonVertexCount];
        int vertexCount = 0;

        for(int i = 0; i < flags.Length; i++)
        {
            ref PhysicsBodyFlags flag = ref flags[i];

            if((flag & PhysicsBodyFlags.Allocated) == 0 || 
                (flag & PhysicsBodyFlags.Active) == 0 ||
                (flag & PhysicsBodyFlags.RectangleShape) == 0)
            {
                continue;
            }

            if((flag & PhysicsBodyFlags.Kinematic) != 0)
            {
                drawColour = kinematicColour;               
            }
            else if ((flag & PhysicsBodyFlags.Trigger) != 0)
            {
                drawColour = triggerColour;                
            }
            else // dynamic body.
            {
                drawColour = dynamicColour;
            }

            GetPolygonVertices(verticesX, verticesY, firstVertexIndices, nextVertexIndices,
                polygonX, polygonY, i, ref vertexCount
            );

            Debug.Draw.WireframePolygon(drawColour, camera, polygonX, polygonY, vertexCount);
        }        
    }

    public static void DrawCollisionInformation(Camera camera, Soa_Collision collisions, Colour ownerColour, Colour otherColour, Colour contactPointColour, 
        Colour normalColour, int count
    )
    {
        // hoisitng invariance.
        Span<float> firstContactPointsX = collisions.FirstContactPoints.X;
        Span<float> firstContactPointsY = collisions.FirstContactPoints.Y;
        Span<float> secondContactPointsX = collisions.SecondContactPoints.X;
        Span<float> secondContactPointsY = collisions.SecondContactPoints.Y;
        Span<float> normalsX = collisions.Normals.X;
        Span<float> normalsY = collisions.Normals.Y;
        Span<float> ownerCentroidsX = collisions.OwnerCentroids.X;
        Span<float> ownerCentroidsY = collisions.OwnerCentroids.Y;
        Span<float> otherCentroidsX = collisions.OtherCentroids.X;
        Span<float> otherCentroidsY = collisions.OtherCentroids.Y;
        Span<bool> twoContactPoints = collisions.TwoContactPoints;


        float contactPointX;
        float contactPointY;
        float normalX;
        float normalY;
        float ownerCentroidX;
        float ownerCentroidY;

        float otherCentroidX;
        float otherCentroidY;

        for(int i = 0; i < count; i++)
        {
            // get normal data.
            normalX = normalsX[i];
            normalY = normalsY[i];
            
            // get contact point 1 data.
            contactPointX = firstContactPointsX[i];
            contactPointY = firstContactPointsY[i];
            
            // get centroid data.
            ownerCentroidX = ownerCentroidsX[i];
            ownerCentroidY = ownerCentroidsY[i];
            otherCentroidX = otherCentroidsX[i];
            otherCentroidY = otherCentroidsY[i];

            // draw centroids.
            Debug.Draw.WireframeCircle(camera, new Circle(ownerCentroidX, ownerCentroidY, 0.1f), ownerColour);
            Debug.Draw.WireframeCircle(camera, new Circle(otherCentroidX, otherCentroidY, 0.1f), otherColour);

            // draw contact point 1.
            Debug.Draw.WireframeCircle(camera, new Circle(contactPointX, contactPointY, 0.1f), contactPointColour);
            
            // draw normal from contact point. 
            Debug.Draw.Line(normalColour, camera.Zoom, camera.Position.X, camera.Position.Y, 
                contactPointX, contactPointY, contactPointX + normalX, contactPointY + normalY
            );

            if (twoContactPoints[i])
            {
                // get contact point 2.
                contactPointX = secondContactPointsX[i];
                contactPointY = secondContactPointsY[i];

                // draw contact point 2.
                Debug.Draw.WireframeCircle(camera, new Circle(contactPointX, contactPointY, 0.1f), contactPointColour);
                
                // draw normal from contact point.
                Debug.Draw.Line(normalColour, camera.Zoom, camera.Position.X, camera.Position.Y, 
                    contactPointX, contactPointY, contactPointX + normalX, contactPointY + normalY
                );
            }
        }
    }

    public static void DrawLinearVelocities(Camera camera, Soa_Vector2 linearVelocities, Soa_Vector2 centroids, Span<PhysicsBodyFlags> flags, Colour colour, 
        int count
    )
    {
        // hoisting invariance.
        Span<float> linearVelocitiesX = linearVelocities.X;
        Span<float> linearVelocitiesY = linearVelocities.Y;
        Span<float> centroidsX = centroids.X;
        Span<float> centroidsY = centroids.Y;

        for(int i = 0; i < count; i++)
        {
            ref PhysicsBodyFlags flag = ref flags[i];
            if((flag & PhysicsBodyFlags.Allocated) == 0 || (flag & PhysicsBodyFlags.Active) == 0)
            {
                continue;
            }

            float startX = centroidsX[i];
            float startY = centroidsY[i];
            float endX = startX + linearVelocitiesX[i];
            float endY = startY + linearVelocitiesY[i];

            Debug.Draw.Line(colour, camera.Zoom, camera.Position.X, camera.Position.Y,
                startX, startY, endX, endY
            );
        }
    }

    public static void DrawPositions(Camera camera, Soa_Vector2 positions, Span<PhysicsBodyFlags> flags, Colour colour, int count)
    {
        // hoisting invariance.
        Span<float> positionsX = positions.X;
        Span<float> positionsY = positions.Y;

        for(int i = 0; i < count; i++)
        {
            ref PhysicsBodyFlags flag = ref flags[i];
            if((flag & PhysicsBodyFlags.Allocated) == 0 || (flag & PhysicsBodyFlags.Active) == 0)
            {
                continue;
            }

            Debug.Draw.WireframeCircle(colour, camera.Position.X, camera.Position.Y, camera.Zoom,
                positionsX[i], positionsY[i], 0.1f
            );
        }
    }

    public static void DrawCentroids(Camera camera, Soa_Vector2 centroids, Span<PhysicsBodyFlags> flags, Colour colour, int count)
    {
        // hoisting invariance.
        Span<float> centroidsX = centroids.X;
        Span<float> centroidsY = centroids.Y;

        for(int i = 0 ; i < count; i++)
        {
            ref PhysicsBodyFlags flag = ref flags[i];
            if((flag & PhysicsBodyFlags.Allocated) == 0 || (flag & PhysicsBodyFlags.Active) == 0)
            {
                continue;
            }

            Debug.Draw.WireframeCircle(colour, camera.Position.X, camera.Position.Y, camera.Zoom,
                centroidsX[i], centroidsY[i], 0.1f
            );
        }
    }
}