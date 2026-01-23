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
    Howl.Graphics.Colour clearColour;
    public Howl.Graphics.Colour ClearColour
    {
        get => clearColour;
        set => clearColour = value;
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
    private List<int> primitiveIndices;

    private MonoGameApp monoGameApp;

    public float MainRenderTargetWidth => RenderTarget.Width;

    public float MainRenderTargetHeight => RenderTarget.Height;

    public float MainRenderTargetAspectRatio => (float)RenderTarget.Width / RenderTarget.Height;

    private bool disposed = false;
    public bool IsDisposed => disposed;


    ///
    /// Constructors.
    /// 



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
        
        SetMainRenderTargetResolution(mainRenderTargetWidth, mainRenderTargetHeight);
        SetBackBufferResolution(backBufferWidth, backbufferHeight);
        UpdateMainRenderDestinationRectangle();

        textureManager = new TextureManager(monoGameApp);
        camera = new Camera(monoGameApp, this, cameraPosition, cameraZoomVirtualheight);

        LinkEvents();
    }


    /// 
    /// Validation.
    /// 


    private void ValidateDependencies()
    {
        if (monoGameApp.IsDisposed)
        {
            throw new ObjectDisposedException("Renderer cannot operate on/with a disposed MonoGameApp.");
        }
    }


    /// 
    /// Setters.
    /// 


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


    /// 
    /// Screen Mode Handling.
    /// 


    public void Windowed()
    {
        ValidateDependencies();

        if(monoGameApp.GraphicsDeviceManager.IsFullScreen == false)
        {
            return;
        }

        monoGameApp.GraphicsDeviceManager.ToggleFullScreen();
    }

    public void Fullscreen()
    {
        // throw new Exception("This method should not be used as MonoGame currently has a bad bug. Read the comment within this function.");


        ValidateDependencies();

        if(monoGameApp.GraphicsDeviceManager.IsFullScreen == true
        && monoGameApp.GraphicsDeviceManager.HardwareModeSwitch == true)
        {
            return;
        }

        monoGameApp.GraphicsDeviceManager.HardwareModeSwitch = true;

        monoGameApp.GraphicsDeviceManager.ToggleFullScreen();


        // NOTE:
        // Monogame "corrupts" the computers back buffer when toggling fullscreen upon closing the application afterwards. 
        // The screen is fine for a split second then switches to the "Clear Colour" of the renderer.
        // nothing  can be clicked on the computer, alt+f4 doesnt work, it completely nukes the computer.
        
        // UpdateMainRenderDestinationRectangle(); <-- DONT DO THIS at the end of this, subscribe to the monogame OnGraphicsDeviceReset(object caller, EventArgs e).
    }

    public void BorderlessFullscreen()
    {
        ValidateDependencies();

        if(monoGameApp.GraphicsDeviceManager.IsFullScreen == true
        && monoGameApp.GraphicsDeviceManager.HardwareModeSwitch == false)
        {
            return;
        }

        monoGameApp.GraphicsDeviceManager.HardwareModeSwitch = false;

        monoGameApp.GraphicsDeviceManager.ToggleFullScreen();

    }


    ///
    /// Updating.
    /// 


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
    /// Calculates the detination rectangle for the render target onto the backbuffer of the window this application is painting to.
    /// </summary>
    /// <returns>The calculated destination rectangle.</returns>
    private void UpdateMainRenderDestinationRectangle()
    {
        ValidateDependencies();

        //     Rectangle backbufferBounds = monoGameApp.GraphicsDevice.PresentationParameters.Bounds;
        int backBufferWidth = monoGameApp.Window.ClientBounds.Width;
        int backBufferHeight = monoGameApp.Window.ClientBounds.Height;
        float backbufferAspectRatio = (float)backBufferWidth / backBufferHeight;
        float renderTargetAspectRatio = (float)RenderTarget.Width / RenderTarget.Height;

        // scale the image to fit into the window's back buffer.
        float rectX = 0;
        float rectY = 0f;
        float rectWidth = backBufferWidth;
        float rectHeight = backBufferHeight;

        // stretch image (render target) width to fit on the window's back buffer.
        if(backbufferAspectRatio > renderTargetAspectRatio)
        {
            rectWidth = rectHeight * renderTargetAspectRatio;
            rectX = ((float)backBufferWidth - rectWidth) * 0.5f;
        }

        // shrink image (render target) height to fit on the window's back buffer.
        else if (backbufferAspectRatio < renderTargetAspectRatio)
        {
            rectHeight = rectWidth / renderTargetAspectRatio;
            rectY = ((float)backBufferHeight - rectHeight) * 0.5f;
        }

        DestinationRectangle = new(
            (int)rectX,
            (int)rectY,
            (int)rectWidth, 
            (int)rectHeight
        );            
    }


    /// 
    /// Drawing Code.
    /// 


    public void BeginDraw()
    {   
        ValidateDependencies();

        // draw all sprites to a render target.

        monoGameApp.GraphicsDevice.SetRenderTarget(RenderTarget);
        
        // ensure that the projection matrix is relative to the render target
        // being drawn to and not the actual backbuffer dimensions of the program.
        UpdateProjectionMatrix();
        
        monoGameApp.GraphicsDevice.Clear(clearColour.ToMonoGame());

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

    public void DrawLine(Howl.Math.Vector2 a, Howl.Math.Vector2 b, Howl.Graphics.Colour colour, float thickness, bool scaleThickness = true)
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

        Microsoft.Xna.Framework.Color monoGameColor = colour.ToMonoGame();

        primitiveVertices.Add(new(corner1.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(corner2.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(corner3.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(corner4.ToMonoGame(), monoGameColor));
    }

    public void DrawWireframeShape(Howl.Math.Transform transform, Howl.Graphics.RectangleShape shape, float thickness = 4)
    {
        if(primitiveVertices.Count > short.MaxValue)
        {
            throw new OverflowException();
        }

        // (Note):
        // Dont reverse y-coordinates because draw line already does that.

        Howl.Math.Vector2 topLeft       = shape.TopLeft.Transform(transform);
        Howl.Math.Vector2 topRight      = shape.TopRight.Transform(transform);
        Howl.Math.Vector2 bottomLeft    = shape.BottomLeft.Transform(transform);
        Howl.Math.Vector2 bottomRight   = shape.BottomRight.Transform(transform); 

        DrawLine(topLeft, topRight, shape.Colour, thickness);
        DrawLine(topRight, bottomRight, shape.Colour, thickness);
        DrawLine(bottomRight, bottomLeft, shape.Colour, thickness);
        DrawLine(bottomLeft, topLeft, shape.Colour, thickness);
    }

    public void DrawSolidShape(Howl.Math.Transform transform, Howl.Graphics.RectangleShape shape)
    {
        // Note: triangle vertices and indexes are done in
        // a clockwise motion. 

        int totalvVertices = primitiveVertices.Count;
        primitiveIndices.Add(totalvVertices);
        primitiveIndices.Add(totalvVertices+1);
        primitiveIndices.Add(totalvVertices+2);
        primitiveIndices.Add(totalvVertices);
        primitiveIndices.Add(totalvVertices+2);
        primitiveIndices.Add(totalvVertices+3);

        // translate in relation to the camera.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Howl.Math.Vector3 topLeft       = new(shape.TopLeft.Transform(transform),0);
        Howl.Math.Vector3 topRight      = new(shape.TopRight.Transform(transform),0);
        Howl.Math.Vector3 bottomLeft    = new(shape.BottomLeft.Transform(transform),0);
        Howl.Math.Vector3 bottomRight   = new(shape.BottomRight.Transform(transform),0);

        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        topLeft.Y *= -1;
        topRight.Y *= -1;
        bottomLeft.Y *= -1;
        bottomRight.Y *= -1;

        Howl.Math.Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);

        // apply the rectangles world coordinates.

        Howl.Math.Vector3 a = -cameraPosition + topLeft;
        Howl.Math.Vector3 b = -cameraPosition + topRight;
        Howl.Math.Vector3 c = -cameraPosition + bottomRight;
        Howl.Math.Vector3 d = -cameraPosition + bottomLeft;
        
        Microsoft.Xna.Framework.Color monoGameColor = shape.Colour.ToMonoGame();
        primitiveVertices.Add(new(a.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(b.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(c.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(d.ToMonoGame(), monoGameColor));        
    }

    public void DrawSolidShape(Howl.Math.Transform transform, CircleShape shape, int verticeCount = IRenderer.DefaultCirclePointAmount)
    {
        if(verticeCount == System.Math.Clamp(verticeCount, 3, int.MaxValue))
        {
            // Note: triangle vertices and indexes are done in
            // a clockwise motion. Triangles are made from the 
            // first vertice in the circle. 

            int index = 1;
            int totalVertices = primitiveVertices.Count;
            int triangleCount = verticeCount - 2; 
            for(int i = 0; i < triangleCount; i++)
            {
                primitiveIndices.Add(totalVertices);
                
                primitiveIndices.Add(totalVertices+index);
                
                primitiveIndices.Add(totalVertices+index+1);
                
                index+=1;
            }

            // add the vertices.

            float rotation = (float)System.Math.Tau / verticeCount;            
            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);
            Howl.Math.Vector2 start = new(0f, shape.Circle.Radius);
            Howl.Math.Vector2 position = new(shape.Circle.X, shape.Circle.Y);
            Howl.Math.Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);

            for(int i = 0; i < verticeCount; i++)
            {
                Howl.Math.Vector3 vertice = new Howl.Math.Vector3((start + position).Transform(transform),0);
                
                vertice.Y *= -1;

                vertice = -cameraPosition + vertice;

                start = new(
                    cos * start.X  - sin * start.Y,
                    sin * start.X  + cos * start.Y
                );

                primitiveVertices.Add(new(vertice.ToMonoGame(),shape.Colour.ToMonoGame()));
            }

        }
        else
        {
            throw new InvalidOperationException($"Renderer can only draw a solid circle with 3 or int.MaxValue 'verticeCount', not {verticeCount} amount of vertices.");               
        }        
    }

    public void DrawWireframeShape(
        Howl.Math.Transform transform, 
        CircleShape shape,  
        int verticeCount = IRenderer.DefaultCirclePointAmount,
        float thickness = IRenderer.DefaultWireframeThickness)   
    {
        if(verticeCount == System.Math.Clamp(verticeCount, 3, int.MaxValue))
        {
            float rotation = (float)System.Math.Tau / verticeCount;            
            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);
            Howl.Math.Vector2 start = new(0f, shape.Circle.Radius);
            Howl.Math.Vector2 position = new(shape.Circle.X, shape.Circle.Y);

            for(int i = 0; i < verticeCount; i++)
            {
                Howl.Math.Vector2 end = new(
                    cos * start.X  - sin * start.Y,
                    sin * start.X  + cos * start.Y
                );

                DrawLine(
                    (start + position).Transform(transform), 
                    (end + position).Transform(transform), 
                    shape.Colour, 
                    thickness
                );

                start = end;
            }
        }
        else
        {
            throw new InvalidOperationException($"Renderer can only draw a wireframe circle with 3 or int.MaxValue 'verticeCount', not {verticeCount} amount of vertices.");   
        }
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        monoGameApp.GraphicsDevice.DeviceReset += OnGraphicsDeviceReset;
    }

    private void UnlinkEvents()
    {
        monoGameApp.GraphicsDevice.DeviceReset -= OnGraphicsDeviceReset;        
    }

    private void OnGraphicsDeviceReset(object caller, EventArgs e)
    {
        // This ensures that the main render destination rectangle
        // will always be the correct size when the window's back buffer resizes;
        // including when toggling fullscreen and manually setting the back buffer.
        UpdateMainRenderDestinationRectangle();
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
            UnlinkEvents();
        }

        disposed = true;
    }

    ~Renderer()
    {
        Dispose(false);
    }
}