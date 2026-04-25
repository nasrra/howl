using System;
using Howl.Ecs;
using Howl.Generic;
using Howl.Graphics;
using Howl.Graphics.Text;
using Howl.Vendors.MonoGame.Math.Shapes;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework.Graphics;
using Howl.Math;
using Microsoft.Xna.Framework;
using Howl.Debug;
using Howl.Vendors.MonoGame.FontStashSharp;
using FontStashSharp;

namespace Howl.Vendors.MonoGame.Graphics;

public static class RendererSystem
{    

    /// <summary>
    /// Registers all necessary components for this system into a .
    /// </summary>
    /// <param name="ecs">the specified component registry.</param>
    public static void RegisterComponents(ComponentRegistryNew registry)
    {
        ComponentRegistryNew.RegisterComponent<Sprite>(registry);
        ComponentRegistryNew.RegisterComponent<Text16>(registry);
        ComponentRegistryNew.RegisterComponent<Text32>(registry);
        ComponentRegistryNew.RegisterComponent<Text4096>(registry);
        ComponentRegistryNew.RegisterComponent<Transform>(registry);
    }

    /// <summary>
    /// Creates a new draw system for this MonoGame renderer.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static void Draw(EcsState ecs, MonoGameAppState app)
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);

        ref Camera mainCamera = ref ComponentArray.GetData(cameras, ecs, CameraSystem.MainCameraId, ref result);
        if(result != GenIdResult.Ok)
        {
            System.Diagnostics.Debug.Assert(false);
            return;            
        }
        
        ref Camera guiCamera = ref ComponentArray.GetData(cameras, ecs, CameraSystem.GuiCameraId, ref result);
        if(result != GenIdResult.Ok)
        {
            System.Diagnostics.Debug.Assert(false);
            return;
        }

        app.GraphicsDevice.SetRenderTarget(app.FinalRenderTarget);                    
        app.GraphicsDevice.Clear(mainCamera.ClearColour.ToMonoGame());
        
        DrawSprites(ecs, app, ref mainCamera, DrawSpace.World);
        DrawTexts(ecs, app, ref mainCamera, DrawSpace.World);
        DrawPrimitives(app);
        DrawSprites(ecs, app, ref guiCamera, DrawSpace.Gui);
        DrawTexts(ecs, app, ref guiCamera, DrawSpace.Gui);
        
        app.GraphicsDevice.SetRenderTarget(null);

        // draw the infal render target to the back buffer.
        app.GraphicsDevice.SetRenderTarget(null);            
        app.GraphicsDevice.Clear(Color.Black);
        app.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp
        );
        app.SpriteBatch.Draw(
            app.FinalRenderTarget,
            RectangleExtensions.ToMonoGame(app.DestinationRectangle), // this will probably need to be changed for calc dest rectangle.
            Color.White
        );
        app.SpriteBatch.End();
    }

    /// <summary>
    /// Draws all sprites to the currently bound render target.
    /// </summary>
    /// <param name="ecs">The ecs state where the sprites are stored.</param>
    /// <param name="app">The state of the renderer.</param>
    /// <param name="camera">The camera to draw in relation to.</param>
    /// <param name="worldSpace">filters sprites; drawing sprites that are within the specified world space.</param>
    private static void DrawSprites(EcsState ecs, MonoGameAppState app, ref Camera camera, DrawSpace worldSpace)
    {
        ComponentArray<Transform> transforms = EcsState.GetComponents<Transform>(ecs);
        ComponentArray<Sprite> sprites = EcsState.GetComponents<Sprite>(ecs);

        // update effects to use the new projection matrix.        
        app.EffectManager.UpdateProjectionMatrix(camera.ProjectionMatrix.ToMonoGame());

        app.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp, 
            rasterizerState: RasterizerState.CullNone,
            effect: app.EffectManager.DefaultSpriteEffect
        );   

        // draw sprites in relation to it.
        for(int i = 1; i < sprites.Active.Count; i++)
        {
            GenId genId = sprites.Active[i];
            
            ref Sprite sprite = ref ComponentArray.GetDataUnsafe(sprites, genId);
            if(sprite.DrawSpace != worldSpace)
            {
                continue;
            }

            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, genId);
        
            DrawSprite(app, ref camera, ref transform, ref sprite);
        }
        app.SpriteBatch.End();
    }

    /// <summary>
    /// Draws a sprite to the currently bound render target.
    /// </summary>
    /// <param name="app">The renderer state containing drawing context.</param>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">The transformation to apply to the sprite.</param>
    /// <param name="sprite">The sprite to draw.</param>
    /// <returns><see cref="GenIndexResult"/></returns>
    public static void DrawSprite(MonoGameAppState app, ref Camera camera, ref Transform transform, ref Sprite sprite)
    {   
        // translate by the cameras position.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Howl.Math.Vector2 position = transform.Position;
        position.Y *= -1;
        position -= new Howl.Math.Vector2(camera.Position.X, -camera.Position.Y);
        
        ref Texture2D texture = ref app.TextureManagerState.Textures[sprite.TextureId];
        if(texture == null)

        app.EffectManager.DefaultSpriteEffect.Texture = texture;

        app.SpriteBatch.Draw(texture, new(position.X, position.Y), RectangleExtensions.ToMonoGame(sprite.SourceRectangle),
            sprite.ColourTint.ToMonoGame(), -transform.Rotation, // rotate with negative rotation as sprite batch draws in reverse for some reason. 
            Vector2Extensions.ToMonoGame(sprite.Origin), Vector2Extensions.ToMonoGame(sprite.Scale * transform.Scale), 
            SpriteEffects.None, sprite.LayerDepth
        );
    }

    /// <summary>
    /// Draws all stored primitive shapes to the next frame/screen, clearing the internal primitives cache when drawn for the frame/screen after. 
    /// </summary>
    private static void DrawPrimitives(MonoGameAppState app)
    {
        DebugDrawState state = app.DebugDrawState;
        if(state.PrimitiveIndices.Count == 0 || state.PrimitiveVertices.Count == 0)
        {
            return;
        }

        if(app.GraphicsDevice == null)
        {
            return;
        }

        foreach(EffectPass pass in app.EffectManager.PrimitivesEffect.CurrentTechnique.Passes)
        {
            pass.Apply();            
            app.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                state.PrimitiveVertices.Data,
                0,
                state.PrimitiveVertices.Count,
                state.PrimitiveIndices.Data,
                0,
                state.PrimitiveIndices.Count / 3
            );
        }

        // clear cache so primitive data is not persistent between draw calls/frames.
        DebugDraw.Clear(state);
    }

    /// <summary>
    /// Sets the back buffer resolution (The actual application window size).
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="resolution">the width (x) and height (y) in pixels.</param>
    public static void SetBackBufferResolution(MonoGameAppState app, Howl.Math.Vector2Int resolution)
    {
        SetBackBufferResolution(app, resolution.X, resolution.Y);
    }
    
    /// <summary>
    /// Sets the back buffer resolution (The actual application window size).
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="width">the width in pixels.</param>
    /// <param name="height">the height in pixels.</param>
    public static void SetBackBufferResolution(MonoGameAppState app, int width, int height)
    {        
        int clampedWidth = System.Math.Clamp(width, 1, int.MaxValue);  
        int clampedHeight = System.Math.Clamp(height, 1, int.MaxValue);  
        if(width == clampedWidth && height == clampedHeight)
        {
            app.GraphicsDeviceManager.PreferredBackBufferHeight = height;
            app.GraphicsDeviceManager.PreferredBackBufferWidth = width;
            app.GraphicsDeviceManager.ApplyChanges();
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
    public static Howl.Math.Shapes.Rectangle CalculateRenderDestinationRectangle(MonoGameAppState state, RenderTarget2D renderTarget)
    {
        //     Rectangle backbufferBounds = MonoGameAppState.GraphicsDevice.PresentationParameters.Bounds;
        int backBufferWidth = state.Window.ClientBounds.Width;
        int backBufferHeight = state.Window.ClientBounds.Height;
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
    /// <param name="ecs">The ecs state where the texts are stored.</param>
    /// <param name="state">The state of the renderer.</param>
    /// <param name="camera">The camera to draw in relation to.</param>
    /// <param name="worldSpace">filters text; drawing texts that are within the specified world space.</param>
    public static void DrawTexts(EcsState ecs, MonoGameAppState state, ref Camera camera, DrawSpace worldSpace)
    {
        ComponentArray<Transform> transforms = EcsState.GetComponents<Transform>(ecs);
        ComponentArray<Text16> text16 = EcsState.GetComponents<Text16>(ecs);
        ComponentArray<Text32> text32 = EcsState.GetComponents<Text32>(ecs);
        ComponentArray<Text4096> text4096 = EcsState.GetComponents<Text4096>(ecs);        

        state.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp, 
            rasterizerState: RasterizerState.CullNone, 
            effect: state.EffectManager.DefaultSpriteEffect
        );

        // draw text 16.
        for(int i = 1; i < text16.Active.Count; i++)
        {
            GenId genId = text16.Active[i];
            ref Text16 text = ref ComponentArray.GetDataUnsafe(text16, genId);

            if(text.TextParameters.WorldSpace != worldSpace)
            {
                continue;
            }

            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, genId);

            DrawText(state, ref camera, ref transform, text.AsSpanUsed(), text.FontId, ref text.TextParameters);            
        }

        // draw text 32.
        for(int i = 1; i < text32.Active.Count; i++)
        {
            GenId genId = text32.Active[i];
            ref Text32 text = ref ComponentArray.GetDataUnsafe(text32, genId);

            if(text.TextParameters.WorldSpace != worldSpace)
            {
                continue;
            }

            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, genId);

            DrawText(state, ref camera, ref transform, text.AsSpanUsed(), text.FontId, ref text.TextParameters);            
        }

        // draw text 4096.
        for(int i = 1; i < text4096.Active.Count; i++)
        {
            GenId genId = text4096.Active[i];
            ref Text4096 text = ref ComponentArray.GetDataUnsafe(text4096, genId);

            if(text.TextParameters.WorldSpace != worldSpace)
            {
                continue;
            }

            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, genId);

            DrawText(state, ref camera, ref transform, text.AsSpanUsed(), text.FontId, ref text.TextParameters);            
        }

        state.SpriteBatch.End();
    }

    /// <summary>
    /// Draws text to the currently bound render target.
    /// </summary>
    /// <param name="state">The renderer state containing drawing context.</param>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">The transformation to apply to the text.</param>
    /// <param name="characters">The span of characters to draw.</param>
    /// <param name="textParameters">The text parameters.</param>
    /// <returns><see cref="GenIndexResult"/></returns>
    public static void DrawText(MonoGameAppState state, ref Camera camera, ref Transform transform, ReadOnlySpan<char> characters,
        int fontId, ref TextParameters textParameters 
    )
    {
        Font font = state.FontManagerState.Fonts[fontId];

        // fallback to nill if there is not font.
        if (font == null)
        {
            font = state.FontManagerState.Fonts[0];
        }

        Howl.Math.Vector2 position = transform.Position.InvertY() - camera.Position.InvertY();

        state.StringBuilder.Clear();
        state.StringBuilder.Append(characters);

        font.SpriteFontBase.DrawText(state.SpriteBatch, characters.ToString(), Vector2Extensions.ToMonoGame(position), textParameters.Colour.ToMonoGame(), 
            -transform.Rotation, Vector2Extensions.ToMonoGame(textParameters.Offset), Vector2Extensions.ToMonoGame(transform.Scale)
        );
    }
}