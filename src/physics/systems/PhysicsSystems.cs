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
    private static List<Collision> CollisionManifold = new(4096);

    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.RegisterComponent<CircleCollider>();
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
            // CollisionManifold.Clear();
            // FindCircleCollisions(componentRegistry);
            // ResolveCircleCollisions(componentRegistry);
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
            FindCircleCollisions(componentRegistry);
            ResolveCircleCollisions(componentRegistry);
        };
    }

    private static void FindCircleCollisions(ComponentRegistry componentRegistry)
    {
        GenIndexList<Transform> transforms = componentRegistry.Get<Transform>();
        GenIndexList<CircleCollider> circleColliders = componentRegistry.Get<CircleCollider>();
        Span<DenseEntry<CircleCollider>> denseEntries = circleColliders.GetDenseAsSpan();

        for(int i = 0; i < denseEntries.Length - 1; i++)
        {
            ref DenseEntry<CircleCollider> denseEntryA = ref denseEntries[i];
            ref CircleCollider circleA = ref denseEntryA.Value;
            circleColliders.GetGenIndex(denseEntryA.sparseIndex, out GenIndex genIndexA);
            
            switch(transforms.GetDenseRef(genIndexA, out Ref<Transform> transformRefA))
            {
                case GenIndexResult.DenseNotAllocated:
                    throw new DenseNotAllocatedException(genIndexA);
                case GenIndexResult.StaleGenIndex:
                    continue;
            }
                        
            ref Transform tranformA = ref transformRefA.Value;

            for(int j = i + 1; j < denseEntries.Length; j++)
            {

                ref DenseEntry<CircleCollider> denseEntryB = ref denseEntries[j];
                ref CircleCollider circleB = ref denseEntryB.Value;
                circleColliders.GetGenIndex(denseEntryB.sparseIndex, out GenIndex genIndexB);                
                switch(transforms.GetDenseRef(genIndexB, out Ref<Transform> transformRefB))
                {
                    case GenIndexResult.DenseNotAllocated:
                        throw new DenseNotAllocatedException(genIndexA);
                    case GenIndexResult.StaleGenIndex:
                        continue;
                }
        
                ref Transform transformB = ref transformRefB.Value;

                if(Util.CirclesIntersect(
                    circleA.Shape,
                    circleB.Shape,
                    tranformA.Position,
                    transformB.Position,
                    out Vector2 normal,
                    out float depth
                ))
                {
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

    private static void ResolveCircleCollisions(ComponentRegistry componentRegistry)
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