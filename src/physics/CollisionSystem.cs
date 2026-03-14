using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Howl.DataStructures;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;
using Howl.Math.Shapes;
using System.Threading.Tasks;
using static Howl.Physics.CircleCollisionContext;
using static Howl.Physics.RectangleCollisionContext;
using static Howl.Physics.RectangleToCircleCollisionContext;
using static Howl.ECS.GenIndexListProc;
using static Howl.Math.Shapes.Circle;
using static Howl.Math.Shapes.PolygonRectangle;
using static Howl.Math.Shapes.Rectangle;
using static Howl.Math.Shapes.AABB;
using static Howl.DataStructures.BoundingVolumeHierarchy;
using static System.Runtime.InteropServices.CollectionsMarshal;
using static Howl.Math.Shapes.ShapeUtils; 
using static Howl.Physics.Collision;

namespace Howl.Physics;

public static class CollisionSystem
{
    public const float PolygonContactPointEpsilon = 1e-5f;

    /// <summary>
    /// Gets and sets the currently bound collision context for parallel circle to circle collision checks.
    /// </summary>
    public static CircleCollisionContext CircleCollisionContext;

    /// <summary>
    /// Gets and sets the currently bound collision context for parallel rectangle to rectangle collision checks.
    /// </summary>
    public static RectangleCollisionContext RectangleCollisionContext;

    /// <summary>
    /// Gets and sets the currently bound collision context for parallel rectangle to circle collision checks.
    /// </summary>
    public static RectangleToCircleCollisionContext RectangleToCircleCollisionContext;

    /// <summary>
    /// Registers all necessary components of this system.
    /// </summary>
    /// <param name="componentRegistry">The component registry to register to.</param>
    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.ThrowIfDisposed();
        componentRegistry.RegisterComponent<CircleCollider>();
        componentRegistry.RegisterComponent<RectangleCollider>();
        componentRegistry.RegisterComponent<RigidBody>();
    }

    /// <summary>
    /// Draw step for this Collision System.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    public static void Draw(ComponentRegistry componentRegistry, CollisionSystemState state, float deltaTime)
    {        
        if (state.DrawColliderWireframes)
        {            
            DebugDrawCircleColliders(componentRegistry, state);
            DebugDrawRectangleColliders(componentRegistry, state);            
        }

        if (state.DrawAABBWireframes)
        {
            DebugDrawCircleAABBs(componentRegistry, state);
            DebugDrawRectangleAABBs(componentRegistry, state);
        }

        if (state.DrawBvhBranches)
        {
            DebugDrawBvhBranches(componentRegistry, state, state.Bvh);
        }

        if (state.DrawContactPoints)
        {
            DebugDrawContactPoints(componentRegistry, state);
        }
    }


    /*******************
    
        Collider Syncing.
    
    ********************/


    /// <summary>
    /// Syncs all colliders transformed shapes to their associated transform components in a component registry.
    /// </summary>
    /// <remarks>
    /// This should be the first function call before any other in the collision system so that all transform data is relevant.
    /// </remarks>
    /// <param name="componentRegistry">The component registry that stores the colliders and transforms.</param>
    public static void SyncCollidersToTransforms(ComponentRegistry componentRegistry)
    {
        TransformCircleColliders(componentRegistry);
        TransformRectangleColliders(componentRegistry);
    }

    /// <summary>
    /// Transforms the circle colliders by their associated transform component.
    /// </summary>
    /// <param name="componentRegistry">the component registry to gather the colliders and transforms from.</param>
    public static void TransformCircleColliders(ComponentRegistry componentRegistry)
    {        
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();

        GenIndexList<CircleCollider> colliders = componentRegistry.Get<CircleCollider>();
        Span<DenseEntry<CircleCollider>> denseEntries = GetDenseAsSpan(colliders);
        
        for(int i = 0; i < denseEntries.Length; i++)
        {
            
            ref DenseEntry<CircleCollider> denseEntry = ref denseEntries[i];
            ref CircleCollider collider = ref denseEntry.Value;
            GetGenIndex(colliders, denseEntry.sparseIndex, out GenIndex genIndex);

            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail())
            {
                System.Diagnostics.Debug.Assert(false, "All circle colliders must have a transform component.");
                continue;
            }

            collider.TransformedShape = Transform(collider.BaseShape, transformRef);
        }
    } 

    /// <summary>
    /// Transforms the rectangle colliders by their associated transform component.
    /// </summary>
    /// <param name="componentRegistry">the component registry to gather the colliders and transforms from.</param>
    public static void TransformRectangleColliders(ComponentRegistry componentRegistry)
    {        
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();

        GenIndexList<RectangleCollider> colliders = componentRegistry.Get<RectangleCollider>();
        Span<DenseEntry<RectangleCollider>> denseEntries = GetDenseAsSpan(colliders);
        
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RectangleCollider> denseEntry = ref denseEntries[i];
            ref RectangleCollider collider = ref denseEntry.Value;
            GetGenIndex(colliders, denseEntry.sparseIndex, out GenIndex genIndex);

            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail())
            {
                System.Diagnostics.Debug.Assert(false, "All rectangle colliders must have a transform component.");
                continue;
            }

            collider.TransformedShape = Transform(new PolygonRectangle(collider.BaseShape), transformRef);
        }
    } 

    /// <summary>
    /// Finds all colliders that are near eachother in a constructed bvh and adds them to the 
    /// collision system states respective near lists.
    /// </summary>
    /// <param name="state">The state instance that holds a constructed bvh with collider data.</param>
    public static void FindNearColliderPairs(CollisionSystemState state)
    {
        Span<SpatialPair> spatialPairs = AsSpan(state.Bvh.SpatialPairs);
        state.NearCircleColliders.Clear();
        state.NearRectangleColliders.Clear();
        state.NearRectangleToCircleColliders.Clear();

        for(int i = 0; i < spatialPairs.Length; i++)
        {
            ref readonly SpatialPair spatialPair = ref spatialPairs[i];
            ref readonly QueryResult owner = ref spatialPair.Owner;
            ref readonly QueryResult other = ref spatialPair.Other;

            ColliderType ownerType = (ColliderType)owner.Flag;
            ColliderType otherType = (ColliderType)other.Flag;

            switch (ownerType)
            {
                case ColliderType.Circle:
                    switch (otherType)
                    {
                        case ColliderType.Circle:
                            state.NearCircleColliders.Add(
                                new ColliderPair(
                                    owner.GenIndex, 
                                    other.GenIndex
                                )
                            );
                        break;
                        case ColliderType.Rectangle:
                            state.NearRectangleToCircleColliders.Add(
                                new ColliderPair(
                                    other.GenIndex, 
                                    owner.GenIndex
                                )
                            );
                        break;
                    }
                break;
                case ColliderType.Rectangle:
                    switch (otherType)
                    {
                        case ColliderType.Rectangle:
                            state.NearRectangleColliders.Add(
                                new ColliderPair(
                                    owner.GenIndex, 
                                    other.GenIndex
                                )
                            );
                        break;
                        case ColliderType.Circle:
                            state.NearRectangleToCircleColliders.Add(
                                new ColliderPair(
                                    owner.GenIndex,
                                    other.GenIndex
                                )
                            );
                        break;
                    }
                break;
            }

        }
    }

    /// <summary>
    /// Processes all near collider pairs in a collision system state.
    /// </summary>
    /// <remarks>
    /// Note: this function should not be called in parallel.
    /// </remarks>
    /// <param name="registry">The component registry that stores all the collision system states colliders.</param>
    /// <param name="state">The  collision system state that stores the collider pairs.</param>
    public static void ProcessNearColliderPairs(ComponentRegistry registry, CollisionSystemState state)
    {        
        ProcessNearCircles(registry, state.NearCircleColliders, state.CollisionManifold);
        ProcessNearRectangles(registry, state.NearRectangleColliders, state.CollisionManifold);
        ProcessNearRectangleToCircles(registry, state.NearRectangleToCircleColliders, state.CollisionManifold);
    }

    /// <summary>
    /// Checks all near circle colliders for intersection.
    /// </summary>
    /// <remarks>
    /// Note: this function should not be called in parallel.
    /// </remarks>
    /// <param name="registry">the component registry that stores all the collision system state's colliders.</param>
    /// <param name="pairs">the near collider pairs.</param>
    public static void ProcessNearCircles(
        ComponentRegistry registry, 
        List<ColliderPair> pairs,
        CollisionManifold manifold
    )
    {        
        CircleCollisionContext = new CircleCollisionContext(registry.Get<CircleCollider>(), pairs, manifold);        

        Parallel.For(0, pairs.Count, static i =>
        {
            GenIndexList<CircleCollider> colliders = CircleCollisionContext.Colliders;
            Span<ColliderPair> pairs = AsSpan(CircleCollisionContext.Pairs);
            ref ColliderPair pair = ref pairs[i];

            GetDenseRef(colliders, pair.ColliderA, out Ref<CircleCollider> colliderRefA);
            GetDenseRef(colliders, pair.ColliderB, out Ref<CircleCollider> colliderRefB);
            
            ref CircleCollider colliderA = ref colliderRefA.Value;
            ref CircleCollider colliderB = ref colliderRefB.Value;

            // Broad Phase:
            if(Intersect(GetAABB(colliderA.TransformedShape), GetAABB(colliderB.TransformedShape)) == false)
            {
                return;    
            }


            // Narrow Phase:
            // perform an SAT check.
            if(SAT.Intersect(
                colliderA.TransformedShape,
                colliderB.TransformedShape,
                out float normalX,
                out float normalY,
                out float depth
            ))
            {

                // submit the collision with contact points if one of the colliders needs them.
                SAT.FindContactPoints(colliderA.TransformedShape, colliderB.TransformedShape, out float xContactPoint, out float yContactPoint);
                RegisterCollision(
                    CircleCollisionContext.CollisionManifold,
                    pair.ColliderA,
                    pair.ColliderB,
                    colliderA.Parameters,
                    colliderB.Parameters,
                    [xContactPoint],
                    [yContactPoint],
                    normalX, 
                    normalY, 
                    colliderA.TransformedShape.X, 
                    colliderA.TransformedShape.Y, 
                    colliderB.TransformedShape.X, 
                    colliderB.TransformedShape.Y, 
                    in depth
                );
            }              
        });

        Clear(ref CircleCollisionContext);
    }

    /// <summary>
    /// Checks all near rectangle colliders for intersection.
    /// </summary>
    /// <remarks>
    /// Note: this function should not be called in parallel.
    /// </remarks>
    /// <param name="registry">the component registry that stores all the collision system state's colliders.</param>
    /// <param name="pairs">the near collider pairs.</param>
    /// <param name="manifold">the collision manifold to register any found intersections to.</param>
    public static void ProcessNearRectangles(
        ComponentRegistry registry, 
        List<ColliderPair> pairs,
        CollisionManifold manifold
    )
    {        
        RectangleCollisionContext = new RectangleCollisionContext(registry.Get<RectangleCollider>(), pairs, manifold);

        Parallel.For(0, pairs.Count, static i =>
        {            
            CollisionManifold manifold = RectangleCollisionContext.CollisionManifold;
            GenIndexList<RectangleCollider> colliders =  RectangleCollisionContext.Colliders;
            Span<ColliderPair> pairs =  AsSpan(RectangleCollisionContext.Pairs);
            ref ColliderPair pair = ref pairs[i];

            GetDenseRef(colliders, pair.ColliderA, out Ref<RectangleCollider> colliderRefA);
            GetDenseRef(colliders, pair.ColliderB, out Ref<RectangleCollider> colliderRefB);
            ref RectangleCollider colliderA = ref colliderRefA.Value;
            ref RectangleCollider colliderB = ref colliderRefB.Value;

            // Broad Phase:
            if(Intersect(GetAABB(colliderA.TransformedShape), GetAABB(colliderB.TransformedShape)) == false)
            {
                return;    
            }

            Centroid(colliderA.TransformedShape, out float centroidAX, out float centroidAY);
            Centroid(colliderB.TransformedShape, out float centroidBX, out float centroidBY);

            Span<float> verticesAX = VerticesXAsSpan(colliderA.TransformedShape);
            Span<float> verticesAY = VerticesYAsSpan(colliderA.TransformedShape);
            Span<float> verticesBX = VerticesXAsSpan(colliderB.TransformedShape);
            Span<float> verticesBY = VerticesYAsSpan(colliderB.TransformedShape);

            // Narrow Phase:
            // perform an SAT check.
            if(SAT.Intersect(
                verticesAX,
                verticesAY,
                verticesBX,
                verticesBY,
                centroidAX, 
                centroidAY, 
                centroidBX, 
                centroidBY, 
                out float normalX, 
                out float normalY,
                out float depth

            ))
            {

                SAT.FindContactPoints(
                    verticesAX,
                    verticesAY, 
                    verticesBX,
                    verticesBY, 
                    PolygonContactPointEpsilon,
                    out float xContactPoint1, 
                    out float yContactPoint1,
                    out float xContactPoint2, 
                    out float yContactPoint2,
                    out int contactCount
                );

                // check if there are one or two contact points found.
                switch (contactCount)
                {
                    case 1:
                        RegisterCollision(
                            manifold,
                            pair.ColliderA,
                            pair.ColliderB,
                            colliderA.Parameters,
                            colliderB.Parameters,
                            [xContactPoint1],
                            [yContactPoint1],
                            normalX, 
                            normalY, 
                            centroidAX, 
                            centroidAY, 
                            centroidBX, 
                            centroidBY, 
                            depth
                        );
                    break;
                    case 2:
                        RegisterCollision(
                            manifold,
                            pair.ColliderA,
                            pair.ColliderB,
                            colliderA.Parameters,
                            colliderB.Parameters,
                            [xContactPoint1, xContactPoint2],
                            [yContactPoint1, yContactPoint2],
                            normalX, 
                            normalY, 
                            centroidAX, 
                            centroidAY, 
                            centroidBX, 
                            centroidBY, 
                            depth
                        );
                    break;
                    default:
                        throw new Exception();
                }
            }  
        });

        Clear(ref RectangleCollisionContext);
    }

    /// <summary>
    /// Checks all near rectangle to circle colliders for intersection.
    /// </summary>
    /// <remarks>
    /// Note: 
    /// - This function should not be called in parallel.
    /// - it is assumed that the collider pairs have collider A as the rectangle and collider B as the circle.
    /// </remarks>
    /// <param name="registry">the component registry that stores all the collision system state's colliders.</param>
    /// <param name="pairs">the near collider pairs.</param>
    /// <param name="manifold">the collision manifold to register any found intersections to.</param>
    public static void ProcessNearRectangleToCircles(
        ComponentRegistry registry, 
        List<ColliderPair> pairs,
        CollisionManifold manifold
    )
    {        
        RectangleToCircleCollisionContext = new RectangleToCircleCollisionContext(
            registry.Get<RectangleCollider>(),
            registry.Get<CircleCollider>(),
            pairs,
            manifold
        );

        Parallel.For(0, pairs.Count, static i =>
        {
            CollisionManifold manifold = RectangleToCircleCollisionContext.CollisionManifold;
            Span<ColliderPair> pairs = AsSpan(RectangleToCircleCollisionContext.Pairs);
            ref ColliderPair pair = ref pairs[i];

            GetDenseRef(RectangleToCircleCollisionContext.Rectangles, pair.ColliderA, out Ref<RectangleCollider> colliderRefA);
            GetDenseRef(RectangleToCircleCollisionContext.Circles, pair.ColliderB, out Ref<CircleCollider> colliderRefB);
            ref RectangleCollider rectangle = ref colliderRefA.Value;
            ref CircleCollider circle = ref colliderRefB.Value;

            // Broad Phase:
            if(Intersect(GetAABB(rectangle.TransformedShape), GetAABB(circle.TransformedShape)) == false)
            {
                return;    
            }

            // cache vertices span.
            Span<float> rectangleVerticesX = VerticesXAsSpan(rectangle.TransformedShape);
            Span<float> rectangleVerticesY = VerticesYAsSpan(rectangle.TransformedShape);
            
            // pre compute centroid.
            Centroid(
                rectangleVerticesX, 
                rectangleVerticesY, 
                out float rectangleCentroidX,
                out float rectangleCentroidY
            );

            // Narrow Phase:
            // perform an SAT check.
            if(SAT.Intersect
            (
                rectangleVerticesX,
                rectangleVerticesY,
                circle.TransformedShape,
                rectangleCentroidX,
                rectangleCentroidY,
                circle.TransformedShape.X,
                circle.TransformedShape.Y,
                out float normalX,
                out float normalY,
                out float depth
            ))
            {
                SAT.FindContactPoints(
                    rectangleVerticesX,
                    rectangleVerticesY, 
                    circle.TransformedShape, 
                    out float xContactPoint, 
                    out float yContactPoint
                );
                RegisterCollision(
                    manifold,
                    pair.ColliderA,
                    pair.ColliderB,
                    rectangle.Parameters,
                    circle.Parameters,
                    [xContactPoint],
                    [yContactPoint],
                    normalX, 
                    normalY, 
                    rectangleCentroidX, 
                    rectangleCentroidY, 
                    circle.TransformedShape.X, 
                    circle.TransformedShape.Y, 
                    depth
                );
            }              
        });

        Clear(ref RectangleToCircleCollisionContext);
    }

    /// <summary>
    /// Registers a collision into the collision manifold.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="genIndexA">The gen index of collider a.</param>
    /// <param name="genIndexB">The gen index of collider b.</param>
    /// <param name="parametersA">The parameters of collider a.</param>
    /// <param name="parametersB">The parameters of collider b.</param>
    /// <param name="xContactPoints">the x-values of the calculated contact points.</param>
    /// <param name="yContactPoints">the y-values of the calculated contact points.</param>
    /// <param name="normal">The normal of the collision.</param>
    /// <param name="depth">The depth of the collision.</param>
    private static void RegisterCollision(
        CollisionManifold collisionManifold,
        in GenIndex genIndexA, 
        in GenIndex genIndexB,
        in ColliderParameters parametersA,
        in ColliderParameters parametersB,
        Span<float> xContactPoints,
        Span<float> yContactPoints,
        float normalX,
        float normalY,
        float colliderAShapeCenterX,
        float colliderAShapeCenterY,
        float colliderBShapeCenterX,
        float colliderBShapeCenterY,
        in float depth
    )
    {                
        // register two collisions, one for each collider.
        
        // Add one to the collisions to resolve and the other directly to collisions.
        // this is done so that during the resolution step, a seperating force is 
        // not applied twice to the same colliders.

        // collisions to resolve is later added back to the collision list after
        // they have been resolved.

        collisionManifold.CollisionsToResolve.Add(
            new Collision(
                genIndexA, 
                genIndexB,
                parametersA,
                parametersB, 
                xContactPoints,
                yContactPoints,
                normalX,
                normalY,
                colliderAShapeCenterX,
                colliderAShapeCenterY,
                colliderBShapeCenterX,
                colliderBShapeCenterY,
                depth
            )
        );

        collisionManifold.Collisions.Add(
            new Collision(
                genIndexB,
                genIndexA, 
                parametersB,
                parametersA, 
                xContactPoints,
                yContactPoints,
                normalX,
                normalY,
                colliderBShapeCenterX,
                colliderBShapeCenterY,
                colliderAShapeCenterX,
                colliderAShapeCenterY,
                depth
            )
        );
    }

    public static void ResolveCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        Span<Collision> span = CollectionsMarshal.AsSpan(state.CollisionManifold.CollisionsToResolve);

        for(int i = 0; i < span.Length; i++) // NOTE: increment by two as collisions are stored as siblings before the collision manifold is sorted.
        {
            ref readonly Collision collision = ref span[i]; 
            
            if(GetDenseRef(transforms, collision.Owner, out Ref<Transform> transformRefA).Fail())
            {
                continue;
            }
            
            if(GetDenseRef(transforms, collision.Other, out Ref<Transform> transformRefB).Fail())
            {
                continue;
            }

            if(collision.OwnerParameters.Mode == ColliderMode.Kinematic)
            {
                // separate only collider B if A is Kinematic.
                float displacementX = collision.NormalX * collision.Depth;
                float displacementY = collision.NormalY * collision.Depth;
                transformRefB.Value.Position.X += displacementX;
                transformRefB.Value.Position.Y += displacementY;
            }
            else if(collision.OtherParameters.Mode == ColliderMode.Kinematic)
            {
                // separate only collider A if B is Kinematic.
                float displacementX = collision.NormalX * collision.Depth;
                float displacementY = collision.NormalY * collision.Depth;
                transformRefA.Value.Position.X -= displacementX;
                transformRefA.Value.Position.Y -= displacementY;
            }
            else
            {
                // separate both colliders if they are both dynamic.
                float displacementX = collision.NormalX * collision.Depth * 0.5f;
                float displacementY = collision.NormalY * collision.Depth * 0.5f;
                transformRefA.Value.Position.X -= displacementX;
                transformRefA.Value.Position.Y -= displacementY;    
                transformRefB.Value.Position.X += displacementX;
                transformRefB.Value.Position.Y += displacementY;    
            }
            
            var a = transformRefA.Value.Position;
            var b = transformRefB.Value.Position;

            if (float.IsNaN(a.X) || float.IsNaN(a.Y) ||
                float.IsNaN(b.X) || float.IsNaN(b.Y))
            {
                System.Diagnostics.Debug.Assert(false, "NaN!");
            }
        }


        // add the resolved collisions to the collisions list.
        state.CollisionManifold.Collisions.AddRange(state.CollisionManifold.CollisionsToResolve);
        state.CollisionManifold.CollisionsToResolve.Clear();
    }
    
    /// <summary>
    /// Reconstructs a bvh tree with the colliders within a component registry.
    /// </summary>
    /// <param name="componentRegistry">The compoonent registry to source the colliders from.</param>
    /// <param name="bvh">the bvh destination to clear and reconstruct with the new collider data.</param>
    public static void ReconstructBvhTree(ComponentRegistry componentRegistry, BoundingVolumeHierarchy bvh)
    {   
        // clear the previous bvh data.
        Clear(bvh);

        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        GenIndexList<RectangleCollider> rectangleColliders = componentRegistry.Get<RectangleCollider>();
        
        // add circle colliders.
        Span<DenseEntry<CircleCollider>> circleDenseEntries = GetDenseAsSpan(circleColliders);
        for(int i = 0; i < circleDenseEntries.Length; i++)
        {
            ref DenseEntry<CircleCollider> denseEntry = ref circleDenseEntries[i];
            ref CircleCollider collider = ref denseEntry.Value;
            GetGenIndex(circleColliders, denseEntry.sparseIndex, out GenIndex genIndex);

            InsertLeaf(
                bvh,
                new Leaf(
                    GetAABB(collider.TransformedShape),
                    genIndex, 
                    (byte)ColliderType.Circle
                )
            );
        }

        // Add rectangle colliders.
        Span<DenseEntry<RectangleCollider>> rectangleDenseEntries = GetDenseAsSpan(rectangleColliders);
        for(int i = 0; i < rectangleDenseEntries.Length; i++)
        {
            ref DenseEntry<RectangleCollider> denseEntry = ref rectangleDenseEntries[i];
            ref RectangleCollider collider = ref denseEntry.Value;
            GetGenIndex(circleColliders, denseEntry.sparseIndex, out GenIndex genIndex);

            InsertLeaf(
                bvh,
                new Leaf(
                    GetAABB(collider.TransformedShape),
                    genIndex, 
                    (byte)ColliderType.Rectangle
                )
            );
        }

        // construct the bvh with the new data.
        ConstructTree(bvh);
    }




    /*******************
    
        Debug drawing.
    
    ********************/




    private static void DebugDrawBvhBranches(ComponentRegistry componentRegistry, CollisionSystemState state, BoundingVolumeHierarchy bvh)
    {
        Span<Branch> branches = AsSpan(bvh.Branches);

        for(int i = 0; i < branches.Length; i++)
        {
            Debug.Draw.Wireframe(
                componentRegistry,
                new Transform(Vector2.Zero, Vector2.One, 0),
                new Rectangle(
                    new Vector2(branches[i].BoundingBoxMinX, branches[i].BoundingBoxMinY), 
                    new Vector2(branches[i].BoundingBoxMaxX, branches[i].BoundingBoxMaxY)
                ), 
                state.BvhBranchAABBColour
            );
        }
    }

    private static void DebugDrawCircleColliders(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<CircleCollider> colliders = componentRegistry.Get<CircleCollider>();
        Span<DenseEntry<CircleCollider>> denseEntries = GetDenseAsSpan(colliders);
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<CircleCollider> denseEntry = ref denseEntries[i];
            ref CircleCollider collider = ref denseEntry.Value;
            GetGenIndex(colliders, denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail(out var result))
            {
                System.Diagnostics.Debug.Assert(false, $"{result}");
                continue;
            }

            Colour drawColour = state.GetColliderColour(collider.Parameters);

            Debug.Draw.Wireframe(
                componentRegistry,
                transformRef.Value,
                collider.BaseShape, 
                drawColour
            );
        }
    }

    private static void DebugDrawRectangleColliders(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<RectangleCollider> colliders = componentRegistry.Get<RectangleCollider>();
        Span<DenseEntry<RectangleCollider>> denseEntries = GetDenseAsSpan(colliders);
        Span<Vector2> vertices = stackalloc Vector2[PolygonRectangle.MaxVertices];
        
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RectangleCollider> denseEntry = ref denseEntries[i];
            ref RectangleCollider collider = ref denseEntry.Value;
            PolygonRectangle shape = new PolygonRectangle(collider.BaseShape);
            GetGenIndex(colliders, denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail(out var result))
            {
                System.Diagnostics.Debug.Assert(false, $"{result}");
                continue;
            }

            ReadOnlySpan<float> verticesX = VerticesXAsSpan(shape);
            ReadOnlySpan<float> verticesY = VerticesYAsSpan(shape);

            for(int j = 0; j < PolygonRectangle.MaxVertices; j++)
            {
                vertices[j].X = verticesX[j];
                vertices[j].Y = verticesY[j];
            }

            Debug.Draw.Wireframe(
                componentRegistry,
                transformRef.Value, 
                new Polygon4(vertices), 
                state.GetColliderColour(collider.Parameters) 
            );
        }
    }

    private static void DebugDrawCircleAABBs(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<CircleCollider> colliders = componentRegistry.Get<CircleCollider>();
        Span<DenseEntry<CircleCollider>> denseEntries = GetDenseAsSpan(colliders);
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<CircleCollider> denseEntry = ref denseEntries[i];
            ref CircleCollider collider = ref denseEntry.Value;
            GetGenIndex(colliders, denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail(out var result))
            {
                System.Diagnostics.Debug.Assert(false, $"{result}");
                continue;
            }
        
            ref Transform transform = ref transformRef.Value;
            AABB aabb = GetAABB(collider.BaseShape);

            Debug.Draw.Wireframe(
                componentRegistry,
                new Transform(transform.Position, transform.Scale, 0), // no rotation for AABB's 
                new Rectangle(MinVector(aabb), MaxVector(aabb)), 
                state.AABBColour 
            );
        }
    }

    private static void DebugDrawRectangleAABBs(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<RectangleCollider> colliders = componentRegistry.Get<RectangleCollider>();
        Span<DenseEntry<RectangleCollider>> denseEntries = GetDenseAsSpan(colliders);
        Span<Vector2> vertices = stackalloc Vector2[PolygonRectangle.MaxVertices];
        
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RectangleCollider> denseEntry = ref denseEntries[i];
            ref RectangleCollider collider = ref denseEntry.Value;
            GetGenIndex(colliders, denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail(out var result))
            {
                System.Diagnostics.Debug.Assert(false, $"{result}");
                continue;
            }

            // ref Transform transform = ref transformRef.Value;

            // // convert and transform polygon rectangle to get the rotated and scaled AABB.
            // PolygonRectangle polygonRectangle = new PolygonRectangle(collider.BaseShape);
            // polygonRectangle = PolygonRectangle.Transform(polygonRectangle, transform);

            AABB aabb = GetAABB(collider.TransformedShape);
            Debug.Draw.Wireframe(
                componentRegistry,
                Math.Transform.Identity, // no rotation for AABB's 
                new Rectangle(MinVector(aabb), MaxVector(aabb)), 
                state.AABBColour
            );
        }
    }

    private static void DebugDrawContactPoints(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        ReadOnlySpan<Collision> span = state.CollisionManifold.GetCollisionsAsReadOnlySpan();
        for(int i = 0; i < span.Length; i++)
        {
            ref readonly Collision collision = ref span[i];
            Span<float> x = ContactPointsXAsSpan(collision);
            Span<float> y = ContactPointsYAsSpan(collision);


            for(int j = 0; j < collision.ContactPointsCount; j++)
            {
                Debug.Draw.Filled(
                    componentRegistry,
                    new Transform(
                        new Vector2(x[j], y[j]),
                        Vector2.One, 
                        0
                    ),
                    new Rectangle(-0.5f,0.5f, 1f, 1f), 
                    state.ContactPointColour
                );
            }            
        }
    }
}