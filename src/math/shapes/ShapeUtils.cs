using System;
using System.Runtime.CompilerServices;
using static Howl.Math.Math;

namespace Howl.Math.Shapes;

public static class ShapeUtils
{
    /// <summary>
    /// Finds the closest vertex on a polygon to a given position.
    /// </summary>
    /// <param name="queryPosition">position to find the closest vertex to.</param>
    /// <param name="verticesX">The x-componentes of a polygons vertices.</param>
    /// <param name="verticesY">The y-componentes of a polygons vertices.</param>
    /// <returns>The index of the vertex in the vertices span that is the closest point.</returns>
    /// <exception cref="ArgumentException">Throws when the passed in vertex-spans do not match in length.</exception>
    public static int FindClosestVertexOnPolygon(Vector2 queryPosition, Span<float> verticesX, Span<float> verticesY)
    {
        return FindClosestVertexOnPolygon(queryPosition.X, queryPosition.Y, verticesX, verticesY);
    }

    /// <summary>
    /// Finds the closest vertex on a polygon to a given position.
    /// </summary>
    /// <param name="queryPositionX">the x-component of the position to find the closest vertex to.</param>
    /// <param name="queryPositionY">the x-component of the position to find the closest vertex to.</param>
    /// <param name="verticesX">The x-componentes of a polygons vertices.</param>
    /// <param name="verticesY">The y-componentes of a polygons vertices.</param>
    /// <returns>The index of the vertex in the vertices span that is the closest point.</returns>
    public static int FindClosestVertexOnPolygon(float queryPositionX, float queryPositionY, Span<float> verticesX, Span<float> verticesY)
    {
        int result = -1;
        float minDistance = float.MaxValue;

        if(verticesX.Length != verticesY.Length)
        {
            throw new ArgumentException($"verticesX length '{verticesX.Length}' is not equal to verticesY length '{verticesY.Length}'");
        }
        
        for(int i = 0; i < verticesX.Length; i++)
        {
            float distance = DistanceSquared(verticesX[i], verticesY[i], queryPositionX, queryPositionY);

            if(distance < minDistance)
            {
                minDistance = distance;
                result = i;
            }
        }

        return result;        
    }

    /// <summary>
    /// Calculates the centroid-vector of of a polygon.
    /// </summary>
    /// <param name="polygonVerticesX">The x-values of a polygon's vertices.</param>
    /// <param name="polygonVerticesY">The y-values of a polygon's vertices.</param>
    /// <returns>The centroid-vector.</returns>
    /// <exception cref="ArgumentException">Throws when the passed in vertex-spans do not match in length.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 GetCentroid(Span<float> polygonVerticesX, Span<float> polygonVerticesY)
    {
        GetCentroid(polygonVerticesX, polygonVerticesY, out float centroidX, out float centroidY);
        return new Vector2(centroidX, centroidY);
    }

    /// <summary>
    /// Calculates the centroid of a polygon.
    /// </summary>
    /// <param name="polygonVerticesX">The x-values of a polygon's vertices.</param>
    /// <param name="polygonVerticesY">The y-values of a polygon's vertices.</param>
    /// <param name="centroidX">The x-component centroid.</param>
    /// <param name="centroidY">The y-component centroid.</param>
    /// <exception cref="ArgumentException">Throws when the passed in vertex-spans do not match in length.</exception>
    public static void GetCentroid(Span<float> polygonVerticesX, Span<float> polygonVerticesY, out float centroidX, out float centroidY)
    {        
        if(polygonVerticesX.Length != polygonVerticesY.Length)
        {
            throw new ArgumentException($"polygonVerticesX length '{polygonVerticesX.Length}' is not equal to polygonVerticesY length {polygonVerticesY.Length}'");
        }

        int length = polygonVerticesX.Length;

        centroidX = 0;
        centroidY = 0;
        
        for(int i = 0; i < length; i++)
        {
            centroidX += polygonVerticesX[i];
            centroidY += polygonVerticesY[i];
        }

        centroidX /= length;
        centroidY /= length;
    }

    /// <summary>
    /// Gets the vertices of a polygon.
    /// </summary>
    /// <param name="verticesX">a span that contains the vertices of the polygon.</param>
    /// <param name="verticesY">a span that contains the vertices of the polygon.</param>
    /// <param name="firstVertexIndices">a span that contains the index for the first vertice of a polygon.</param>
    /// <param name="nextVertexIndices">a span that contains the next index for a given vertex index in the vertices span.</param>
    /// <param name="x">a span to store the found polygon's vertice x-component's</param>
    /// <param name="y">a span to store the found polygon's vertice y-component's</param>
    /// <param name="index">the index of the polygon in the first vertex indices span.</param>
    /// <param name="vertexCount">an int to store the count of vertices found.</param
    public static void GetPolygonVertices(Span<float> verticesX, Span<float> verticesY, Span<int> firstVertexIndices, 
        Span<int> nextVertexIndices, Span<float> x, Span<float> y, int index, ref int vertexCount
    ){
        vertexCount = 0;
        
        // gather polygon vertices.
        int firstVertexindex = firstVertexIndices[index]; 
        int nextVertexIndex = firstVertexindex;
        while (true)
        {
            // get the vertice.
            x[vertexCount] = verticesX[nextVertexIndex];
            y[vertexCount] = verticesY[nextVertexIndex];
            vertexCount++;

            // break out when reaching the end of the circular loop.
            nextVertexIndex = nextVertexIndices[nextVertexIndex];
            if (nextVertexIndex == firstVertexindex)
                break;
        }
    }

    /// <summary>
    /// Gets the vertices of a polygon.
    /// </summary>
    /// <param name="vertices">the soa vector2 that contains the vertices of the polygon.</param>
    /// <param name="firstVertexIndices">a span that contains the index for the first vertice of a polygon.</param>
    /// <param name="nextVertexIndices">a span that contains the next index for a given vertex index in the vertices span.</param>
    /// <param name="x">a span to store the found polygon's vertice x-component's</param>
    /// <param name="y">a span to store the found polygon's vertice y-component's</param>
    /// <param name="index">the index of the polygon in the first vertex indices span.</param>
    /// <param name="vertexCount">an int to store the count of vertices found.</param
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetPolygonVertices(
        Soa_Vector2 vertices,
        Span<int> firstVertexIndices,
        Span<int> nextVertexIndices,
        Span<float> x,
        Span<float> y,
        int index,
        ref int vertexCount
    )
    {
        GetPolygonVertices(vertices.X, vertices.Y, firstVertexIndices, nextVertexIndices, x, y, index, ref vertexCount);
    } 

    /// <summary>
    /// Gets the vertices of a polygon.
    /// </summary>
    /// <param name="vertices">the soa vector2 that contains the vertices of the polygon.</param>
    /// <param name="firstVertexIndices">a span that contains the index for the first vertice of a polygon.</param>
    /// <param name="nextVertexIndices">a span that contains the next index for a given vertex index in the vertices span.</param>
    /// <param name="x">a span to store the found polygon's vertice x-component's</param>
    /// <param name="y">a span to store the found polygon's vertice y-component's</param>
    /// <param name="index">the index of the polygon in the first vertex indices span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetPolygonVertices(
        Soa_Vector2 vertices,
        Span<int> firstVertexIndices,
        Span<int> nextVertexIndices,
        Span<float> x,
        Span<float> y,
        int index
    )
    {
        int vertexCount = 0;
        GetPolygonVertices(vertices.X, vertices.Y, firstVertexIndices, nextVertexIndices, x, y, index, ref vertexCount);        
    }

    /// <summary>
    /// Rotates a radian value by a given amount.
    /// </summary>
    /// <param name="increment">the amount - in radians - to increment the rotational radians by.</param>
    /// <param name="radians">the rotational radians to mutate.</param>
    /// <param name="sin">a float to store the sin value of the new rotation.</param>
    /// <param name="cos">a float to store the cos value of the new rotation.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void RotateRadians(float increment, ref float radians, ref float sin, ref float cos)
    {
        radians += increment;
        sin = MathF.Sin(radians);
        cos = MathF.Cos(radians);
    }

    /// <summary>
    /// Rotates a radian value by a given amount.
    /// </summary>
    /// <param name="increment">the amount - in radians - to increment the rotational radians by.</param>
    /// <param name="radians">a span containing the rotational radians to mutate.</param>
    /// <param name="index">the index in the radians span of the value to rotate.</param>
    /// <param name="sin">a span containing the rotational sin that will be mutated with the new sin.</param>
    /// <param name="cos">a span containing the rotational cos that will be mutated with the new cos.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void RotateRadians(float increment, Span<float> radians, Span<float> sin, Span<float> cos, int index)
    {
        ref float r = ref radians[index];
        r += increment;
        sin[index] = MathF.Sin(r);
        cos[index] = MathF.Cos(r);
    }

}