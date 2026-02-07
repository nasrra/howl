using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Howl.Math.Shapes;

/// <summary>
/// This is a utility class that contains functions for
/// Separating-Axis-Theorem intersection checks.
/// </summary>
public static class SAT
{
    /// <summary>
    /// Checks for intersection between two circles.
    /// </summary>
    /// <param name="lhs">The lhs-circle data.</param>
    /// <param name="rhs">The rhs-circle data.</param>
    /// <param name="normal">The normal of the intersection in relation to the rhs-circle.</param>
    /// <param name="depth">The depth of the intersection in relation to the rhs-circle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(
        Circle lhs,
        Circle rhs,
        out Vector2 normal,
        out float depth
    )
    {
        normal = Vector2.Zero;
        depth = 0f;

        float distanceSqrd = lhs.Origin.DistanceSquared(rhs.Origin);

        float radiusSum = lhs.Radius + rhs.Radius;
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
        normal = (rhs.Origin - lhs.Origin).Normalise();
        depth = radiusSum - distance;

        return true;
    }

    /// <summary>
    /// Checks for intersection between two rectangles.
    /// </summary>
    /// <param name="lhs">The lhs-rectangle.</param>
    /// <param name="rhs">The rhs-rectangle.</param>
    /// <param name="normal">The normal of the intersection in relation to the rhs-rectangle.</param>
    /// <param name="depth">The depth of the intersection in relation to the rhs-rectangle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(PolygonRectangle lhs, PolygonRectangle rhs, out Vector2 normal, out float depth)
    {

        Vector2 foundNormal;
        float foundDepth;

        normal = Vector2.Up;
        depth = float.MaxValue;


        if (OneWayIntersect(lhs.GetVerticesXAsSpan(), lhs.GetVerticesYAsSpan(), rhs.GetVerticesXAsSpan(), rhs.GetVerticesYAsSpan(), out foundNormal, out foundDepth))
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

        if (OneWayIntersect(rhs.GetVerticesXAsSpan(), rhs.GetVerticesYAsSpan(), lhs.GetVerticesXAsSpan(), lhs.GetVerticesYAsSpan(), out foundNormal, out foundDepth))
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

        // when a new smaller   
        // depth is found but in relation to rect B, not A.
        // this is so that the resolution code will always push A out of B
        // and not push the two into each other when a smaller depth is found when 
        // looping through rect B.
        if ((rhs.GetCentroid() - lhs.GetCentroid()).Dot(normal) < 0)
        {
            normal = -normal;
        }

        
        return true;
    }

    /// <summary>
    /// Checks for intersection from polygon A to polygon B, but not the vice versa.
    /// </summary>
    /// <param name="polygonVerticesXA">the x-values of polygonA's vertices.</param>
    /// <param name="polygonVerticesYA">the y-values of polygonA's vertices.</param>
    /// <param name="polygonVerticesXB">the x-values of polygonB's vertices.</param>
    /// <param name="poylgonVerticesYB">the y-values of polygonB's vertices.</param>
    /// <param name="normal">The normal of the intersection in relation to polygon B.</param>
    /// <param name="depth">The depth of the intersection in relation to polygon B.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    /// <exception cref="ArgumentException">throw when polygonA or polygonB vertex spans do not share the same length.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool OneWayIntersect(        
        ReadOnlySpan<float> polygonVerticesXA, 
        ReadOnlySpan<float> polygonVerticesYA, 
        ReadOnlySpan<float> polygonVerticesXB, 
        ReadOnlySpan<float> poylgonVerticesYB, 
        out Vector2 normal,
        out float depth
    )
    {
        depth = float.MaxValue;
        normal = Vector2.Up;

        if(polygonVerticesXA.Length != polygonVerticesYA.Length)
        {
            throw new ArgumentException($"polygonVerticesXA length '{polygonVerticesXA.Length}' does not equal polygonVerticesYA length '{polygonVerticesYA.Length}'");
        }
        if(polygonVerticesXB.Length != poylgonVerticesYB.Length)
        {
            throw new ArgumentException($"poylgonVerticesXB length '{polygonVerticesXB.Length}' does not equal poylgonVerticesYB length '{poylgonVerticesYB.Length}'");
        }

        for(int i = 0; i < polygonVerticesXA.Length; i++)
        {
            int vAIndex = i;
            int vBIndex = (i+1)%polygonVerticesXA.Length;

            Vector2 va = new Vector2(polygonVerticesXA[vAIndex], polygonVerticesYA[vAIndex]);
            Vector2 vb = new Vector2(polygonVerticesXA[vBIndex], polygonVerticesYA[vBIndex]);

            Vector2 edge = vb - va;

            // the normal of the edge.
            // note: this only works as vertices are assumed to be in clockwise winding order.
            // change to new Vector2(edge.Y, -edge.X); if anti-clockwise.
            Vector2 axis = new Vector2(-edge.Y, edge.X).Normalise(); 
        
            // project all vertices onto the current edge to find the min and max values
            // of the two rectangles along the edge.
            ProjectPolygon(polygonVerticesXA, polygonVerticesYA, axis, out float minA, out float maxA);
            ProjectPolygon(polygonVerticesXB, poylgonVerticesYB, axis, out float minB, out float maxB);
        
            if(minA > maxB || minB > maxA)
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

    /// <summary>
    /// Checks whether a rectangle and a circle intersect.
    /// </summary>
    /// <param name="rectangle">The rectangle data.</param>
    /// <param name="circle">The circle data.</param>
    /// <param name="normal">The normal of the intersection in relation to the circle.</param>
    /// <param name="depth">The depth of the intersection in relation to the circle.</param>
    /// <returns>true, if there was an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(PolygonRectangle rectangle, Circle circle, out Vector2 normal, out float depth)
    {
        return Intersect(rectangle.GetVerticesXAsSpan(), rectangle.GetVerticesYAsSpan(), circle, out normal, out depth);
    }

    /// <summary>
    /// Checks whether a polygon and a circle intersect.
    /// </summary>
    /// <param name="polygonVerticesX">The x-values of a polygon's vertices.</param>
    /// <param name="polygonVerticesY">The y-values of a polygon's vertices.</param>
    /// <param name="circle">The circle data.</param>
    /// <param name="normal">The normal of the intersect in relation to the circle.</param>
    /// <param name="depth">The depth of the intersection in relation to the circle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    /// <exception cref="ArgumentException">Throws when the passed in vertex-spans do not match in length.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect
    (
        ReadOnlySpan<float> polygonVerticesX,
        ReadOnlySpan<float> polygonVerticesY,
        Circle circle,
        out Vector2 normal,
        out float depth
    )
    {
        depth = float.MaxValue;
        normal = Vector2.Up;

        Vector2 axis;
        float axisDepth;
        float minA;
        float maxA;
        float minB;
        float maxB;

        if(polygonVerticesX.Length != polygonVerticesY.Length)
        {
            throw new ArgumentException($"polygonVerticesX length '{polygonVerticesX.Length}' does not equal polygonVerticesY length '{polygonVerticesY.Length}'");
        }

        for(int i = 0; i < polygonVerticesX.Length; i++)
        {
            int vAIndex = i;
            int vBIndex = (i+1)%polygonVerticesX.Length;

            Vector2 va = new Vector2(polygonVerticesX[vAIndex], polygonVerticesY[vAIndex]);
            Vector2 vb = new Vector2(polygonVerticesX[vBIndex], polygonVerticesY[vBIndex]);

            Vector2 edge = vb - va;

            // the normal of the edge.
            // note: this only works as vertices are assumed to be in clockwise winding order.
            // change to new Vector2(edge.Y, -edge.X); if anti-clockwise.
            axis = new Vector2(-edge.Y, edge.X).Normalise(); 
        
            // project all vertices onto the current edge to find the min and max values
            // of the two rectangles along the edge.
            ProjectPolygon(polygonVerticesX, polygonVerticesY, axis, out minA, out maxA);
            ProjectCircle(circle, axis, out minB, out maxB);
        
            if(minA > maxB || minB > maxA)
            {
                // there is separation.
                return false;
            }

            axisDepth = MathF.Min(maxB - minA, maxA - minB);
            if(depth > axisDepth)
            {
                // only assign if the newly found intersection depth is smaller.
                depth = axisDepth;
                normal = axis;
            }
        }

        int closestPointIndex = ShapeUtils.FindClosestVertexOnPolygon(circle.Origin, polygonVerticesX, polygonVerticesY);
        Vector2 closestPoint = new Vector2(polygonVerticesX[closestPointIndex], polygonVerticesY[closestPointIndex]);
        
        axis = (closestPoint - circle.Origin).Normalise();

        // project all vertices onto the current edge to find the min and max values
        // of the two rectangles along the edge.
        ProjectPolygon(polygonVerticesX, polygonVerticesY, axis, out minA, out maxA);
        ProjectCircle(circle, axis, out minB, out maxB);
    
        if(minA > maxB || minB > maxA)
        {
            // there is separation.
            return false;
        }

        axisDepth = MathF.Min(maxB - minA, maxA - minB);
        if(depth > axisDepth)
        {
            // only assign if the newly found intersection depth is smaller.
            depth = axisDepth;
            normal = axis;
        }

        Vector2 polygonCentroid = ShapeUtils.GetCentroid(polygonVerticesX, polygonVerticesY);
        Vector2 circleCentroid = circle.Origin;

        // when a new smaller   
        // depth is found but in relation to rect B, not A.
        // this is so that the resolution code will always push A out of B
        // and not push the two into each other when a smaller depth is found when 
        // looping through rect B.
        if ((circleCentroid - polygonCentroid).Dot(normal) < 0)
        {
            normal = -normal;
        }
        return true;
    }

    /// <summary>
    /// projects a set of vertices onto a normalised axis.
    /// </summary>
    /// <remarks>
    /// Remarks: the 'edge' of a polygon is defined as the outer most vertices that are projected onto the axis.
    /// </remarks>
    /// <param name="verticesX">The x-values of the vertices.</param>
    /// <param name="verticesY">The y-values of the vertices.</param>
    /// <param name="axis">The normalised axis.</param>
    /// <param name="min">the minimum edge-value of the polygon.</param>
    /// <param name="max">the maximum edge-value of the polygon.</param>
    /// <exception cref="ArgumentException">Throws when the passed in vertex-spans do not match in length.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void ProjectPolygon(
        ReadOnlySpan<float> verticesX, 
        ReadOnlySpan<float> verticesY,
        Vector2 axis, 
        out float min, 
        out float max
    )
    {
        min = float.MaxValue;
        max = float.MinValue;

        if(verticesX.Length != verticesY.Length)
        {
            throw new ArgumentException($"verticesX length '{verticesX.Length}' is not equal verticesY length '{verticesY.Length}'");
        }

        for(int i = 0; i < verticesX.Length; i++)
        {
            Vector2 vector = new Vector2(verticesX[i], verticesY[i]);
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

    /// <summary>
    /// Projects the edges of circle onto a given axis.
    /// </summary>
    /// <remarks>
    /// Remarks: The 'edge' of a circle is defined as the radius from the origin of the circle.
    /// </remarks>
    /// <param name="circle">The circle to project.</param>
    /// <param name="axis">The axis to project onto.</param>
    /// <param name="min">The minimum-edge value of the circle.</param>
    /// <param name="max">The maximum-edge value of the circle.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void ProjectCircle(
        Circle circle,
        Vector2 axis,
        out float min,
        out float max
    )
    {
        Vector2 directionAndRadius = axis * circle.Radius;
        
        Vector2 vertex1 = circle.Origin + directionAndRadius;
        Vector2 vertex2 = circle.Origin - directionAndRadius;
        
        min = vertex1.Dot(axis);
        max = vertex2.Dot(axis);

        if(min > max)
        {
            float temp = min;
            min = max;
            max = temp;
        }
    }  

    /// <summary>
    /// Finds the contact point between two intersecting circles.
    /// </summary>
    /// <param name="a">circle a.</param>
    /// <param name="b">circle b.</param>
    /// <param name="xContactPoint">the x-value of the calculated contact point vector.</param>
    /// <param name="yContactPoint">the y-value of the calculated contact point vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(in Circle a, in Circle b, out float xContactPoint, out float yContactPoint)
    {
        FindContactPoints(a,b,out Vector2 contactPoint);
        xContactPoint = contactPoint.X;
        yContactPoint = contactPoint.Y;
    }

    /// <summary>
    /// Finds the contact point between two intersecting circles.
    /// </summary>
    /// <param name="a">circle a.</param>
    /// <param name="b">circle b.</param>
    /// <param name="contactPoint">The calculated contact point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(in Circle a, in Circle b, out Vector2 contactPoint)
    {
        Vector2 distance = b.Origin - a.Origin;
        Vector2 direction = distance.Normalise();
        contactPoint = a.Origin + (direction * a.Radius);
    }

    /// <summary>
    /// Finds the contact point between an intersecting rectangle and circle.
    /// </summary>
    /// <param name="rectangle">the rectangle.</param>
    /// <param name="circle">the circle.</param>
    /// <param name="xContactPoint">the x-value of the calculated contact point vector.</param>
    /// <param name="yContactPoint">the y-value of the calculated contact point vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(in PolygonRectangle rectangle, in Circle circle, out float xContactPoint, out float yContactPoint)
    {
        FindContactPoints(rectangle, circle, out Vector2 contactPoint);
        xContactPoint = contactPoint.X;
        yContactPoint = contactPoint.Y;
    }

    /// <summary>
    /// Finds the contact point between an intersecting rectangle and circle.
    /// </summary>
    /// <param name="rectangle">the rectangle.</param>
    /// <param name="circle">the circle.</param>
    /// <param name="contactPoint">the contact point.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(in PolygonRectangle rectangle, in Circle circle, out Vector2 contactPoint)
    {
        contactPoint = Vector2.MaxValue;
        float minDistSqrd = float.MaxValue;
        ReadOnlySpan<float> x = rectangle.GetVerticesXAsReadOnlySpan();
        ReadOnlySpan<float> y = rectangle.GetVerticesYAsReadOnlySpan();

        // find the closest point for each edge of the rectangle.
        for(int startIndex = 0; startIndex < PolygonRectangle.MaxVertices; startIndex++)
        {
            Vector2 start = new Vector2(x[startIndex], y[startIndex]);
            int endIndex = (startIndex + 1) % PolygonRectangle.MaxVertices;
            Vector2 end = new Vector2(x[endIndex], y[endIndex]);
            Util.ClosestPoint(start, end, circle.Origin, out Vector2 closestPoint, out float distSqrd);
            if(distSqrd < minDistSqrd)
            {
                minDistSqrd = distSqrd;
                contactPoint = closestPoint;
            }
        }
    }
}