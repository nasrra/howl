using System;
using Howl.Debug;
using Howl.Ecs;
using Howl.Generic;
using Howl.Math;

namespace Howl.Graphics;

public static class CameraSystem
{
    /// <summary>
    ///     Updates all camera's projection matrices with a renderer states output resolution aspect ratio.
    /// </summary>
    /// <param name="cameras">the cameras to update.</param>
    /// <param name="outputResolutionAspectRatio">the output resolution aspect ratio.</param>
    public static void UpdateProjectionMatrices(ComponentArray<Camera> cameras, float outputResolutionAspectRatio)
    {
        for(int i = 1; i < cameras.Active.Count; i++)
        {
            ref Camera camera = ref ComponentArray.GetDataUnsafe(cameras, cameras.Active[i]);
            camera.UpdateProjectionMatrix(outputResolutionAspectRatio);
        }
    }

    public static ref Camera GetDrawSpaceCamera(HowlAppState state, DrawSpace drawSpace)
    {
        switch (drawSpace)
        {
            case DrawSpace.World:
                return ref state.WorldCamera;
            case DrawSpace.Screen:
                return ref state.ScreenCamera;
            default:
                throw new Exception("unrecognised camera draw space");
        }
    }
}