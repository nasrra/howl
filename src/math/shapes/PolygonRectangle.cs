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
    /// <returns>The span</returns>
    public Span<float> GetVerticesXAsSpan()
    {
        Span<float> span;
        fixed(float* ptr = VerticesX)
        {
            span = new Span<float>(ptr, MaxVertices);
        }
        return span;
    }

    /// <summary>
    /// Gets the x-value of vertices in a readonly span. 
    /// </summary>
    /// <returns>the readonly span.</returns>
    public ReadOnlySpan<float> GetVerticesXAsReadOnlySpan()
    {
        Span<float> span;
        fixed(float* ptr = VerticesX)
        {
            span = new Span<float>(ptr, MaxVertices);
        }
        return span;
    }

    /// <summary>
    /// Gets y-value of the vertices in a span.
    /// </summary>
    /// <returns>The span</returns>
    public Span<float> GetVerticesYAsSpan()
    {
        Span<float> span;
        fixed(float* ptr = VerticesY)
        {
            span = new Span<float>(ptr, MaxVertices);
        }
        return span;
    }

    /// <summary>
    /// Gets the y-value of the vertices in a readonly span.
    /// </summary>
    /// <returns>the readonly span.</returns>
    public ReadOnlySpan<float> GetVerticesYAsReadOnlySpan()
    {
        Span<float> span;
        fixed(float* ptr = VerticesY)
        {
            span = new Span<float>(ptr, MaxVertices);
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
    public static PolygonRectangle Transform(PolygonRectangle rectangle, Transform transform)
    {
        Span<Vector2> transformedVertices = stackalloc Vector2[MaxVertices];
        for(int i = 0; i < MaxVertices; i++)
        {
            transformedVertices[i] = Vector2.Transform(rectangle.VerticesX[i], rectangle.VerticesY[i], transform);
        }
        return new PolygonRectangle(transformedVertices);
    }

    /// <summary>
    /// Calculates the centroid-vector of this polygon rectangle.
    /// </summary>
    /// <returns>The centroid-vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vector2 GetCentroid()
    {
        return ShapeUtils.GetCentroid(GetVerticesXAsSpan(), GetVerticesYAsSpan());
    }

    /// <summary>
    /// Gets the Axis-Aligned-Bounding-Box of this rectangle.
    /// </summary>
    /// <returns>The calculated AABB.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public AABB GetAABB()
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        ReadOnlySpan<float> verticesX = GetVerticesXAsSpan();
        ReadOnlySpan<float> verticesY = GetVerticesYAsSpan();

        for(int i = 0; i < MaxVertices; i++)
        {
            float x = verticesX[i];
            if (x < minX)
            {
                minX = x;
            }
            if(x > maxX)
            {
                maxX = x;
            }
        }

        for(int i = 0; i < MaxVertices; i++)
        {
            float y = verticesY[i];
            if(y < minY)
            {
                minY = y;
            }
            if(y > maxY)
            {
                maxY = y;
            }
        }

        return new(
            minX,
            minY,
            maxX,
            maxY
        );
    }
}