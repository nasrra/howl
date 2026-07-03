using Howl.Text;
using N_Howl.N_DataStructures;
using Howl.Unmanaged.Collections;
using N_Howl.N_Math;

public static class TransformHierarchy{

public static void UpdateChildren(
    Array<IntrusiveListNode> hierarchy, Array<Transform> globalTransforms, Array<Transform> localTransforms, int nodeIndex
){
    ref IntrusiveListNode node = ref hierarchy[nodeIndex];

    int firstChildIndex = node.FirstChild;
        
    if(firstChildIndex != 0)
    {
        UpdateNodeRecursive(globalTransforms, localTransforms, nodeIndex, firstChildIndex, firstChildIndex);
    }

    void UpdateNodeRecursive(
        Array<Transform> globalTransforms, Array<Transform> localTransforms, int parentIndex, int nodeIndex, int parentFirstChildIndex
    ){
        // transform the child.
        ref Transform parentGlobalTransform = ref globalTransforms[parentIndex];
        ref Transform localTransform = ref localTransforms[nodeIndex];
        globalTransforms[nodeIndex] = Math.TransformRelative(parentGlobalTransform, localTransform);

        ref IntrusiveListNode node = ref hierarchy[nodeIndex];
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

public static void UpdateChildren(
    Array<IntrusiveListNode> hierarchy, ComponentArray<Transform> globalTransforms, ComponentArray<Transform> localTransforms, int nodeIndex
){
    ref IntrusiveListNode node = ref hierarchy[nodeIndex];

    int firstChildIndex = node.FirstChild;
        
    if(firstChildIndex != 0)
    {
        UpdateNodeRecursive(globalTransforms, localTransforms, nodeIndex, firstChildIndex, firstChildIndex);
    }

    void UpdateNodeRecursive(
        ComponentArray<Transform> globalTransforms, ComponentArray<Transform> localTransforms, int parentIndex, int nodeIndex, int parentFirstChildIndex
    ){
        // transform the child.
        ref Transform parentGlobalTransform = ref globalTransforms.Sparse[parentIndex];
        ref Transform localTransform = ref localTransforms.Sparse[nodeIndex];
        globalTransforms.Sparse[nodeIndex] = Math.TransformRelative(localTransform, parentGlobalTransform);

        ref IntrusiveListNode node = ref hierarchy[nodeIndex];
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

public static void UpdateChildrenPositions(
    Array<IntrusiveListNode> hierarchy, ComponentArray<Transform> globalTransforms, ComponentArray<Transform> localTransforms, int nodeIndex
){
    ref IntrusiveListNode node = ref hierarchy[nodeIndex];

    int firstChildIndex = node.FirstChild;
        
    if(firstChildIndex != 0)
    {
        UpdateNodeRecursive(globalTransforms, localTransforms, nodeIndex, firstChildIndex, firstChildIndex);
    }

    void UpdateNodeRecursive(
        ComponentArray<Transform> globalTransforms, ComponentArray<Transform> localTransforms, int parentIndex, int nodeIndex, int parentFirstChildIndex
    ){
        // transform the child.
        ref Transform parentGlobalTransform = ref globalTransforms.Sparse[parentIndex];
        ref Transform localTransform = ref localTransforms.Sparse[nodeIndex];
        globalTransforms.Sparse[nodeIndex].Position = localTransform.Position + parentGlobalTransform.Position;

        ref IntrusiveListNode node = ref hierarchy[nodeIndex];
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


}