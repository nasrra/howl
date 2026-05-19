using System.Runtime.CompilerServices;
using Howl.Allocators;
using Howl.DataStructures;
using Howl.Ecs;
using Howl.Math;
using Howl.Systems;

namespace Howl.Physics;

public static class Syncer
{
    /// <summary>
    ///     Syncs an SoaTransform collection to entities that contain both a transform component and a physics body id component. 
    /// </summary>
    public static void SyncStateToTransforms(TransformAllocator tranforms, ComponentArray<PhysicsBodyComponent> physicsBodyTags, PhysicsSystemState state
    )
    {        
        // hoisting invariance.
        Soa_Transform physicsTransforms = state.Transforms;
        int[] physicsGenerations = state.Generations;
        ComponentArray<Transform> entityTransforms = tranforms.GlobalTransforms;

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
            ref Transform transform = ref ComponentArray.GetDataUnsafe(entityTransforms, entityGenId);
            Soa_Transform.Insert(physicsTransforms, physicsBodyIndex, transform);
        }
    }

    /// <summary>
    ///     Syncs a entities that contain both a transform and physics body id component to an soa transform collection.
    /// </summary>
    public static void SyncTransformsToState(TransformAllocator tranforms, ComponentArray<PhysicsBodyComponent> physicsBodyTags, PhysicsSystemState state
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
                Debug.LogError("physics body tag has stale gen id, physics body may have not been deallocated when entity was deallocated.");
                continue;
            }

            int index = GenId.GetIndex(entityGenId);
            ref IntrusiveList.Node node = ref tranforms.TransformsHierarchy.Nodes[index];
            if(node.InTree == false)
            {
                Debug.LogError("physics body entity transform has not been inserted into transform hierarchy.");
                continue;                
            }
            
            int physicsBodyIndex = GenId.GetIndex(tag.PhysicsBodyGenId);
            float x = physicsTransforms.Positions.X[physicsBodyIndex];
            float y = physicsTransforms.Positions.Y[physicsBodyIndex];
            float prevX = state.PreviousStepPositions.X[physicsBodyIndex];
            float prevY = state.PreviousStepPositions.Y[physicsBodyIndex];
            Vector2 physicsBodyPosition = new Vector2(x,y);

            if(x==prevX && y == prevY)
            {
                continue;
            }

            // warp the global position if the transform is a root node.             
            GlobalTransform.Warp(tranforms, physicsBodyPosition, entityGenId);
        }
    }
}