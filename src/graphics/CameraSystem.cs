using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;


namespace Howl.Graphics;

public static class CameraSystem
{
    // /// <summary>
    // /// Gets the main camera scene id.
    // /// </summary>
    // public static GenIndex MainCameraSceneId => mainCameraSceneId;
    
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
    public static UpdateSystem UpdateSystem(ComponentRegistry componentRegistry, IRendererState state)
    {
        return deltaTime =>
        {
            UpdateProjectionMatrices(componentRegistry, state);  
        };
    }

    /// <summary>
    /// Updates all camera's projection matrices with a renderer states output resolution aspect ratio.
    /// </summary>
    /// <param name="componentRegistry">The component registry with the camera data.</param>
    /// <param name="state">the renderer state to update in accordance with.</param>
    public static void UpdateProjectionMatrices(ComponentRegistry componentRegistry, IRendererState state)
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