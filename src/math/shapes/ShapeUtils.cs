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
    public static Vector2 Centroid(Span<float> polygonVerticesX, Span<float> polygonVerticesY)
    {
        Centroid(polygonVerticesX, polygonVerticesY, out float centroidX, out float centroidY);
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
    public static void Centroid(Span<float> polygonVerticesX, Span<float> polygonVerticesY, out float centroidX, out float centroidY)
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
}