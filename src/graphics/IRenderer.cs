

using System;
using Howl.ECS;
using Howl.Math;

namespace Howl.Graphics;

public interface IRenderer : IDisposable
{
    /// <summary>
    /// Gets and sets the color to clear to every frame.
    /// </summary>
    public Color ClearColor {get; set;}

    /// <summary>
    /// Gets the texture manager used by this renderer.
    /// </summary>
    public ITextureManager TextureManager{get;}

    /// <summary>
    /// Gets the camera used by this renderer.
    /// </summary>
    public ICamera Camera{get;}

    /// <summary>
    /// Sets the resolution this howl application is drawing at.
    /// </summary>
    /// <param name="resolution">The resolution parameters.</param>
    public void SetResolution(Resolution resolution);

    /// <summary>
    /// Sets the back buffer resolution (The actual window size).
    /// </summary>
    /// <param name="resolution">the width (x) and height (y) in pixels.</param>
    public void SetBackBufferResolution(Vector2Int resolution);

    /// <summary>
    /// Sets the back buffer resolution (The actual window size).
    /// </summary>
    /// <param name="width">the width in pixels.</param>
    /// <param name="height">the height in pixels.</param>
    public void SetBackBufferResolution(int width, int height);

    /// <summary>
    /// Sets the main render target resolution (the resolution the application is renderer at.)
    /// </summary>
    /// <param name="resolution">the width (x) and height (y) in pixels.</param>
    public void SetMainRenderTargetResolution(Vector2Int resolution);

    /// <summary>
    /// Sets the main render target resolution (the resolution the application is renderer at.)
    /// </summary>
    /// <param name="width">the width in pixels.</param>
    /// <param name="height">the height in pixels.</param>
    public void SetMainRenderTargetResolution(int width, int height);

    /// <summary>
    /// Starts the draw call from the renderer to the gpu.
    /// </summary>
    public void BeginDraw();

    /// <summary>
    /// completes the draw call from the renderer to the gpu, displaying an image to the window.
    /// </summary>
    public void EndDraw();

    /// <summary>
    /// Draws a primitive for a single frame.
    /// </summary>
    /// <param name="rectangle"></param>
    /// <param name="color"></param>
    public void DrawPrimitive(Rectangle rectangle, Color color);

    /// <summary>
    /// Draws a line between to points in world space.
    /// </summary>
    /// <param name="a">The point to start the line segment from.</param>
    /// <param name="b">The point to end the line segment at.</param>
    /// <param name="thickness">The thickness of th  line segment.</param>
    /// <param name="color">The color od the line segment.</param>
    public void DrawLine(Vector2 a, Vector2 b, float thickness, Color color);

    /// <summary>
    /// Draws a sprite to the currently bound render texture.
    /// </summary>
    /// <param name="textureId"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool DrawSprite(in GenIndex textureId, Vector2 position);

    /// <summary>
    /// Gets the main render target width.
    /// </summary>
    public float MainRenderTargetWidth{get;}

    /// <summary>
    /// Gets the main render target height.
    /// </summary>
    public float MainRenderTargetHeight{get;}

    /// <summary>
    /// Gets the main render target aspect ratio.
    /// </summary>
    public float MainRenderTargetAspectRatio{get;}
}