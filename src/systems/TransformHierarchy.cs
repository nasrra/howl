using Howl.DataStructures;
using Howl.Math;

public static class TransformHierarchy
{
    public static void UpdateChildren(IntrusiveList.Node[] hierarchy, Transform[] globalTransforms, Transform[] localTransforms, int nodeIndex)
    {
        ref IntrusiveList.Node node = ref hierarchy[nodeIndex];

        int firstChildIndex = node.FirstChild;
            
        if(firstChildIndex != 0)
        {
            UpdateNodeRecursive(globalTransforms, localTransforms, nodeIndex, firstChildIndex, firstChildIndex);
        }

        void UpdateNodeRecursive(Transform[] globalTransforms, Transform[] localTransforms, int parentIndex, int nodeIndex, int parentFirstChildIndex)
        {
            // transform the child.
            ref Transform parentGlobalTransform = ref globalTransforms[parentIndex];
            ref Transform localTransform = ref localTransforms[nodeIndex];
            globalTransforms[nodeIndex] = Transform.TransformRelative(parentGlobalTransform, localTransform);

            ref IntrusiveList.Node node = ref hierarchy[nodeIndex];
            int firstChildIndex = node.FirstChild;
            if(node.FirstChild != 0)
            {
                UpdateNodeRecursive(globalTransforms, localTransforms, nodeIndex, firstChildIndex, firstChildIndex);
            }

            int nextIndex = node.NextSibling;
            if(nextIndex == parentFirstChildIndex)
            {
                return;
            }
            else
            {
                UpdateNodeRecursive(globalTransforms, localTransforms, parentIndex, nextIndex, parentFirstChildIndex);
            }
        }
    }


    // public static void UpdateChildren(IntrusiveList.Node[] hierarchy, Transform[] globalTransforms, Transform[] localTransforms, int nodeIndex)
    // {
    //     ref IntrusiveList.Node node = ref hierarchy[nodeIndex];

    //     int firstChildIndex = node.FirstChild;
            
    //     if(firstChildIndex != 0)
    //     {
    //         UpdateNodeRecursive(globalTransforms, localTransforms, nodeIndex, firstChildIndex, firstChildIndex);
    //     }

    //     void UpdateNodeRecursive(Transform[] globalTransforms, Transform[] localTransforms, int parentIndex, int nodeIndex, int parentFirstChildIndex)
    //     {
    //         // transform the child.
    //         ref Transform parentGlobalTransform = ref globalTransforms[parentIndex];
    //         ref Transform localTransform = ref localTransforms[nodeIndex];
    //         globalTransforms[nodeIndex] = Transform.Combine(parentGlobalTransform, localTransform);

    //         ref IntrusiveList.Node node = ref hierarchy[nodeIndex];
    //         int firstChildIndex = node.FirstChild;
    //         if(node.FirstChild != 0)
    //         {
    //             UpdateNodeRecursive(globalTransforms, localTransforms, nodeIndex, firstChildIndex, firstChildIndex);
    //         }

    //         int nextIndex = node.NextSibling;
    //         if(nextIndex == parentFirstChildIndex)
    //         {
    //             return;
    //         }
    //         else
    //         {
    //             UpdateNodeRecursive(globalTransforms, localTransforms, parentIndex, nextIndex, parentFirstChildIndex);
    //         }
    //     }
    // }
}