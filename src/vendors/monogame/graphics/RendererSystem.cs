using System;
using Howl.Ecs;
using Howl.Generic;
using Howl.Graphics;
using Howl.Vendors.MonoGame.Math.Shapes;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework.Graphics;
using Howl.Math;
using Microsoft.Xna.Framework;
using Howl.Debug;
using Howl.Vendors.MonoGame.FontStashSharp;
using FontStashSharp;
using Howl.Text;

namespace Howl.Vendors.MonoGame.Graphics;

public static class RendererSystem
{    
    /// <summary>
    ///     Performs a draw step for a monogame app state.
    /// </summary>
    /// <param name="monoGame">the state to draw to.</param>
    /// <param name="strings">the character data for the strings in the labels to draw.</param>
    /// <param name="transforms">the transforms of the elements to draw.</param>
    /// <param name="sprites">the sprites to draw.</param>
    /// <param name="labels">the labels to draw.</param>
    /// <param name="worldCamera">the world camera data.</param>
    /// <param name="screenCamera">the screen camera data.</param>
    public static void Draw(MonoGameAppState monoGame, StringRegistryState strings, ComponentArray<Transform> transforms, ComponentArray<Sprite> sprites, ComponentArray<Label> labels, 
        Camera worldCamera, Camera screenCamera
    )
    {
        monoGame.GraphicsDevice.SetRenderTarget(monoGame.FinalRenderTarget);                    
        monoGame.GraphicsDevice.Clear(worldCamera.ClearColour.ToMonoGame());
        
        DrawSprites(monoGame, transforms, sprites, ref worldCamera, DrawSpace.World);
        DrawLabels(monoGame, strings, transforms, labels, ref worldCamera, DrawSpace.World);

        DrawPrimitives(monoGame);

        DrawSprites(monoGame, transforms, sprites, ref screenCamera, DrawSpace.Screen);
        DrawLabels(monoGame, strings, transforms, labels, ref screenCamera, DrawSpace.Screen);
        
        monoGame.GraphicsDevice.SetRenderTarget(null);

        // draw the infal render target to the back buffer.
        monoGame.GraphicsDevice.SetRenderTarget(null);            
        monoGame.GraphicsDevice.Clear(Color.Black);
        monoGame.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp
        );
        monoGame.SpriteBatch.Draw(
            monoGame.FinalRenderTarget,
            RectangleExtensions.ToMonoGame(monoGame.DestinationRectangle), // this will probably need to be changed for calc dest rectangle.
            Color.White
        );
        monoGame.SpriteBatch.End();
    }

    /// <summary>
    /// Draws all sprites to the currently bound render target.
    /// </summary>
    /// <param name="ecs">The ecs state where the sprites are stored.</param>
    /// <param name="app">The state of the renderer.</param>
    /// <param name="camera">The camera to draw in relation to.</param>
    /// <param name="worldSpace">filters sprites; drawing sprites that are within the specified world space.</param>
    private static void DrawSprites(MonoGameAppState app, ComponentArray<Transform> transforms, ComponentArray<Sprite> sprites, ref Camera camera, DrawSpace worldSpace)
    {
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
    ///     Draws all texts to the currently bound render target.
    /// </summary>
    /// <param name="state">the state to draw to.</param>
    /// <param name="strings">the collection containing the string data for the labels.</param>
    /// <param name="transforms">the collection containing the label transforms.</param>
    /// <param name="labels">the labels to draw.</param>
    /// <param name="camera">the camera data to draw in relation to.</param>
    /// <param name="drawSpace">the space to draw in.</param>
    public static void DrawLabels(MonoGameAppState state, StringRegistryState strings, ComponentArray<Transform> transforms, ComponentArray<Label> labels, ref Camera camera, DrawSpace drawSpace)
    {
        state.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp, 
            rasterizerState: RasterizerState.CullNone, 
            effect: state.EffectManager.DefaultSpriteEffect
        );

        bool isValid = false;

        // draw labels.
        for(int i = 1; i < labels.Active.Count; i++)
        {
            GenId genId = labels.Active[i];
            ref Label label = ref ComponentArray.GetDataUnsafe(labels, genId);
            if(label.DrawSpace != drawSpace)
            {
                continue;
            }

            ref Transform transform = ref ComponentArray.GetDataUnsafe(transforms, genId);


            Span<char> chars = StringRegistry.GetString(strings, label.StringId, ref isValid);
            DrawLabel(state, ref camera, ref transform, ref label, chars);            
        }

        state.SpriteBatch.End();
    }

    /// <summary>
    /// Draws text to the currently bound render target.
    /// </summary>
    /// <param name="state">The renderer state containing drawing context.</param>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">The transformation to apply to the text.</param>
    /// <param name="chars">The span of characters to draw.</param>
    /// <param name="textParameters">The text parameters.</param>
    /// <returns><see cref="GenIndexResult"/></returns>
    public static void DrawLabel(MonoGameAppState state, ref Camera camera, ref Transform transform, ref Label label, Span<char> chars)
    {
        Font font = state.FontManagerState.Fonts[label.FontId];

        // fallback to nill if there is not font.
        if (font == null)
        {
            font = state.FontManagerState.Fonts[0];
        }

        Howl.Math.Vector2 position = transform.Position.InvertY() - camera.Position.InvertY();

        font.SpriteFontBase.DrawText(state.SpriteBatch, chars.ToString(), Vector2Extensions.ToMonoGame(position), 
            label.Colour.ToMonoGame(), -transform.Rotation, Vector2Extensions.ToMonoGame(label.Offset), 
            Vector2Extensions.ToMonoGame(transform.Scale)
        );
    }
}