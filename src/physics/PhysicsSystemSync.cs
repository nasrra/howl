using Howl.Ecs;
using Howl.Math;

namespace Howl.Physics;

public static class Syncer
{
    /// <summary>
    ///     Syncs an SoaTransform collection to entities that contain both a transform component and a physics body id component. 
    /// </summary>
    public static void SyncStateToTransforms(ComponentArray<PhysicsBodyComponent> physicsBodyTags, 
        ComponentArray<Transform> transforms, PhysicsSystemState state
    )
    {        
        // hoisting invariance.
        Soa_Transform physicsTransforms = state.Transforms;
        int[] physicsGenerations = state.Generations;

        for(int i = 1; i < physicsBodyTags.Active.Count; i++) // skip Nil.
        {
            GenId entityGenId = physicsBodyTags.Active[i];
            ref PhysicsBodyComponent tag = ref ComponentArray.GetDataUnsafe(physicsBodyTags, entityGenId);            

            int physicsBodyIndex = GenId.GetIndex(tag.PhysicsBodyGenId);

            // skip if the physics body id isn't valid.
            if(physicsGenerations[physicsBodyIndex] != GenId.GetGeneration(tag.PhysicsBodyGenId))
            {
                Debug.LogError("physics body tag has stale gen id, physics body may have not been deallocated when entity was deallocated.");
                continue;
            }
            
            // sync the transform data to the physics simulation 
            // if it has an associated physics body id.
            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, entityGenId);
            Soa_Transform.Insert(physicsTransforms, physicsBodyIndex, transform);
        }
    }

    /// <summary>
    ///     Syncs a entities that contain both a transform and physics body id component to an soa transform collection.
    /// </summary>
    public static void SyncTransformsToState(ComponentArray<PhysicsBodyComponent> physicsBodyTags, 
        ComponentArray<Transform> transforms, PhysicsSystemState state
    )
    {
        // hoisting invariance.
        Soa_Transform physicsTransforms = state.Transforms;
        int[] physicsGenerations = state.Generations;

        for(int i = 1; i < physicsBodyTags.Active.Count; i++)
        {
            GenId entityGenId = physicsBodyTags.Active[i];
            ref PhysicsBodyComponent tag = ref ComponentArray.GetDataUnsafe(physicsBodyTags, entityGenId);

            // skip the tag if it is stale.
            if(physicsGenerations[GenId.GetIndex(tag.PhysicsBodyGenId)] != GenId.GetGeneration(tag.PhysicsBodyGenId))
            {
                continue;
            }

            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, entityGenId);
            Soa_Transform.CopySoaToTransform(physicsTransforms, ref transform, GenId.GetIndex(tag.PhysicsBodyGenId));
        }
    }
}