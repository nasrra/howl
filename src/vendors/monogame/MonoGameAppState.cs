using System;
using System.Text;
using Howl.Vendors.MonoGame.FontStashSharp;
using Howl.Vendors.MonoGame.Graphics;
using Howl.Vendors.MonoGame.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Vendors.MonoGame;

public class MonoGameAppState : Game
{




    /*******************
    
        Member Variables.
    
    ********************/




    /// <summary>
    ///     The graphics device manager.
    /// </summary>
    public GraphicsDeviceManager GraphicsDeviceManager {get; private set;}

    /// <summary>
    ///     The destination rectangle for stretching or shrinking the output render target into the backbuffer.
    /// </summary>
    public Howl.Math.Shapes.Rectangle DestinationRectangle;

    /// <summary>
    ///     The final render target to draw all sprites, text, shaders, etc. to.
    /// </summary>
    public RenderTarget2D FinalRenderTarget;
    
    /// <summary>
    ///     A copy of the output resolution for the final render target
    /// </summary>
    public Vector2 OutputResolution;

    /// <summary>
    ///     Gets the output resolutions aspect ratio.
    /// </summary>
    public float OutputResolutionAspectRatio => OutputResolution.X / OutputResolution.Y;

    /// <summary>
    ///     The sprite batch for drawing monogame sprites.
    /// </summary>
    public SpriteBatch SpriteBatch;

    /// <summary>
    ///     The effects manager for monogame shaders.
    /// </summary>
    public EffectManager EffectManager;

    /// <summary>
    ///     The Debug Draw state.
    /// </summary>
    public DebugDrawState DebugDrawState;

    /// <summary>
    ///     The font manager state.
    /// </summary>
    public FontManagerState FontManagerState;

    /// <summary>
    ///     The texture manager state.
    /// </summary>
    public TextureManagerState TextureManagerState;

    /// <summary>
    ///     The input manager state.
    /// </summary>
    public InputManagerState InputManagerState;

    /// <summary>
    ///     Whether this instance has been disposed of.
    /// </summary>
    public bool IsDisposed;




    /*******************
    
        Function Pointers.
    
    ********************/




    /// <summary>
    ///     The update callback for external classes.
    /// </summary>
    public Action<float> UpdateCallback;

    /// <summary>
    ///     The render callback for external classes.
    /// </summary>
    public Action<float> RenderCallback;




    /*******************
    
        Constructor.
    
    ********************/


    /// <summary>
    ///     Creates a new Moongame app instance.
    /// </summary>
    /// <param name="backBufferWidth">the width of the back buffer.</param>
    /// <param name="backBufferHeight">the height of the back buffer.</param>
    /// <param name="outputResolutionHeight">the width of the render target output resolution.</param>
    /// <param name="outputResolutionWidth">the height of the render target output resolution.</param>
    /// <param name="maxTextureCount">the maximum amount of textures that can be registered to the texture manager.</param>
    /// <param name="maxFontCount">the maximum amount of fonts that can be registered to the font manager.</param>
    /// <param name="debugDrawMaxPolygons">the maximum amount of debug polygons that can be drawn.</param>
    public MonoGameAppState(int backBufferWidth, int backBufferHeight, int outputResolutionWidth, int outputResolutionHeight, 
        int maxTextureCount, int maxFontCount, int debugDrawMaxPolygons
    )
    {
        IsMouseVisible = true;
        GraphicsDeviceManager = new(this);
        IsFixedTimeStep = false;
        GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
        GraphicsDeviceManager.ApplyChanges();
        
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        EffectManager = new(this);
        DebugDrawState = new DebugDrawState(debugDrawMaxPolygons*3);

        RendererSystem.SetBackBufferResolution(this, backBufferWidth, backBufferHeight);
        MonoGameApp.SetFinalRenderTargetResolution(this, outputResolutionWidth, outputResolutionHeight);
        DestinationRectangle = RendererSystem.CalculateRenderDestinationRectangle(this, FinalRenderTarget);

        TextureManagerState = new(maxTextureCount);
        InputManagerState = new();
        FontManagerState = new(maxFontCount);

        MonoGameApp.LinkEvents(this);
    }




    /*******************
    
        Loop.
    
    ********************/




    protected override void Update(GameTime gameTime)
    {
        UpdateCallback?.Invoke(GameTimeToDeltaTime(gameTime));
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // this submits to the gpu.
        // and should stay at the bottom.

        base.Draw(gameTime);
        RenderCallback?.Invoke(GameTimeToDeltaTime(gameTime));
    }

    protected float GameTimeToDeltaTime(GameTime gameTime)
    {
        return (float)gameTime.ElapsedGameTime.TotalSeconds;
    }




    /******************
    
        Linkage
    
    *******************/


    public void OnGraphicsDeviceReset(object caller, EventArgs e)
    {
        // This ensures that the main render destination rectangle
        // will always be the correct size when the window's back buffer resizes;
        // including when toggling fullscreen and manually setting the back buffer.
        DestinationRectangle = RendererSystem.CalculateRenderDestinationRectangle(this, FinalRenderTarget);
    }
}