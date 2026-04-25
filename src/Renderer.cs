using Howl.Ecs;
using Howl.Graphics;
using Howl.Input;
using Howl.Math;
using Howl.Vendors.MonoGame;

namespace Howl;

public static class Renderer
{
    /// <summary>
    ///     Sets the application to windowed mode.
    /// </summary>
    public static void SetWindowed(HowlApp app)
    {
        MonoGameApp.SetWindowed(app.MonoGameAppState);
    }

    /// <summary>
    ///     Sets the application to fullscreen mode.
    /// </summary>
    /// <param name="app"></param>
    public static void SetFullscreen(HowlApp app)
    {
        MonoGameApp.SetFullscreen(app.MonoGameAppState);
    }

    /// <summary>
    ///     Sets the application to borderless fullscreen mode.
    /// </summary>
    /// <param name="app"></param>
    public static void SetBorderlessFullscreen(HowlApp app)
    {
        MonoGameApp.SetBorderlessFullscreen(app.MonoGameAppState);
    }

    /// <summary>
    ///     Sets the target frame rate of the application.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="targetFrameRate"></param>
    public static void SetTargetFrameRate(HowlApp app, TargetFrameRate targetFrameRate)
    {
        MonoGameApp.SetTargetFrameRate(app.MonoGameAppState, targetFrameRate);
    }

    /// <summary>
    ///     Sets the resolution of the final render target.
    /// </summary>
    /// <param name="app">the howl app instance.</param>
    /// <param name="resolution">the resolution to set.</param>
    public static void SetFinalRenderTargetResolution(HowlApp app, Vector2Int resolution)
    {
        MonoGameApp.SetFinalRenderTargetResolution(app.MonoGameAppState, resolution.X, resolution.Y);
    }

    /// <summary>
    ///     Sets the resolution of the final render target.
    /// </summary>
    /// <param name="app">the howl app instance.</param>
    /// <param name="width">the new width of the final render target.</param>
    /// <param name="height">the new height of the final render target.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void SetFinalRenderTargetResolution(HowlApp app, int width, int height)
    {
        MonoGameApp.SetFinalRenderTargetResolution(app.MonoGameAppState, width, height);
    }

    /// <summary>
    ///     Registers a texture into the renderer instance.
    /// </summary>
    /// <param name="app">the howl app containing the renderer instance.</param>
    /// <param name="filePath">the file path of the texture.</param>
    /// <param name="textureId">output for the assigned id of the texture in the renderer state.</param>
    /// <returns>tre, if ther texture was successfully registered; otherwise false.</returns>
    public static bool RegisterTexture(HowlApp app, string filePath, ref int textureId)
    {
        return Vendors.MonoGame.Graphics.TextureManager.RegisterTexture(app.MonoGameAppState.TextureManagerState, filePath, ref textureId);
    }

    /// <summary>
    ///     Loads a texture from disc into video memory.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="filePath">the file path of the texture.</param>
    /// <returns>true, if the texture was successfully loaded; otherwise false.</returns>
    public static bool LoadTexture(HowlApp app, string filePath)
    {
        return Vendors.MonoGame.Graphics.TextureManager.LoadTexture(app.MonoGameAppState.TextureManagerState, app.MonoGameAppState.GraphicsDevice, filePath);
    }

    /// <summary>
    ///     Gets the dimensions of a loaded texture in pixels.
    /// </summary>
    /// <param name="app">the howl app instance containing the loaded texture.</param>
    /// <param name="textureId">the texture id.</param>
    /// <param name="dimensions">output for the texture dimensions.</param>
    /// <returns>true, if the texture's dimensions were successfully retrieved otherwise false.</returns>
    public static bool GetTextureDimensions(HowlApp app, int textureId, ref Vector2Int dimensions)
    {
        return Vendors.MonoGame.Graphics.TextureManager.GetTextureDimensions(app.MonoGameAppState.TextureManagerState, textureId, ref dimensions.X, ref dimensions.Y);
    }

    /// <summary>
    ///     Unloads a loaded texture from video memory.
    /// </summary>
    /// <param name="app">the howl app instance containing the loaded texture.</param>
    /// <param name="filePath">the file path of the registered texture to unload</param>
    /// <returns>true, if the texture was successfully unloaded; otherwise false.</returns>
    public static bool UnloadTexture(HowlApp app, string filePath)
    {
        return Vendors.MonoGame.Graphics.TextureManager.UnloadTexture(app.MonoGameAppState.TextureManagerState, filePath);        
    }

    /// <summary>
    ///     Gets whether a texture has been loaded.
    /// </summary>
    /// <param name="app">the howl app instance containing the loaded texture.</param>
    /// <param name="textureId">the id of the texture.</param>
    /// <returns>true, if the texture has been loaded; otherwise false.</returns>
    public static bool IsTextureLoaded(HowlApp app, int textureId)
    {
        return app.MonoGameAppState.TextureManagerState.Textures[textureId] != null;
    }

    /// <summary>
    ///     Constructs a sprite from a loaded texture.
    /// </summary>
    /// <param name="app">the howl app instance containing the loaded texture.</param>
    /// <param name="colourTint">the colour to tint the sprite.</param>
    /// <param name="sourceRectangle">the source rectangle - in pixels - of the sprite on the texture image.</param>
    /// <param name="scale">the scaling vector to apply to the sprite when drawing.</param>
    /// <param name="textureFilePath">the file path of the loaded texture.</param>
    /// <param name="layerDepth">the layer depth.</param>
    /// <param name="spriteOrigin">where the origin of the sprite will be placed.</param>
    /// <param name="worldSpace">whether or not the sprite is in world space.</param>
    /// <returns>the newly constructed sprite.</returns>
    public static Sprite ConstructSprite(HowlApp app, Colour colourTint, Math.Shapes.Rectangle sourceRectangle, Vector2 scale, int textureId, 
        float layerDepth, SpriteOrigin spriteOrigin, DrawSpace worldSpace
    )
    {
        return MonoGameApp.ConstructSprite(app.MonoGameAppState.TextureManagerState, colourTint, sourceRectangle, scale, textureId, 
            layerDepth, spriteOrigin, worldSpace
        );
    }

    /// <summary>
    ///     Constructs a sprite from a loaded texture.
    /// </summary>
    /// <param name="app">the howl app instance containing the loaded texture.</param>
    /// <param name="colourTint">the colour to tint the  </param>
    /// <param name="sourceRectangle"></param>
    /// <param name="scale"></param>
    /// <param name="textureFilePath"></param>
    /// <param name="layerDepth"></param>
    /// <param name="spriteOrigin"></param>
    /// <param name="worldSpace"></param>
    /// <returns></returns>
    public static Sprite ConstructSprite(HowlApp app, Colour colourTint, Math.Shapes.Rectangle sourceRectangle, Vector2 scale, 
        string textureFilePath, float layerDepth, SpriteOrigin spriteOrigin, DrawSpace worldSpace
    )
    {
        return MonoGameApp.ConstructSprite(app.MonoGameAppState.TextureManagerState, colourTint, sourceRectangle, scale, textureFilePath, 
            layerDepth, spriteOrigin, worldSpace
        );
    }

    /// <summary>
    ///     Gets the texture 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="texturePath"></param>
    /// <returns></returns>
    public static int GetTextureId(HowlApp app, string texturePath)
    {
        return Vendors.MonoGame.Graphics.TextureManager.GetTextureIndex(app.MonoGameAppState.TextureManagerState, texturePath);
    }

    /// <summary>
    ///     Sets the Nil texture value in a texture manager state instance.
    /// </summary>
    /// <param name="app">the howl app renderer instance to set the Nil value to.</param>
    /// <param name="filePath">the file path of the registered texture to load.</param>
    /// <returns>true; if the texture was successfully loaded; otherwise false.</returns>
    public static bool LoadNilTexture(HowlApp app, string filePath)
    {
        return Vendors.MonoGame.Graphics.TextureManager.LoadNilTexture(app.MonoGameAppState.TextureManagerState, 
            app.MonoGameAppState.GraphicsDevice, filePath
        );   
    }
}