using System;
using System.Collections.Generic;
using System.Diagnostics;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Vendors.MonoGame.Graphics;

public class Renderer : IRenderer
{
    Howl.Graphics.Color clearColor;
    public Howl.Graphics.Color ClearColor
    {
        get => clearColor;
        set => clearColor = value;
    }
    
    public EffectManager EffectManager { get; private set; }
    private SpriteBatch spriteBatch;

    TextureManager<Texture2D> textureManager;
    public ITextureManager TextureManager => textureManager;
    
    Camera camera;
    public ICamera Camera => camera;

    public Matrix ProjectionMatrix { get; private set; }
    
    public Rectangle DestinationRectangle { get; private set; }
    
    public RenderTarget2D RenderTarget { get; private set; }
    
    private List<VertexPositionColor> primitiveVertices;
    private List<short> primitiveIndices;

    private MonoGameApp monoGameApp;

    private bool disposed = false;
    public bool IsDisposed => disposed;

    public float MainRenderTargetWidth => RenderTarget.Width;

    public float MainRenderTargetHeight => RenderTarget.Height;

    public float MainRenderTargetAspectRatio => (float)RenderTarget.Width / RenderTarget.Height;

    /// <summary>
    /// Creates a new MonoGame renderer instance.
    /// </summary>
    /// <param name="monoGameApp">The MonoGame app that is used as the HowlApp backend.</param>
    /// <param name="resolution">The resolution of this application.</param>
    /// <param name="cameraPosition">The position to place the camera.</param>
    /// <param name="cameraZoomVirtualHeight">The base height - in world units - for the default camera zoom level.</param>
    public Renderer(
        MonoGameApp monoGameApp, 
        Resolution resolution,
        Howl.Math.Vector2 cameraPosition,
        float cameraZoomVirtualHeight
    )
    : this(
        monoGameApp, 
        cameraPosition,
        cameraZoomVirtualHeight,
        resolution.BackBufferWidth, 
        resolution.BackBufferHeight, 
        resolution.MainRenderTargetWidth, 
        resolution.MainRenderTargetHeight
    )
    {
        
    }

    /// <summary>
    /// Creates a new MonoGame Renderer instances.
    /// </summary>
    /// <param name="monoGameApp">The MonoGame app that is used as the HowlApp backend.</param>
    /// <param name="cameraPosition">The postion to place the camera at.</param>
    /// <param name="cameraZoomVirtualheight">The base height - in world units - for the default camera zoom level.</param>
    /// <param name="backBufferWidth">The back buffer width in pixels.</param>
    /// <param name="backbufferHeight">The back buffer height in pixels.</param>
    /// <param name="mainRenderTargetWidth">The main render target width in pixels.</param>
    /// <param name="mainRenderTargetHeight">The main render target height in pixels.</param>
    public Renderer(
        MonoGameApp monoGameApp, 
        Howl.Math.Vector2 cameraPosition,
        float cameraZoomVirtualheight,
        int backBufferWidth, 
        int backbufferHeight, 
        int mainRenderTargetWidth, 
        int mainRenderTargetHeight
    )
    {
        this.monoGameApp = monoGameApp;        

        ValidateDependencies();

        primitiveVertices = new();
        primitiveIndices = new();     
        
        EffectManager = new(monoGameApp);        
        
        spriteBatch = new SpriteBatch(monoGameApp.GraphicsDevice);
        
        SetBackBufferResolution(backBufferWidth, backbufferHeight);
        SetMainRenderTargetResolution(mainRenderTargetWidth, mainRenderTargetHeight);
        DestinationRectangle = CalculateDestinationRectangle(); 
        
        textureManager = new TextureManager(monoGameApp);
        camera = new Camera(monoGameApp, this, cameraPosition, cameraZoomVirtualheight);
    }

    private void ValidateDependencies()
    {
        if (monoGameApp.IsDisposed)
        {
            throw new ObjectDisposedException("Renderer cannot operate on/with a disposed MonoGameApp.");
        }
    }

    public void SetResolution(Resolution resolution)
    {
        SetBackBufferResolution(resolution.BackBufferWidth, resolution.BackBufferHeight);
        SetMainRenderTargetResolution(resolution.MainRenderTargetWidth, resolution.MainRenderTargetHeight);    
    }

    public void SetBackBufferResolution(Howl.Math.Vector2Int resolution)
    {
        SetBackBufferResolution(resolution.X, resolution.Y);
    }
    
    public void SetBackBufferResolution(int width, int height)
    {        
        ValidateDependencies();
        int clampedWidth = System.Math.Clamp(width, 1, int.MaxValue);  
        int clampedHeight = System.Math.Clamp(height, 1, int.MaxValue);  
        if(width == clampedWidth && height == clampedHeight)
        {
            monoGameApp.GraphicsDeviceManager.PreferredBackBufferHeight = height;
            monoGameApp.GraphicsDeviceManager.PreferredBackBufferWidth = width;
            monoGameApp.GraphicsDeviceManager.ApplyChanges();
        }
        else
        {
            throw new InvalidOperationException($"BackBuffer resolution cannot be set to ({width}, {height}), values must be above zero and lower than or equal to int.MaxValue");            
        }
    }

    public void SetMainRenderTargetResolution(Howl.Math.Vector2Int resolution)
    {
        SetMainRenderTargetResolution(resolution.X, resolution.Y);
    }

    public void SetMainRenderTargetResolution(int width, int height)
    {        
        ValidateDependencies();
        int clampedWidth = System.Math.Clamp(width, 1, int.MaxValue);  
        int clampedHeight = System.Math.Clamp(height, 1, int.MaxValue);  
        if(width == clampedWidth && height == clampedHeight)
        {            
            if(RenderTarget != null)
            {
                RenderTarget.Dispose();
            }
            RenderTarget = new RenderTarget2D(monoGameApp.GraphicsDevice, width, height);
        }
        else
        {
            throw new InvalidOperationException($"RenderTarget resolution cannot be set to ({width}, {height}), values must be above zero and lower than or equal to int.MaxValue");            
        }
    }

    public void BeginDraw()
    {   
        ValidateDependencies();

        // draw all sprites to a render target.

        monoGameApp.GraphicsDevice.SetRenderTarget(RenderTarget);
        
        // ensure that the projection matrix is relative to the render target
        // being drawn to and not the actual backbuffer dimensions of the program.
        UpdateProjectionMatrix();
        
        monoGameApp.GraphicsDevice.Clear(clearColor.ToMonoGame());

        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointWrap, 
            rasterizerState: RasterizerState.CullNone, 
            effect: EffectManager.DefaultSpriteEffect
        );   
    }

    public void EndDraw()
    {
        ValidateDependencies();

        // present the render target over the windows back buffer.

        spriteBatch.End();

        // Note: 
        // Primitive drawing should always be outside of a sprite bath begin and end call.
        // Primitive drawing within those states causes undefined behvaiour.

        DrawPrimitives();

        // Note: when setting the render target to null, monogame draws directly to the backbuffer.

        monoGameApp.GraphicsDevice.SetRenderTarget(null);
    

        // draw the render target back to the back buffer.
        
        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointWrap
        );

        spriteBatch.Draw(
            RenderTarget,
            DestinationRectangle, // ensure to properly scale the render target to fit within the window's back buffer.
            Microsoft.Xna.Framework.Color.White
        );

        spriteBatch.End();
    }

    /// <summary>
    // Calculates ans sets the new projection matix so that -y is down and -x is left.
    /// </summary>
    private void UpdateProjectionMatrix()
    {
        ValidateDependencies();

        camera.Update();

        // update effects to use the new projection matrix.
        
        EffectManager.UpdateProjectionMatrix(camera.ProjectionMatrix.ToMonoGame());
    }

    /// <summary>
    /// Draws all stored primitive shapes to the next frame/screen, clearing the internal primitives cache when drawn for the frame/screen after. 
    /// </summary>
    private void DrawPrimitives()
    {
        ValidateDependencies();

        if(primitiveIndices.Count == 0 && primitiveVertices.Count == 0)
        {
            return;
        }

        if(monoGameApp.GraphicsDevice == null)
        {
            return;
        }

        foreach(EffectPass pass in EffectManager.PrimitivesEffect.CurrentTechnique.Passes)
        {
            pass.Apply();            
            monoGameApp.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                PrimitiveType.TriangleList,
                primitiveVertices.ToArray(),
                0,
                primitiveVertices.Count,
                primitiveIndices.ToArray(),
                0,
                primitiveIndices.Count / 3
            );
        }

        // clear cache so primitive data is not persistent between draw calls/frames.

        primitiveVertices.Clear();
        primitiveIndices.Clear();
    }

    /// <summary>
    /// Calculates the detination rectangle for the render target onto the backbuffer of the window this application is painting to.
    /// </summary>
    /// <returns>The calculated destination rectangle.</returns>
    private Rectangle CalculateDestinationRectangle()
    {
        ValidateDependencies();

        Rectangle backbufferBounds = monoGameApp.GraphicsDevice.PresentationParameters.Bounds;
        float backbufferAspectRatio = (float)backbufferBounds.Width / backbufferBounds.Height;
        float renderTargetAspectRatio = (float)RenderTarget.Width / RenderTarget.Height;

        // scale the image to fit into the window's back buffer.
        float rectX = 0;
        float rectY = 0f;
        float rectWidth = backbufferBounds.Width;
        float rectHeight = backbufferBounds.Height;

        // stretch image (render target) width to fit on the window's back buffer.
        if(backbufferAspectRatio > renderTargetAspectRatio)
        {
            rectWidth = rectHeight * renderTargetAspectRatio;
            rectX = ((float)backbufferBounds.Width - rectWidth) * 0.5f;
        }

        // shrink image (render target) height to fit on the window's back buffer.
        else if (backbufferAspectRatio < renderTargetAspectRatio)
        {
            rectHeight = rectWidth / renderTargetAspectRatio;
            rectY = ((float)backbufferBounds.Height - rectHeight) * 0.5f;
        }

        return new(
            (int)rectX,
            (int)rectY,
            (int)rectWidth, 
            (int)rectHeight
        );            
    }


    /// 
    /// Drawing Code.
    /// 



    public bool DrawSprite(in GenIndex textureId, Howl.Math.Vector2 position)
    {   
        ReadonlyRef<Texture2D> texture = textureManager.GetTextureReadonlyRef(textureId);
        if (texture.Valid == false)
        {
            return false;
        }
        else
        {
            // translate by the cameras position.
            // (Note):
            // reverse y-coordinates because monogame
            // sprite batch is y+ = down, Howl is y+ = up.
            position.Y *= -1;
            position -= new Howl.Math.Vector2(camera.Position.X, -camera.Position.Y);
            
            // reverse y-coordinates because monogame
            // sprite batch is y+ = down, Howl is y+ = up.
            // position.Y *= -1;

            spriteBatch.Draw(
                texture.Value, 
                position.ToMonogame(), 
                null, 
                Microsoft.Xna.Framework.Color.White, 
                0, 
                System.Numerics.Vector2.Zero, 
                System.Numerics.Vector2.One, 
                SpriteEffects.None, 
                0
            );
            return true;
        }
    }

    public void DrawPrimitive(Howl.Math.Rectangle rectangle, Howl.Graphics.Color color, bool wireframe)
    {
        if (wireframe)
        {
            DrawRectangleWireframe(rectangle, color);
        }
        else
        {
            DrawRectangleSolid(rectangle, color);
        }
    }

    public void DrawRectangleWireframe(Howl.Math.Rectangle rectangle, Howl.Graphics.Color color, float thickness = 4)
    {
        if(primitiveVertices.Count > short.MaxValue)
        {
            throw new OverflowException();
        }

        // (Note):
        // Dont reverse y-coordinates because draw line already does that.

        DrawLine(rectangle.TopLeft,rectangle.TopRight, color, thickness);
        DrawLine(rectangle.TopRight,rectangle.BottomRight, color, thickness);
        DrawLine(rectangle.BottomRight,rectangle.BottomLeft, color, thickness);
        DrawLine(rectangle.BottomLeft,rectangle.TopLeft, color, thickness);
    }

    public void DrawRectangleSolid(Howl.Math.Rectangle rectangle, Howl.Graphics.Color color)
    {
        if(primitiveVertices.Count > short.MaxValue)
        {
            throw new OverflowException();
        }


        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        rectangle.Y *= -1;

        // Note: triangle vertices and indexes are done in
        // a clockwise motion. 

        short totalvVertices = (short)primitiveVertices.Count;
        primitiveIndices.Add(totalvVertices);
        primitiveIndices.Add((short)(totalvVertices+1));
        primitiveIndices.Add((short)(totalvVertices+2));
        primitiveIndices.Add(totalvVertices);
        primitiveIndices.Add((short)(totalvVertices+2));
        primitiveIndices.Add((short)(totalvVertices+3));


        // translate in relation to the camera.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Howl.Math.Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);
        Howl.Math.Vector3 a = -cameraPosition;
        Howl.Math.Vector3 b = -cameraPosition;
        Howl.Math.Vector3 c = -cameraPosition;
        Howl.Math.Vector3 d = -cameraPosition;

        // apply the rectangles world coordinates.
        a += new Howl.Math.Vector3(rectangle.Left, rectangle.Top, 0f); 
        b += new Howl.Math.Vector3(rectangle.Right, rectangle.Top, 0f); 
        c += new Howl.Math.Vector3(rectangle.Right, rectangle.Bottom, 0f);
        d += new Howl.Math.Vector3(rectangle.Left, rectangle.Bottom, 0f); 
        
        Microsoft.Xna.Framework.Color monoGameColor = color.ToMonoGame();


        primitiveVertices.Add(new(a.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(b.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(c.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(d.ToMonoGame(), monoGameColor));        
    }

    public void DrawLine(Howl.Math.Vector2 a, Howl.Math.Vector2 b, Howl.Graphics.Color color, float thickness, bool scaleThickness = true)
    {
        thickness /= camera.Zoom;

        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        a.Y *= -1;
        b.Y *= -1;

        System.Math.Clamp(thickness, float.Epsilon, float.MaxValue);
        float halfThickness = thickness * 0.5f;

        // note that we apply the half thickness to the direction so that the line segment
        // corners are offseted by the thickness amount.
        Howl.Math.Vector2 direction = (b - a).Normalise() * halfThickness;
        Howl.Math.Vector2 oppositeDirection = -direction;   
        
        Howl.Math.Vector2 normal = new(-direction.Y, direction.X);
        Howl.Math.Vector2 oppositeNormal = -normal;

        // Note: triangle vertices and indexes are done in
        // a clockwise motion. 

        short totalvVertices = (short)primitiveVertices.Count;
        primitiveIndices.Add(totalvVertices);
        primitiveIndices.Add((short)(totalvVertices+1));
        primitiveIndices.Add((short)(totalvVertices+2));
        primitiveIndices.Add(totalvVertices);
        primitiveIndices.Add((short)(totalvVertices+2));
        primitiveIndices.Add((short)(totalvVertices+3));


        // translate in relation to the camera.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Howl.Math.Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);
        Howl.Math.Vector3 corner1 = -cameraPosition;
        Howl.Math.Vector3 corner2 = -cameraPosition;
        Howl.Math.Vector3 corner3 = -cameraPosition;
        Howl.Math.Vector3 corner4 = -cameraPosition;

        // apply the line world coordinates.
        corner1 += new Howl.Math.Vector3(a + normal + oppositeDirection, 0); 
        corner2 += new Howl.Math.Vector3(b + normal + direction, 0); 
        corner3 += new Howl.Math.Vector3(b + oppositeNormal + direction, 0);
        corner4 += new Howl.Math.Vector3(a + oppositeNormal + oppositeDirection, 0); 

        Microsoft.Xna.Framework.Color monoGameColor = color.ToMonoGame();

        primitiveVertices.Add(new(corner1.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(corner2.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(corner3.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(corner4.ToMonoGame(), monoGameColor));
    }

    public void DrawCircleWireframe(Howl.Math.Circle circle, Howl.Graphics.Color color, float thickness, int points = 16)
    {
        if(points == System.Math.Clamp(points, 3, int.MaxValue))
        {
            float deltaAngle = (float)System.Math.Tau / points;
            float angle = 0f;

            for(int i = 0; i < points; i++)
            {
                float ax = MathF.Sin(angle) * circle.Radius + circle.X;
                float ay = MathF.Cos(angle) * circle.Radius + circle.Y;

                angle += deltaAngle;

                float bx = MathF.Sin(angle) * circle.Radius + circle.X;
                float by = MathF.Cos(angle) * circle.Radius + circle.Y;

                DrawLine(new(ax, ay), new(bx,by), color, thickness);
            }
        }
        else
        {
            throw new InvalidOperationException($"Renderer can only draw a circle wireframe with 3 or inr.MaxValue points, not {points} amount of points.");   
        }
    }


    /// 
    /// Disposal.
    /// 


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            EffectManager.Dispose();
            TextureManager.Dispose();
            spriteBatch.Dispose();
            RenderTarget.Dispose();
        }

        disposed = true;
    }

    ~Renderer()
    {
        Dispose(false);
    }
}