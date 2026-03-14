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
using System.Runtime.CompilerServices;

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

        RigidBodySystemState rigState = state.RigidbodySystemState;

        // scale delta time by the substeps.
        deltaTime /= (float)subSteps;

        for(int i = 0; i < subSteps; i++)
        {
            state.FixedUpdateSubStepStopwatch.Restart();

            // Movement Step.
            rigState.MovementStepStopwatch.Restart();
            rigState.MovementStepStopwatch.Stop();

            // Sync Colliders to Transforms Step.
            state.SyncCollidersToTransformsStopwatch.Restart();
            SyncPhysicsBodiesToEntityTransforms(registry, state.Transforms, state.Generations);
            state.SyncCollidersToTransformsStopwatch.Stop();

            // Reconstruct Bvh.
            state.BvhReconstructionStopwatch.Restart();
            ReconstructBvhTree(
                state.TransformedVertices, 
                state.TransformedRadii, 
                state.FirstVertexIndice, 
                state.NextVertexIndice, 
                state.Generations, 
                state.Flags, 
                state.Bvh,
                state.MaxPhysicsBodyVertexCount
            );
            state.BvhReconstructionStopwatch.Stop();

            // Process Near Colliders.
            state.ProcessNearColliderPairsStopwatch.Restart();
            state.ProcessNearColliderPairsStopwatch.Stop();

            // Resolve Collider Collisions.
            // NOTE: ordering matters here, make sure to resolve 
            // collisions before sorting the collision manifold.
            // Also make sure that this is above rigidbody collision resolution.
            // this function also moves the transforms of the colliders.
            state.CollisionResolutionStopwatch.Restart();
            state.CollisionResolutionStopwatch.Stop();

            // Resolve RigidBody Collisions.
            // NOTE: ordering matters here, make sure to resolve 
            // collisions before sorting the collision manifold.
            // Also make sure that this is below collision resolution.
            // this function also moves the transforms of the colliders.
            rigState.CollisionResolutionStepStopwatch.Restart();
            rigState.CollisionResolutionStepStopwatch.Stop();

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
    public static void SyncPhysicsBodiesToEntityTransforms(ComponentRegistry componentRegistry, Soa_Transform soaTransform, Span<int> generation)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<PhysicsBodyId> bodyIds = componentRegistry.Get<PhysicsBodyId>();
        
        Span<DenseEntry<Transform>> denseEntries = GetDenseAsSpan(transforms);

        // loop through all transforms.
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<Transform> entry = ref denseEntries[i];
            ref Transform transform = ref entry.Value;
            GetGenIndex(transforms, entry.sparseIndex, out GenIndex genIndex);
            
            // sync the transform data to the physics simulation 
            // if it has an associated physics body id.
            
            if(GetDenseRef(bodyIds, genIndex, out Ref<PhysicsBodyId> bodyIdRef).Ok())
            {
                SetTransform(soaTransform, generation, bodyIdRef.Value.GenIndex, transform);
            }
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
                if((flag & PhysicsBodyFlags.PolygonShape) != 0)
                {
                    int first = firstVertice[i]; 
                    int verticeIndex = first;
                    while (true)
                    {

                        // transform the base/un-transformed vertice.
                        TransformVector(
                            soaVertice.X[verticeIndex],
                            soaVertice.Y[verticeIndex],
                            soaTransform.Scale.X[verticeIndex],
                            soaTransform.Scale.Y[verticeIndex],
                            soaTransform.Cos[verticeIndex],
                            soaTransform.Sin[verticeIndex],
                            soaTransform.Position.X[verticeIndex],
                            soaTransform.Position.Y[verticeIndex],
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
                    Circle.Transform(
                        soaVertice.X[i],
                        soaVertice.Y[i],
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

                    soaTransformedVertice.X[i] = x;
                    soaTransformedVertice.Y[i] = y;
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

                if((flag & PhysicsBodyFlags.PolygonShape) != 0)
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

    public static void FindCollisions(
        CollisionManifoldNew manifold,
        Soa_Vector2 vertices,
        Span<SpatialPair> spatialPairs,
        Span<int> nextVertexIndices,
        Span<float> radii,
        int maxPolygonVerticeCount
    )
    {
        // delcarations.
        float normalX;
        float normalY;
        float depth;
        float contactPointX1;
        float contactPointY1;
        float contactPointX2;
        float contactPointY2; 
        bool colliding = false;
        
        for(int i = 0; i < spatialPairs.Length; i++)
        {
            // set at one as the default.
            int contactPointsAmount = 1;

            // hoisting invariance.
            ref SpatialPair pair = ref spatialPairs[i];
            PhysicsBodyFlags ownerFlag = (PhysicsBodyFlags)pair.Owner.Flag;
            PhysicsBodyFlags otherFlag = (PhysicsBodyFlags)pair.Other.Flag;
            int ownerIndex = pair.Owner.GenIndex.Index;
            int ownerGeneration = pair.Owner.GenIndex.Generation;
            int otherIndex = pair.Other.GenIndex.Index; 
            int otherGeneration = pair.Other.GenIndex.Generation;

            // the resolution buffer to append the newly found collision to.
            Soa_Collision resolutionSoa;

            if((ownerFlag & PhysicsBodyFlags.PolygonShape) != 0)
            {
                if((otherFlag & PhysicsBodyFlags.PolygonShape) != 0)
                {
                    resolutionSoa = manifold.PolygonCollisionsToResolve;

                    colliding = PolygonBodiesAreColliding(
                        vertices,
                        nextVertexIndices,
                        ownerIndex,
                        otherIndex,
                        maxPolygonVerticeCount,
                        out normalX,
                        out normalY,
                        out depth,
                        out contactPointX1,
                        out contactPointY1,
                        out contactPointX2,
                        out contactPointY2,
                        out contactPointsAmount
                    );
                }
                else // other is circle.
                {
                    resolutionSoa = manifold.PolygonToCircleCollisionsToResolve;

                    colliding = PolygonAndCircleBodiesAreColliding(
                        vertices,
                        radii,
                        nextVertexIndices,
                        ownerIndex, // polygon is owner.
                        otherIndex, // circle is other.
                        maxPolygonVerticeCount,
                        out normalX, 
                        out normalY,
                        out depth,
                        out contactPointX1,
                        out contactPointY1
                    );
                }
            }
            else // owner is circle.
            {
                if((otherFlag & PhysicsBodyFlags.PolygonShape) != 0)
                {
                    resolutionSoa = manifold.PolygonToCircleCollisionsToResolve;

                    colliding = PolygonAndCircleBodiesAreColliding(
                        vertices,
                        radii,
                        nextVertexIndices,
                        otherIndex, // polygon is other.
                        ownerIndex, // circle is owner.
                        maxPolygonVerticeCount,
                        out normalX, 
                        out normalY,
                        out depth,
                        out contactPointX1,
                        out contactPointY1
                    );
                }
                else // other is circle.
                {
                    resolutionSoa = manifold.CircleCollisionsToResolve;

                    colliding = CircleBodiesAreColliding(
                        vertices,
                        radii,
                        ownerIndex, 
                        otherIndex,
                        out normalX,
                        out normalY,
                        out depth,
                        out contactPointX1,
                        out contactPointY1
                    );
                }
            }

            if (colliding)
            {
                // register two collisions, one for each collider.
                
                // Add one to the collisions to resolve and the other directly to collisions.
                // this is done so that during the resolution step, a seperating force is 
                // not applied twice to the same colliders.

                // collisions to resolve is later added back to the collision list after
                // they have been resolved.
                // Append(
                //     resolutionSoa, 
                //     ownerIndex, 
                //     ownerGeneration, 
                //     otherIndex,
                //     otherGeneration,
                //     normalX,
                //     normalY,
                //     ownerShapeCenterX,
                //     ownerShapeCenterY,
                //     otherShapeCenterX,
                //     otherShapeCenterY,
                //     contactPointX,
                //     contactPointY,
                //     depth,
                //     ownerFlags,
                //     otherFlags
                // );

                // Append(
                //     resolutionSoa, 
                //     otherIndex,
                //     otherGeneration,
                //     ownerIndex, 
                //     ownerGeneration, 
                //     normalX,
                //     normalY,
                //     otherShapeCenterX,
                //     otherShapeCenterY,
                //     ownerShapeCenterX,
                //     ownerShapeCenterY,
                //     contactPointX,
                //     contactPointY,
                //     depth,
                //     otherFlags,
                //     ownerFlags
                // );
            }

        }
    }

    /// <summary>
    /// Checks whether or not two circle bodies are colliding with eachother.
    /// </summary>
    /// <remarks>
    /// Note: the 'out' variables will be garbage values when this function returns false.
    /// </remarks>
    /// <param name="vertices">an SoaVector2 containing the positional verticces of the circles.</param>
    /// <param name="radii">a span containing the radii of the circles.</param>
    /// <param name="circleAIndex">the index of circle a's positional vertex in the transformed vertice span.</param>
    /// <param name="circleBIndex">the index of circle b's positional vertex in the transformed vertice span.</param>
    /// <param name="normalX">the x-component of the intersection normal in relation to  circle b.</param>
    /// <param name="normalY">the y-component of the intersection normal in relation to  circle b.</param>
    /// <param name="depth">the depth of the intersection in relation circle b.</param>
    /// <param name="contactPointX">the x-component of the point of contact.</param>
    /// <param name="contactPointY">the y-component of the point of contact.</param>
    /// <returns>true, if the two circle bodies are colliding; otherwise false.</returns>
    public static bool CircleBodiesAreColliding(
        Soa_Vector2 vertices,
        Span<float> radii,
        int circleAIndex, 
        int circleBIndex,
        out float normalX,
        out float normalY,
        out float depth,
        out float contactPointX,
        out float contactPointY
    )
    {
        
        Span<float> x = vertices.X;
        Span<float> y = vertices.Y;

        normalX         = 0;
        normalY         = 0;
        depth           = 0;
        contactPointX   = 0;
        contactPointY   = 0;

        float aX        = x[circleAIndex];
        float aY        = y[circleAIndex];
        float aRadius   = radii[circleAIndex];
        float bX        = x[circleBIndex];
        float bY        = y[circleBIndex];
        float bRadius   = radii[circleBIndex];

        // get AABB of circle A.
        Circle.GetMinMaxVectors(
            x[circleAIndex], 
            y[circleAIndex], 
            radii[circleAIndex], 
            out float aMinX,
            out float aMinY,
            out float aMaxX,
            out float aMaxY
        );

        // get AABB of circle B.
        Circle.GetMinMaxVectors(
            x[circleBIndex], 
            y[circleBIndex], 
            radii[circleBIndex], 
            out float bMinX,
            out float bMinY,
            out float bMaxX,
            out float bMaxY
        );

        // Broad Phase:
        if (Intersect(aMinX, aMinY, aMaxX, aMaxY, bMinX, bMinY, bMaxX, bMaxY) == false)
        {
            return false;
        }

        // Narrow Phase:
        // perform an SAT check.
        if(SAT.Intersect(aX, aY, aRadius, bX, bY, bRadius, out normalX, out normalY, out depth))
        {
            // submit the collision with contact points if one of the colliders needs them.
            SAT.FindContactPoints(aX, aY, aRadius, bX, bY, out contactPointX, out contactPointY);
            return true;
        }   
        return false;
    }

    /// <summary>
    /// Checks whether or not two polygon bodies are colliding with eachother.
    /// </summary>
    /// <remarks>
    /// Note: the 'out' variables will be garbage values when this function returns false.
    /// </remarks>
    /// <param name="vertices">the SoaVector2 containing the positional verticces of the polygons.</param>
    /// <param name="nextVertexIndices">a span containing the next index for a vertice in the vertices collection for a given vertice.</param>
    /// <param name="aPolygonFirstVertexIndex">polygon a's first vertex index in the vertices collection.</param>
    /// <param name="bPolygonFirstVertexIndex">polygon b's first vertex index in the vertices collection.</param>
    /// <param name="maxPolygonVerticeCount">the max count of vertices a polygon can have.</param>
    /// <param name="normalX">the x-component of the intersection normal in relation to  circle b.</param>
    /// <param name="normalY">the y-component of the intersection normal in relation to  circle b.</param>
    /// <param name="depth">the depth of the intersection in relation circle b.</param>
    /// <param name="contactPointX1">the x-component of the first given point of contact.</param>
    /// <param name="contactPointY1">the y-component of the first given point of contact.</param>
    /// <param name="contactPointX2">the x-component of the second given point of contact.</param>
    /// <param name="contactPointY2">the y-component of the second given point of contact.</param>
    /// <param name="contactPointsAmount">the amount of contact points (0,1,2).</param>
    /// <returns>true, it the two bodies are colliding; otherwise false.</returns>
    public static bool PolygonBodiesAreColliding(
        Soa_Vector2 vertices,
        Span<int> nextVertexIndices,
        int aPolygonFirstVertexIndex,
        int bPolygonFirstVertexIndex,
        int maxPolygonVerticeCount,
        out float normalX,
        out float normalY,
        out float depth,
        out float contactPointX1,
        out float contactPointY1,
        out float contactPointX2,
        out float contactPointY2,
        out int contactPointsAmount
    )
    {

        // hoisting of invariance.
        Span<float> xVertices = vertices.X;
        Span<float> yVertices = vertices.Y;
        Span<float> aX = stackalloc float[maxPolygonVerticeCount];
        Span<float> aY = stackalloc float[maxPolygonVerticeCount];
        Span<float> bX = stackalloc float[maxPolygonVerticeCount];
        Span<float> bY = stackalloc float[maxPolygonVerticeCount];

        int nextVerticeIndex = 0;
        int aVertexCount = 0;
        int bVertexCount = 0;

        // decalarations.
        normalX             = 0;
        normalY             = 0;
        depth               = 0;
        contactPointX1      = 0;
        contactPointY1      = 0;
        contactPointX2      = 0;
        contactPointY2      = 0;
        contactPointsAmount = 0;


        // gather polygon a vertices.
        nextVerticeIndex = aPolygonFirstVertexIndex;
        while (true)
        {
            // get the vertice.
            aX[aVertexCount] = xVertices[nextVerticeIndex];
            aY[aVertexCount] = yVertices[nextVerticeIndex];
            aVertexCount++;

            // break out when reaching the end of the circular loop.
            nextVerticeIndex = nextVertexIndices[nextVerticeIndex];
            if (nextVerticeIndex == aPolygonFirstVertexIndex)
                break;
        }

        // gather polygon b vertices.
        nextVerticeIndex = bPolygonFirstVertexIndex;
        while (true)
        {
            // get the vertice.
            bX[bVertexCount] = xVertices[nextVerticeIndex];
            bY[bVertexCount] = yVertices[nextVerticeIndex];
            aVertexCount++;

            // break out when reaching the end of the circular loop.
            nextVerticeIndex = nextVertexIndices[nextVerticeIndex];
            if (nextVerticeIndex == bPolygonFirstVertexIndex)
                break;
        }

        // get the min and max vectors of polygon a and b.
        GetMinMaxVectors(aX, aY, out float aMinX, out float aMinY, out float aMaxX, out float aMaxY);
        GetMinMaxVectors(bX, bY, out float bMinX, out float bMinY, out float bMaxX, out float bMaxY);

        // broad phase AABB intersect check.
        if(Intersect(aMinX, aMinY, aMaxX, aMaxY, bMinX, bMinY, bMaxX, bMaxY) == false)
            return false;

        // get the centers of both polygons.
        Centroid(aX, aY, out float aCX, out float aCY);
        Centroid(bX, bY, out float bCX, out float bCY);

        // narrow phase SAT intersect check.
        if(SAT.Intersect(aX, aY, bX, bY, aCX, aCY, bCX, bCY, out normalX, out normalY, out depth))
        {
            SAT.FindContactPoints(aX, aY, bX, bY, SAT.PolygonContactPointEpsilon, 
                out contactPointX1, 
                out contactPointY1, 
                out contactPointX2, 
                out contactPointY2, 
                out contactPointsAmount
            );

            return true;
        }

        return false;
    }

    public static bool PolygonAndCircleBodiesAreColliding(
        Soa_Vector2 vertices,
        Span<float> radii,
        Span<int> nextVertexIndices,
        int polygonFirstVertexIndex,
        int circleVertexIndex, 
        int maxPolygonVerticeCount,
        out float normalX, 
        out float normalY,
        out float depth,
        out float contactPointX,
        out float contactPointY
    )
    {
        // decalarations.
        Span<float> x           = vertices.X;
        Span<float> y           = vertices.Y;
        Span<float> polygonX    = stackalloc float[maxPolygonVerticeCount];
        Span<float> polygonY    = stackalloc float[maxPolygonVerticeCount];
        normalX                 = 0;
        normalY                 = 0;
        depth                   = 0;
        contactPointX           = 0;
        contactPointY           = 0;


        // get the vertices of polygon A.
        int polygonVertexCount = 0;
        int nextVertexIndex = polygonFirstVertexIndex;
        while (true)
        {
            // get the vertice
            polygonX[polygonVertexCount] = x[nextVertexIndex];
            polygonY[polygonVertexCount] = y[nextVertexIndex];
            polygonVertexCount++;

            // break out when reaching the endof the circular loop.
            nextVertexIndex = nextVertexIndices[nextVertexIndex];
            if(nextVertexIndex == polygonFirstVertexIndex)
                break;
        }

        // get the AABB of the circle.
        float circleX           = x[circleVertexIndex];
        float circleY           = y[circleVertexIndex];
        float circleRadius      = radii[circleVertexIndex];     
        Circle.GetMinMaxVectors(
            circleX,
            circleY,
            circleRadius,
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

        SAT.FindContactPoints(polygonX, polygonY, circleX, circleY, out float xContactPoint, out float yContactPoint);

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
            state.NextVertexIndice[previousIndex] = index;
        }

        // loop back to the beginning.
        // note: this is very important, do not remove this.
        state.NextVertexIndice[index] = firstIndex;

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
    
        Circle.
    
    ********************/




    /// <summary>
    /// Allocates a circle collider into a phsyics system state.
    /// </summary>
    /// <param name="state">The physics system state to allocate into.</param>
    /// <param name="shape">the shape data of the circle.</param>
    /// <param name="isKinematic">whether or not the physics body behvaiour is 'kinematic'.</param>
    /// <param name="isTrigger">whether or not the physics body behvaiour is 'trigger'.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static void AllocateCircleCollider(SoaPhysicsSystemState state, in Circle shape, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {
        // handle flags.
        // note: no circle shape is needed to be set as it is implied by the system that when a shape is not
        // set, a physics body is a circle.
        PhysicsBodyFlags flags = PhysicsBodyFlags.None; 

        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, false);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        // apply data.
        int index = state.FreePhysicsBodyIndex.Pop();

        state.Radii[index]      = shape.Radius;
        state.Vertices.X[index] = shape.X;
        state.Vertices.Y[index] = shape.Y;
        state.Flags[index]      = flags;

        // return gen index.

        genIndex = new(index, state.Generations[index]);

        state.AlloctedPhysicsBodyCount++;
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

        // apply data.
        int index = state.FreePhysicsBodyIndex.Pop();

        state.Radii[index]      = shape.Radius;
        state.Vertices.X[index] = shape.X;
        state.Vertices.Y[index] = shape.Y;
        state.Flags[index]      = flags;

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

        // apply data.
        int index = state.FreePhysicsBodyIndex.Pop();

        state.Radii[index]              = shape.Radius;
        state.Vertices.X[index]         = shape.X;
        state.Vertices.Y[index]         = shape.Y;
        state.Flags[index]              = flags;
        state.StaticFrictions[index]    = physicsMaterial.StaticFriction;
        state.KineticFrictions[index]   = physicsMaterial.KineticFriction;

        // return gen index.

        genIndex = new(index, state.Generations[index]);        
        
        state.AlloctedPhysicsBodyCount++;
    }




    /*******************
    
        Rectangle.
    
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

        PhysicsBodyFlags flags = PhysicsBodyFlags.PolygonShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, false);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);

        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticeFirstIndex, out int verticeCount);

        state.Heights[bodyIndex]        = shape.Height;
        state.Widths[bodyIndex]         = shape.Width;
        state.Flags[bodyIndex]          = flags;
        state.FirstVertexIndice[bodyIndex]  = verticeFirstIndex;

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

        PhysicsBodyFlags flags = PhysicsBodyFlags.PolygonShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, false);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);
    
        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticeFirstIndex, out int verticeCount);

        state.Heights[bodyIndex]        = shape.Height;
        state.Widths[bodyIndex]         = shape.Width;
        state.Flags[bodyIndex]          = flags;
        state.FirstVertexIndice[bodyIndex]  = verticeFirstIndex;

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

        PhysicsBodyFlags flags = PhysicsBodyFlags.PolygonShape;
        SetActive(ref flags, true);
        SetAllocated(ref flags, true);
        SetRigidBody(ref flags, true);
        SetHasPhysicsMaterial(ref flags, true);
        SetTrigger(ref flags, isTrigger);
        SetKinematic(ref flags, isKinematic);
    
        // apply data.

        PolygonRectangle polyRect = new(shape);

        int bodyIndex = state.FreePhysicsBodyIndex.Pop();
        AddVertices(state, VerticesXAsSpan(polyRect), VerticesYAsSpan(polyRect), out int verticeFirstIndex, out int verticeCount);

        state.Heights[bodyIndex]            = shape.Height;
        state.Widths[bodyIndex]             = shape.Width;
        state.Flags[bodyIndex]              = flags;
        state.FirstVertexIndice[bodyIndex]      = verticeFirstIndex;
        state.KineticFrictions[bodyIndex]   = physicsMaterial.KineticFriction;
        state.StaticFrictions[bodyIndex]    = physicsMaterial.StaticFriction;

        // return gen index.

        genIndex = new(bodyIndex, state.Generations[bodyIndex]);
    
        state.AlloctedPhysicsBodyCount++;
    }
}