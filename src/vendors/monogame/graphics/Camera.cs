
using System;
using Howl.Graphics;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework;


namespace Howl.Vendors.MonoGame.Graphics;

public class Camera : ICamera
{

    private Matrix projectionMatrix;
    public Howl.Math.Matrix ProjectionMatrix 
    { 
        get => projectionMatrix.ToHowl(); 
        set => value.ToMonoGame(); 
    }

    private Howl.Math.Vector2 position;
    public Howl.Math.Vector2 Position
    {
        get => position;
        set => position = value;
    }

    private Howl.Math.Vector2 extents;
    public Howl.Math.Vector2 Extents {get;}

    private float zoom = 1;
    public float Zoom
    {
        get => zoom;
        set => zoom = System.Math.Clamp(value, ICamera.MinZoom, ICamera.MaxZoom);
    }
    
    private float zoomVirtualHeight;
    public float ZoomVirtualHeight
    {
        get => zoomVirtualHeight;
        set => zoomVirtualHeight = value;
    }

    private CoordinateSpace coordinateSpace;
    public CoordinateSpace CoordinateSpace
    {
        get => coordinateSpace;
        set => coordinateSpace = value;
    }

    private MonoGameApp monoGameApp;
    private Renderer renderer;

    private bool disposed;
    public bool IsDisposed => disposed;


    /// <summary>
    /// Creates a new MonoGame camera instance.
    /// </summary>
    /// <param name="monoGameApp">The MonoGame app that is used as the HowlApp backend.</param>
    /// <param name="renderer">The MonoGame renderer that is in use by the HowlApp.</param>
    /// <param name="position">The position to be placed at.</param>
    /// <param name="zoomVirtualHeight">The base height - in world units - for the default zoom level.</param>
    /// <param name="coordinateSpace">The coordinate space to project in.</param>
    public Camera(MonoGameApp monoGameApp, Renderer renderer, Howl.Math.Vector2 position, float zoomVirtualHeight, CoordinateSpace coordinateSpace)
    {
        this.monoGameApp = monoGameApp;
        this.renderer = renderer;
        this.position = position;
        this.zoomVirtualHeight = zoomVirtualHeight;
        this.coordinateSpace = coordinateSpace;
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


        // Note:
        // Up is y+ and right is x+;
        // Centered orthographic projection

        // Compute half-width and half-height in world units based on virtual resolution
        float halfHeight = (zoomVirtualHeight * 0.5f) / zoom;
        float halfWidth = halfHeight * renderer.OutputResolutionAspectRatio; // keep aspect ratio correct
        extents = new(halfWidth*2, halfHeight*2);

        switch (coordinateSpace)
        {
            case CoordinateSpace.Cartesian:
                extents = new(halfWidth*2, halfHeight*2);
                projectionMatrix = Matrix.CreateOrthographicOffCenter(
                    -halfWidth,  halfWidth,
                    halfHeight, -halfHeight,
                    float.Epsilon,
                    float.MaxValue
                );
                break;
            case CoordinateSpace.Rasterized:
                float height = halfHeight * 2;
                float width = halfWidth * 2;
                extents = new(width, height);
                projectionMatrix = Matrix.CreateOrthographicOffCenter(
                    0,  width,
                    height, 0,
                    float.Epsilon,
                    float.MaxValue
                );
                break;
            default:
                throw new InvalidOperationException($"Camera does not support coordinate space: '{coordinateSpace}'");
        }
        
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