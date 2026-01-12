using System.Diagnostics;
using Howl.Graphics;
using Howl.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Howl;

public class HowlApp : Game
{
    public static HowlApp Instance {get; private set;}

    public static new GraphicsDevice GraphicsDevice {get; private set;}

    public static GraphicsDeviceManager GraphicsDeviceManager {get; private set;}
    
    public static Renderer Renderer {get; private set;}

    public static InputManager InputManager {get; private set;}

    public HowlApp()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.WriteLine("[Error]: there can only be one Howl App Instance.");   
        }
        GraphicsDeviceManager = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        GraphicsDevice = base.GraphicsDevice;
        GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        GraphicsDeviceManager.PreferredBackBufferHeight = 720;
        GraphicsDeviceManager.ApplyChanges();
        Renderer = new(1, 1920, 1080);
        InputManager = new();
        base.Initialize();
    }

    protected sealed override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        InputManager.Update(deltaTime);
        Update(deltaTime);
        base.Update(gameTime);
    }

    
    protected sealed override void Draw(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Renderer.BeginDraw();
        Draw(deltaTime);
        Renderer.EndDraw();
        // this submits to the gpu.
        // and should stay at the bottom.

        base.Draw(gameTime);
    }

    protected virtual void Draw(float deltaTime)
    {
    }
    
    protected virtual void Update(float deltaTime)
    {
    }


    protected virtual void InitialiseShaders()
    {
    }
}
