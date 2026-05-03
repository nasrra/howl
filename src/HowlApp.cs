using System;
using Howl.Debug;
using Howl.Graphics;
using Howl.Input;
using Howl.LevelManagement;
using Howl.LevelManagement.Ldtk;
using Howl.Vendors.MonoGame;

namespace Howl;

public unsafe static class HowlApp
{




    /******************
    
        Constants.
    
    *******************/




    public const float FixedDt = 1f / 60f;




    /******************
    
        Functions.
    
    *******************/




    /// <summary>
    ///     Updates/Ticks a HowlApp forward by a set amount of time.
    /// </summary>
    /// <param name="deltaTime">the specified amount of time to tick forwards by.</param>
    public static void Update(HowlAppState state, float deltaTime)
    {
        // try fixed update.s
        state.FixedUpdateTime += deltaTime;
        if(state.FixedUpdateTime >= FixedDt)
        {            
            state.FixedUpdateStepStopwatch.Restart();
            
            // iterate and do fixed update steps.
            while (state.FixedUpdateTime >= FixedDt)
            {
                state.FixedUpdateCallback?.Invoke(FixedDt);
                state.FixedUpdateTime -= FixedDt;
            }

            state.FixedUpdateStepStopwatch.Stop();
        }

        state.UpdateStepStopwatch.Restart();
        
        UpdateInputContext(state);
        state.UpdateCallback?.Invoke(deltaTime);

        state.UpdateStepStopwatch.Stop();
    }

    /// <summary>
    ///     Initialises the MonoGame backend of the howl app.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="resolution"></param>
    public static void InitialiseMonoGame(HowlAppState state, Resolution resolution, int maxTextures, int maxFonts, int debugDrawMaxPolygons)
    {
        if (state.MonoGameAppState == null)
        {
            state.MonoGameAppState = new(resolution.BackBufferWidth, resolution.BackBufferHeight, resolution.OutputWidth, 
                resolution.OutputHeight, maxTextures, maxFonts, debugDrawMaxPolygons
            );
            MonoGameAppState monoGame = state.MonoGameAppState; 
            
            // THIS WILL NEED TO CHANGE.
            
            monoGame.UpdateCallback += (float deltaTime) =>
            {
                Update(state, deltaTime);
            };

            monoGame.RenderCallback += (float deltaTime) =>
            {
                state.DrawCallback?.Invoke(deltaTime);
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
    public static void IntialiseLdtk(HowlAppState app, delegate* <HowlAppState, IntGridView, void> parseLevelIntGrid, float scratchBufferSizeInMb, int pixelsPerUnit)
    {
        app.LdtkParserState = new LdtkParserState(parseLevelIntGrid, scratchBufferSizeInMb, pixelsPerUnit);
    }

    /// <summary>
    ///     Shutsdown a state instance.
    /// </summary>
    /// <param name="state"></param>
    public static void Shutdown(HowlAppState state)
    {
        state.MonoGameAppState?.Exit();
    }

    /// <summary>
    ///     Runs a state instance.
    /// </summary>
    /// <param name="state"></param>
    public static void Run(HowlAppState state)
    {
        state.MonoGameAppState?.Run();
    }

    /// <summary>
    ///     Updates a state instance's input context.
    /// </summary>
    /// <param name="app">the state instance to update.</param>
    public static void UpdateInputContext(HowlAppState state)
    {
        Vendors.MonoGame.Input.InputManager.Update(state.MonoGameAppState.InputManagerState);
    }

    /// <summary>
    ///     Disposes of a state instance.
    /// </summary>
    /// <param name="state">the state instance to dispose of.</param>
    public static void Dispose(HowlAppState state)
    {
        if (state.IsDisposed)
        {
            return;
        }
        
        state.IsDisposed = true;
        state.MonoGameAppState?.Dispose();
        state.UpdateStepStopwatch = null;
        state.FixedUpdateStepStopwatch = null;
        state.DrawStepStopwatch = null;
        state.UpdateCallback = null;
        state.FixedUpdateCallback = null;
        state.DrawCallback = null;
                
        if(state.LdtkParserState != null)
        {
            LdtkParserState.Dispose(state.LdtkParserState);
            state.LdtkParserState = null;
        }
        
        if(state.MonoGameAppState != null)
        {
            MonoGameApp.Dispose(state.MonoGameAppState);
            state.MonoGameAppState = null;
        }

        GC.SuppressFinalize(state);        
    }
}