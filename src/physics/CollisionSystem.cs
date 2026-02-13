using System;
using Howl.DataStructures;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;
using Howl.Math.Shapes;
using static Howl.ECS.GenIndexListProc;

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

    public static void FindCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        GenIndexList<RectangleCollider> rectangleColliders = componentRegistry.Get<RectangleCollider>(); 

        ReadOnlySpan<Leaf> leaves = state.Bvh.GetLeaves();

        for(int i = 0; i < leaves.Length; i++)
        {
            ColliderType colliderTypeA = (ColliderType)leaves[i].Flag;
            
            // get all near collider to the leaf AABB.
            ReadOnlySpan<QueryResult> near = state.Bvh.Query(leaves[i].AABB);
            
            for(int j = 0; j < near.Length; j++)
            {

                // ensure that intersection tests are one way.
                // This stops two colliding objects applying the same
                // collision resolution to eachother. Instead its only
                // one that applys the resolution to eachother. This also stops 
                // the same collider from colliding with itself.
                if(leaves[i].GenIndex.Index <= near[j].GenIndex.Index)
                {
                    continue;
                }

                ColliderType colliderTypeB = (ColliderType)near[j].Flag;

                switch (colliderTypeA)
                {
                    case ColliderType.Circle:
                        switch (colliderTypeB)
                        {
                            case ColliderType.Circle:
                                CircleToCircleIntersect(state, transforms, circleColliders, leaves[i].GenIndex, near[j].GenIndex);
                            break;
                            case ColliderType.Rectangle:
                                RectangleToCircleIntersect(state, transforms, rectangleColliders, circleColliders, near[j].GenIndex, leaves[i].GenIndex);
                            break;
                        }
                    break;
                    case ColliderType.Rectangle:
                        switch (colliderTypeB)
                        {
                            case ColliderType.Rectangle:
                                RectangleToRectangleIntersect(state, transforms, rectangleColliders, leaves[i].GenIndex, near[j].GenIndex);
                            break;
                            case ColliderType.Circle:
                                RectangleToCircleIntersect(state, transforms, rectangleColliders, circleColliders, leaves[i].GenIndex, near[j].GenIndex);
                            break;
                        }
                    break;
                }
            }
        }
    }
    
    private static void CircleToCircleIntersect(
        CollisionSystemState state, 
        GenIndexList<Transform> transforms,
        GenIndexList<CircleCollider> circleColliders,
        in GenIndex genIndexA, 
        in GenIndex genIndexB
    )
    {
        GetDenseRef(circleColliders, genIndexA, out Ref<CircleCollider> colliderRefA);
        GetDenseRef(circleColliders, genIndexB, out Ref<CircleCollider> colliderRefB);
        ref CircleCollider colliderA = ref colliderRefA.Value;
        ref CircleCollider colliderB = ref colliderRefB.Value;

        // make sure the circle has a transform component.
        if(GetDenseRef(transforms, genIndexA, out Ref<Transform> transformRefA).Fail(out var result1))
        {
            System.Diagnostics.Debug.Assert(false, $"{result1}");
            return;
        }

        if(GetDenseRef(transforms, genIndexB, out Ref<Transform> transformRefB).Fail(out var result2))
        {
            System.Diagnostics.Debug.Assert(false, $"{result2}");
            return;
        }

        Circle circleA = Circle.Transform(colliderA.Shape, transformRefA);
        Circle circleB = Circle.Transform(colliderB.Shape, transformRefB);

        // Broad Phase:
        if(AABB.Intersect(circleA.GetAABB(), circleB.GetAABB()) == false)
        {
            return;    
        }


        // Narrow Phase:
        // perform an SAT check.
        if(SAT.Intersect(
            circleA,
            circleB,
            out Vector2 normal,
            out float depth
        ))
        {
            // submit the collision with contact points if one of the colliders needs them.
            SAT.FindContactPoints(circleA, circleB, out float xContactPoint, out float yContactPoint);
            RegisterCollision(
                state,
                genIndexA,
                genIndexB,
                colliderA.Parameters,
                colliderB.Parameters,
                [xContactPoint],
                [yContactPoint],
                colliderA.Shape.Center,
                colliderB.Shape.Center,
                normal,
                depth
            );
        }                
    }

    private static void RectangleToRectangleIntersect(
        CollisionSystemState state, 
        GenIndexList<Transform> transforms,
        GenIndexList<RectangleCollider> rectangleColliders,
        in GenIndex genIndexA, 
        in GenIndex genIndexB
    )
    {
        GetDenseRef(rectangleColliders, genIndexA, out Ref<RectangleCollider> colliderRefA);
        GetDenseRef(rectangleColliders, genIndexB, out Ref<RectangleCollider> colliderRefB);
        ref RectangleCollider colliderA = ref colliderRefA.Value;
        ref RectangleCollider colliderB = ref colliderRefB.Value;

        // make sure the circle has a transform component.
        GetDenseRef(transforms, genIndexA, out Ref<Transform> transformRefA).Ok();
        GetDenseRef(transforms, genIndexB, out Ref<Transform> transformRefB).Ok();

        PolygonRectangle rectangleA = PolygonRectangle.Transform(new PolygonRectangle(colliderA.Shape),transformRefA.Value);
        PolygonRectangle rectangleB = PolygonRectangle.Transform(new PolygonRectangle(colliderB.Shape),transformRefB.Value); 

        // Broad Phase:
        if(AABB.Intersect(rectangleA.GetAABB(), rectangleB.GetAABB()) == false)
        {
            return;    
        }


        // Narrow Phase:
        // perform an SAT check.
        if(SAT.Intersect(
            rectangleA,
            rectangleB,
            out Vector2 normal,
            out float depth
        ))
        {

            SAT.FindContactPoints(
                rectangleA.GetVerticesXAsReadOnlySpan(),
                rectangleA.GetVerticesYAsReadOnlySpan(), 
                rectangleB.GetVerticesXAsReadOnlySpan(),
                rectangleB.GetVerticesYAsReadOnlySpan(), 
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
                        state,
                        genIndexA,
                        genIndexB,
                        colliderA.Parameters,
                        colliderB.Parameters,
                        [xContactPoint1],
                        [yContactPoint1],
                        colliderA.Shape.Center,
                        colliderB.Shape.Center,
                        normal,
                        depth
                    );
                break;
                case 2:
                    RegisterCollision(
                        state,
                        genIndexA,
                        genIndexB,
                        colliderA.Parameters,
                        colliderB.Parameters,
                        [xContactPoint1, xContactPoint2],
                        [yContactPoint1, yContactPoint2],
                        colliderA.Shape.Center,
                        colliderB.Shape.Center,
                        normal,
                        depth
                    );
                break;
                default:
                    throw new Exception();
            }
        }                
    }

    private static void RectangleToCircleIntersect(
        CollisionSystemState state, 
        GenIndexList<Transform> transforms,
        GenIndexList<RectangleCollider> rectangleColliders,
        GenIndexList<CircleCollider> circleColliders,
        in GenIndex rectangleGenIndex, 
        in GenIndex circleGenIndex
    )
    {
        GetDenseRef(rectangleColliders, rectangleGenIndex, out Ref<RectangleCollider> colliderRefA);
        GetDenseRef(circleColliders, circleGenIndex, out Ref<CircleCollider> colliderRefB);
        ref RectangleCollider rectangleCollider = ref colliderRefA.Value;
        ref CircleCollider circleCollider = ref colliderRefB.Value;

        // make sure the circle has a transform component.
        if(GetDenseRef(transforms, rectangleGenIndex, out Ref<Transform> transformRefA).Fail())
        {
            System.Diagnostics.Debug.Assert(false);
            return;            
        }
        if(GetDenseRef(transforms, circleGenIndex, out Ref<Transform> transformRefB).Fail())
        {
            System.Diagnostics.Debug.Assert(false);
            return;
        }

        ref Transform t = ref transformRefB.Value;

        PolygonRectangle rectangle = PolygonRectangle.Transform(new PolygonRectangle(rectangleCollider.Shape),transformRefA.Value);
        Circle circle = Circle.Transform(circleCollider.Shape,transformRefB.Value); 

        // Broad Phase:
        if(AABB.Intersect(rectangle.GetAABB(), circle.GetAABB()) == false)
        {
            return;    
        }


        // Narrow Phase:
        // perform an SAT check.
        if(SAT.Intersect(
            rectangle,
            circle,
            out Vector2 normal,
            out float depth
        ))
        {
            SAT.FindContactPoints(
                rectangle.GetVerticesXAsReadOnlySpan(),
                rectangle.GetVerticesYAsReadOnlySpan(), 
                circle, 
                out float xContactPoint, 
                out float yContactPoint
            );
            RegisterCollision(
                state,
                rectangleGenIndex, 
                circleGenIndex, 
                rectangleCollider.Parameters,
                circleCollider.Parameters,
                [xContactPoint],
                [yContactPoint],
                rectangleCollider.Shape.Center,
                circleCollider.Shape.Center,
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
        CollisionSystemState state,
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
        state.CollisionManifold.AddCollision(
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

        state.CollisionManifold.AddCollision(
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

    public static void ReconstructBvhTree(ComponentRegistry componentRegistry, BoundingVolumeHierarchy bvh)
    {   
        // clear the previous bvh data.
        bvh.Clear();

        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        GenIndexList<RectangleCollider> rectangleColliders = componentRegistry.Get<RectangleCollider>();
        
        // add circle colliders.
        Span<DenseEntry<CircleCollider>> circleDenseEntries = GetDenseAsSpan(circleColliders);
        for(int i = 0; i < circleDenseEntries.Length; i++)
        {
            ref DenseEntry<CircleCollider> denseEntry = ref circleDenseEntries[i];
            ref CircleCollider collider = ref denseEntry.Value;
            GetGenIndex(circleColliders, denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail(out GenIndexResult result))
            {
                System.Diagnostics.Debug.Assert(false, $"{result}");
                return;
            }

            bvh.InsertLeaf(
                new Leaf(
                    Circle.Transform(collider.Shape, transformRef).GetAABB(),
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

            // ensure the collider has a transform component.
            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail(out var result))
            {
                System.Diagnostics.Debug.Assert(false, $"{result}");
                continue;
            }

            bvh.InsertLeaf(
                new Leaf(
                    PolygonRectangle.Transform(new PolygonRectangle(collider.Shape), transformRef).GetAABB(),
                    genIndex, 
                    (byte)ColliderType.Rectangle
                )
            );
        }

        // construct the bvh with the new data.
        bvh.Construct();
    }

    private static void DebugDrawBvhBranches(ComponentRegistry componentRegistry, CollisionSystemState state, BoundingVolumeHierarchy bvh)
    {
        ReadOnlySpan<Branch> branch = bvh.GetBranches();

        for(int i = 0; i < branch.Length; i++)
        {
            Debug.Draw.Wireframe(
                componentRegistry,
                new Transform(Vector2.Zero, Vector2.One, 0),
                new Rectangle(branch[i].AABB.Min, branch[i].AABB.Max), 
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
                collider.Shape, 
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
            PolygonRectangle shape = new PolygonRectangle(collider.Shape);
            GetGenIndex(colliders, denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            if(GetDenseRef(transforms, genIndex, out Ref<Transform> transformRef).Fail(out var result))
            {
                System.Diagnostics.Debug.Assert(false, $"{result}");
                continue;
            }

            ReadOnlySpan<float> verticesX = shape.GetVerticesXAsSpan();
            ReadOnlySpan<float> verticesY = shape.GetVerticesYAsSpan();

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
            AABB aabb = collider.Shape.GetAABB();

            Debug.Draw.Wireframe(
                componentRegistry,
                new Transform(transform.Position, transform.Scale, 0), // no rotation for AABB's 
                new Rectangle(aabb.Min, aabb.Max), 
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

            ref Transform transform = ref transformRef.Value;

            // convert and transform polygon rectangle to get the rotated and scaled AABB.
            PolygonRectangle polygonRectangle = new PolygonRectangle(collider.Shape);
            polygonRectangle = PolygonRectangle.Transform(polygonRectangle, transform);
            AABB aabb = polygonRectangle.GetAABB();

            Debug.Draw.Wireframe(
                componentRegistry,
                Transform.Identity, // no rotation for AABB's 
                new Rectangle(aabb.Min, aabb.Max), 
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