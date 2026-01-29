using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public static class Util
{
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float ToRadians(float degrees)
    {
        return (float)((double)degrees * (System.Math.PI / 180.0));
    }

    public static bool CirclesIntersect(
        Circle a,
        Circle b,
        out Vector2 normal,
        out float depth
    )
    {
        normal = Vector2.Zero;
        depth = 0f;

        float distanceSqrd = a.Origin.DistanceSquared(b.Origin);

        float radiusSum = a.Radius + b.Radius;
        float radiusSumSq = radiusSum * radiusSum;

        if (distanceSqrd >= radiusSumSq)
            return false;

        // Apply a full up force if the two colliders are in the exact same position.
        // this also stops the whole collision system from exploding.
        if (distanceSqrd < float.Epsilon)
        {
            normal = Vector2.Up;
            depth = radiusSum;
            return true;
        }

        float distance = MathF.Sqrt(distanceSqrd);
        normal = (b.Origin - a.Origin).Normalise();
        depth = radiusSum - distance;

        return true;
    }

    public static bool RectanglesIntersect(
        PolygonRectangle a,
        PolygonRectangle b,
        out Vector2 normal,
        out float depth
    )
    {

        Vector2 foundNormal;
        float foundDepth;

        normal = Vector2.Up;
        depth = float.MaxValue;


        if (PolygonIntersect(
                a.GetXVerticesAsSpan(), 
                a.GetYVerticesAsSpan(), 
                b.GetXVerticesAsSpan(), 
                b.GetYVerticesAsSpan(), 
                out foundNormal, 
                out foundDepth
            )
        )
        {            
            if(depth > foundDepth)
            {
                depth = foundDepth;
                normal = foundNormal;
            }
        }
        else
        {
            return false;
        }

        if (PolygonIntersect(
                b.GetXVerticesAsSpan(), 
                b.GetYVerticesAsSpan(), 
                a.GetXVerticesAsSpan(), 
                a.GetYVerticesAsSpan(), 
                out foundNormal, 
                out foundDepth
            )
        )
        {            
            if(depth > foundDepth)
            {
                depth = foundDepth;
                normal = foundNormal;
            }
        }
        else
        {
            return false;
        }

        depth /= normal.Length();
        normal = normal.Normalise();

        // when a new smaller   
        // depth is found but in relation to rect B, not A.
        // this is so that the resolution code will always push A out of B
        // and not push the two into each other when a smaller depth is found when 
        // looping through rect B.
        if ((b.GetCentroid() - a.GetCentroid()).Dot(normal) < 0)
        {
            normal = -normal;
        }

        
        return true;
    }

    private static bool PolygonIntersect(        
        ReadOnlySpan<float> xVerticesA, 
        ReadOnlySpan<float> yVerticesA, 
        ReadOnlySpan<float> xVerticesB, 
        ReadOnlySpan<float> yVerticesB, 
        out Vector2 normal,
        out float depth)
    {
        depth = float.MaxValue;
        normal = Vector2.Up;

        if(xVerticesA.Length != yVerticesA.Length)
        {
            throw new ArgumentException($"xVerticesA with length '{xVerticesA.Length}' does not match yVerticesA length '{xVerticesA.Length}'");
        }
        if(xVerticesB.Length != yVerticesB.Length)
        {
            throw new ArgumentException($"xVerticesB with length '{xVerticesB.Length}' does not match yVerticesB length '{xVerticesB.Length}'");
        }

        for(int i = 0; i < xVerticesA.Length; i++)
        {
            int vAIndex = i;
            int vBIndex = (i+1)%xVerticesA.Length;

            Vector2 va = new Vector2(xVerticesA[vAIndex], yVerticesA[vAIndex]);
            Vector2 vb = new Vector2(xVerticesA[vBIndex], yVerticesA[vBIndex]);

            Vector2 edge = vb - va;

            // the normal of the edge.
            // note: this only works as vertices are assumed to be in clockwise winding order.
            // change to new Vector2(edge.Y, -edge.X); if anti-clockwise.
            Vector2 axis = new Vector2(-edge.Y, edge.X); 
        
            // project all vertices onto the current edge to find the min and max values
            // of the two rectangles along the edge.
            ProjectVertices(xVerticesA, yVerticesA, axis, out float minA, out float maxA);
            ProjectVertices(xVerticesB, yVerticesB, axis, out float minB, out float maxB);
        
            if(minA >= maxB || minB >= maxA)
            {
                // there is separation.
                return false;
            }

            float axisDepth = MathF.Min(maxB - minA, maxA - minB);
            if(depth > axisDepth)
            {
                // only assign if the newly found intersection depth is smaller.
                depth = axisDepth;
                normal = axis;
            }
        }

        return true;
    }

    private static void ProjectVertices(
        ReadOnlySpan<float> xVertices, 
        ReadOnlySpan<float> yVertices,
        Vector2 axis, 
        out float min, 
        out float max)
    {
        min = float.MaxValue;
        max = float.MinValue;

        if(xVertices.Length != yVertices.Length)
        {
            throw new InvalidOperationException($"Projecting Vertices must have two spans of equal length. xVertices length '{xVertices.Length}' does not equal yVertices length '{yVertices.Length}'");
        }

        for(int i = 0; i < xVertices.Length; i++)
        {
            Vector2 vector = new Vector2(xVertices[i], yVertices[i]);
            float projection = Vector2.Dot(vector,axis);

            if(projection < min)
            {
                min = projection;
            }
            if(projection > max)
            {
                max = projection;
            }
        }
    }
}