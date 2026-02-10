using Howl.ECS;
using Howl.Math;

namespace Howl.Graphics;

/// <remarks>
/// Note: a camera should not have a transform component attatched to the same entity.
/// </remarks>
public struct Camera
{
    private const float MinZoom = float.Epsilon;
    private const float MaxZoom = float.MaxValue;

    // Note:
    //  - The projection matrix is responsbible for turning a three dimensional
    //  image into a two dimensional image to be displayed to the screen.
    //  - The view matrix is location and orientation of this camera.
    //  - The world matrix is responsbible for positioning
    //  an entity into the world. (Position and Rotation in three dimensional space).

    /// <summary>
    /// Gets and sets the projection matrix.
    /// </summary>
    private Matrix projectionMatrix;

    /// <summary>
    /// Gets the projection matrix.
    /// </summary>
    /// <remarks>
    /// The projection matrix is responsbible for turning a three dimensional
    /// image into a two dimensional image to be displayed to the screen.
    /// </remarks>
    public readonly Matrix ProjectionMatrix => projectionMatrix; 

    /// <summary>
    /// Gets and sets the clear colour for the world render target.
    /// </summary>
    public Colour ClearColour;

    /// <summary>
    /// Gets and sets the horizontal (x) and vertical (x) viewing area size of the camera in pixels.
    /// </summary>
    private Vector2 extents;

    /// <summary>
    /// Gets the horizontal (x) and vertical (x) viewing area size of the camera in pixels.
    /// </summary>
    public readonly Vector2 Extents => extents;

    /// <summary>
    /// Gets and sets the position.
    /// </summary>
    public Vector2 Position;
    
    /// <summary>
    /// Gets and sets the zoom of the camera.
    /// </summary>
    private float zoom;

    /// <summary>
    /// Gets the zoom of the camera.
    /// </summary>
    public readonly float Zoom => zoom;

    /// <summary>
    /// Gets and sets the reference size for a camera's view in world units;
    /// independent of the actual screen resolution.
    /// </summary>
    /// <remarks>
    /// Note: The value should reflect the target resolution of the application.
    /// - a game meant to be played at a resolution of 1920x1080: set this to 1080.
    /// - a pixel art game meant to be played at a resolution of 640x360: set this to 360.
    /// </remarks>
    private float zoomVirtualHeight;

    /// <summary>
    /// Gets the reference size for a camera's view in world units;
    /// independent of the actual screen resolution.
    /// </summary>
    /// <remarks>
    /// Note: The value should reflect the target resolution of the application.
    /// - a game meant to be played at a resolution of 1920x1080: set this to 1080.
    /// - a pixel art game meant to be played at a resolution of 640x360: set this to 360.
    /// </remarks>
    public readonly float ZoomVirtualHeight => zoomVirtualHeight;

    /// <summary>
    /// Gets and sets the draw layer of the camera.
    /// </summary>
    public int Layer;

    /// <summary>
    /// Gets and sets the coordinate space of the camera.
    /// </summary>
    public CoordinateSpace CoordinateSpace;

    /// <summary>
    /// Whether or not this camera is actively drawing to the screen.
    /// </summary>
    public bool IsActive;

    /// <summary>
    /// Contructs a camera.
    /// </summary>
    /// <param name="clearColour">The colour to clear the render target to before every draw.</param>
    /// <param name="zoom">the zoom level.</param>
    /// <param name="zoomVirtualHeight">the virtual height.</param>
    /// <param name="layer">The draw order for the camera - higher will be drawn last and lower will be drawn before..</param>
    /// <param name="coordinateSpace">the coordinate space this camera projects in.</param>
    /// <param name="active">whether or not this camera is actively drawing.</param>
    public Camera(
        Colour clearColour, 
        float zoom, 
        float zoomVirtualHeight, 
        int layer,
        CoordinateSpace coordinateSpace, 
        bool active
    )
    {
        ClearColour = clearColour;
        SetZoom(zoom);
        this.zoomVirtualHeight = zoomVirtualHeight; 
        Layer = layer;
        CoordinateSpace = coordinateSpace;
        IsActive = active;
    }

    /// <summary>
    /// Sets the zoom of the camera.
    /// </summary>
    /// <remarks>
    /// Zoom will be clamped by a min and max value to ensure there is no 
    /// numerical overflow or negative values.
    /// </remarks>
    /// <param name="zoom"></param>
    public void SetZoom(float zoom)
    {
        this.zoom = Math.Math.Clamp(zoom, MinZoom, MaxZoom);
    }

    /// <summary>
    /// Updates the projection matrix in accordance with the applications render target output resolution aspect ratio.
    /// </summary>
    /// <param name="outputResolutionAspectRatio">the applications render target output resolution aspect ratio</param>
    public void UpdateProjectionMatrix(float outputResolutionAspectRatio)
    {
        // Note:
        // Up is y+ and right is x+;
        // Centered orthographic projection

        // Compute half-width and half-height in world units based on virtual resolution
        float halfHeight = zoomVirtualHeight * 0.5f / zoom;
        float halfWidth = halfHeight * outputResolutionAspectRatio; // keep aspect ratio correct
        float height = halfHeight * 2;
        float width = halfWidth * 2;
        extents = new(width, height);

        switch (CoordinateSpace)
        {
            case CoordinateSpace.Cartesian:
                projectionMatrix = Matrix.CreateOrthographicOffCenter(
                    -halfWidth,  halfWidth,
                    halfHeight, -halfHeight,
                    float.Epsilon,
                    float.MaxValue
                );
                break;
            case CoordinateSpace.Rasterized:
                projectionMatrix = Matrix.CreateOrthographicOffCenter(
                    0,  width,
                    height, 0,
                    float.Epsilon,
                    float.MaxValue
                );
                break;
            default:
                throw new System.NotImplementedException($"Camera does not support coordinate space: '{CoordinateSpace}'");
        }
        
    }
}