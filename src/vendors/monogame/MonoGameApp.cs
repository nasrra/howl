using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        IsFixedTimeStep = false;
        GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
        GraphicsDeviceManager.ApplyChanges();
        
        // set this to the same directory as the csproj as loading is handled via full paths fomr AssetManager.
        Content.RootDirectory = "";
        
        Disposed += OnDisposed;
    }

    private void OnDisposed(object caller, EventArgs e)
    {
        disposed = true;
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
        // this submits to the gpu.
        // and should stay at the bottom.

        base.Draw(gameTime);

        ValidateDependencies();
        float deltaTime = GameTimeToDeltaTime(gameTime);
        
        howlApp.Renderer.BeginDraw();
        howlApp.Draw(deltaTime); 
        howlApp.Renderer.EndDraw();
        
        howlApp.Renderer.BeginDrawGui();
        howlApp.DrawGui(deltaTime);
        howlApp.Renderer.EndDrawGui();

        howlApp.Renderer.SubmitDraw();
    }

    protected float GameTimeToDeltaTime(GameTime gameTime)
    {
        return (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}