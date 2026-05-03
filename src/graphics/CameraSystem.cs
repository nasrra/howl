using System;
using Howl.Debug;
using Howl.Ecs;
using Howl.Generic;
using Howl.Math;

namespace Howl.Graphics;

public static class CameraSystem
{

    /// <summary>
    /// Registers all necessary components for this system into the specified component registry. 
    /// </summary>
    /// <param name="componentRegistry">the component registry.</param>
    public static void RegisterComponents(ComponentRegistry registry)
    {
        ComponentRegistry.RegisterComponent<Camera>(registry);
    }

    /// <summary>
    /// Creates a new update system instance.
    /// </summary>
    /// <param name="ecs">the ecs state containing the cameras to update.</param>
    /// <param name="state">the renderer state.</param>
    /// <returns>the new update system instance.</returns>
    public static void Update(EcsState ecs, float outputResolutionAspectRatio)
    {
        UpdateProjectionMatrices(ecs, outputResolutionAspectRatio);  
    }
    
    /// <summary>
    /// Updates all camera's projection matrices with a renderer states output resolution aspect ratio.
    /// </summary>
    /// <param name="ecs">The ecs state with the camera data.</param>
    /// <param name="state">the renderer state to update in accordance with.</param>
    private static void UpdateProjectionMatrices(EcsState ecs, float outputResolutionAspectRatio)
    {
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
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