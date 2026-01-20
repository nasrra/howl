namespace Howl.Graphics;

public readonly struct Resolution
{
    /// <summary>
    /// The back buffer (The actual application window) width in pixels.
    /// </summary>
    public readonly int BackBufferWidth; 

    /// <summary>
    /// The back buffer (The actual application window) height in pixels.
    /// </summary>
    public readonly int BackBufferHeight; 

    /// <summary>
    /// The main render target (what everything is drawn onto) width in pixels.
    /// </summary>
    public readonly int MainRenderTargetWidth; 

    /// <summary>
    /// The main render target (what everything is drawn onto) height in pixels.
    /// </summary>
    public readonly int MainRenderTargetHeight;

    /// <summary>
    /// Constructs a new Resolution.
    /// </summary>
    /// <param name="backBufferWidth">The back buffer width in pixels.</param>
    /// <param name="backBufferHeight">The back buffer height in pixels.</param>
    /// <param name="mainRenderTargetWidth">The main render target width in pixels.</param>
    /// <param name="mainRenderTargetHeight">The main render target height in pixels.</param>
    public Resolution(int backBufferWidth, int backBufferHeight, int mainRenderTargetWidth, int mainRenderTargetHeight)
    {
        BackBufferWidth = backBufferWidth;
        BackBufferHeight = backBufferHeight;
        MainRenderTargetWidth = mainRenderTargetWidth;
        MainRenderTargetHeight = mainRenderTargetHeight;
    }
}