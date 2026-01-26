using System;
using System.Diagnostics;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;

namespace Howl.Physics.Systems;

public static class PhysicsSystems
{
    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.RegisterComponent<CircleRigidBody>();
        // componentRegistry.RegisterComponent<RectangleRigidBody>();
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
            SyncCircleRigidBodyTransforms(componentRegistry);            
        };
    }

    public static FixedUpdateSystem FixedUpdateSystem(ComponentRegistry componentRegistry)
    {
        return dt =>
        {  
            ResolveCircleCollisions(componentRegistry);
        };
    }

    private static void ResolveCircleCollisions(ComponentRegistry componentRegistry)
    {
        GenIndexList<CircleRigidBody> rigidBodyComponents = componentRegistry.Get<CircleRigidBody>();
        Span<DenseEntry<CircleRigidBody>> denseEntries = rigidBodyComponents.GetDenseAsSpan();

        for(int i = 0; i < denseEntries.Length - 1; i++)
        {
            ref DenseEntry<CircleRigidBody> denseEntryA = ref denseEntries[i];
            ref CircleRigidBody rigidBodyA = ref denseEntryA.Value;
            rigidBodyComponents.GetGenIndex(denseEntryA.sparseIndex, out GenIndex genIndexA);
            
            for(int j = i + 1; j < denseEntries.Length; j++)
            {

                ref DenseEntry<CircleRigidBody> denseEntryB = ref denseEntries[j];
                ref CircleRigidBody rigidBodyB = ref denseEntryB.Value;
                rigidBodyComponents.GetGenIndex(denseEntryB.sparseIndex, out GenIndex genIndexB);

                if(Collisions.CirclesIntersect(
                    rigidBodyA.RigidBody.Position,
                    rigidBodyB.RigidBody.Position,
                    rigidBodyA.RadiusSquared,
                    rigidBodyA.Radius,
                    rigidBodyB.RadiusSquared,
                    rigidBodyB.Radius,
                    out Vector2 normal,
                    out float depth
                ))
                {
                    // separate them if there is a collision.

                    rigidBodyA.RigidBody.Move(-normal * depth * 0.5f);
                    rigidBodyB.RigidBody.Move(normal * depth * 0.5f);
                }
            }
        }
    }

    private static void SyncCircleRigidBodyTransforms(ComponentRegistry componentRegistry)
    {
        GenIndexList<CircleRigidBody> rigidBodyComponents = componentRegistry.Get<CircleRigidBody>();
        Span<DenseEntry<CircleRigidBody>> denseEntries = rigidBodyComponents.GetDenseAsSpan();

        GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();

        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<CircleRigidBody> denseEntry = ref denseEntries[i];
            ref CircleRigidBody rigidBody = ref denseEntry.Value;
            rigidBodyComponents.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

            if(transformComponents.GetDenseRef(genIndex, out Ref<Transform> transformRef) == GenIndexResult.Success)
            {
                // sync the position.

                transformRef.Value.Position = rigidBody.RigidBody.Position;
            }
        } 
    }
}