using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Graphics.Text;
using Howl.Vendors.MonoGame.Math.Shapes;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework.Graphics;
using Howl.Vendors.MonoGame.Text;
using Howl.Math;
using Microsoft.Xna.Framework;

namespace Howl.Vendors.MonoGame.Graphics;

public static class RendererSystem
{    

    /// <summary>
    /// Registers all necessary components for this system in a component registry.
    /// </summary>
    /// <param name="componentRegistry">the specified component registry.</param>
    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.RegisterComponent<Sprite>();
        componentRegistry.RegisterComponent<Text16>();
        componentRegistry.RegisterComponent<Text4096>();
        componentRegistry.RegisterComponent<Transform>();
    }

    /// <summary>
    /// Creates a new draw system for this MonoGame renderer.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static DrawSystem DrawSystem(ComponentRegistry componentRegistry, RendererState state)
    {
        return deltaTime =>
        {
            GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>();

            // get the main camera.
            if(cameraComponents.GetDenseRef(CameraSystem.MainCameraId, out Ref<Camera> mainCamera).Fail())
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }
            
            // get the ui camera.
            if(cameraComponents.GetDenseRef(CameraSystem.GuiCameraId, out Ref<Camera> guiCamera).Fail())
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            state.MonoGameApp.GraphicsDevice.SetRenderTarget(state.FinalRenderTarget);                    
            state.MonoGameApp.GraphicsDevice.Clear(mainCamera.Value.ClearColour.ToMonoGame());
            
            DrawSprites(componentRegistry, state, ref mainCamera.Value, WorldSpace.World);
            DrawTexts(componentRegistry, state, ref mainCamera.Value, WorldSpace.World);
            DrawPrimitives(state);
            DrawSprites(componentRegistry, state, ref guiCamera.Value, WorldSpace.Gui);
            DrawTexts(componentRegistry, state, ref guiCamera.Value, WorldSpace.Gui);
            
            state.MonoGameApp.GraphicsDevice.SetRenderTarget(null);

            // draw the infal render target to the back buffer.
            state.MonoGameApp.GraphicsDevice.SetRenderTarget(null);            
            state.MonoGameApp.GraphicsDevice.Clear(Color.Black);
            state.SpriteBatch.Begin(
                blendState: BlendState.AlphaBlend, 
                samplerState: SamplerState.PointClamp
            );
            state.SpriteBatch.Draw(
                state.FinalRenderTarget,
                state.DestinationRectangle.ToMonoGame(), // this will probably need to be changed for calc dest rectangle.
                Color.White
            );
            state.SpriteBatch.End();
        };
    }

    /// <summary>
    /// Draws all sprites to the currently bound render target.
    /// </summary>
    /// <param name="componentRegistry">The componenst registry where the sprites are stored.</param>
    /// <param name="state">The state of the renderer.</param>
    /// <param name="camera">The camera to draw in relation to.</param>
    /// <param name="worldSpace">filters sprites; drawing sprites that are within the specified world space.</param>
    private static void DrawSprites(ComponentRegistry componentRegistry, RendererState state, ref Camera camera, WorldSpace worldSpace)
    {
        GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();
        GenIndexList<Sprite> spritesComponents = componentRegistry.Get<Sprite>();
        Span<DenseEntry<Sprite>> spriteDenseEntries = spritesComponents.GetDenseAsSpan();

        // update effects to use the new projection matrix.        
        state.EffectManager.UpdateProjectionMatrix(camera.ProjectionMatrix.ToMonoGame());

        state.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp, 
            rasterizerState: RasterizerState.CullNone, 
            effect: state.EffectManager.DefaultSpriteEffect
        );   

        // draw sprites in relation to it.
        for(int i = 0; i < spriteDenseEntries.Length; i++)
        {

            ref DenseEntry<Sprite> spriteDenseEntry = ref spriteDenseEntries[i];
            ref Sprite sprite = ref spriteDenseEntry.Value;
            
            if(sprite.WorldSpace != worldSpace)
                continue;
            
            spritesComponents.GetGenIndex(spriteDenseEntry.sparseIndex, out GenIndex genIndex);

            if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformRef).Fail())
                continue;
            
            DrawSprite(state, ref camera, ref transformRef.Value, ref sprite).Ok();
        }

        state.SpriteBatch.End();
    }

    /// <summary>
    /// Draws a sprite to the currently bound render target.
    /// </summary>
    /// <param name="state">The renderer state containing drawing context.</param>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">The transformation to apply to the sprite.</param>
    /// <param name="sprite">The sprite to draw.</param>
    /// <returns><see cref="GenIndexResult"/></returns>
    public static GenIndexResult DrawSprite(
        RendererState state, 
        ref Camera camera, 
        ref Transform transform, 
        ref Sprite sprite
    )
    {   
        TextureManager textureManager = (TextureManager)state.TextureManager;
        GenIndexResult result = textureManager.GetTextureReadonlyRef(sprite.Texture, out ReadOnlyRef<Texture2D> texture);
        if (result != GenIndexResult.Ok || texture.Valid == false)
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
            position -= new Howl.Math.Vector2(camera.Position.X, -camera.Position.Y);
            
            state.SpriteBatch.Draw(
                texture.Value, 
                new(position.X, position.Y), 
                sprite.SourceRectangle.ToMonoGame(), 
                sprite.ColourTint.ToMonoGame(), 
                -transform.Rotation, // rotate with negative rotation as sprite batch draws in reverse for some reason. 
                sprite.Origin.ToMonogame(), 
                (sprite.Scale * transform.Scale).ToMonogame(), 
                SpriteEffects.None, 
                sprite.LayerDepth
            );
            return result;
        }
    }

    /// <summary>
    /// Draws all stored primitive shapes to the next frame/screen, clearing the internal primitives cache when drawn for the frame/screen after. 
    /// </summary>
    private static void DrawPrimitives(RendererState state)
    {
        if(Debug.Draw.PrimitiveIndices.Count == 0 && Debug.Draw.PrimitiveVertices.Count == 0)
        {
            return;
        }

        if(state.MonoGameApp.GraphicsDevice == null)
        {
            return;
        }

        foreach(EffectPass pass in state.EffectManager.PrimitivesEffect.CurrentTechnique.Passes)
        {
            pass.Apply();            
            state.MonoGameApp.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                PrimitiveType.TriangleList,
                Debug.Draw.PrimitiveVertices.ToMonoGameArray(),
                0,
                Debug.Draw.PrimitiveVertices.Count,
                Debug.Draw.PrimitiveIndices.ToArray(),
                0,
                Debug.Draw.PrimitiveIndices.Count / 3
            );
        }

        // clear cache so primitive data is not persistent between draw calls/frames.
        Debug.Draw.Clear();
    }

    /// <summary>
    /// Sets the back buffer resolution (The actual application window size).
    /// </summary>
    /// <param name="monoGameApp">the monogame app instance.</param>
    /// <param name="resolution">the width (x) and height (y) in pixels.</param>
    public static void SetBackBufferResolution(MonoGameApp monoGameApp, Howl.Math.Vector2Int resolution)
    {
        SetBackBufferResolution(monoGameApp, resolution.X, resolution.Y);
    }
    
    /// <summary>
    /// Sets the back buffer resolution (The actual application window size).
    /// </summary>
    /// <param name="monoGameApp">the monogame app instance.</param>
    /// <param name="width">the width in pixels.</param>
    /// <param name="height">the height in pixels.</param>
    public static void SetBackBufferResolution(MonoGameApp monoGameApp, int width, int height)
    {        
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
            throw new ArgumentException($"BackBuffer resolution cannot be set to ({width}, {height}), values must be above zero and lower than or equal to int.MaxValue");            
        }
    }

    /// <summary>
    /// Calculates the detination rectangle for a render target onto the backbuffer of the window this application is painting to.
    /// </summary>
    /// <returns>The calculated destination rectangle.</returns>
    public static Howl.Math.Shapes.Rectangle CalculateRenderDestinationRectangle(MonoGameApp monoGameApp, RenderTarget2D renderTarget)
    {
        //     Rectangle backbufferBounds = monoGameApp.GraphicsDevice.PresentationParameters.Bounds;
        int backBufferWidth = monoGameApp.Window.ClientBounds.Width;
        int backBufferHeight = monoGameApp.Window.ClientBounds.Height;
        float backbufferAspectRatio = (float)backBufferWidth / backBufferHeight;
        float renderTargetAspectRatio = (float)renderTarget.Width / renderTarget.Height;

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

        return new Howl.Math.Shapes.Rectangle(
            (int)rectX,
            (int)rectY,
            (int)rectWidth, 
            (int)rectHeight
        );            
    }

    /// <summary>
    /// Draws all texts to the currently bound render target.
    /// </summary>
    /// <param name="componentRegistry">The componenst registry where the sprites are stored.</param>
    /// <param name="state">The state of the renderer.</param>
    /// <param name="camera">The camera to draw in relation to.</param>
    /// <param name="worldSpace">filters sprites; drawing sprites that are within the specified world space.</param>
    public static void DrawTexts(ComponentRegistry componentRegistry, RendererState state, ref Camera camera, WorldSpace worldSpace)
    {
        GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();            
        
        // draw text 16.
        GenIndexList<Text16> text16Components = componentRegistry.Get<Text16>();
        Span<DenseEntry<Text16>> text16DenseEntries = text16Components.GetDenseAsSpan();

        state.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp, 
            rasterizerState: RasterizerState.CullNone, 
            effect: state.EffectManager.DefaultSpriteEffect
        );

        for(int i = 0; i < text16DenseEntries.Length; i++)
        {
            ref DenseEntry<Text16> denseEntry = ref text16DenseEntries[i];
            ref Text16 text = ref denseEntry.Value;
            text16Components.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);
            
            if(text.TextParameters.WorldSpace != worldSpace)
                continue;

            if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformReadOnlyRef).Fail())
            {
                System.Diagnostics.Debug.Assert(false);
                continue;
            }

            DrawText(state, ref camera, ref transformReadOnlyRef.Value, ref text);
        }

        // draw text 4096
        GenIndexList<Text4096> text4096Components = componentRegistry.Get<Text4096>();
        Span<DenseEntry<Text4096>> text4096DenseEntries = text4096Components.GetDenseAsSpan();
        
        for(int i = 0; i < text4096DenseEntries.Length; i++)
        {
            ref DenseEntry<Text4096> denseEntry = ref text4096DenseEntries[i];
            ref Text4096 text = ref denseEntry.Value;
            text16Components.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);
            
            if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformReadOnlyRef).Fail())
                continue;

            DrawText(state, ref camera, ref transformReadOnlyRef.Value, ref text);
        }

        state.SpriteBatch.End();
    }

    /// <summary>
    /// Draws a text to the currently bound render target.
    /// </summary>
    /// <param name="state">The renderer state containing drawing context.</param>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">The transformation to apply to the text.</param>
    /// <param name="text">The text to draw.</param>
    /// <returns><see cref="GenIndexResult"/></returns>
    public static unsafe GenIndexResult DrawText(
        RendererState state, 
        ref Camera camera, 
        ref Transform transform, 
        ref Text16 text
    )
    {
        FontManager fontManager = (FontManager)state.FontManager;
        GenIndexResult result = fontManager.GetFontReadOnlyRef(text.TextParameters.FontGenIndex, out ReadOnlyRef<SpriteFont> font);

        if(result != GenIndexResult.Ok)
        {
            return result;
        }

        Howl.Math.Vector2 position = transform.Position.InvertY() - camera.Position.InvertY();

        state.StringBuilder.Clear();
        fixed (char* characters = text.Characters)
        {
            state.StringBuilder.Append(new ReadOnlySpan<char>(characters, text.Length));
        }

        state.SpriteBatch.DrawString(
            font.Value, 
            state.StringBuilder, 
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


    /// <summary>
    /// Draws a text to the currently bound render target.
    /// </summary>
    /// <param name="state">The renderer state containing drawing context.</param>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">The transformation to apply to the text.</param>
    /// <param name="text">The text to draw.</param>
    /// <returns><see cref="GenIndexResult"/></returns>
    public static unsafe GenIndexResult DrawText(
        RendererState state, 
        ref Camera camera, 
        ref Transform transform, 
        ref Text4096 text
    )
    {
        FontManager fontManager = (FontManager)state.FontManager;

        GenIndexResult result = fontManager.GetFontReadOnlyRef(text.TextParameters.FontGenIndex, out ReadOnlyRef<SpriteFont> font);

        if(result != GenIndexResult.Ok)
        {
            return result;
        }

        Howl.Math.Vector2 position = transform.Position.InvertY() - camera.Position.InvertY();

        state.StringBuilder.Clear();
        fixed (char* characters = text.Characters)
        {
            state.StringBuilder.Append(new ReadOnlySpan<char>(characters, text.Length));
        }

        state.SpriteBatch.DrawString(
            font.Value, 
            state.StringBuilder, 
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
}