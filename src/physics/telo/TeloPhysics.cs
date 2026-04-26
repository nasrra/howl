using System;
using Howl.Ecs;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Graphics;
using System.Runtime.CompilerServices;
using System.Numerics;
using static Howl.Math.Math;
using static Howl.Math.Shapes.ShapeUtils;
using Howl.DataStructures.Bvh;

namespace Howl.Physics.Telo;

public static class TeloPhysics
{
    public const float RectangleRotationalInertia = 0.0833333333333f;
    public const float CircleRotationalInertia = 0.5f;
    public const float MinBodySize = float.Epsilon;
    public const float MaxBodySize = float.MaxValue;
    public const int MaxCollisionContactPoints = 2;

    public static readonly Vector<float> VectorRectangleRotationalInertia = new(RectangleRotationalInertia);
    public static readonly Vector<float> VectorCircleRotationalInertia = new(CircleRotationalInertia);

    public static void RegisterComponents(ComponentRegistry registry)
    {
        ComponentRegistry.RegisterComponent<Transform>(registry);
        ComponentRegistry.RegisterComponent<PhysicsBodyComponent>(registry);
    }

    public static void FixedUpdate(EcsState ecs, TeloPhysicsState state, float deltaTime, int subSteps)
    {
        state.FixedUpdateStepStopwatch.Restart();

        // Sync Colliders to Transforms Step.
        state.SyncTransformsToEntitiesStopwatch.Restart();
        SyncTransformsToEntityTransforms(ecs, state.Transforms, state.Generations);
        state.SyncTransformsToEntitiesStopwatch.Stop();

        state.IntegrateBodyPropertiesStopwatch.Restart();
        IntegrateBodyProperties(state.Transforms.Scales.X, state.Transforms.Scales.Y, state.Masses, state.InverseMasses, 
            state.RotationalInertia, state.InverseRotationalInertia, state.PhysicsMaterials.Density, state.LocalRadii, state.WorldRadii, 
            state.LocalWidths, state.LocalHeights, state.Flags, state.MaxPhysicsBodyCount
        );
        state.IntegrateBodyPropertiesStopwatch.Stop();

        // scale delta time by the substeps.
        deltaTime /= (float)subSteps;

        for(int i = 0; i < subSteps; i++)
        {
            state.FixedUpdateSubStepStopwatch.Restart();

            Soa_Collision.Clear(state.CollisionManifold.Collisions);

            // RigidBody Movement Step.
            state.RigidBodyMovementStepStopwatch.Restart();
            RigidBodyMovementStep(state.Transforms, state.LinearVelocities, state.Forces, 
                state.Masses, state.Flags, state.AngularVelocities, 
                state.GravityDirection.X, state.GravityDirection.Y, state.Gravity, deltaTime, state.MaxPhysicsBodyCount
            );
            state.RigidBodyMovementStepStopwatch.Stop();

            // transform physics bodies
            state.TransformPhysicsBodiesStopwatch.Restart();
            TransformPhysicsBodyVertices(state.Centroids, state.MinAABBVertices, state.MaxAABBVertices, state.LocalVertices, state.WorldVertices, state.Transforms, 
                state.Flags, state.LocalRadii, state.WorldRadii, state.LocalWidths, state.LocalHeights, state.MaxPhysicsBodyCount, state.AlloctedPhysicsBodyCount
            );
            state.TransformPhysicsBodiesStopwatch.Stop();

            // Reconstruct Bvh.
            state.BvhReconstructionStopwatch.Restart();
            ReconstructBvhTree(state.MinAABBVertices, state.MaxAABBVertices, state.Centroids, state.Flags, state.BvhIndices, state.Bvh);
            state.BvhReconstructionStopwatch.Stop();

            // Find collisions.
            state.FindCollisionsStopwatch.Restart();
            FindCollisions(state.Bvh, state.Entities.GenIds, state.CollisionManifold.Collisions, state.Centroids, state.WorldVertices, 
                state.WorldRadii, state.Flags, state.MaxPhysicsBodyVertexCount
            );
            state.FindCollisionsStopwatch.Stop();

            // Resolve Collider Collisions.
            // NOTE: ordering matters here, make sure to resolve 
            // collisions before sorting the collision manifold.
            // Also make sure that this is above rigidbody collision resolution.
            // this function also moves the transforms of the colliders.
            state.ColliderCollisionResolutionStopwatch.Restart();
            ResolveColliderCollisions(state.CollisionManifold.Collisions, state.Transforms);
            state.ColliderCollisionResolutionStopwatch.Stop();

            // Resolve RigidBody Collisions.
            // NOTE: ordering matters here, make sure to resolve 
            // collisions before sorting the collision manifold.
            // Also make sure that this is below collision resolution.
            // this function also moves the transforms of the colliders.
            state.RigidBodyCollisionResolutionStepStopwatch.Restart();
            ResolveRigidBodyCollisions(state.CollisionManifold.Collisions, state.LinearVelocities, state.PhysicsMaterials.Restitution, state.AngularVelocities, 
                state.InverseMasses, state.InverseRotationalInertia, state.PhysicsMaterials.KineticFriction, state.PhysicsMaterials.StaticFriction, 
                state.Masses
            );
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
            TransformPhysicsBodyVertices(state.Centroids, state.MinAABBVertices, state.MaxAABBVertices, state.LocalVertices, state.WorldVertices, state.Transforms, 
                state.Flags, state.LocalRadii, state.WorldRadii, state.LocalWidths, state.LocalHeights, state.MaxPhysicsBodyCount, state.AlloctedPhysicsBodyCount
            );
        state.FixedUpdateStepStopwatch.Stop();
    }

    public static void Draw(HowlApp app, TeloPhysicsState state, float deltaTime)
    {
        if(state.DrawBvhBranches)
        {
            BoundingVolumeHierarchy.DrawBranches(app, state.Bvh, Colour.Yellow);
        }

        if (state.DrawColliderWireframes)
        {
            DrawCirclePhysicsBodies(app, state.Centroids, state.WorldRadii, state.Flags, state.DynamicPhysicsBodyColour, 
                state.KinematicPhysicsBodyColour, state.TriggeredPhysicsBodyColour
            );

            DrawPolygonPhysicsBodies(app, state.WorldVertices, state.Flags, state.DynamicPhysicsBodyColour, state.KinematicPhysicsBodyColour, 
                state.TriggerPhysicsBodyColour 
            );
        }

        if (state.DrawCollisionInformation)
        {
            DrawCollisionInformation(app, state.CollisionManifold.Collisions, state.CollisionOwnerColour, state.CollisionOtherColour, 
                state.ContactPointColour, state.NormalColour, state.CollisionManifold.Collisions.Count
            );
        }

        if (state.DrawAABBWireframes)
        {
            DrawAabbs(app, state.MinAABBVertices, state.MaxAABBVertices, state.Flags, state.AABBColour);            
        }

        if (state.DrawLinearVelocities)
        {
            DrawLinearVelocities(app, state.LinearVelocities, state.Centroids, state.Flags, state.LinearVelocityColour, 
                state.MaxPhysicsBodyCount
            );
        }

        if (state.DrawPositions)
        {
            DrawPositions(app, state.Transforms.Positions, state.Flags, state.PositionColour, state.MaxPhysicsBodyCount);
        }

        if (state.DrawCentroids)
        {
            DrawCentroids(app, state.Centroids, state.Flags, state.CentroidColour, state.MaxPhysicsBodyCount);
        }
    }

    /// <summary>
    /// Syncs an SoaTransform collection to entities that contain both a transform component and a physics body id component. 
    /// </summary>
    /// <param name="ecs">the ecs state with the component registry housing the entity components.</param>
    /// <param name="soaTransform">the structure-of-array transforms to mutate in relation to the entity data.</param>
    /// <param name="generation">the generations for each entry in the SOA transform's.</param>
    public static void SyncTransformsToEntityTransforms(EcsState ecs, Soa_Transform soaTransform, Span<int> generation)
    {
        ComponentArray<PhysicsBodyComponent> tagged = EcsState.GetComponents<PhysicsBodyComponent>(ecs);
        ComponentArray<Transform> transforms = EcsState.GetComponents<Transform>(ecs);
        
        for(int i = 1; i < tagged.Active.Count; i++)
        {
            GenId genId = tagged.Active[i];
            ref PhysicsBodyComponent tag = ref ComponentArray.GetDataUnsafe(tagged, genId);            

            // skip if the physics body id isn't valid.
            if(generation[GenId.GetIndex(tag.GenId)] != GenId.GetGeneration(tag.GenId))
                continue;
            
            // sync the transform data to the physics simulation 
            // if it has an associated physics body id.
            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, genId);
            Soa_Transform.Insert(soaTransform, GenId.GetIndex(genId), transform);
        }
    }

    public static void DeallocateAllDynamicBodies(TeloPhysicsState state)
    {
        for(int i = 0; i < state.MaxPhysicsBodyCount; i++)
        {            
            if((state.Flags[i] & PhysicsBodyFlags.Allocated) != 0 && (state.Flags[i] & PhysicsBodyFlags.Kinematic) == 0)
            {
                state.Flags[i] = PhysicsBodyFlags.None;
                state.AlloctedPhysicsBodyCount--;
                EntityRegistry.Deallocate(state.Entities, state.Entities.GenIds[i]);
            }
        }
    }

    /// <summary>
    /// Syncs a entities that contain both a transform and physics body id component to an soa transform collection.
    /// </summary>
    /// <param name="transforms">the entity transform components to mutate.</param>
    /// <param name="bodyIds">the entity physics body id components to mutate.</param>
    /// <param name="soaTransform">the soa transforms to copy into the entity transform components.</param>
    /// <param name="generation">the generation of each soa transform entry.</param>
    public static void SyncEntityTransformsToPhysicsBodies(EcsState ecs, Soa_Transform soaTransform, Span<int> generation)
    {
        ComponentArray<PhysicsBodyComponent> tags = EcsState.GetComponents<PhysicsBodyComponent>(ecs);
        ComponentArray<Transform> transforms = EcsState.GetComponents<Transform>(ecs);

        for(int i = 1; i < tags.Active.Count; i++)
        {
            GenId genId = tags.Active[i];
            ref PhysicsBodyComponent tag = ref ComponentArray.GetDataUnsafe(tags, genId);

            // skip the tag if it is stale.
            if(generation[GenId.GetIndex(tag.GenId)] != GenId.GetGeneration(tag.GenId))
            {
                continue;
            }

            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, genId);
            Soa_Transform.CopySoaToTransform(soaTransform, ref transform, GenId.GetIndex(tag.GenId));
        }
    }

    /// <summary>
    /// Performs a movement step for all physics bodies with a rigidbody.
    /// </summary>
    /// <remarks>
    /// All provided spans must be indexed by a integer <c>physicsBodyIndex</c>:
    /// <list type="bullet">
    /// <item><description><paramref name="transforms"/>
    /// <item><description><paramref name="linearVelocities"/>
    /// <item><description><paramref name="masses"/>
    /// <item><description><paramref name="angularVelocities"/>
    /// <item><description><paramref name="flags"/></description></item>
    /// </list>
    /// </remarks>
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
    /// <remarks>
    /// All provided spans must be indexed by a integer <c>physicsBodyIndex</c>:
    /// <list type="bullet">
    /// <item><description><paramref name="transforms"/>
    /// <item><description><paramref name="linearVelocities"/>
    /// <item><description><paramref name="masses"/>
    /// <item><description><paramref name="angularVelocities"/>
    /// <item><description><paramref name="flags"/></description></item>
    /// </list>
    /// </remarks>
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
            Vector<float> vPosX = Vector.LoadUnsafe(ref transforms.Positions.X[i]);
            Vector<float> vPosY = Vector.LoadUnsafe(ref transforms.Positions.Y[i]);
            Vector<float> vSin = Vector.LoadUnsafe(ref transforms.Sins[i]);
            Vector<float> vCos = Vector.LoadUnsafe(ref transforms.Coses[i]);
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
            vPosX.StoreUnsafe(ref transforms.Positions.X[i]);
            vPosY.StoreUnsafe(ref transforms.Positions.Y[i]);
            vCos.StoreUnsafe(ref transforms.Coses[i]);
            vSin.StoreUnsafe(ref transforms.Sins[i]);
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
    /// <remarks>
    /// All provided spans must be indexed by a integer <c>physicsBodyIndex</c>:
    /// <list type="bullet">
    /// <item><description><paramref name="transforms"/>
    /// <item><description><paramref name="linearVelocities"/>
    /// <item><description><paramref name="masses"/>
    /// <item><description><paramref name="angularVelocities"/>
    /// <item><description><paramref name="flags"/></description></item>
    /// </list>
    /// </remarks>
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
        Span<float> positionsX = transforms.Positions.X;
        Span<float> positionsY = transforms.Positions.Y;
        Span<float> sin = transforms.Sins;
        Span<float> cos = transforms.Coses;

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
    /// <param name="worldVertices">output for the generated world-space vertex values of all physics bodies.</param>
    /// <param name="transforms">the world-space transforms of all physics bodies.</param>
    /// <param name="flags">the flags of all physics bodies.</param>
    /// <param name="worldRadii">the world-space radius values of all phsyics bodies.</param>
    /// <param name="localWidths">the local-space width values of all physics bodies.</param>
    /// <param name="localHeights">the local-space height values of all physics bodies.</param>
    /// <param name="maxPhysicsBodyCount">the max amount of physics bodies that can be stored.</param>
    /// <param name="physicsBodyCount">the current amount of allocated physics bodies.</param>
    public static void TransformPhysicsBodyVertices(Soa_Vector2 centroids, Soa_Vector2 minAABBVertices, Soa_Vector2 maxAABBVertices,
        FsSoa_Vector2 localVertices, FsSoa_Vector2 worldVertices, Soa_Transform transforms, Span<PhysicsBodyFlags> flags, 
        Span<float> localRadii, Span<float> worldRadii, Span<float> localWidths, Span<float> localHeights,
        int maxPhysicsBodyCount, int physicsBodyCount
    )
    {
        FsSoa_Vector2.ClearAppendCounts(worldVertices);

        // hoisting invariance.
        Span<float> localVertsX = localVertices.X;
        Span<float> localVertsY = localVertices.Y;
        Span<float> worldVertsX = worldVertices.X;
        Span<float> worldVertsY = worldVertices.Y;
        Span<float> centroidsX = centroids.X;
        Span<float> centroidsY = centroids.Y;
        Span<float> minAABBVectorsX = minAABBVertices.X;
        Span<float> minAABBVectorsY = minAABBVertices.Y;
        Span<float> maxAABBVectorsX = maxAABBVertices.X;
        Span<float> maxAABBVectorsY = maxAABBVertices.Y;
        Span<float> scalesX = transforms.Scales.X;
        Span<float> scalesY = transforms.Scales.Y;
        Span<float> cos = transforms.Coses;
        Span<float> sin = transforms.Sins;
        Span<float> positionsX = transforms.Positions.X;
        Span<float> positionsY = transforms.Positions.Y;
        Span<float> polygonX = default;
        Span<float> polygonY = default;

        int physicsBodiesProcessed = 0;

        for(int physicsBodyIndex = 0; physicsBodyIndex < maxPhysicsBodyCount; physicsBodyIndex++)
        {
            PhysicsBodyFlags flag = flags[physicsBodyIndex];
            
            // if the physics body had been allocated and is active.
            if((flag & PhysicsBodyFlags.Allocated) == 0)
            {
                continue;
            }

            physicsBodiesProcessed++;

            if((flag & PhysicsBodyFlags.Active) != 0)
            {
                // hoisting in variance.
                ref float scaleX = ref scalesX[physicsBodyIndex];
                ref float scaleY = ref scalesY[physicsBodyIndex];

                if((flag & PhysicsBodyFlags.RectangleShape) != 0)
                {
                    int vertexCount = localVertices.AppendCounts[physicsBodyIndex];
                    int startIndex = FixedStrideArray.GetElementIndex(physicsBodyIndex, localVertices.Stride, 0);                        
                    for(int vertex = 0; vertex < vertexCount; vertex++){
                        int currentIndex = vertex + startIndex;

                        // transform the base/un-transformed vertice.
                        TransformVector(localVertsX[currentIndex], localVertsY[currentIndex], scaleX, scaleY,
                            cos[physicsBodyIndex], sin[physicsBodyIndex], positionsX[physicsBodyIndex], positionsY[physicsBodyIndex], 
                            out float x, out float y
                        );

                        // store the newly transformed vertex into the world vertices array.
                        // (TODO): this will need to be changed so that you can append directly to an entry element index
                        // if you already know the element index. Create a new unsafe function for it.
                        FsSoa_Vector2.Append(worldVertices, physicsBodyIndex, x, y);
                    }

                    // set the new centroid.
                    PhysicsBody.GetPolygonVerticesUnsafe(worldVertices, physicsBodyIndex, ref polygonX, ref polygonY);

                    GetCentroid(polygonX, polygonY, ref centroidsX[physicsBodyIndex], ref centroidsY[physicsBodyIndex]);

                    // set the new min and max vectors.
                    GetMinMaxVectors(polygonX, polygonY, out minAABBVectorsX[physicsBodyIndex], out minAABBVectorsY[physicsBodyIndex], 
                        out maxAABBVectorsX[physicsBodyIndex], out maxAABBVectorsY[physicsBodyIndex]
                    );
                }
                else // circle shape.
                {
                    int vertexIndex = FixedStrideArray.GetElementIndex(physicsBodyIndex, worldVertices.Stride, 0);
                    TransformVector(localVertsX[vertexIndex], localVertsY[vertexIndex],scaleX, scaleY, cos[physicsBodyIndex], sin[physicsBodyIndex], positionsX[physicsBodyIndex], positionsY[physicsBodyIndex], out float x, out float y);

                    // store the newly transformed vertex into the world vertices array.
                    // (TODO): this will need to be changed so that you can append directly to an entry element index
                    // if you already know the element index. Create a new unsafe function for it.
                    FsSoa_Vector2.Append(worldVertices, physicsBodyIndex, x, y);

                    // set the new centroid.
                    centroidsX[physicsBodyIndex] = x;
                    centroidsY[physicsBodyIndex] = y;

                    worldRadii[physicsBodyIndex] = Circle.ScaleRadius(localRadii[physicsBodyIndex], scaleX, scaleY);

                    // set the new min and max vectors. 
                    Circle.GetMinMaxVectors(x, y, worldRadii[physicsBodyIndex], 
                        out minAABBVectorsX[physicsBodyIndex], out minAABBVectorsY[physicsBodyIndex], out maxAABBVectorsX[physicsBodyIndex], out maxAABBVectorsY[physicsBodyIndex]
                    );
                }
            }

            if(physicsBodiesProcessed >= physicsBodyCount)
            {
                break;
            }
        }
    }





    /*******************
    
        Integrate Body Properties.
    
    ********************/

    /// <summary>
    /// Calculates world-space dimensions and rigidbody data for physics bodies.
    /// </summary>
    /// <remarks>
    /// All provided spans must be indexed by a integer <c>physicsBodyIndex</c>:
    /// <list type="bullet">
    /// <item><description><paramref name="scalesX"/> / <paramref name="scalesY"/></description></item>
    /// <item><description><paramref name="masses"/> / <paramref name="inverseMasses"/></description></item>
    /// <item><description><paramref name="rotationalInertia"/> / <paramref name="inverseRotationalInertia"/></description></item>
    /// <item><description><paramref name="flags"/></description></item>
    /// </list>
    /// </remarks>
    /// <param name="scalesX">the x-component's of all physics bodies scaling vectors.</param>
    /// <param name="scalesY">the y-component's of all physic bodies scaling vectors.</param>
    /// <param name="masses">output for mass values.</param>
    /// <param name="inverseMasses">output for inverse mass values.</param>
    /// <param name="rotationalInertia">output for rotational inertia values.</param>
    /// <param name="inverseRotationalInertia">output for inverse rotational inertia values.</param>
    /// <param name="densities">the densities of all physics bodies.</param>
    /// <param name="localRadii">the local-space radii of all physics bodies.</param>
    /// <param name="worldRadii">output for world-space radii of all physics bodies.</param>
    /// <param name="localWidths">the local-space widths of all physics bodies.</param>
    /// <param name="localHeights">the local-space heights of all physics bodies.</param>
    /// <param name="flags">the flags of all physics bodies.</param>
    /// <param name="maxPhysicsBodyCount">the maximium amount of physics bodies.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void IntegrateBodyProperties(Span<float> scalesX, Span<float> scalesY, Span<float> masses, Span<float> inverseMasses, 
        Span<float> rotationalInertia, Span<float> inverseRotationalInertia, Span<float> densities, Span<float> localRadii, Span<float> worldRadii, 
        Span<float> localWidths, Span<float> localHeights, Span<PhysicsBodyFlags> flags, int maxPhysicsBodyCount)
    {
        IntegrateBodyProperties_Simd(scalesX, scalesY, masses, inverseMasses, 
            rotationalInertia, inverseRotationalInertia, densities, localRadii, worldRadii, 
            localWidths, localHeights, flags, maxPhysicsBodyCount
        );
    }

    /// <summary>
    /// Calculates world-space dimensions and rigidbody data for physics bodies.
    /// </summary>
    /// <remarks>
    /// All provided spans must be indexed by a integer <c>physicsBodyIndex</c>:
    /// <list type="bullet">
    /// <item><description><paramref name="scalesX"/> / <paramref name="scalesY"/></description></item>
    /// <item><description><paramref name="masses"/> / <paramref name="inverseMasses"/></description></item>
    /// <item><description><paramref name="rotationalInertia"/> / <paramref name="inverseRotationalInertia"/></description></item>
    /// <item><description><paramref name="flags"/></description></item>
    /// </list>
    /// </remarks>
    /// <param name="scalesX">the x-component's of all physics bodies scaling vectors.</param>
    /// <param name="scalesY">the y-component's of all physic bodies scaling vectors.</param>
    /// <param name="masses">output for mass values.</param>
    /// <param name="inverseMasses">output for inverse mass values.</param>
    /// <param name="rotationalInertia">output for rotational inertia values.</param>
    /// <param name="inverseRotationalInertia">output for inverse rotational inertia values.</param>
    /// <param name="densities">the densities of all physics bodies.</param>
    /// <param name="localRadii">the local-space radii of all physics bodies.</param>
    /// <param name="worldRadii">output for world-space radii of all physics bodies.</param>
    /// <param name="localWidths">the local-space widths of all physics bodies.</param>
    /// <param name="localHeights">the local-space heights of all physics bodies.</param>
    /// <param name="flags">the flags of all physics bodies.</param>
    /// <param name="maxPhysicsBodyCount">the maximium amount of physics bodies.</param>
    public static void IntegrateBodyProperties_Simd(Span<float> scalesX, Span<float> scalesY, Span<float> masses, Span<float> inverseMasses, 
        Span<float> rotationalInertia, Span<float> inverseRotationalInertia, Span<float> densities, Span<float> localRadii, Span<float> worldRadii, 
        Span<float> localWidths, Span<float> localHeights, Span<PhysicsBodyFlags> flags, int maxPhysicsBodyCount
    )
    {
        int simdSize = Vector<float>.Count;

        PhysicsBodyFlags RectangleRigidBodyFlags = PhysicsBodyFlags.InUse | PhysicsBodyFlags.RigidBody | PhysicsBodyFlags.RectangleShape;
        PhysicsBodyFlags CircleRigidBodyFlags    = PhysicsBodyFlags.InUse | PhysicsBodyFlags.RigidBody;
        PhysicsBodyFlags CirclePhysicsBodyFlags  = PhysicsBodyFlags.InUse;
        PhysicsBodyFlags NotCircleShapeFlags     = PhysicsBodyFlags.RectangleShape;

        Vector<int> vRectangleRigidBodyFlags = new((int)RectangleRigidBodyFlags);
        Vector<int> vCircleleRigidBodyFlags = new((int)CircleRigidBodyFlags);
        Vector<int> vCirclePhysicsBodyFlags = new((int)CirclePhysicsBodyFlags);
        Vector<int> vNotCircleShapeFlags = new((int)NotCircleShapeFlags);

        int i = 0;
        for(; i <= maxPhysicsBodyCount - simdSize; i += simdSize)
        {
            ref int flagsAsInt = ref Unsafe.As<PhysicsBodyFlags, int>(ref flags[i]);
            Vector<int> vFlags = Vector.LoadUnsafe(ref flagsAsInt);

            Vector<int> flagMask = Vector.Equals(vFlags & vRectangleRigidBodyFlags, vRectangleRigidBodyFlags);

            // short circuit if the entire mask is zero.
            // all bodies in this chunk dont have the required flags.
            if (Vector.EqualsAll(flagMask, Vector<int>.Zero))
            {
                continue;
            }

            // load data.
            Vector<float> vScaleX = Vector.LoadUnsafe(ref scalesX[i]);
            Vector<float> vScaleY = Vector.LoadUnsafe(ref scalesY[i]);
            Vector<float> vHeight = Vector.LoadUnsafe(ref localHeights[i]);
            Vector<float> vWidth = Vector.LoadUnsafe(ref localWidths[i]);
            Vector<float> vMass = Vector.LoadUnsafe(ref masses[i]);
            Vector<float> vInvMass = Vector.LoadUnsafe(ref inverseMasses[i]);
            Vector<float> vRotInertia = Vector.LoadUnsafe(ref rotationalInertia[i]);
            Vector<float> vInvRotInertia = Vector.LoadUnsafe(ref inverseRotationalInertia[i]);
            Vector<float> vDensity = Vector.LoadUnsafe(ref densities[i]);

            // calculate world-space dimensions.
            Vector<float> newHeight = vHeight * vScaleY;
            Vector<float> newWidth = vWidth * vScaleX;

            // calculate the new mass.
            Vector<float> newMass = PhysicsBody.CalculateRectangleMass(newWidth, newHeight, vDensity);

            // create a mask to ensure mass values are above zero.
            Vector<int> massMask = Vector.GreaterThan(newMass, Vector<float>.Zero);

            // calculate new inv mass.
            Vector<float> newInvMass = Vector<float>.One / newMass;

            // set the new mass values.
            // Note: use the mass mask to remove any NaN's as a result of divide by zero.
            newMass = Vector.ConditionalSelect(massMask, newMass, vMass);
            newInvMass = Vector.ConditionalSelect(massMask, newInvMass, vInvMass);
        
            Vector<float> newRotInertia = PhysicsBody.CalculateRectangleRotationalInertia(newWidth, newHeight, newMass);
            
            // create a mask to ensure inerta values are above zero.
            Vector<int> inertiaMask = Vector.GreaterThan(newMass, Vector<float>.Zero);

            // calculate new inverse inertia values.
            Vector<float> newInvRotInertia = Vector<float>.One / newRotInertia;

            // set the new inertia values.
            // Note: use the mass mask to remove any NaN's as a result of divide by zero.
            newRotInertia = Vector.ConditionalSelect(inertiaMask, newRotInertia, vRotInertia);
            newInvRotInertia = Vector.ConditionalSelect(inertiaMask, newInvRotInertia, vInvRotInertia);

            // conditional select (only keep results for valid flags)
            vMass = Vector.ConditionalSelect(flagMask, newMass, vMass);
            vInvMass = Vector.ConditionalSelect(flagMask, newInvMass, vInvMass);
            vRotInertia = Vector.ConditionalSelect(flagMask, newRotInertia, vRotInertia);
            vInvRotInertia = Vector.ConditionalSelect(flagMask, newInvRotInertia, vInvRotInertia);

            // store values.
            vMass.StoreUnsafe(ref masses[i]);
            vInvMass.StoreUnsafe(ref inverseMasses[i]);
            vRotInertia.StoreUnsafe(ref rotationalInertia[i]);
            vInvRotInertia.StoreUnsafe(ref inverseRotationalInertia[i]);
        }

        i = 0;
        for(; i <= maxPhysicsBodyCount - simdSize; i += simdSize)
        {
            ref int flagsAsInt = ref Unsafe.As<PhysicsBodyFlags, int>(ref flags[i]);
            Vector<int> vFlags = Vector.LoadUnsafe(ref flagsAsInt);

            Vector<int> isCircleBodyMask = Vector.Equals(vFlags & vCirclePhysicsBodyFlags, vCirclePhysicsBodyFlags);
            Vector<int> notCircleBodyMask = Vector.Equals(vFlags & vNotCircleShapeFlags, Vector<int>.Zero);
            Vector<int> mask = isCircleBodyMask & notCircleBodyMask;

            // short circuit if there are no circle physics bodies.
            if (Vector.EqualsAll(mask, Vector<int>.Zero))
            {
                continue;
            }

            // load data.
            Vector<float> vRadii = Vector.LoadUnsafe(ref localRadii[i]);
            Vector<float> vScaleX = Vector.LoadUnsafe(ref scalesX[i]);
            Vector<float> vScaleY = Vector.LoadUnsafe(ref scalesY[i]);

            // choose the largest scale.
            Vector<float> vScale = Vector.ConditionalSelect(Vector.GreaterThan(vScaleX, vScaleY), vScaleX, vScaleY);
        
            // transform local to world.
            Vector<float> vNewRadii = vRadii * vScale;

            // apply the radius transformation only to circles.
            vRadii = Vector.ConditionalSelect(mask, vNewRadii, vRadii);

            // store data.
            vRadii.StoreUnsafe(ref worldRadii[i]);
            
            Vector<int> isCircleRigidMask = Vector.Equals(vFlags & vCircleleRigidBodyFlags, vCircleleRigidBodyFlags);
            mask = isCircleRigidMask & notCircleBodyMask;

            // short circuit if there are not circle rigidbodies.
            if(Vector.EqualsAll(mask, Vector<int>.Zero))
            {
                continue;
            }

            Vector<float> vMass = Vector.LoadUnsafe(ref masses[i]);
            Vector<float> vInvMass = Vector.LoadUnsafe(ref inverseMasses[i]);
            Vector<float> vRotInertia = Vector.LoadUnsafe(ref rotationalInertia[i]);
            Vector<float> vInvRotInertia = Vector.LoadUnsafe(ref inverseRotationalInertia[i]);
            Vector<float> vDensity = Vector.LoadUnsafe(ref densities[i]);

            // calculate the new mass.
            Vector<float> newMass = PhysicsBody.CalculateCircleMass(vRadii, vDensity);

            // create a mask to ensure mass values are above zero.
            Vector<int> massMask = Vector.GreaterThan(newMass, Vector<float>.Zero);

            // calculate new inv mass.
            Vector<float> newInvMass = Vector<float>.One / newMass;

            // set the new mass values.
            // Note: use the mass mask to remove any NaN's as a result of divide by zero.
            newMass = Vector.ConditionalSelect(massMask, newMass, vMass);
            newInvMass = Vector.ConditionalSelect(massMask, newInvMass, vInvMass);
        
            Vector<float> newRotInertia = PhysicsBody.CalculateCircleRotationalInertia(vRadii, newMass);
            
            // create a mask to ensure inerta values are above zero.
            Vector<int> inertiaMask = Vector.GreaterThan(newMass, Vector<float>.Zero);

            // calculate new inverse inertia values.
            Vector<float> newInvRotInertia = Vector<float>.One / newRotInertia;

            // set the new inertia values.
            // Note: use the mass mask to remove any NaN's as a result of divide by zero.
            newRotInertia = Vector.ConditionalSelect(inertiaMask, newRotInertia, vRotInertia);
            newInvRotInertia = Vector.ConditionalSelect(inertiaMask, newInvRotInertia, vInvRotInertia);

            // conditional select (only keep results for valid flags)
            vMass = Vector.ConditionalSelect(mask, newMass, vMass);
            vInvMass = Vector.ConditionalSelect(mask, newInvMass, vInvMass);
            vRotInertia = Vector.ConditionalSelect(mask, newRotInertia, vRotInertia);
            vInvRotInertia = Vector.ConditionalSelect(mask, newInvRotInertia, vInvRotInertia);  

            // store values.
            vMass.StoreUnsafe(ref masses[i]);
            vInvMass.StoreUnsafe(ref inverseMasses[i]);
            vRotInertia.StoreUnsafe(ref rotationalInertia[i]);
            vInvRotInertia.StoreUnsafe(ref inverseRotationalInertia[i]);
        }

        // tail end.
        IntegrateBodyProperties_Sisd(scalesX, scalesY, masses, inverseMasses, 
            rotationalInertia, inverseRotationalInertia, densities, localRadii, worldRadii, 
            localWidths, localHeights, flags, maxPhysicsBodyCount, i
        );
    }

    /// <summary>
    /// Calculates world-space dimensions and rigidbody data for physics bodies.
    /// </summary>
    /// <remarks>
    /// All provided spans must be indexed by a integer <c>physicsBodyIndex</c>:
    /// <list type="bullet">
    /// <item><description><paramref name="scalesX"/> / <paramref name="scalesY"/></description></item>
    /// <item><description><paramref name="masses"/> / <paramref name="inverseMasses"/></description></item>
    /// <item><description><paramref name="rotationalInertia"/> / <paramref name="inverseRotationalInertia"/></description></item>
    /// <item><description><paramref name="flags"/></description></item>
    /// </list>
    /// </remarks>
    /// <param name="scalesX">the x-component's of all physics bodies scaling vectors.</param>
    /// <param name="scalesY">the y-component's of all physic bodies scaling vectors.</param>
    /// <param name="masses">output for mass values.</param>
    /// <param name="inverseMasses">output for inverse mass values.</param>
    /// <param name="rotationalInertia">output for rotational inertia values.</param>
    /// <param name="inverseRotationalInertia">output for inverse rotational inertia values.</param>
    /// <param name="densities">the densities of all physics bodies.</param>
    /// <param name="localRadii">the local-space radii of all physics bodies.</param>
    /// <param name="worldRadii">output for world-space radii of all physics bodies.</param>
    /// <param name="localWidths">the local-space widths of all physics bodies.</param>
    /// <param name="localHeights">the local-space heights of all physics bodies.</param>
    /// <param name="flags">the flags of all physics bodies.</param>
    /// <param name="maxPhysicsBodyCount">the maximium amount of physics bodies.</param>
    /// <param name="startIndex">the <c>physicsBodyIndex</c> to start at.</param>    
    public static void IntegrateBodyProperties_Sisd(Span<float> scalesX, Span<float> scalesY, Span<float> masses, Span<float> inverseMasses, 
        Span<float> rotationalInertia, Span<float> inverseRotationalInertia, Span<float> densities, Span<float> localRadii, Span<float> worldRadii, 
        Span<float> localWidths, Span<float> localHeights, Span<PhysicsBodyFlags> flags, int maxPhysicsBodyCount, int startIndex
    )
    {
        float width;
        float height;
        float radius;
        float scaleX;
        float scaleY;
        bool isRigid;

        for(int i = startIndex; i < maxPhysicsBodyCount; i++)
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
                    height = localHeights[i] * scaleY;
                    width = localWidths[i] * scaleX;

                    float mass = PhysicsBody.CalculateRectangleMass(width, height, densities[i]); 
                    masses[i] = mass;
                    inverseMasses[i] = mass == 0? 0 : 1f/mass;

                    float inertia = PhysicsBody.CalculateRectangleRotationalInertia(width, height, mass);
                    rotationalInertia[i] = inertia;
                    inverseRotationalInertia[i] = inertia == 0? 0 : 1f/inertia;
                }
            }
            else // circle shape
            {
                radius = Circle.ScaleRadius(localRadii[i], scaleX, scaleY);
                worldRadii[i] = radius;

                // set rigidbody data if it is enabled.
                if(isRigid)
                {                    
                    float mass = PhysicsBody.CalculateCircleMass(radius, densities[i]);
                    masses[i] = mass;
                    inverseMasses[i] = mass==0? 0 : 1f/mass;

                    float rI = PhysicsBody.CalculateCircleRotationalInertia(radius, mass);
                    rotationalInertia[i] = rI;
                    inverseRotationalInertia[i] = rI == 0? 0f : 1f/rI;
                }
            }

        }
    }

    /// <summary>
    ///     Reconstructs a bounding volume hierarchy tree with physics body data.
    /// </summary>
    /// <remarks>
    ///     Matching lengths are required as data is associated via index (SOA).
    ///     <list type="bullet">
    ///         <item> <paramref name="minAabbs"/> </item>
    ///         <item> <paramref name="maxAabbs"/> </item>
    ///         <item> <paramref name="flags"/> </item>
    ///         <item> <paramref name="centroids"/> </item>
    ///         <item> <paramref name="bvhIndices"/> </item>
    ///     </list>
    /// </remarks>
    /// <param name="vertices">the vertices of all physics bodies to insert into the bounding volume hierarchy.</param>
    /// <param name="radii">the radii of circle physics bodies to calculate the bounding box necessary for insertion in the bounding volume hierarchy.</param>
    /// <param name="generations">the generation for each physics body.</param>
    /// <param name="flags">the flags for each physics body.</param>
    /// <param name="bvhIndices">output for the new indices in the bvh that were assigned to <c>Active</c> and <c>Allocated</c> physics bodies.</param>
    /// <param name="bvh">the bounding volume hierarchy instance.</param>
    public static void ReconstructBvhTree(Soa_Vector2 minAabbs, Soa_Vector2 maxAabbs, Soa_Vector2 centroids, Span<PhysicsBodyFlags> flags, 
        Span<int> bvhIndices, BoundingVolumeHierarchy bvh
    )
    {   
        // clear the previous bvh data.
        BoundingVolumeHierarchy.Clear(bvh);

        for(int i = 0; i < flags.Length; i++)
        {
            ref PhysicsBodyFlags flag = ref flags[i];
            if((flag & PhysicsBodyFlags.Allocated) != 0 && (flag & PhysicsBodyFlags.Active) != 0)
            {
                float minX = minAabbs.X[i];
                float minY = minAabbs.Y[i];
                float maxX = maxAabbs.X[i];
                float maxY = maxAabbs.Y[i];

                // insert into the bvh.
                bvhIndices[i] = Soa_Leaf.Append(bvh.Leaves, minX, minY, maxX, maxY, centroids.X[i], centroids.Y[i]);
            }
        }

        // construct the bvh with the new data.
        // Soa_BoundingVolumeHierarchy.ConstructTree_Slow(bvh);
        BoundingVolumeHierarchy.ConstructTree(bvh);
    }

    public static void FindCollisions(BoundingVolumeHierarchy bvh, GenId[] genIds, Soa_Collision collisions,  Soa_Vector2 centroids, 
        FsSoa_Vector2 vertices, Span<float> radii, Span<PhysicsBodyFlags> flags, int maxPolygonVerticeCount
    )
    {
        // hoisting invariance.
        Soa_Overlap overlaps = bvh.Overlaps;
        PhysicsBodyFlags ownerFlags;
        PhysicsBodyFlags otherFlags;

        for(int i = 0; i < overlaps.AppendCount; i++)
        {
            // get the physics bodies that may be colliding.
            // Note: add one to leaf indices as the bvh doesnt have a Nil value; unlike this physics system.
            int ownerIndex = overlaps.OwnerLeafIndices[i]+1;
            int otherIndex = overlaps.OtherLeafIndices[i]+1;

            // get the flags.
            ownerFlags = flags[ownerIndex];
            otherFlags = flags[otherIndex];

            // perform the narrow phase SAT check.
            // Note: BVH performed Broad Phase Aabb checks already.
            if((ownerFlags & PhysicsBodyFlags.RectangleShape) != 0)
            {
                if((otherFlags & PhysicsBodyFlags.RectangleShape) != 0)
                {
                    PolygonBodiesAreColliding(collisions, centroids, vertices, genIds, ownerIndex, otherIndex, ownerFlags, otherFlags);
                }
                else
                {                    
                    PolygonToCircleBodiesAreColliding(collisions, vertices, centroids, radii, genIds, ownerIndex, otherIndex, ownerFlags, otherFlags);
                }
            }
            else
            {
                if((otherFlags & PhysicsBodyFlags.RectangleShape) != 0)
                {                    
                    PolygonToCircleBodiesAreColliding(collisions, vertices, centroids, radii, genIds, otherIndex, ownerIndex, otherFlags, ownerFlags);
                }
                else
                {
                    CircleBodiesAreColliding(collisions, centroids, radii, ownerIndex, otherIndex, genIds, ownerFlags, otherFlags);
                }         
            }
        }
    }

    public static void PolygonToCircleBodiesAreColliding(Soa_Collision collisions, FsSoa_Vector2 vertices, Soa_Vector2 centroids, Span<float> radii, 
        Span<GenId> genIds, int ownerIndex, int otherIndex, PhysicsBodyFlags ownerFlags, PhysicsBodyFlags otherFlags
    )
    {
        float polyPosX = centroids.X[ownerIndex];
        float circPosX = centroids.X[otherIndex];
        float polyPosY = centroids.Y[ownerIndex];
        float circPosY = centroids.Y[otherIndex];
        float circRadius = radii[otherIndex];
        Span<float> polyVertsX = default;
        Span<float> polyVertsY = default;

        // gather polygon a vertices.
        PhysicsBody.GetPolygonVerticesUnsafe(vertices, ownerIndex, ref polyVertsX, ref polyVertsY);

        bool intersect = SAT.PolygonAndCircleIntersect(polyVertsX, polyVertsY, polyPosX, polyPosY, circPosX, circPosY, circRadius, circPosX, circPosY, 
            out float normalX, out float normalY, out float depth
        );
        // narrow phase intersect check.
        if(intersect)
        {
            // get the gen ids.
            GenId ownerGenId = genIds[ownerIndex]; 
            GenId otherGenId = genIds[otherIndex]; 
            
            SAT.FindContactPoints(polyVertsX, polyVertsY, circPosX, circPosY, out float contactPointX, out float contactPointY);
            Soa_Collision.AppendCollision(collisions, ownerGenId, otherGenId, normalX, normalY, polyPosX, polyPosY, circPosX, circPosY,
                contactPointX, contactPointY, depth, ownerFlags, otherFlags
            );
        }        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CircleBodiesAreColliding(Soa_Collision collisions, Soa_Vector2 centroids, Span<float> radii, int ownerIndex, int otherIndex, 
        Span<GenId> genIds, PhysicsBodyFlags ownerFlags, PhysicsBodyFlags otherFlags
    )
    {
        float ownerPosX = centroids.X[ownerIndex];
        float otherPosX = centroids.X[otherIndex];
        float ownerPosY = centroids.Y[ownerIndex];
        float otherPosY = centroids.Y[otherIndex];
        float ownerPosR = radii[ownerIndex];
        float otherPosR = radii[otherIndex];
        
        bool satIntersects = SAT.CirclesIntersect(ownerPosX, ownerPosY, ownerPosR, otherPosX, otherPosY, otherPosR, out float normalX, 
            out float normalY, out float depth
        );
        
        if(satIntersects)
        {
            // submit the collision with contact points if one of the colliders needs them.
            SAT.FindContactPoints(ownerPosX, ownerPosY, ownerPosR, otherPosX, otherPosY, out float contactPointX, out float contactPointY);
            
            // get the gen ids.
            GenId ownerGenId = genIds[ownerIndex]; 
            GenId otherGenId = genIds[otherIndex]; 

            Soa_Collision.AppendCollision(collisions, ownerGenId, otherGenId, normalX, normalY, ownerPosX, ownerPosY, otherPosX, otherPosY,
                contactPointX, contactPointY, depth, ownerFlags, otherFlags
            );
        }   
    }

    public static void PolygonBodiesAreColliding(Soa_Collision collisions, Soa_Vector2 centroids, FsSoa_Vector2 vertices, Span<GenId> genIds, 
        int ownerIndex, int otherIndex, PhysicsBodyFlags ownerFlags, PhysicsBodyFlags otherFlags
    )
    {
        float ownerPosX = centroids.X[ownerIndex];
        float otherPosX = centroids.X[otherIndex];
        float ownerPosY = centroids.Y[ownerIndex];
        float otherPosY = centroids.Y[otherIndex];
        Span<float> ownerVertsX = default;
        Span<float> ownerVertsY = default;
        Span<float> otherVertsX = default;
        Span<float> otherVertsY = default;

        // gather polygon a vertices.
        PhysicsBody.GetPolygonVerticesUnsafe(vertices, ownerIndex, ref ownerVertsX, ref ownerVertsY);
        PhysicsBody.GetPolygonVerticesUnsafe(vertices, otherIndex, ref otherVertsX, ref otherVertsY);

        // narrow phase SAT intersect check.
        if(SAT.PolygonsIntersect(ownerVertsX, ownerVertsY, otherVertsX, otherVertsY, ownerPosX, ownerPosY, 
            otherPosX, otherPosY, out float normalX, out float normalY, out float depth
        ))
        {
            SAT.FindContactPoints(ownerVertsX, ownerVertsY, otherVertsX, otherVertsY, SAT.PolygonContactPointEpsilon, 
                out float firstContactPointX, out float firstContactPointY, out float secondContactPointX, out float secondContactPointY, 
                out int contactCount
            );
    
            // get the gen ids.
            GenId ownerGenId = genIds[ownerIndex]; 
            GenId otherGenId = genIds[otherIndex]; 

            switch (contactCount)
            {
                case 1:
                    Soa_Collision.AppendCollision(collisions, ownerGenId, otherGenId, normalX, normalY, ownerPosX, ownerPosY, otherPosX, otherPosY,
                        firstContactPointX, firstContactPointY, depth, ownerFlags, otherFlags
                    );
                    break;
                case 2:
                    Soa_Collision.AppendCollision(collisions, ownerGenId, otherGenId, normalX, normalY, ownerPosX, ownerPosY, otherPosX, otherPosY,
                        firstContactPointX, firstContactPointY, secondContactPointX, secondContactPointY, depth, ownerFlags, otherFlags
                    );
                    break;
            }                
        }
    }




    /*******************
    
        Utility.
    
    ********************/




    /// <summary>
    ///     Adds un-transformed/local-space vertices into a physics system state.
    /// </summary>
    /// <remarks>
    ///     Note: the next index for a given shape is inserted as a circular intrusive linked list; 
    ///     meaning that the next vertice index of the final vertice will be the first vertice index. 
    /// </remarks>
    /// <param name="state">the physics system state to insert into.</param>
    /// <param name="verticesX">the x-component values of the vertices to insert.</param>
    /// <param name="verticesY">the y-component values of the vertices to insert.</param>
    /// <param name="firstIndex">the index in the physics system state's vertice array that contains the first vertice index in the state's vertice array.</param>
    /// <param name="vertexCount">the amount of vertices added.</param>
    /// <exception cref="ArgumentException">throws if verticesX is not of the same length as verticesY.</exception>
    public static void AddLocalVertices(TeloPhysicsState state, Span<float> verticesX, Span<float> verticesY, out int firstIndex, out int vertexCount)
    {
        if(verticesX.Length != verticesY.Length)
            throw new ArgumentException($"vertices X length '{verticesX.Length}' must be equalt to vertices Y length '{verticesY.Length}'");

        vertexCount = verticesX.Length;

        if(vertexCount > state.MaxPhysicsBodyVertexCount)
            throw new ArgumentException($"vertices cannot have a length greater than the state's set max physics body vertice count '{state.MaxPhysicsBodyVertexCount}'");

        // add the vertices.
        firstIndex = StackArray.Pop(state.FreeVertexEntries);
        for(int i = 0; i < vertexCount; i++)
        {
            FsSoa_Vector2.Append(state.LocalVertices, firstIndex, verticesX[i], verticesY[i]);
        }
    }

    public static void ResolveColliderCollisions(Soa_Collision collisions, Soa_Transform transforms)
    {
        // hoisting invariance.
        float depth;
        float displacementX;
        float displacementY;
        Span<float> positionX = transforms.Positions.X;
        Span<float> positionY = transforms.Positions.Y;
        Span<float> normalX = collisions.Normals.X;
        Span<float> normalY = collisions.Normals.Y;
        Span<float> depths = collisions.Depths;
        Span<GenId> ownerIndices = collisions.OwnerGenIds;
        Span<GenId> otherIndices = collisions.OtherGenIds;
        Span<PhysicsBodyFlags> ownerFlags = collisions.OwnerFlags;
        Span<PhysicsBodyFlags> otherFlags = collisions.OtherFlags;

        for(int i = 0; i < collisions.Count; i++)
        {
            int ownerIndex = GenId.GetIndex(ownerIndices[i]);
            int otherIndex = GenId.GetIndex(otherIndices[i]);
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
        Span<GenId> ownerIndices = collisions.OwnerGenIds;
        Span<GenId> otherIndices = collisions.OtherGenIds;
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

            int ownerIndex = GenId.GetIndex(ownerIndices[i]);
            int otherIndex = GenId.GetIndex(otherIndices[i]);
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
    public static void ClearForcesAndVelocities(TeloPhysicsState state, int bodyIndex)
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
    /// Draws wireframes for all circle physics bodies.
    /// </summary>
    /// <param name="camera">the camera to draw in relation to.</param>
    /// <param name="centroids">the source containing the centroids for the circles.</param>
    /// <param name="radii">the radii of the circles.</param>
    /// <param name="flags">a span containing the flags of the circles to draw.</param>
    /// <param name="dynamicColour">the colour to draw any 'dynamic' bodies with.</param>
    /// <param name="kinematicColour">the colour to draw any 'kinematic' bodies with.</param>
    /// <param name="triggerColour">the colour to draw any 'trigger' bodies with.</param>
    public static void DrawCirclePhysicsBodies(HowlApp app, Soa_Vector2 centroids, Span<float> radii, Span<PhysicsBodyFlags> flags, 
        Colour dynamicColour, Colour kinematicColour, Colour triggerColour
    )
    {
        Span<float> centroidX = centroids.X;
        Span<float> centroidY = centroids.Y;
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

            Circle shape = new(centroidX[i], centroidY[i], radii[i]);

            Debug.Draw.WireCircle(app, shape, drawColour, DrawSpace.World);
        }    
    }

    /// <summary>
    /// Draws wireframes for all polygon physics bodies.
    /// </summary>
    /// <param name="camera">the camera to draw in relation to.</param>
    /// <param name="vertices">the vertices for all the polygons.</param>
    /// <param name="flags">a span containing the flags of the polygons to draw.</param>
    /// <param name="dynamicColour">the colour to draw 'dynamic' bodies with.</param>
    /// <param name="kinematicColour">the colour to draw 'kinematic' bodies with.</param>
    /// <param name="triggerColour">the colour to draw 'trigger' bodies with.</param>
    public static void DrawPolygonPhysicsBodies(HowlApp app, FsSoa_Vector2 vertices, Span<PhysicsBodyFlags> flags, 
        Colour dynamicColour, Colour kinematicColour, Colour triggerColour
    )
    {
        Span<float> polyVertsX = default;
        Span<float> polyVertsY = default;
        Colour drawColour;

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

            PhysicsBody.GetPolygonVerticesUnsafe(vertices, i, ref polyVertsX, ref polyVertsY);
            Debug.Draw.WirePoly(app, polyVertsX, polyVertsY, drawColour, DrawSpace.World);
        }        
    }

    public static void DrawCollisionInformation(HowlApp app, Soa_Collision collisions, Colour ownerColour, Colour otherColour, Colour contactPointColour, 
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
        
        Math.Vector2 normalStart = default;
        Math.Vector2 normalEnd = default;

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
            Debug.Draw.WireCircle(app, new Circle(ownerCentroidX, ownerCentroidY, 0.1f), ownerColour, DrawSpace.World);
            Debug.Draw.WireCircle(app, new Circle(otherCentroidX, otherCentroidY, 0.1f), otherColour, DrawSpace.World);

            // draw contact point 1.
            Debug.Draw.WireCircle(app, new Circle(contactPointX, contactPointY, 0.1f), contactPointColour, DrawSpace.World);            

            // draw normal from contact point. 
            normalStart = new Math.Vector2(contactPointX, contactPointY);
            normalEnd = normalStart + new Math.Vector2(normalX, normalY);
            Debug.Draw.Line(app, normalColour, normalStart, normalEnd, DrawSpace.World);

            if (twoContactPoints[i])
            {
                // get contact point 2.
                contactPointX = secondContactPointsX[i];
                contactPointY = secondContactPointsY[i];

                // draw contact point 2.
                Debug.Draw.WireCircle(app, new Circle(contactPointX, contactPointY, 0.1f), contactPointColour, DrawSpace.World);            

                // draw normal from contact point. 
                normalStart = new Math.Vector2(contactPointX, contactPointY);
                normalEnd = normalStart + new Math.Vector2(normalX, normalY);
                Debug.Draw.Line(app, normalColour, normalStart, normalEnd, DrawSpace.World);
            }
        }
    }

    public static void DrawLinearVelocities(HowlApp app, Soa_Vector2 linearVelocities, Soa_Vector2 centroids, Span<PhysicsBodyFlags> flags, 
        Colour colour, int count
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

            Debug.Draw.Line(app, colour, new Math.Vector2(startX, startY), new Math.Vector2(endX, endY), DrawSpace.World);
        }
    }

    public static void DrawPositions(HowlApp app, Soa_Vector2 positions, Span<PhysicsBodyFlags> flags, Colour colour, int count)
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

            Debug.Draw.WireCircle(app, new Circle(positionsX[i], positionsY[i], 0.1f), colour, DrawSpace.World);
        }
    }

    public static void DrawCentroids(HowlApp app, Soa_Vector2 centroids, Span<PhysicsBodyFlags> flags, Colour colour, int count)
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

            Debug.Draw.WireCircle(app, new Circle(centroidsX[i], centroidsY[i], 0.1f), colour, DrawSpace.World);
        }
    }

    public static void DrawAabbs(HowlApp app, Soa_Vector2 min, Soa_Vector2 max, Span<PhysicsBodyFlags> flags, Colour colour)
    {
        for(int i = 0; i < flags.Length; i++)
        {
            if((flags[i] & PhysicsBodyFlags.InUse) == 0)
                continue;

            float minX = min.X[i];
            float minY = min.Y[i];
            float maxX = max.X[i];
            float maxY = max.Y[i];

            Debug.Draw.WirePoly(app, [minX, maxX, maxX, minX], [maxY, maxY, minY, minY], colour, DrawSpace.World);
        }
    }
}