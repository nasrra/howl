using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Graphics;

public class Renderer
{
    public EffectManager EffectManager { get; private set; }
    public SpriteBatch SpriteBatch { get; private set; }
    public Matrix ProjectionMatrix { get; private set; }
    public Rectangle DestinationRectangle { get; private set; }
    public RenderTarget2D RenderTarget { get; private set; }

    public Renderer(int effectsAmount = 1, int renderTargetWidth = 1280, int renderTargetHeight = 720)
    {
        EffectManager = new(effectsAmount);        
        SpriteBatch = new SpriteBatch(HowlApp.GraphicsDevice);
        RenderTarget = new(HowlApp.GraphicsDevice, renderTargetWidth, renderTargetHeight);
        DestinationRectangle = CalculateDestinationRectangle(); 
    }

    /// <summary>
    /// Sets the render target width.
    /// </summary>
    /// <param name="value">The width to set the render target to.</param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    
    public void SetRenderTargetWidth(int value)
    {      
        int width = Math.Clamp(value, 1, int.MaxValue);  
        if(width == value)
        {            
            if(RenderTarget != null)
            {
                RenderTarget = new RenderTarget2D(HowlApp.GraphicsDevice, width, RenderTarget.Height);
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

    /// <summary>
    /// Sets the render target height. 
    /// </summary>
    /// <param name="value">The height to set the render target to.</param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="InvalidOperationException"></exception>

    public void SetRenderTargetHeight(int value)
    {
        int height = Math.Clamp(value, 1, int.MaxValue);
        if(height == value)
        {
            if(RenderTarget != null)
            {
                RenderTarget = new RenderTarget2D(HowlApp.GraphicsDevice, RenderTarget.Width, (int)value);
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

    /// <summary>
    /// Starts the draw call from the renderer to the gpu.
    /// </summary>

    public void BeginDraw()
    {   
        // draw all sprites to a render target.

        HowlApp.GraphicsDevice.SetRenderTarget(RenderTarget);
        
        // ensure that the projection matrix is relative to the render target
        // being drawn to and not the actual backbuffer dimensions of the program.
        UpdateProjectionMatrix();
        
        HowlApp.GraphicsDevice.Clear(Color.Wheat);

        SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointWrap, 
            rasterizerState: RasterizerState.CullNone, 
            effect: EffectManager.EffectsSpan()[0]
        );   
    }

    /// <summary>
    /// completes the draw call from the renderer to the gpu, displaying an image to the window.
    /// </summary>

    public void EndDraw()
    {
        // present the render target over the windows back buffer.

        SpriteBatch.End();

        // Note: when setting the render target to null, monogame draws directly to the backbuffer.

        HowlApp.GraphicsDevice.SetRenderTarget(null);
    

        // draw the render target back to the back buffer.
        
        SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointWrap
        );

        SpriteBatch.Draw(
            RenderTarget,
            DestinationRectangle, // ensure to properly scale the render target to fit within the window's back buffer.
            Color.White
        );

        SpriteBatch.End();
    }

    /// <summary>
    // Calculates ans sets the new projection matix so that -y is down and -x is left.
    /// </summary>

    private void UpdateProjectionMatrix()
    {
        Viewport viewport = HowlApp.GraphicsDevice.Viewport;
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
        Rectangle backbufferBounds = HowlApp.GraphicsDevice.PresentationParameters.Bounds;
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