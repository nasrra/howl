using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public static class CollisionSystem
{
    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.RegisterComponent<CircleCollider>();
        componentRegistry.RegisterComponent<RectangleCollider>();
        componentRegistry.RegisterComponent<RigidBody>();
    }

    public static FixedUpdateSystem FixedUpdateSystem(ComponentRegistry componentRegistry, CollisionSystemState state)
    => deltaTime => 
    {
        if (state.IsDisposed)
        {
            throw new ObjectDisposedException("Collision System cannot operate on/with a disposed Collision System State");
        }

        if(state.CollisionManifold.Count > 0)
        {
            state.CollisionManifold.Clear();
        }
        
        state.IntersectStep.Restart();
        FindCircleCollisions(componentRegistry, state);
        FindRectangleCollisions(componentRegistry, state);
        FindRectangleToCircleCollisions(componentRegistry, state);
        state.IntersectStep.Stop();

        state.ResolutionStep.Restart();
        ResolveCollisions(componentRegistry, state);
        state.ResolutionStep.Stop();            
    };

    public static DrawSystem DrawSystem(ComponentRegistry componentRegistry, IRenderer renderer, CollisionSystemState state)
    => deltaTime =>
    {
        if (state.IsDisposed)
        {
            throw new ObjectDisposedException("Collision System cannot operate on/with a disposed Collision System State");
        }

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

    };

    /// <summary>
    /// Finds all intersecting circles in the component registry and adds it to the collision systems collision manifold.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <exception cref="DenseNotAllocatedException"></exception>
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

                // check if the two circles intersect.
                if(SAT.Intersect(
                    Circle.Transform(colliderA.Shape, transformA),
                    Circle.Transform(colliderB.Shape, transformB),
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

                // check if the two circles intersect.
                if(SAT.Intersect(
                    PolygonRectangle.Transform(colliderA.Shape,transformRefA.Value),
                    PolygonRectangle.Transform(colliderB.Shape,transformRefB.Value),
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
            ref RectangleCollider rectangle = ref rectangleDenseEntry.Value;
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
                ref CircleCollider circle = ref circleDenseEntry.Value;
                circleColliders.GetGenIndex(circleDenseEntry.sparseIndex, out GenIndex circleGenIndex);

                // make sure the collider has a transform component.
                switch(transforms.GetDenseRef(circleGenIndex, out Ref<Transform> circleTransformRef))
                {
                    case GenIndexResult.DenseNotAllocated:
                        throw new DenseNotAllocatedException(circleGenIndex);
                    case GenIndexResult.StaleGenIndex:
                        throw new StaleGenIndexException(circleGenIndex);
                }

                // check if the two circles intersect.
                if(SAT.Intersect(
                    PolygonRectangle.Transform(rectangle.Shape,rectangleTransformRef.Value),
                    Circle.Transform(circle.Shape,circleTransformRef.Value),
                    out Vector2 normal,
                    out float depth
                ))
                {
                    // add the collision to the collisions manifold for later resolution.
                    state.CollisionManifold.Add(
                        new Collision(
                            rectangleGenIndex,
                            circleGenIndex,
                            rectangle.Parameters,
                            circle.Parameters,
                            normal, 
                            depth
                        )
                    );                
                }
            }
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
                Vector2 displacement = collision.Normal * collision.Depth * 0.5f;
                transformRefB.Value.Position += displacement;
            }
            else if(collision.ColliderBParameters.Mode == ColliderMode.Kinematic)
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
}