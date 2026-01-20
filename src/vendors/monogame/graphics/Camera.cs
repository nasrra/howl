
using System;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Howl.Vendors.MonoGame.Graphics;

public class Camera : ICamera
{
    private Howl.Math.Vector3 position;
    public Howl.Math.Vector3 Position
    {
        get => position;
        set => position = value;
    }

    private float zoom;
    public float Zoom
    {
        get => zoom;
        set => zoom = value;
    }

    private Matrix projectionMatrix;
    public Howl.Math.Matrix ProjectionMatrix 
    { 
        get => projectionMatrix.ToHowl(); 
        set => value.ToMonoGame(); 
    }

    private MonoGameApp monoGameApp;

    private bool disposed;
    public bool IsDisposed => disposed;

    public Camera(MonoGameApp monoGameApp)
    {
        this.monoGameApp = monoGameApp;
    }

    public Camera(MonoGameApp monoGameApp, Howl.Math.Vector3 position)
    {
        this.monoGameApp = monoGameApp;
        this.position = position;
    }

    private void ValidateDependencies()
    {
        if (monoGameApp.IsDisposed)
        {
            throw new ObjectDisposedException("Camera cannot operate on/with a diposed MonoGameApp.");
        }
    }

    public void Update()
    {
        ValidateDependencies();
        
        // Note:
        // Up is y+ and right is x+;
        Viewport viewport = monoGameApp.GraphicsDevice.Viewport;
        projectionMatrix = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, float.Epsilon, float.MaxValue);
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            
        }

        disposed = true;
    }

    ~Camera()
    {
        Dispose(false);       
    }
}