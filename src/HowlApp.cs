using System;
using System.Diagnostics;
using Howl.Graphics;
using Howl.Input;
using Howl.MonoGame;
using Howl.MonoGame.Graphics;

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
    public InputManager InputManager {get; private set;}

    private MonoGameApp monoGameApp;
    private HowlAppBackend backend;

    public HowlApp(HowlAppBackend howlAppBackend)
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
        InitialiseBackend();
        Initialise();
    }

    /// <summary>
    /// Initialises the backend used for the App.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void InitialiseBackend()
    {
        switch (backend)
        {
            case HowlAppBackend.MonoGame:
                InitialiseMonoGameBackend();
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
                ShutdownMonoGameBackend();
            break;
        }
        Dispose();        
    }

    /// <summary>
    /// Draw function for the HowlApp.
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void Draw(float deltaTime)
    {
    }
    
    /// <summary>
    /// Update tick for the HowlApp.
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void Update(float deltaTime)
    {
        InputManager.Update(deltaTime);
    }

    // public virtual void InitialiseShaders()
    // {
    // }


    ///
    /// Monogame Backend.
    /// 


    private void InitialiseMonoGameBackend()
    {
        monoGameApp = new(new(this));
        InputManager = new();
        Renderer = new MonoGameRenderer(new(monoGameApp));
    }

    private void RunMonoGameBackend()
    {
        monoGameApp.Run();
    } 

    private void ShutdownMonoGameBackend()
    {
        monoGameApp.Exit();
    }

    public void Dispose()
    {
    }

}
