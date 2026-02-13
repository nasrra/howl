using System;
using System.Collections.Generic;
using System.Text;
using Howl.ECS;
using Howl.Graphics;
using Howl.Graphics.Text;
using Howl.Input;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Vendors.MonoGame.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Vendors.MonoGame.Graphics;

public sealed class RendererState : IRendererState
{    
    /// <summary>
    /// Gets and sets the font manager.
    /// </summary>
    private FontManager fontManager;
    public IFontManager FontManager => fontManager;

    /// <summary>
    /// Gets and sets the string builder used to render text.
    /// </summary>
    private StringBuilder stringBuilder;
    public StringBuilder StringBuilder => stringBuilder;

    /// <summary>
    /// Gets the effects manager for monogame shaders.
    /// </summary>
    public EffectManager EffectManager { get; private set; }

    /// <summary>
    /// Gets the sprite batch for drawing monogame sprites.
    /// </summary>
    private SpriteBatch spriteBatch;

    /// <summary>
    /// Gets and sets the sprite batch for drawing monogame sprites.
    /// </summary>
    public SpriteBatch SpriteBatch => spriteBatch;

    /// <summary>
    /// Gets and sets the monogame texture manager.
    /// </summary>
    private TextureManager<Texture2D> textureManager;
    public ITextureManager TextureManager => textureManager;

    /// <summary>
    /// Gets and sets the monogame app dependency.
    /// </summary>
    public MonoGameApp MonoGameApp;

    /// <summary>
    /// Gets and sets the MonoGame render target component collection.
    /// </summary>
    private DisposableClassComponentCollection<RenderTarget2D> renderTargets;

    /// <summary>
    /// Gets the MonoGame render target component collection.
    /// </summary>
    public DisposableClassComponentCollection<RenderTarget2D> RenderTargets => renderTargets;

    /// <summary>
    /// Gets and sets the final render target.
    /// </summary>
    private RenderTarget2D finalRenderTarget;

    /// <summary>
    /// Gets the final render target.
    /// </summary>
    public RenderTarget2D FinalRenderTarget => finalRenderTarget;

    /// <summary>
    /// Gets and sets the destination rectangle for stretching or shrinking the output render target into the backbuffer.
    /// </summary>
    private Rectangle destinationRectangle;
    public Rectangle DestinationRectangle => destinationRectangle;

    /// <summary>
    /// Gets and sets the output resolution for the final render target
    /// </summary>
    private Vector2Int outputResolution;
    public Vector2Int OutputResolution => outputResolution;

    public RendererBackend RendererBackend => RendererBackend.MonoGame;
    
    /// <summary>
    /// Gets and sets whether or not this instance is disposed.
    /// </summary>
    private bool disposed = false;
    public bool IsDisposed => disposed;


    /// <summary>
    /// Creates a new MonoGame renderer state instance.
    /// </summary>
    /// <param name="monoGameApp">The MonoGame app that is used as the HowlApp backend.</param>
    /// <param name="resolution">the output resolution - in pixels.</param>
    public RendererState(MonoGameApp monoGameApp,Resolution resolution)
    :this(monoGameApp, resolution.BackBufferWidth, resolution.BackBufferHeight, resolution.OutputWidth, resolution.OutputHeight)
    {}

    /// <summary>
    /// Creates a new MonoGame renderer state instace.
    /// </summary>
    /// <param name="monoGameApp">The MonoGame app that is used as the HowlApp backend.</param>
    /// <param name="backBufferResolution">the back buffer resolution - in pixels.</param>
    /// <param name="outputResolution">the output resolution - in pixels.</param>
    public RendererState(MonoGameApp monoGameApp,Vector2Int backBufferResolution,Vector2Int outputResolution)
    : this(monoGameApp, backBufferResolution.X, backBufferResolution.Y, outputResolution.X, outputResolution.Y)
    {}

    /// <summary>
    /// Creates a new MonoGame renderer state instances.
    /// </summary>
    /// <param name="monoGameApp">The MonoGame app that is used as the HowlApp backend.</param>
    /// <param name="backBufferWidth">The back buffer width in pixels.</param>
    /// <param name="backbufferHeight">The back buffer height in pixels.</param>
    /// <param name="outputResolutionWidth">The output resolution width in pixels.</param>
    /// <param name="outputResolutionHeight">The output resolution height in pixels.</param>
    public RendererState(
        MonoGameApp monoGameApp, 
        int backBufferWidth, 
        int backbufferHeight, 
        int outputResolutionWidth, 
        int outputResolutionHeight
    )
    {
        MonoGameApp = monoGameApp;

        if (monoGameApp.IsDisposed)
        {
            throw new ObjectDisposedException("Renderer cannot operate on/with a disposed MonoGameApp.");
        } 
            
        EffectManager = new(monoGameApp);        
        
        spriteBatch = new SpriteBatch(monoGameApp.GraphicsDevice);
        
        RendererSystem.SetBackBufferResolution(monoGameApp, backBufferWidth, backbufferHeight);
        SetFinalRenderTargetResolution(outputResolutionWidth, outputResolutionHeight);
        destinationRectangle = RendererSystem.CalculateRenderDestinationRectangle(monoGameApp, finalRenderTarget);

        textureManager = new TextureManager(monoGameApp);
        
        fontManager = new(monoGameApp);
        stringBuilder = new(Text4096.MaxCharacters);

        renderTargets = new();        

        LinkEvents();
    }

    public void SetFinalRenderTargetResolution(Vector2Int resolution)
    {
        SetFinalRenderTargetResolution(resolution.X, resolution.Y);
    }

    public void SetFinalRenderTargetResolution(int width, int height)
    {
        int clampedWidth = System.Math.Clamp(width, 1, int.MaxValue);  
        int clampedHeight = System.Math.Clamp(height, 1, int.MaxValue);  
        if(width == clampedWidth && height == clampedHeight)
        {            
            finalRenderTarget?.Dispose();
            finalRenderTarget = new RenderTarget2D(MonoGameApp.GraphicsDevice, width, height);
            outputResolution = new Vector2Int(width, height);
        }
        else
        {
            throw new ArgumentException($"Output resolution cannot be set to ({width}, {height}), values must be above zero and lower than or equal to int.MaxValue");            
        }
    }

    public void Windowed()
    {
        if(MonoGameApp.GraphicsDeviceManager.IsFullScreen == false)
        {
            return;
        }

        // NOTE:
        // this may need to be removed.
        // monoGameApp.GraphicsDeviceManager.HardwareModeSwitch = false;

        MonoGameApp.GraphicsDeviceManager.ToggleFullScreen();
    }

    public void Fullscreen()
    {
        if(MonoGameApp.GraphicsDeviceManager.IsFullScreen == true
        && MonoGameApp.GraphicsDeviceManager.HardwareModeSwitch == true)
        {
            return;
        }

        MonoGameApp.GraphicsDeviceManager.HardwareModeSwitch = true;
        MonoGameApp.GraphicsDeviceManager.ToggleFullScreen();


        // NOTE:
        // Monogame "corrupts" the computers back buffer when toggling fullscreen upon closing the application afterwards. 
        // The screen is fine for a split second then switches to the "Clear Colour" of the renderer.
        // nothing  can be clicked on the computer, alt+f4 doesnt work, it completely nukes the computer.
        
        // UpdateMainRenderDestinationRectangle(); <-- DONT DO THIS at the end of this, subscribe to the monogame OnGraphicsDeviceReset(object caller, EventArgs e).
    }

    public void BorderlessFullscreen()
    {
        if(MonoGameApp.GraphicsDeviceManager.IsFullScreen == true
        && MonoGameApp.GraphicsDeviceManager.HardwareModeSwitch == false)
        {
            return;
        }

        MonoGameApp.GraphicsDeviceManager.HardwareModeSwitch = false;
        MonoGameApp.GraphicsDeviceManager.ToggleFullScreen();
    }
    
    public void SetTargetFrameRate(TargetFrameRate targetFrameRate)
    {
        switch (targetFrameRate)
        {
            case TargetFrameRate.D60:
                MonoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(16);
            break;
            case TargetFrameRate.D90:
                MonoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(11);
            break;
            case TargetFrameRate.D120:
                MonoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(8);
            break;
            case TargetFrameRate.D144:
                MonoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(7);
            break;
            case TargetFrameRate.D165:
                MonoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(6f);
            break;
            case TargetFrameRate.D240:
                MonoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(4);
            break;
            case TargetFrameRate.D360:
                MonoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(3);
            break;
        }
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        MonoGameApp.GraphicsDevice.DeviceReset += OnGraphicsDeviceReset;
    }

    private void UnlinkEvents()
    {
        MonoGameApp.GraphicsDevice.DeviceReset -= OnGraphicsDeviceReset;        
    }

    private void OnGraphicsDeviceReset(object caller, EventArgs e)
    {
        // This ensures that the main render destination rectangle
        // will always be the correct size when the window's back buffer resizes;
        // including when toggling fullscreen and manually setting the back buffer.
        destinationRectangle = RendererSystem.CalculateRenderDestinationRectangle(MonoGameApp, finalRenderTarget);
    }

    ///
    /// Disposal.
    /// 

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if(disposed)
            return;

        if (disposing)
        {
            EffectManager.Dispose();
            TextureManager.Dispose();
            spriteBatch.Dispose();
            FinalRenderTarget.Dispose();
            UnlinkEvents();
        }

        GC.SuppressFinalize(this);
        disposed = true;
    }

    public void AllocateNewRenderTarget(IRendererState state, Vector2Int resolution, out GenIndex genIndex)
    {
        renderTargets.AllocateNew(new RenderTarget2D(MonoGameApp.GraphicsDevice, resolution.X, resolution.Y), out genIndex);
    }

    public Vector2 GetMouseWorldPosition(IMouse mouse)
    {
        ref readonly Camera camera = ref CameraSystem.MainCamera;
        Vector2Int renderTargetPosition = mouse.GetPositionRelative(destinationRectangle, OutputResolution);
        
        // offset by half the output resolution as the world camera (0,0) is at the center of the screen.
        Vector2 offset = new Vector2(outputResolution.X*0.5f, outputResolution.Y*0.5f);
        
        return new Vector2( 
            ((renderTargetPosition.X - offset.X)/camera.Zoom) + camera.Position.X,
            ((renderTargetPosition.Y - offset.Y)/camera.Zoom) - camera.Position.Y
        ).InvertY(); // invert y as world space in monogame is Y+ is down; where as howl engine is y+ is up.
    }

    public Vector2 GetMouseGuiPosition(IMouse mouse)
    {
        ref readonly Camera camera = ref CameraSystem.GuiCamera;
        Vector2Int renderTargetPosition = mouse.GetPositionRelative(destinationRectangle, new Vector2Int(finalRenderTarget.Width, finalRenderTarget.Height));        
        return new Vector2( 
            (renderTargetPosition.X + camera.Position.X)/camera.Zoom,
            (renderTargetPosition.Y + camera.Position.Y)/camera.Zoom
        );
    }

    public GenIndexResult DeallocateRenderTarget(GenIndex genIndex)
    {
        renderTargets.Deallocate(genIndex).Ok(out var result);
        return result;
    }

    ~RendererState()
    {
        Dispose(false);
    }
}