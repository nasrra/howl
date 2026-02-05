using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Physics.BVH;

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
    /// Creates a new FixedUpdateSystem instance.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static FixedUpdateSystem FixedUpdateSystem(ComponentRegistry componentRegistry, CollisionSystemState state)
    => deltaTime =>
    {
        FixedUpdateStep(componentRegistry, state, deltaTime);
    };

    /// <summary>
    /// FixedUpdate step for this collision system.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <param name="deltaTime"></param>
    public static void FixedUpdateStep(ComponentRegistry componentRegistry, CollisionSystemState state, float deltaTime)
    {        
        if(state.CollisionManifold.Count > 0)
        {
            state.CollisionManifold.Clear();
        }
        
        // reconstruct the bvh.
        state.IntersectStepStopwatch.Restart();
        ReconstructBvhTree(componentRegistry, state.BVH);
        FindCollisions(componentRegistry, state);

        // FindCircleCollisions(componentRegistry, state);
        // FindRectangleCollisions(componentRegistry, state);
        // FindRectangleToCircleCollisions(componentRegistry, state);

        state.IntersectStepStopwatch.Stop();

        state.ResolutionStepStopwatch.Restart();
        ResolveCollisions(componentRegistry, state);
        state.ResolutionStepStopwatch.Stop();            
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

        if (state.DrawBvhLeaves)
        {
            DebugDrawBvhLeaves(state, renderer, state.BVH);
        }

        if (state.DrawBvhBranches)
        {
            DebugDrawBvhBranches(state, renderer, state.BVH);
        }

        if (state.DrawBvhRoot)
        {
            DebugDrawBvhRoot(state, renderer, state.BVH);
        }
    }

    private static void FindCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        GenIndexList<RectangleCollider> rectangleColliders = componentRegistry.Get<RectangleCollider>(); 

        ReadOnlySpan<Leaf> leaves = state.BVH.GetLeavesAsReadOnlySpan();
        Span<GenIndex> genIndices = stackalloc GenIndex[Leaf.MaxEntries];

        for(int i = 0; i < leaves.Length; i++)
        {
            leaves[i].GetGenIndices(ref genIndices, out int written);
            ReadOnlySpan<byte> flags = leaves[i].GetFlags();

            // get all near collider to the leaf AABB.
            ReadOnlySpan<QueryResult> near = state.BVH.QuerySlow(leaves[i].AABB);
            
            for(int j = 0; j < leaves[i].EntriesCount; j++)
            {
                ColliderType colliderTypeA = (ColliderType)flags[j];

                for(int k = 0; k < near.Length; k++)
                {
                    // ensure that intersection tests are one way.
                    // This stops two colliding objects applying the same
                    // collision resolution to eachother. Instead its only
                    // one that applys the resolution to eachother. This also stops 
                    // the same collider from colliding with itself.
                    if(genIndices[j].index <= near[k].GenIndex.index)
                    {
                        continue;
                    }

                    ColliderType colliderTypeB = (ColliderType)near[k].Flags;

                    switch (colliderTypeA)
                    {
                        case ColliderType.Circle:
                            switch (colliderTypeB)
                            {
                                case ColliderType.Circle:
                                    CircleToCircleIntersect(state, transforms, circleColliders, genIndices[j], near[k].GenIndex);
                                break;
                                case ColliderType.Rectangle:
                                    RectangleToCircleIntersect(state, transforms, rectangleColliders, circleColliders, near[k].GenIndex, genIndices[j]);
                                break;
                            }
                        break;
                        case ColliderType.Rectangle:
                            switch (colliderTypeB)
                            {
                                case ColliderType.Rectangle:
                                    RectangleToRectangleIntersect(state, transforms, rectangleColliders, genIndices[j], near[k].GenIndex);
                                break;
                                case ColliderType.Circle:
                                    RectangleToCircleIntersect(state, transforms, rectangleColliders, circleColliders, genIndices[j], near[k].GenIndex);
                                break;
                            }
                        break;
                    }
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

            // add the collision to the collisions manifold for later resolution.
            state.CollisionManifold.Add(
                new Collision(
                    genIndexA, 
                    genIndexB,
                    colliderA.Parameters,
                    colliderB.Parameters, 
                    normal, 
                    depth
                )
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

            // add the collision to the collisions manifold for later resolution.
            state.CollisionManifold.Add(
                new Collision(
                    genIndexA, 
                    genIndexB,
                    colliderA.Parameters,
                    colliderB.Parameters, 
                    normal, 
                    depth
                )
            );
        }                
    }

    private static void RectangleToCircleIntersect(
        CollisionSystemState state, 
        GenIndexList<Transform> transforms,
        GenIndexList<RectangleCollider> rectangleColliders,
        GenIndexList<CircleCollider> circleColliders,
        in GenIndex rectangleGenIndexA, 
        in GenIndex circleGenIndexB
    )
    {
        rectangleColliders.GetDenseRef(rectangleGenIndexA, out Ref<RectangleCollider> colliderRefA);
        circleColliders.GetDenseRef(circleGenIndexB, out Ref<CircleCollider> colliderRefB);
        ref RectangleCollider colliderA = ref colliderRefA.Value;
        ref CircleCollider colliderB = ref colliderRefB.Value;

        // make sure the circle has a transform component.
        switch(transforms.GetDenseRef(rectangleGenIndexA, out Ref<Transform> transformRefA))
        {
            case GenIndexResult.DenseNotAllocated:
                throw new DenseNotAllocatedException(rectangleGenIndexA);
            case GenIndexResult.StaleGenIndex:
                return;
        }

        switch(transforms.GetDenseRef(circleGenIndexB, out Ref<Transform> transformRefB))
        {
            case GenIndexResult.DenseNotAllocated:
                throw new DenseNotAllocatedException(circleGenIndexB);
            case GenIndexResult.StaleGenIndex:
                return;
        }

        PolygonRectangle rectangle = PolygonRectangle.Transform(colliderA.Shape,transformRefA.Value);
        Circle circle = Circle.Transform(colliderB.Shape,transformRefB.Value); 

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

            // add the collision to the collisions manifold for later resolution.
            state.CollisionManifold.Add(
                new Collision(
                    rectangleGenIndexA, 
                    circleGenIndexB,
                    colliderA.Parameters,
                    colliderB.Parameters, 
                    normal, 
                    depth
                )
            );
        }                
    }

    private static void ResolveCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        Span<Collision> span = CollectionsMarshal.AsSpan(state.CollisionManifold);

        for(int i = 0; i < span.Length; i++)
        {
            ref Collision collision = ref span[i];
            switch(transforms.GetDenseRef(collision.ColliderA, out Ref<Transform> transformRefA))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(collision.ColliderA);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(collision.ColliderA);
            }

            switch(transforms.GetDenseRef(collision.ColliderB, out Ref<Transform> transformRefB))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(collision.ColliderB);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(collision.ColliderB);
            }

            if(collision.ColliderAParameters.Mode == ColliderMode.Kinematic)
            {
                // separate only collider B if A is Kinematic.
                // Debug.WriteLine($"{collision.ColliderA}, {collision.ColliderB}");
                Vector2 displacement = collision.Normal * collision.Depth * 0.5f;
                transformRefB.Value.Position += displacement;
            }
            else if(collision.ColliderBParameters.Mode == ColliderMode.Kinematic)
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

    private static void ReconstructBvhTree(ComponentRegistry componentRegistry, BoundingVolumeHierarchy bvh)
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

            bvh.AddEntry(
                new Entry(
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

            bvh.AddEntry(
                new Entry(
                    PolygonRectangle.Transform(collider.Shape, transformRef).GetAABB(),
                    genIndex, 
                    (byte)ColliderType.Rectangle
                )
            );
        }

        // construct the bvh with the new data.
        bvh.Construct();
    }

    private static void DebugDrawBvhLeaves(CollisionSystemState state, IRenderer renderer, BoundingVolumeHierarchy bvh)
    {
        ReadOnlySpan<Leaf> leaves = bvh.GetLeavesAsReadOnlySpan();

        for(int i = 0; i < leaves.Length; i++)
        {
            renderer.DrawWireframeShape(
                new Transform(Vector2.Zero, Vector2.One, 0),
                new RectangleShape(
                    new Rectangle(leaves[i].AABB.Min, leaves[i].AABB.Max), 
                    state.BvhLeafAABBColour,
                    DrawMode.Wireframe
                )
            );
        }
    }

    private static void DebugDrawBvhBranches(CollisionSystemState state, IRenderer renderer, BoundingVolumeHierarchy bvh)
    {
        ReadOnlySpan<Branch> branch = bvh.GetBranchesAsReadOnlySpan();

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

    private static void DebugDrawBvhRoot(CollisionSystemState state, IRenderer renderer, BoundingVolumeHierarchy bvh)
    {
        ReadOnlySpan<Branch> branch = bvh.GetBranchesAsReadOnlySpan();

        for(int i = 0; i < branch.Length; i++)
        {
            renderer.DrawWireframeShape(
                new Transform(Vector2.Zero, Vector2.One, 0),
                new RectangleShape(
                    new Rectangle(branch[branch.Length - 1].AABB.Min, branch[branch.Length - 1].AABB.Max), 
                    state.BvhBranchAABBColour,
                    DrawMode.Wireframe
                )
            );
        }
    }


    // slow.




    private static void FindCircleCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        Span<DenseEntry<CircleCollider>> denseEntries = circleColliders.GetDenseAsSpan();

        for(int i = 0; i < denseEntries.Length - 1; i++)
        {

            // get the current circle.
            ref DenseEntry<CircleCollider> denseEntryA = ref denseEntries[i];
            ref CircleCollider colliderA = ref denseEntryA.Value;
            circleColliders.GetGenIndex(denseEntryA.sparseIndex, out GenIndex genIndexA);
            
            // make sure the circle has a transform component.
            switch(transforms.GetDenseRef(genIndexA, out Ref<Transform> transformRefA))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndexA);
                case GenIndexResult.StaleGenIndex:
                    continue;
            }
            ref Transform transformA = ref transformRefA.Value;

            for(int j = i + 1; j < denseEntries.Length; j++)
            {

                // get the other circle to check intersection against.
                ref DenseEntry<CircleCollider> denseEntryB = ref denseEntries[j];
                ref CircleCollider colliderB = ref denseEntryB.Value;
                circleColliders.GetGenIndex(denseEntryB.sparseIndex, out GenIndex genIndexB);                
                
                // make sure this circle has a transform component.
                switch(transforms.GetDenseRef(genIndexB, out Ref<Transform> transformRefB))
                {
                    case GenIndexResult.DenseNotAllocated:
                        throw new DenseNotAllocatedException(genIndexA);
                    case GenIndexResult.StaleGenIndex:
                        continue;
                }
                ref Transform transformB = ref transformRefB.Value;

                // transform shapes.
                Circle circleA = Circle.Transform(colliderA.Shape, transformA); 
                Circle circleB = Circle.Transform(colliderB.Shape, transformB); 

                // Broad Phase:
                // perform an AABB check. 
                if(AABB.Intersect(circleA.GetAABB(), circleB.GetAABB()) == false)
                {
                    continue;
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

                    // add the collision to the collisions manifold for later resolution.
                    state.CollisionManifold.Add(
                        new Collision(
                            genIndexA, 
                            genIndexB,
                            colliderA.Parameters,
                            colliderB.Parameters, 
                            normal, 
                            depth
                        )
                    );
                    
                }
            }
        }
    }

    private static void FindRectangleCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<RectangleCollider> colliders = componentRegistry.Get<RectangleCollider>();
        Span<DenseEntry<RectangleCollider>> denseEntries = colliders.GetDenseAsSpan();

        for(int i = 0; i < denseEntries.Length - 1; i++)
        {

            // get the current collider.
            ref DenseEntry<RectangleCollider> denseEntryA = ref denseEntries[i];
            ref RectangleCollider colliderA = ref denseEntryA.Value;
            colliders.GetGenIndex(denseEntryA.sparseIndex, out GenIndex genIndexA);
            
            // make sure the collider has a transform component.
            switch(transforms.GetDenseRef(genIndexA, out Ref<Transform> transformRefA))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndexA);
                case GenIndexResult.StaleGenIndex:
                    continue;
            }

            for(int j = i + 1; j < denseEntries.Length; j++)
            {

                // get the other collider to check intersection against.
                ref DenseEntry<RectangleCollider> denseEntryB = ref denseEntries[j];
                ref RectangleCollider colliderB = ref denseEntryB.Value;
                colliders.GetGenIndex(denseEntryB.sparseIndex, out GenIndex genIndexB);                
                
                // make sure this collider has a transform component.
                switch(transforms.GetDenseRef(genIndexB, out Ref<Transform> transformRefB))
                {
                    case GenIndexResult.DenseNotAllocated:
                        throw new DenseNotAllocatedException(genIndexA);
                    case GenIndexResult.StaleGenIndex:
                        continue;
                }

                // transform shapes.
                PolygonRectangle rectangleA = PolygonRectangle.Transform(colliderA.Shape,transformRefA.Value);
                PolygonRectangle rectangleB = PolygonRectangle.Transform(colliderB.Shape,transformRefB.Value); 

                // Broad Phase:
                // perform an AABB check.
                if(AABB.Intersect(rectangleA.GetAABB(), rectangleB.GetAABB()) == false)
                {
                    continue;
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
                    // add the collision to the collisions manifold for later resolution.
                    state.CollisionManifold.Add(
                        new Collision(
                            genIndexA, 
                            genIndexB,
                            colliderA.Parameters,
                            colliderB.Parameters,
                            normal, 
                            depth
                        )
                    );                
                }
            }
        }        
    }

    private static void FindRectangleToCircleCollisions(ComponentRegistry componentRegistry, CollisionSystemState state)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        
        GenIndexList<RectangleCollider> rectangleColliders = componentRegistry.Get<RectangleCollider>();
        Span<DenseEntry<RectangleCollider>> rectangleDenseEntries = rectangleColliders.GetDenseAsSpan();
        
        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        Span<DenseEntry<CircleCollider>> circleDenseEntries = circleColliders.GetDenseAsSpan();

        for(int i = 0; i < rectangleDenseEntries.Length; i++)
        {
            // get the rectangle collider.
            ref DenseEntry<RectangleCollider> rectangleDenseEntry = ref rectangleDenseEntries[i];
            ref RectangleCollider rectangleCollider = ref rectangleDenseEntry.Value;
            rectangleColliders.GetGenIndex(rectangleDenseEntry.sparseIndex, out GenIndex rectangleGenIndex);

            // make sure the collider has a transform component
            switch(transforms.GetDenseRef(rectangleGenIndex, out Ref<Transform> rectangleTransformRef))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(rectangleGenIndex);
                case GenIndexResult.StaleGenIndex:
                    throw new StaleGenIndexException(rectangleGenIndex);
            }

            for(int j = 0; j < circleDenseEntries.Length; j++)
            {
                
                // get the circle to test intersect against.
                ref DenseEntry<CircleCollider> circleDenseEntry = ref circleDenseEntries[j];
                ref CircleCollider circleCollider = ref circleDenseEntry.Value;
                circleColliders.GetGenIndex(circleDenseEntry.sparseIndex, out GenIndex circleGenIndex);

                // make sure the collider has a transform component.
                switch(transforms.GetDenseRef(circleGenIndex, out Ref<Transform> circleTransformRef))
                {
                    case GenIndexResult.DenseNotAllocated:
                        throw new DenseNotAllocatedException(circleGenIndex);
                    case GenIndexResult.StaleGenIndex:
                        throw new StaleGenIndexException(circleGenIndex);
                }

                // transform shapes.
                PolygonRectangle rectangle = PolygonRectangle.Transform(rectangleCollider.Shape,rectangleTransformRef.Value); 
                Circle circle = Circle.Transform(circleCollider.Shape,circleTransformRef.Value); 

                // Broad Phase:
                // perform AABB check.
                if(AABB.Intersect(rectangle.GetAABB(), circle.GetAABB()) == false)
                {
                    continue;
                }

                // Narrow Phase:
                // perfom SAT check.
                if(SAT.Intersect(
                    rectangle,
                    circle,
                    out Vector2 normal,
                    out float depth
                ))
                {
                    // add the collision to the collisions manifold for later resolution.
                    state.CollisionManifold.Add(
                        new Collision(
                            rectangleGenIndex,
                            circleGenIndex,
                            rectangleCollider.Parameters,
                            circleCollider.Parameters,
                            normal, 
                            depth
                        )
                    );                
                }
            }
        }
    }    
}