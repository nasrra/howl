using System;
using System.Diagnostics;
using System.Text;
using Howl.Ecs;
using Howl.Graphics;
using Howl.Graphics.Text;
using Howl.Input;
using Howl.Vendors.MonoGame.Graphics;
using Howl.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Howl.Vendors.MonoGame;

public unsafe class MonoGameApp : Game
{




    /*******************
    
        Singleton.
    
    ********************/




    /// <summary>
    ///     Singleton.
    /// </summary>
    public static MonoGameApp Instance;




    /*******************
    
        Member Variables.
    
    ********************/




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

    public GenIndexAllocator TextureIds;
    public GenIndexList<Texture2D> Textures;

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
    public MonoGameApp(int backBufferWidth, int backBufferHeight, int outputResolutionWidth, int outputResolutionHeight)
    {
        // singleton guard.
        if (Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            throw new Exception("Only one MonoGameApp Instance can created at a time.");
        }

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
        TextureIds = new();
        Textures = new();

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
    
        Texture Management.
    
    ********************/



    /// <summary>
    ///     Loads a texture from disc.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="texturePath"></param>
    /// <param name="genIndex"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public static GenIndexResult LoadTexture(MonoGameApp app, string texturePath, out GenIndex genIndex)
    {
        app.TextureIds.Allocate(out genIndex, out bool reusedFreeGenIndex);
        if(reusedFreeGenIndex == false)
        {
            // resize the sparse entries to match the texture ids so every texture id has a possible entry point
            // into the textures storage.
            GenIndexListProc.ResizeSparseEntries(app.Textures, app.TextureIds.Entries.Count);
        }

        Texture2D texture = null;
        string path = Path.Combine(AssetManagement.AssetManager.AssetsFolder, texturePath);
        try
        {
            using(FileStream stream = new FileStream(path, FileMode.Open))
            {
                texture = Texture2D.FromStream(app.GraphicsDevice, stream);
            }
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"Texture2D file not found: {path}");
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }
        catch(IOException e)
        {
            throw new IOException($"Error reading file: {path}: {e.Message}");
        }
        
        GenIndexListProc.Allocate(app.Textures, genIndex, texture).Ok(out GenIndexResult result);
        return result;
    }

    /// <summary>
    ///     Gets the dimensions of a loaded texture.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="genIndex"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public static GenIndexResult GetTextureDimensions(MonoGameApp app, in GenIndex genIndex, out Howl.Math.Vector2 dimensions)
    {
        GenIndexResult result = GenIndexListProc.GetDenseReadOnlyRef(app.Textures, genIndex, out ReadOnlyRef<Texture2D> textureRef);
        
        dimensions = result == GenIndexResult.Ok
        ? new(textureRef.Value.Width, textureRef.Value.Height)
        : default;

        return result;
    }

    /// <summary>
    ///     Unloads a loaded texture from memory.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static GenIndexResult UnloadTexture(MonoGameApp app, in GenIndex index)
    {
        // ensure to dispose of the monogame texture before deallocating it.
        GenIndexResult result;

        if(GenIndexListProc.GetDenseRef(app.Textures, index, out Ref<Texture2D> reference).Fail(out result))
            goto Fail;

        reference.Value.Dispose();
        
        if(GenIndexListProc.Deallocate(app.Textures.Dense, app.Textures.Sparse, index).Fail(out result))
            goto Fail;

        if(app.TextureIds.Deallocate(index).Fail(out result))
            goto Fail;

        return result;

        Fail:
            return result;
    }


    public static GenIndexResult GetTextureReadonlyRef(MonoGameApp app, in GenIndex index, out ReadOnlyRef<Texture2D> readOnlyRef)
    {
        return GenIndexListProc.GetDenseReadOnlyRef(app.Textures, index, out readOnlyRef);
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