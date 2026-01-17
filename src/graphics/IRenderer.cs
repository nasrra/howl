

using Howl.ECS;
using Howl.Math;

namespace Howl.Graphics;

public interface IRenderer
{
    /// <summary>
    /// Gets and sets the color to clear to every frame.
    /// </summary>
    public Color ClearColor {get; set;}

    public ITextureManager TextureManager{get;}
    
    /// <summary>
    /// Sets the render target width.
    /// </summary>
    /// <param name="value">The width to set the render target to.</param>
    public void SetRenderTargetWidth(int value);    

    /// <summary>
    /// Sets the render target height. 
    /// </summary>
    /// <param name="value">The height to set the render target to.</param>
    public void SetRenderTargetHeight(int value);
    
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
    /// <returns></returns>
    public bool DrawSprite(in GenIndex textureId);
}