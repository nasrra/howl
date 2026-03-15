using System;
using System.Runtime.CompilerServices;

namespace Howl.Math.Shapes;

public unsafe struct PolygonRectangle
{
    /// <summary>
    /// The maximum amount of vertices a PolygonRectangle can store.
    /// </summary>
    public const int MaxVertices = 4;

    /// <summary>
    /// Gets and sets the x-coordinate value for each vertice.
    /// </summary>
    public fixed float VerticesX[MaxVertices];

    /// <summary>
    /// Gets and sets the y-coordinate value for each vertice.
    /// </summary>
    public fixed float VerticesY[MaxVertices];


    /// <summary>
    /// Constructs a PolygonRectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle to construct from.</param>
    public PolygonRectangle(in Rectangle rectangle)
    : this(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height)
    {}

    /// <summary>
    /// Constructs a PolygonRectangle.
    /// </summary>
    /// <param name="vertices">The vertices to insert into this polygon.</param>
    /// <exception cref="ArgumentException">thrown when the passed vertices span length is unsupported.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public PolygonRectangle(ReadOnlySpan<Vector2> vertices)
    {
        if(vertices.Length != MaxVertices)
        {
            throw new ArgumentException($"PolygonRectangle cannot store '{vertices.Length}' amount of vertices. The amount of vertices length must be '{MaxVertices}'.");
        }

        fixed(float* xDst = VerticesX)
        {
            fixed(float* yDst = VerticesY)
            {
                for(int i = 0; i < MaxVertices; i++)
                {
                    xDst[i] = vertices[i].X;
                    yDst[i] = vertices[i].Y;
                }
            }
        }
    }

    /// <summary>
    /// Constructs a PolygonRectangle.
    /// </summary>
    /// <param name="x">The x-coordinate of the origin point.</param>
    /// <param name="y">The y-coordinate of the origin point.</param>
    /// <param name="width">The width of this rectangle.</param>
    /// <param name="height">The height of this rectangle.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public PolygonRectangle(float x, float y, float width, float height)
    {
        fixed(float* xDst = VerticesX)
        {
            fixed(float* yDst = VerticesY)
            {
                float left = x;
                float top = y;
                float right = x+width;
                float bottom = y-height;

                // top left.
                xDst[0] = left;
                yDst[0] = top;

                // top right.
                xDst[1] = right;
                yDst[1] = top;

                // bottom right.
                xDst[2] = right;
                yDst[2] = bottom;

                // bottom left.
                xDst[3] = left;
                yDst[3] = bottom;
            }
        }
    }

    /// <summary>
    /// Gets the x-value of the vertices in a span.
    /// </summary>
    /// <param name="polygonRectangle">the polygon rectangle.</param>
    /// <returns>The span</returns>
    public static Span<float> VerticesXAsSpan(in PolygonRectangle polygonRectangle)
    {
        Span<float> span;
        fixed(float* ptr = polygonRectangle.VerticesX)
        {
            span = new Span<float>(ptr, PolygonRectangle.MaxVertices);
        }
        return span;
    }

    /// <summary>
    /// Gets y-value of the vertices in a span.
    /// </summary>
    /// <param name="polygonRectangle">the polygon rectangle</param>
    /// <returns>the span.</returns>
    public static Span<float> VerticesYAsSpan(in PolygonRectangle polygonRectangle)
    {
        Span<float> span;
        fixed(float* ptr = polygonRectangle.VerticesY)
        {
            span = new Span<float>(ptr, PolygonRectangle.MaxVertices);
        }
        return span;
    }

    /// <summary>
    /// Constructs a new rectangle by transform the vertices of the specified rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle to transform.</param>
    /// <param name="transform">The transform data.</param>
    /// <returns>The resultant rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static PolygonRectangle Transform(in PolygonRectangle rectangle, in Transform transform)
    {
        Span<Vector2> transformedVertices = stackalloc Vector2[PolygonRectangle.MaxVertices];
        for(int i = 0; i < PolygonRectangle.MaxVertices; i++)
        {
            transformedVertices[i] = Vector2.Transform(transform, rectangle.VerticesX[i], rectangle.VerticesY[i]);
        }
        return new PolygonRectangle(transformedVertices);
    }

    /// <summary>
    /// Calculates the centroid-vector of a polygon rectangle.
    /// </summary>
    /// <param name="polygonRectangle">The polygon rectangle.</param>
    /// <returns>The centroid-vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 Centroid(in PolygonRectangle polygonRectangle)
    {
        return ShapeUtils.GetCentroid(VerticesXAsSpan(polygonRectangle), VerticesYAsSpan(polygonRectangle));
    }

    /// <summary>
    /// Calculates the centroid-vector of a polygon rectangle.
    /// </summary>
    /// <param name="polygonRectangle">The polygon rectangle.</param>
    /// <param name="centroidX">the x-component of the centroid.</param>
    /// <param name="centroidY">the y-component of the centroid.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Centroid(in PolygonRectangle polygonRectangle, out float centroidX, out float centroidY)
    {
        ShapeUtils.GetCentroid(VerticesXAsSpan(polygonRectangle), VerticesYAsSpan(polygonRectangle), out centroidX, out centroidY);
    }


    /// <summary>
    /// Gets the Axis-Aligned-Bounding-Box of a polygon rectangle.
    /// </summary>
    /// <param name="polygonRectangle">The polygon rectangle.</param>
    /// <returns>The calculated AABB.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static AABB GetAABB(in PolygonRectangle polygonRectangle)
    {
        Math.GetMinMaxVectors(VerticesXAsSpan(polygonRectangle), VerticesYAsSpan(polygonRectangle), out float minX, out float minY, out float maxX, out float maxY);
        return new(minX, minY, maxX, maxY);
    }

    /// <summary>
    /// Gets the width of a polygon rectangle.
    /// </summary>
    /// <param name="rectangle">the polygon rectangle.</param>
    /// <returns>the width of the rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Width(in PolygonRectangle rectangle)
    {   
        Vector2 vertexA = new Vector2(rectangle.VerticesX[0], rectangle.VerticesY[0]); 
        Vector2 vertexB = new Vector2(rectangle.VerticesX[1], rectangle.VerticesY[1]); 
        return Vector2.Distance(vertexA, vertexB);
    }

    /// <summary>
    /// Gets the height of a polygon rectangle.
    /// </summary>
    /// <param name="rectangle">the polygon rectangle.</param>
    /// <returns>the height of the rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Height(in PolygonRectangle rectangle)
    {
        Vector2 vertexA = new Vector2(rectangle.VerticesX[0], rectangle.VerticesY[0]); 
        Vector2 vertexB = new Vector2(rectangle.VerticesX[3], rectangle.VerticesY[3]); 
        return Vector2.Distance(vertexA, vertexB);
    }
}