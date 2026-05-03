using Howl.Graphics;
using Howl.Math;
using System;

namespace Howl.Debug;

public static class Draw
{





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
    /// <param name="app">the howl app to store the line segement to draw.</param>
    /// <param name="colour">the colour of the line when drawing.</param>
    /// <param name="start">the start point of the line segment.</param>
    /// <param name="end">the end point of the line segment.</param>
    /// <param name="drawSpace">The drawing space to render the line.</param>
    /// <param name="thickness">the thickness of the line segment.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness of the line by the camera zoom.</param>
    public static void Line(HowlAppState app, Colour colour, Vector2 start, Vector2 end, DrawSpace drawSpace, 
        float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        Camera camera = default;
        CameraSystem.GetDrawSpaceCamera(app.EcsState, drawSpace, ref camera);
        float outputResolutionHeight = app.MonoGameAppState.OutputResolution.Y;

        // convert to monogame.

        Microsoft.Xna.Framework.Vector2 mStart = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(start);
        Microsoft.Xna.Framework.Vector2 mEnd = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(end);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        // run monogame code.

        Vendors.MonoGame.DebugDraw.Line(app.MonoGameAppState.DebugDrawState, mColour, mStart, mEnd, mCameraPos, camera.Zoom, camera.BaseVerticalFov, 
            outputResolutionHeight, thickness, scaleThickness
        );
    }




    /******************
    
        Rectangle.
    
    *******************/




    /// <summary>
    ///     Draws a wireframe rectangle.
    /// </summary>
    /// <param name="app">the howl app to store the wireframe rectangle to draw. </param>
    /// <param name="shape">the shape data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space to render the shape in.</param>
    /// <param name="thickness">the thickness of the wireframe line segments.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness of the wireframe by the camera zoom.</param>
    public static void WireRect(HowlAppState app, Math.Shapes.Rectangle shape, Colour colour, DrawSpace drawSpace, 
        float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        Camera camera = default;
        CameraSystem.GetDrawSpaceCamera(app.EcsState, drawSpace, ref camera);
        float outputResolutionHeight = app.MonoGameAppState.OutputResolution.Y;

        // convert to monogame.        
        Microsoft.Xna.Framework.Vector2 mRectMin = new Microsoft.Xna.Framework.Vector2(shape.X, shape.Y - shape.Height);
        Microsoft.Xna.Framework.Vector2 mRectMax = new Microsoft.Xna.Framework.Vector2(shape.X + shape.Width, shape.Y);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);
        
        // execute monogame code.
        Vendors.MonoGame.DebugDraw.WireRect(app.MonoGameAppState.DebugDrawState, mColour, mRectMin, mRectMax, mCameraPos, camera.Zoom, camera.BaseVerticalFov, 
            outputResolutionHeight, thickness, scaleThickness
        );
    }

    /// <summary>
    ///     Draws a filled rectangle.
    /// </summary>
    /// <param name="app">the howl app to store the filled rectangle to draw.</param>
    /// <param name="shape">the shape data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space to render that shape in.</param>
    public static void FillRect(HowlAppState app, Math.Shapes.Rectangle shape, Colour colour, DrawSpace drawSpace)
    {
        Camera camera = default;
        CameraSystem.GetDrawSpaceCamera(app.EcsState, drawSpace, ref camera);

        // convert to monogame.
        Microsoft.Xna.Framework.Vector2 mRectMin = new Microsoft.Xna.Framework.Vector2(shape.X, shape.Y - shape.Height);
        Microsoft.Xna.Framework.Vector2 mRectMax = new Microsoft.Xna.Framework.Vector2(shape.X + shape.Width, shape.Y);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        // execute monogame code.
        Vendors.MonoGame.DebugDraw.FillRect(app.MonoGameAppState.DebugDrawState, mColour, mRectMin, mRectMax, mCameraPos);
    }




    /******************
    
        Circle
    
    *******************/




    /// <summary>
    ///     Draws a wireframe circle.
    /// </summary>
    /// <param name="app">the howl app to store the wireframe circle to draw.</param>
    /// <param name="shape">the shape data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space to render the shape in.</param>
    /// <param name="thickness">the thickness of the wireframe line segments.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness of the wireframe by the camera zoom.</param>
    public static void WireCircle(HowlAppState app, Math.Shapes.Circle shape, Colour colour, DrawSpace drawSpace, float thickness = DefaultWireframeThickness, 
        int verticeCount = DefaultCircleVerticeAmount, bool scaleThickness = true
    )
    {
        Camera camera = default;
        CameraSystem.GetDrawSpaceCamera(app.EcsState, drawSpace, ref camera);
        float outputResolutionHeight = app.MonoGameAppState.OutputResolution.Y;

        // convert to monogame.        

        Microsoft.Xna.Framework.Vector2 mCirclePos = new Microsoft.Xna.Framework.Vector2(shape.X, shape.Y);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);
    
        // execute monogame code.
        Vendors.MonoGame.DebugDraw.WireCircle(app.MonoGameAppState.DebugDrawState, mColour, mCirclePos, shape.Radius, mCameraPos, 
            camera.Zoom, camera.BaseVerticalFov, outputResolutionHeight, thickness, verticeCount, scaleThickness
        );
    }

    /// <summary>
    ///     Draws a filled circle.
    /// </summary>
    /// <param name="app">the howl app to store the filled circle to draw.</param>
    /// <param name="shape">the shape data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space of the filled shape.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    public static void FillCircle(HowlAppState app, Math.Shapes.Circle shape, Colour colour, DrawSpace drawSpace, 
        int verticeCount = DefaultCircleVerticeAmount
    )
    {
        Camera camera = default;
        CameraSystem.GetDrawSpaceCamera(app.EcsState, drawSpace, ref camera);

        // convert to monogame.        
        Microsoft.Xna.Framework.Vector2 mCirclePos = new Microsoft.Xna.Framework.Vector2(shape.X, shape.Y);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        // execute monogame code.
        Vendors.MonoGame.DebugDraw.FillCircle(app.MonoGameAppState.DebugDrawState, mColour, mCirclePos, shape.Radius, mCameraPos, verticeCount);
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
    /// <param name="app">the howl app to store the wireframe polygon to draw.</param>
    /// <param name="verticesX">the x-components of the polygon's vertices.</param>
    /// <param name="verticesY">the x-components of the polygon's vertices.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="drawSpace">the drawing space of the wireframe shape.</param>
    /// <param name="thickness">the thickness of the wireframe line segments.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness of the wireframe by the camera zoom.</param>
    public static void WirePoly(HowlAppState app, Span<float> verticesX, Span<float> verticesY, Colour colour, DrawSpace drawSpace,
        float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        Camera camera = default;
        CameraSystem.GetDrawSpaceCamera(app.EcsState, drawSpace, ref camera);
        float outputResolutionHeight = app.MonoGameAppState.OutputResolution.Y;

        // convert to monogame.
        Microsoft.Xna.Framework.Vector2 mCameraPos = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        // execute monogame code.
        Vendors.MonoGame.DebugDraw.WirePoly(app.MonoGameAppState.DebugDrawState, mColour, verticesX, verticesY, mCameraPos, camera.Zoom, camera.BaseVerticalFov, 
            outputResolutionHeight, thickness, scaleThickness
        );
    }
}