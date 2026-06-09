using Howl.Graphics;
using Howl.Input;
using Howl.Math;
using Howl.Text;
using Howl.Unmanaged.Collections;
using Howl.Vendors.MonoGame;

namespace Howl;

public static class Renderer
{
    /// <summary>
    ///     Sets the application to windowed mode.
    /// </summary>
    public static void SetWindowed(HowlAppState app)
    {
        MonoGameApp.SetWindowed(app.MonoGameAppState);
    }

    /// <summary>
    ///     Sets the application to fullscreen mode.
    /// </summary>
    /// <param name="app"></param>
    public static void SetFullscreen(HowlAppState app)
    {
        MonoGameApp.SetFullscreen(app.MonoGameAppState);
    }

    /// <summary>
    ///     Sets the application to borderless fullscreen mode.
    /// </summary>
    /// <param name="app"></param>
    public static void SetBorderlessFullscreen(HowlAppState app)
    {
        MonoGameApp.SetBorderlessFullscreen(app.MonoGameAppState);
    }

    /// <summary>
    ///     Sets the target frame rate of the application.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="targetFrameRate"></param>
    public static void SetTargetFrameRate(HowlAppState app, TargetFrameRate targetFrameRate)
    {
        MonoGameApp.SetTargetFrameRate(app.MonoGameAppState, targetFrameRate);
    }

    /// <summary>
    ///     Sets the resolution of the final render target.
    /// </summary>
    /// <param name="app">the howl app instance.</param>
    /// <param name="resolution">the resolution to set.</param>
    public static void SetFinalRenderTargetResolution(HowlAppState app, Vector2Int resolution)
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
    public static void SetFinalRenderTargetResolution(HowlAppState app, int width, int height)
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
    public static bool RegisterTexture(HowlAppState app, string filePath, ref int textureId)
    {
        return Vendors.MonoGame.Graphics.TextureManager.RegisterTexture(app.MonoGameAppState.TextureManagerState, filePath, ref textureId);
    }

    /// <summary>
    ///     Loads a texture from disc into video memory.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="filePath">the file path of the texture.</param>
    /// <returns>true, if the texture was successfully loaded; otherwise false.</returns>
    public static bool LoadTexture(HowlAppState app, string filePath)
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
    public static bool GetTextureDimensions(HowlAppState app, int textureId, ref Vector2Int dimensions)
    {
        return Vendors.MonoGame.Graphics.TextureManager.GetTextureDimensions(app.MonoGameAppState.TextureManagerState, textureId, ref dimensions.X, ref dimensions.Y);
    }

    /// <summary>
    ///     Unloads a loaded texture from video memory.
    /// </summary>
    /// <param name="app">the howl app instance containing the loaded texture.</param>
    /// <param name="filePath">the file path of the registered texture to unload</param>
    /// <returns>true, if the texture was successfully unloaded; otherwise false.</returns>
    public static bool UnloadTexture(HowlAppState app, string filePath)
    {
        return Vendors.MonoGame.Graphics.TextureManager.UnloadTexture(app.MonoGameAppState.TextureManagerState, filePath);        
    }

    /// <summary>
    ///     Gets whether a texture has been loaded.
    /// </summary>
    /// <param name="app">the howl app instance containing the loaded texture.</param>
    /// <param name="textureId">the id of the texture.</param>
    /// <returns>true, if the texture has been loaded; otherwise false.</returns>
    public static bool IsTextureLoaded(HowlAppState app, int textureId)
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
    public static Sprite ConstructSprite(HowlAppState app, Colour colourTint, Math.Shapes.Rectangle sourceRectangle, Vector2 scale, int textureId, 
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
    public static Sprite ConstructSprite(HowlAppState app, Colour colourTint, Math.Shapes.Rectangle sourceRectangle, Vector2 scale, 
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
    public static int GetTextureId(HowlAppState app, string texturePath)
    {
        return Vendors.MonoGame.Graphics.TextureManager.GetTextureIndex(app.MonoGameAppState.TextureManagerState, texturePath);
    }

    /// <summary>
    ///     Draws.
    /// </summary>
    public static void Draw(ComponentArray<Transform> transforms, ComponentArray<Sprite> sprites, ComponentArray<Label> labels, 
        ref String.Allocator strings, HowlAppState app
    )
    {
        Vendors.MonoGame.Graphics.RendererSystem.Draw(app.MonoGameAppState, ref strings, sprites.Active, labels.Active, transforms.Sparse, sprites.Sparse, 
            labels.Sparse, app.WorldCamera, app.ScreenCamera
        );
    }

    /// <summary>
    ///     Sets the Nil texture value in a texture manager state instance.
    /// </summary>
    /// <param name="app">the howl app renderer instance to set the Nil value to.</param>
    /// <param name="filePath">the file path of the registered texture to load.</param>
    /// <returns>true; if the texture was successfully loaded; otherwise false.</returns>
    public static bool LoadNilTexture(HowlAppState app, string filePath)
    {
        return Vendors.MonoGame.Graphics.TextureManager.LoadNilTexture(app.MonoGameAppState.TextureManagerState, 
            app.MonoGameAppState.GraphicsDevice, filePath
        );   
    }




    /******************
    
        Constants.
    
    *******************/




    /// <summary>
    ///     The default thickness - in pixels - for a wireframe.
    /// </summary>
    public const float DefaultWireframeThickness = 2;
    
    /// <summary>
    ///     The default amount of point segments for a circle.
    /// </summary>
    public const int DefaultCircleVerticeAmount = 16;




    /******************
    
        Line.
    
    *******************/




    /// <summary>
    ///     Draws a line segement between two points.
    /// </summary>
    /// <param name="state">the howl app to store the line segement to draw.</param>
    /// <param name="colour">the colour of the line when drawing.</param>
    /// <param name="start">the start point of the line segment.</param>
    /// <param name="end">the end point of the line segment.</param>
    /// <param name="drawSpace">The drawing space to render the line.</param>
    /// <param name="thickness">the thickness of the line segment.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness of the line by the camera zoom.</param>
    public static void DrawLine(HowlAppState state, Colour colour, Vector2 start, Vector2 end, DrawSpace drawSpace, 
        float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        ref Camera camera = ref CameraSystem.GetDrawSpaceCamera(state, drawSpace);

        float outputResolutionHeight = state.MonoGameAppState.OutputResolution.Y;

        // convert to monogame.

        Microsoft.Xna.Framework.Vector2 mStart = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(start);
        Microsoft.Xna.Framework.Vector2 mEnd = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(end);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Howl.Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        DebugDrawState drawState = drawSpace switch
        {
            DrawSpace.World => state.MonoGameAppState.WorldSpaceDebugDrawState,
            DrawSpace.Screen => state.MonoGameAppState.ScreenSpaceDebugDrawState,
            _ => state.MonoGameAppState.ScreenSpaceDebugDrawState
        };

        // run monogame code.
        Howl.Vendors.MonoGame.DebugDraw.Line(drawState, mColour, mStart, mEnd, mCameraPos, camera.Zoom, camera.BaseVerticalFov, 
            outputResolutionHeight, thickness, scaleThickness
        );
    }




    /******************
    
        Rectangle.
    
    *******************/




    /// <summary>
    ///     Draws a wireframe rectangle.
    /// </summary>
    /// <param name="state">the howl app to store the wireframe rectangle to draw. </param>
    /// <param name="shape">the shape data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space to render the shape in.</param>
    /// <param name="thickness">the thickness of the wireframe line segments.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness of the wireframe by the camera zoom.</param>
    public static void DrawWireRect(HowlAppState state, Howl.Math.Shapes.Rectangle shape, Colour colour, DrawSpace drawSpace, 
        float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        ref Camera camera = ref CameraSystem.GetDrawSpaceCamera(state, drawSpace);
        float outputResolutionHeight = state.MonoGameAppState.OutputResolution.Y;

        // convert to monogame.        
        Microsoft.Xna.Framework.Vector2 mRectMin = new Microsoft.Xna.Framework.Vector2(shape.X, shape.Y - shape.Height);
        Microsoft.Xna.Framework.Vector2 mRectMax = new Microsoft.Xna.Framework.Vector2(shape.X + shape.Width, shape.Y);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Howl.Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);
        
        DebugDrawState drawState = drawSpace switch
        {
            DrawSpace.World => state.MonoGameAppState.WorldSpaceDebugDrawState,
            DrawSpace.Screen => state.MonoGameAppState.ScreenSpaceDebugDrawState,
            _ => state.MonoGameAppState.ScreenSpaceDebugDrawState
        };

        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.WireRect(drawState, mColour, mRectMin, mRectMax, mCameraPos, camera.Zoom, camera.BaseVerticalFov, 
            outputResolutionHeight, thickness, scaleThickness
        );
    }

    /// <summary>
    ///     Draws a filled rectangle.
    /// </summary>
    /// <param name="state">the howl app to store the filled rectangle to draw.</param>
    /// <param name="shape">the shape data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space to render that shape in.</param>
    public static void DrawFillRect(HowlAppState state, Howl.Math.Shapes.Rectangle shape, Colour colour, DrawSpace drawSpace)
    {
        ref Camera camera = ref CameraSystem.GetDrawSpaceCamera(state, drawSpace);

        // convert to monogame.
        Microsoft.Xna.Framework.Vector2 mRectMin = new Microsoft.Xna.Framework.Vector2(shape.X, shape.Y - shape.Height);
        Microsoft.Xna.Framework.Vector2 mRectMax = new Microsoft.Xna.Framework.Vector2(shape.X + shape.Width, shape.Y);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Howl.Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        DebugDrawState drawState = drawSpace switch
        {
            DrawSpace.World => state.MonoGameAppState.WorldSpaceDebugDrawState,
            DrawSpace.Screen => state.MonoGameAppState.ScreenSpaceDebugDrawState,
            _ => state.MonoGameAppState.ScreenSpaceDebugDrawState
        };

        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.FillRect(drawState, mColour, mRectMin, mRectMax, mCameraPos);
    }




    /******************
    
        Circle
    
    *******************/




    /// <summary>
    ///     Draws a wireframe circle.
    /// </summary>
    /// <param name="state">the howl app to store the wireframe circle to draw.</param>
    /// <param name="shape">the shape data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space to render the shape in.</param>
    /// <param name="thickness">the thickness of the wireframe line segments.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness of the wireframe by the camera zoom.</param>
    public static void DrawWireCircle(HowlAppState state, Howl.Math.Shapes.Circle shape, Colour colour, DrawSpace drawSpace, float thickness = DefaultWireframeThickness, 
        int verticeCount = DefaultCircleVerticeAmount, bool scaleThickness = true
    )
    {
        ref Camera camera = ref CameraSystem.GetDrawSpaceCamera(state, drawSpace);
        float outputResolutionHeight = state.MonoGameAppState.OutputResolution.Y;

        // convert to monogame.        

        Microsoft.Xna.Framework.Vector2 mCirclePos = new Microsoft.Xna.Framework.Vector2(shape.X, shape.Y);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Howl.Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);
    
        DebugDrawState drawState = drawSpace switch
        {
            DrawSpace.World => state.MonoGameAppState.WorldSpaceDebugDrawState,
            DrawSpace.Screen => state.MonoGameAppState.ScreenSpaceDebugDrawState,
            _ => state.MonoGameAppState.ScreenSpaceDebugDrawState
        };

        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.WireCircle(drawState, mColour, mCirclePos, shape.Radius, mCameraPos, 
            camera.Zoom, camera.BaseVerticalFov, outputResolutionHeight, thickness, verticeCount, scaleThickness
        );
    }

    /// <summary>
    ///     Draws a filled circle.
    /// </summary>
    /// <param name="state">the howl app to store the filled circle to draw.</param>
    /// <param name="shape">the shape data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space of the filled shape.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    public static void DrawFillCircle(HowlAppState state, Howl.Math.Shapes.Circle shape, Colour colour, DrawSpace drawSpace, 
        int verticeCount = DefaultCircleVerticeAmount
    )
    {
        ref Camera camera = ref CameraSystem.GetDrawSpaceCamera(state, drawSpace);

        DebugDrawState drawState = drawSpace switch
        {
            DrawSpace.World => state.MonoGameAppState.WorldSpaceDebugDrawState,
            DrawSpace.Screen => state.MonoGameAppState.ScreenSpaceDebugDrawState,
            _ => state.MonoGameAppState.ScreenSpaceDebugDrawState
        };

        // convert to monogame.        
        Microsoft.Xna.Framework.Vector2 mCirclePos = new Microsoft.Xna.Framework.Vector2(shape.X, shape.Y);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Howl.Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.FillCircle(drawState, mColour, mCirclePos, shape.Radius, mCameraPos, verticeCount);
    }




    /******************
    
        Polygon
    
    *******************/




    /// <summary>
    ///     Draws a wireframe polygon.
    /// </summary>
    /// <remarks>
    ///     This function assumes <c><paramref name="verticesX"/></c> is the same length as <c><paramref name="verticesY"/></c>.
    /// </remarks>
    /// <param name="state">the howl app to store the wireframe polygon to draw.</param>
    /// <param name="verticesX">the x-components of the polygon's vertices.</param>
    /// <param name="verticesY">the x-components of the polygon's vertices.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space of the wireframe shape.</param>
    /// <param name="thickness">the thickness of the wireframe line segments.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness of the wireframe by the camera zoom.</param>
    public static void DrawWirePoly(HowlAppState state, System.Span<float> verticesX, System.Span<float> verticesY, Colour colour, 
        DrawSpace drawSpace, float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        ref Camera camera = ref CameraSystem.GetDrawSpaceCamera(state, drawSpace);
        float outputResolutionHeight = state.MonoGameAppState.OutputResolution.Y;

        DebugDrawState drawState = drawSpace switch
        {
            DrawSpace.World => state.MonoGameAppState.WorldSpaceDebugDrawState,
            DrawSpace.Screen => state.MonoGameAppState.ScreenSpaceDebugDrawState,
            _ => state.MonoGameAppState.ScreenSpaceDebugDrawState
        };

        // convert to monogame.
        Microsoft.Xna.Framework.Vector2 mCameraPos = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Howl.Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.WirePoly(drawState, mColour, verticesX, verticesY, mCameraPos, camera.Zoom, camera.BaseVerticalFov, 
            outputResolutionHeight, thickness, scaleThickness
        );
    }
}