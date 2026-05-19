using Howl.DataStructures;
using Howl.Ecs;
using Howl.Math;

namespace Howl.Allocators;

public class TransformAllocator
{
    public IntrusiveList.State TransformsHierarchy;
    public ComponentArray<Transform> GlobalTransforms;
    public ComponentArray<Transform> LocalTransforms;
    
    public TransformAllocator(int length)
    {
        TransformsHierarchy = new(length);
        GlobalTransforms = new(length);
        LocalTransforms = new(length);
    }

    public static bool Allocate(TransformAllocator allocator, Transform transform, GenId genId)
    {
        bool success = false;
        success = ComponentArray.Allocate(allocator.GlobalTransforms, genId, transform);
        if (success == false)
        {
            return false;
        }

        success = ComponentArray.Allocate(allocator.LocalTransforms, genId, transform);
        if (success == false)
        {
            return false;
        }

        IntrusiveList.AddToTree(allocator.TransformsHierarchy, GenId.GetIndex(genId));
        
        return true;
    }

    public static bool Allocate(TransformAllocator allocator, Transform transform, GenId genId, GenId parentId)
    {
        bool success = false;
        
        success = ComponentArray.Allocate(allocator.GlobalTransforms, genId, transform);
        if (success == false)
        {
            return false;
        }

        success = ComponentArray.Allocate(allocator.LocalTransforms, genId, transform);
        if (success == false)
        {
            return false;
        }

        int entityIndex = GenId.GetIndex(genId);
        int parentIndex = GenId.GetIndex(parentId);

        IntrusiveList.AddToTree(allocator.TransformsHierarchy, entityIndex, parentIndex);
        
        // set the global transform.

        allocator.GlobalTransforms.Sparse[entityIndex] = Transform.Combine(
            allocator.LocalTransforms.Sparse[entityIndex], allocator.GlobalTransforms.Sparse[parentIndex]
        ); 

        return true;
    }
}