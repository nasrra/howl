using System;
using System.Collections.Generic;
using Howl.Ecs;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math.Shapes;
using Howl.Math;
using static Howl.Ecs.GenIndexListProc;
using static Howl.Math.Shapes.Rectangle;
using static Howl.Math.Math;
using System.Runtime.CompilerServices;

namespace Howl.Debug;

public static class Draw
{
    public const float DefaultWireframeThickness = 2;
    public const int DefaultCirclePointAmount = 16;

    /// <summary>
    /// Gets and sets the primitive vertex data for debug drawing.
    /// </summary>
    private static List<VertexPositionColour> primitiveVertices = new();

    /// <summary>
    /// Gets the primitive vertex data for debug drawing.
    /// </summary>
    public static List<VertexPositionColour> PrimitiveVertices => primitiveVertices;

    /// <summary>
    /// Gets and sets the primitive vertex indice data for debug drawing.
    /// </summary>
    private static List<int> primitiveIndices = new();

    /// <summary>
    /// Gets the primitive vertex indice data for debug drawing.
    /// </summary>
    public static List<int> PrimitiveIndices => primitiveIndices;

    public static void Clear()
    {
        primitiveIndices.Clear();
        primitiveVertices.Clear();
    }




    /******************

        LINE

    *******************/




    /// <summary>
    ///     Draws a line between to points.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="a">The point to start the line segment from.</param>
    /// <param name="b">The point to end the line segment at.</param>
    /// <param name="thickness">The thickness of the line segment.</param>
    /// <param name="color">The color od the line segment.</param>
    /// <param name="scaleThickness">Scale the thickness by the camera zoom.</param>
    public static void Line(EcsState ecs, Vector2 a, Vector2 b, Colour colour, 
        float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        Line(ecs, CameraSystem.MainCameraId, a, b, colour, thickness, scaleThickness);
    }

    /// <summary>
    ///     Draws a line between to points.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="cameraId">The gen id associated with the camera.</param> 
    /// <param name="a">The point to start the line segment from.</param>
    /// <param name="b">The point to end the line segment at.</param>
    /// <param name="thickness">The thickness of the line segment.</param>
    /// <param name="color">The color od the line segment.</param>
    /// <param name="scaleThickness">Scale the thickness by the camera zoom.</param>
    public static void Line(EcsState ecs,GenId cameraId,Vector2 a, Vector2 b, Colour colour, 
        float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
        ref Camera camera = ref ComponentArray.GetData(cameras, ecs, cameraId, ref result);

        if(result == GenIdResult.Ok)
        {
            Line(camera, a, b, colour, thickness, scaleThickness);
        }
    }

    /// <summary>
    ///     Draws a line between to points.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="start">The point to start the line segment from.</param>
    /// <param name="end">The point to end the line segment at.</param>
    /// <param name="thickness">The thickness of the line segment.</param>
    /// <param name="color">The color od the line segment.</param>
    /// <param name="scaleThickness">Scale the thickness by the camera zoom.</param>
    public static void Line(in Camera camera, Vector2 start, Vector2 end, Colour colour, 
        float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        Line(colour, camera.Zoom, camera.Position.X, camera.Position.Y, start.X, start.Y, end.X, end.Y, thickness, scaleThickness);
    }

    public static void Line(Colour colour, float cameraZoom, float cameraPositionX, float cameraPositionY, 
        float startX, float startY, float endX, float endY, float thickness = DefaultWireframeThickness,
        bool scaleThickness = true
    )
    {
        if (scaleThickness)
        {
            thickness /= cameraZoom;
        }

        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        startY  *= -1;
        endY    *= -1;

        System.Math.Clamp(thickness, float.Epsilon, float.MaxValue);
        float halfThickness = thickness * 0.5f;

        // note that we apply the half thickness to the direction so that the line segment
        // corners are offseted by the thickness amount.
        float distanceX = endX - startX;
        float distanceY = endY - startY;
        Normalise(distanceX, distanceY, out float directionX, out float directionY);
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

        short totalVertices = (short)PrimitiveVertices.Count;
        PrimitiveIndices.Add(totalVertices);
        PrimitiveIndices.Add((short)(totalVertices+1));
        PrimitiveIndices.Add((short)(totalVertices+2));
        PrimitiveIndices.Add(totalVertices);
        PrimitiveIndices.Add((short)(totalVertices+2));
        PrimitiveIndices.Add((short)(totalVertices+3));


        // translate in relation to the camera.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Vector3 cameraPosition = new(cameraPositionX, -cameraPositionY, 0);
        Vector3 corner1 = -cameraPosition;
        Vector3 corner2 = -cameraPosition;
        Vector3 corner3 = -cameraPosition;
        Vector3 corner4 = -cameraPosition;

        // apply the line world coordinates.
        corner1 += new Vector3(
            startX + normalX + oppositeDirectionX,
            startY + normalY + oppositeDirectionY,
            0
        );

        corner2 += new Vector3(
            endX + normalX + directionX,
            endY + normalY + directionY,
            0
        );

        corner3 += new Vector3(
            endX + oppositeNormalX + directionX,
            endY + oppositeNormalY + directionY,
            0
        );

        corner4 += new Vector3(
            startX + oppositeNormalX + oppositeDirectionX,
            startY + oppositeNormalY + oppositeDirectionY,
            0
        );

        PrimitiveVertices.Add(new(corner1, colour));
        PrimitiveVertices.Add(new(corner2, colour));
        PrimitiveVertices.Add(new(corner3, colour));
        PrimitiveVertices.Add(new(corner4, colour));
    }




    /******************

        RECTANGLE

    *******************/




    /// <summary>
    ///     Draws a wireframe shape in relation to the main camera.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(EcsState ecs, in Transform transform, in Rectangle rectangle, in Colour colour, 
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(ecs, CameraSystem.MainCameraId, transform, rectangle, colour, thickness);
    }

    /// <summary>
    ///     Draws a wireframe shape.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="cameraId">The gen id associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(EcsState ecs, GenId cameraId, in Transform transform, in Rectangle rectangle,
        in Colour colour, float thickness = DefaultWireframeThickness
    )
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
        ref Camera camera = ref ComponentArray.GetData(cameras, ecs, cameraId, ref result);

        if(result == GenIdResult.Ok)
        {
            Wireframe(camera, transform, rectangle, colour, thickness);
        }
    }

    /// <summary>
    ///     Draws a wireframe shape.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <exception cref="OverflowException">throws when too many primitive vertices are pushed to the monogame app.</exception>
    public static void Wireframe(in Camera camera, in Transform transform, in Rectangle rectangle, in Colour colour, 
        float thickness = DefaultWireframeThickness
    )
    {
        if(PrimitiveVertices.Count > int.MaxValue)
        {
            throw new OverflowException();
        }

        // (Note):
        // Dont reverse y-coordinates because draw line already does that.

        Vector2 topLeft       = TopLeft(rectangle).Transform(transform);
        Vector2 topRight      = TopRight(rectangle).Transform(transform);
        Vector2 bottomLeft    = BottomLeft(rectangle).Transform(transform);
        Vector2 bottomRight   = BottomRight(rectangle).Transform(transform); 

        Line(camera, topLeft, topRight, colour, thickness);
        Line(camera, topRight, bottomRight, colour, thickness);
        Line(camera, bottomRight, bottomLeft, colour, thickness);
        Line(camera, bottomLeft, topLeft, colour, thickness);
    }

    /// <summary>
    ///     Draws a filled shape in relation to the main camera.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    public static void Filled(EcsState ecs, in Transform transform, in Rectangle rectangle,in Colour colour)
    {
        Filled(ecs, CameraSystem.MainCameraId, transform, rectangle, colour);
    }

    /// <summary>
    ///     Draws a filled shape.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="cameraId">The gen id associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    public static void Filled(EcsState ecs, GenId cameraId, in Transform transform, in Rectangle rectangle, in Colour colour)
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
        ref Camera camera = ref ComponentArray.GetData(cameras, ecs, cameraId, ref result);

        if(result == GenIdResult.Ok)
        {
            Filled(camera, transform, rectangle, colour);
        }
    }

    /// <summary>
    ///     Draws a filled shape to the currently bound render target.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    public static void Filled(in Camera camera, in Transform transform, in Rectangle rectangle, in Colour colour)
    {
        // Note: triangle vertices and indexes are done in
        // a clockwise motion. 

        int totalvVertices = PrimitiveVertices.Count;
        PrimitiveIndices.Add(totalvVertices);
        PrimitiveIndices.Add(totalvVertices+1);
        PrimitiveIndices.Add(totalvVertices+2);
        PrimitiveIndices.Add(totalvVertices);
        PrimitiveIndices.Add(totalvVertices+2);
        PrimitiveIndices.Add(totalvVertices+3);

        // translate in relation to the camera.
        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        Vector3 topLeft       = new(TopLeft(rectangle).Transform(transform),0);
        Vector3 topRight      = new(TopRight(rectangle).Transform(transform),0);
        Vector3 bottomLeft    = new(BottomLeft(rectangle).Transform(transform),0);
        Vector3 bottomRight   = new(BottomRight(rectangle).Transform(transform),0);

        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        topLeft.Y *= -1;
        topRight.Y *= -1;
        bottomLeft.Y *= -1;
        bottomRight.Y *= -1;

        Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);

        // apply the rectangles world coordinates.

        Vector3 a = -cameraPosition + topLeft;
        Vector3 b = -cameraPosition + topRight;
        Vector3 c = -cameraPosition + bottomRight;
        Vector3 d = -cameraPosition + bottomLeft;
        
        PrimitiveVertices.Add(new(a, colour));
        PrimitiveVertices.Add(new(b, colour));
        PrimitiveVertices.Add(new(c, colour));
        PrimitiveVertices.Add(new(d, colour));        
    }




    /******************

        CIRCLE

    ********************/




    /// <summary>
    ///     Draws a filled shape in relation to the main camera.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    public static void Filled(EcsState ecs, in Transform transform, in Circle circle, in Colour colour, int verticeCount = DefaultCirclePointAmount)
    {
        Filled(ecs, CameraSystem.MainCameraId, transform, circle, colour, verticeCount);
    }

    /// <summary>
    ///     Draws a filled shape.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="cameraId">The gen id associated with the camera to use for transforming coordinates..</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    public static void Filled(EcsState ecs, GenId cameraId, in Transform transform, in Circle circle, in Colour colour, 
        int verticeCount = DefaultCirclePointAmount
    )
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
        ref Camera camera = ref ComponentArray.GetData(cameras, ecs, cameraId, ref result);

        if(result == GenIdResult.Ok)
        {
            Filled(camera, transform, circle, colour, verticeCount);
        }
    }

    /// <summary>
    ///     Draws a filled shape.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void Filled(in Camera camera, in Transform transform, in Circle circle, in Colour colour, int verticeCount = DefaultCirclePointAmount)
    {
        if(verticeCount == System.Math.Clamp(verticeCount, 3, int.MaxValue))
        {
            // Note: triangle vertices and indexes are done in
            // a clockwise motion. Triangles are made from the 
            // first vertice in the circle. 

            int index = 1;
            int totalVertices = PrimitiveVertices.Count;
            int triangleCount = verticeCount - 2; 
            for(int i = 0; i < triangleCount; i++)
            {
                PrimitiveIndices.Add(totalVertices);
                PrimitiveIndices.Add(totalVertices+index);
                PrimitiveIndices.Add(totalVertices+index+1);
                index+=1;
            }

            // add the vertices.

            float rotation = (float)System.Math.Tau / verticeCount;            
            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);
            Vector2 start = new(0f, circle.Radius);
            Vector2 position = new(circle.X, circle.Y);
            Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);

            for(int i = 0; i < verticeCount; i++)
            {
                Vector3 vertice = new Vector3((start + position).Transform(transform),0);
                
                vertice.Y *= -1;

                vertice = -cameraPosition + vertice;

                start = new(
                    cos * start.X  - sin * start.Y,
                    sin * start.X  + cos * start.Y
                );

                PrimitiveVertices.Add(new(vertice,colour));
            }

        }
        else
        {
            throw new ArgumentException($"Renderer can only draw a solid circle with 3 or int.MaxValue 'verticeCount', not {verticeCount} amount of vertices.");               
        }        
    }

    /// <summary>
    ///     Draws a wireframe shape in relation to the main camera.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(EcsState ecs, in Transform transform, in Circle circle, in Colour colour,
        int verticeCount = DefaultCirclePointAmount, float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(ecs, CameraSystem.MainCameraId, transform, circle, colour, verticeCount, thickness);
    }

    /// <summary>
    ///     Draws a wireframe shape.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="cameraId">The gen-id associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(EcsState ecs, GenId cameraId, in Transform transform, in Circle circle,
        in Colour colour, int verticeCount = DefaultCirclePointAmount, float thickness = DefaultWireframeThickness
    )
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
        ref Camera camera = ref ComponentArray.GetData(cameras, ecs, cameraId, ref result);

        if(result == GenIdResult.Ok)
        {
            Wireframe(camera, transform, circle, colour, verticeCount, thickness);
        }
    }

    /// <summary>
    ///     Draws a wireframe shape.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void Wireframe(in Camera camera, in Transform transform, in Circle circle, in Colour colour,
        int verticeCount = DefaultCirclePointAmount, float thickness = DefaultWireframeThickness
    )   
    {
        if(verticeCount == System.Math.Clamp(verticeCount, 3, int.MaxValue))
        {
            float rotation = (float)System.Math.Tau / verticeCount;            
            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);
            Vector2 start = new(0f, circle.Radius);
            Vector2 position = new(circle.X, circle.Y);

            for(int i = 0; i < verticeCount; i++)
            {
                Vector2 end = new(
                    cos * start.X  - sin * start.Y,
                    sin * start.X  + cos * start.Y
                );

                Line(
                    camera,
                    (start + position).Transform(transform), 
                    (end + position).Transform(transform), 
                    colour, 
                    thickness
                );

                start = end;
            }
        }
        else
        {
            throw new ArgumentException($"Renderer can only draw a wireframe circle with 3 or int.MaxValue 'verticeCount', not {verticeCount} amount of vertices.");   
        }
    }

    /// <summary>
    /// Draws a wireframe of a circle.
    /// </summary>
    /// <param name="camera">the camera to draw the wireframe in relation to.</param>
    /// <param name="circle">the circle data to draw the wireframe.</param>
    /// <param name="colour">the colour of the wireframe.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness by the camera zoom.</param>
    public static void WireframeCircle(Camera camera, Circle circle, Colour colour,         
        int verticeCount = DefaultCirclePointAmount, float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        WireframeCircle(colour, camera.Position.X, camera.Position.Y, camera.Zoom,
            circle.X, circle.Y, circle.Radius, thickness, verticeCount, scaleThickness
        );
    }

    /// <summary>
    /// Draw a wireframe of a circle.
    /// </summary>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="cameraPositionX">the x-component of the camera's positional vector.</param>
    /// <param name="cameraPositionY">the y-component of the camera's positional vector.</param>
    /// <param name="cameraZoom">the zoom level of the camera.</param>
    /// <param name="circleX">the x-component of the circle's positional vector.</param>
    /// <param name="circleY">the y-component of the circle's positional vector.</param>
    /// <param name="circleRadius">the radius of the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <param name="vertexCount">the amount of vertices used to draw the circle.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness by the camera zoom.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void WireframeCircle(Colour colour, float cameraPositionX, float cameraPositionY, float cameraZoom,
        float circleX, float circleY, float circleRadius, float thickness = DefaultWireframeThickness, 
        int vertexCount = DefaultCirclePointAmount, bool scaleThickness = true
    )
    {
        if (vertexCount < 3)
            throw new ArgumentException($"Renderer can only draw a wireframe circle with >=3 vertices, not {vertexCount}.");

        float rotation = MathF.Tau / vertexCount;
        float sin = MathF.Sin(rotation);
        float cos = MathF.Cos(rotation);

        float startX = circleX;
        float startY = circleY + circleRadius;

        for (int i = 0; i < vertexCount; i++)
        {
            float relX = startX - circleX; // remove the circle position as rotation must be around the origin.
            float relY = startY - circleY; // remove the circle position as rotation must be around the origin.

            float endX = cos * relX - sin * relY + circleX; // add back the circle position at the end.
            float endY = sin * relX + cos * relY + circleY; // add back the circle position at the end.

            Line(colour, cameraZoom, cameraPositionX, cameraPositionY,
                startX, startY, endX, endY, thickness, scaleThickness);

            startX = endX;
            startY = endY;
        }
    }


    /*******************

        Polygon.

    ********************/




    /// <summary>
    /// Draws a wireframe polygon.
    /// </summary>
    /// <param name="colour">the colour of the wireframe.</param>
    /// <param name="camera">the camera to draw in relation to.</param>
    /// <param name="verticesX">the x-components of the polygon's vertices.</param>
    /// <param name="verticesY">the y-components of the polygon's vertices.</param>
    /// <param name="verticesCount">the count of vertices.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness by the camera's zoom level.</param>
    public static void WireframePolygon(Colour colour, Camera camera, Span<float> verticesX, Span<float> verticesY, 
        int verticesCount, float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        WireframePolygon(colour, camera.Position.X, camera.Position.Y, camera.Zoom, verticesX, verticesY, verticesCount, thickness, scaleThickness);    
    }

    /// <summary>
    /// Draws a wireframe polygon.
    /// </summary>
    /// <param name="colour">the colour of the wireframe.</param>
    /// <param name="cameraPositionX">the x-component of the camera's position vector.</param>
    /// <param name="cameraPositionY">the y-component of the camera's position vector.</param>
    /// <param name="cameraZoom">the zoom level of the camera.</param>
    /// <param name="verticesX">the x-components of the polygon's vertices.</param>
    /// <param name="verticesY">the y-components of the polygon's vertices.</param>
    /// <param name="verticesCount">the count of vertices.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <param name="scaleThickness">whether or not to scale the thickness by the camera's zoom level.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void WireframePolygon(Colour colour, float cameraPositionX, float cameraPositionY, float cameraZoom,
        Span<float> verticesX, Span<float> verticesY, int verticesCount, float thickness = DefaultWireframeThickness, bool scaleThickness = true
    )
    {
        int nextIndex;
        for(int startIndex = 0; startIndex < verticesCount; startIndex++)
        {
            nextIndex = (startIndex + 1) % verticesCount;
            Line(colour, cameraZoom, cameraPositionX, cameraPositionY, verticesX[startIndex], verticesY[startIndex], 
                verticesX[nextIndex], verticesY[nextIndex], thickness, scaleThickness
            );
        }
    }




    /******************

        POLYGON 16
    
    *******************/




    /// <summary>
    /// Draws a wireframe shape in relation to the main camera.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(EcsState ecs, in Transform transform, in Polygon16 polygon, in Colour colour,
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(ecs, CameraSystem.MainCameraId, transform, polygon, colour, thickness);
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="cameraId">The gen-id associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(EcsState ecs, GenId cameraId, in Transform transform, in Polygon16 polygon, in Colour colour,
        float thickness = DefaultWireframeThickness
    )
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
        ref Camera camera = ref ComponentArray.GetData(cameras, ecs, cameraId, ref result);
        if(result == GenIdResult.Ok)
        {
            Wireframe(camera, transform, polygon, colour, thickness);
        }
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(in Camera camera, in Transform transform, in Polygon16 polygon, in Colour colour, 
        float thickness = DefaultWireframeThickness
    )
    {
        Vector2 start = polygon.GetVertex(0).Transform(transform); 
        for(int i = 1; i <= polygon.VerticesCount; i++)
        {
            int index = i % polygon.VerticesCount;
            Vector2 end = polygon.GetVertex(index).Transform(transform); 
            Line(camera, start, end, colour, thickness);
            start = end;
        }
    }




    /******************
    
        POLYGON 4

    *******************/




    /// <summary>
    /// Draws a wireframe shape in relation to the main camera.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(EcsState ecs, in Transform transform, in Polygon4 polygon, in Colour colour,
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(ecs, CameraSystem.MainCameraId, transform, polygon, colour, thickness);
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="ecs"></param>
    /// <param name="cameraId">The gen-id associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(EcsState ecs, GenId cameraId, in Transform transform, in Polygon4 polygon,
        in Colour colour, float thickness = DefaultWireframeThickness
    )
    {
        GenIdResult result = default;
        ComponentArray<Camera> cameras = EcsState.GetComponents<Camera>(ecs);
        ref Camera camera = ref ComponentArray.GetData(cameras, ecs, cameraId, ref result);
        
        if(result == GenIdResult.Ok)
        {
            Wireframe(camera, transform, polygon, colour, thickness);
        }
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(in Camera camera, in Transform transform, in Polygon4 polygon, in Colour colour, 
        float thickness = DefaultWireframeThickness
    )
    {
        Vector2 start = polygon.GetVertex(0).Transform(transform); 
        for(int i = 1; i <= polygon.VerticesCount; i++)
        {
            int index = i % polygon.VerticesCount;
            Vector2 end = polygon.GetVertex(index).Transform(transform); 
            Line(camera, start, end, colour, thickness);
            start = end;
        }
    }
}