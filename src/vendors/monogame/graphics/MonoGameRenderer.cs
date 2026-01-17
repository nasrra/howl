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

public class MonoGameRenderer : IRenderer
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
    
    public Matrix ProjectionMatrix { get; private set; }
    
    public Rectangle DestinationRectangle { get; private set; }
    
    public RenderTarget2D RenderTarget { get; private set; }
    
    private List<VertexPositionColor> primitiveVertices;
    private List<short> primitiveIndices;

    WeakReference<MonoGameApp> monoGameApp;

    private MonoGameApp GetMonoGameApp()
    {
        if(monoGameApp.TryGetTarget(out MonoGameApp app))
        {
            return app;
        }
        else
        {
            throw new NullReferenceException($"MonoGameRenderer cannot operate on a MonoGameApp that is null");   
        }

    }

    public MonoGameRenderer(WeakReference<MonoGameApp> monoGameApp, int renderTargetWidth = 1280, int renderTargetHeight = 720)
    {
        this.monoGameApp = monoGameApp;
        MonoGameApp app = GetMonoGameApp();
        
        primitiveVertices = new();
        primitiveIndices = new();     
        EffectManager = new(monoGameApp);        
        spriteBatch = new SpriteBatch(app.GraphicsDevice);
        RenderTarget = new(app.GraphicsDevice, renderTargetWidth, renderTargetHeight);
        DestinationRectangle = CalculateDestinationRectangle(); 
        textureManager = new MonoGameTextureManager(monoGameApp);
    }

    public void SetRenderTargetWidth(int value)
    {      
        MonoGameApp app = GetMonoGameApp();

        int width = System.Math.Clamp(value, 1, int.MaxValue);  
        if(width == value)
        {            
            if(RenderTarget != null)
            {
                RenderTarget.Dispose();
                RenderTarget = new RenderTarget2D(app.GraphicsDevice, width, RenderTarget.Height);
            }
            else
            {
                throw new NullReferenceException($"Renderer has not created a RenderTarget instance.");                
            }
        }
        else
        {
            throw new InvalidOperationException($"width cannot be set to {value}, it must be above zero and lower than or equal to int.MaxValue");            
        }
    }

    public void SetRenderTargetHeight(int value)
    {
        MonoGameApp app = GetMonoGameApp();

        int height = System.Math.Clamp(value, 1, int.MaxValue);
        if(height == value)
        {
            if(RenderTarget != null)
            {
                RenderTarget.Dispose();
                RenderTarget = new RenderTarget2D(app.GraphicsDevice, RenderTarget.Width, (int)value);
            }
            else
            {
                throw new NullReferenceException($"Renderer has not created a RenderTarget instance.");                
            }            
        }
        else
        {
            throw new InvalidOperationException($"height cannot be set to {value}, it must be above zero and lower than or equal to int.MaxValue");                        
        }
    }

    public void BeginDraw()
    {   
        MonoGameApp app = GetMonoGameApp();

        // draw all sprites to a render target.

        app.GraphicsDevice.SetRenderTarget(RenderTarget);
        
        // ensure that the projection matrix is relative to the render target
        // being drawn to and not the actual backbuffer dimensions of the program.
        UpdateProjectionMatrix();
        
        app.GraphicsDevice.Clear(clearColor.ToMonoGame());

        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointWrap, 
            rasterizerState: RasterizerState.CullNone, 
            effect: EffectManager.DefaultSpriteEffect
        );   
    }

    public void EndDraw()
    {
        MonoGameApp app = GetMonoGameApp();

        // present the render target over the windows back buffer.

        spriteBatch.End();

        // Note: 
        // Primitive drawing should always be outside of a sprite bath begin and end call.
        // Primitive drawing within those states causes undefined behvaiour.

        DrawPrimitives();

        // Note: when setting the render target to null, monogame draws directly to the backbuffer.

        app.GraphicsDevice.SetRenderTarget(null);
    

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

    public bool DrawSprite(in GenIndex textureId)
    {   
        ReadonlyRef<Texture2D> texture = textureManager.GetTextureReadonlyRef(textureId);
        if (texture.Valid == false)
        {
            return false;
        }
        else
        {
            spriteBatch.Draw(
                texture.Value, 
                new System.Numerics.Vector2(0, 0), 
                null, 
                Microsoft.Xna.Framework.Color.White, 
                0, 
                System.Numerics.Vector2.Zero, 
                System.Numerics.Vector2.One, 
                SpriteEffects.FlipVertically, 
                0
            );
            return true;
        }
    }

    /// <summary>
    // Calculates ans sets the new projection matix so that -y is down and -x is left.
    /// </summary>
    private void UpdateProjectionMatrix()
    {
        MonoGameApp app = GetMonoGameApp();
    
        Viewport viewport = app.GraphicsDevice.Viewport;
        ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, viewport.Width, 0, viewport.Height, -1, 1);
        
        // update effects to use the new projection matrix.
        
        EffectManager.UpdateProjectionMatrix(ProjectionMatrix);
    }

    /// <summary>
    /// Draws all stored primitive shapes to the next frame/screen, clearing the internal primitives cache when drawn for the frame/screen after. 
    /// </summary>
    private void DrawPrimitives()
    {
        if(primitiveIndices.Count == 0 && primitiveVertices.Count == 0)
        {
            return;
        }

        MonoGameApp app = GetMonoGameApp();

        if(app.GraphicsDevice == null)
        {
            return;
        }

        foreach(EffectPass pass in EffectManager.PrimitivesEffect.CurrentTechnique.Passes)
        {
            pass.Apply();            
            app.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
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
        MonoGameApp app = GetMonoGameApp();

        Rectangle backbufferBounds = app.GraphicsDevice.PresentationParameters.Bounds;
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

    public void DrawPrimitive(Howl.Math.Rectangle rectangle, Howl.Graphics.Color color)
    { 

        if(primitiveVertices.Count > short.MaxValue)
        {
            throw new OverflowException();
        }

        // Note: triangle vertices and indexes are done in
        // a clockwise motion. 

        short totalvVertices = (short)primitiveVertices.Count;
        primitiveIndices.Add(totalvVertices);
        primitiveIndices.Add((short)(totalvVertices+1));
        primitiveIndices.Add((short)(totalvVertices+2));
        primitiveIndices.Add(totalvVertices);
        primitiveIndices.Add((short)(totalvVertices+2));
        primitiveIndices.Add((short)(totalvVertices+3));

        // Note that we are always
        // drawing at zero z-coordinate.

        Vector3 a = new(rectangle.Left, rectangle.Top, 0); 
        Vector3 b = new(rectangle.Right, rectangle.Top, 0); 
        Vector3 c = new(rectangle.Right, rectangle.Bottom, 0);
        Vector3 d = new(rectangle.Left, rectangle.Bottom, 0); 
        
        Microsoft.Xna.Framework.Color monoGameColor = color.ToMonoGame();


        primitiveVertices.Add(new(a, monoGameColor));
        primitiveVertices.Add(new(b, monoGameColor));
        primitiveVertices.Add(new(c, monoGameColor));
        primitiveVertices.Add(new(d, monoGameColor));
    }

    public void DrawLine(Howl.Math.Vector2 a, Howl.Math.Vector2 b, float thickness, Howl.Graphics.Color color)
    {
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

        // Note that we are always
        // drawing at zero z-coordinate.

        Howl.Math.Vector3 corner1 = new(a + normal + oppositeDirection, 0); 
        Howl.Math.Vector3 corner2 = new(b + normal + direction, 0); 
        Howl.Math.Vector3 corner3 = new(b + oppositeNormal + direction, 0);
        Howl.Math.Vector3 corner4 = new(a + oppositeNormal + oppositeDirection, 0); 
        
        Microsoft.Xna.Framework.Color monoGameColor = color.ToMonoGame();

        primitiveVertices.Add(new(corner1.ToMonogame(), monoGameColor));
        primitiveVertices.Add(new(corner2.ToMonogame(), monoGameColor));
        primitiveVertices.Add(new(corner3.ToMonogame(), monoGameColor));
        primitiveVertices.Add(new(corner4.ToMonogame(), monoGameColor));
    }
}