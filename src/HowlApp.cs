using System;
using Howl.ECS;
using Howl.Graphics;
using Howl.Input;

namespace Howl;

public abstract class HowlApp : IDisposable
{
    public static HowlApp Instance {get; private set;}

    /// <summary>
    /// Gets the renderer used by this HowlApp.
    /// </summary>
    public IRenderer Renderer {get; private set;}

    /// <summary>
    /// Gets the InputManager used by this HowlApp.
    /// </summary>
    public IInputManager InputManager {get; private set;}

    /// <summary>
    /// Gets the ComponentRegistry used by this HowlApp.
    /// </summary>
    public ComponentRegistry ComponentRegistry {get; private set;}

    /// <summary>
    /// Gets the SystemRegistry used by this HowlApp.
    /// </summary>
    public SystemRegistry SystemRegistry {get; private set;}

    /// <summary>
    /// gets the GenIndexAllocator used by this HowlApp.
    /// </summary>
    public GenIndexAllocator GenIndexAllocator {get; private set;}

    private Vendors.MonoGame.MonoGameApp monoGameApp;
    private HowlAppBackend backend;

    private bool disposed = false;
    public bool IsDisposed => disposed;

    public HowlApp(HowlAppBackend howlAppBackend, Resolution resolution, Math.Vector2 cameraPosition, float cameraZoomVirtualResolution)
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            throw new System.Exception("[Error]: there can only be one Howl App Instance.");   
        }

        if(howlAppBackend == HowlAppBackend.None)
        {
            throw new InvalidOperationException($"HowlApp cannot be created with a backend of {howlAppBackend}");
        }
        
        backend = howlAppBackend;
        InitialiseBackend(resolution, cameraPosition, cameraZoomVirtualResolution);
        GenIndexAllocator = new();
        ComponentRegistry = new(GenIndexAllocator);
        SystemRegistry = new();
        Initialise();
    }

    /// <summary>
    /// Initialises the backend used for the App.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void InitialiseBackend(Resolution resolution, Math.Vector2 cameraPosition, float cameraZoomVirtualResolution)
    {
        switch (backend)
        {
            case HowlAppBackend.MonoGame:
                InitialiseMonoGameBackend(resolution, cameraPosition, cameraZoomVirtualResolution);
            break;
            default:
                throw new InvalidOperationException($"HowlApp cannot be initialised with a backend of {backend}");
        }
    }

    /// <summary>
    /// Application Initialisation Code for Users.
    /// </summary>
    public abstract void Initialise();

    /// <summary>
    /// Runs the HowlApp.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Run()
    {
        switch (backend)
        {
            case HowlAppBackend.MonoGame:
                RunMonoGameBackend();
            break;
            default:
                throw new InvalidOperationException($"HowlApp cannot be run with a backend of {backend}");
        }
    }

    /// <summary>
    /// Shutsdown the HowlApp.
    /// </summary>
    public void Shutdown()
    {
        switch (backend)
        {
            case HowlAppBackend.MonoGame:
                ShutDownMonoGameBackend();
                Dispose();
            break;
            default:
                throw new InvalidOperationException($"HowlApp cannot be shutdonw with a backend of {backend}");
        }
        Dispose();        
    }

    /// <summary>
    /// Draw function for the HowlApp.
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void Draw(float deltaTime)
    {        
        SystemRegistry.Draw(deltaTime);
    }
    
    /// <summary>
    /// Update tick for the HowlApp.
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void Update(float deltaTime)
    {
        InputManager.Update(deltaTime);
        SystemRegistry.Update(deltaTime);
        fixedUpdateTime += deltaTime;
        if(fixedUpdateTime >= 0.016)
        {
            FixedUpdate(deltaTime);
            fixedUpdateTime = 0;
        }
    }

    private float fixedUpdateTime = 0;
    public virtual void FixedUpdate(float deltaTime)
    {
        SystemRegistry.FixedUpdate(deltaTime);
    }

    /// <summary>
    /// Sets the application window to be windowed.
    /// </summary>
    public void Windowed()
    {
        Renderer.Windowed();
    }

    /// <summary>
    /// Sets the application window to be fullscreen.
    /// </summary>
    public void Fullscreen()
    {
        Renderer.Fullscreen();
    }

    /// <summary>
    /// Sets the application window to be borderless fullscreen.
    /// </summary>
    public void BorderlessFullscreen()
    {
        Renderer.BorderlessFullscreen();
    }

    /// <summary>
    /// Sets the target frame rate.
    /// </summary>
    /// <param name="targetFrameRate">The specified target frame rate.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SetTargetFrameRate(TargetFrameRate targetFrameRate)
    {
        switch (backend)
        {
            case HowlAppBackend.MonoGame:
                SetTargetFrameRateMonoGameBackend(targetFrameRate);
            break;
            default:
                throw new InvalidOperationException($"HowlApp cannot set target frame rate with a backend of {backend}");
        }
    }


    ///
    /// MonoGameBackEnd.
    /// 


    private void InitialiseMonoGameBackend(Resolution resolution, Math.Vector2 cameraPosition, float cameraZoomVirtualResolution)
    {
        monoGameApp = new(this);
        InputManager = new Vendors.MonoGame.Input.InputManager();
        Renderer = new Vendors.MonoGame.Graphics.Renderer(monoGameApp, resolution, cameraPosition, cameraZoomVirtualResolution);

    }

    private void RunMonoGameBackend()
    {
        monoGameApp.Run();
    }

    private void ShutDownMonoGameBackend()
    {
        monoGameApp.Exit();        
    }

    private void SetTargetFrameRateMonoGameBackend(TargetFrameRate targetFrameRate)
    {
        switch (targetFrameRate)
        {
            case TargetFrameRate.D60:
                monoGameApp.TargetElapsedTime = TimeSpan.FromSeconds(1/60.0);
            break;
            case TargetFrameRate.D90:
                monoGameApp.TargetElapsedTime = TimeSpan.FromSeconds(1/90.0);
            break;
            case TargetFrameRate.D120:
                monoGameApp.TargetElapsedTime = TimeSpan.FromSeconds(1/120.0);
            break;
            case TargetFrameRate.D144:
                monoGameApp.TargetElapsedTime = TimeSpan.FromSeconds(1/144.0);
            break;
            case TargetFrameRate.D165:
                monoGameApp.TargetElapsedTime = TimeSpan.FromSeconds(1/165.0);
            break;
            case TargetFrameRate.D240:
                monoGameApp.TargetElapsedTime = TimeSpan.FromSeconds(1/240.0);
            break;
            case TargetFrameRate.D360:
                monoGameApp.TargetElapsedTime = TimeSpan.FromSeconds(1/360.0);
            break;
        }
    }


    /// 
    /// Disposal.
    /// 


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            Renderer?.Dispose();
            ComponentRegistry?.Dispose();
            Instance = null;
        }

        disposed = true;
    }

    ~HowlApp()
    {
        Dispose(false);
    }

}
