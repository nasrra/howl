using System.Runtime.CompilerServices;
using Howl.Unmanaged.Collections;

namespace Howl.Math.Shapes;

public unsafe struct PolygonRectangle
{
    /// <summary>
    /// The maximum amount of vertices a PolygonRectangle can store.
    /// </summary>
    public const int VerticesLength = 4;

    public fixed float VerticesX[VerticesLength];
    public fixed float VerticesY[VerticesLength];

    public static void Initialise(ref PolygonRectangle polyRect, Rectangle rect)
    {
        Initialise(ref polyRect, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static void Initialise(ref PolygonRectangle polyRect, Array<Vector2> verts)
    {
        Debug.Assert(verts.Length == VerticesLength, $"verts length '{verts.Length}' is not equal to '{VerticesLength}'");

        fixed(float* xDst = polyRect.VerticesX)
        {
            fixed(float* yDst = polyRect.VerticesY)
            {
                for(int i = 0; i < VerticesLength; i++)
                {
                    xDst[i] = verts[i].X;
                    yDst[i] = verts[i].Y;
                }
            }
        }
    }

    public static void Initialise(ref PolygonRectangle polyRect, Vector2 vert0, Vector2 vert1, Vector2 vert2, Vector2 vert3)
    {
        fixed(float* xDst = polyRect.VerticesX)
        {
            fixed(float* yDst = polyRect.VerticesY)
            {
                xDst[0] = vert0.X;
                yDst[0] = vert0.Y;
                xDst[1] = vert1.X;
                yDst[1] = vert1.Y;
                xDst[2] = vert2.X;
                yDst[2] = vert2.Y;
                xDst[3] = vert3.X;
                yDst[3] = vert3.Y;
            }
        }        
    }

    public static void Initialise(ref PolygonRectangle polyRect, Array<float> vertsX, Array<float> vertsY)
    {
        Debug.Assert(vertsX.Length == VerticesLength, $"vertsX length '{vertsX.Length}' is not equal to '{VerticesLength}'");
        Debug.Assert(vertsY.Length == VerticesLength, $"vertsY length '{vertsY.Length}' is not equal to '{VerticesLength}'");

        fixed(float* xDst = polyRect.VerticesX)
        {
            fixed(float* yDst = polyRect.VerticesY)
            {
                for(int i = 0; i < VerticesLength; i++)
                {
                    xDst[i] = vertsX[i];
                    yDst[i] = vertsY[i];
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Initialise(ref PolygonRectangle polyRect, float x, float y, float width, float height)
    {
        fixed(float* xDst = polyRect.VerticesX)
        {
            fixed(float* yDst = polyRect.VerticesY)
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

    public static System.Span<float> GetVerticesXAsSpan(in PolygonRectangle polygonRectangle)
    {
        System.Span<float> span;
        fixed(float* ptr = polygonRectangle.VerticesX)
        {
            span = new System.Span<float>(ptr, VerticesLength);
        }
        return span;
    }

    public static System.Span<float> GetVerticesYAsSpan(in PolygonRectangle polygonRectangle)
    {
        System.Span<float> span;
        fixed(float* ptr = polygonRectangle.VerticesY)
        {
            span = new System.Span<float>(ptr, VerticesLength);
        }
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static PolygonRectangle Transform(in PolygonRectangle polyRect, in Transform transform)
    {
        PolygonRectangle other = default;
        Initialise(
            ref other,
            Vector2.Transform(transform, polyRect.VerticesX[0], polyRect.VerticesY[0]),
            Vector2.Transform(transform, polyRect.VerticesX[1], polyRect.VerticesY[1]),
            Vector2.Transform(transform, polyRect.VerticesX[2], polyRect.VerticesY[2]),
            Vector2.Transform(transform, polyRect.VerticesX[3], polyRect.VerticesY[3])
        );
        return other;
    }

    /// <summary>
    /// Calculates the centroid-vector of a polygon rectangle.
    /// </summary>
    /// <param name="polygonRectangle">The polygon rectangle.</param>
    /// <returns>The centroid-vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 GetCentroid(in PolygonRectangle polygonRectangle)
    {
        return ShapeUtils.CalculateCentroid(GetVerticesXAsSpan(polygonRectangle), GetVerticesYAsSpan(polygonRectangle));
    }

    /// <summary>
    /// Calculates the centroid-vector of a polygon rectangle.
    /// </summary>
    /// <param name="polygonRectangle">The polygon rectangle.</param>
    /// <param name="centroidX">the x-component of the centroid.</param>
    /// <param name="centroidY">the y-component of the centroid.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetCentroid(in PolygonRectangle polygonRectangle, ref float centroidX, ref float centroidY)
    {
        ShapeUtils.CalculateCentroid(GetVerticesXAsSpan(polygonRectangle), GetVerticesYAsSpan(polygonRectangle), ref centroidX, ref centroidY);
    }


    /// <summary>
    /// Gets the Axis-Aligned-Bounding-Box of a polygon rectangle.
    /// </summary>
    /// <param name="polygonRectangle">The polygon rectangle.</param>
    /// <returns>The calculated AABB.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Aabb GetAABB(in PolygonRectangle polygonRectangle)
    {
        Math.GetMinMaxVectors(GetVerticesXAsSpan(polygonRectangle), GetVerticesYAsSpan(polygonRectangle), out float minX, out float minY, out float maxX, out float maxY);
        return new(minX, minY, maxX, maxY);
    }

    /// <summary>
    /// Gets the width of a polygon rectangle.
    /// </summary>
    /// <param name="rectangle">the polygon rectangle.</param>
    /// <returns>the width of the rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float GetWidth(in PolygonRectangle rectangle)
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
    public static float GetHeight(in PolygonRectangle rectangle)
    {
        Vector2 vertexA = new Vector2(rectangle.VerticesX[0], rectangle.VerticesY[0]); 
        Vector2 vertexB = new Vector2(rectangle.VerticesX[3], rectangle.VerticesY[3]); 
        return Vector2.Distance(vertexA, vertexB);
    }
}