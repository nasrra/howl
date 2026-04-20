using System;
using Howl.Debug;
using Howl.Ecs;
using Howl.Generic;
using Howl.Math;
using static Howl.Ecs.GenIndexListProc;

namespace Howl.Graphics;

public static class CameraSystem
{
    /// <summary>
    /// Gets and sets the main camera.
    /// </summary>
    private static Camera mainCamera;

    /// <summary>
    /// Gets a copy of the main camera data.
    /// </summary>
    public static ref readonly Camera MainCamera => ref mainCamera;

    /// <summary>
    /// Gets and sets the gui camera.
    /// </summary>
    private static Camera guiCamera;

    /// <summary>
    /// Gets a copy of the gui camera data.
    /// </summary>
    public static ref readonly Camera GuiCamera => ref guiCamera;
    
    /// <summary>
    /// Gets and sets the main camera id.
    /// </summary>
    public static GenId MainCameraId;

    /// <summary>
    /// Gets and sets the gui camera id.
    /// </summary>
    public static GenId GuiCameraId;

    /// <summary>
    /// Registers all necessary components for this system into the specified component registry. 
    /// </summary>
    /// <param name="componentRegistry">the component registry.</param>
    public static void RegisterComponents(ComponentRegistryNew registry)
    {
        ComponentRegistryNew.RegisterComponent<Camera>(registry);
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
        
        // order matters here:
        // updating the main camera's copies the data of the stored camera
        // in the component registry; make sure to update the projection matrices 
        // before  copying.
        UpdateMainCamera(ecs);
        UpdateGuiCamera(ecs);
    }

    /// <summary>
    /// Updates the cached main camera.
    /// </summary>
    /// <param name="ecs">The ecs state that stores the camera.</param>
    private static void UpdateMainCamera(EcsState ecs)
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
        ref Camera camera = ref ComponentArray.GetData(cameras, ecs, MainCameraId, ref result);
        
        if(result != GenIdResult.Ok)
        {
            // there must always be a main camera.
            System.Diagnostics.Debug.Assert(false);
            return;            
        }

        // copy camera data.
        mainCamera = camera;
    }

    /// <summary>
    /// Updates the cached gui camera.
    /// </summary>
    /// <param name="componentRegistry">The ecs state that stores the camera.</param>
    private static void UpdateGuiCamera(EcsState ecs)
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
        ref Camera camera = ref ComponentArray.GetData(cameras, ecs, MainCameraId, ref result);
        
        if(result != GenIdResult.Ok)
        {
            // there must always be a main camera.
            System.Diagnostics.Debug.Assert(false);
            return;            
        }
        
        // copy camera data.
        mainCamera = camera;
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

    /// <summary>
    ///     Gets the camera assigned to the draw space.
    /// </summary>
    /// <param name="ecs">the ecs state containing the camera.</param>
    /// <param name="drawSpace">the draw space of the camera to retrieve.</param>
    /// <param name="camera">output for the found camera.</param>
    /// <returns>true, if the camera was successfully found, otherwise false.</returns>
    public static bool GetDrawSpaceCamera(EcsState ecs, DrawSpace drawSpace, ref Camera camera)
    {
        GenIdResult result = default;
        GenId genid = drawSpace switch
        {
            DrawSpace.World => MainCameraId,
            DrawSpace.Gui => GuiCameraId,
            _ => default
        };

        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);

        ref Camera c = ref ComponentArray.GetData(cameras, ecs, genid, ref result);
        if (result != GenIdResult.Ok)
        {
            Log.WriteLine(LogType.Warn, $"{drawSpace} camera is a stale gen id");
            return false;
        }

        camera = c;
        return true;
    }
}