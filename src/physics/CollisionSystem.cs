using System;
using System.Runtime.InteropServices;
using Howl.DataStructures;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public static class CollisionSystem
{
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
    /// Creates a new DrawSystem instance.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="renderer"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public static DrawSystem DrawSystem(ComponentRegistry componentRegistry, IRenderer renderer, CollisionSystemState state)
    => deltaTime =>
    {
        DrawStep(componentRegistry, renderer, state, deltaTime);
    };

    /// <summary>
    /// Draw step for this Collision System.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="renderer"></param>
    /// <param name="state"></param>
    public static void DrawStep(ComponentRegistry componentRegistry, IRenderer renderer, CollisionSystemState state, float deltaTime)
    {        
        if (state.DrawColliderWireframes)
        {            
            DebugDrawCircleColliders(componentRegistry, renderer, state);
            DebugDrawRectangleColliders(componentRegistry, renderer, state);            
        }

        if (state.DrawAABBWireframes)
        {
            DebugDrawCircleAABBs(componentRegistry, renderer, state);
            DebugDrawRectangleAABBs(componentRegistry, renderer, state);
        }

        if (state.DrawBvhBranches)
        {
            DebugDrawBvhBranches(state, renderer, state.Bvh);
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
        circleColliders.GetDenseRef(genIndexA, out Ref<CircleCollider> colliderRefA);
        circleColliders.GetDenseRef(genIndexB, out Ref<CircleCollider> colliderRefB);
        ref CircleCollider colliderA = ref colliderRefA.Value;
        ref CircleCollider colliderB = ref colliderRefB.Value;

        // make sure the circle has a transform component.
        switch(transforms.GetDenseRef(genIndexA, out Ref<Transform> transformRefA))
        {
            case GenIndexResult.DenseNotAllocated:
                throw new DenseNotAllocatedException(genIndexA);
            case GenIndexResult.StaleGenIndex:
                return;
        }

        switch(transforms.GetDenseRef(genIndexB, out Ref<Transform> transformRefB))
        {
            case GenIndexResult.DenseNotAllocated:
                throw new DenseNotAllocatedException(genIndexB);
            case GenIndexResult.StaleGenIndex:
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
            RegisterCollision(
                state,
                genIndexA,
                genIndexB,
                colliderA.Parameters,
                colliderB.Parameters,
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
        rectangleColliders.GetDenseRef(genIndexA, out Ref<RectangleCollider> colliderRefA);
        rectangleColliders.GetDenseRef(genIndexB, out Ref<RectangleCollider> colliderRefB);
        ref RectangleCollider colliderA = ref colliderRefA.Value;
        ref RectangleCollider colliderB = ref colliderRefB.Value;

        // make sure the circle has a transform component.
        switch(transforms.GetDenseRef(genIndexA, out Ref<Transform> transformRefA))
        {
            case GenIndexResult.DenseNotAllocated:
                throw new DenseNotAllocatedException(genIndexA);
            case GenIndexResult.StaleGenIndex:
                return;
        }

        switch(transforms.GetDenseRef(genIndexB, out Ref<Transform> transformRefB))
        {
            case GenIndexResult.DenseNotAllocated:
                throw new DenseNotAllocatedException(genIndexB);
            case GenIndexResult.StaleGenIndex:
                return;
        }

        PolygonRectangle rectangleA = PolygonRectangle.Transform(colliderA.Shape,transformRefA.Value);
        PolygonRectangle rectangleB = PolygonRectangle.Transform(colliderB.Shape,transformRefB.Value); 

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
            RegisterCollision(
                state,
                genIndexA,
                genIndexB,
                colliderA.Parameters,
                colliderB.Parameters,
                normal,
                depth
            );
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
        rectangleColliders.GetDenseRef(rectangleGenIndex, out Ref<RectangleCollider> colliderRefA);
        circleColliders.GetDenseRef(circleGenIndex, out Ref<CircleCollider> colliderRefB);
        ref RectangleCollider rectangleCollider = ref colliderRefA.Value;
        ref CircleCollider circleCollider = ref colliderRefB.Value;

        // make sure the circle has a transform component.
        switch(transforms.GetDenseRef(rectangleGenIndex, out Ref<Transform> transformRefA))
        {
            case GenIndexResult.DenseNotAllocated:
                throw new DenseNotAllocatedException(rectangleGenIndex);
            case GenIndexResult.StaleGenIndex:
                return;
        }

        switch(transforms.GetDenseRef(circleGenIndex, out Ref<Transform> transformRefB))
        {
            case GenIndexResult.DenseNotAllocated:
                throw new DenseNotAllocatedException(circleGenIndex);
            case GenIndexResult.StaleGenIndex:
                return;
        }

        PolygonRectangle rectangle = PolygonRectangle.Transform(rectangleCollider.Shape,transformRefA.Value);
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
            RegisterCollision(
                state,
                rectangleGenIndex, 
                circleGenIndex, 
                rectangleCollider.Parameters,
                circleCollider.Parameters,
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
    /// <param name="normal">The normal of the collision.</param>
    /// <param name="depth">The depth of the collision.</param>
    private static void RegisterCollision(
        CollisionSystemState state,
        in GenIndex genIndexA, 
        in GenIndex genIndexB,
        in ColliderParameters parametersA,
        in ColliderParameters parametersB,
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
            switch(transforms.GetDenseRef(collision.Owner, out Ref<Transform> transformRefA))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(collision.Owner);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(collision.Owner);
            }

            switch(transforms.GetDenseRef(collision.Other, out Ref<Transform> transformRefB))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(collision.Other);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(collision.Other);
            }

            if(collision.OwnerParameters.Mode == ColliderMode.Kinematic)
            {
                // separate only collider B if A is Kinematic.
                // Debug.WriteLine($"{collision.ColliderA}, {collision.ColliderB}");
                Vector2 displacement = collision.Normal * collision.Depth * 0.5f;
                transformRefB.Value.Position += displacement;
            }
            else if(collision.OtherParameters.Mode == ColliderMode.Kinematic)
            {
                // separate only collider A if B is Kinematic.
                // Debug.WriteLine($"{collision.ColliderA}, {collision.ColliderB}");
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

    private static void DebugDrawCircleColliders(ComponentRegistry componentRegistry, IRenderer renderer, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<CircleCollider> colliders = componentRegistry.Get<CircleCollider>();
        Span<DenseEntry<CircleCollider>> denseEntries = colliders.GetDenseAsSpan();
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<CircleCollider> denseEntry = ref denseEntries[i];
            ref CircleCollider collider = ref denseEntry.Value;
            colliders.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            switch(transforms.GetDenseRef(genIndex, out Ref<Transform> transformRef))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndex);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(genIndex);
            } 

            Colour drawColour = state.GetColliderColour(collider.Parameters);
            renderer.DrawWireframeShape(transformRef.Value, new CircleShape(collider.Shape, drawColour, DrawMode.Wireframe));
        }
    }

    private static void DebugDrawRectangleColliders(ComponentRegistry componentRegistry, IRenderer renderer, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<RectangleCollider> colliders = componentRegistry.Get<RectangleCollider>();
        Span<DenseEntry<RectangleCollider>> denseEntries = colliders.GetDenseAsSpan();
        Span<Vector2> vertices = stackalloc Vector2[PolygonRectangle.MaxVertices];
        
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RectangleCollider> denseEntry = ref denseEntries[i];
            ref RectangleCollider collider = ref denseEntry.Value;
            colliders.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            switch(transforms.GetDenseRef(genIndex, out Ref<Transform> transformRef))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndex);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(genIndex);
            } 

            ReadOnlySpan<float> verticesX = collider.Shape.GetVerticesXAsSpan();
            ReadOnlySpan<float> verticesY = collider.Shape.GetVerticesYAsSpan();

            for(int j = 0; j < PolygonRectangle.MaxVertices; j++)
            {
                vertices[j].X = verticesX[j];
                vertices[j].Y = verticesY[j];
            }

            renderer.DrawWireframeShape(
                transformRef.Value, 
                new Polygon4Shape(
                    new Polygon4(vertices), 
                    state.GetColliderColour(collider.Parameters), 
                    Vector2.Zero // note that origin is zero, as polygon rectangle does not have an origin field at all.
                )
            );
        }
    }

    private static void DebugDrawCircleAABBs(ComponentRegistry componentRegistry, IRenderer renderer, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<CircleCollider> colliders = componentRegistry.Get<CircleCollider>();
        Span<DenseEntry<CircleCollider>> denseEntries = colliders.GetDenseAsSpan();
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<CircleCollider> denseEntry = ref denseEntries[i];
            ref CircleCollider collider = ref denseEntry.Value;
            colliders.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            switch(transforms.GetDenseRef(genIndex, out Ref<Transform> transformRef))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndex);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(genIndex);
            } 

            AABB aabb = collider.Shape.GetAABB();

            renderer.DrawWireframeShape(
                transformRef.Value, 
                new RectangleShape(
                    new Rectangle(aabb.Min, aabb.Max), 
                    state.AABBColour, 
                    DrawMode.Wireframe
                )
            );
        }
    }

    private static void DebugDrawRectangleAABBs(ComponentRegistry componentRegistry, IRenderer renderer, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<RectangleCollider> colliders = componentRegistry.Get<RectangleCollider>();
        Span<DenseEntry<RectangleCollider>> denseEntries = colliders.GetDenseAsSpan();
        Span<Vector2> vertices = stackalloc Vector2[PolygonRectangle.MaxVertices];
        
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<RectangleCollider> denseEntry = ref denseEntries[i];
            ref RectangleCollider collider = ref denseEntry.Value;
            colliders.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            switch(transforms.GetDenseRef(genIndex, out Ref<Transform> transformRef))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndex);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(genIndex);
            } 

            AABB aabb = collider.Shape.GetAABB();

            renderer.DrawWireframeShape(
                transformRef.Value,
                new RectangleShape(
                    new Rectangle(aabb.Min, aabb.Max), 
                    state.AABBColour,
                    DrawMode.Wireframe
                )
            );
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
        Span<DenseEntry<CircleCollider>> circleDenseEntries = circleColliders.GetDenseAsSpan();
        for(int i = 0; i < circleDenseEntries.Length; i++)
        {
            ref DenseEntry<CircleCollider> denseEntry = ref circleDenseEntries[i];
            ref CircleCollider collider = ref denseEntry.Value;
            circleColliders.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            switch(transforms.GetDenseRef(genIndex, out Ref<Transform> transformRef))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndex);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(genIndex);
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
        Span<DenseEntry<RectangleCollider>> rectangleDenseEntries = rectangleColliders.GetDenseAsSpan();
        for(int i = 0; i < rectangleDenseEntries.Length; i++)
        {
            ref DenseEntry<RectangleCollider> denseEntry = ref rectangleDenseEntries[i];
            ref RectangleCollider collider = ref denseEntry.Value;
            circleColliders.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

            // ensure the collider has a transform component.
            switch(transforms.GetDenseRef(genIndex, out Ref<Transform> transformRef))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndex);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(genIndex);
            } 

            bvh.InsertLeaf(
                new Leaf(
                    PolygonRectangle.Transform(collider.Shape, transformRef).GetAABB(),
                    genIndex, 
                    (byte)ColliderType.Rectangle
                )
            );
        }

        // construct the bvh with the new data.
        bvh.Construct();
    }

    private static void DebugDrawBvhBranches(CollisionSystemState state, IRenderer renderer, BoundingVolumeHierarchy bvh)
    {
        ReadOnlySpan<Branch> branch = bvh.GetBranches();

        for(int i = 0; i < branch.Length; i++)
        {
            renderer.DrawWireframeShape(
                new Transform(Vector2.Zero, Vector2.One, 0),
                new RectangleShape(
                    new Rectangle(branch[i].AABB.Min, branch[i].AABB.Max), 
                    state.BvhBranchAABBColour,
                    DrawMode.Wireframe
                )
            );
        }
    }
}