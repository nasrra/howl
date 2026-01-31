

using System;
using Howl.ECS;
using Howl.Math;
using Howl.Graphics.Text;

namespace Howl.Graphics;

public interface IRenderer : IDisposable
{
    protected const float DefaultWireframeThickness = 4;
    protected const int DefaultCirclePointAmount = 16;

    /// <summary>
    /// Gets the minimum back buffer resolution (horizontal and vertical) in pixels.
    /// </summary>
    protected const int MinBackBufferResolutionInPixels = 1;

    /// <summary>
    /// Gets the maximum back buffer resolution (horizontal and vertical) in pixels.
    /// </summary>
    protected const int MaxBackBufferResolutionInPixels = int.MaxValue;

    /// <summary>
    /// Gets the minimum output resolution (horizontal and vertical) in pixels.
    /// </summary>
    protected const int MinOutputResolutionInPixels = 1;

    /// <summary>
    /// Gets the maximum output resolution (horizontal and vertical) in pixels.
    /// </summary>
    protected const int MaxOutputResolutionInPixels = int.MaxValue;

    /// <summary>
    /// Gets and sets the color to clear the world to every frame.
    /// </summary>
    public Colour WorldClearColour {get; set;}

    /// <summary>
    /// Gets the colour to clear the Gui to every frame.
    /// </summary>
    public static Colour GuiClearColour => Colour.Transparent; 

    /// <summary>
    /// Gets the texture manager used by this renderer.
    /// </summary>
    public ITextureManager TextureManager{get;}

    /// <summary>
    /// Gets the font manager used by this renderer.
    /// </summary>
    public IFontManager FontManager{get;}

    /// <summary>
    /// Gets the world-space camera used by this renderer.
    /// </summary>
    public ICamera WorldCamera{get;}

    /// <summary>
    /// Gets the screen-space camera used by this renderer.
    /// </summary>
    public ICamera GuiCamera{get;}

    /// <summary>
    /// Gets the set output resolution - in pixels..
    /// </summary>
    public Vector2Int OutputResolution{get;}

    /// <summary>
    /// Gets the output resolution aspect ratio.
    /// </summary>
    public float OutputResolutionAspectRatio{get;}

    /// <summary>
    /// Sets the resolution this howl application is drawing at.
    /// </summary>
    /// <param name="resolution">The resolution parameters.</param>
    public void SetResolution(Resolution resolution);

    /// <summary>
    /// Sets the back buffer resolution (The actual application window size).
    /// </summary>
    /// <param name="resolution">the width (x) and height (y) in pixels.</param>
    public void SetBackBufferResolution(Vector2Int resolution);

    /// <summary>
    /// Sets the back buffer resolution (The actual application window size).
    /// </summary>
    /// <param name="width">the width in pixels.</param>
    /// <param name="height">the height in pixels.</param>
    public void SetBackBufferResolution(int width, int height);

    /// <summary>
    /// Sets the output resolution (the resolution the application is renderer at.)
    /// </summary>
    /// <param name="resolution">the width (x) and height (y) in pixels.</param>
    public void SetOutputResolution(Vector2Int resolution);

    /// <summary>
    /// Sets the output resolution (the resolution the application is renderer at.)
    /// </summary>
    /// <param name="width">the width in pixels.</param>
    /// <param name="height">the height in pixels.</param>
    public void SetOutputResolution(int width, int height);

    /// <summary>
    /// Starts the draw-world state.
    /// </summary>
    public void BeginDrawWorld();

    /// <summary>
    /// Completes the draw-world state.
    /// </summary>
    public void EndDrawWorld();

    /// <summary>
    /// Starts the draw-gui state.
    /// </summary>
    public void BeginDrawGui();

    /// <summary>
    /// Completes the draw-gui state.
    /// </summary>
    public void EndDrawGui();

    /// <summary>
    /// Submits the draw calls to the gpu to be displayed to the screen.
    /// </summary>
    public void SubmitDraw();

    /// <summary>
    /// Draws a sprite to the currently bound render texture.
    /// </summary>
    /// <param name="transform">The transform data.</param>
    /// <param name="sprite">The sprite data.</param>
    /// <returns>A gen index result in relation to retrieving the texture stored within the sprite.</returns>
    public GenIndexResult DrawSprite(in Transform transform, in Sprite sprite);

    /// <summary>
    /// Draws a line between to points in world space for a single frame.
    /// </summary>
    /// <param name="a">The point to start the line segment from.</param>
    /// <param name="b">The point to end the line segment at.</param>
    /// <param name="thickness">The thickness of the line segment.</param>
    /// <param name="color">The color od the line segment.</param>
    /// <param name="scaleThickness">Scale the thickness by the camera zoom.</param>
    public void DrawLine(Vector2 a, Vector2 b, Colour color, float thickness, bool scaleThickness = true);

    /// <summary>
    /// Draws a filled shape for a single frame.
    /// </summary>
    /// <param name="transform">The transform data.</param>
    /// <param name="shape">The rectangle to draw.</param>
    public void DrawFilledShape(in Transform transform, in RectangleShape shape);

    /// <summary>
    /// Draws a wireframe shape for a single frame.
    /// </summary>
    /// <param name="transform">The transform data.</param>
    /// <param name="shape">The rectangle data.</param>
    /// <param name="thickness">The thickness of the wireframe line segments.</param>
    public void DrawWireframeShape(in Transform transform, in RectangleShape shape, float thickness = DefaultWireframeThickness);

    /// <summary>
    /// Draws a filled shape for a single frame.
    /// </summary>
    /// <param name="transform">The transform data.</param>
    /// <param name="shape">The circle data.</param>
    /// <param name="verticeCount">The amount of vertices used to draw the circle.</param>
    public void DrawFilledShape(in Transform transform, in CircleShape shape, int verticeCount = DefaultCirclePointAmount);

    /// <summary>
    /// Draws a wireframe shape for a single frame.
    /// </summary>
    /// <param name="transform">The transform data.</param>
    /// <param name="shape">The circle data.</param>
    /// <param name="verticeCount">The amount of vertices used to draw the circle.</param>
    /// <param name="thickness">The thickness of the wireframe line segments.</param>
    public void DrawWireframeShape(in Transform transform, in CircleShape shape, int verticeCount = DefaultCirclePointAmount, float thickness = DefaultWireframeThickness);

    /// <summary>
    /// Draws a wireframe shape for a single frame.
    /// </summary>
    /// <param name="transform">The transform data.</param>
    /// <param name="shape">The polygon data.</param>
    /// <param name="thickness">The thickness of the wireframe line segments.</param>
    public void DrawWireframeShape(in Transform transform, in Polygon16Shape shape, float thickness = DefaultWireframeThickness);

    /// <summary>
    /// Draws a wireframe shape for a single frame.
    /// </summary>
    /// <param name="transform">The transform data.</param>
    /// <param name="shape">The polygon data.</param>
    /// <param name="thickness">The thickness of the wireframe line segments.</param>
    public void DrawWireframeShape(in Transform transform, in Polygon4Shape shape, float thickness = DefaultWireframeThickness);

    /// <summary>
    /// Draws text for a single frame.
    /// </summary>
    /// <param name="transform">The transform data.</param>
    /// <param name="text">The text to draw.</param>
    /// <returns>The result when retrieving the font of the text.</returns>
    public GenIndexResult DrawText(in Transform transform, in Text16 text);    

    /// <summary>
    /// Draws text for a single frame.
    /// </summary>
    /// <param name="transform">The transform data.</param>
    /// <param name="text">The text to draw.</param>
    /// <returns>The result when retrieving the font of the text.</returns>
    public GenIndexResult DrawText(in Transform transform, in Text4096 text);

    /// <summary>
    /// Sets the application window to be windowed.
    /// </summary>
    internal void Windowed();

    /// <summary>
    /// Sets the application window to be fullscreen. 
    /// </summary>
    internal void Fullscreen();

    /// <summary>
    /// Sets the application window to be borderless fullscreen.
    /// </summary>
    internal void BorderlessFullscreen();
}