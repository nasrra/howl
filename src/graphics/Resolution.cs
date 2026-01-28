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
    /// The output resolution (the resolution the app is renderer at) width in pixels.
    /// </summary>
    public readonly int OutputWidth; 

    /// <summary>
    /// The output resolution (the resolution the app is renderer at) height in pixels.
    /// </summary>
    public readonly int OutputHeight;

    /// <summary>
    /// Constructs a new Resolution.
    /// </summary>
    /// <param name="backBufferWidth">The back buffer width in pixels.</param>
    /// <param name="backBufferHeight">The back buffer height in pixels.</param>
    /// <param name="outputWidth">The output resolution width in pixels.</param>
    /// <param name="outputHeight">The output resolution height in pixels.</param>
    public Resolution(int backBufferWidth, int backBufferHeight, int outputWidth, int outputHeight)
    {
        BackBufferWidth = backBufferWidth;
        BackBufferHeight = backBufferHeight;
        OutputWidth = outputWidth;
        OutputHeight = outputHeight;
    }
}