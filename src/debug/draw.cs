using System;
using System.Collections.Generic;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;

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
        Howl.Math.Vector2 a, 
        Howl.Math.Vector2 b, 
        Howl.Graphics.Colour colour, 
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
        Howl.Math.Vector2 a, 
        Howl.Math.Vector2 b, 
        Howl.Graphics.Colour colour, 
        float thickness = DefaultWireframeThickness, 
        bool scaleThickness = true
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(cameraComponents.GetDenseRef(cameraId, out Ref<Camera> camera).Ok())
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
        Howl.Math.Vector2 a, 
        Howl.Math.Vector2 b, 
        Howl.Graphics.Colour colour, 
        float thickness = DefaultWireframeThickness, 
        bool scaleThickness = true
    )
    {
        thickness /= camera.Zoom;

        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        a.Y *= -1;
        b.Y *= -1;

        System.Math.Clamp(thickness, float.Epsilon, float.MaxValue);
        float halfThickness = thickness * 0.5f;

        // note that we apply the half thickness to the direction so that the line segment
        // corners are offseted by the thickness amount.
        Howl.Math.Vector2 direction = (b - a).Normalise() * halfThickness;
        Howl.Math.Vector2 oppositeDirection = -direction;   
        
        Howl.Math.Vector2 normal = new(-direction.Y, direction.X);
        Howl.Math.Vector2 oppositeNormal = -normal;

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
        Howl.Math.Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);
        Howl.Math.Vector3 corner1 = -cameraPosition;
        Howl.Math.Vector3 corner2 = -cameraPosition;
        Howl.Math.Vector3 corner3 = -cameraPosition;
        Howl.Math.Vector3 corner4 = -cameraPosition;

        // apply the line world coordinates.
        corner1 += new Howl.Math.Vector3(a + normal + oppositeDirection, 0); 
        corner2 += new Howl.Math.Vector3(b + normal + direction, 0); 
        corner3 += new Howl.Math.Vector3(b + oppositeNormal + direction, 0);
        corner4 += new Howl.Math.Vector3(a + oppositeNormal + oppositeDirection, 0); 

        PrimitiveVertices.Add(new(corner1, colour));
        PrimitiveVertices.Add(new(corner2, colour));
        PrimitiveVertices.Add(new(corner3, colour));
        PrimitiveVertices.Add(new(corner4, colour));
    }




    /******************

        RECTANGLE

    *******************/




    /// <summary>
    /// Draws a wireframe shape to the main camera render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        in Math.Transform transform,
        in Howl.Graphics.RectangleShape rectangle, 
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(componentRegistry, CameraSystem.MainCameraId, transform, rectangle, thickness);
    }

    /// <summary>
    /// Draws a wireframe shape to the currently bound render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Math.Transform transform,
        in Howl.Graphics.RectangleShape rectangle, 
        float thickness = DefaultWireframeThickness
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(cameraComponents.GetDenseRef(cameraId, out Ref<Camera> camera).Ok())
        {
            Wireframe(camera, transform, rectangle, thickness);
        }
    }

    /// <summary>
    /// Draws a wireframe shape to the currently bound render target.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <exception cref="OverflowException">throws when too many primitive vertices are pushed to the monogame app.</exception>
    public static void Wireframe(
        in Camera camera,
        in Howl.Math.Transform transform, 
        in Howl.Graphics.RectangleShape rectangle, 
        float thickness = DefaultWireframeThickness
    )
    {
        if(PrimitiveVertices.Count > int.MaxValue)
        {
            throw new OverflowException();
        }

        // (Note):
        // Dont reverse y-coordinates because draw line already does that.

        Howl.Math.Vector2 topLeft       = rectangle.Shape.TopLeft.Transform(transform);
        Howl.Math.Vector2 topRight      = rectangle.Shape.TopRight.Transform(transform);
        Howl.Math.Vector2 bottomLeft    = rectangle.Shape.BottomLeft.Transform(transform);
        Howl.Math.Vector2 bottomRight   = rectangle.Shape.BottomRight.Transform(transform); 

        Line(camera, topLeft, topRight, rectangle.Colour, thickness);
        Line(camera, topRight, bottomRight, rectangle.Colour, thickness);
        Line(camera, bottomRight, bottomLeft, rectangle.Colour, thickness);
        Line(camera, bottomLeft, topLeft, rectangle.Colour, thickness);
    }
    
    /// <summary>
    /// Draws a filled shape to the main camera render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    public static void Filled(
        ComponentRegistry componentRegistry,
        in Math.Transform transform,
        in Howl.Graphics.RectangleShape rectangle
    )
    {
        Filled(componentRegistry, CameraSystem.MainCameraId, transform, rectangle);
    }

    /// <summary>
    /// Draws a filled shape to the currently bound render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    public static void Filled(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Math.Transform transform,
        in Howl.Graphics.RectangleShape rectangle
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(cameraComponents.GetDenseRef(cameraId, out Ref<Camera> camera).Ok())
        {
            Filled(camera, transform, rectangle);
        }
    }

    /// <summary>
    /// Draws a filled shape to the currently bound render target.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="rectangle">the rectangle data.</param>
    public static void Filled(
        in Camera camera,
        in Howl.Math.Transform transform, 
        in Howl.Graphics.RectangleShape rectangle)
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
        Howl.Math.Vector3 topLeft       = new(rectangle.Shape.TopLeft.Transform(transform),0);
        Howl.Math.Vector3 topRight      = new(rectangle.Shape.TopRight.Transform(transform),0);
        Howl.Math.Vector3 bottomLeft    = new(rectangle.Shape.BottomLeft.Transform(transform),0);
        Howl.Math.Vector3 bottomRight   = new(rectangle.Shape.BottomRight.Transform(transform),0);

        // (Note):
        // reverse y-coordinates because monogame
        // sprite batch is y+ = down, Howl is y+ = up.
        topLeft.Y *= -1;
        topRight.Y *= -1;
        bottomLeft.Y *= -1;
        bottomRight.Y *= -1;

        Howl.Math.Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);

        // apply the rectangles world coordinates.

        Howl.Math.Vector3 a = -cameraPosition + topLeft;
        Howl.Math.Vector3 b = -cameraPosition + topRight;
        Howl.Math.Vector3 c = -cameraPosition + bottomRight;
        Howl.Math.Vector3 d = -cameraPosition + bottomLeft;
        
        PrimitiveVertices.Add(new(a, rectangle.Colour));
        PrimitiveVertices.Add(new(b, rectangle.Colour));
        PrimitiveVertices.Add(new(c, rectangle.Colour));
        PrimitiveVertices.Add(new(d, rectangle.Colour));        
    }




    /******************

        CIRCLE

    ********************/




    /// <summary>
    /// Draws a filled shape to the main camera render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    public static void Filled(
        ComponentRegistry componentRegistry,
        in Math.Transform transform,
        in Howl.Graphics.CircleShape circle 
    )
    {
        Wireframe(componentRegistry, CameraSystem.MainCameraId, transform, circle);
    }

    /// <summary>
    /// Draws a filled shape to the currently bound render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    public static void Filled(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Math.Transform transform,
        in Howl.Graphics.CircleShape circle 
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(cameraComponents.GetDenseRef(cameraId, out Ref<Camera> camera).Ok())
        {
            Wireframe(camera, transform, circle);
        }
    }

    /// <summary>
    /// Draws a filled shape to the currently bound render target.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void Filled(
        in Camera camera,
        in Howl.Math.Transform transform, 
        in CircleShape circle, 
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
            Howl.Math.Vector2 start = new(0f, circle.Shape.Radius);
            Howl.Math.Vector2 position = new(circle.Shape.X, circle.Shape.Y);
            Howl.Math.Vector3 cameraPosition = new(camera.Position.X, -camera.Position.Y, 0);

            for(int i = 0; i < verticeCount; i++)
            {
                Howl.Math.Vector3 vertice = new Howl.Math.Vector3((start + position).Transform(transform),0);
                
                vertice.Y *= -1;

                vertice = -cameraPosition + vertice;

                start = new(
                    cos * start.X  - sin * start.Y,
                    sin * start.X  + cos * start.Y
                );

                PrimitiveVertices.Add(new(vertice,circle.Colour));
            }

        }
        else
        {
            throw new ArgumentException($"Renderer can only draw a solid circle with 3 or int.MaxValue 'verticeCount', not {verticeCount} amount of vertices.");               
        }        
    }

    /// <summary>
    /// Draws a wireframe shape to the main camera render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        in Math.Transform transform,
        in Howl.Graphics.CircleShape circle,
        int verticeCount = DefaultCirclePointAmount,
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(componentRegistry, CameraSystem.MainCameraId, transform, circle, verticeCount, thickness);
    }

    /// <summary>
    /// Draws a wireframe shape to the currently bound render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Math.Transform transform,
        in Howl.Graphics.CircleShape circle,
        int verticeCount = DefaultCirclePointAmount,
        float thickness = DefaultWireframeThickness
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(cameraComponents.GetDenseRef(cameraId, out Ref<Camera> camera).Ok())
        {
            Wireframe(camera, transform, circle, verticeCount, thickness);
        }
    }

    /// <summary>
    /// Draws a wireframe shape to the currently bound render target.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="circle">the circle data.</param>
    /// <param name="verticeCount">the amount of vertices used to draw the circle.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void Wireframe(
        in Camera camera,
        in Howl.Math.Transform transform, 
        in CircleShape circle,  
        int verticeCount = DefaultCirclePointAmount,
        float thickness = DefaultWireframeThickness
    )   
    {
        if(verticeCount == System.Math.Clamp(verticeCount, 3, int.MaxValue))
        {
            float rotation = (float)System.Math.Tau / verticeCount;            
            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);
            Howl.Math.Vector2 start = new(0f, circle.Shape.Radius);
            Howl.Math.Vector2 position = new(circle.Shape.X, circle.Shape.Y);

            for(int i = 0; i < verticeCount; i++)
            {
                Howl.Math.Vector2 end = new(
                    cos * start.X  - sin * start.Y,
                    sin * start.X  + cos * start.Y
                );

                Line(
                    camera,
                    (start + position).Transform(transform), 
                    (end + position).Transform(transform), 
                    circle.Colour, 
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
    /// Draws a wireframe shape to the main camera render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="shape">the polygon data.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        in Math.Transform transform,
        in Polygon16Shape polygon,
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(componentRegistry, CameraSystem.MainCameraId, transform, polygon, thickness);
    }

    /// <summary>
    /// Draws a wireframe shape to the currently bound render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="shape">the polygon data.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Math.Transform transform,
        in Polygon16Shape polygon,
        float thickness = DefaultWireframeThickness
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(cameraComponents.GetDenseRef(cameraId, out Ref<Camera> camera).Ok())
        {
            Wireframe(camera, transform, polygon, thickness);
        }
    }

    /// <summary>
    /// Draws a wireframe shape to the currently bound render target.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="shape">the polygon data.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        in Camera camera,
        in Howl.Math.Transform transform, 
        in Polygon16Shape shape, 
        float thickness = DefaultWireframeThickness
    )
    {
        Howl.Math.Vector2 start = shape.GetVertex(0).Transform(transform); 
        for(int i = 1; i <= shape.Polygon.VerticesCount; i++)
        {
            int index = i % shape.Polygon.VerticesCount;
            Howl.Math.Vector2 end = shape.GetVertex(index).Transform(transform); 
            Line(camera, start, end, shape.Colour, thickness);
            start = end;
        }
    }




    /******************
    
        POLYGON 4

    *******************/




    /// <summary>
    /// Draws a wireframe shape to the main camera render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="shape">the polygon data.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        in Math.Transform transform,
        in Polygon4Shape polygon,
        float thickness = DefaultWireframeThickness
    )
    {
        Wireframe(componentRegistry, CameraSystem.MainCameraId, transform, polygon, thickness);
    }


    /// <summary>
    /// Draws a wireframe shape to the currently bound render target.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="cameraId">The gen index associated with the camera.</param> 
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="shape">the polygon data.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        ComponentRegistry componentRegistry,
        GenIndex cameraId,
        in Math.Transform transform,
        in Polygon4Shape polygon,
        float thickness = DefaultWireframeThickness
    )
    {
        GenIndexList<Camera> cameraComponents = componentRegistry.Get<Camera>(); 
        if(cameraComponents.GetDenseRef(cameraId, out Ref<Camera> camera).Ok())
        {
            Wireframe(camera, transform, polygon, thickness);
        }
    }

    /// <summary>
    /// Draws a wireframe shape to the currently bound render target.
    /// </summary>
    /// <param name="camera">The camera to use for transforming coordinates.</param>
    /// <param name="transform">the transformation to apply to the shape.</param>
    /// <param name="shape">the polygon data.</param>
    /// <param name="thickness">the thickness of the wireframe.</param>
    public static void Wireframe(
        in Camera camera,
        in Howl.Math.Transform transform, 
        in Polygon4Shape shape, 
        float thickness = DefaultWireframeThickness
    )
    {
        Howl.Math.Vector2 start = shape.GetVertex(0).Transform(transform); 
        for(int i = 1; i <= shape.Polygon.VerticesCount; i++)
        {
            int index = i % shape.Polygon.VerticesCount;
            Howl.Math.Vector2 end = shape.GetVertex(index).Transform(transform); 
            Line(camera, start, end, shape.Colour, thickness);
            start = end;
        }
    }
}