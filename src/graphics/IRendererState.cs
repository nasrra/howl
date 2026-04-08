using System;
using System.Collections.Generic;
using System.Text;
using Howl.Ecs;
using Howl.Graphics;
using Howl.Graphics.Text;
using Howl.Input;
using Howl.Math;
using Howl.Math.Shapes;

public interface IRendererState : IDisposable
{    
    /// <summary>
    /// Gets the texture manager.
    /// </summary>
    public ITextureManager TextureManager{get;}

    /// <summary>
    /// Gets the font system.
    /// </summary>
    public IFontManager FontManager{get;}

    /// <summary>
    /// Gets the string builder used to render text.
    /// </summary>
    public StringBuilder StringBuilder{get;}

    /// <summary>
    /// Gets the destination rectangle for stretching or shrinking the output 
    /// render target into the backbuffer.
    /// </summary>
    public Rectangle DestinationRectangle{get;}

    /// <summary>
    /// Gets the output resolution for the final render target 
    /// </summary>
    public Vector2Int OutputResolution{get;}

    /// <summary>
    /// Gets the output resolutions aspect ratio.
    /// </summary>
    public float OutputResolutionAspectRatio => (float)OutputResolution.X / OutputResolution.Y;

    /// <summary>
    /// Gets the renderer backend.
    /// </summary>
    public RendererBackend RendererBackend {get;}

    /// <summary>
    /// Gets whether or not this instance is disposed.
    /// </summary>
    public bool IsDisposed{get;}

    /// <summary>
    /// Sets the resolution of the final render target that everything gets draw to 
    /// before being draw to the back buffer.
    /// </summary>
    /// <param name="resolution">the width (x) and height (y) in pixels.</param>
    public void SetFinalRenderTargetResolution(Vector2Int resolution);

    /// <summary>
    /// Sets the resolution of the final render target that everything gets draw to 
    /// before being draw to the back buffer.
    /// </summary>
    /// <param name="width">the width in pixels.</param>
    /// <param name="height">the height in pixels.</param>
    public void SetFinalRenderTargetResolution(int width, int height);

    /// <summary>
    /// Sets the application window to be windowed.
    /// </summary>
    public void Windowed();

    /// <summary>
    /// Sets the application window to be fullscreen. 
    /// </summary>
    public void Fullscreen();

    /// <summary>
    /// Sets the application window to be borderless fullscreen.
    /// </summary>
    public void BorderlessFullscreen();

    /// <summary>
    /// Sets the target frame rate of the application.
    /// </summary>
    public void SetTargetFrameRate(TargetFrameRate targetFrameRate);

    /// <summary>
    /// Allocates a new render target.
    /// </summary>
    /// <param name="state">the renderer state instance to allocate with.</param>
    /// <param name="resolution">The resolution of the render target.</param>
    /// <param name="genIndex">The newly constructed gen-index associated with the render target.</param>
    public void AllocateNewRenderTarget(IRendererState state, Vector2Int resolution, out GenIndex genIndex);

    /// <summary>
    /// Deallocates a render target.
    /// </summary>
    /// <param name="genIndex">the gen index associated with the render target.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <description>
    ///                 <see cref="GenIndexResult.DenseNotAllocated"/>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public GenIndexResult DeallocateRenderTarget(GenIndex genIndex);

    /// <summary>
    /// Gets the mouse position relative to the main camera.
    /// </summary>
    /// <param name="mouse">the mouse to project onto the main camera.</param>
    /// <returns>the projected position.</returns>
    public Vector2 GetMouseWorldPosition(IMouse mouse);


    /// <summary>
    /// Gets the mouse position relative to the gui camera.
    /// </summary>
    /// <param name="mouse">the mouse to project onto the gui camera.</param>
    /// <returns>the projected position.</returns>
    public Vector2 GetMouseGuiPosition(IMouse mouse);
}