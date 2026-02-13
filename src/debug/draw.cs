using System;
using System.Collections.Generic;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math.Shapes;
using Howl.Math;
using static Howl.ECS.GenIndexListProc;

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
    /// Draws a line between to points.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="a">The point to start the line segment from.</param>
    /// <param name="b">The point to end the line segment at.</param>
    /// <param name="thickness">The thickness of the line segment.</param>
    /// <param name="color">The color od the line segment.</param>
    /// <param name="scaleThickness">Scale the thickness by the camera zoom.</param>
    public static void Line(
        ComponentRegistry componentRegistry,
        Vector2 a, 
        Vector2 b, 
        Colour colour, 
        float thickness = DefaultWireframeThickness, 
        bool scaleThickness = true
    )
    {
        Line(componentRegistry, CameraSystem.MainCameraId, a, b, colour, thickness, scaleThickness);
    }

    /// <summary>
    /// Draws a line between to points.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera.</param> 
    /// <param name="a">The point to start the line segment from.</param>
    /// <param name="b">The point to end the line segment at.</param>
    /// <param name="thickness">The thickness of the line segment.</param>
    /// <param name="color">The color od the line segment.</param>
    /// <param name="scaleThickness">Scale the thickness by the camera zoom.</param>
    public static void Line(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        Vector2 a, 
        Vector2 b, 
        Colour colour, 
        float thickness = DefaultWireframeThickness, 
        bool scaleThickness = true
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(GetDenseRef(cameraComponents, cameraId, out Ref<Camera> camera).Ok())
        {
            Line(camera, a, b, colour, thickness, scaleThickness);
        }
    }

    /// <summary>
    /// Draws a line between to points.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="a">The point to start the line segment from.</param>
    /// <param name="b">The point to end the line segment at.</param>
    /// <param name="thickness">The thickness of the line segment.</param>
    /// <param name="color">The color od the line segment.</param>
    /// <param name="scaleThickness">Scale the thickness by the camera zoom.</param>
    public static void Line(
        in Camera camera, 
        Vector2 a, 
        Vector2 b, 
        Colour colour, 
        float thickness = DefaultWireframeThickness, 
        bool scaleThickness = true
    )
    {
        if (scaleThickness)
        {
            thickness /= camera.Zoom;
        }

        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        a.Y *= -1;
        b.Y *= -1;

        System.Math.Clamp(thickness, float.Epsilon, float.MaxValue);
        float halfThickness = thickness * 0.5f;

        // note that we apply the half thickness to the direction so that the line segment
        // corners are offseted by the thickness amount.
        Vector2 direction = (b - a).Normalise() * halfThickness;
        Vector2 oppositeDirection = -direction;   
        
        Vector2 normal = new(-direction.Y, direction.X);
        Vector2 oppositeNormal = -normal;

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
        Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);
        Vector3 corner1 = -cameraPosition;
        Vector3 corner2 = -cameraPosition;
        Vector3 corner3 = -cameraPosition;
        Vector3 corner4 = -cameraPosition;

        // apply the line world coordinates.
        corner1 += new Vector3(a + normal + oppositeDirection, 0); 
        corner2 += new Vector3(b + normal + direction, 0); 
        corner3 += new Vector3(b + oppositeNormal + direction, 0);
        corner4 += new Vector3(a + oppositeNormal + oppositeDirection, 0); 

        PrimitiveVertices.Add(new(corner1, colour));
        PrimitiveVertices.Add(new(corner2, colour));
        PrimitiveVertices.Add(new(corner3, colour));
        PrimitiveVertices.Add(new(corner4, colour));
    }




    /******************

        RECTANGLE

    *******************/




    /// <summary>
    /// Draws a wireframe shape in relation to the main camera.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        in Transform transform,
        in Rectangle rectangle,
        in Colour colour, 
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(componentRegistry, CameraSystem.MainCameraId, transform, rectangle, colour, thickness);
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Transform transform,
        in Rectangle rectangle,
        in Colour colour,
        float thickness = DefaultWireframeThickness
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(GetDenseRef(cameraComponents, cameraId, out Ref<Camera> camera).Ok())
        {
            Wireframe(camera, transform, rectangle, colour, thickness);
        }
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <exception cref="OverflowException">throws when too many primitive vertices are pushed to the monogame app.</exception>
    public static void Wireframe(
        in Camera camera,
        in Transform transform, 
        in Rectangle rectangle,
        in Colour colour, 
        float thickness = DefaultWireframeThickness
    )
    {
        if(PrimitiveVertices.Count > int.MaxValue)
        {
            throw new OverflowException();
        }

        // (Note):
        // Dont reverse y-coordinates because draw line already does that.

        Vector2 topLeft       = rectangle.TopLeft.Transform(transform);
        Vector2 topRight      = rectangle.TopRight.Transform(transform);
        Vector2 bottomLeft    = rectangle.BottomLeft.Transform(transform);
        Vector2 bottomRight   = rectangle.BottomRight.Transform(transform); 

        Line(camera, topLeft, topRight, colour, thickness);
        Line(camera, topRight, bottomRight, colour, thickness);
        Line(camera, bottomRight, bottomLeft, colour, thickness);
        Line(camera, bottomLeft, topLeft, colour, thickness);
    }

    /// <summary>
    /// Draws a filled shape in relation to the main camera.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    public static void Filled(
        ComponentRegistry componentRegistry,
        in Transform transform,
        in Rectangle rectangle,
        in Colour colour
    )
    {
        Filled(componentRegistry, CameraSystem.MainCameraId, transform, rectangle, colour);
    }

    /// <summary>
    /// Draws a filled shape.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    public static void Filled(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Transform transform,
        in Rectangle rectangle,
        in Colour colour
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(GetDenseRef(cameraComponents, cameraId, out Ref<Camera> camera).Ok())
        {
            Filled(camera, transform, rectangle, colour);
        }
    }

    /// <summary>
    /// Draws a filled shape to the currently bound render target.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    public static void Filled(
        in Camera camera,
        in Transform transform, 
        in Rectangle rectangle,
        in Colour colour
    )
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
        Vector3 topLeft       = new(rectangle.TopLeft.Transform(transform),0);
        Vector3 topRight      = new(rectangle.TopRight.Transform(transform),0);
        Vector3 bottomLeft    = new(rectangle.BottomLeft.Transform(transform),0);
        Vector3 bottomRight   = new(rectangle.BottomRight.Transform(transform),0);

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
    /// Draws a filled shape in relation to the main camera.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    public static void Filled(
        ComponentRegistry componentRegistry,
        in Transform transform,
        in Circle circle,
        in Colour colour,
        int verticeCount = DefaultCirclePointAmount
    )
    {
        Filled(componentRegistry, CameraSystem.MainCameraId, transform, circle, colour, verticeCount);
    }

    /// <summary>
    /// Draws a filled shape.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera to use for transforming coordinates..</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    public static void Filled(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Transform transform,
        in Circle circle,
        in Colour colour,
        int verticeCount = DefaultCirclePointAmount
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(GetDenseRef(cameraComponents, cameraId, out Ref<Camera> camera).Ok())
        {
            Filled(camera, transform, circle, colour, verticeCount);
        }
    }

    /// <summary>
    /// Draws a filled shape.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the filled area.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void Filled(
        in Camera camera,
        in Transform transform, 
        in Circle circle,
        in Colour colour, 
        int verticeCount = DefaultCirclePointAmount
    )
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
    /// Draws a wireframe shape in relation to the main camera.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        in Transform transform,
        in Circle circle,
        in Colour colour,
        int verticeCount = DefaultCirclePointAmount,
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(componentRegistry, CameraSystem.MainCameraId, transform, circle, colour, verticeCount, thickness);
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Transform transform,
        in Circle circle,
        in Colour colour,
        int verticeCount = DefaultCirclePointAmount,
        float thickness = DefaultWireframeThickness
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(GetDenseRef(cameraComponents, cameraId, out Ref<Camera> camera).Ok())
        {
            Wireframe(camera, transform, circle, colour, verticeCount, thickness);
        }
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void Wireframe(
        in Camera camera,
        in Transform transform, 
        in Circle circle,
        in Colour colour,
        int verticeCount = DefaultCirclePointAmount,
        float thickness = DefaultWireframeThickness
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




    /******************

        POLYGON 16
    
    *******************/




    /// <summary>
    /// Draws a wireframe shape in relation to the main camera.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        in Transform transform,
        in Polygon16 polygon,
        in Colour colour,
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(componentRegistry, CameraSystem.MainCameraId, transform, polygon, colour, thickness);
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Transform transform,
        in Polygon16 polygon,
        in Colour colour,
        float thickness = DefaultWireframeThickness
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(GetDenseRef(cameraComponents, cameraId, out Ref<Camera> camera).Ok())
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
    public static void Wireframe(
        in Camera camera,
        in Transform transform, 
        in Polygon16 polygon,
        in Colour colour, 
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
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        in Transform transform,
        in Polygon4 polygon,
        in Colour colour,
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(componentRegistry, CameraSystem.MainCameraId, transform, polygon, colour, thickness);
    }

    /// <summary>
    /// Draws a wireframe shape.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera to use for transforming coordinates.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="polygon">the polygon data.</param>
    /// <param name="colour">the colour used to draw the wireframe.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Transform transform,
        in Polygon4 polygon,
        in Colour colour,
        float thickness = DefaultWireframeThickness
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(GetDenseRef(cameraComponents, cameraId, out Ref<Camera> camera).Ok())
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
    public static void Wireframe(
        in Camera camera,
        in Transform transform, 
        in Polygon4 polygon,
        in Colour colour, 
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