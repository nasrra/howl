using System;
using Howl.Ecs;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Graphics;
using System.Runtime.CompilerServices;
using System.Numerics;
using static Howl.Math.Shapes.ShapeUtils;
using Howl.DataStructures.Bvh;
using Howl.Collections;

namespace Howl.Physics;

public static class PhysicsSystem
{
    public const float RectangleRotationalInertia = 0.0833333333333f;
    public const float CircleRotationalInertia = 0.5f;
    public const float MinBodySize = float.Epsilon;
    public const float MaxBodySize = float.MaxValue;
    public const int MaxCollisionContactPoints = 2;

    public static readonly Vector<float> VectorRectangleRotationalInertia = new(RectangleRotationalInertia);
    public static readonly Vector<float> VectorCircleRotationalInertia = new(CircleRotationalInertia);

    public static void FixedUpdate(HowlAppState app, PhysicsSystemState state, float deltaTime, int subSteps)
    {
        state.FixedUpdateStepStopwatch.Restart();

        // == hoisting invariance. ==.
        
        int[] bvhLeafIndices = state.BvhLeafIndices;
        CollisionManifoldState collisions = state.CollisionManifoldState;
        float[] collisionNormalsX = collisions.Normals.X;
        float[] collisionNormalsY = collisions.Normals.Y;
        float[] collisionDepths = collisions.Depths;
        float[] collisionFirstContactPointsX = collisions.FirstContactPoints.X;
        float[] collisionFirstContactPointsY = collisions.FirstContactPoints.Y;
        float[] collisionSecondContactPointsX = collisions.SecondContactPoints.X;
        float[] collisionSecondContactPointsY = collisions.SecondContactPoints.Y;
        bool[] collisionTwoContactPoints = collisions.TwoContactPoints;
        int collisionsStride = collisions.Stride;
        Soa_Vector2 centroids = state.Centroids;
        FsSoa_Vector2 worldVertices = state.WorldVertices;
        CategorisedOverlapArray<int> colliderCollisionsToResolve = state.SubStepColliderCollisionsToResolve;
        StackArray<int> rigidBodyCollisionsToResolve = state.SubStepRigidbodyCollisionsToResolve;
        float[] worldRadii = state.WorldRadii;
        CategorisedLeafOverlaps overlaps = state.Overlaps;
        BoundingVolumeHierarchy bvh = state.Bvh;
        float[] bvhLeafPaddings = state.BvhLeafPaddings;
        int[] bvhCategories = state.BvhCategories;
        PhysicsBodyFlags[] flags = state.Flags;
        FsSoa_Vector2 localVertices = state.LocalVertices;
        float[] localRadii = state.LocalRadii;
        float[] localWidths = state.LocalWidths;
        float[] localHeights = state.LocalHeights;
        Soa_Transform transforms = state.Transforms;
        float[] positionsX = transforms.Positions.X;
        float[] positionsY = transforms.Positions.Y;  
        float[] scalesX = transforms.Scales.X;
        float[] scalesY = transforms.Scales.Y;
        float[] cosines = transforms.Cosines;
        float[] sines = transforms.Sines;
        float[] masses = state.Masses;
        float[] inverseMasses = state.InverseMasses;
        float[] rotationalInertia = state.RotationalInertia;
        float[] inverseRotationalInertia = state.InverseRotationalInertia;
        float[] previousPositionsX = state.PreviousStepPositions.X;
        float[] previousPositionsY = state.PreviousStepPositions.Y;
        float[] densities = state.PhysicsMaterials.Density;
        float[] staticFrictions = state.PhysicsMaterials.StaticFriction;
        float[] kineticFrictions = state.PhysicsMaterials.KineticFriction;
        float[] restitutions = state.PhysicsMaterials.Restitution;
        float[] minAabbsX = state.MinAABBVertices.X;
        float[] minAabbsY = state.MinAABBVertices.Y;
        float[] maxAabbsX = state.MaxAABBVertices.X;
        float[] maxAabbsY = state.MaxAABBVertices.Y;
        float[] centroidsX = state.Centroids.X;
        float[] centroidsY = state.Centroids.Y;
        float[] linearVelocitiesX = state.LinearVelocities.X;
        float[] linearVelocitiesY = state.LinearVelocities.Y;
        float[] forcesX = state.Forces.X;
        float[] forcesY = state.Forces.Y;
        float[] angularVelocities = state.AngularVelocities;
        SwapBackArray<int> activeBodies = state.Active;
        int maxPhysicsBodyCount = state.MaxPhysicsBodyCount;
        float gravity = state.Gravity;
        float gravityDirectionX = state.GravityDirection.X;
        float gravityDirectionY = state.GravityDirection.Y;

        // scratch buffers for rigid body reslution.
        Span<float> impulseMagnitudes = stackalloc float[MaxCollisionContactPoints]; 
        Span<float> contactPointsX = stackalloc float[MaxCollisionContactPoints];
        Span<float> contactPointsY = stackalloc float[MaxCollisionContactPoints];
        Span<float> impulsesX = stackalloc float[MaxCollisionContactPoints];
        Span<float> impulsesY = stackalloc float[MaxCollisionContactPoints];
        Span<float> distsAX = stackalloc float[MaxCollisionContactPoints];
        Span<float> distsAY = stackalloc float[MaxCollisionContactPoints];
        Span<float> distsBX = stackalloc float[MaxCollisionContactPoints];
        Span<float> distsBY = stackalloc float[MaxCollisionContactPoints];

        // scale delta time by the substeps.
        deltaTime /= (float)subSteps;
        
        {   // Sync Bodies To Entities.

            state.SyncTransformsToEntitiesStopwatch.Restart();
        
            // SyncTransformsToEntityTransforms(physicsBodyTags, transforms, state.Transforms, state.Generations);
                    
            state.SyncTransformsToEntitiesStopwatch.Stop();
        }

        {   // Integrate Body Properties.
            
            state.IntegrateBodyPropertiesStopwatch.Restart();

            IntegrateBodyProperties(scalesX, scalesY, masses, inverseMasses, rotationalInertia, inverseRotationalInertia, 
                densities, localRadii, worldRadii, localWidths, localHeights, flags, maxPhysicsBodyCount
            );

            state.IntegrateBodyPropertiesStopwatch.Stop();
        }

        {   // Prepare Collisions
            
            CollisionManifold.PrepareForNextStep(collisions);

            // prepare sub step collision resolution collection.
            int solidCount = state.SolidPolygonColliderCount + state.SolidCircleColliderCount + state.SolidPolygonRigidBodyCount + state.SolidCircleRigidBodyCount;
            int kinematicCount = state.KinematicPolygonColliderCount + state.KinematicCircleColliderCount + state.KinematicPolygonRigidBodyCount + state.KinematicCircleRigidBodyCount;
            colliderCollisionsToResolve.CategoryLengths[SubStepResolutionBvhCategory.Solid] = solidCount;
            colliderCollisionsToResolve.CategoryLengths[SubStepResolutionBvhCategory.Kinematic] = kinematicCount;
            CategorisedOverlapArray.BuildChunks(colliderCollisionsToResolve);

            CategorisedOverlapArray.ClearCounts(colliderCollisionsToResolve);
            StackArray.ClearCount(rigidBodyCollisionsToResolve);
        }

        {   // Bvh
            
            state.BvhStopwatch.Restart();
            
            CalculateBvhLeafPadding(positionsX, positionsY, previousPositionsX, previousPositionsY, activeBodies, bvhLeafPaddings, deltaTime);

            // Update Overlap Scratch Buffer Category Length.       
            {                
             
                CategorisedLeafOverlaps.ClearCounts(overlaps);
                overlaps.CategoryLengths[BvhCategory.SolidCircleCollider]       = state.SolidCircleColliderCount;
                overlaps.CategoryLengths[BvhCategory.TriggerCircleCollider]     = state.TriggerCircleColliderCount;
                overlaps.CategoryLengths[BvhCategory.KinematicCircleCollider]   = state.KinematicCircleColliderCount;
                
                overlaps.CategoryLengths[BvhCategory.SolidCircleRigidBody]      = state.SolidCircleRigidBodyCount;
                overlaps.CategoryLengths[BvhCategory.TriggerCircleRigidBody]    = state.TriggerCircleRigidBodyCount;
                overlaps.CategoryLengths[BvhCategory.KinematicCircleRigidBody]  = state.KinematicCircleRigidBodyCount;

                overlaps.CategoryLengths[BvhCategory.SolidPolygonCollider]      = state.SolidPolygonColliderCount;
                overlaps.CategoryLengths[BvhCategory.TriggerPolygonCollider]    = state.TriggerPolygonColliderCount;
                overlaps.CategoryLengths[BvhCategory.KinematicPolygonCollider]  = state.KinematicPolygonColliderCount;
                
                overlaps.CategoryLengths[BvhCategory.SolidPolygonRigidBody]     = state.SolidPolygonRigidBodyCount;
                overlaps.CategoryLengths[BvhCategory.TriggerPolygonRigidBody]   = state.TriggerPolygonRigidBodyCount;
                overlaps.CategoryLengths[BvhCategory.KinematicPolygonRigidBody] = state.KinematicPolygonRigidBodyCount;
                
                CategorisedLeafOverlaps.BuildChunks(overlaps);
            }

            // Reconstruct Bvh.
            ConstructBvhTree(minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, centroidsX, centroidsY, flags, bvhCategories, bvhLeafPaddings, 
                bvhLeafIndices, bvh
            );

            BoundingVolumeHierarchy.FindOverlaps(bvh.Branches, bvh.Leaves, overlaps);
            FormatCategorisedOverlaps(overlaps, bvhLeafIndices, bvhCategories);
            
            state.BvhStopwatch.Stop();
        }
        
        // note: ordering matters here; keep this below the bvh section always.
        SetPreviousPositions(positionsX, positionsY, previousPositionsX, previousPositionsY);      

        // == retrieve overlap info.

        // solid polygon rigidbody.        
        OverlapInfo overlaps_SolPolRig_To_SolPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.SolidPolygonRigidBody);
        OverlapInfo overlaps_SolPolRig_To_SolCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.SolidCircleRigidBody);
        OverlapInfo overlaps_SolPolRig_To_KinPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.KinematicPolygonRigidBody);
        OverlapInfo overlaps_SolPolRig_To_KinCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.KinematicCircleRigidBody);
        OverlapInfo overlaps_SolPolRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.TriggerPolygonRigidBody);
        OverlapInfo overlaps_SolPolRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.TriggerCircleRigidBody);
        OverlapInfo overlaps_SolPolRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.SolidPolygonCollider);
        OverlapInfo overlaps_SolPolRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.SolidCircleCollider);
        OverlapInfo overlaps_SolPolRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.KinematicPolygonCollider);
        OverlapInfo overlaps_SolPolRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_SolPolRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_SolPolRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonRigidBody, BvhCategory.TriggerCircleCollider);
        
        // solid circle rigid body.
        OverlapInfo overlaps_SolCirRig_To_SolCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.SolidCircleRigidBody);
        OverlapInfo overlaps_SolCirRig_To_KinPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.KinematicPolygonRigidBody);
        OverlapInfo overlaps_SolCirRig_To_KinCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.KinematicCircleRigidBody);
        OverlapInfo overlaps_SolCirRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.TriggerPolygonRigidBody);
        OverlapInfo overlaps_SolCirRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.TriggerCircleRigidBody);
        OverlapInfo overlaps_SolCirRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.SolidPolygonCollider);
        OverlapInfo overlaps_SolCirRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.SolidCircleCollider);
        OverlapInfo overlaps_SolCirRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.KinematicPolygonCollider);
        OverlapInfo overlaps_SolCirRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_SolCirRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_SolCirRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleRigidBody, BvhCategory.TriggerCircleCollider);

        // kinematic polygon rigid body.
        OverlapInfo overlaps_KinPolRig_To_KinPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.KinematicPolygonRigidBody);
        OverlapInfo overlaps_KinPolRig_To_KinCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.KinematicCircleRigidBody);
        OverlapInfo overlaps_KinPolRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.TriggerPolygonRigidBody);
        OverlapInfo overlaps_KinPolRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.TriggerCircleRigidBody);
        OverlapInfo overlaps_KinPolRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.SolidPolygonCollider);
        OverlapInfo overlaps_KinPolRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.SolidCircleCollider);
        OverlapInfo overlaps_KinPolRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.KinematicPolygonCollider);
        OverlapInfo overlaps_KinPolRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_KinPolRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_KinPolRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonRigidBody, BvhCategory.TriggerCircleCollider);
        
        // kinematic circle rigid body.
        OverlapInfo overlaps_KinCirRig_To_KinCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleRigidBody, BvhCategory.KinematicCircleRigidBody);
        OverlapInfo overlaps_KinCirRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleRigidBody, BvhCategory.TriggerPolygonRigidBody);
        OverlapInfo overlaps_KinCirRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleRigidBody, BvhCategory.TriggerCircleRigidBody);
        OverlapInfo overlaps_KinCirRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleRigidBody, BvhCategory.SolidPolygonCollider);
        OverlapInfo overlaps_KinCirRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleRigidBody, BvhCategory.SolidCircleCollider);
        OverlapInfo overlaps_KinCirRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleRigidBody, BvhCategory.KinematicPolygonCollider);
        OverlapInfo overlaps_KinCirRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleRigidBody, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_KinCirRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleRigidBody, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_KinCirRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleRigidBody, BvhCategory.TriggerCircleCollider);
        
        // trigger polygon rigid body.
        OverlapInfo overlaps_TriPolRig_To_TriPolRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonRigidBody, BvhCategory.TriggerPolygonRigidBody);    
        OverlapInfo overlaps_TriPolRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonRigidBody, BvhCategory.TriggerCircleRigidBody);
        OverlapInfo overlaps_TriPolRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonRigidBody, BvhCategory.SolidPolygonCollider);
        OverlapInfo overlaps_TriPolRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonRigidBody, BvhCategory.SolidCircleCollider);
        OverlapInfo overlaps_TriPolRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonRigidBody, BvhCategory.KinematicPolygonCollider);
        OverlapInfo overlaps_TriPolRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonRigidBody, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_TriPolRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonRigidBody, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_TriPolRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonRigidBody, BvhCategory.TriggerCircleCollider);
        
        // trigger circle rigidbody.
        OverlapInfo overlaps_TriCirRig_To_TriCirRig = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerCircleRigidBody, BvhCategory.TriggerCircleRigidBody);
        OverlapInfo overlaps_TriCirRig_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerCircleRigidBody, BvhCategory.SolidPolygonCollider);
        OverlapInfo overlaps_TriCirRig_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerCircleRigidBody, BvhCategory.SolidCircleCollider);
        OverlapInfo overlaps_TriCirRig_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerCircleRigidBody, BvhCategory.KinematicPolygonCollider);
        OverlapInfo overlaps_TriCirRig_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerCircleRigidBody, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_TriCirRig_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerCircleRigidBody, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_TriCirRig_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerCircleRigidBody, BvhCategory.TriggerCircleCollider);
        
        // solid polygon collider.
        OverlapInfo overlaps_SolPolCol_To_SolPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonCollider, BvhCategory.SolidPolygonCollider);
        OverlapInfo overlaps_SolPolCol_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonCollider, BvhCategory.SolidCircleCollider);
        OverlapInfo overlaps_SolPolCol_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonCollider, BvhCategory.KinematicPolygonCollider);
        OverlapInfo overlaps_SolPolCol_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonCollider, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_SolPolCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonCollider, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_SolPolCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidPolygonCollider, BvhCategory.TriggerCircleCollider);
        
        // solid circle collider.
        OverlapInfo overlaps_SolCirCol_To_SolCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleCollider, BvhCategory.SolidCircleCollider);
        OverlapInfo overlaps_SolCirCol_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleCollider, BvhCategory.KinematicPolygonCollider);
        OverlapInfo overlaps_SolCirCol_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleCollider, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_SolCirCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleCollider, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_SolCirCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.SolidCircleCollider, BvhCategory.TriggerCircleCollider);
        
        // kinematic polygon collider.
        OverlapInfo overlaps_KinPolCol_To_KinPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonCollider, BvhCategory.KinematicPolygonCollider);
        OverlapInfo overlaps_KinPolCol_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonCollider, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_KinPolCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonCollider, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_KinPolCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicPolygonCollider, BvhCategory.TriggerCircleCollider);
        
        // kinematic circle collider.
        OverlapInfo overlaps_KinCirCol_To_KinCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleCollider, BvhCategory.KinematicCircleCollider);
        OverlapInfo overlaps_KinCirCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleCollider, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_KinCirCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.KinematicCircleCollider, BvhCategory.TriggerCircleCollider);
        
        // trigger polygon collider.
        OverlapInfo overlaps_TriPolCol_To_TriPolCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonCollider, BvhCategory.TriggerPolygonCollider);
        OverlapInfo overlaps_TriPolCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerPolygonCollider, BvhCategory.TriggerCircleCollider);
        
        // trigger circle collider.
        OverlapInfo overlaps_TriCirCol_To_TriCirCol = CategorisedLeafOverlaps.GetOverlaps(overlaps, BvhCategory.TriggerCircleCollider, BvhCategory.TriggerCircleCollider);

        for(int i = 0; i < subSteps; i++)
        {
            // clear any grabage collisions that were resolved last sub step.
            CategorisedOverlapArray.ClearCounts(colliderCollisionsToResolve);
            StackArray.ClearCount(rigidBodyCollisionsToResolve);

            state.FixedUpdateSubStepStopwatch.Restart();

            // RigidBody Movement Step.
            state.RigidBodyMovementStepStopwatch.Restart();
            RigidBodyMovementStep(linearVelocitiesX, linearVelocitiesY, forcesX, forcesY, masses, positionsX, positionsY, sines, cosines, 
                angularVelocities, flags, gravityDirectionX, gravityDirectionY, gravity, deltaTime
            );
            state.RigidBodyMovementStepStopwatch.Stop();

            // transform physics bodies
            state.TransformPhysicsBodiesStopwatch.Restart();
            TransformPhysicsBodyVertices(worldVertices, localVertices, flags, scalesX, scalesY, positionsX, positionsY, sines, cosines, 
                minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, centroidsX, centroidsY, localRadii, worldRadii
            );
            state.TransformPhysicsBodiesStopwatch.Stop();


            // Find collisions.
            state.FindCollisionsStopwatch.Restart();
                        
            Collisions.Detection.SolidPolygonRigidBody_To_SolidPolygonRigidBody(    overlaps_SolPolRig_To_SolPolRig, bvhLeafIndices, collisions, centroids, worldVertices, colliderCollisionsToResolve, rigidBodyCollisionsToResolve);
            Collisions.Detection.SolidPolygonRigidBody_To_SolidCircleRigidBody(     overlaps_SolPolRig_To_SolCirRig, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve, rigidBodyCollisionsToResolve);
            Collisions.Detection.SolidPolygonRigidBody_To_KinematicPolygonRigidBody(overlaps_SolPolRig_To_KinPolRig, bvhLeafIndices, collisions, centroids, worldVertices, colliderCollisionsToResolve, rigidBodyCollisionsToResolve);
            Collisions.Detection.SolidPolygonRigidBody_To_KinematicCircleRigidBody( overlaps_SolPolRig_To_KinCirRig, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve, rigidBodyCollisionsToResolve);
            Collisions.Detection.SolidPolygonRigidBody_To_TriggerPolygonRigidBody(  overlaps_SolPolRig_To_TriPolRig, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.SolidPolygonRigidBody_To_TriggerCircleRigidBody(   overlaps_SolPolRig_To_TriCirRig, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.SolidPolygonRigidBody_To_SolidPolygonCollider(     overlaps_SolPolRig_To_SolPolCol, bvhLeafIndices, collisions, centroids, worldVertices, colliderCollisionsToResolve);
            Collisions.Detection.SolidPolygonRigidBody_To_SolidCircleCollider(      overlaps_SolPolRig_To_SolCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidPolygonRigidBody_To_KinematicPolygonCollider( overlaps_SolPolRig_To_KinPolCol, bvhLeafIndices, collisions, centroids, worldVertices, colliderCollisionsToResolve);
            Collisions.Detection.SolidPolygonRigidBody_To_KinematicCircleCollider(  overlaps_SolPolRig_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidPolygonRigidBody_To_TriggerPolygonCollider(   overlaps_SolPolRig_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.SolidPolygonRigidBody_To_TriggerCircleCollider(    overlaps_SolPolRig_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);

            Collisions.Detection.SolidCircleRigidBody_To_SolidCircleRigidBody(      overlaps_SolCirRig_To_SolCirRig, bvhLeafIndices, collisions, centroids, worldRadii, colliderCollisionsToResolve, rigidBodyCollisionsToResolve);
            Collisions.Detection.SolidCircleRigidBody_To_KinematicPolygonRigidBody( overlaps_SolCirRig_To_KinPolRig, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve, rigidBodyCollisionsToResolve);
            Collisions.Detection.SolidCircleRigidBody_To_KinematicCircleRigidBody(  overlaps_SolCirRig_To_KinCirRig, bvhLeafIndices, collisions, centroids, worldRadii, colliderCollisionsToResolve, rigidBodyCollisionsToResolve);
            Collisions.Detection.SolidCircleRigidBody_To_TriggerPolygonRigidBody(   overlaps_SolCirRig_To_TriPolRig, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.SolidCircleRigidBody_To_TriggerCircleRigidBody(    overlaps_SolCirRig_To_TriCirRig, bvhLeafIndices, collisions, centroids, worldRadii);
            Collisions.Detection.SolidCircleRigidBody_To_SolidPolygonCollider(      overlaps_SolCirRig_To_SolPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidCircleRigidBody_To_SolidCircleCollider(       overlaps_SolCirRig_To_SolCirCol, bvhLeafIndices, collisions, centroids, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidCircleRigidBody_To_KinematicPolygonCollider(  overlaps_SolCirRig_To_KinPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidCircleRigidBody_To_KinematicCircleCollider(   overlaps_SolCirRig_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidCircleRigidBody_To_TriggerPolygonCollider(    overlaps_SolCirRig_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.SolidCircleRigidBody_To_TriggerCircleCollider(     overlaps_SolCirRig_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldRadii);

            Collisions.Detection.KinematicPolygonRigidBody_To_KinematicPolygonRigidBody(overlaps_KinPolRig_To_KinPolRig, bvhLeafIndices, collisions, centroids, worldVertices);            
            Collisions.Detection.KinematicPolygonRigidBody_To_KinematicCircleRigidBody( overlaps_KinPolRig_To_KinCirRig, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.KinematicPolygonRigidBody_To_TriggerPolygonRigidBody(  overlaps_KinPolRig_To_TriPolRig, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.KinematicPolygonRigidBody_To_TriggerCircleRigidBody(   overlaps_KinPolRig_To_TriCirRig, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.KinematicPolygonRigidBody_To_SolidPolygonCollider(     overlaps_KinPolRig_To_SolPolCol, bvhLeafIndices, collisions, centroids, worldVertices, colliderCollisionsToResolve);
            Collisions.Detection.KinematicPolygonRigidBody_To_SolidCircleCollider(      overlaps_KinPolRig_To_SolCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.KinematicPolygonRigidBody_To_KinematicPolygonCollider( overlaps_KinPolRig_To_KinPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.KinematicPolygonRigidBody_To_KinematicCircleCollider(  overlaps_KinPolRig_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.KinematicPolygonRigidBody_To_TriggerPolygonCollider(   overlaps_KinPolRig_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.KinematicPolygonRigidBody_To_TriggerCircleCollider(    overlaps_KinPolRig_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);

            Collisions.Detection.KinematicCircleRigidBody_To_KinematicCircleRigidBody(  overlaps_KinCirRig_To_KinCirRig, bvhLeafIndices, collisions, centroids, worldRadii);
            Collisions.Detection.KinematicCircleRigidBody_To_TriggerPolygonRigidBody(   overlaps_KinCirRig_To_TriPolRig, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.KinematicCircleRigidBody_To_TriggerCircleRigidBody(    overlaps_KinCirRig_To_TriCirRig, bvhLeafIndices, collisions, centroids, worldRadii);
            Collisions.Detection.KinematicCircleRigidBody_To_SolidPolygonCollider(      overlaps_KinCirRig_To_SolPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.KinematicCircleRigidBody_To_SolidCircleCollider(       overlaps_KinCirRig_To_SolCirCol, bvhLeafIndices, collisions, centroids, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.KinematicCircleRigidBody_To_KinematicPolygonCollider(  overlaps_KinCirRig_To_KinPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.KinematicCircleRigidBody_To_KinematicCircleCollider(   overlaps_KinCirRig_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldRadii);
            Collisions.Detection.KinematicCircleRigidBody_To_TriggerPolygonCollider(    overlaps_KinCirRig_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.KinematicCircleRigidBody_To_TriggerCircleCollider(     overlaps_KinCirRig_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldRadii);
        
            Collisions.Detection.TriggerPolygonRigidBody_To_TriggerPolygonRigidBody(  overlaps_TriPolRig_To_TriPolRig, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.TriggerPolygonRigidBody_To_TriggerCircleRigidBody(   overlaps_TriPolRig_To_TriCirRig, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.TriggerPolygonRigidBody_To_SolidPolygonCollider(     overlaps_TriPolRig_To_SolPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.TriggerPolygonRigidBody_To_SolidCircleCollider(      overlaps_TriPolRig_To_SolCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.TriggerPolygonRigidBody_To_KinematicPolygonCollider( overlaps_TriPolRig_To_KinPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.TriggerPolygonRigidBody_To_KinematicCircleCollider(  overlaps_TriPolRig_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.TriggerPolygonRigidBody_To_TriggerPolygonCollider(   overlaps_TriPolRig_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.TriggerPolygonRigidBody_To_TriggerCircleCollider(    overlaps_TriPolRig_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);

            Collisions.Detection.TriggerCircleRigidBody_To_TriggerCircleRigidBody(    overlaps_TriCirRig_To_TriCirRig, bvhLeafIndices, collisions, centroids, worldRadii);
            Collisions.Detection.TriggerCircleRigidBody_To_SolidPolygonCollider(      overlaps_TriCirRig_To_SolPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.TriggerCircleRigidBody_To_SolidCircleCollider(       overlaps_TriCirRig_To_SolCirCol, bvhLeafIndices, collisions, centroids, worldRadii);
            Collisions.Detection.TriggerCircleRigidBody_To_KinematicPolygonCollider(  overlaps_TriCirRig_To_KinPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.TriggerCircleRigidBody_To_KinematicCircleCollider(   overlaps_TriCirRig_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldRadii);
            Collisions.Detection.TriggerCircleRigidBody_To_TriggerPolygonCollider(    overlaps_TriCirRig_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.TriggerCircleRigidBody_To_TriggerCircleCollider(     overlaps_TriCirRig_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldRadii);

            Collisions.Detection.SolidPolygonCollider_To_SolidPolygonCollider(     overlaps_SolPolCol_To_SolPolCol, bvhLeafIndices, collisions, centroids, worldVertices, colliderCollisionsToResolve);
            Collisions.Detection.SolidPolygonCollider_To_SolidCircleCollider(      overlaps_SolPolCol_To_SolCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidPolygonCollider_To_KinematicPolygonCollider( overlaps_SolPolCol_To_KinPolCol, bvhLeafIndices, collisions, centroids, worldVertices, colliderCollisionsToResolve);
            Collisions.Detection.SolidPolygonCollider_To_KinematicCircleCollider(  overlaps_SolPolCol_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidPolygonCollider_To_TriggerPolygonCollider(   overlaps_SolPolCol_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.SolidPolygonCollider_To_TriggerCircleCollider(    overlaps_SolPolCol_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);

            Collisions.Detection.SolidCircleCollider_To_SolidCircleCollider(       overlaps_SolCirCol_To_SolCirCol, bvhLeafIndices, collisions, centroids, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidCircleCollider_To_KinematicPolygonCollider(  overlaps_SolCirCol_To_KinPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidCircleCollider_To_KinematicCircleCollider(   overlaps_SolCirCol_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldRadii, colliderCollisionsToResolve);
            Collisions.Detection.SolidCircleCollider_To_TriggerPolygonCollider(    overlaps_SolCirCol_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.SolidCircleCollider_To_TriggerCircleCollider(     overlaps_SolCirCol_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldRadii);
        
            Collisions.Detection.KinematicPolygonCollider_To_KinematicPolygonCollider( overlaps_KinPolCol_To_KinPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.KinematicPolygonCollider_To_KinematicCircleCollider(  overlaps_KinPolCol_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.KinematicPolygonCollider_To_TriggerPolygonCollider(   overlaps_KinPolCol_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.KinematicPolygonCollider_To_TriggerCircleCollider(    overlaps_KinPolCol_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
        
            Collisions.Detection.KinematicCircleCollider_To_KinematicCircleCollider(  overlaps_KinCirCol_To_KinCirCol, bvhLeafIndices, collisions, centroids, worldRadii);
            Collisions.Detection.KinematicCircleCollider_To_TriggerPolygonCollider(   overlaps_KinCirCol_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);
            Collisions.Detection.KinematicCircleCollider_To_TriggerCircleCollider(    overlaps_KinCirCol_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldRadii);

            Collisions.Detection.TriggerPolygonCollider_To_TriggerPolygonCollider(  overlaps_TriPolCol_To_TriPolCol, bvhLeafIndices, collisions, centroids, worldVertices);
            Collisions.Detection.TriggerPolygonCollider_To_TriggerCircleCollider(   overlaps_TriPolCol_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldVertices, worldRadii);

            Collisions.Detection.TriggerCircleCollider_To_TriggerCircleCollider(overlaps_TriCirCol_To_TriCirCol, bvhLeafIndices, collisions, centroids, worldRadii);

            state.FindCollisionsStopwatch.Stop();

            // Resolve Collider Collisions.
            // NOTE: ordering matters here, make sure to this is above rigidbody collision resolution.
            state.ColliderCollisionResolutionStopwatch.Restart();
            ResolveColliderCollisions(colliderCollisionsToResolve, collisionDepths, collisionNormalsX, collisionNormalsY, 
                positionsX, positionsY, collisionsStride
            );
            state.ColliderCollisionResolutionStopwatch.Stop();

            // Resolve RigidBody Collisions.
            // NOTE: ordering matters here, make sure this is below collision resolution.
            state.RigidBodyCollisionResolutionStepStopwatch.Restart();
            ResolveRigidBodyCollisions(rigidBodyCollisionsToResolve, collisionNormalsX, collisionNormalsY, centroidsX, centroidsY, 
                collisionFirstContactPointsX, collisionFirstContactPointsY, collisionSecondContactPointsX, collisionSecondContactPointsY, 
                linearVelocitiesX, linearVelocitiesY, restitutions, kineticFrictions, staticFrictions, angularVelocities, masses, inverseMasses, 
                inverseRotationalInertia, collisionTwoContactPoints, contactPointsX, contactPointsY, distsAX, distsAY, distsBX, distsBY, impulseMagnitudes, 
                impulsesX, impulsesY, flags, collisionsStride
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

        CollisionManifold.CompleteStep(state.CollisionManifoldState);

        // Transform bodies by collision resolution.
        // NOTE: this is needed at the end as the final
        // sub-step iteration does not transform the bodies
        // at the end of it's loop; meaning the final collision
        // resolution wouldn't be applied.
        TransformPhysicsBodyVertices(worldVertices, localVertices, flags, scalesX, scalesY, positionsX, positionsY, sines, cosines, 
            minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, centroidsX, centroidsY, localRadii, worldRadii
        );

        state.FixedUpdateStepStopwatch.Stop();
    }

    public static void Draw(HowlAppState app, PhysicsSystemState state, float deltaTime)
    {
        if(state.DrawBvhBranches)
        {
            BoundingVolumeHierarchy.DrawBranches(app, state.Bvh, Colour.Yellow);
        }

        if (state.DrawLeaves)
        {
            BoundingVolumeHierarchy.DrawLeaves(app, state.Bvh, Colour.Yellow);            
        }

        if (state.DrawColliderWireframes)
        {
            DrawCirclePhysicsBodies(app, state.CollisionManifoldState, state.Centroids, state.WorldRadii, state.Flags, 
                state.DynamicPhysicsBodyColour, state.KinematicPhysicsBodyColour, state.TriggerPhysicsBodyColour, state.TriggeredPhysicsBodyColour
            );

            DrawPolygonPhysicsBodies(app, state.CollisionManifoldState, state.WorldVertices, state.Flags, state.DynamicPhysicsBodyColour, 
                state.KinematicPhysicsBodyColour, state.TriggerPhysicsBodyColour, state.TriggeredPhysicsBodyColour
            );
        }

        if (state.DrawCollisionInformation)
        {
            DrawCollisionInformation(app, state.CollisionManifoldState, state.CollisionOwnerColour, state.CollisionOtherColour, 
                state.ContactPointColour, state.NormalColour
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
    ///     Syncs an SoaTransform collection to entities that contain both a transform component and a physics body id component. 
    /// </summary>
    /// <param name="physicsBodyTags">the physics body tags of every entity.</param>
    /// <param name="transforms">the transforms of all entities.</param>
    /// <param name="soaTransform">the structure-of-array transforms to mutate in relation to the entity data.</param>
    /// <param name="generation">the generations for each entry in the SOA transform's.</param>
    public static void SyncTransformsToEntityTransforms(ComponentArray<PhysicsBodyComponent> physicsBodyTags, ComponentArray<Transform> transforms, Soa_Transform soaTransform, Span<int> generation)
    {        
        for(int i = 1; i < physicsBodyTags.Active.Count; i++)
        {
            GenId genId = physicsBodyTags.Active[i];
            ref PhysicsBodyComponent tag = ref ComponentArray.GetDataUnsafe(physicsBodyTags, genId);            

            // skip if the physics body id isn't valid.
            if(generation[GenId.GetIndex(tag.GenId)] != GenId.GetGeneration(tag.GenId))
                continue;
            
            // sync the transform data to the physics simulation 
            // if it has an associated physics body id.
            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, genId);
            Soa_Transform.Insert(soaTransform, GenId.GetIndex(genId), transform);
        }
    }

    public static void DeallocateAllDynamicBodies(PhysicsSystemState state)
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
    ///     Syncs a entities that contain both a transform and physics body id component to an soa transform collection.
    /// </summary>
    /// <param name="physicsBodyTags">the physics body tags of every entity.</param>
    /// <param name="transforms">the transforms of all entities.</param>
    /// <param name="soaTransform">the soa transforms to copy into the entity transform components.</param>
    /// <param name="generation">the generation of each soa transform entry.</param>
    public static void SyncEntityTransformsToPhysicsBodies(ComponentArray<PhysicsBodyComponent> physicsBodyTags, ComponentArray<Transform> transforms, Soa_Transform soaTransform, Span<int> generation)
    {
        for(int i = 1; i < physicsBodyTags.Active.Count; i++)
        {
            GenId genId = physicsBodyTags.Active[i];
            ref PhysicsBodyComponent tag = ref ComponentArray.GetDataUnsafe(physicsBodyTags, genId);

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
    ///     Performs a movement step for all physics bodies with a rigidbody.
    /// </summary>
    /// <remarks>
    ///     Remarks: All provided spans must be indexed by a integer <c>physicsBodyIndex</c>:
    /// </remarks>
    public static void RigidBodyMovementStep(float[] linearVelocitiesX, float[] linearVelocitiesY, float[] forcesX, float[] forcesY,
        float[] masses, float[] positionsX, float[] positionsY, float[] sines, float[] cosines, float[] angularVelocities, PhysicsBodyFlags[] flags,
        float gravityDirectionX, float gravityDirectionY, float gravity, float deltaTime
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

        int length = positionsX.Length;

        int i = 0;

        {   // SIMD

            for(; i <= length - simdSize; i+= simdSize)
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
                Vector<float> vLinVelX = Vector.LoadUnsafe(ref linearVelocitiesX[i]);
                Vector<float> vLinVelY = Vector.LoadUnsafe(ref linearVelocitiesY[i]);
                Vector<float> vForceX = Vector.LoadUnsafe(ref forcesX[i]);
                Vector<float> vForceY = Vector.LoadUnsafe(ref forcesY[i]);
                Vector<float> vMass = Vector.LoadUnsafe(ref masses[i]);
                Vector<float> vPosX = Vector.LoadUnsafe(ref positionsX[i]);
                Vector<float> vPosY = Vector.LoadUnsafe(ref positionsY[i]);
                Vector<float> vSin = Vector.LoadUnsafe(ref sines[i]);
                Vector<float> vCos = Vector.LoadUnsafe(ref cosines[i]);
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
                vLinVelX.StoreUnsafe(ref linearVelocitiesX[i]);
                vLinVelY.StoreUnsafe(ref linearVelocitiesY[i]);
                vPosX.StoreUnsafe(ref positionsX[i]);
                vPosY.StoreUnsafe(ref positionsY[i]);
                vCos.StoreUnsafe(ref cosines[i]);
                vSin.StoreUnsafe(ref sines[i]);
            }            
        }

        {   // SISD
            
            float gravityLinearForceX = gravityDirectionX * gravity;
            float gravityLinearForceY = gravityDirectionY * gravity;

            for(int j = i; j < length; j++)
            {
                PhysicsBodyFlags flag = (PhysicsBodyFlags)flags[j];
                
                if((flag & PhysicsBodyFlags.Allocated) == 0 ||
                    (flag & PhysicsBodyFlags.Active) == 0 ||
                    (flag & PhysicsBodyFlags.Kinematic) != 0 ||
                    (flag & PhysicsBodyFlags.RigidBody) == 0)
                {
                    continue;
                }

                ref float linearVelocityX = ref linearVelocitiesX[j];
                ref float linearVelocityY = ref linearVelocitiesY[j];
                ref float mass = ref masses[j];

                // apply gravity.
                linearVelocityX += gravityLinearForceX * deltaTime;
                linearVelocityY += gravityLinearForceY * deltaTime;

                // force = mass * acceleration.
                // acceleration = force / mass.
                if(mass > 0)
                {
                    if (forcesX[j] > 0)
                    {
                        linearVelocityX += forcesX[j] / mass * deltaTime;
                    }
                    if (forcesY[j] > 0)
                    {
                        linearVelocityY += forcesY[j] / mass * deltaTime;
                    }
                }
                
                // apply linear velocity.
                positionsX[j] += linearVelocityX * deltaTime;
                positionsY[j] += linearVelocityY * deltaTime;
        
                Math.Math.RotorMultiply(sines[j], cosines[j], angularVelocities[j] * deltaTime ,ref sines[j], ref cosines[j]);
            }
        }

    }

    /// <summary>
    ///     Transforms <c>InUse<c/> physics bodies local-space vertices by their world-space transforms.
    /// </summary>
    /// <remarks>
    ///     All arrays must be of the same length and elements should be vertivally accessible via <c>physicsBodyIndex</c>. 
    /// </remarks>
    public static void TransformPhysicsBodyVertices(FsSoa_Vector2 worldVertices, FsSoa_Vector2 localVertices, PhysicsBodyFlags[] flags, 
        float[] scalesX, float[] scalesY, float[] positionsX, float[] positionsY, float[] sines, float[] cosines, float[] minAabbsX,
        float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY, float[] centroidsX, float[] centroidsY, float[] localRadii, float[] worldRadii
    )
    {
        FsSoa_Vector2.ClearAppendCounts(worldVertices);
        float[] localVertsX = localVertices.X;
        float[] localVertsY = localVertices.Y;
        Span<float> polygonX = default;
        Span<float> polygonY = default;
        int physicsBodiesProcessed = 0;
        int length = flags.Length;

        for(int physicsBodyIndex = 0; physicsBodyIndex < length; physicsBodyIndex++)
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
                        Math.Math.TransformVector(localVertsX[currentIndex], localVertsY[currentIndex], scaleX, scaleY,
                            cosines[physicsBodyIndex], sines[physicsBodyIndex], positionsX[physicsBodyIndex], positionsY[physicsBodyIndex], 
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
                    Math.Math.GetMinMaxVectors(polygonX, polygonY, out minAabbsX[physicsBodyIndex], out minAabbsY[physicsBodyIndex], 
                        out maxAabbsX[physicsBodyIndex], out maxAabbsY[physicsBodyIndex]
                    );
                }
                else // circle shape.
                {
                    int vertexIndex = FixedStrideArray.GetElementIndex(physicsBodyIndex, worldVertices.Stride, 0);
                    Math.Math.TransformVector(localVertsX[vertexIndex], localVertsY[vertexIndex],scaleX, scaleY, cosines[physicsBodyIndex], sines[physicsBodyIndex], positionsX[physicsBodyIndex], positionsY[physicsBodyIndex], out float x, out float y);

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
                        out minAabbsX[physicsBodyIndex], out minAabbsY[physicsBodyIndex], out maxAabbsX[physicsBodyIndex], out maxAabbsY[physicsBodyIndex]
                    );
                }
            }

            if(physicsBodiesProcessed >= length)
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
        IntegrateBodyProperties_Sisd(scalesX, scalesY, masses, inverseMasses, 
            rotationalInertia, inverseRotationalInertia, densities, localRadii, worldRadii, 
            localWidths, localHeights, flags, maxPhysicsBodyCount, 0
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

                    float mass = 0;

                    if((flag & PhysicsBodyFlags.Kinematic) != 0)
                    {
                        inverseMasses[i] = 0;
                    }
                    else
                    {
                        mass = PhysicsBody.CalculateRectangleMass(width, height, densities[i]); 
                        masses[i] = mass;
                        inverseMasses[i] = mass == 0? 0 : 1f/mass;
                    }

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
                    float mass = 0;

                    if((flag & PhysicsBodyFlags.Kinematic) != 0)
                    {
                        inverseMasses[i] = 0;
                    }
                    else
                    {
                        mass = PhysicsBody.CalculateCircleMass(radius, densities[i]); 
                        masses[i] = mass;
                        inverseMasses[i] = mass == 0? 0 : 1f/mass;
                    }

                    float rI = PhysicsBody.CalculateCircleRotationalInertia(radius, mass);
                    rotationalInertia[i] = rI;
                    inverseRotationalInertia[i] = rI == 0? 0f : 1f/rI;
                }
            }

        }
    }

    /// <summary>
    ///     constructs the bvh tree in relation to physics body data.
    /// </summary>
    /// <remarks>
    ///     All arrays must be of equal length and elements should be accessed via a <c>physicsBodyIndex</c> integer.
    /// </remarks>
    /// <param name="minAabbsX"></param>
    /// <param name="minAabbsY"></param>
    /// <param name="maxAabbsX"></param>
    /// <param name="maxAabbsY"></param>
    /// <param name="centroidsX"></param>
    /// <param name="centroidsY"></param>
    /// <param name="flags"></param>
    /// <param name="bvhCategories"></param>
    /// <param name="bvhLeafPaddings"></param>
    /// <param name="bvhLeafIndices"></param>
    /// <param name="bvh"></param>
    public static void ConstructBvhTree(float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY, float[] centroidsX, 
        float[] centroidsY, PhysicsBodyFlags[] flags, int[] bvhCategories, float[] bvhLeafPaddings, int[] bvhLeafIndices, 
        BoundingVolumeHierarchy bvh
    )
    {
        // clear the previous bvh data.
        BoundingVolumeHierarchy.Clear(bvh);

        for(int i = 0; i < flags.Length; i++)
        {
            ref PhysicsBodyFlags flag = ref flags[i];


            if((flag & PhysicsBodyFlags.Allocated) != 0 && (flag & PhysicsBodyFlags.Active) != 0)
            {
                ref float padding = ref bvhLeafPaddings[i];
                float minX = minAabbsX[i] - padding;
                float minY = minAabbsY[i] - padding;
                float maxX = maxAabbsX[i] + padding;
                float maxY = maxAabbsY[i] + padding;

                // insert into the bvh.
                bvhLeafIndices[Soa_Leaf.Append(bvh.Leaves, minX, minY, maxX, maxY, centroidsX[i], centroidsY[i], bvhCategories[i])] = i;
            }
        }

        // construct the bvh with the new data.
        BoundingVolumeHierarchy.ConstructTree(bvh);
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
    public static void AddLocalVertices(PhysicsSystemState state, Span<float> verticesX, Span<float> verticesY, out int firstIndex, out int vertexCount)
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




    /******************
    
        Collider Collision Resolution.
    
    *******************/




    public static void ResolveColliderCollisions(CategorisedOverlapArray<int> subStepCollisionsToResolve, float[] collisionDepths,
        float[] collisionNormalsX, float[] collisionNormalsY, float[] positionsX, float[] positionsY, int collisionsStride
    )
    {
        // hoisting invariance.
        float depth;
        float displacementX;
        float displacementY;
        int ownerIndex; // always the solid collider.
        int otherIndex; // always the kinematic or other solid collider.

        Span<int> collisionsToResolve;

        // == resolve solid to solid collisions ==.
        collisionsToResolve = CategorisedOverlapArray.GetOverlaps(subStepCollisionsToResolve,
            SubStepResolutionBvhCategory.Solid,
            SubStepResolutionBvhCategory.Solid
        );

        for(int i = 0; i < collisionsToResolve.Length; i++)
        {            
            int collisionIndex = collisionsToResolve[i];
            ownerIndex = collisionIndex / collisionsStride; // int div truncates the remainder, always giving the owner index.
            otherIndex = collisionIndex % collisionsStride;
            depth = collisionDepths[collisionIndex];
            displacementX = collisionNormalsX[collisionIndex] * depth * 0.5f;
            displacementY = collisionNormalsY[collisionIndex] * depth * 0.5f;
            positionsX[otherIndex] -= displacementX;
            positionsY[otherIndex] -= displacementY;
            positionsX[ownerIndex] += displacementX;
            positionsY[ownerIndex] += displacementY; 
        }

        // == resolve solid to kinematic collisions ==.

        collisionsToResolve = CategorisedOverlapArray.GetOverlaps(subStepCollisionsToResolve,
            SubStepResolutionBvhCategory.Solid,
            SubStepResolutionBvhCategory.Kinematic
        );

        for(int i = 0; i < collisionsToResolve.Length; i++)
        {            
            int collisionIndex = collisionsToResolve[i];
            ownerIndex = collisionIndex / collisionsStride; // int div truncates the remainder, always giving the owner index.
            depth = collisionDepths[collisionIndex];
            displacementX = collisionNormalsX[collisionIndex] * depth;
            displacementY = collisionNormalsY[collisionIndex] * depth;
            positionsX[ownerIndex] += displacementX;
            positionsY[ownerIndex] += displacementY; 
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para>
    ///     <para>Elements accesible by <c>collisionIndex</c>:</para>
    ///     <list type="bullet">
    ///         <item><paramref name="normalsX"/></item>
    ///         <item><paramref name="normalsY"/></item>
    ///         <item><paramref name="firstContactPointsX"/></item>
    ///         <item><paramref name="firstContactPointsY"/></item>
    ///         <item><paramref name="secondContactPointsX"/></item>
    ///         <item><paramref name="secondContactPointsY"/></item>
    ///         <item><paramref name="twoContactPoints"/></item>
    ///     </list>
    ///     <para>Elements accesible by <c>physicsBodyIndex</c>:</para>
    ///     <list type="bullet">
    ///         <item><paramref name="centroidsX"/></item>
    ///         <item><paramref name="centroidsY"/></item>
    ///         <item><paramref name="linearVelocitiesX"/></item>
    ///         <item><paramref name="linearVelocitiesY"/></item>
    ///         <item><paramref name="restitutions"/></item>
    ///         <item><paramref name="kineticFrictions"/></item>
    ///         <item><paramref name="staticFrictions"/></item>
    ///         <item><paramref name="angularVelocities"/></item>
    ///         <item><paramref name="masses"/></item>
    ///         <item><paramref name="inverseMasses"/></item>
    ///         <item><paramref name="inverseRotationalInertia"/></item>
    ///         <item><paramref name="flags"/></item>
    ///     </list>
    ///     <para><c>NOTE:</c> All spans are scratch buffers and should have a length of <see cref="MaxCollisionContactPoints"/></para>
    /// </remarks>
    /// <param name="contactPointsX">scratch buffer</param>
    /// <param name="contactPointsY">scratch buffer</param>
    /// <param name="distsAX">scratch buffer</param>
    /// <param name="distsAY">scratch buffer</param>
    /// <param name="distsBX">scratch buffer</param>
    /// <param name="distsBY">scratch buffer</param>
    /// <param name="impulseMagnitudes">scratch buffer</param>
    /// <param name="impulsesX">scratch buffer</param>
    /// <param name="impulsesY">scratch buffer</param>
    /// <param name="collisionsStride">the stride of elements in a collision entry.</param>
    public static void ResolveRigidBodyCollisions(StackArray<int> collisionsToResolve, float[] normalsX, float[] normalsY,
        float[] centroidsX, float[] centroidsY,float[] firstContactPointsX, float[] firstContactPointsY, 
        float[] secondContactPointsX, float[] secondContactPointsY, float[] linearVelocitiesX, float[] linearVelocitiesY, 
        float[] restitutions, float[] kineticFrictions, float[] staticFrictions, float[] angularVelocities,
        float[] masses, float[] inverseMasses, float[] inverseRotationalInertia, bool[] twoContactPoints,
        Span<float> contactPointsX, Span<float> contactPointsY, Span<float> distsAX, Span<float> distsAY, 
        Span<float> distsBX, Span<float> distsBY, Span<float> impulseMagnitudes, Span<float> impulsesX, Span<float> impulsesY, 
        PhysicsBodyFlags[] flags, int collisionsStride
    )
    {
        int contactPointsCount;
        float revNormalX = 0;
        float revNormalY = 0;

        ref PhysicsBodyFlags ownerFlag = ref Unsafe.NullRef<PhysicsBodyFlags>();
        ref PhysicsBodyFlags otherFlag = ref Unsafe.NullRef<PhysicsBodyFlags>();
        ref float normalX = ref Unsafe.NullRef<float>();
        ref float normalY = ref Unsafe.NullRef<float>();
        ref float ownerCentroidX = ref Unsafe.NullRef<float>();
        ref float ownerCentroidY = ref Unsafe.NullRef<float>();
        ref float ownerRestitution = ref Unsafe.NullRef<float>();
        ref float ownerAngularVelocity = ref Unsafe.NullRef<float>();
        ref float ownerLinearVelocityX = ref Unsafe.NullRef<float>();
        ref float ownerLinearVelocityY = ref Unsafe.NullRef<float>();
        ref float ownerInverseMass = ref Unsafe.NullRef<float>();
        ref float ownerInverseRotationalInertia = ref Unsafe.NullRef<float>();
        ref float ownerStaticFriction = ref Unsafe.NullRef<float>();
        ref float ownerKineticFriction = ref Unsafe.NullRef<float>();
        ref float ownerMass = ref Unsafe.NullRef<float>();;
        ref float otherCentroidX = ref Unsafe.NullRef<float>();
        ref float otherCentroidY = ref Unsafe.NullRef<float>();
        ref float otherRestitution = ref Unsafe.NullRef<float>();
        ref float otherAngularVelocity = ref Unsafe.NullRef<float>();
        ref float otherLinearVelocityX = ref Unsafe.NullRef<float>();
        ref float otherLinearVelocityY = ref Unsafe.NullRef<float>();
        ref float otherInverseMass = ref Unsafe.NullRef<float>();
        ref float otherInverseRotationalInertia = ref Unsafe.NullRef<float>();
        ref float otherStaticFriction = ref Unsafe.NullRef<float>();
        ref float otherKineticFriction = ref Unsafe.NullRef<float>();
        ref float otherMass = ref Unsafe.NullRef<float>();



        for(int i = 0; i < collisionsToResolve.Count; i++)
        {
            int collisionIndex = collisionsToResolve[i];
            int ownerIndex = collisionIndex / collisionsStride; // int div truncates the remainder, always giving the owner index.
            int otherIndex = collisionIndex % collisionsStride;

            ownerFlag = ref flags[ownerIndex];
            otherFlag = ref flags[otherIndex];
            normalX = ref normalsX[collisionIndex];
            normalY = ref normalsY[collisionIndex];
            ownerCentroidX = ref centroidsX[ownerIndex];
            ownerCentroidY = ref centroidsY[ownerIndex];
            ownerRestitution = ref restitutions[ownerIndex];
            ownerAngularVelocity = ref angularVelocities[ownerIndex];
            ownerLinearVelocityX = ref linearVelocitiesX[ownerIndex];
            ownerLinearVelocityY = ref linearVelocitiesY[ownerIndex];
            ownerInverseMass = ref inverseMasses[ownerIndex];
            ownerInverseRotationalInertia = ref inverseRotationalInertia[ownerIndex];
            ownerStaticFriction = ref staticFrictions[ownerIndex];
            ownerKineticFriction = ref kineticFrictions[ownerIndex];
            ownerMass = ref masses[ownerIndex];
            otherCentroidX = ref centroidsX[otherIndex];
            otherCentroidY = ref centroidsY[otherIndex];
            otherRestitution = ref restitutions[otherIndex];
            otherAngularVelocity = ref angularVelocities[otherIndex];
            otherLinearVelocityX = ref linearVelocitiesX[otherIndex];
            otherLinearVelocityY = ref linearVelocitiesY[otherIndex];
            otherInverseMass = ref inverseMasses[otherIndex];
            otherInverseRotationalInertia = ref inverseRotationalInertia[otherIndex];
            otherStaticFriction = ref staticFrictions[otherIndex];
            otherKineticFriction = ref kineticFrictions[otherIndex];
            otherMass = ref masses[otherIndex];

            revNormalX = normalX * -1;
            revNormalY = normalY * -1;

            if (twoContactPoints[collisionIndex])
            {
                contactPointsCount = 2;
                contactPointsX[0] = firstContactPointsX[collisionIndex];
                contactPointsX[1] = secondContactPointsX[collisionIndex];
                contactPointsY[0] = firstContactPointsY[collisionIndex];
                contactPointsY[1] = secondContactPointsY[collisionIndex];
            }
            else
            {
                contactPointsCount = 1;  
                contactPointsX[0] = firstContactPointsX[collisionIndex];
                contactPointsY[0] = firstContactPointsY[collisionIndex];
            }

            if((ownerFlag & PhysicsBodyFlags.RotationalPhysics) != 0 || (otherFlag & PhysicsBodyFlags.RotationalPhysics) != 0)
            {
                goto ResolveCollisionRotational;
            }
            else
            {
                goto ResolveCollisionBasic;
            }       

            LoopEnd:
            continue;

            ResolveCollisionBasic:
            {
                for(int j = 0; j < contactPointsCount; j++)
                {                    
                    float relativeVelocityX = otherLinearVelocityX - ownerLinearVelocityX;
                    float relativeVelocityY = otherLinearVelocityY - ownerLinearVelocityY;

                    // the magnitude of the relative velocity relative to the normal
                    float magnitude = Math.Math.Dot(relativeVelocityX, relativeVelocityY, revNormalX, revNormalY);

                    if(magnitude > 0)
                    {
                        // note: these have to be set to zero.
                        // this function resuses these stack allocated spans
                        // so without this, the loop could operate on garbage data from the previous step.
                        impulseMagnitudes[j] = 0;
                        continue;
                    }

                    float restitution = MathF.Min(ownerRestitution, otherRestitution);

                    // magnitude of the impulse
                    float impulseMagnitude = -(1f + restitution) * magnitude;
                    impulseMagnitude /= ownerInverseMass + otherInverseMass;
                    
                    // divide by the contact point count to ensure that impulse is evenly spread 
                    // across all contact points.
                    impulseMagnitude /= (float)contactPointsCount;

                    impulseMagnitudes[j] = impulseMagnitude;
                }


                float impulseForceX;
                float impulseForceY;

                for(int j = 0; j < contactPointsCount; j++)
                {                    
                    float mag = impulseMagnitudes[j];
                    if((ownerFlag & PhysicsBodyFlags.Kinematic) == 0 && (ownerFlag & PhysicsBodyFlags.Trigger) == 0)
                    {
                        impulseForceX = -(mag / ownerMass * revNormalX);
                        impulseForceY = -(mag / ownerMass * revNormalY);
                        ownerLinearVelocityX += impulseForceX;
                        ownerLinearVelocityY += impulseForceY;
                    }

                    if((otherFlag & PhysicsBodyFlags.Kinematic) == 0 && (otherFlag & PhysicsBodyFlags.Trigger) == 0)
                    {
                        impulseForceX = mag / otherMass * revNormalX;
                        impulseForceY = mag / otherMass * revNormalY;
                        otherLinearVelocityX += impulseForceX;
                        otherLinearVelocityY += impulseForceY;
                    }
                }

                goto ResolveFriction;    
            }

            ResolveCollisionRotational:
            {
                float restitution = MathF.Min(ownerRestitution, otherRestitution);
                
                for(int j = 0; j < contactPointsCount; j++)
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
                    float magnitude = Math.Math.Dot(relativeVelocityX, relativeVelocityY, revNormalX, revNormalY);

                    if(magnitude > 0)
                    {
                        // note: these have to be set to zero.
                        // this function resuses these stack allocated spans
                        // so without this, the loop could operate on garbage data from the previous step.
                        impulseMagnitudes[j] = 0;
                        impulsesX[j] = 0;
                        impulsesY[j] = 0;
                        continue;
                    }

                    // calculate the denominator.
                    float perpADotNormal = Math.Math.Dot(perpendicularAX, perpendicularAY, revNormalX, revNormalY);
                    float perpBDotNormal = Math.Math.Dot(perpendicularBX, perpendicularBY, revNormalX, revNormalY);
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
                    impulsesX[j] = impulseMagnitude * revNormalX;
                    impulsesY[j] = impulseMagnitude * revNormalY;
                }

                // keep these outside the for loop so they dont allocate each time.
                float impulseX;
                float impulseY;
                float distAX;
                float distAY;
                float distBX;
                float distBY;

                for(int j = 0; j < contactPointsCount; j++)
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
                            ownerAngularVelocity += -Math.Math.Cross(distAX, distAY, impulseX, impulseY) * ownerInverseRotationalInertia;
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
                            otherAngularVelocity += Math.Math.Cross(distBX, distBY, impulseX, impulseY) * otherInverseRotationalInertia;
                        }
                    }
                }

                goto ResolveFriction;
            }

            ResolveFriction:
            {                
                // get an approximation of the friction values.
                // this is faster than the actual physics way.
                float staticFriction = 0;
                float kineticFriction = 0;

                staticFriction = (ownerStaticFriction + otherStaticFriction) * 0.5f;
                kineticFriction = (ownerKineticFriction + otherKineticFriction) * 0.5f;
                
                for(int j = 0; j < contactPointsCount; j++)
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
                    float relativeDotNormal = Math.Math.Dot(relativeVelocityX, relativeVelocityY, revNormalX, revNormalY);
                    float tangentX = relativeVelocityX - relativeDotNormal * revNormalX;
                    float tangentY = relativeVelocityY - relativeDotNormal * revNormalY;

                    if(Math.Math.NearlyEqual((tangentX * tangentX) + (tangentY * tangentY), 0, 1e-12f))
                    {
                        // note: these have to be set to zero.
                        // this function resuses these stack allocated spans
                        // so without this, the loop could operate on garbage data from the previous step.
                        impulsesX[j] = 0;
                        impulsesY[j] = 0;    
                        continue;
                    }

                    Math.Math.Normalise(tangentX, tangentY, out tangentX, out tangentY);

                    // calculate the denominator.
                    float perpADotTangent = Math.Math.Dot(perpendicularAX, perpendicularAY, tangentX, tangentY);
                    float perpBDotTangent = Math.Math.Dot(perpendicularBX, perpendicularBY, tangentX, tangentY);
                    float denominator = ownerInverseMass + otherInverseMass + 
                        (perpADotTangent * perpADotTangent) * ownerInverseRotationalInertia +
                        (perpBDotTangent * perpBDotTangent) * otherInverseRotationalInertia;

                    // Calculate the DESIRED friction magnitude to stop all sliding.
                    float frictionImpulseMag = -Math.Math.Dot(relativeVelocityX, relativeVelocityY, tangentX, tangentY) / denominator;

                    // Coulomb's Law:
                    // Limit that desire by the static friction. 
                    float maxFriction = impulseMagnitudes[j] * staticFriction;

                    // the the desired friction amount is greater than static friction
                    // that means that the object should be sliding with kinetic friction.
                    if (Math.Math.Abs(frictionImpulseMag) > maxFriction)
                    {
                        // Note: We multiply by the SIGN of frictionImpulseMag to keep the direction correct.
                        frictionImpulseMag = (impulseMagnitudes[j] * kineticFriction) * MathF.Sign(frictionImpulseMag);
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

                for(int j = 0; j < contactPointsCount; j++)
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
                            ownerAngularVelocity += -Math.Math.Cross(distAX, distAY, impulseX, impulseY) * ownerInverseRotationalInertia;
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
                            otherAngularVelocity += Math.Math.Cross(distBX, distBY, impulseX, impulseY) * otherInverseRotationalInertia;
                        }       
                    }   
                } 

                goto LoopEnd;
            }
        }
    }

    // / <summary>
    // / Clears all forces and velocities being applied to a rigidbody.
    // / </summary>
    /// <param name="state">the phsysics system state.</param>
    /// <param name="bodyIndex">the index of the physics body to stop.</param>
    public static void ClearForcesAndVelocities(PhysicsSystemState state, int bodyIndex)
    {
        state.LinearVelocities.X[bodyIndex] = 0;
        state.LinearVelocities.Y[bodyIndex] = 0;
        state.AngularVelocities[bodyIndex] = 0;
        state.Forces.X[bodyIndex] = 0;
        state.Forces.Y[bodyIndex] = 0;
    }


    /// <summary>
    ///     Formats overlap data so that the <c>owner</c> of an overlap is always the <c>solid</c> collider.
    /// </summary>
    /// <param name="overlaps">the overlap instance to format.</param>
    /// <param name="bvhLeafIndices">the mapping of bvh leaf indices onto a physics body.</param>
    /// <param name="bvhCategories">the categories of all physics bodies when being put into the bvh.</param>
    /// <exception cref="Exception"></exception>
    public static void FormatCategorisedOverlaps(CategorisedLeafOverlaps overlaps, Span<int> bvhLeafIndices, Span<int> bvhCategories)
    {
        // hoisting invariance.
        int temp;
        int otherCategory;
        int ownerCategory;

        for(int i = 0; i < BvhCategory.Count; i++)
        {
            for(int j = i; j < BvhCategory.Count; j++)
            {    
                OverlapInfo info = CategorisedLeafOverlaps.GetOverlaps(overlaps, i, j);
                for(int w = 0; w < info.Length; w++)
                {
                    // get the data about the owner and other.
                    ref int ownerLeaf = ref info.OwnerLeafIndices[w];
                    ref int otherLeaf = ref info.OtherLeafIndices[w];
                    ref int ownerIndex = ref bvhLeafIndices[ownerLeaf];
                    ref int otherIndex = ref bvhLeafIndices[otherLeaf];
                    ownerCategory = bvhCategories[ownerIndex];
                    otherCategory = bvhCategories[otherIndex];

                    // order the leaves in ascending order.
                    if(ownerCategory > otherCategory)
                    {                
                        temp = ownerLeaf;
                        ownerLeaf = otherLeaf;
                        otherLeaf = temp;
                    }
                }
            }
        }
    }




    /******************
    
        Step Preparation
    
    *******************/



    /// <summary>
    ///     Deep copies the current position to the previous position arrays.
    /// </summary>
    /// <remarks>
    ///     All passed arrays should be the same length.
    /// </remarks>
    /// <param name="currentPosX"></param>
    /// <param name="currentPosY"></param>
    /// <param name="previousPosX"></param>
    /// <param name="previousPosY"></param>
    public static void SetPreviousPositions(float[] currentPosX, float[] currentPosY, float[] previousPosX, float[] previousPosY)
    {
        int simdSize = Vector<float>.Count;
        int i = 0;
        
        float length = currentPosX.Length;

        // bulk.
        for(; i <= length - simdSize; i += simdSize)
        {
            Vector<float> cX = Vector.LoadUnsafe(ref currentPosX[i]);
            Vector<float> cY = Vector.LoadUnsafe(ref currentPosY[i]);
            Vector.StoreUnsafe(cX, ref previousPosX[i]);
            Vector.StoreUnsafe(cY, ref previousPosY[i]);
        }

        // tail end.
        for(int j = i; j < length; j++)
        {
            previousPosX[j] = currentPosX[j];
            previousPosY[j] = currentPosY[j];
        }
    }

    public static void CalculateBvhLeafPadding(float[] currentPositionX, float[] currentPositionY, 
        float[] previousPositionX, float[] previousPositionY, SwapBackArray<int> active, 
        float[] bvhLeafPadding, float deltaTime
    )
    {
        for(int i = 0; i < active.Count; i++)
        {            
            int index = active[i];
            float deltaMovementX = currentPositionX[index] - previousPositionX[index];
            float deltaMovementY = currentPositionY[index] - previousPositionY[index];
            float deltaMovement = Math.Math.Max(Math.Math.Abs(deltaMovementX),Math.Math.Abs(deltaMovementY));   
            float timeFactor = 1 + deltaTime;
            bvhLeafPadding[index] = deltaMovement * timeFactor;
        }
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
    public static void DrawCirclePhysicsBodies(HowlAppState app, CollisionManifoldState manifold, Soa_Vector2 centroids, 
        Span<float> radii, Span<PhysicsBodyFlags> flags, Colour dynamicColour, Colour kinematicColour, Colour triggerPassiveColour,
        Colour triggerActiveColour
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
                drawColour = CollisionManifold.HasContacts(manifold, i)
                ? triggerActiveColour
                : triggerPassiveColour;            
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
    ///     Draws wireframes for all polygon physics bodies.
    /// </summary>
    /// <param name="camera">the camera to draw in relation to.</param>
    /// <param name="vertices">the vertices for all the polygons.</param>
    /// <param name="flags">a span containing the flags of the polygons to draw.</param>
    /// <param name="dynamicColour">the colour to draw 'dynamic' bodies with.</param>
    /// <param name="kinematicColour">the colour to draw 'kinematic' bodies with.</param>
    /// <param name="triggerPassiveColour">the colour to draw 'trigger' bodies with.</param>


    /// <summary>
    ///     Draws wireframes for all polygon physics bodies.
    /// </summary>
    /// <param name="app">the howl app state instance.</param>
    /// <param name="manifold">the manifold containing the collision information.</param>
    /// <param name="vertices">the vertices of the physics bodies.</param>
    /// <param name="flags">the physics body flags.</param>
    /// <param name="dynamicColour">the colour to draw <c>dynamic</c> bodies with.</param>
    /// <param name="kinematicColour">the colour to draw <c>kinematic</c> bodies with.</param>
    /// <param name="triggerPassiveColour">the colour to draw <c>trigger</c> bodies that are not in contact with other bodies.</param>
    /// <param name="triggerActiveColour">the colour to draw <c>trigger</c> bodies that are in contact with other bodies.</param>
    public static void DrawPolygonPhysicsBodies(HowlAppState app, CollisionManifoldState manifold, FsSoa_Vector2 vertices, Span<PhysicsBodyFlags> flags,
        Colour dynamicColour, Colour kinematicColour, Colour triggerPassiveColour, Colour triggerActiveColour
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
                drawColour = CollisionManifold.HasContacts(manifold, i)
                ? triggerActiveColour
                : triggerPassiveColour;
            }
            else // dynamic body.
            {
                drawColour = dynamicColour;
            }

            PhysicsBody.GetPolygonVerticesUnsafe(vertices, i, ref polyVertsX, ref polyVertsY);
            Debug.Draw.WirePoly(app, polyVertsX, polyVertsY, drawColour, DrawSpace.World);
        }        
    }

    public static void DrawCollisionInformation(HowlAppState app, CollisionManifoldState collisions, Colour ownerColour, Colour otherColour, 
        Colour contactPointColour, Colour normalColour
    )
    {
        // hoisitng invariance.
        Span<float> firstContactPointsX = collisions.FirstContactPoints.X;
        Span<float> firstContactPointsY = collisions.FirstContactPoints.Y;
        Span<float> secondContactPointsX = collisions.SecondContactPoints.X;
        Span<float> secondContactPointsY = collisions.SecondContactPoints.Y;
        Span<float> normalsX = collisions.Normals.X;
        Span<float> normalsY = collisions.Normals.Y;
        Span<float> otherCentroidsX = collisions.ColliderCentroids.X;
        Span<float> otherCentroidsY = collisions.ColliderCentroids.Y;
        Span<bool> twoContactPoints = collisions.TwoContactPoints;


        float contactPointX;
        float contactPointY;
        float normalX;
        float normalY;
        float otherCentroidX;
        float otherCentroidY;
        
        Math.Vector2 normalStart;
        Math.Vector2 normalEnd;

        int[] active = collisions.ActiveIndices;
        int[] activeCounts = collisions.ActiveIndicesCount;

        for(int i = 0; i < activeCounts.Length; i++)
        {
            int count = activeCounts[i];
            if(count<= 0)
            {
                continue;
            }
            int entryElementIndex = FixedStrideArray.GetElementIndex(i, collisions.Stride, 0);
            for(int j = 0; j < count; j++)
            {
                int elementIndex = entryElementIndex+j;
                int collisionIndex = active[elementIndex];

                int ownerIndex = collisionIndex / collisions.Stride; // int div truncates the remainder, always giving the owner index.
                int otherIndex = collisionIndex % collisions.Stride;

                // avoid duplicate collisions.
                if(ownerIndex > otherIndex)
                {
                    continue;
                }

                // get normal data.
                normalX = normalsX[collisionIndex];
                normalY = normalsY[collisionIndex];
                
                // get contact point 1 data.
                contactPointX = firstContactPointsX[collisionIndex];
                contactPointY = firstContactPointsY[collisionIndex];
                
                // get centroid data.
                otherCentroidX = otherCentroidsX[collisionIndex];
                otherCentroidY = otherCentroidsY[collisionIndex];

                // draw centroids.
                Debug.Draw.WireCircle(app, new Circle(otherCentroidX, otherCentroidY, 0.1f), otherColour, DrawSpace.World);

                // draw contact point 1.
                Debug.Draw.WireCircle(app, new Circle(contactPointX, contactPointY, 0.1f), contactPointColour, DrawSpace.World);            

                // draw normal from contact point. 
                normalStart = new Math.Vector2(contactPointX, contactPointY);
                normalEnd = normalStart + new Math.Vector2(normalX, normalY);
                Debug.Draw.Line(app, normalColour, normalStart, normalEnd, DrawSpace.World);

                if (twoContactPoints[collisionIndex])
                {
                    // get contact point 2.
                    contactPointX = secondContactPointsX[collisionIndex];
                    contactPointY = secondContactPointsY[collisionIndex];

                    // draw contact point 2.
                    Debug.Draw.WireCircle(app, new Circle(contactPointX, contactPointY, 0.1f), contactPointColour, DrawSpace.World);            

                    // draw normal from contact point. 
                    normalStart = new Math.Vector2(contactPointX, contactPointY);
                    normalEnd = normalStart + new Math.Vector2(normalX, normalY);
                    Debug.Draw.Line(app, normalColour, normalStart, normalEnd, DrawSpace.World);
                }
            }
        }
    }

    public static void DrawLinearVelocities(HowlAppState app, Soa_Vector2 linearVelocities, Soa_Vector2 centroids, Span<PhysicsBodyFlags> flags, 
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

    public static void DrawPositions(HowlAppState app, Soa_Vector2 positions, Span<PhysicsBodyFlags> flags, Colour colour, int count)
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

    public static void DrawCentroids(HowlAppState app, Soa_Vector2 centroids, Span<PhysicsBodyFlags> flags, Colour colour, int count)
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

    public static void DrawAabbs(HowlAppState app, Soa_Vector2 min, Soa_Vector2 max, Span<PhysicsBodyFlags> flags, Colour colour)
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