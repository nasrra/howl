using System;
using Howl.ECS;
using Howl.Graphics;
using Howl.Input;

namespace Howl;

public unsafe sealed class HowlApp : IDisposable
{
    /// <summary>
    /// Gets the InputManager used by this HowlApp.
    /// </summary>
    public IInputManager InputManager {get; private set;}

    /// <summary>
    /// Gets the renderer state.
    /// </summary>
    public IRendererState RendererState {get; private set;}

    /// <summary>
    /// gets the GenIndexAllocator used by this HowlApp.
    /// </summary>
    public GenIndexAllocator GenIndexAllocator {get; private set;}

    /// <summary>
    /// Gets the ComponentRegistry used for gameplay.
    /// </summary>
    public ComponentRegistry ComponentRegistry {get; private set;}


    /// <summary>
    /// Gets and sets the update-step stopwatch.
    /// </summary>
    public System.Diagnostics.Stopwatch UpdateStepStopwatch;    

    /// <summary>
    /// Gets and sets the fixed-update-step stopwatch.
    /// </summary>
    public System.Diagnostics.Stopwatch FixedUpdateStepStopwatch;

    /// <summary>
    /// Gets and sets the draw-step stopwatch.
    /// </summary>
    public System.Diagnostics.Stopwatch DrawStepStopwatch;

    /// <summary>
    /// Gets and sets the initialise procedures.
    /// </summary>
    public delegate*<void> Initialise;

    /// <summary>
    /// Gets and sets the update procedures.
    /// </summary>
    public delegate*<float, void> Update;

    /// <summary>
    /// Gets and sets the fixed update procedures.
    /// </summary>
    public delegate*<float, void> FixedUpdate;

    /// <summary>
    /// Gets and sets the draw procedures.
    /// </summary>
    public delegate*<float, void> Draw;

    /// <summary>
    /// Gets and sets the draw procedures for the renderering backend.
    /// </summary>
    private delegate*<ComponentRegistry, IRendererState, void> RendererDraw;

    /// <summary>
    /// Gets and sets the monogame app.
    /// </summary>
    /// <remarks>
    /// Note: the monogame app is only intialised if the howl app was created with the monogame backend.
    /// </remarks>
    private Howl.Vendors.MonoGame.MonoGameApp monoGameApp;

    /// <summary>
    /// Gets the monogame app.
    /// </summary>
    /// <remarks>
    /// Note: the monogame app is only intialised if the howl app was created with the monogame backend.
    /// </remarks>
    public Howl.Vendors.MonoGame.MonoGameApp MonoGameApp => monoGameApp;

    private const float FixedDt = 1f / 60f;
    private float fixedUpdateTime = 0;

    private bool disposed = false;
    public bool IsDisposed => disposed;

    public HowlApp(RendererBackend rendererBackend, Resolution resolution)
    {
        // instantiate debug stop watches.
        UpdateStepStopwatch         = new();
        FixedUpdateStepStopwatch    = new();
        DrawStepStopwatch           = new();
    
        GenIndexAllocator = new();
        ComponentRegistry = new(GenIndexAllocator);

        switch (rendererBackend)
        {
            case RendererBackend.MonoGame:
                monoGameApp = new(this);
                InputManager = new Vendors.MonoGame.Input.InputManager();
                var rendererState = new Vendors.MonoGame.Graphics.RendererState(monoGameApp, resolution);
                RendererState = rendererState;
                RendererDraw = &Vendors.MonoGame.Graphics.RendererSystem.Draw;
                Run = monoGameApp.Run;
                Shutdown = monoGameApp.Exit;
                break;
            default:
                throw new Exception();
        }   
    }

    public Action Run {get; private set;}
    public Action Shutdown {get; private set;}

    public void Tick(float deltaTime)
    {
        UpdateStepStopwatch.Restart();
        
        InputManager.Update(deltaTime);
        Update(deltaTime);

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

    public void Render(float deltaTime)
    {
        Draw(deltaTime);
        RendererDraw(ComponentRegistry, RendererState);
    }

    public void Dispose()
    {
        Dispose(true);        
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            System.Diagnostics.Debug.Assert(false);
            return;
        }

        if (disposing)
        {
            GenIndexAllocator.Dispose();
            GenIndexAllocator = null;

            RendererState.Dispose();
            RendererState = null;

            ComponentRegistry.Dispose();
            ComponentRegistry = null;

            monoGameApp?.Dispose();
            monoGameApp = null;

            UpdateStepStopwatch = null;
            FixedUpdateStepStopwatch = null;
            DrawStepStopwatch = null;
        }

        disposed = true;
        GC.SuppressFinalize(this);
    }

    ~HowlApp()
    {
        Dispose(false);
    }
}