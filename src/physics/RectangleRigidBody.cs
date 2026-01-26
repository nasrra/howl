// using Howl.Math;

// namespace Howl.Physics;

// public struct RectangleRigidBody
// {
//     public readonly RigidBody RigidBody;
    
//     public float Width;

//     public float Height;

//     /// <summary>
//     /// Contructs a RectangleRigdyBody.
//     /// </summary>
//     /// <param name="position"></param>
//     /// <param name="rotation"></param>
//     /// <param name="density"></param>
//     /// <param name="restitution"></param>
//     /// <param name="width"></param>
//     /// <param name="height"></param>
//     public RectangleRigidBody(
//         Transform transform,
//         float density,
//         float restitution,
//         float width,
//         float height
//     )
//     {
//         float area = width * height;

//         Width = width;
//         Height = height;
        
//         RigidBody = new RigidBody(
//             transform,
//             area,
//             density,
//             restitution
//         );    
//     }
// }