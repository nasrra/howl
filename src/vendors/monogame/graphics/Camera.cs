
using System;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Howl.Vendors.MonoGame.Graphics;

public class Camera : ICamera
{
    private Howl.Math.Vector2 position;
    public Howl.Math.Vector2 Position
    {
        get => position;
        set => position = value;
    }

    private float zoom = 1;
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
    private Renderer renderer;

    private bool disposed;
    public bool IsDisposed => disposed;

    public Camera(MonoGameApp monoGameApp, Renderer renderer)
    : this(monoGameApp, renderer, Howl.Math.Vector2.Zero)
    {
        
    }
    
    public Camera(MonoGameApp monoGameApp, Renderer renderer, Howl.Math.Vector2 position)
    {
        this.monoGameApp = monoGameApp;
        this.renderer = renderer;
        this.position = position;
    }

    private void ValidateDependencies()
    {
        if (monoGameApp.IsDisposed)
        {
            throw new ObjectDisposedException("Camera cannot operate on/with a diposed MonoGameApp.");
        }
        if (renderer.IsDisposed)
        {
            throw new ObjectDisposedException("Camera cannot operate on/with a disposed MonoGame Renderer.");
        }
    }

    public void Update()
    {
        ValidateDependencies();

        // Viewport viewport = monoGameApp.GraphicsDevice.Viewport;

        float halfWidth  = renderer.MainRenderTargetWidth  * 0.5f / zoom;
        float halfHeight = renderer.MainRenderTargetHeight * 0.5f / zoom;

        // Note:
        // Up is y+ and right is x+;
        // Centered orthographic projection
        projectionMatrix = Matrix.CreateOrthographicOffCenter(
            -halfWidth,  halfWidth,
            halfHeight, -halfHeight,
            float.Epsilon,
            float.MaxValue
        );
        // Viewport viewport = monoGameApp.GraphicsDevice.Viewport;
        // projectionMatrix = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, float.Epsilon, float.MaxValue);
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