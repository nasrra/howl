using System;
using System.Runtime.CompilerServices;
using static Howl.Math.Shapes.Rectangle;
using static Howl.Math.Shapes.Circle;
using static Howl.Math.Shapes.PolygonRectangle;
using static Howl.Math.Math;

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

        float distanceSqrd = DistanceSquared(lhs.X, lhs.Y, rhs.X, rhs.Y);

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
        normal = (Center(rhs) - Center(lhs)).Normalise();
        depth = radiusSum - distance;

        return true;
    }

    /// <summary>
    /// Checks for intersection between two rectangles.
    /// </summary>
    /// <param name="lhs">The lhs-rectangle.</param>
    /// <param name="rhs">The rhs-rectangle.</param>
    /// <param name="lhsCentroid">the centroid of the lhs-rectangle.</param>
    /// <param name="rhsCentroid">the centroid of the rhs-rectangle.</param>
    /// <param name="normal">The normal of the intersection in relation to the rhs-rectangle.</param>
    /// <param name="depth">The depth of the intersection in relation to the rhs-rectangle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    /// <exception cref="ArgumentException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(PolygonRectangle lhs, PolygonRectangle rhs, Vector2 lhsCentroid, Vector2 rhsCentroid, out Vector2 normal, out float depth)
    {

        Span<float> lhsX = VerticesXAsSpan(lhs);
        Span<float> lhsY = VerticesYAsSpan(lhs);
        Span<float> rhsX = VerticesXAsSpan(rhs);
        Span<float> rhsY = VerticesYAsSpan(rhs);

        if(lhsX.Length != lhsY.Length)
        {
            throw new ArgumentException($"lhs vertices length do not match: '{lhsX.Length}' != '{lhsY.Length}'");
        }
        if(rhsX.Length != rhsY.Length)
        {
            throw new ArgumentException($"rhs vertices length do not match: '{rhsX.Length}' != '{rhsY.Length}'");
        }

        float foundNormalX;
        float foundNormalY;
        float foundDepth;

        normal = Vector2.Up;
        depth = float.MaxValue;


        if (OneWayIntersect(lhsX, lhsY, rhsX, rhsY, out foundNormalX, out foundNormalY, out foundDepth))
        {            
            if(depth > foundDepth)
            {
                depth = foundDepth;
                normal = new Vector2(foundNormalX, foundNormalY);
            }
        }
        else
        {
            return false;
        }

        if (OneWayIntersect(rhsX, rhsY, lhsX, lhsY, out foundNormalX, out foundNormalY, out foundDepth))
        {            
            if(depth > foundDepth)
            {
                depth = foundDepth;
                normal = new Vector2(foundNormalX, foundNormalY);
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
        if(Dot(rhsCentroid.X - lhsCentroid.X, rhsCentroid.Y - lhsCentroid.Y, normal.X, normal.Y) < 0)
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
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool OneWayIntersect(        
        ReadOnlySpan<float> polygonVerticesXA, 
        ReadOnlySpan<float> polygonVerticesYA, 
        ReadOnlySpan<float> polygonVerticesXB, 
        ReadOnlySpan<float> poylgonVerticesYB, 
        out float normalX,
        out float normalY,
        out float depth
    )
    {
        depth = float.MaxValue;
        normalX = 0;
        normalY = 1; // note: the default normal is up.

        for(int i = 0; i < polygonVerticesXA.Length; i++)
        {
            int vAIndex = i;
            int vBIndex = i+1;
            
            // this is faster than modulo.
            if(vBIndex >= polygonVerticesXA.Length)
                vBIndex = 0;

            float xA = polygonVerticesXA[vAIndex];
            float xB = polygonVerticesXA[vBIndex];
            float yA = polygonVerticesYA[vAIndex];
            float yB = polygonVerticesYA[vBIndex];

            float edgeX = xB - xA; 
            float edgeY = yB - yA;

            // the normal of the edge.
            // note: this only works as vertices are assumed to be in clockwise winding order.
            // change to new Vector2(edge.Y, -edge.X); if anti-clockwise.
            float axisX = -edgeY;
            float axisY = edgeX;
        
            // normalize (important for correct depth).
            Normalise(axisX, axisY, out axisX, out axisY);

            // project all vertices onto the current edge to find the min and max values
            // of the two rectangles along the edge.
            ProjectPolygon(polygonVerticesXA, polygonVerticesYA, axisX, axisY, out float minA, out float maxA);
            ProjectPolygon(polygonVerticesXB, poylgonVerticesYB, axisX, axisY, out float minB, out float maxB);
        
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
                normalX = axisX;
                normalY = axisY;
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
        return Intersect(VerticesXAsSpan(rectangle), VerticesYAsSpan(rectangle), circle, out normal, out depth);
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

        float axisX;
        float axisY;
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
            int vBIndex = i+1;

            // this is faster than modulo.
            if(vBIndex >= polygonVerticesX.Length)
                vBIndex = 0;

            float xA = polygonVerticesX[vAIndex];
            float xB = polygonVerticesX[vBIndex];
            float yA = polygonVerticesY[vAIndex];
            float yB = polygonVerticesY[vBIndex];

            float edgeX = xB - xA; 
            float edgeY = yB - yA; 

            // the normal of the edge.
            // note: this only works as vertices are assumed to be in clockwise winding order.
            // change to new Vector2(edge.Y, -edge.X); if anti-clockwise.
            axisX = -edgeY;
            axisY = edgeX;
        
            // normalize (important for correct depth).
            Normalise(axisX, axisY, out axisX, out axisY);

        
            // project all vertices onto the current edge to find the min and max values
            // of the two rectangles along the edge.
            ProjectPolygon(polygonVerticesX, polygonVerticesY, axisX, axisY, out minA, out maxA);
            ProjectCircle(circle, axisX, axisY, out minB, out maxB);
        
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
                normal = new Vector2(axisX, axisY);
            }
        }

        int closestPointIndex = ShapeUtils.FindClosestVertexOnPolygon(Center(circle), polygonVerticesX, polygonVerticesY);
        float closestPointX = polygonVerticesX[closestPointIndex];
        float closestPointY = polygonVerticesY[closestPointIndex];

        axisX = closestPointX - circle.X;
        axisY = closestPointY - circle.Y;
        Normalise(axisX, axisY, out axisX, out axisY);

        // project all vertices onto the current edge to find the min and max values
        // of the two rectangles along the edge.
        ProjectPolygon(polygonVerticesX, polygonVerticesY, axisX, axisY, out minA, out maxA);
        ProjectCircle(circle, axisX, axisY, out minB, out maxB);
    
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
            normal = new Vector2(axisX, axisY);
        }

        Vector2 polygonCentroid = ShapeUtils.Centroid(polygonVerticesX, polygonVerticesY);
        Vector2 circleCentroid = Center(circle);

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
        float axisX,
        float axisY, 
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
            float projection = Dot(verticesX[i], verticesY[i], axisX, axisY);

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
    /// <param name="circle">The circle to project.</param>
    /// <param name="axisX">The x-component of the axis to project onto.</param>
    /// <param name="axisY">The y-component of the axis to project onto.</param>
    /// <param name="min">The minimum-edge value of the circle.</param>
    /// <param name="max">The maximum-edge value of the circle.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void ProjectCircle(
        in Circle circle,
        float axisX,
        float axisY,
        out float min,
        out float max
    )
    {
        float directionAndRadiusX = axisX * circle.Radius;
        float directionAndRadiusY = axisY * circle.Radius;

        float vAX = circle.X + directionAndRadiusX;
        float vAY = circle.Y + directionAndRadiusY;
        float vBX = circle.X - directionAndRadiusX;
        float vBY = circle.Y - directionAndRadiusY;
        
        min = Dot(vAX, vAY, axisX, axisY);
        max = Dot(vBX, vBY, axisX, axisY);

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
        Vector2 distance = Center(b) - Center(a);
        Vector2 direction = distance.Normalise();
        contactPoint = Center(a) + (direction * a.Radius);
    }

    /// <summary>
    /// Finds the contact point between an intersecting polygon and circle.
    /// </summary>
    /// <param name="polygonXVertices">the x-values of the polygon's vertices </param>
    /// <param name="polygonYVertices">the y-values of the polygon's vertices.</param>
    /// <param name="circle">the circle.</param>
    /// <param name="xContactPoint">the x-value of the calculated contact point vector.</param>
    /// <param name="yContactPoint">the y-value of the calculated contact point vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(
        ReadOnlySpan<float> polygonXVertices, 
        ReadOnlySpan<float> polygonYVertices,
        in Circle circle, 
        out float xContactPoint, 
        out float yContactPoint)        
    {
        FindContactPoints(polygonXVertices, polygonYVertices, circle, out Vector2 contactPoint);
        xContactPoint = contactPoint.X;
        yContactPoint = contactPoint.Y;
    }

    /// <summary>
    /// Finds the contact point between an intersecting polygon and circle.
    /// </summary>
    /// <param name="polygonVerticesX">the x-values of the polygon's vertices.</param>
    /// <param name="polygonVerticesY">the y-values of the polygon's vertices.</param>
    /// <param name="circle">the circle.</param>
    /// <param name="contactPoint">the contact point.</param>
    /// <exception cref="ArgumentException">throws if the two vertice spans do not have the same length.</exception>    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(
        ReadOnlySpan<float> polygonVerticesX, 
        ReadOnlySpan<float> polygonVerticesY, 
        in Circle circle, 
        out Vector2 contactPoint
    )
    {
        if(polygonVerticesX.Length != polygonVerticesY.Length)
        {
            throw new ArgumentException($"polygonVerticesX length '{polygonVerticesX.Length}' is not equal to polygonVerticesY length '{polygonVerticesY.Length}'");
        }

        contactPoint = Vector2.MaxValue;
        float minDistSqrd = float.MaxValue;
        int length = polygonVerticesX.Length;

        // find the closest point for each edge of the rectangle.
        for(int startIndex = 0; startIndex < length; startIndex++)
        {
            Vector2 edgeStart = new Vector2(polygonVerticesX[startIndex], polygonVerticesY[startIndex]);
            
            int endIndex = startIndex + 1;
            
            // this is faster than modulo.
            if(endIndex >= length)
                endIndex = 0;

            Vector2 edgeEnd = new Vector2(polygonVerticesX[endIndex], polygonVerticesY[endIndex]);
            Math.ClosestPoint(edgeStart, edgeEnd, Center(circle), out Vector2 closestPoint, out float distSqrd);
            if(distSqrd < minDistSqrd)
            {
                minDistSqrd = distSqrd;
                contactPoint = closestPoint;
            }
        }
    }

    /// <summary>
    /// Finds the contact points between two intersecting polygons.
    /// </summary>
    /// <remarks>
    /// Note: ensure to check contact points amount before using contactPoint2.
    /// </remarks>
    /// <param name="polygonAVerticesX">the x-values of the polygon A's vertices.</param>
    /// <param name="polygonAVerticesY">the y-values of the polygon A's vertices.</param>
    /// <param name="polygonBVerticesX">the x-values of the polygon B's vertices.</param>
    /// <param name="polygonBVerticesY">the y-values of the polygon B's vertices.</param>
    /// <param name="epsilon">the threshold for equality in the case of two contact points being found.</param>
    /// <param name="contactPoint1X">the x-value of contact point 1 vector.</param>
    /// <param name="contactPoint1Y">the y-value of contact point 1 vector.</param>
    /// <param name="contactPoint2X">the x-value of contact point 2 vector.</param>
    /// <param name="contactPoint2Y">the y-value of contact point 2 vector.</param>
    /// <param name="contactPointsAmount">the amount of contact points found; can be 1 or 2.</param>
    /// <exception cref="ArgumentException">throws if polygon A or B vertices X and Y spans do not have matching lengths.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(
        ReadOnlySpan<float> polygonAVerticesX, 
        ReadOnlySpan<float> polygonAVerticesY, 
        ReadOnlySpan<float> polygonBVerticesX, 
        ReadOnlySpan<float> polygonBVerticesY,
        float epsilon, 
        out float contactPoint1X, 
        out float contactPoint1Y, 
        out float contactPoint2X, 
        out float contactPoint2Y, 
        out int contactPointsAmount
    )
    {
        if(polygonAVerticesX.Length != polygonAVerticesY.Length)
        {
            throw new ArgumentException($"polygonAVerticesX length '{polygonAVerticesX.Length}' is not equal to polygonAVerticesY length '{polygonAVerticesY.Length}'");
        }

        if(polygonBVerticesX.Length != polygonBVerticesY.Length)
        {
            throw new ArgumentException($"polygonBVerticesX length '{polygonBVerticesX.Length}' is not equal to polygonBVerticesY length '{polygonBVerticesY.Length}'");
        }

        contactPoint1X = 0;
        contactPoint1Y = 0;
        contactPoint2X = 0;
        contactPoint2Y = 0;
        contactPointsAmount = 0;
        float minDistSqrd = float.MaxValue;

        // polygon a to b.
        FindContactPointsOneWay(
            polygonAVerticesX, 
            polygonAVerticesY, 
            polygonBVerticesX, 
            polygonBVerticesY,
            epsilon,
            ref minDistSqrd,
            ref contactPoint1X,
            ref contactPoint1Y,
            ref contactPoint2X,
            ref contactPoint2Y,
            ref contactPointsAmount
        );

        // polygon b to a.
        FindContactPointsOneWay(
            polygonBVerticesX, 
            polygonBVerticesY,
            polygonAVerticesX, 
            polygonAVerticesY, 
            epsilon,
            ref minDistSqrd,
            ref contactPoint1X,
            ref contactPoint1Y,
            ref contactPoint2X,
            ref contactPoint2Y,
            ref contactPointsAmount
        );
    }

    /// <summary>
    /// Finds the contact points between two intersecting polygons.
    /// </summary>
    /// <remarks>
    /// Note: this function assumes polygon A and B vertices X and Y spans have matching lengths.
    /// </remarks>
    /// <param name="polygonAVerticesX">the x-values of the polygon A's vertices.</param>
    /// <param name="polygonAVerticesY">the y-values of the polygon A's vertices.</param>
    /// <param name="polygonBVerticesX">the x-values of the polygon B's vertices.</param>
    /// <param name="polygonBVerticesY">the y-values of the polygon B's vertices.</param>
    /// <param name="epsilon">the threshold for equality in the case of two contact points being found.</param>
    /// <param name="minDistSqrd">the current minimum dist sqrd found.</param>
    /// <param name="contactPoint1X">the x-component of the first contact point.</param>
    /// <param name="contactPoint1Y">the y-component of the first contact point.</param>
    /// <param name="contactPoint2X">the x-component of the second contact point.</param>
    /// <param name="contactPoint2Y">the y-component of the second contact point.</param>
    /// <param name="contactPointsAmount"></param>
    private static void FindContactPointsOneWay(
        ReadOnlySpan<float> polygonAVerticesX, 
        ReadOnlySpan<float> polygonAVerticesY, 
        ReadOnlySpan<float> polygonBVerticesX, 
        ReadOnlySpan<float> polygonBVerticesY,
        float epsilon, 
        ref float minDistSqrd,
        ref float contactPoint1X, 
        ref float contactPoint1Y, 
        ref float contactPoint2X, 
        ref float contactPoint2Y, 
        ref int contactPointsAmount
    )
    {
        int polygonAVerticesLength = polygonAVerticesX.Length;
        int polygonBVerticesLength = polygonBVerticesX.Length;

        for(int i = 0; i < polygonAVerticesLength; i++)
        {
            float pointX = polygonAVerticesX[i];
            float pointY = polygonAVerticesY[i];

            for(int startIndex = 0; startIndex < polygonBVerticesLength; startIndex++)
            {
                // find the closest point on polygon b to the vertice on polygon a.
                
                float edgeStartX = polygonBVerticesX[startIndex];
                float edgeStartY = polygonBVerticesY[startIndex];

                int endIndex = startIndex + 1;

                // this is faster than modulo.
                if(endIndex >= polygonBVerticesLength)
                    endIndex = 0;
                
                float edgeEndX = polygonBVerticesX[endIndex];
                float edgeEndY = polygonBVerticesY[endIndex];

                ClosestPoint(
                    edgeStartX, 
                    edgeStartY, 
                    edgeEndX, 
                    edgeEndY,
                    pointX,
                    pointY, 
                    out float closestPointX, 
                    out float closestPointY, 
                    out float distSqrd
                );

                if(NearlyEqual(distSqrd, minDistSqrd, epsilon))
                {
                    // note: there is a chance that two contact points can be in the same place.
                    // this is caused by when two vertices - one from each polygon - are in contact.
                    // without this 'if check', all the contact information will be wiped out 
                    // when those two corners hit eachother.

                    if(NearlyEqual(closestPointX, contactPoint1X, epsilon) == false
                    || NearlyEqual(closestPointY, contactPoint1Y, epsilon) == false)
                    {
                        // there are two contact points.
                        contactPointsAmount = 2;
                        contactPoint2X = closestPointX;             
                        contactPoint2Y = closestPointY;
                    }
                }
                else if(distSqrd < minDistSqrd)
                {
                    // a new absolute minimum contact point has been found.
                    // meaning that there is only one contact point.

                    minDistSqrd = distSqrd;
                    contactPointsAmount = 1;
                    contactPoint1X = closestPointX;
                    contactPoint1Y = closestPointY;
                }
            } 
        } 
    }
}