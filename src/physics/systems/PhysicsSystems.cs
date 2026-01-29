using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;

namespace Howl.Physics.Systems;

public static class PhysicsSystems
{
    public readonly static Stopwatch IntersectStep = new Stopwatch();
    public readonly static Stopwatch ResolutionStep = new Stopwatch();

    private static List<Collision> CollisionManifold = new(4096);

    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.RegisterComponent<CircleCollider>();
        componentRegistry.RegisterComponent<RectangleCollider>();
        componentRegistry.RegisterComponent<RigidBody>();
    }

    public static void RegisterSystems(SystemRegistry systemRegistry, ComponentRegistry componentRegistry)
    {
        systemRegistry.RegisterUpdateSystem(UpdateSystem(componentRegistry));
        systemRegistry.RegisterFixedUpdateSystem(FixedUpdateSystem(componentRegistry));
    }

    public static UpdateSystem UpdateSystem(ComponentRegistry componentRegistry)
    {
        return dt =>
        {
        };
    }

    public static FixedUpdateSystem FixedUpdateSystem(ComponentRegistry componentRegistry)
    {
        return dt =>
        {  
            if(CollisionManifold.Count > 0)
            {
                CollisionManifold.Clear();
            }
            
            IntersectStep.Restart();
            FindCircleCollisions(componentRegistry);
            FindRectangleCollisions(componentRegistry);
            IntersectStep.Stop();

            ResolutionStep.Restart();
            ResolveCollisions(componentRegistry);
            ResolutionStep.Stop();    
        };
    }

    private static void FindCircleCollisions(ComponentRegistry componentRegistry)
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
                if(Util.CirclesIntersect(
                    Circle.Transform(colliderA.Shape, transformA),
                    Circle.Transform(colliderB.Shape, transformB),
                    out Vector2 normal,
                    out float depth
                ))
                {

                    // add the collision to the collisions manifold for later resolution.
                    CollisionManifold.Add(
                        new Collision(
                            genIndexA, 
                            genIndexB, 
                            normal, 
                            depth
                        )
                    );
                    
                }
            }
        }
    }

    private static void FindRectangleCollisions(ComponentRegistry componentRegistry)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<RectangleCollider> colliders = componentRegistry.Get<RectangleCollider>();
        Span<DenseEntry<RectangleCollider>> denseEntries = colliders.GetDenseAsSpan();

        for(int i = 0; i < denseEntries.Length - 1; i++)
        {

            // get the current collider.
            ref DenseEntry<RectangleCollider> denseEntryA = ref denseEntries[i];
            ref RectangleCollider rectangleA = ref denseEntryA.Value;
            colliders.GetGenIndex(denseEntryA.sparseIndex, out GenIndex genIndexA);
            
            // make sure the collider has a transform component.
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

                // get the other collider to check intersection against.
                ref DenseEntry<RectangleCollider> denseEntryB = ref denseEntries[j];
                ref RectangleCollider rectangleB = ref denseEntryB.Value;
                colliders.GetGenIndex(denseEntryB.sparseIndex, out GenIndex genIndexB);                
                
                // make sure this collider has a transform component.
                switch(transforms.GetDenseRef(genIndexB, out Ref<Transform> transformRefB))
                {
                    case GenIndexResult.DenseNotAllocated:
                        throw new DenseNotAllocatedException(genIndexA);
                    case GenIndexResult.StaleGenIndex:
                        continue;
                }
                ref Transform transformB = ref transformRefB.Value;

                // check if the two circles intersect.
                if(Util.RectanglesIntersect(
                    PolygonRectangle.Transform(rectangleA.Shape,transformA),
                    PolygonRectangle.Transform(rectangleB.Shape,transformB),
                    out Vector2 normal,
                    out float depth
                ))
                {
                    // add the collision to the collisions manifold for later resolution.
                    CollisionManifold.Add(
                        new Collision(
                            genIndexA, 
                            genIndexB, 
                            normal, 
                            depth
                        )
                    );                
                }
            }
        }        
    }

    private static void ResolveCollisions(ComponentRegistry componentRegistry)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        Span<Collision> span = CollectionsMarshal.AsSpan(CollisionManifold);

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

            Vector2 displacement = collision.Normal * collision.Depth * 0.5f;

            transformRefA.Value.Position += -displacement;
            transformRefB.Value.Position += displacement;            
        }
    }

    // private static void SyncCircleRigidBodyTransforms(ComponentRegistry componentRegistry)
    // {
    //     GenIndexList<CircleRigidBody> rigidBodyComponents = componentRegistry.Get<CircleRigidBody>();
    //     Span<DenseEntry<CircleRigidBody>> denseEntries = rigidBodyComponents.GetDenseAsSpan();

    //     GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();

    //     for(int i = 0; i < denseEntries.Length; i++)
    //     {
    //         ref DenseEntry<CircleRigidBody> denseEntry = ref denseEntries[i];
    //         ref CircleRigidBody rigidBody = ref denseEntry.Value;
    //         rigidBodyComponents.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

    //         if(transformComponents.GetDenseRef(genIndex, out Ref<Transform> transformRef) == GenIndexResult.Success)
    //         {
    //             // sync the position.

    //             transformRef.Value.Position = rigidBody.RigidBody.Position;
    //         }
    //     } 
    // }
}