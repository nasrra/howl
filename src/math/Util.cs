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

    public unsafe static bool FixedRectanglesIntersect(
        PolygonRectangle a,
        PolygonRectangle b,
        out Vector2 normal,
        out float depth
    )
    {
        normal = Vector2.Up;
        depth = float.MaxValue;

        for(int i = 0; i < PolygonRectangle.MaxVertices; i++)
        {
            int vAIndex = i;
            int vBIndex = (i+1)%PolygonRectangle.MaxVertices;

            Vector2 va = new Vector2(a.XVertices[vAIndex], a.YVertices[vAIndex]);
            Vector2 vb = new Vector2(a.XVertices[vBIndex], a.YVertices[vBIndex]);

            Vector2 edge = vb - va;

            // the normal of the edge.
            // note: this only works as vertices are assumed to be in clockwise winding order.
            // change to new Vector2(edge.Y, -edge.X); if anti-clockwise.
            Vector2 axis = new Vector2(-edge.Y, edge.X); 
        
            // project all vertices onto the current edge to find the min and max values
            // of the two rectangles along the edge.
            ProjectVertices(a.GetXVerticesAsSpan(), a.GetYVerticesAsSpan(), axis, out float minA, out float maxA);
            ProjectVertices(b.GetXVerticesAsSpan(), b.GetYVerticesAsSpan(), axis, out float minB, out float maxB);
        
            if(minA >= maxB || minB >= maxA)
            {
                // there is separation.
                return false;
            }

            float axisDepth = MathF.Min(maxB - minA, maxA - minB);
            if(depth > axisDepth)
            {
                depth = axisDepth;
                normal = axis;
            }
        }

        for(int i = 0; i < PolygonRectangle.MaxVertices; i++)
        {
            int vAIndex = i;
            int vBIndex = (i+1)%PolygonRectangle.MaxVertices;

            Vector2 va = new Vector2(b.XVertices[vAIndex], b.YVertices[vAIndex]);
            Vector2 vb = new Vector2(b.XVertices[vBIndex], b.YVertices[vBIndex]);

            Vector2 edge = vb - va;

            // the normal of the edge.
            // note: this only works as vertices are assumed to be in clockwise winding order.
            // change to new Vector2(edge.Y, -edge.X); if anti-clockwise.
            Vector2 axis = new Vector2(-edge.Y, edge.X); 
        
            // project all vertices onto the current edge to find the min and max values
            // of the two rectangles along the edge.
            ProjectVertices(a.GetXVerticesAsSpan(), a.GetYVerticesAsSpan(), axis, out float minA, out float maxA);
            ProjectVertices(b.GetXVerticesAsSpan(), b.GetYVerticesAsSpan(), axis, out float minB, out float maxB);
        
            if(minA >= maxB || minB >= maxA)
            {
                // there is separation.
                return false;
            }

            float axisDepth = MathF.Min(maxB - minA, maxA - minB);
            if(depth > axisDepth)
            {
                depth = axisDepth;
                normal = axis;
            }
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