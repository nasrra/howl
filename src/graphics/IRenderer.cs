

using Howl.ECS;

namespace Howl.Graphics;

public interface IRenderer
{
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
    /// Draws a sprite to the currently bound render texture.
    /// </summary>
    /// <param name="textureId"></param>
    /// <returns></returns>
    public bool DrawSprite(in GenIndex textureId);
}