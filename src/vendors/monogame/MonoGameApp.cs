using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Howl.Vendors.MonoGame;

public class MonoGameApp : Game
{
    public GraphicsDeviceManager GraphicsDeviceManager {get; private set;}

    private HowlApp howlApp;

    private bool disposed = false;
    public bool IsDisposed => disposed;

    private void ValidateDependencies()
    {
        if (howlApp.IsDisposed)
        {
            throw new ObjectDisposedException("MonoGameApp cannot operate on/with a disposed HowlApp");
        }
    }

    public MonoGameApp(HowlApp howlApp)
    {
        this.howlApp = howlApp;
        IsMouseVisible = true;
        GraphicsDeviceManager = new(this);
        Initialize();
        Disposed += OnDisposed;
    }

    private void OnDisposed(object caller, EventArgs e)
    {
        disposed = true;
    }

    protected override void Initialize()
    {
        GraphicsDeviceManager.ApplyChanges();
        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        ValidateDependencies();
        float deltaTime = GameTimeToDeltaTime(gameTime);
        howlApp.Update(deltaTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        ValidateDependencies();
        float deltaTime = GameTimeToDeltaTime(gameTime);
        howlApp.Renderer.BeginDraw();
        howlApp.Draw(deltaTime); 
        howlApp.Renderer.EndDraw();

        // this submits to the gpu.
        // and should stay at the bottom.

        base.Draw(gameTime);
    }

    protected float GameTimeToDeltaTime(GameTime gameTime)
    {
        return (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}