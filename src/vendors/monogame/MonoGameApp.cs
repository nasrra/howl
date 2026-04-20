using System;
using System.Text;
using Howl.Ecs;
using Howl.Graphics;
using Howl.Graphics.Text;
using Howl.Input;
using Howl.Vendors.MonoGame.Graphics;
using Howl.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace Howl.Vendors.MonoGame;

public class MonoGameApp : Game
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
    public Howl.Math.Vector2Int OutputResolution;

    /// <summary>
    ///     Gets the output resolutions aspect ratio.
    /// </summary>
    public float OutputResolutionAspectRatio => (float)OutputResolution.X / OutputResolution.Y;

    /// <summary>
    ///     The sprite batch for drawing monogame sprites.
    /// </summary>
    public SpriteBatch SpriteBatch;

    /// <summary>
    ///     The effects manager for monogame shaders.
    /// </summary>
    public EffectManager EffectManager;

    /// <summary>
    ///     The string builder used to render text.
    /// </summary>
    public StringBuilder StringBuilder;

    public GenIndexAllocator SpriteFontIds;
    public GenIndexList<SpriteFont> SpriteFonts;

    public TextureManagerState Textures;

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
    /// <exception cref="Exception"></exception>
    public MonoGameApp(int backBufferWidth, int backBufferHeight, int outputResolutionWidth, int outputResolutionHeight, int maxTextureCount)
    {
        IsMouseVisible = true;
        GraphicsDeviceManager = new(this);
        IsFixedTimeStep = false;
        GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
        GraphicsDeviceManager.ApplyChanges();
        
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        EffectManager = new(this);

        RendererSystem.SetBackBufferResolution(this, backBufferWidth, backBufferHeight);
        SetFinalRenderTargetResolution(this, outputResolutionWidth, outputResolutionHeight);
        DestinationRectangle = RendererSystem.CalculateRenderDestinationRectangle(this, FinalRenderTarget);

        StringBuilder = new(Text4096.MaxCharacters);
        SpriteFontIds = new();
        SpriteFonts = new();
        Textures = new(maxTextureCount);

        // set this to the same directory as the csproj as loading is handled via full paths fomr AssetManager.
        Content.RootDirectory = "";

        LinkEvents(this);
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




    /*******************
    
        Font Management.
    
    ********************/




    /// <summary>
    ///     Loads a font into a monogame app instance.
    /// </summary>
    /// <param name="app">the monogame app instance to contain the font instance.</param>
    /// <param name="fontFilePath">the file path of the font.</param>
    /// <param name="genIndex">output for the gen id of the newly allocated font.</param>
    public static void LoadFont(MonoGameApp app, string fontFilePath, out GenIndex genIndex)
    {
        app.SpriteFontIds.Allocate(out genIndex, out bool reusedFreeIndex);

        if (reusedFreeIndex == false)
        {
            GenIndexListProc.ResizeSparseEntries(app.SpriteFonts, app.SpriteFontIds.Entries.Count);
        }

        SpriteFont spriteFont = app.Content.Load<SpriteFont>(AssetManagement.AssetManager.FontFolder+fontFilePath);

        GenIndexListProc.Allocate(app.SpriteFonts, genIndex, spriteFont);
    }

    public static GenIndexResult GetFontReadOnlyRef(MonoGameApp app, in GenIndex genIndex, out ReadOnlyRef<SpriteFont> readOnlyRef)
    {
        return GenIndexListProc.GetDenseReadOnlyRef(app.SpriteFonts, genIndex, out readOnlyRef);
    }

    /// <summary>
    ///     Gets whether or not a font has been loaded.
    /// </summary>
    /// <param name="app">the monogame app instance that contains the font.</param>
    /// <param name="genIndex">the gen id of the font.</param>
    /// <returns>true, if the font is loaded; otherwise false.</returns>
    public static GenIndexResult IsFontLoaded(MonoGameApp app, GenIndex genIndex)
    {
        return GetFontReadOnlyRef(app, in genIndex, out ReadOnlyRef<SpriteFont> readOnlyRef);
    }

    /// <summary>
    ///     Sets the application window to be windowed.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    public static void SetWindowed(MonoGameApp app)
    {
        if(app.GraphicsDeviceManager.IsFullScreen == false)
        {
            return;
        }

        // NOTE:
        // this may need to be removed.
        // monoGameApp.GraphicsDeviceManager.HardwareModeSwitch = false;

        app.GraphicsDeviceManager.ToggleFullScreen();
    }

    /// <summary>
    ///     Sets the application window to be fullscreen.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    public static void SetFullscreen(MonoGameApp app)
    {
        if(app.GraphicsDeviceManager.IsFullScreen == true
        && app.GraphicsDeviceManager.HardwareModeSwitch == true)
        {
            return;
        }

        app.GraphicsDeviceManager.HardwareModeSwitch = true;
        app.GraphicsDeviceManager.ToggleFullScreen();


        // NOTE:
        // Monogame "corrupts" the computers back buffer when toggling fullscreen upon closing the application afterwards. 
        // The screen is fine for a split second then switches to the "Clear Colour" of the renderer.
        // nothing  can be clicked on the computer, alt+f4 doesnt work, it completely nukes the computer.
        
        // UpdateMainRenderDestinationRectangle(); <-- DONT DO THIS at the end of this, subscribe to the monogame OnGraphicsDeviceReset(object caller, EventArgs e).
    }

    /// <summary>
    ///     Sets the application window to be borderless fullscreen.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    public static void SetBorderlessFullscreen(MonoGameApp app)
    {
        if(app.GraphicsDeviceManager.IsFullScreen == true
        && app.GraphicsDeviceManager.HardwareModeSwitch == false)
        {
            return;
        }

        app.GraphicsDeviceManager.HardwareModeSwitch = false;
        app.GraphicsDeviceManager.ToggleFullScreen();
    }

    /// <summary>
    ///     Sets the target frame rate of the renderer.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="targetFrameRate"the new target frame rate.></param>
    public static void SetTargetFrameRate(MonoGameApp app, TargetFrameRate targetFrameRate)
    {
        switch (targetFrameRate)
        {
            case TargetFrameRate.D60:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(16);
            break;
            case TargetFrameRate.D90:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(11);
            break;
            case TargetFrameRate.D120:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(8);
            break;
            case TargetFrameRate.D144:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(7);
            break;
            case TargetFrameRate.D165:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(6f);
            break;
            case TargetFrameRate.D240:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(4);
            break;
            case TargetFrameRate.D360:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(3);
            break;
        }
    }

    /// <summary>
    ///     Gets the mouse position in world-space.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="mouse">the mouse data.</param>
    /// <returns>the position of the mouse in world-space.</returns>
    public static Howl.Math.Vector2 GetMouseWorldPosition(MonoGameApp app, IMouse mouse)
    {
        ref readonly Camera camera = ref CameraSystem.MainCamera;
        Howl.Math.Vector2Int renderTargetPosition = mouse.GetPositionRelative(app.DestinationRectangle, app.OutputResolution);
        
        // offset by half the output resolution as the world camera (0,0) is at the center of the screen.
        Howl.Math.Vector2 offset = new Howl.Math.Vector2(app.OutputResolution.X*0.5f, app.OutputResolution.Y*0.5f);
        
        return new Howl.Math.Vector2( 
            ((renderTargetPosition.X - offset.X)/camera.Zoom) + camera.Position.X,
            ((renderTargetPosition.Y - offset.Y)/camera.Zoom) - camera.Position.Y
        ).InvertY(); // invert y as world space in monogame is Y+ is down; where as howl engine is y+ is up.
    }

    /// <summary>
    ///     Gets the mouse postion in screen-space.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="mouse">the mouse data.</param>
    /// <returns>the position of the mouse in screen-space.</returns>
    public static Howl.Math.Vector2 GetMouseGuiPosition(MonoGameApp app, IMouse mouse)
    {
        ref readonly Camera camera = ref CameraSystem.GuiCamera;
        Howl.Math.Vector2Int renderTargetPosition = mouse.GetPositionRelative(app.DestinationRectangle, 
            new Howl.Math.Vector2Int(app.FinalRenderTarget.Width, app.FinalRenderTarget.Height)
        );        
        return new Howl.Math.Vector2( 
            (renderTargetPosition.X + camera.Position.X)/camera.Zoom,
            (renderTargetPosition.Y + camera.Position.Y)/camera.Zoom
        );
    }

    /// <summary>
    ///     Sets the resolution of the final render target.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="resolution">the resolution to set.</param>
    public static void SetFinalRenderTargetResolution(MonoGameApp app, Howl.Math.Vector2Int resolution)
    {
        SetFinalRenderTargetResolution(app, resolution.X, resolution.Y);
    }

    /// <summary>
    ///     Sets the resolution of the final render target.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="width">the new width of the final render target.</param>
    /// <param name="height">the new height of the final render target.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void SetFinalRenderTargetResolution(MonoGameApp app, int width, int height)
    {
        int clampedWidth = System.Math.Clamp(width, 1, int.MaxValue);  
        int clampedHeight = System.Math.Clamp(height, 1, int.MaxValue);  
        if(width == clampedWidth && height == clampedHeight)
        {            
            app.FinalRenderTarget?.Dispose();
            app.FinalRenderTarget = new RenderTarget2D(app.GraphicsDevice, width, height);
            app.OutputResolution = new Howl.Math.Vector2Int(width, height);
        }
        else
        {
            throw new ArgumentException($"Output resolution cannot be set to ({width}, {height}), values must be above zero and lower than or equal to int.MaxValue");            
        }
    }

    /// <summary>
    ///     Constructs a sprite from a loaded texture.
    /// </summary>
    /// <remarks>
    ///     Note: if the texture is not loaded, the returned sprite will instead fallback to the Nil value texture.
    /// </remarks>
    /// <param name="state">the texture manager state containing the loaded texture.</param>
    /// <param name="colourTint">the colour to tint the sprite.</param>
    /// <param name="sourceRectangle">the source rectangle - in pixels - of the sprite on the texture image.</param>
    /// <param name="scale">the scaling vector to apply to the sprite when drawing.</param>
    /// <param name="textureFilePath">the file path of the loaded texture.</param>
    /// <param name="layerDepth">the layer depth.</param>
    /// <param name="spriteOrigin">where the origin of the sprite will be placed.</param>
    /// <param name="worldSpace">whether or not the sprite is in world space.</param>
    /// <returns>the newly constructed sprite.</returns>
    public static Sprite ConstructSprite(TextureManagerState state, Colour colourTint, Howl.Math.Shapes.Rectangle sourceRectangle, 
        Howl.Math.Vector2 scale, string textureFilePath, float layerDepth, SpriteOrigin spriteOrigin, WorldSpace worldSpace
    )
    {
        int textureIndex = TextureManager.GetTextureIndex(state, textureFilePath);
        return ConstructSprite(state, colourTint, sourceRectangle, scale, textureIndex, layerDepth, spriteOrigin, worldSpace);
    }

    /// <summary>
    ///     Constructs a sprite from a loaded texture.
    /// </summary>
    /// <remarks>
    ///     Note: if the texture is not loaded, the returned sprite will instead fallback to the Nil value texture.
    /// </remarks>
    /// <param name="state">the texture manager state containing the loaded texture.</param>
    /// <param name="colourTint">the colour to tint the sprite.</param>
    /// <param name="sourceRectangle">the source rectangle - in pixels - of the sprite on the texture image.</param>
    /// <param name="scale">the scaling vector to apply to the sprite when drawing.</param>
    /// <param name="textureIndex">the index of the loaded texture.</param>
    /// <param name="layerDepth">the layer depth.</param>
    /// <param name="spriteOrigin">where the origin of the sprite will be placed.</param>
    /// <param name="worldSpace">whether or not the sprite is in world space.</param>
    /// <returns>the newly constructed sprite.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Sprite ConstructSprite(TextureManagerState state, Colour colourTint, Howl.Math.Shapes.Rectangle sourceRectangle, 
        Howl.Math.Vector2 scale, int textureIndex, float layerDepth, SpriteOrigin spriteOrigin, WorldSpace worldSpace
    )
    {
        if(state.Textures[textureIndex] == null)
        {
            // return the nil value if the texture is not loaded.
            textureIndex = 0;
        }

        // return the requested sprite.
        ref Texture2D texture = ref state.Textures[textureIndex];
        float originX = 0;
        float originY = 0;

        switch (spriteOrigin)
        {
            case SpriteOrigin.Center:
                int width = 0;
                int height = 0;
                TextureManager.GetTextureDimensionsUnsafe(state, textureIndex, ref width, ref height);
                originX = width*0.5f;
                originY = height*0.5f;
                break;
            case SpriteOrigin.TopLeft:
                // do nothing as origin x and y are already zero.
                break;
            default:
                goto case SpriteOrigin.TopLeft;
        }

        return new Sprite(sourceRectangle, colourTint, new Howl.Math.Vector2(originX, originY), scale, textureIndex, worldSpace, layerDepth);
    }

    /// <summary>
    ///     Links a monogame app to its internal events.
    /// </summary>
    /// <param name="app">the monogame app instance to link.</param>
    public static void LinkEvents(MonoGameApp app)
    {
        app.Disposed += app.OnDisposed;
        app.GraphicsDevice.DeviceReset += app.OnGraphicsDeviceReset;
    }

    /// <summary>
    ///     Unlinks a monogame app from its internal events.
    /// </summary>
    /// <param name="app">the monogame ap instance to unlink.</param>
    public static void UnlinkEvents(MonoGameApp app)
    {
        app.Disposed -= app.OnDisposed;
        app.GraphicsDevice.DeviceReset -= app.OnGraphicsDeviceReset;        
    }

    private void OnDisposed(object caller, EventArgs e)
    {
        UnlinkEvents(this);
        IsDisposed = true;

        SpriteFontIds.Dispose();
        SpriteFontIds = null;

        // this is fine as SpriteFont does not implement a Dispose method.
        SpriteFonts.Dispose();
        SpriteFonts = null;

        UpdateCallback = null;
        RenderCallback = null;
    }

    private void OnGraphicsDeviceReset(object caller, EventArgs e)
    {
        // This ensures that the main render destination rectangle
        // will always be the correct size when the window's back buffer resizes;
        // including when toggling fullscreen and manually setting the back buffer.
        DestinationRectangle = RendererSystem.CalculateRenderDestinationRectangle(this, FinalRenderTarget);
    }

}