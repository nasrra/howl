using System;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Generic;
using Howl.DataStructures;
using Howl.Collections;
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

namespace Howl.Physics;

public static class SoaPhysicsSystem
{
    public static void RegisterComponents(ComponentRegistry registry)
    {
        registry.RegisterComponent<Transform>();
        registry.RegisterComponent<PhysicsBodyId>();
    }

    public static void FixedUpdate(ComponentRegistry registry, SoaPhysicsSystemState state, float deltaTime, int subSteps)
    {
        state.FixedUpdateStepStopwatch.Restart();
        // scale delta time by the substeps.
        deltaTime /= (float)subSteps;

        for(int i = 0; i < subSteps; i++)
        {
            state.FixedUpdateSubStepStopwatch.Restart();

            // Movement Step.
            state.MovementStepStopwatch.Restart();
            state.MovementStepStopwatch.Stop();

            // Sync Colliders to Transforms Step.
            state.SyncPhysicsBodiesToEntitiesStopwatch.Restart();
            SyncPhysicsBodiesToEntityTransforms(registry.Get<Transform>(), registry.Get<PhysicsBodyId>(), 
                state.Transforms, state.Generations
            );
            state.SyncPhysicsBodiesToEntitiesStopwatch.Stop();

            // Reconstruct Bvh.
            state.BvhReconstructionStopwatch.Restart();
            ReconstructBvhTree(state.TransformedVertices, state.TransformedRadii, state.FirstVertexIndices, 
                state.NextVertexIndices, state.Generations, state.Flags, state.Bvh, state.MaxPhysicsBodyVertexCount
            );
            state.BvhReconstructionStopwatch.Stop();

            state.FilterBvhIntoCollisionManifoldStopwatch.Restart();
                FilterBvhIntoCollisionManifold(        
                    state.CollisionManifold.CircleSpatialPairs,
                    state.CollisionManifold.PolygonSpatialPairs,
                    state.CollisionManifold.PolygonToCircleSpatialPairs,
                    AsSpan(state.Bvh.SpatialPairs)
                );
            state.FilterBvhIntoCollisionManifoldStopwatch.Stop();

            // Process Near Colliders.
            state.FindColliderPairsStopwatch.Restart();
            FindCircleCollisions(state.CollisionManifold.CircleCollisionsToResolve, state.CollisionManifold.CircleSpatialPairs, 
                state.TransformedVertices, state.TransformedRadii, state.FirstVertexIndices
            );
            FindPolygonCollisions(state.CollisionManifold.PolygonCollisionsToResolve, state.CollisionManifold.PolygonSpatialPairs, 
                state.TransformedVertices, state.FirstVertexIndices, state.NextVertexIndices, state.MaxPhysicsBodyVertexCount
            );
            FindPolygonToCircleCollisions(state.CollisionManifold.PolygonToCircleCollisionsToResolve, 
                state.CollisionManifold.PolygonToCircleSpatialPairs, state.TransformedVertices, state.FirstVertexIndices, 
                state.NextVertexIndices, state.Radii, state.MaxPhysicsBodyVertexCount
            );
            state.FindColliderPairsStopwatch.Stop();

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
            state.RigidBodyCollisionResolutionStepStopwatch.Stop();

            // Sort Collision Manifold.
            // sort the collision manifold after resolution step.
            // this is to ensure that binary searching for collisions
            // using a GenIndex work outside of this function.
            state.CollisionManifoldSortStopwatch.Restart();
            state.CollisionManifoldSortStopwatch.Stop();

            state.FixedUpdateSubStepStopwatch.Stop();
        }

        state.FixedUpdateStepStopwatch.Stop();
    }

    /// <summary>
    /// Syncs an SoaTransform collection to entities that contain both a transform component and a physics body id component. 
    /// </summary>
    /// <param name="componentRegistry">the component registry housing the entity components.</param>
    /// <param name="soaTransform">the structure-of-array transforms to mutate in relation to the entity data.</param>
    /// <param name="generation">the generations for each entry in the SOA transform's.</param>
    public static void SyncPhysicsBodiesToEntityTransforms(GenIndexList<Transform> transforms, 
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
    /// Transforms all active and allocated physics bodies by their stored transforms.
    /// </summary>
    /// <param name="soaVertice">a structure-of-array vector2 containing the vertices to transform.</param>
    /// <param name="soaTransformedVertice">a structure-of-array vector2 that will be mutated and store the transformed vertices.</param>
    /// <param name="soaTransform">a structure-of-array transform containing the associated transforms for each physics body.</param>
    /// <param name="flags">the flags for each physics body.</param>
    /// <param name="radius">the radius for each physics body.</param>
    /// <param name="transformedRadius">the transformed radius for each physics body.</param>
    /// <param name="firstVertice">the first vertice for each physics body shape.</param>
    /// <param name="nextVertice">an array that stores the next vertice in relation to the soaVertice array.</param>
    /// <param name="startIndex">the starting physics body index to transform.</param>
    /// <param name="length">the amount of bodies after the start index to transform.</param>
    public static void TransformPhysicsBodyVertices(
        Soa_Vector2 soaVertice,
        Soa_Vector2 soaTransformedVertice,
        Soa_Transform soaTransform,
        Span<PhysicsBodyFlags> flags, 
        Span<float> radius,
        Span<float> transformedRadius,
        Span<int> firstVertice,
        Span<int> nextVertice, 
        int startIndex, 
        int length
    )
    {
        for(int i = startIndex; i < length; i++)
        {
            PhysicsBodyFlags flag = flags[i];
            
            // if the physics body had been allocated and is active.
            if((flag & PhysicsBodyFlags.Allocated) != 0 && (flag & PhysicsBodyFlags.Active) != 0)
            {
                if((flag & PhysicsBodyFlags.RectangleShape) != 0)
                {
                    int first = firstVertice[i]; 
                    int verticeIndex = first;
                    while (true)
                    {

                        // transform the base/un-transformed vertice.
                        TransformVector(
                            soaVertice.X[verticeIndex],
                            soaVertice.Y[verticeIndex],
                            soaTransform.Scale.X[i],
                            soaTransform.Scale.Y[i],
                            soaTransform.Cos[i],
                            soaTransform.Sin[i],
                            soaTransform.Position.X[i],
                            soaTransform.Position.Y[i],
                            out float x,
                            out float y
                        );

                        // mutate the transformed vertices array.
                        soaTransformedVertice.X[verticeIndex] = x;
                        soaTransformedVertice.Y[verticeIndex] = y;

                        verticeIndex = nextVertice[verticeIndex];

                        if (verticeIndex == first)
                            break;
                    }
                }
                else // circle shape.
                {
                    int vertexIndex = firstVertice[i];
                    Circle.Transform(
                        soaVertice.X[vertexIndex],
                        soaVertice.Y[vertexIndex],
                        radius[i],
                        soaTransform.Scale.X[i],
                        soaTransform.Scale.Y[i],
                        soaTransform.Cos[i],
                        soaTransform.Sin[i],
                        soaTransform.Position.X[i],
                        soaTransform.Position.Y[i],
                        out float x,
                        out float y,
                        out float r
                    );

                    soaTransformedVertice.X[vertexIndex] = x;
                    soaTransformedVertice.Y[vertexIndex] = y;
                    transformedRadius[i] = r;
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
    /// <param name="transformedVertices">the transformed vertices of all physics bodies to insert into the bounding volume hierarchy.</param>
    /// <param name="transformedRadii">the transformed radii of circle physics bodies to calculate the bounding box necessary for insertion in the bounding volume hierarchy.</param>
    /// <param name="firstVertexIndices">the first vertex indices for each physics body.</param>
    /// <param name="nextVertexIndices">the next vertex indices for each vertex in transform vertices.</param>
    /// <param name="generations">the generation for each physics body.</param>
    /// <param name="flags">the flags for each physics body.</param>
    /// <param name="bvh">the bounding volume hierarchy.</param>
    /// <param name="maxPhysicsBodyVertexCount">the max amount of vertices that a physics body shape can have.</param>
    public static void ReconstructBvhTree(
        Soa_Vector2 transformedVertices, 
        Span<float> transformedRadii,
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
                    int firstVerticeIndex = firstVertexIndices[i];
                    int verticeIndex = firstVerticeIndex;
                    
                    while (true)
                    {
                        // store the vertice data.
                        x[verticeCount] = transformedVertices.X[verticeIndex];
                        y[verticeCount] = transformedVertices.Y[verticeIndex];
                        
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
                        transformedVertices.X[i],
                        transformedVertices.Y[i],
                        transformedRadii[i],
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
                        (byte)flag // this is okay as PhysicsBodyFlags is a byte under the hood.
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
    /// <param name="vertices">a soa vector2 containing all circle body vertices.</param>
    /// <param name="radii">the radii of the circle bodies.</param>
    /// <param name="firstVertexIndices">the index of the first vertex of a body in the vertices collection.</param>
    public static void FindCircleCollisions(
        Soa_Collision collisionsToResolve,
        Soa_SpatialPair spatialPairs,
        Soa_Vector2 vertices,
        Span<float> radii,
        Span<int> firstVertexIndices
    )
    {
        // hoisting invariance.
        Span<int> ownerIndices      = spatialPairs.OwnerGenIndices.Indices;
        Span<int> ownerGenerations  = spatialPairs.OwnerGenIndices.Generations;
        Span<int> otherIndices      = spatialPairs.OtherGenIndices.Indices;
        Span<int> otherGenerations  = spatialPairs.OtherGenIndices.Generations;
        Span<byte> ownerFlags       = spatialPairs.OwnerFlags;
        Span<byte> otherFlags       = spatialPairs.OtherFlags;
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
            int ownerIndex              = ownerIndices[i];
            int ownerGeneration         = ownerGenerations[i];
            int otherIndex              = otherIndices[i];
            int otherGeneration         = otherGenerations[i];
            PhysicsBodyFlags ownerFlag = (PhysicsBodyFlags)ownerFlags[i];
            PhysicsBodyFlags otherFlag = (PhysicsBodyFlags)otherFlags[i];
            float ownerX = x[firstVertexIndices[ownerIndex]];
            float ownerY = y[firstVertexIndices[ownerIndex]];
            float ownerR = radii[ownerIndex];
            float otherX = x[firstVertexIndices[otherIndex]];
            float otherY = y[firstVertexIndices[otherIndex]];
            float otherR = radii[otherIndex];

            if(CircleBodiesAreColliding(ownerX, ownerY, ownerR, otherX, otherY, otherR, ref normalX, ref normalY, 
                ref depth, ref contactPointX, ref contactPointY))
            {
                AppendCollision(collisionsToResolve, ownerIndex, ownerGeneration, otherIndex, otherGeneration,
                    normalX, normalY, ownerX, ownerY, otherX, otherY,
                    contactPointX, contactPointY, depth, ownerFlag, otherFlag
                );
            } 
        }
    }

    public static void FindPolygonCollisions(
        Soa_Collision collisionsToResolve,
        Soa_SpatialPair spatialPairs,
        Soa_Vector2 vertices,
        Span<int> firstVertexIndices,
        Span<int> nextVertexIndices,
        int maxPolygonVerticeCount
    )
    {
        // hoisting of invariance.
        Span<int> ownerIndices      = spatialPairs.OwnerGenIndices.Indices;
        Span<int> ownerGenerations  = spatialPairs.OwnerGenIndices.Generations;
        Span<int> otherIndices      = spatialPairs.OtherGenIndices.Indices;
        Span<int> otherGenerations  = spatialPairs.OtherGenIndices.Generations;
        Span<byte> ownerFlags       = spatialPairs.OwnerFlags;
        Span<byte> otherFlags       = spatialPairs.OtherFlags;
        Span<float> vertsX = vertices.X;
        Span<float> vertsY = vertices.Y;        
        Span<float> ownerVertsX = stackalloc float[maxPolygonVerticeCount];
        Span<float> ownerVertsY = stackalloc float[maxPolygonVerticeCount];
        Span<float> otherVertsX = stackalloc float[maxPolygonVerticeCount];
        Span<float> otherVertsY = stackalloc float[maxPolygonVerticeCount];
        float normalX = 0;
        float normalY = 0;
        float depth = 0;        
        float contactPointX1 = 0;
        float contactPointY1 = 0;
        float contactPointX2 = 0;
        float contactPointY2 = 0;
        
        int vertexCountA = 0;
        int vertexCountB = 0;
        int contactCount = 0;

        for(int i = 0; i < spatialPairs.Count; i++)
        {            
            int ownerIndex              = ownerIndices[i];
            int ownerGeneration         = ownerGenerations[i];
            int otherIndex              = otherIndices[i];
            int otherGeneration         = otherGenerations[i];
            PhysicsBodyFlags ownerFlag  = (PhysicsBodyFlags)ownerFlags[i];
            PhysicsBodyFlags otherFlag  = (PhysicsBodyFlags)otherFlags[i];

            // gather polygon a vertices.
            GetPolygonVertices(vertsX, vertsY, firstVertexIndices, nextVertexIndices, ownerVertsX, ownerVertsY, ownerIndex, ref vertexCountA);
            GetPolygonVertices(vertsX, vertsY, firstVertexIndices, nextVertexIndices, otherVertsX, otherVertsY, otherIndex, ref vertexCountB);

            if (PolygonBodiesAreColliding(ownerVertsX.Slice(0, vertexCountA), ownerVertsY.Slice(0, vertexCountA),
                otherVertsX.Slice(0, vertexCountB), otherVertsY.Slice(0, vertexCountB),
                ref normalX, ref normalY, ref depth, ref contactPointX1, ref contactPointY1, ref contactPointX2,
                ref contactPointY2, ref contactCount
            ))
            {
                GetCentroid(ownerVertsX, ownerVertsY, out float ownerCentroidX, out float ownerCentroidY);
                GetCentroid(otherVertsX, otherVertsY, out float otherCentroidX, out float otherCentroidY);

                switch (contactCount)
                {
                    case 1:
                        AppendCollision(collisionsToResolve, ownerIndex, ownerGeneration, otherIndex, otherGeneration,
                            normalX, normalY, ownerCentroidX, ownerCentroidY, otherCentroidX, otherCentroidY,
                            contactPointX1, contactPointY1, depth, ownerFlag, otherFlag
                        );
                        break;
                    case 2:
                        AppendCollision(collisionsToResolve, ownerIndex, ownerGeneration, otherIndex, otherGeneration,
                            normalX, normalY, ownerCentroidX, ownerCentroidY, otherCentroidX, otherCentroidY,
                            contactPointX1, contactPointY1,contactPointX2, contactPointY2, depth, ownerFlag, otherFlag
                        );
                        break;
                }                
            }
        }

    }

    public static void FindPolygonToCircleCollisions(Soa_Collision collisionsToResolve, Soa_SpatialPair spatialPairs, 
        Soa_Vector2 vertices, Span<int> firstVertexIndices, Span<int> nextVertexIndices, Span<float> radii, 
        int maxPolygonVerticeCount
    )
    {
        // hoisting of invariance.
        Span<int> ownerIndices      = spatialPairs.OwnerGenIndices.Indices;
        Span<int> ownerGenerations  = spatialPairs.OwnerGenIndices.Generations;
        Span<int> otherIndices      = spatialPairs.OtherGenIndices.Indices;
        Span<int> otherGenerations  = spatialPairs.OtherGenIndices.Generations;
        Span<byte> ownerFlags       = spatialPairs.OwnerFlags;
        Span<byte> otherFlags       = spatialPairs.OtherFlags;
        Span<float> polygonX = stackalloc float[maxPolygonVerticeCount];
        Span<float> polygonY = stackalloc float[maxPolygonVerticeCount];
        Span<float> verticesX = vertices.X;
        Span<float> verticesY = vertices.Y;

        // declarations:
        float normalX = 0;
        float normalY = 0;
        float depth = 0;
        float contactPointX = 0;
        float contactPointY = 0;
        int polygonVertexCount = 0;

        for(int i = 0; i < spatialPairs.Count; i++)
        {            
            int ownerIndex      = ownerIndices[i];
            int ownerGeneration = ownerGenerations[i];
            int otherIndex      = otherIndices[i];
            int otherGeneration = otherGenerations[i];
            PhysicsBodyFlags ownerFlag = (PhysicsBodyFlags)ownerFlags[i];
            PhysicsBodyFlags otherFlag = (PhysicsBodyFlags)otherFlags[i];

            // get circle data.
            float circleX = verticesX[firstVertexIndices[otherIndex]];
            float circleY = verticesY[firstVertexIndices[otherIndex]];
            float circleR = radii[otherIndex]; 

            // get polygon data.
            GetPolygonVertices(
                vertices, firstVertexIndices, nextVertexIndices, polygonX, polygonY, ownerIndex, ref polygonVertexCount);
            
            if(PolygonAndCircleBodiesAreColliding(
                polygonX, polygonY, circleX, circleY, circleR, 
                ref normalX, ref normalY, ref depth, ref contactPointX, ref contactPointY
            ))
            {
                GetCentroid(polygonX, polygonY, out float polygonCentroidX, out float polygonCentroidY);

                AppendCollision(collisionsToResolve, ownerIndex, ownerGeneration, otherIndex, otherGeneration,
                    normalX, normalY, polygonCentroidX, polygonCentroidY, circleX, circleY,
                    contactPointX, contactPointY, depth, ownerFlag, otherFlag
                );
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
    /// <param name="nX">a float to store the x-component of the normal vector.</param>
    /// <param name="nY">a float to store the y-component of the normal vector.</param>
    /// <param name="d">a float to the store the depth of the collision.</param>
    /// <param name="cX">a float to store the x-component of the contact point vector.</param>
    /// <param name="cY">a float to store the y-component of the contact point vector.</param>
    /// <returns></returns>
    public static bool CircleBodiesAreColliding(float xA, float yA, float rA, float xB, float yB, float rB, ref float nX, ref float nY, ref float d, ref float cX, ref float cY)
    {
        // get AABB of circle A.
        Circle.GetMinMaxVectors(xA, yA, rA, out float minXA, out float minYA, out float maxXA, out float maxYA);

        // get AABB of circle B.
        Circle.GetMinMaxVectors(xB, yB, rB, out float minXB, out float minYB, out float maxXB, out float maxYB);

        // Broad Phase:
        if (Intersect(minXA, minYA, maxXA, maxYA, minXB, minYB, maxXB, maxYB) == false)
        {
            return false;
        }

        // Narrow Phase:
        // perform an SAT check.
        if(SAT.CirclesIntersect(xA, yA, rA, xB, yB, rB, out nX, out nY, out d))
        {
            // submit the collision with contact points if one of the colliders needs them.
            SAT.FindContactPoints(xA, yA, rA, xB, yB, out cX, out cY);
            return true;
        }   
        return false;
    }

    /// <summary>
    /// Gets whether or not two polygon bodies are colliding with eachother.
    /// </summary>
    /// <param name="aX">the x-components of polygon a's vertices.</param>
    /// <param name="aY">the y-components of polygon a's vertices.</param>
    /// <param name="bX">the x-components of polygon b's vertices.</param>
    /// <param name="bY">the y-components of polygon b's vertices.</param>
    /// <param name="nX">a float to store the x-component of the normal vector.</param>
    /// <param name="nY">a float to store the y-component of the normal vector.</param>
    /// <param name="d">a float to store the depth of the collision.</param>
    /// <param name="cX1">a float to store the x-component of the first contact point vector.</param>
    /// <param name="cY1">a float to store the y-component of the first contact point vector.</param>
    /// <param name="cX2">a float to store the x-component of the second contact point vector.</param>
    /// <param name="cY2">a float to store the y-component of the second contact point vector.</param>
    /// <param name="contactCount">the amount of contact points (0, 1, 2).</param>
    /// <returns>true, if the two bodies are colliding; otherwise false.</returns>
    public static bool PolygonBodiesAreColliding(
        Span<float> aX,
        Span<float> aY,
        Span<float> bX,
        Span<float> bY,
        ref float nX,
        ref float nY,
        ref float d,
        ref float cX1,
        ref float cY1,
        ref float cX2,
        ref float cY2,
        ref int contactCount
    )
    {
        // get the min and max vectors of polygon a and b.
        GetMinMaxVectors(aX, aY, out float aMinX, out float aMinY, out float aMaxX, out float aMaxY);
        GetMinMaxVectors(bX, bY, out float bMinX, out float bMinY, out float bMaxX, out float bMaxY);

        // broad phase AABB intersect check.
        if(Intersect(aMinX, aMinY, aMaxX, aMaxY, bMinX, bMinY, bMaxX, bMaxY) == false)
            return false;

        // get the centers of both polygons.
        GetCentroid(aX, aY, out float aCX, out float aCY);
        GetCentroid(bX, bY, out float bCX, out float bCY);

        // narrow phase SAT intersect check.
        if(SAT.PolygonsIntersect(aX, aY, bX, bY, aCX, aCY, bCX, bCY, out nX, out nY, out d))
        {
            SAT.FindContactPoints(aX, aY, bX, bY, SAT.PolygonContactPointEpsilon, 
                out cX1, 
                out cY1, 
                out cX2, 
                out cY2, 
                out contactCount
            );

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets whether or not a polygon can a circle are colliding.
    /// </summary>
    /// <param name="polygonX">the x-components of a polygon's vertices.</param>
    /// <param name="polygonY">the y-components of a polygon's vertices.</param>
    /// <param name="circleX">the x-component of a circles position vector.</param>
    /// <param name="circleY">the y-component of a circles position vector.</param>
    /// <param name="circleR">the radius of a circle.</param>
    /// <param name="nX">the x-component of the collision normal vector.</param>
    /// <param name="nY">the y-component of the collision normal vector.</param>
    /// <param name="d">the depth of the collision.</param>
    /// <param name="cPX">the x-component of the contact point vector.</param>
    /// <param name="cPY">the y-component of the contact point vector.</param>
    /// <returns>true, if the two bodies are colliding; otherwise false.</returns>
    public static bool PolygonAndCircleBodiesAreColliding(
        Span<float> polygonX,
        Span<float> polygonY,
        float circleX,
        float circleY,
        float circleR,
        ref float nX,
        ref float nY,
        ref float d,
        ref float cPX,
        ref float cPY
    )
    {
        // get the AABB of the circle.
        Circle.GetMinMaxVectors(
            circleX,
            circleY,
            circleR,
            out float circleMinX,
            out float circleMinY,
            out float circleMaxX,
            out float circleMaxY
        );

        // get AABB of the polygon.
        GetMinMaxVectors(
            polygonX, 
            polygonY, 
            out float polygonMinX, 
            out float polygonMinY, 
            out float polygonMaxX, 
            out float polygonMaxY
        );

        // broad phase intersect check.
        if(Intersect(polygonMinX, polygonMinY, polygonMaxX, polygonMaxY, circleMinX, circleMinY, circleMaxX, circleMaxY) == false)
            return false;

        GetCentroid(polygonX, polygonY, out float polygonCentroidX, out float polygonCentroidY);

        // narrow phase intersect check.
        if(PolygonAndCircleIntersect(
            polygonX, 
            polygonY, 
            polygonCentroidX, 
            polygonCentroidY, 
            circleX, 
            circleY, 
            circleR, 
            circleX, 
            circleY,
            out nX,
            out nY,
            out d
        ))
        {
            FindContactPoints(polygonX, polygonY, circleX, circleY, out cPX, out cPY);
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
        state.Vertices.X[index] = verticesX[0];
        state.Vertices.Y[index] = verticesY[0];

        // add the rest of them.
        for(int i = 1; i < vertexCount; i++)
        {
            previousIndex = index;
            index = state.FreeVertexIndex.Pop();
            state.Vertices.X[index] = verticesX[i];
            state.Vertices.Y[index] = verticesY[i];
            state.NextVertexIndices[previousIndex] = index;
        }

        // loop back to the beginning.
        // note: this is very important, do not remove this.
        state.NextVertexIndices[index] = firstIndex;

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
    public static bool HasPhysicsMaterial(SoaPhysicsSystemState state, GenIndex genIndex)
    {
        return (state.Flags[genIndex.Index] & PhysicsBodyFlags.HasPhysicsMaterial) != 0;
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
        soaTransform.Rotation[genIndex.Index]    = transform.Rotation;
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
    public static void AllocateCircleCollider(SoaPhysicsSystemState state, in Circle shape, 
        bool isKinematic, bool isTrigger, ref GenIndex genIndex
    )
    {
        Span<float> verticesX = state.Vertices.X;
        Span<float> verticesY = state.Vertices.Y;

        // handle flags.
        // note: no circle shape is needed to be set as it is implied by the system that when a shape is not
        // set, a physics body is a circle.
        PhysicsBodyFlags flag = PhysicsBodyFlags.None; 

        SetActive(ref flag, true);
        SetAllocated(ref flag, true);
        SetRigidBody(ref flag, false);
        SetHasPhysicsMaterial(ref flag, false);
        SetTrigger(ref flag, isTrigger);
        SetKinematic(ref flag, isKinematic);

        AddVertices(state, [shape.X], [shape.Y], out int verticesFirstIndex, out int verticeCount);

        // apply data.
        int index = state.FreePhysicsBodyIndex.Pop();
        state.Radii[index]              = shape.Radius;
        verticesX[index]                = shape.X;
        verticesY[index]                = shape.Y;
        state.Flags[index]              = flag;
        state.FirstVertexIndices[index]  = verticesFirstIndex;

        state.AlloctedPhysicsBodyCount++;

        genIndex.Index = index;
        genIndex.Generation = state.Generations[index];
    }

    /// <summary>
    /// Allocates a circle rigidbody - without friction - into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the shape data of the circle.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateCircleRigidBody(SoaPhysicsSystemState state, in Circle shape, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.
        // note: no circle shape is needed to be set as it is implied by the system that when a shape is not
        // set, a physics body is a circle.
        PhysicsBodyFlags flags = PhysicsBodyFlags.None; 

        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        AddVertices(state, [shape.X], [shape.Y], out int verticesFirstIndex, out int verticeCount);

        // apply data.
        int index = state.FreePhysicsBodyIndex.Pop();

        state.Radii[index]              = shape.Radius;
        state.Vertices.X[index]         = shape.X;
        state.Vertices.Y[index]         = shape.Y;
        state.Flags[index]              = flags;
        state.FirstVertexIndices[index]  = verticesFirstIndex;

        // return gen index.

        genIndex = new(index, state.Generations[index]);

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
    public static void AllocateCircleRigidBody(SoaPhysicsSystemState state, in Circle shape, in PhysicsMaterial physicsMaterial, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.
        // note: no circle shape is needed to be set as it is implied by the system that when a shape is not
        // set, a physics body is a circle.
        PhysicsBodyFlags flags = PhysicsBodyFlags.None; 
        
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, true);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        AddVertices(state, [shape.X], [shape.Y], out int verticesFirstIndex, out int verticeCount);

        // apply data.
        int index = state.FreePhysicsBodyIndex.Pop();

        state.Radii[index]              = shape.Radius;
        state.Vertices.X[index]         = shape.X;
        state.Vertices.Y[index]         = shape.Y;
        state.Flags[index]              = flags;
        state.StaticFrictions[index]    = physicsMaterial.StaticFriction;
        state.KineticFrictions[index]   = physicsMaterial.KineticFriction;
        state.FirstVertexIndices[index]  = verticesFirstIndex;

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
    public static void AllocateRectangleCollider(SoaPhysicsSystemState state, in Rectangle shape, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.RectangleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, false);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticesFirstIndex, out int verticeCount);

        state.Heights[bodyIndex]            = shape.Height;
        state.Widths[bodyIndex]             = shape.Width;
        state.Flags[bodyIndex]              = flags;
        state.FirstVertexIndices[bodyIndex]  = verticesFirstIndex;

        // return gen index.

        genIndex = new(bodyIndex, state.Generations[bodyIndex]);    

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
    public static void AllocateRectangleRigidBody(SoaPhysicsSystemState state, in Rectangle shape, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.RectangleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);
    
        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticesFirstIndex, out int verticeCount);

        state.Heights[bodyIndex]            = shape.Height;
        state.Widths[bodyIndex]             = shape.Width;
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
    public static void AllocateRectangleRigidBody(SoaPhysicsSystemState state, in Rectangle shape, PhysicsMaterial physicsMaterial, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.

        PhysicsBodyFlags flags = PhysicsBodyFlags.RectangleShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, true);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);
    
        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticesFirstIndex, out int verticeCount);

        state.Heights[bodyIndex]                = shape.Height;
        state.Widths[bodyIndex]                 = shape.Width;
        state.Flags[bodyIndex]                  = flags;
        state.FirstVertexIndices[bodyIndex]      = verticesFirstIndex;
        state.KineticFrictions[bodyIndex]       = physicsMaterial.KineticFriction;
        state.StaticFrictions[bodyIndex]        = physicsMaterial.StaticFriction;

        // return gen index.

        genIndex = new(bodyIndex, state.Generations[bodyIndex]);
    
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




        /*******************
        
            Header.
        
        ********************/




        
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

    public static void DrawCircleColliders(Soa_Transform transforms, Soa_Vector2 vertices, Span<float> radii, 
        Span<int> firstIndices, Span<PhysicsBodyFlags> flags
    )
    {
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
        }    
    }
}