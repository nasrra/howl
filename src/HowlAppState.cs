using System;
using Howl.Ecs;
using Howl.LevelManagement;
using Howl.Text;
using Howl.Vendors.MonoGame;

namespace Howl;

public class HowlAppState
{




    /******************
    
        Callbacks
    
    *******************/




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




    /******************
    
        Diagnostics.
    
    *******************/




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




    /******************
    
        Member Variables.
    
    *******************/




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




    /******************
    
        Constructor and Finaliser.
    
    *******************/




    /// <summary>
    ///     Creates a new HowlAppState instance.
    /// </summary>
    /// <param name="maxEntities">the maximum number of entities.</param>
    public HowlAppState(int maxEntities)
    {
        // instantiate debug stop watches.
        UpdateStepStopwatch         = new();
        FixedUpdateStepStopwatch    = new();
        DrawStepStopwatch           = new();
        EcsState = new EcsState(maxEntities);
    }

    ~HowlAppState()
    {
        HowlApp.Dispose(this);
    }    
}