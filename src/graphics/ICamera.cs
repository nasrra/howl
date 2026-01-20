using System;
using Howl.Math;

public interface ICamera : IDisposable
{
    private const float MinZoom = float.Epsilon;

    private const float MaxZoom = float.MaxValue;

    // Note:
    // The projection matrix is responsbible for turning a three dimensional
    // image into a two dimensional image to be displayed to the screen.

    /// <summary>
    /// Gets and sets the projection matrix.
    /// </summary>
    public Matrix ProjectionMatrix {get; set;}

    // Note:
    // The view matrix is location and orientation of this camera.

    // /// <summary>
    // /// Gets and sets the view matrix.
    // /// </summary>
    // public Matrix ViewMatrix {get; set;}

    // // Note:
    // // The world matrix is responsbible for positioning
    // // an entity into the world. (Position and Rotation in three dimensional space).

    // /// <summary>
    // /// Gets and sets the world matrix.
    // /// </summary>
    // public Matrix WorldMatrix {get; set;}

    /// <summary>
    /// The position of the camera in world-space.
    /// </summary>
    public Vector2 Position {get; set;}
    
    // /// <summary>
    // /// The position in world-space where this camera is pointing at.
    // /// </summary>
    // public Vector3 Target {get; set;}

    /// <summary>
    /// Gets and sets the zoom of the camera.
    /// </summary>
    public float Zoom {get; set;}    

    /// <summary>
    /// Updates this camera.
    /// </summary>
    public void Update();

    /// <summary>
    /// Gets the reference siz for a camera's view in world units;
    /// independent of the actual screen resolution.
    /// Note:
    /// The value should reflect the target resolution of the application.
    /// - a game meant to be played at a resolution of 1920x1080: set this to 1080.
    /// - a pixel art game meant to be played at a resolution of 640x360: set this to 360.
    /// </summary>
    public float ZoomVirtualHeight {get;}
}