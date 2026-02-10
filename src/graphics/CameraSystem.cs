using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;


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
    private static GenIndex mainCameraId;

    /// <summary>
    /// Gets the main camera id.
    /// </summary>
    public static GenIndex MainCameraId => mainCameraId;

    /// <summary>
    /// Gets and sets the gui camera id.
    /// </summary>
    private static GenIndex guiCameraId;

    /// <summary>
    /// Gets the gui camera id.
    /// </summary>
    public static GenIndex GuiCameraId => guiCameraId;

    /// <summary>
    /// Registers all necessary components for this system into the specified component registry. 
    /// </summary>
    /// <param name="componentRegistry">the component registry.</param>
    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.RegisterComponent<Camera>();
    }

    /// <summary>
    /// Creates a new update system instance.
    /// </summary>
    /// <param name="componentRegistry">the component registry containing the cameras to update.</param>
    /// <param name="state">the renderer state.</param>
    /// <returns>the new update system instance.</returns>
    public static void Update(ComponentRegistry componentRegistry, IRendererState state)
    {
        UpdateProjectionMatrices(componentRegistry, state);  
        
        // order matters here:
        // updating the main camera's copies the data of the stored camera
        // in the component registry; make sure to update the projection matrices 
        // before  copying.
        UpdateMainCamera(componentRegistry);
        UpdateGuiCamera(componentRegistry);
    }

    /// <summary>
    /// Updates the cached main camera.
    /// </summary>
    /// <param name="componentRegistry">The component registry that stores the camera.</param>
    private static void UpdateMainCamera(ComponentRegistry componentRegistry)
    {
        if(componentRegistry.Get<Camera>().GetDenseRef(mainCameraId, out Ref<Camera> camera).Fail())
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
    /// <param name="componentRegistry">The component registry that stores the camera.</param>
    private static void UpdateGuiCamera(ComponentRegistry componentRegistry)
    {
        if(componentRegistry.Get<Camera>().GetDenseRef(guiCameraId, out Ref<Camera> camera).Fail())
        {
            // there must always be a gui camera.
            System.Diagnostics.Debug.Assert(false);
            return;
        }

        // copy camera data.
        guiCamera = camera;
    }


    /// <summary>
    /// Updates all camera's projection matrices with a renderer states output resolution aspect ratio.
    /// </summary>
    /// <param name="componentRegistry">The component registry with the camera data.</param>
    /// <param name="state">the renderer state to update in accordance with.</param>
    private static void UpdateProjectionMatrices(ComponentRegistry componentRegistry, IRendererState state)
    {
        GenIndexList<Camera> cameras = componentRegistry.Get<Camera>();
        Span<DenseEntry<Camera>> denseEntries = cameras.GetDenseAsSpan();
        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<Camera> denseEntry = ref denseEntries[i];
            ref Camera camera = ref denseEntry.Value;
            camera.UpdateProjectionMatrix(state.OutputResolutionAspectRatio);
        }
    }

    /// <summary>
    /// Sets a camera to the main camera.
    /// </summary>
    /// <param name="cameraId"></param>
    public static void SetMainCamera(GenIndex cameraId)
    {
        mainCameraId = cameraId;
    }

    /// <summary>
    /// Sets a camera to the gui camera.
    /// </summary>
    /// <param name="cameraId"></param>
    public static void SetGuiCamera(GenIndex cameraId)
    {
        guiCameraId = cameraId;
    }
}