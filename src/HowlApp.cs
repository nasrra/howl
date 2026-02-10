using System;
using System.Diagnostics;
using Howl.ECS;
using Howl.Graphics;
using Howl.Input;

namespace Howl;

public abstract class HowlApp : IDisposable
{
    public static HowlApp Instance {get; private set;}

    /// <summary>
    /// Gets the InputManager used by this HowlApp.
    /// </summary>
    public IInputManager InputManager {get; private set;}

    /// <summary>
    /// Gets the renderer state.
    /// </summary>
    public IRendererState RendererState {get; private set;}

    /// <summary>
    /// Gets the ComponentRegistry used for gameplay.
    /// </summary>
    public ComponentRegistry ComponentRegistry {get; private set;}

    /// <summary>
    /// Gets the SystemRegistry used by this HowlApp.
    /// </summary>
    public SystemRegistry SystemRegistry {get; private set;}

    /// <summary>
    /// Gets and sets the update-step stopwatch.
    /// </summary>
    public Stopwatch UpdateStepStopwatch;    

    /// <summary>
    /// Gets and sets the fixed-update-step stopwatch.
    /// </summary>
    public Stopwatch FixedUpdateStepStopwatch;

    /// <summary>
    /// Gets and sets the draw-step stopwatch.
    /// </summary>
    public Stopwatch DrawStepStopwatch;

    /// <summary>
    /// gets the GenIndexAllocator used by this HowlApp.
    /// </summary>
    public GenIndexAllocator GenIndexAllocator {get; private set;}

    private Vendors.MonoGame.MonoGameApp monoGameApp;
    private HowlAppBackend backend;

    private DrawSystem drawSystem;

    private bool disposed = false;
    public bool IsDisposed => disposed;

    public HowlApp(HowlAppBackend howlAppBackend, Resolution resolution)
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

        GenIndexAllocator = new();
        ComponentRegistry = new(GenIndexAllocator);
        SystemRegistry = new();

        // instantiate debug stop watches.
        UpdateStepStopwatch         = new();
        FixedUpdateStepStopwatch    = new();
        DrawStepStopwatch           = new();

        backend = howlAppBackend;
        InitialiseBackend(resolution);
        Initialise();
    }

    /// <summary>
    /// Initialises the backend used for the App.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void InitialiseBackend(Resolution resolution)
    {
        switch (backend)
        {
            case HowlAppBackend.MonoGame:
                InitialiseMonoGameBackend(resolution);
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
        drawSystem(deltaTime);
    }

    /// <summary>
    /// Draw function for the HowlApp.
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void DrawGui(float deltaTime)
    {   
        SystemRegistry.DrawGui(deltaTime);
    }

    private const float FixedDt = 1f / 60f;
    private float fixedUpdateTime = 0;
    
    /// <summary>
    /// Update tick for the HowlApp.
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void Update(float deltaTime)
    {
        UpdateStepStopwatch.Restart();

        InputManager.Update(deltaTime);
        SystemRegistry.Update(deltaTime);

        UpdateStepStopwatch.Stop();

        // try fixed update.
        fixedUpdateTime += deltaTime;
        if(fixedUpdateTime >= FixedDt)
        {            
            FixedUpdateStepStopwatch.Restart();
            
            // iterate and do fixed update steps.
            while (fixedUpdateTime >= FixedDt)
            {
                FixedUpdate(FixedDt);
                fixedUpdateTime -= FixedDt;
            }

            FixedUpdateStepStopwatch.Stop();
        }
    }

    public virtual void FixedUpdate(float deltaTime)
    {
        SystemRegistry.FixedUpdate(deltaTime);
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


    private void InitialiseMonoGameBackend(Resolution resolution){
        monoGameApp = new(this);
        InputManager = new Vendors.MonoGame.Input.InputManager();
        var rendererState = new Vendors.MonoGame.Graphics.RendererState(monoGameApp, resolution);
        RendererState = rendererState;
        drawSystem = Vendors.MonoGame.Graphics.RendererSystem.DrawSystem(ComponentRegistry, rendererState);
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
                monoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(16);
            break;
            case TargetFrameRate.D90:
                monoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(11);
            break;
            case TargetFrameRate.D120:
                monoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(8);
            break;
            case TargetFrameRate.D144:
                monoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(7);
            break;
            case TargetFrameRate.D165:
                monoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(6f);
            break;
            case TargetFrameRate.D240:
                monoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(4);
            break;
            case TargetFrameRate.D360:
                monoGameApp.TargetElapsedTime = TimeSpan.FromMilliseconds(3);
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
            ComponentRegistry?.Dispose();
            Instance = null;
            UpdateStepStopwatch = null;
            FixedUpdateStepStopwatch = null;
            DrawStepStopwatch = null;
        }

        disposed = true;
    }

    ~HowlApp()
    {
        Dispose(false);
    }

}
