using System;
using System.Runtime.CompilerServices;
using Howl.Vendors.MonoGame.Graphics;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework;

namespace Howl.Vendors.MonoGame;

public static class DebugDraw
{

    


    /******************
    
        Line
    
    *******************/





    /// <summary>
    ///     Draws a line between two points.
    /// </summary>
    /// <param name="state">the debug draw state to store the line to draw.</param>
    /// <param name="color">the color of the line.</param>
    /// <param name="start">the starting position of the line segment.</param>
    /// <param name="end">the end position of the line segment.</param>
    /// <param name="cameraPosition">the position of the camera.</param>
    /// <param name="cameraZoom">the zoom level of the camera.</param>
    /// <param name="cameraVerticalFov">the camera vertical fov; how many units/pixels the camera can seen.</param>
    /// <param name="outputResolutionHeight">the height of the output resolution of the monogame application.</param>
    /// <param name="thickness">the thickness - in pixels - in relation to the output resolution.</param>
    /// <param name="scaleThickness">whether or not the line should scale with the camera zoom.</param>
    public static void Line(DebugDrawState state, Color color, Vector2 start, Vector2 end, Vector2 cameraPosition, float cameraZoom, float cameraVerticalFov, 
        float outputResolutionHeight, float thickness, bool scaleThickness
    )
    {
        if (scaleThickness)
        {
            thickness /= cameraZoom * ( outputResolutionHeight / cameraVerticalFov );
        }

        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        start.Y  *= -1;
        end.Y    *= -1;

        System.Math.Clamp(thickness, float.Epsilon, float.MaxValue);
        float halfThickness = thickness * 0.5f;

        // note that we apply the half thickness to the direction so that the line segment
        // corners are offseted by the thickness amount.
        float distanceX = end.X - start.X;
        float distanceY = end.Y - start.Y;
        Howl.Math.Math.Normalise(distanceX, distanceY, out float directionX, out float directionY);
        directionX *= halfThickness;
        directionY *= halfThickness;
        float oppositeDirectionX = -directionX;
        float oppositeDirectionY = -directionY;
        
        float normalX = oppositeDirectionY;
        float normalY = directionX;
        float oppositeNormalX = -normalX;
        float oppositeNormalY = -normalY;

        // Note: triangle vertices and indexes are done in
        // a clockwise motion. 
        int totalVertices = state.PrimitiveVertices.Count;
        StackArray.Push(state.PrimitiveIndices, totalVertices);
        StackArray.Push(state.PrimitiveIndices, totalVertices+1);
        StackArray.Push(state.PrimitiveIndices, totalVertices+2);
        StackArray.Push(state.PrimitiveIndices, totalVertices);
        StackArray.Push(state.PrimitiveIndices, totalVertices+2);
        StackArray.Push(state.PrimitiveIndices, totalVertices+3);


        // translate in relation to the camera.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Vector3 tangCamPos = new(cameraPosition.X, -cameraPosition.Y, 0);
        Vector3 corner1 = -tangCamPos;
        Vector3 corner2 = -tangCamPos;
        Vector3 corner3 = -tangCamPos;
        Vector3 corner4 = -tangCamPos;

        // apply the line world coordinates.
        corner1 += new Vector3(
            start.X + normalX + oppositeDirectionX,
            start.Y + normalY + oppositeDirectionY,
            0
        );

        corner2 += new Vector3(
            end.X + normalX + directionX,
            end.Y + normalY + directionY,
            0
        );

        corner3 += new Vector3(
            end.X + oppositeNormalX + directionX,
            end.Y + oppositeNormalY + directionY,
            0
        );

        corner4 += new Vector3(
            start.X + oppositeNormalX + oppositeDirectionX,
            start.Y + oppositeNormalY + oppositeDirectionY,
            0
        );

        StackArray.Push(state.PrimitiveVertices, new(corner1, color));
        StackArray.Push(state.PrimitiveVertices, new(corner2, color));
        StackArray.Push(state.PrimitiveVertices, new(corner3, color));
        StackArray.Push(state.PrimitiveVertices, new(corner4, color));
    }




    /******************
    
        Rectangle
    
    *******************/



    /// <summary>
    ///     Draws a wireframe rectangle.
    /// </summary>
    /// <param name="state">the debug draw state to store the wireframe shape to draw.</param>
    /// <param name="color">the color used to draw the wireframe.</param>
    /// <param name="cameraPosition">the position of the camera.</param>
    /// <param name="cameraZoom">the zoom level of the camera.</param>
    /// <param name="cameraVerticalFov">the camera vertical fov; how many units/pixels the camera can seen.</param>
    /// <param name="outputResolutionHeight">the height of the output resolution of the monogame application.</param>
    /// <param name="thickness">the thickness - in pixels - in relation to the output resolution.</param>
    /// <param name="scaleThickness">whether or not the line should scale with the camera zoom.</param>

    /// <summary>
    ///     Draws a wireframe rectangle.
    /// </summary>
    /// <param name="state">the debug draw state to store the wireframe shape to draw.</param>
    /// <param name="color">the color used to draw the wireframe.</param>
    /// <param name="rectMin">the minimum vertice of the rectangle.</param>
    /// <param name="rectMax">the maximum vertice of the rectangle.</param>
    /// <param name="cameraPosition">the position of the camera.</param>
    /// <param name="cameraZoom">the zoom level of the camera.</param>
    /// <param name="cameraVerticalFov">the camera vertical fov; how many units/pixels the camera can seen.</param>
    /// <param name="outputResolutionHeight">the height of the output resolution of the monogame application.</param>
    /// <param name="thickness">the thickness - in pixels - in relation to the output resolution.</param>
    /// <param name="scaleThickness">whether or not the line should scale with the camera zoom.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void WireRect(DebugDrawState state, Color color, Vector2 rectMin, Vector2 rectMax, Vector2 cameraPosition, float cameraZoom,
        float cameraVerticalFov, float outputResolutionHeight, float thickness, bool scaleThickness
    )
    {
        // (Note):
        // Dont reverse y-coordinates because draw line already does that.
        Vector2 bottomLeft    = rectMin;
        Vector2 topRight      = rectMax;
        Vector2 topLeft       = new Vector2(rectMin.X, rectMax.Y);
        Vector2 bottomRight   = new Vector2(rectMax.X, rectMin.Y); 

        Line(state, color, topLeft, topRight, cameraPosition, cameraZoom, cameraVerticalFov, outputResolutionHeight, thickness, scaleThickness);
        Line(state, color, topRight, bottomRight, cameraPosition, cameraZoom, cameraVerticalFov, outputResolutionHeight, thickness, scaleThickness);
        Line(state, color, bottomRight, bottomLeft, cameraPosition, cameraZoom, cameraVerticalFov, outputResolutionHeight, thickness, scaleThickness);
        Line(state, color, bottomLeft, topLeft, cameraPosition, cameraZoom, cameraVerticalFov, outputResolutionHeight, thickness, scaleThickness);
    }

    /// <summary>
    ///     Draws a filled rectangle.
    /// </summary>
    /// <param name="state">the debug draw state to store the filled shape to draw.</param>
    /// <param name="color">the colour used to draw the wireframe.</param>
    /// <param name="rectMin">the minimum vertice of the rectangle.</param>
    /// <param name="rectMax">the maximum vertice of the rectangle.</param>
    /// <param name="cameraPosition">the position of the camera.</param>
    public static void FillRect(DebugDrawState state, Color color, Vector2 rectMin, Vector2 rectMax, Vector2 cameraPosition)
    {
        // Note: triangle vertices and indexes are done in
        // a clockwise motion. 

        int totalvVertices = state.PrimitiveVertices.Count;
        StackArray.Push(state.PrimitiveIndices, totalvVertices);
        StackArray.Push(state.PrimitiveIndices, totalvVertices+1);
        StackArray.Push(state.PrimitiveIndices, totalvVertices+2);
        StackArray.Push(state.PrimitiveIndices, totalvVertices);
        StackArray.Push(state.PrimitiveIndices, totalvVertices+2);
        StackArray.Push(state.PrimitiveIndices, totalvVertices+3);

        // translate in relation to the camera.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Vector2 bottomLeft    = rectMin;
        Vector2 topRight      = rectMax;
        Vector2 topLeft       = new Vector2(rectMin.X, rectMax.Y);
        Vector2 bottomRight   = new Vector2(rectMax.X, rectMin.Y); 

        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        topLeft.Y *= -1;
        topRight.Y *= -1;
        bottomLeft.Y *= -1;
        bottomRight.Y *= -1;

        Vector3 tangCamPos = new(cameraPosition.X, -cameraPosition.Y, 0);

        // apply the rectangles world coordinates.

        Vector3 a = -tangCamPos + new Vector3(topLeft, 0);
        Vector3 b = -tangCamPos + new Vector3(topRight, 0);
        Vector3 c = -tangCamPos + new Vector3(bottomRight, 0);
        Vector3 d = -tangCamPos + new Vector3(bottomLeft, 0);
        
        StackArray.Push(state.PrimitiveVertices, new(a, color));
        StackArray.Push(state.PrimitiveVertices, new(b, color));
        StackArray.Push(state.PrimitiveVertices, new(c, color));
        StackArray.Push(state.PrimitiveVertices, new(d, color));        
    }




    /******************
    
        Circle
    
    *******************/




    /// <summary>
    ///     Draws a wireframe circle.
    /// </summary>
    /// <param name="state">the debug draw state to store the wireframe shape to draw.</param>
    /// <param name="color">the colour used to draw the wireframe.</param>
    /// <param name="shape">the shape data.</param>
    /// <param name="cameraPosition">the postion of the camera.</param>
    /// <param name="cameraZoom">the zoom level of the camera.</param>
    /// <param name="cameraVerticalFov">the camera vertical fov; how many units/pixels the camera can seen.</param>
    /// <param name="outputResolutionHeight">the height of the output resolution of the monogame application.</param>
    /// <param name="thickness">the thickness - in pixels - in relation to the output resolution.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="scaleThickness">whether or not the line should scale with the camera zoom.</param>

    /// <summary>
    ///     Draws a wireframe circle.
    /// </summary>
    /// <param name="state">the debug draw state to store the wireframe shape to draw.</param>
    /// <param name="color">the colour used to draw the wireframe.</param>
    /// <param name="circlePos">the position of the circle.</param>
    /// <param name="circleRadius">the radius of the circle.</param>
    /// <param name="cameraPosition">the postion of the camera.</param>
    /// <param name="cameraZoom">the zoom level of the camera.</param>
    /// <param name="cameraVerticalFov">the camera vertical fov; how many units/pixels the camera can seen.</param>
    /// <param name="outputResolutionHeight">the height of the output resolution of the monogame application.</param>
    /// <param name="thickness">the thickness - in pixels - in relation to the output resolution.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="scaleThickness">whether or not the line should scale with the camera zoom.</param>
    public static void WireCircle(DebugDrawState state, Color color, Vector2 circlePos, float circleRadius, Vector2 cameraPosition, 
        float cameraZoom, float cameraVerticalFov, float outputResolutionHeight, float thickness, int verticeCount, bool scaleThickness
    )
    {
        float rotation = MathF.Tau / verticeCount;
        float sin = MathF.Sin(rotation);
        float cos = MathF.Cos(rotation);

        float startX = circlePos.X;
        float startY = circlePos.Y + circleRadius;

        for (int i = 0; i < verticeCount; i++)
        {
            float relX = startX - circlePos.X; // remove the circle position as rotation must be around the origin.
            float relY = startY - circlePos.Y; // remove the circle position as rotation must be around the origin.

            float endX = cos * relX - sin * relY + circlePos.X; // add back the circle position at the end.
            float endY = sin * relX + cos * relY + circlePos.Y; // add back the circle position at the end.

            Line(state, color, new Vector2(startX, startY), new Vector2(endX, endY), cameraPosition, cameraZoom, cameraVerticalFov, 
                outputResolutionHeight, thickness, scaleThickness
            );

            startX = endX;
            startY = endY;
        }
    }

    /// <summary>
    ///     Draw a filled circle.
    /// </summary>
    /// <param name="state">the debug draw state to store the wireframe shape to draw.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="circlePos">the position of the circle.</param>
    /// <param name="circleRadius">the radius of the circle.</param>
    /// <param name="cameraPosition">the position of the camera.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    public static void FillCircle(DebugDrawState state, Color color, Vector2 circlePos, float circleRadius, Vector2 cameraPosition, 
        int verticeCount
    )
    {
        verticeCount = System.Math.Clamp(verticeCount, 3, int.MaxValue);

        // Note: triangle vertices and indexes are done in
        // a clockwise motion. Triangles are made from the 
        // first vertice in the circle. 

        int index = 1;
        int totalVertices = state.PrimitiveVertices.Count;
        int triangleCount = verticeCount - 2; 
        for(int i = 0; i < triangleCount; i++)
        {
            StackArray.Push(state.PrimitiveIndices, totalVertices);
            StackArray.Push(state.PrimitiveIndices, totalVertices+index);
            StackArray.Push(state.PrimitiveIndices, totalVertices+index+1);
            index+=1;
        }

        // add the vertices.

        float rotation = (float)System.Math.Tau / verticeCount;            
        float sin = MathF.Sin(rotation);
        float cos = MathF.Cos(rotation);
        Vector2 start = new(0f, circleRadius);
        Vector2 position = new(circlePos.X, circlePos.Y);
        
        Vector3 tangCamPos = new(cameraPosition.X, -cameraPosition.Y, 0);

        for(int i = 0; i < verticeCount; i++)
        {
            Vector3 vertice = new Vector3(start + position,0);
            
            vertice.Y *= -1;

            vertice = -tangCamPos + vertice;

            start = new(
                cos * start.X  - sin * start.Y,
                sin * start.X  + cos * start.Y
            );

            StackArray.Push(state.PrimitiveVertices, new(vertice, color));
        }
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
    /// <param name="state">the debug draw state to store the wireframe shape to draw.</param>
    /// <param name="color">the colour used to draw the wireframe.</param>
    /// <param name="verticesX">the x-components of the polygon's vertices.</param>
    /// <param name="verticesY">the y-components of the polygon's vertices.</param>
    /// <param name="cameraPosition">the postion of the camera.</param>
    /// <param name="cameraZoom">the zoom level of the camera.</param>
    /// <param name="cameraVerticalFov">the camera vertical fov; how many units/pixels the camera can seen.</param>
    /// <param name="outputResolutionHeight">the height of the output resolution of the monogame application.</param>
    /// <param name="thickness">the thickness - in pixels - in relation to the output resolution.</param>
    /// <param name="scaleThickness">whether or not the line should scale with the camera zoom.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void WirePoly(DebugDrawState state, Color color, Span<float> verticesX, Span<float> verticesY, Vector2 cameraPosition, 
        float cameraZoom, float cameraVerticalFov, float outputResolutionHeight, float thickness, bool scaleThickness
    )
    {
        int nextIndex;
        int count = verticesX.Length;
        for(int startIndex = 0; startIndex < count; startIndex++)
        {
            nextIndex = (startIndex + 1) % count;
            Vector2 start = new Vector2(verticesX[startIndex], verticesY[startIndex]);
            Vector2 end = new Vector2(verticesX[nextIndex], verticesY[nextIndex]);
            Line(state, color, start, end, cameraPosition, cameraZoom, cameraVerticalFov, outputResolutionHeight, thickness, scaleThickness);
        }
    }




    /******************
    
        Disposal.
    
    *******************/



    /// <summary>
    ///     Sets a debug draw states backing array counts to zero.
    /// </summary>
    /// <param name="state">the state instance to clear. </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(DebugDrawState state)
    {
        StackArray.ClearCount(state.PrimitiveIndices);
        StackArray.ClearCount(state.PrimitiveVertices);
    }

    /// <summary>
    ///     Disposes a state instance.
    /// </summary>
    /// <param name="state">the state instance to dispose of.</param>
    public static void Dispose(DebugDrawState state)
    {
        if (state.Disposed)
        {
            return;
        }
        state.Disposed = true;
        state.PrimitiveVertices = null;
        state.PrimitiveIndices = null;
        GC.SuppressFinalize(state);
    }
}