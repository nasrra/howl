using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Howl;
using Howl.Graphics;
using Howl.Math;
using Howl.Vendors;

public static class Debug
{
    /// <summary>
    ///     Enables and disables debug logging.
    /// </summary>
    public static bool SuppressLog = false;

    public static ConsoleColor LogErrorColour = ConsoleColor.Red;
    public static ConsoleColor LogWarningColour = ConsoleColor.Yellow;
    public static ConsoleColor LogInfoColour = ConsoleColor.Cyan;
    public static ConsoleColor LogTextColour = ConsoleColor.Cyan;

    public static string InfoTag = "[Info]";
    public static string WarningTag = "[Warning]";
    public static string ErrorTag = "[Error]";

    public static void Log(string msg, int stackDepth = 0, int stackStart = 0, [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0
    )
    {
        if (SuppressLog)
        {
            return;
        }        

        Console.ForegroundColor = LogTextColour;

        if(stackDepth > 0)
        {            
            // Passing (stackStart + 1) discards Debug.Log AND the wrapper (LogError/LogWarning)
            var stackTrace = new StackTrace(stackStart + 1);

            string stack = "";

            for(int i = 0; i < stackDepth; i--)
            {                
                var frame = stackTrace.GetFrame(i);

                // Stop if the stack isn't deep enough
                if (frame == null) 
                    break; 
                
                var method = frame.GetMethod();
                
                if(method != null)
                {
                    stack += $"{method.DeclaringType?.Name ?? "Unknown"}";            
                }
            }
            Console.WriteLine($"{stack}.{methodName}():line {lineNumber}, {msg}");
        }
        else
        {   
            Console.WriteLine($"{msg}");        
        }
    }

    public static void LogInfo(string msg, int stackDepth = 0, [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0
    )
    {
        if (SuppressLog)
        {
            return;
        }        

        Console.ForegroundColor = LogInfoColour;
        Console.Write($"{InfoTag} ");
        Log(msg, stackDepth, 1, filePath, methodName, lineNumber);
    }

    public static void LogWarning(string msg, int stackDepth = 0, [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0
    )
    {
        if (SuppressLog)
        {
            return;
        }        

        Console.ForegroundColor = LogWarningColour;
        Console.Write($"{WarningTag} ");
        Console.ForegroundColor = LogTextColour;
        Log(msg, stackDepth, 1, filePath, methodName, lineNumber);
    }

    public static void LogError(string msg, int stackDepth = 0, [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0
    )
    {
        if (SuppressLog)
        {
            return;
        }        

        Console.ForegroundColor = LogErrorColour;
        Console.Write($"{ErrorTag} ");
        Console.ForegroundColor = LogTextColour;
        Log(msg, stackDepth, 1, filePath, methodName, lineNumber);
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

        // run monogame code.

        Howl.Vendors.MonoGame.DebugDraw.Line(state.MonoGameAppState.DebugDrawState, mColour, mStart, mEnd, mCameraPos, camera.Zoom, camera.BaseVerticalFov, 
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
        
        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.WireRect(state.MonoGameAppState.DebugDrawState, mColour, mRectMin, mRectMax, mCameraPos, camera.Zoom, camera.BaseVerticalFov, 
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

        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.FillRect(state.MonoGameAppState.DebugDrawState, mColour, mRectMin, mRectMax, mCameraPos);
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
    
        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.WireCircle(state.MonoGameAppState.DebugDrawState, mColour, mCirclePos, shape.Radius, mCameraPos, 
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

        // convert to monogame.        
        Microsoft.Xna.Framework.Vector2 mCirclePos = new Microsoft.Xna.Framework.Vector2(shape.X, shape.Y);
        Microsoft.Xna.Framework.Vector2 mCameraPos = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Howl.Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.FillCircle(state.MonoGameAppState.DebugDrawState, mColour, mCirclePos, shape.Radius, mCameraPos, verticeCount);
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
    public static void DrawWirePoly(HowlAppState state, Span<float> verticesX, Span<float> verticesY, Colour colour, DrawSpace drawSpace,
        float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        ref Camera camera = ref CameraSystem.GetDrawSpaceCamera(state, drawSpace);
        float outputResolutionHeight = state.MonoGameAppState.OutputResolution.Y;

        // convert to monogame.
        Microsoft.Xna.Framework.Vector2 mCameraPos = Howl.Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Color mColour = Howl.Vendors.MonoGame.Graphics.ColourExtensions.ToMonoGame(colour);

        // execute monogame code.
        Howl.Vendors.MonoGame.DebugDraw.WirePoly(state.MonoGameAppState.DebugDrawState, mColour, verticesX, verticesY, mCameraPos, camera.Zoom, camera.BaseVerticalFov, 
            outputResolutionHeight, thickness, scaleThickness
        );
    }
}