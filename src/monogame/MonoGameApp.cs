using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Howl.MonoGame;

public class MonoGameApp : Game
{
    public GraphicsDeviceManager GraphicsDeviceManager {get; private set;}

    private WeakReference<HowlApp> howlApp;

    public MonoGameApp(WeakReference<HowlApp> howlApp)
    {
        this.howlApp = howlApp;
        IsMouseVisible = true;
        GraphicsDeviceManager = new(this);
        Initialize();
    }

    protected override void Initialize()
    {
        GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        GraphicsDeviceManager.PreferredBackBufferHeight = 720;
        GraphicsDeviceManager.ApplyChanges();
        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        float deltaTime = GameTimeToDeltaTime(gameTime);
        if(howlApp.TryGetTarget(out HowlApp app)){
            app.Update(deltaTime);
        }
        else
        {
            throw new NullReferenceException("MonoGameApp cannot operate on assigned HowlApp as it is null");
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        float deltaTime = GameTimeToDeltaTime(gameTime);
        if(howlApp.TryGetTarget(out HowlApp app))
        {
            app.Renderer.BeginDraw();
            app.Draw(deltaTime); 
            app.Renderer.EndDraw();

            // this submits to the gpu.
            // and should stay at the bottom.

            base.Draw(gameTime);
        }
        else
        {
            throw new NullReferenceException("MonoGameApp cannot operate on assigned HowlApp as it is null");            
        }
    }

    protected float GameTimeToDeltaTime(GameTime gameTime)
    {
        return (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}