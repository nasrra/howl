using System.ComponentModel.DataAnnotations;
using Howl.Allocators;
using Howl.DataStructures;
using Howl.Ecs;
using Howl.Math;

namespace Howl.Systems;

public static class GlobalTransform
{
    public static bool Translate(TransformAllocator allocator, Vector2 position, GenId genId)
    {
        bool isValid = false;
        
        ComponentArray<Transform> globalTransforms = allocator.GlobalTransforms;
        ComponentArray<Transform> localTransforms = allocator.LocalTransforms;
        int index = GenId.GetIndex(genId);
        ref IntrusiveList.Node node = ref allocator.TransformsHierarchy.Nodes[index]; 
        
        {   // attempt the translation.

            ref Transform globalTransform = ref ComponentArray.GetData(globalTransforms, genId, ref isValid);

            if (isValid == false)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            ref Transform localTransform = ref ComponentArray.GetData(localTransforms, genId, ref isValid);

            if(isValid == false)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            if(node.Parent == 0)
            {
                // snap to position.
                globalTransform.Position += position;
                localTransform = globalTransform;
            }
            else
            {
                // set the local position relative to parent.
                globalTransform.Position += position;
                localTransform.Position += position;
            }
        }

        TransformHierarchy.UpdateChildren(allocator.TransformsHierarchy, allocator.GlobalTransforms.Sparse, allocator.LocalTransforms.Sparse, 
            index
        );

        return true; 
    }

    public static bool Warp(TransformAllocator allocator, Vector2 position, GenId genId)
    {
        bool isValid = false;
        
        ComponentArray<Transform> globalTransforms = allocator.GlobalTransforms;
        ComponentArray<Transform> localTransforms = allocator.LocalTransforms;

        int index = GenId.GetIndex(genId);
        ref IntrusiveList.Node node = ref allocator.TransformsHierarchy.Nodes[index]; 
        
        
        {   // attempt the warp.

            ref Transform globalTransform = ref ComponentArray.GetData(globalTransforms, genId, ref isValid);

            if (isValid == false)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            ref Transform localTransform = ref ComponentArray.GetData(localTransforms, genId, ref isValid);

            if(isValid == false)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            if(node.Parent == 0)
            {
                // snap to position.
                globalTransform.Position = position;
                localTransform = globalTransform;
            }
            else
            {
                // set the local position relative to parent.
                Vector2 difference = globalTransform.Position - position;
                globalTransform.Position = position;
                localTransform.Position += difference;
            }
        }

        TransformHierarchy.UpdateChildren(allocator.TransformsHierarchy, allocator.GlobalTransforms.Sparse, allocator.LocalTransforms.Sparse, 
            index
        );

        return true; 
    }
}