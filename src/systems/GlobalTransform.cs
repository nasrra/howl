// using System.ComponentModel.DataAnnotations;
// using Howl.Allocators;
// using Howl.DataStructures;
// using Howl.Ecs;
// using Howl.Math;

// namespace Howl.Systems;

// public static class GlobalTransform
// {
//     public static bool Translate(TransformAllocator allocator, Vector2 displacement, GenId transformId)
//     {
//         bool isValid = false;
        
//         ComponentArray<Transform> globalTransforms = allocator.GlobalTransforms;
//         ComponentArray<Transform> localTransforms = allocator.LocalTransforms;
//         int index = GenId.GetIndex(transformId);
        
//         {   // attempt the translation.

//             ComponentArray.GetData(globalTransforms, transformId, ref isValid);
//             if (globalTransforms.Allocated[index]!=true)
//             {
//                 System.Diagnostics.Debug.Assert(false);
//                 return false;
//             }

//             if(localTransforms.Allocated[index]!=true)
//             {
//                 System.Diagnostics.Debug.Assert(false);
//                 return false;
//             }
//         }

//         return Translate(allocator.TransformsHierarchy.Nodes, globalTransforms.Sparse, localTransforms.Sparse, 
//             displacement.X, displacement.Y, index
//         );
//     }
    
//     public static bool Translate(IntrusiveList.Node[] nodes, Transform[] globalTransforms, Transform[] localTransforms, 
//         float displacementX, float displacementY, int transformNodeIndex
//     )
//     {
//         ref IntrusiveList.Node node = ref nodes[transformNodeIndex]; 
        
//         if(node.InTree == false)
//         {
//             System.Diagnostics.Debug.Assert(false);
//             return false;
//         }

//         {
//             ref Transform globalTransform = ref globalTransforms[transformNodeIndex];
//             ref Transform localTransform = ref localTransforms[transformNodeIndex];

//             if(node.Parent == 0)
//             {
//                 // snap to position.
//                 globalTransform.Position.X += displacementX;
//                 globalTransform.Position.Y += displacementY;
//                 localTransform = globalTransform;
//             }
//             else
//             {
//                 // set the local position relative to parent.
//                 globalTransform.Position.X += displacementX;
//                 globalTransform.Position.Y += displacementY;
//                 localTransform.Position.X += displacementX;
//                 localTransform.Position.Y += displacementY;
//             }
//         }

//         TransformHierarchy.UpdateChildren(nodes, globalTransforms, localTransforms, 
//             transformNodeIndex
//         );

//         return true; 
//     }

//     public static bool Warp(TransformAllocator allocator, Vector2 position, GenId transformId)
//     {
//         bool isValid = false;
        
//         ComponentArray<Transform> globalTransforms = allocator.GlobalTransforms;
//         ComponentArray<Transform> localTransforms = allocator.LocalTransforms;

//         int index = GenId.GetIndex(transformId);
//         ref IntrusiveList.Node node = ref allocator.TransformsHierarchy.Nodes[index]; 
        
        
//         {   // attempt the warp.

//             ref Transform globalTransform = ref ComponentArray.GetData(globalTransforms, transformId, ref isValid);

//             if (isValid == false)
//             {
//                 System.Diagnostics.Debug.Assert(false);
//                 return false;
//             }

//             ref Transform localTransform = ref ComponentArray.GetData(localTransforms, transformId, ref isValid);

//             if(isValid == false)
//             {
//                 System.Diagnostics.Debug.Assert(false);
//                 return false;
//             }

//             if(node.Parent == 0)
//             {
//                 // snap to position.
//                 globalTransform.Position = position;
//                 localTransform = globalTransform;
//             }
//             else
//             {
//                 // set the local position relative to parent.
//                 Vector2 difference = globalTransform.Position - position;
//                 globalTransform.Position = position;
//                 localTransform.Position += difference;
//             }
//         }

//         TransformHierarchy.UpdateChildren(allocator.TransformsHierarchy.Nodes, allocator.GlobalTransforms.Sparse, allocator.LocalTransforms.Sparse, 
//             index
//         );

//         return true; 
//     }
// }