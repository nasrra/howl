using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Graphics.Text;
using Howl.Vendors.MonoGame.Math;
using Howl.Vendors.MonoGame.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Vendors.MonoGame.Graphics;

public sealed class Renderer : IRenderer
{    
    Colour clearColour;
    public Colour WorldClearColour
    {
        get => clearColour;
        set => clearColour = value;
    }
    
    public EffectManager EffectManager { get; private set; }
    private SpriteBatch spriteBatch;

    private TextureManager<Texture2D> textureManager;
    public ITextureManager TextureManager => textureManager;
    
    private Camera worldCamera;
    public ICamera WorldCamera => worldCamera;
    
    private Camera guiCamera;
    public ICamera GuiCamera => guiCamera;

    public Rectangle DestinationRectangle { get; private set; }
    
    public RenderTarget2D RenderTarget { get; private set; }
    public RenderTarget2D GuiRenderTarget {get; private set; }
    
    private List<VertexPositionColor> primitiveVertices;
    private List<int> primitiveIndices;

    private MonoGameApp monoGameApp;

    private Howl.Math.Vector2Int outputResolution;
    public Howl.Math.Vector2Int OutputResolution => outputResolution;

    public float OutputResolutionAspectRatio => (float)outputResolution.X / outputResolution.Y;

    private RenderState renderState = RenderState.None;

    private bool disposed = false;
    public bool IsDisposed => disposed;

    private FontManager fontManager;
    public IFontManager FontManager => fontManager;

    private StringBuilder stringBuilder;


    ///
    /// Constructors.
    /// 


    /// <summary>
    /// Creates a new MonoGame renderer instance.
    /// </summary>
    /// <param name="monoGameApp">The MonoGame app that is used as the HowlApp backend.</param>
    /// <param name="resolution">The resolution of this application.</param>
    /// <param name="worldCameraPosition">The position to place the world-space camera.</param>
    /// <param name="guiCameraPosition">The position to place the screen-space camera.</param>
    /// <param name="worldCameraZoomVirtualHeight">The base height - in world units - for the default world-space camera zoom level.</param>
    /// <param name="guiCameraZoomVirtualHeight">The base height - in world units - for the default screen-space camera zoom level.</param>
    public Renderer(
        MonoGameApp monoGameApp, 
        Resolution resolution,
        Howl.Math.Vector2 worldCameraPosition,
        Howl.Math.Vector2 guiCameraPosition,
        float worldCameraZoomVirtualHeight,
        float guiCameraZoomVirtualHeight
    )
    : this(
        monoGameApp, 
        worldCameraPosition,
        guiCameraPosition,
        worldCameraZoomVirtualHeight,
        guiCameraZoomVirtualHeight,
        resolution.BackBufferWidth, 
        resolution.BackBufferHeight, 
        resolution.OutputWidth, 
        resolution.OutputHeight
    )
    {
    }

    /// <summary>
    /// Creates a new MonoGame Renderer instances.
    /// </summary>
    /// <param name="monoGameApp">The MonoGame app that is used as the HowlApp backend.</param>
    /// <param name="worldCameraPosition">The postion to place the world-space camera at.</param>
    /// <param name="guiCameraPosition">The position to place the screen-space camera at.</param>
    /// <param name="worldCameraZoomVirtualHeight">The base height - in world units - for the default world-space camera zoom level.</param>
    /// <param name="guiCameraZoomVirtualHeight">The base height - in world units - for the default scree-space camera zoom level.</param>
    /// <param name="backBufferWidth">The back buffer width in pixels.</param>
    /// <param name="backbufferHeight">The back buffer height in pixels.</param>
    /// <param name="outputResolutionWidth">The output resolution width in pixels.</param>
    /// <param name="outputResolutionHeight">The output resolution height in pixels.</param>
    public Renderer(
        MonoGameApp monoGameApp, 
        Howl.Math.Vector2 worldCameraPosition,
        Howl.Math.Vector2 guiCameraPosition,
        float worldCameraZoomVirtualHeight,
        float guiCameraZoomVirtualHeight,
        int backBufferWidth, 
        int backbufferHeight, 
        int outputResolutionWidth, 
        int outputResolutionHeight
    )
    {
        this.monoGameApp = monoGameApp;        

        ValidateDependencies();

        primitiveVertices = new();
        primitiveIndices = new();     
        
        EffectManager = new(monoGameApp);        
        
        spriteBatch = new SpriteBatch(monoGameApp.GraphicsDevice);
        
        SetOutputResolution(outputResolutionWidth, outputResolutionHeight);
        SetBackBufferResolution(backBufferWidth, backbufferHeight);
        UpdateMainRenderDestinationRectangle();

        textureManager = new TextureManager(monoGameApp);
        
        fontManager = new(monoGameApp);
        stringBuilder = new(Text4096.MaxLength);
        
        // create cameras.
        worldCamera = new Camera(monoGameApp, this, worldCameraPosition, worldCameraZoomVirtualHeight, CoordinateSpace.Cartesian);
        guiCamera = new Camera(monoGameApp, this, guiCameraPosition, guiCameraZoomVirtualHeight, CoordinateSpace.Rasterized);

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
        SetOutputResolution(resolution.OutputWidth, resolution.OutputHeight);    
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

    public void SetOutputResolution(Howl.Math.Vector2Int resolution)
    {
        SetOutputResolution(resolution.X, resolution.Y);
    }

    public void SetOutputResolution(int width, int height)
    {        
        ValidateDependencies();
        int clampedWidth = System.Math.Clamp(width, 1, int.MaxValue);  
        int clampedHeight = System.Math.Clamp(height, 1, int.MaxValue);  
        if(width == clampedWidth && height == clampedHeight)
        {            
            RenderTarget?.Dispose();
            RenderTarget = new RenderTarget2D(monoGameApp.GraphicsDevice, width, height);
        
            GuiRenderTarget?.Dispose();
            GuiRenderTarget = new RenderTarget2D(monoGameApp.GraphicsDevice, width, height);
        
            outputResolution = new Howl.Math.Vector2Int(width, height);
        }
        else
        {
            throw new InvalidOperationException($"Output resolution cannot be set to ({width}, {height}), values must be above zero and lower than or equal to int.MaxValue");            
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

        // NOTE:
        // this may need to be removed.
        // monoGameApp.GraphicsDeviceManager.HardwareModeSwitch = false;

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
    /// Calculates the detination rectangle for a render target onto the backbuffer of the window this application is painting to.
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


    public void BeginDrawWorld()
    {   
        ValidateDependencies();

        renderState = RenderState.World;

        // draw all sprites to a render target.
        monoGameApp.GraphicsDevice.SetRenderTarget(RenderTarget);
                
        // Update camera after setting the render target to ensure that the projection matrix is relative to the render target
        // being drawn to and not the actual backbuffer dimensions of the program.
        worldCamera.Update();

        // update effects to use the new projection matrix.        
        EffectManager.UpdateProjectionMatrix(worldCamera.ProjectionMatrix.ToMonoGame());
        
        monoGameApp.GraphicsDevice.Clear(WorldClearColour.ToMonoGame());

        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointWrap, 
            rasterizerState: RasterizerState.CullNone, 
            effect: EffectManager.DefaultSpriteEffect
        );   
    }

    public void EndDrawWorld()
    {
        ValidateDependencies();

        renderState = RenderState.None;

        // present the render target over the windows back buffer.
        spriteBatch.End();

        // Note: 
        // Primitive drawing should always be outside of a sprite bath begin and end call.
        // Primitive drawing within those states causes undefined behvaiour.
        DrawPrimitives();

        // Note: when setting the render target to null, monogame draws directly to the backbuffer.
        monoGameApp.GraphicsDevice.SetRenderTarget(null);
    }

    public void BeginDrawGui()
    {
        ValidateDependencies();

        renderState = RenderState.Gui;

        // // draw all sprites to a render target.
        monoGameApp.GraphicsDevice.SetRenderTarget(GuiRenderTarget);
        
        // Update camera after setting the render target to ensure that the projection matrix is relative to the render target
        // being drawn to and not the actual backbuffer dimensions of the program.
        guiCamera.Update();

        // update effects to use the new projection matrix.        
        EffectManager.UpdateProjectionMatrix(guiCamera.ProjectionMatrix.ToMonoGame());

        monoGameApp.GraphicsDevice.Clear(IRenderer.GuiClearColour.ToMonoGame());

        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointWrap, 
            rasterizerState: RasterizerState.CullNone,
            effect: EffectManager.DefaultGuiSpriteEffect
        );
    }

    public void EndDrawGui()
    {
        ValidateDependencies();

        renderState = RenderState.None;

        // // present the render target over the windows back buffer.
        spriteBatch.End();

        // // Note: when setting the render target to null, monogame draws directly to the backbuffer.
        monoGameApp.GraphicsDevice.SetRenderTarget(null);
    }

    public void SubmitDraw()
    {
        // draw the render target back to the back buffer.
        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp
        );

        spriteBatch.Draw(
            RenderTarget,
            DestinationRectangle, // ensure to properly scale the render target to fit within the window's back buffer.
            Microsoft.Xna.Framework.Color.White
        );

        spriteBatch.Draw(
            GuiRenderTarget,
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
                CollectionsMarshal.AsSpan(primitiveVertices).ToArray(),
                0,
                primitiveVertices.Count,
                CollectionsMarshal.AsSpan(primitiveIndices).ToArray(),
                0,
                primitiveIndices.Count / 3
            );
        }

        // clear cache so primitive data is not persistent between draw calls/frames.

        primitiveVertices.Clear();
        primitiveIndices.Clear();
    }

    public GenIndexResult DrawSprite(in Howl.Math.Transform transform, in Sprite sprite)
    {   
        GenIndexResult result = textureManager.GetTextureReadonlyRef(sprite.Texture, out ReadOnlyRef<Texture2D> texture);
        if (result != GenIndexResult.Success || texture.Valid == false)
        {
            return result;
        }
        else
        {
            // translate by the cameras position.
            // (Note):
            // reverse y-coordinates because monogame
            // sprite batch is y+ = down, Howl is y+ = up.
            Howl.Math.Vector2 position = transform.Position;
            position.Y *= -1;
            position -= new Howl.Math.Vector2(worldCamera.Position.X, -worldCamera.Position.Y);
            
            // reverse y-coordinates because monogame
            // sprite batch is y+ = down, Howl is y+ = up.
            // position.Y *= -1;

            spriteBatch.Draw(
                texture.Value, 
                new(position.X, position.Y), 
                sprite.SourceRectangle.ToMonoGame(), 
                sprite.ColourTint.ToMonoGame(), 
                -transform.Rotation, // rotate with negative rotation as sprite batch draws in reverse for some reason. 
                sprite.Origin.ToMonogame(), 
                sprite.Scale.ToMonogame(), 
                SpriteEffects.None, 
                sprite.LayerDepth
            );
            return result;
        }
    }

    public unsafe GenIndexResult DrawText(in Howl.Math.Transform transform, in Text16 text)
    {
        GenIndexResult result = fontManager.GetFontReadOnlyRef(text.TextParameters.FontGenIndex, out ReadOnlyRef<SpriteFont> font);

        if(result != GenIndexResult.Success)
        {
            return result;
        }

        Howl.Math.Vector2 position = renderState switch
        {
            RenderState.World => transform.Position.InvertY() - worldCamera.Position.InvertY(),
            RenderState.Gui => transform.Position.InvertY() - guiCamera.Position.InvertY(), // example
            _ => throw new Exception($"Cannot draw text in when Renderer is in render state '{renderState}'")
        };

        stringBuilder.Clear();
        fixed (char* characters = text.Characters)
        {
            stringBuilder.Append(new ReadOnlySpan<char>(characters, text.Length));
        }

        spriteBatch.DrawString(
            font.Value, 
            stringBuilder, 
            position.ToMonogame(), 
            text.TextParameters.Colour.ToMonoGame(), 
            -transform.Rotation, 
            text.TextParameters.Offset.ToMonogame(), 
            MathF.Max(transform.Scale.X, transform.Scale.Y), 
            SpriteEffects.None, 
            0
        );

        return result;
    }

    public unsafe GenIndexResult DrawText(in Howl.Math.Transform transform, in Text4096 text)
    {
        GenIndexResult result = fontManager.GetFontReadOnlyRef(text.TextParameters.FontGenIndex, out ReadOnlyRef<SpriteFont> font);

        if(result != GenIndexResult.Success)
        {
            return result;
        }

        Howl.Math.Vector2 position = renderState switch
        {
            RenderState.World => transform.Position.InvertY() - worldCamera.Position.InvertY(),
            RenderState.Gui => transform.Position.InvertY() - guiCamera.Position.InvertY(), // example
            _ => throw new Exception($"Cannot draw text in when Renderer is in render state '{renderState}'")
        };

        stringBuilder.Clear();
        fixed (char* characters = text.Characters)
        {
            stringBuilder.Append(new ReadOnlySpan<char>(characters, text.Length));
        }

        spriteBatch.DrawString(
            font.Value, 
            stringBuilder, 
            position.ToMonogame(), 
            text.TextParameters.Colour.ToMonoGame(), 
            -transform.Rotation, 
            text.TextParameters.Offset.ToMonogame(), 
            MathF.Max(transform.Scale.X, transform.Scale.Y), 
            SpriteEffects.None, 
            0
        );

        return result;
    }

    public void DrawLine(Howl.Math.Vector2 a, Howl.Math.Vector2 b, Howl.Graphics.Colour colour, float thickness, bool scaleThickness = true)
    {
        thickness /= worldCamera.Zoom;

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
        Howl.Math.Vector3 cameraPosition = new(worldCamera.Position.X, -worldCamera.Position.Y, 0);
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

    public void DrawWireframeShape(in Howl.Math.Transform transform, in Howl.Graphics.RectangleShape rectangle, float thickness = 4)
    {
        if(primitiveVertices.Count > short.MaxValue)
        {
            throw new OverflowException();
        }

        // (Note):
        // Dont reverse y-coordinates because draw line already does that.

        Howl.Math.Vector2 topLeft       = rectangle.Shape.TopLeft.Transform(transform);
        Howl.Math.Vector2 topRight      = rectangle.Shape.TopRight.Transform(transform);
        Howl.Math.Vector2 bottomLeft    = rectangle.Shape.BottomLeft.Transform(transform);
        Howl.Math.Vector2 bottomRight   = rectangle.Shape.BottomRight.Transform(transform); 

        DrawLine(topLeft, topRight, rectangle.Colour, thickness);
        DrawLine(topRight, bottomRight, rectangle.Colour, thickness);
        DrawLine(bottomRight, bottomLeft, rectangle.Colour, thickness);
        DrawLine(bottomLeft, topLeft, rectangle.Colour, thickness);
    }

    public void DrawFilledShape(in Howl.Math.Transform transform, in Howl.Graphics.RectangleShape rectangle)
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
        Howl.Math.Vector3 topLeft       = new(rectangle.Shape.TopLeft.Transform(transform),0);
        Howl.Math.Vector3 topRight      = new(rectangle.Shape.TopRight.Transform(transform),0);
        Howl.Math.Vector3 bottomLeft    = new(rectangle.Shape.BottomLeft.Transform(transform),0);
        Howl.Math.Vector3 bottomRight   = new(rectangle.Shape.BottomRight.Transform(transform),0);

        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        topLeft.Y *= -1;
        topRight.Y *= -1;
        bottomLeft.Y *= -1;
        bottomRight.Y *= -1;

        Howl.Math.Vector3 cameraPosition = new(worldCamera.Position.X, -worldCamera.Position.Y, 0);

        // apply the rectangles world coordinates.

        Howl.Math.Vector3 a = -cameraPosition + topLeft;
        Howl.Math.Vector3 b = -cameraPosition + topRight;
        Howl.Math.Vector3 c = -cameraPosition + bottomRight;
        Howl.Math.Vector3 d = -cameraPosition + bottomLeft;
        
        Microsoft.Xna.Framework.Color monoGameColor = rectangle.Colour.ToMonoGame();
        primitiveVertices.Add(new(a.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(b.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(c.ToMonoGame(), monoGameColor));
        primitiveVertices.Add(new(d.ToMonoGame(), monoGameColor));        
    }

    public void DrawFilledShape(in Howl.Math.Transform transform, in CircleShape circle, int verticeCount = IRenderer.DefaultCirclePointAmount)
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
            Howl.Math.Vector2 start = new(0f, circle.Shape.Radius);
            Howl.Math.Vector2 position = new(circle.Shape.X, circle.Shape.Y);
            Howl.Math.Vector3 cameraPosition = new(worldCamera.Position.X, -worldCamera.Position.Y, 0);

            for(int i = 0; i < verticeCount; i++)
            {
                Howl.Math.Vector3 vertice = new Howl.Math.Vector3((start + position).Transform(transform),0);
                
                vertice.Y *= -1;

                vertice = -cameraPosition + vertice;

                start = new(
                    cos * start.X  - sin * start.Y,
                    sin * start.X  + cos * start.Y
                );

                primitiveVertices.Add(new(vertice.ToMonoGame(),circle.Colour.ToMonoGame()));
            }

        }
        else
        {
            throw new InvalidOperationException($"Renderer can only draw a solid circle with 3 or int.MaxValue 'verticeCount', not {verticeCount} amount of vertices.");               
        }        
    }

    public void DrawWireframeShape(
        in Howl.Math.Transform transform, 
        in CircleShape circle,  
        int verticeCount = IRenderer.DefaultCirclePointAmount,
        float thickness = IRenderer.DefaultWireframeThickness)   
    {
        if(verticeCount == System.Math.Clamp(verticeCount, 3, int.MaxValue))
        {
            float rotation = (float)System.Math.Tau / verticeCount;            
            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);
            Howl.Math.Vector2 start = new(0f, circle.Shape.Radius);
            Howl.Math.Vector2 position = new(circle.Shape.X, circle.Shape.Y);

            for(int i = 0; i < verticeCount; i++)
            {
                Howl.Math.Vector2 end = new(
                    cos * start.X  - sin * start.Y,
                    sin * start.X  + cos * start.Y
                );

                DrawLine(
                    (start + position).Transform(transform), 
                    (end + position).Transform(transform), 
                    circle.Colour, 
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

    public void DrawWireframeShape(in Howl.Math.Transform transform, in Polygon16Shape shape, float thickness = IRenderer.DefaultWireframeThickness)
    {
        Howl.Math.Vector2 start = shape.GetVertex(0).Transform(transform); 
        for(int i = 1; i <= shape.Polygon.VerticesCount; i++)
        {
            int index = i % shape.Polygon.VerticesCount;
            Howl.Math.Vector2 end = shape.GetVertex(index).Transform(transform); 
            DrawLine(start, end, shape.Colour, thickness);
            start = end;
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

    private void Dispose(bool disposing)
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