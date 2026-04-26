using System;
using Howl.Graphics;
using Howl.Vendors.MonoGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Howl.Vendors.MonoGame.FontStashSharp;

namespace Howl.Vendors.MonoGame;

public static class MonoGameApp
{

    /// <summary>
    ///     Sets the application window to be windowed.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    public static void SetWindowed(MonoGameAppState app)
    {
        if(app.GraphicsDeviceManager.IsFullScreen == false)
        {
            return;
        }

        // NOTE:
        // this may need to be removed.
        // monoGameApp.GraphicsDeviceManager.HardwareModeSwitch = false;

        app.GraphicsDeviceManager.ToggleFullScreen();
    }

    /// <summary>
    ///     Sets the application window to be fullscreen.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    public static void SetFullscreen(MonoGameAppState app)
    {
        if(app.GraphicsDeviceManager.IsFullScreen == true
        && app.GraphicsDeviceManager.HardwareModeSwitch == true)
        {
            return;
        }

        app.GraphicsDeviceManager.HardwareModeSwitch = true;
        app.GraphicsDeviceManager.ToggleFullScreen();


        // NOTE:
        // Monogame "corrupts" the computers back buffer when toggling fullscreen upon closing the application afterwards. 
        // The screen is fine for a split second then switches to the "Clear Colour" of the renderer.
        // nothing  can be clicked on the computer, alt+f4 doesnt work, it completely nukes the computer.
        
        // UpdateMainRenderDestinationRectangle(); <-- DONT DO THIS at the end of this, subscribe to the monogame OnGraphicsDeviceReset(object caller, EventArgs e).
    }

    /// <summary>
    ///     Sets the application window to be borderless fullscreen.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    public static void SetBorderlessFullscreen(MonoGameAppState app)
    {
        if(app.GraphicsDeviceManager.IsFullScreen == true
        && app.GraphicsDeviceManager.HardwareModeSwitch == false)
        {
            return;
        }

        app.GraphicsDeviceManager.HardwareModeSwitch = false;
        app.GraphicsDeviceManager.ToggleFullScreen();
    }

    /// <summary>
    ///     Sets the target frame rate of the renderer.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="targetFrameRate"the new target frame rate.></param>
    public static void SetTargetFrameRate(MonoGameAppState app, TargetFrameRate targetFrameRate)
    {
        switch (targetFrameRate)
        {
            case TargetFrameRate.D60:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(16);
            break;
            case TargetFrameRate.D90:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(11);
            break;
            case TargetFrameRate.D120:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(8);
            break;
            case TargetFrameRate.D144:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(7);
            break;
            case TargetFrameRate.D165:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(6f);
            break;
            case TargetFrameRate.D240:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(4);
            break;
            case TargetFrameRate.D360:
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(3);
            break;
        }
    }

    /// <summary>
    ///     Sets the resolution of the final render target.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="resolution">the resolution to set.</param>
    public static void SetFinalRenderTargetResolution(MonoGameAppState app, Howl.Math.Vector2Int resolution)
    {
        SetFinalRenderTargetResolution(app, resolution.X, resolution.Y);
    }

    /// <summary>
    ///     Sets the resolution of the final render target.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="width">the new width of the final render target.</param>
    /// <param name="height">the new height of the final render target.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void SetFinalRenderTargetResolution(MonoGameAppState app, int width, int height)
    {
        int clampedWidth = System.Math.Clamp(width, 1, int.MaxValue);  
        int clampedHeight = System.Math.Clamp(height, 1, int.MaxValue);  
        if(width == clampedWidth && height == clampedHeight)
        {            
            app.FinalRenderTarget?.Dispose();
            app.FinalRenderTarget = new RenderTarget2D(app.GraphicsDevice, width, height);
            app.OutputResolution = new Vector2(width, height);
        }
        else
        {
            throw new ArgumentException($"Output resolution cannot be set to ({width}, {height}), values must be above zero and lower than or equal to int.MaxValue");            
        }
    }

    /// <summary>
    ///     Constructs a sprite from a loaded texture.
    /// </summary>
    /// <remarks>
    ///     Note: if the texture is not loaded, the returned sprite will instead fallback to the Nil value texture.
    /// </remarks>
    /// <param name="state">the texture manager state containing the loaded texture.</param>
    /// <param name="colourTint">the colour to tint the sprite.</param>
    /// <param name="sourceRectangle">the source rectangle - in pixels - of the sprite on the texture image.</param>
    /// <param name="scale">the scaling vector to apply to the sprite when drawing.</param>
    /// <param name="textureFilePath">the file path of the loaded texture.</param>
    /// <param name="layerDepth">the layer depth.</param>
    /// <param name="spriteOrigin">where the origin of the sprite will be placed.</param>
    /// <param name="worldSpace">whether or not the sprite is in world space.</param>
    /// <returns>the newly constructed sprite.</returns>
    public static Sprite ConstructSprite(TextureManagerState state, Colour colourTint, Howl.Math.Shapes.Rectangle sourceRectangle, 
        Howl.Math.Vector2 scale, string textureFilePath, float layerDepth, SpriteOrigin spriteOrigin, DrawSpace worldSpace
    )
    {
        int textureIndex = TextureManager.GetTextureIndex(state, textureFilePath);
        return ConstructSprite(state, colourTint, sourceRectangle, scale, textureIndex, layerDepth, spriteOrigin, worldSpace);
    }

    /// <summary>
    ///     Constructs a sprite from a loaded texture.
    /// </summary>
    /// <remarks>
    ///     Note: if the texture is not loaded, the returned sprite will instead fallback to the Nil value texture.
    /// </remarks>
    /// <param name="state">the texture manager state containing the loaded texture.</param>
    /// <param name="colourTint">the colour to tint the sprite.</param>
    /// <param name="sourceRectangle">the source rectangle - in pixels - of the sprite on the texture image.</param>
    /// <param name="scale">the scaling vector to apply to the sprite when drawing.</param>
    /// <param name="textureIndex">the index of the loaded texture.</param>
    /// <param name="layerDepth">the layer depth.</param>
    /// <param name="spriteOrigin">where the origin of the sprite will be placed.</param>
    /// <param name="worldSpace">whether or not the sprite is in world space.</param>
    /// <returns>the newly constructed sprite.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Sprite ConstructSprite(TextureManagerState state, Colour colourTint, Howl.Math.Shapes.Rectangle sourceRectangle, 
        Howl.Math.Vector2 scale, int textureIndex, float layerDepth, SpriteOrigin spriteOrigin, DrawSpace worldSpace
    )
    {
        if(state.Textures[textureIndex] == null)
        {
            // return the nil value if the texture is not loaded.
            textureIndex = 0;
        }

        // return the requested sprite.
        ref Texture2D texture = ref state.Textures[textureIndex];
        float originX = 0;
        float originY = 0;

        switch (spriteOrigin)
        {
            case SpriteOrigin.Center:
                int width = 0;
                int height = 0;
                TextureManager.GetTextureDimensionsUnsafe(state, textureIndex, ref width, ref height);
                originX = width*0.5f;
                originY = height*0.5f;
                break;
            case SpriteOrigin.TopLeft:
                // do nothing as origin x and y are already zero.
                break;
            default:
                goto case SpriteOrigin.TopLeft;
        }

        return new Sprite(sourceRectangle, colourTint, new Howl.Math.Vector2(originX, originY), scale, textureIndex, worldSpace, layerDepth);
    }

    /// <summary>
    ///     Links a monogame app to its internal events.
    /// </summary>
    /// <param name="app">the monogame app instance to link.</param>
    public static void LinkEvents(MonoGameAppState app)
    {
        app.GraphicsDevice.DeviceReset += app.OnGraphicsDeviceReset;
    }

    /// <summary>
    ///     Unlinks a monogame app from its internal events.
    /// </summary>
    /// <param name="app">the monogame ap instance to unlink.</param>
    public static void UnlinkEvents(MonoGameAppState app)
    {
        app.GraphicsDevice.DeviceReset -= app.OnGraphicsDeviceReset;        
    }

    /// <summary>
    ///     Disposes of a state instance.
    /// </summary>
    /// <param name="app">the state instance to dispose of.</param>
    public static void Dispose(MonoGameAppState app)
    {
        if (app.IsDisposed)
        {
            return;
        }

        app.IsDisposed = true;
        UnlinkEvents(app);

        FontStashSharp.FontManager.Dispose(app.FontManagerState);
        app.FontManagerState = null;

        DebugDraw.Dispose(app.DebugDrawState);
        app.DebugDrawState = null;

        TextureManagerState.Dispose(app.TextureManagerState);
        app.TextureManagerState = null;

        app.UpdateCallback = null;
        app.RenderCallback = null;
        
        // call the MonoGame 'Game' class dispose.
        app.Dispose();
    }
}