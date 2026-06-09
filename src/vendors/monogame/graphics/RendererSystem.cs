using Howl.Graphics;
using Howl.Vendors.MonoGame.Math.Shapes;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework.Graphics;
using Howl.Math;
using Microsoft.Xna.Framework;
using Howl.Vendors.MonoGame.FontStashSharp;
using Howl.Text;
using Howl.Unmanaged.Collections;
using System.Runtime.CompilerServices;

namespace Howl.Vendors.MonoGame.Graphics;

public static class RendererSystem
{    
    /// <summary>
    ///     Performs a draw step for a monogame app state.
    /// </summary>
    public static void Draw(MonoGameAppState monoGame, ref String.Allocator strings, SwapBackArray<int> activeSprites, 
        SwapBackArray<int> activeLabels, Array<Transform> transforms, Array<Sprite> sprites, Array<Label> labels, 
        Camera worldCamera, Camera screenCamera
    )
    {
        monoGame.GraphicsDevice.SetRenderTarget(monoGame.FinalRenderTarget);                    
        monoGame.GraphicsDevice.Clear(worldCamera.ClearColour.ToMonoGame());
        
        monoGame.EffectManager.UpdateProjectionMatrix(worldCamera.ProjectionMatrix.ToMonoGame());
        DrawSprites(monoGame, activeSprites, transforms, sprites, worldCamera.Position.X, worldCamera.Position.Y, DrawSpace.World);
        DrawLabels(monoGame, strings, activeLabels, transforms, labels, worldCamera.Position.X, worldCamera.Position.Y,  DrawSpace.World);
        DrawPrimitives(monoGame.WorldSpaceDebugDrawState, monoGame.GraphicsDevice, monoGame.EffectManager.PrimitivesEffect);

        monoGame.EffectManager.UpdateProjectionMatrix(screenCamera.ProjectionMatrix.ToMonoGame());
        DrawSprites(monoGame, activeSprites, transforms, sprites, screenCamera.Position.X, screenCamera.Position.Y, DrawSpace.Screen);
        DrawLabels(monoGame, strings, activeLabels, transforms, labels, screenCamera.Position.X, screenCamera.Position.Y, DrawSpace.Screen);
        DrawPrimitives(monoGame.ScreenSpaceDebugDrawState, monoGame.GraphicsDevice, monoGame.EffectManager.PrimitivesEffect);
        
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
    ///     Draws all sprites to the currently bound render target.
    /// </summary>
    private static void DrawSprites(MonoGameAppState app, SwapBackArray<int> activeSprites, 
        Array<Transform> spriteTransforms, Array<Sprite> sprites, float cameraPosX, float cameraPosY, DrawSpace drawSpace
    )
    {
        app.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp, 
            rasterizerState: RasterizerState.CullNone,
            effect: app.EffectManager.DefaultSpriteEffect
        );   

        // draw sprites in relation to it.
        for(int i = 1; i < activeSprites.Count; i++)
        {
            int index = activeSprites[i];
            ref Sprite sprite = ref sprites[index];
            if(sprite.DrawSpace != drawSpace)
            {
                continue;
            }

            ref Transform transform = ref spriteTransforms[index];
            DrawSprite(app, ref transform, ref sprite, cameraPosX, cameraPosY);
        }
        app.SpriteBatch.End();
    }

    /// <summary>
    ///     Draws a sprite to the currently bound render target.
    /// </summary>
    public static void DrawSprite(MonoGameAppState app, ref Transform spriteTransform, ref Sprite sprite, float cameraPosX, float cameraPosY)
    {   
        // translate by the cameras position.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Howl.Math.Vector2 position = spriteTransform.Position;
        position.Y *= -1;
        position -= new Howl.Math.Vector2(cameraPosX, -cameraPosY);
        
        ref Texture2D texture = ref app.TextureManagerState.Textures[sprite.TextureId];
        if(texture == null)

        app.EffectManager.DefaultSpriteEffect.Texture = texture;

        app.SpriteBatch.Draw(texture, new(position.X, position.Y), RectangleExtensions.ToMonoGame(sprite.SourceRectangle),
            sprite.ColourTint.ToMonoGame(), -spriteTransform.RotationRadians, // rotate with negative rotation as sprite batch draws in reverse for some reason. 
            Vector2Extensions.ToMonoGame(sprite.Origin), Vector2Extensions.ToMonoGame(sprite.Scale * spriteTransform.Scale), 
            SpriteEffects.None, sprite.LayerDepth
        );
    }

    /// <summary>
    /// Draws all stored primitive shapes to the next frame/screen, clearing the internal primitives cache when drawn for the frame/screen after. 
    /// </summary>
    private static void DrawPrimitives(DebugDrawState state, GraphicsDevice gD, BasicEffect effect)
    {

        if(state.PrimitiveIndices.Count == 0 || state.PrimitiveVertices.Count == 0)
        {
            return;
        }
        
        if(gD == null)
        {
            return;
        }

        foreach(EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();            
            gD.DrawUserIndexedPrimitives(
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
            throw new System.ArgumentException($"BackBuffer resolution cannot be set to ({width}, {height}), values must be above zero and lower than or equal to int.MaxValue");            
        }
    }

    /// <summary>
    ///     Calculates the detination rectangle for a render target onto the backbuffer of the window this application is painting to.
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
    public static void DrawLabels(MonoGameAppState state, String.Allocator strings, SwapBackArray<int> activeLabels, 
        Array<Transform> transforms, Array<Label> labels, float cameraPosX, float cameraPosY, DrawSpace drawSpace
    )
    {
        state.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend, 
            samplerState: SamplerState.PointClamp, 
            rasterizerState: RasterizerState.CullNone, 
            effect: state.EffectManager.DefaultSpriteEffect
        );

        GenIdResult result = default;

        // draw labels.
        int count = activeLabels.Count;
        for(int i = 1; i < count; i++)
        {
            int index = activeLabels[i];
            
            ref Label label = ref labels[index];
            if(label.DrawSpace != drawSpace)
            {
                continue;
            }

            ref Transform transform = ref transforms[index];
            
            ref String str = ref String.Allocator.GetString(ref strings, label.StringId, ref result);
            DrawLabel(state, ref transform, ref label, str, cameraPosX, cameraPosY);            
        }

        state.SpriteBatch.End();
    }

    /// <summary>
    ///     Draws text to the currently bound render target.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DrawLabel(MonoGameAppState state, ref Transform labelTransform, ref Label label, String str, 
        float cameraPosX, float cameraPosY
    )
    {
        Font font = state.FontManagerState.Fonts[label.FontId];

        // fallback to nill if there is not font.
        if (font == null)
        {
            font = state.FontManagerState.Fonts[0];
        }

        // translate by the cameras position.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Howl.Math.Vector2 position = labelTransform.Position;
        position.Y *= -1;
        position.X -= cameraPosX;
        position.Y -= -cameraPosY;

        font.SpriteFontBase.DrawText(state.SpriteBatch, String.ToSystemString(str), Vector2Extensions.ToMonoGame(position), 
            label.Colour.ToMonoGame(), -labelTransform.RotationRadians, Vector2Extensions.ToMonoGame(label.Offset), 
            Vector2Extensions.ToMonoGame(labelTransform.Scale)
        );
    }
}