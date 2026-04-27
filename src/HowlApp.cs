using System;
using Howl.Debug;
using Howl.Ecs;
using Howl.Graphics;
using Howl.Input;
using Howl.LevelManagement;
using Howl.LevelManagement.Ldtk;
using Howl.Text;
using Howl.Vendors.MonoGame;

namespace Howl;

public unsafe class HowlApp
{




    /*******************
    
        Constants.
    
    ********************/




    public const float FixedDt = 1f / 60f;



    /*******************
    
        Member Variables.
    
    ********************/




    /// <summary>
    ///     Gets the EcsState.
    /// </summary>
    public EcsState EcsState;

    /// <summary>
    ///     The Monogame Application state used as a pump for this HowlApp.
    /// </summary>
    public MonoGameAppState MonoGameAppState;

    /// <summary>
    ///     The LdtkParserState used for parsing ldtk level files.
    /// </summary>
    public LdtkParserState LdtkParserState;

    /// <summary>
    ///     The registry state storing all strings.
    /// </summary>
    public StringRegistryState StringRegistryState;

    /// <summary>
    ///     The current fixed update step time.
    /// </summary>
    public float FixedUpdateTime = 0;

    /// <summary>
    ///     Whether or not this instance has been dispose of.
    /// </summary>
    public bool IsDisposed;




    /*******************
    
        Debug Diagnostics.
    
    ********************/




    /// <summary>
    /// Gets and sets the update-step stopwatch.
    /// </summary>
    public System.Diagnostics.Stopwatch UpdateStepStopwatch;    

    /// <summary>
    /// Gets and sets the fixed-update-step stopwatch.
    /// </summary>
    public System.Diagnostics.Stopwatch FixedUpdateStepStopwatch;

    /// <summary>
    ///     The draw-step stopwatch.
    /// </summary>
    public System.Diagnostics.Stopwatch DrawStepStopwatch;




    /*******************
    
        Callbacks.
    
    ********************/




    /// <summary>
    ///     The initialise callback.
    /// </summary>
    public Action Initialise;

    /// <summary>
    ///     The update callback.
    /// </summary>
    public Action<float> UpdateCallback;

    /// <summary>
    ///     The fixed update callback.
    /// </summary>
    public Action<float> FixedUpdateCallback;

    /// <summary>
    ///     The draw callback.
    /// </summary>
    public Action<float> DrawCallback;




    /*******************
    
        Functions.
    
    ********************/




    /// <summary>
    ///     Creates a new HowlApp instance.
    /// </summary>
    /// <param name="maxEntities">the maximum number of entities.</param>
    public HowlApp(int maxEntities)
    {
        // instantiate debug stop watches.
        UpdateStepStopwatch         = new();
        FixedUpdateStepStopwatch    = new();
        DrawStepStopwatch           = new();
        EcsState = new EcsState(maxEntities);
    }

    /// <summary>
    ///     Updates/Ticks a HowlApp forward by a set amount of time.
    /// </summary>
    /// <param name="deltaTime">the specified amount of time to tick forwards by.</param>
    public static void Update(HowlApp app, float deltaTime)
    {
        app.UpdateStepStopwatch.Restart();
        
        InputManager.Update(app);
        app.UpdateCallback(deltaTime);

        app.UpdateStepStopwatch.Stop();

        // try fixed update.
        app.FixedUpdateTime += deltaTime;
        if(app.FixedUpdateTime >= FixedDt)
        {            
            app.FixedUpdateStepStopwatch.Restart();
            
            // iterate and do fixed update steps.
            while (app.FixedUpdateTime >= FixedDt)
            {
                app.FixedUpdateCallback(FixedDt);
                app.FixedUpdateTime -= FixedDt;
            }

            app.FixedUpdateStepStopwatch.Stop();
        }
    }

    /// <summary>
    ///     Initialises the MonoGame backend of the howl app.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="resolution"></param>
    public static void InitialiseMonoGame(HowlApp app, Resolution resolution, int maxTextures, int maxFonts, int debugDrawMaxPolygons)
    {
        if (app.MonoGameAppState == null)
        {
            app.MonoGameAppState = new(resolution.BackBufferWidth, resolution.BackBufferHeight, resolution.OutputWidth, 
                resolution.OutputHeight, maxTextures, maxFonts, debugDrawMaxPolygons
            );
            MonoGameAppState state = app.MonoGameAppState; 
            
            // THIS WILL NEED TO CHANGE.
            
            state.UpdateCallback += (float deltaTime) =>
            {
                Update(app, deltaTime);
            };

            state.RenderCallback += (float deltaTime) =>
            {
                Vendors.MonoGame.Graphics.RendererSystem.Draw(app);  
                app.DrawCallback?.Invoke(deltaTime);
            };
        }
        else
        {
            Log.WriteLine(LogType.Warn, "Multiple Initialisations of MonoGame backend occured.");
        }
    }

    /// <summary>
    ///     Initialises the Ldtk backend of the howl app.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="parseLevelIntGrid"></param>
    /// <param name="scratchBufferSizeInMb"></param>
    /// <param name="pixelsPerUnit"></param>
    public static void IntialiseLdtk(HowlApp app, delegate* <HowlApp, IntGridView, void> parseLevelIntGrid, float scratchBufferSizeInMb, int pixelsPerUnit)
    {
        app.LdtkParserState = new LdtkParserState(parseLevelIntGrid, scratchBufferSizeInMb, pixelsPerUnit);
    }

    /// <summary>
    ///     Initialises a string registry to manage memory for strings.
    /// </summary>
    /// <param name="app">The howl app instance to intialise.</param>
    /// <param name="maxStringCharacters">the maximum amount of characters a string can have.</param>
    public static void IntialiseStringRegistry(HowlApp app, int maxStringCharacters)
    {
        app.StringRegistryState = new(maxStringCharacters);
    }

    /// <summary>
    ///     Shutsdown a howl application.
    /// </summary>
    /// <param name="app"></param>
    public static void Shutdown(HowlApp app)
    {
        app.MonoGameAppState?.Exit();
    }

    /// <summary>
    ///     Runs a howl application.
    /// </summary>
    /// <param name="app"></param>
    public static void Run(HowlApp app)
    {
        app.MonoGameAppState?.Run();
    }

    /// <summary>
    ///     Disposes of a howl app.
    /// </summary>
    /// <param name="app"></param>
    public static void Dispose(HowlApp app)
    {
        if (app.IsDisposed)
        {
            return;
        }
        
        app.IsDisposed = true;
        app.MonoGameAppState?.Dispose();
        app.EcsState.Dispose();
        app.EcsState = null;
        app.UpdateStepStopwatch = null;
        app.FixedUpdateStepStopwatch = null;
        app.DrawStepStopwatch = null;
        app.UpdateCallback = null;
        app.FixedUpdateCallback = null;
        app.DrawCallback = null;
                
        if(app.LdtkParserState != null)
        {
            LdtkParserState.Dispose(app.LdtkParserState);
            app.LdtkParserState = null;
        }
        
        if(app.MonoGameAppState != null)
        {
            MonoGameApp.Dispose(app.MonoGameAppState);
            app.MonoGameAppState = null;
        }

        GC.SuppressFinalize(app);        
    }

    ~HowlApp()
    {
        Dispose(this);
    }
}