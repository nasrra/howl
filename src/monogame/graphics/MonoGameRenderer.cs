using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.MonoGame.Graphics;

public class MonoGameRenderer : IRenderer
{
    public EffectManager EffectManager { get; private set; }
    private SpriteBatch spriteBatch;

    TextureManager<Texture2D> textureManager;
    public ITextureManager TextureManager => textureManager;
    
    public Matrix ProjectionMatrix { get; private set; }
    
    public Rectangle DestinationRectangle { get; private set; }
    
    public RenderTarget2D RenderTarget { get; private set; }
    
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

    public MonoGameRenderer(WeakReference<MonoGameApp> monoGameApp, int effectsAmount = 1, int renderTargetWidth = 1280, int renderTargetHeight = 720)
    {
        this.monoGameApp = monoGameApp;
        MonoGameApp app = GetMonoGameApp();
        
        EffectManager = new(monoGameApp, effectsAmount);        
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
                RenderTarget = new RenderTarget2D(app.GraphicsDevice, width, RenderTarget.Height);
                RenderTarget.Dispose();
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
                RenderTarget = new RenderTarget2D(app.GraphicsDevice, RenderTarget.Width, (int)value);
                RenderTarget.Dispose();
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
        
        app.GraphicsDevice.Clear(Color.Wheat);

        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointWrap, 
            rasterizerState: RasterizerState.CullNone, 
            effect: EffectManager.EffectsSpan()[0]
        );   
    }

    public void EndDraw()
    {
        MonoGameApp app = GetMonoGameApp();

        // present the render target over the windows back buffer.

        spriteBatch.End();

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
            Color.White
        );

        spriteBatch.End();
    }

    public bool DrawSprite(ref GenIndex textureId)
    {   
        ReadonlyRef<Texture2D> texture = textureManager.GetTextureReadonlyRef(ref textureId);
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
                Color.White, 
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

}