using System;
using System.Runtime.CompilerServices;

namespace Howl.Math.Shapes;

public static class ShapeUtils
{
    /// <summary>
    /// Finds the closest vertex on a polygon to a given position.
    /// </summary>
    /// <param name="queryPosition">The position to find the closest point from.</param>
    /// <param name="verticesX">The x-values of a polygons vertices.</param>
    /// <param name="verticesY">The y-values of a polygons vertices.</param>
    /// <returns>The index of the vertex in the vertices span that is the closest point.</returns>
    /// <exception cref="ArgumentException">Throws when the passed in vertex-spans do not match in length.</exception>
    public static int FindClosestVertexOnPolygon(Vector2 queryPosition, ReadOnlySpan<float> verticesX, ReadOnlySpan<float> verticesY)
    {
        int result = -1;
        float minDistance = float.MaxValue;

        if(verticesX.Length != verticesY.Length)
        {
            throw new ArgumentException($"verticesX length '{verticesX.Length}' is not equal to verticesY length '{verticesY.Length}'");
        }
        
        int length = verticesX.Length;

        for(int i = 0; i < length; i++)
        {
            Vector2 vector = new Vector2(verticesX[i], verticesY[i]);
            float distance = vector.DistanceSquared(queryPosition);

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
    public static Vector2 GetCentroid(ReadOnlySpan<float> polygonVerticesX, ReadOnlySpan<float> polygonVerticesY)
    {
        if(polygonVerticesX.Length != polygonVerticesY.Length)
        {
            throw new ArgumentException($"polygonVerticesX length '{polygonVerticesX.Length}' is not equal to polygonVerticesY length {polygonVerticesY.Length}'");
        }

        int length = polygonVerticesX.Length;

        Vector2 sum = Vector2.Zero;
        for(int i = 0; i < length; i++)
        {
            sum += new Vector2(polygonVerticesX[i], polygonVerticesY[i]);
        }
        return sum /= length;
    }
}