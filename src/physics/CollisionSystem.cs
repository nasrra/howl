using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl.DataStructures;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;
using Howl.Math.Shapes;
using static Howl.ECS.GenIndexListProc;
using static Howl.Math.Shapes.Circle;
using static Howl.Math.Shapes.PolygonRectangle;
using static Howl.Math.Shapes.Rectangle;
using static Howl.Math.Shapes.AABB;

namespace Howl.Physics;

public static class CollisionSystem
{
    private const float PolygonContactPointEpsilon = 1e-5f;

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
        ReadOnlySpan<SpatialPair> spatialPairs = state.Bvh.GetSpatialPairs();
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
    /// <param name="componentRegistry">The component registry that stores all the collision system states colliders.</param>
    /// <param name="state">The  collision system state that stores the collider pairs.</param>
    public static void ProcessNearColliderPairs(ComponentRegistry componentRegistry, CollisionSystemState state)
    {        
        ProcessNearCircles(componentRegistry, state.NearCircleColliders, state.CollisionManifold);
        ProcessNearRectangles(componentRegistry, state.NearRectangleColliders, state.CollisionManifold);
        ProcessNearRectangleToCircles(componentRegistry, state.NearRectangleToCircleColliders, state.CollisionManifold);
    }

    /// <summary>
    /// Checks all near circle colliders for intersection.
    /// </summary>
    /// <param name="componentRegistry">the component registry that stores all the collision system state's colliders.</param>
    /// <param name="colliderPairs">the near collider pairs.</param>
    /// <param name="collisionManifold">the collision manifold to register any found intersections to.</param>
    public static void ProcessNearCircles(
        ComponentRegistry componentRegistry, 
        List<ColliderPair> colliderPairs,
        CollisionManifold collisionManifold
    )
    {        
        GenIndexList<CircleCollider> colliders = componentRegistry.Get<CircleCollider>();
        ReadOnlySpan<ColliderPair> span = CollectionsMarshal.AsSpan(colliderPairs);
        ref ColliderPair start = ref MemoryMarshal.GetReference(span);

        for (int i = 0; i < span.Length; i++)
        {
            ref readonly ColliderPair pair = ref Unsafe.Add(ref start, i);
            CircleToCircleIntersect(
                collisionManifold,
                colliders,
                pair.ColliderA,
                pair.ColliderB
            );
        }
    }

    /// <summary>
    /// Checks all near rectangle colliders for intersection.
    /// </summary>
    /// <param name="componentRegistry">the component registry that stores all the collision system state's colliders.</param>
    /// <param name="colliderPairs">the near collider pairs.</param>
    /// <param name="collisionManifold">the collision manifold to register any found intersections to.</param>
    public static void ProcessNearRectangles(
        ComponentRegistry componentRegistry, 
        List<ColliderPair> colliderPairs,
        CollisionManifold collisionManifold
    )
    {        
        GenIndexList<RectangleCollider> colliders = componentRegistry.Get<RectangleCollider>();
        ReadOnlySpan<ColliderPair> span = CollectionsMarshal.AsSpan(colliderPairs);
        ref ColliderPair start = ref MemoryMarshal.GetReference(span); 

        for(int i = 0; i < span.Length; i++)
        {
            ref readonly ColliderPair pair = ref Unsafe.Add(ref start, i); 
            RectangleToRectangleIntersect(collisionManifold, colliders, pair.ColliderA, pair.ColliderB);
        }
    }

    /// <summary>
    /// Checks all near rectangle to circle colliders for intersection.
    /// </summary>
    /// <remarks>
    /// Note: it is assumed that the collider pairs have collider A as the rectangle and collider B as the circle.
    /// </remarks>
    /// <param name="componentRegistry">the component registry that stores all the collision system state's colliders.</param>
    /// <param name="colliderPairs">the near collider pairs.</param>
    /// <param name="collisionManifold">the collision manifold to register any found intersections to.</param>
    public static void ProcessNearRectangleToCircles(
        ComponentRegistry componentRegistry, 
        List<ColliderPair> colliderPairs,
        CollisionManifold collisionManifold
    )
    {        
        GenIndexList<RectangleCollider> rectangles = componentRegistry.Get<RectangleCollider>();
        GenIndexList<CircleCollider> circles = componentRegistry.Get<CircleCollider>();
        ReadOnlySpan<ColliderPair> span = CollectionsMarshal.AsSpan(colliderPairs);
        ref ColliderPair start = ref MemoryMarshal.GetReference(span);

        for(int i = 0; i < span.Length; i++)
        {
            ref readonly ColliderPair pair = ref Unsafe.Add(ref start, i); 
            RectangleToCircleIntersect(collisionManifold, rectangles, circles, pair.ColliderA, pair.ColliderB);
        }
    }

    /// <summary>
    /// Performs a intersection check against two circle colliders.
    /// </summary>
    /// <param name="collisionManifold">the collision manifold to register to if an intersection was found.</param>
    /// <param name="circleColliders">the colliders list to source the two colliders from.</param>
    /// <param name="genIndexA">the gen index of collider a.</param>
    /// <param name="genIndexB">the gen index of collider b.</param>
    private static void CircleToCircleIntersect(
        CollisionManifold collisionManifold, 
        GenIndexList<CircleCollider> circleColliders,
        in GenIndex genIndexA, 
        in GenIndex genIndexB
    )
    {
        GetDenseRef(circleColliders, genIndexA, out Ref<CircleCollider> colliderRefA);
        GetDenseRef(circleColliders, genIndexB, out Ref<CircleCollider> colliderRefB);
        ref CircleCollider colliderA = ref colliderRefA.Value;
        ref CircleCollider colliderB = ref colliderRefB.Value;

        // Broad Phase:
        if(AABB.Intersect(GetAABB(colliderA.TransformedShape), GetAABB(colliderB.TransformedShape)) == false)
        {
            return;    
        }


        // Narrow Phase:
        // perform an SAT check.
        if(SAT.Intersect(
            colliderA.TransformedShape,
            colliderB.TransformedShape,
            out Vector2 normal,
            out float depth
        ))
        {
            // submit the collision with contact points if one of the colliders needs them.
            SAT.FindContactPoints(colliderA.TransformedShape, colliderB.TransformedShape, out float xContactPoint, out float yContactPoint);
            RegisterCollision(
                collisionManifold,
                genIndexA,
                genIndexB,
                colliderA.Parameters,
                colliderB.Parameters,
                [xContactPoint],
                [yContactPoint],
                Center(colliderA.TransformedShape),
                Center(colliderB.TransformedShape),
                normal,
                depth
            );
        }                
    }

    /// <summary>
    /// Performs a intersection check against two rectangle colliders.
    /// </summary>
    /// <param name="collisionManifold">the collision manifold to register to if an intersection was found.</param>
    /// <param name="rectangleColliders">the colliders list to source the two colliders from.</param>
    /// <param name="genIndexA">the gen index of collider a.</param>
    /// <param name="genIndexB">the gen index of collider b.</param>
    /// <exception cref="Exception"></exception>
    private static void RectangleToRectangleIntersect(
        CollisionManifold collisionManifold, 
        GenIndexList<RectangleCollider> rectangleColliders,
        in GenIndex genIndexA, 
        in GenIndex genIndexB
    )
    {
        GetDenseRef(rectangleColliders, genIndexA, out Ref<RectangleCollider> colliderRefA);
        GetDenseRef(rectangleColliders, genIndexB, out Ref<RectangleCollider> colliderRefB);
        ref RectangleCollider colliderA = ref colliderRefA.Value;
        ref RectangleCollider colliderB = ref colliderRefB.Value;

        // Broad Phase:
        if(AABB.Intersect(GetAABB(colliderA.TransformedShape), GetAABB(colliderB.TransformedShape)) == false)
        {
            return;    
        }

        Vector2 colliderACentroid = Centroid(colliderA.TransformedShape);
        Vector2 colliderBCentroid = Centroid(colliderB.TransformedShape);

        // Narrow Phase:
        // perform an SAT check.
        if(SAT.Intersect(
            colliderA.TransformedShape,
            colliderB.TransformedShape,
            colliderACentroid,
            colliderBCentroid,
            out Vector2 normal,
            out float depth
        ))
        {

            SAT.FindContactPoints(
                VerticesXAsReadOnlySpan(colliderA.TransformedShape),
                VerticesYAsReadOnlySpan(colliderA.TransformedShape), 
                VerticesXAsReadOnlySpan(colliderB.TransformedShape),
                VerticesYAsReadOnlySpan(colliderB.TransformedShape), 
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
                        collisionManifold,
                        genIndexA,
                        genIndexB,
                        colliderA.Parameters,
                        colliderB.Parameters,
                        [xContactPoint1],
                        [yContactPoint1],
                        colliderACentroid,
                        colliderBCentroid,
                        normal,
                        depth
                    );
                break;
                case 2:
                    RegisterCollision(
                        collisionManifold,
                        genIndexA,
                        genIndexB,
                        colliderA.Parameters,
                        colliderB.Parameters,
                        [xContactPoint1, xContactPoint2],
                        [yContactPoint1, yContactPoint2],
                        colliderACentroid,
                        colliderBCentroid,
                        normal,
                        depth
                    );
                break;
                default:
                    throw new Exception();
            }
        }                
    }

    /// <summary>
    /// Performs a intersection check against a rectangle  and circle collider.
    /// </summary>
    /// <param name="collisionManifold">the collision manifold to register to if an intersection was found.</param>
    /// <param name="rectangleColliders">the colliders list to source the rectangle collider from.</param>
    /// <param name="circleColliders">the colliders list to source the circle collider from.</param>
    /// <param name="rectangleGenIndex">the gen index of the rectangle collider.</param>
    /// <param name="circleGenIndex">the gen index of the circle collider.</param>
    private static void RectangleToCircleIntersect(
        CollisionManifold collisionManifold,
        GenIndexList<RectangleCollider> rectangleColliders,
        GenIndexList<CircleCollider> circleColliders,
        in GenIndex rectangleGenIndex, 
        in GenIndex circleGenIndex
    )
    {
        GetDenseRef(rectangleColliders, rectangleGenIndex, out Ref<RectangleCollider> colliderRefA);
        GetDenseRef(circleColliders, circleGenIndex, out Ref<CircleCollider> colliderRefB);
        ref RectangleCollider rectangle = ref colliderRefA.Value;
        ref CircleCollider circle = ref colliderRefB.Value;

        // Broad Phase:
        if(AABB.Intersect(GetAABB(rectangle.TransformedShape), GetAABB(circle.TransformedShape)) == false)
        {
            return;    
        }

        // Narrow Phase:
        // perform an SAT check.
        if(SAT.Intersect(
            rectangle.TransformedShape,
            circle.TransformedShape,
            out Vector2 normal,
            out float depth
        ))
        {
            SAT.FindContactPoints(
                VerticesXAsReadOnlySpan(rectangle.TransformedShape),
                VerticesYAsReadOnlySpan(rectangle.TransformedShape), 
                circle.TransformedShape, 
                out float xContactPoint, 
                out float yContactPoint
            );
            RegisterCollision(
                collisionManifold,
                rectangleGenIndex, 
                circleGenIndex, 
                rectangle.Parameters,
                circle.Parameters,
                [xContactPoint],
                [yContactPoint],
                Centroid(rectangle.TransformedShape),
                Center(circle.TransformedShape),
                normal,
                depth
            );
        }                
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
        ReadOnlySpan<float> xContactPoints,
        ReadOnlySpan<float> yContactPoints,
        Vector2 colliderShapeCenterA,
        Vector2 colliderShapeCenterB,
        in Vector2 normal,
        in float depth
    )
    {        
        // Add the sibling collisions to the collisions manifold for later resolution.
        // NOTE: this is done so that the collision manifold can correctly binary search
        // for collisions outside of the collision system.
        collisionManifold.AddCollision(
            new Collision(
                genIndexA, 
                genIndexB,
                parametersA,
                parametersB, 
                xContactPoints,
                yContactPoints,
                colliderShapeCenterA,
                colliderShapeCenterB,
                normal, 
                depth
            )
        );

        collisionManifold.AddCollision(
            new Collision(
                genIndexB,
                genIndexA, 
                parametersB,
                parametersA, 
                xContactPoints,
                yContactPoints,
                colliderShapeCenterB,
                colliderShapeCenterA,
                normal, 
                depth
            )
        );
    }

    public static void ResolveCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        ReadOnlySpan<Collision> span = state.CollisionManifold.GetCollisionsAsReadOnlySpan();

        for(int i = 0; i < span.Length; i+=2) // NOTE: increment by two as collisions are stored as siblings before the collision manifold is sorted.
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
                Vector2 displacement = collision.Normal * collision.Depth * 0.5f;
                transformRefB.Value.Position += displacement;
            }
            else if(collision.OtherParameters.Mode == ColliderMode.Kinematic)
            {
                // separate only collider A if B is Kinematic.
                Vector2 displacement = collision.Normal * collision.Depth;                
                transformRefA.Value.Position -= displacement;
            }
            else
            {
                // separate both colliders if they are both dynamic.
                Vector2 displacement = collision.Normal * collision.Depth * 0.5f;
                transformRefA.Value.Position -= displacement;
                transformRefB.Value.Position += displacement;    
            }

        }
    }
    
    /// <summary>
    /// Reconstructs a bvh tree with the colliders within a component registry.
    /// </summary>
    /// <param name="componentRegistry">The compoonent registry to source the colliders from.</param>
    /// <param name="bvh">the bvh destination to clear and reconstruct with the new collider data.</param>
    public static void ReconstructBvhTree(ComponentRegistry componentRegistry, BoundingVolumeHierarchy bvh)
    {   
        // clear the previous bvh data.
        bvh.Clear();

        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        GenIndexList<RectangleCollider> rectangleColliders = componentRegistry.Get<RectangleCollider>();
        
        // add circle colliders.
        Span<DenseEntry<CircleCollider>> circleDenseEntries = GetDenseAsSpan(circleColliders);
        for(int i = 0; i < circleDenseEntries.Length; i++)
        {
            ref DenseEntry<CircleCollider> denseEntry = ref circleDenseEntries[i];
            ref CircleCollider collider = ref denseEntry.Value;
            GetGenIndex(circleColliders, denseEntry.sparseIndex, out GenIndex genIndex);

            bvh.InsertLeaf(
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

            bvh.InsertLeaf(
                new Leaf(
                    GetAABB(collider.TransformedShape),
                    genIndex, 
                    (byte)ColliderType.Rectangle
                )
            );
        }

        // construct the bvh with the new data.
        bvh.Construct();
    }




    /*******************
    
        Debug drawing.
    
    ********************/




    private static void DebugDrawBvhBranches(ComponentRegistry componentRegistry, CollisionSystemState state, BoundingVolumeHierarchy bvh)
    {
        ReadOnlySpan<Branch> branch = bvh.GetBranches();

        for(int i = 0; i < branch.Length; i++)
        {
            Debug.Draw.Wireframe(
                componentRegistry,
                new Transform(Vector2.Zero, Vector2.One, 0),
                new Rectangle(
                    new Vector2(branch[i].BoundingBoxMinX, branch[i].BoundingBoxMinY), 
                    new Vector2(branch[i].BoundingBoxMaxX, branch[i].BoundingBoxMaxY)
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
            ReadOnlySpan<float> x = collision.GetXContactPointsAsReadOnlySpan();
            ReadOnlySpan<float> y = collision.GetYContactPointsAsReadOnlySpan();


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