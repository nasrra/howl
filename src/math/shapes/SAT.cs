using System;
using System.Runtime.CompilerServices;
using static Howl.Math.Shapes.Rectangle;
using static Howl.Math.Shapes.Circle;
using static Howl.Math.Shapes.PolygonRectangle;
using static Howl.Math.Math;
using static Howl.Math.Shapes.ShapeUtils;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Howl.Math.Shapes;

/// <summary>
/// This is a utility class that contains functions for
/// Separating-Axis-Theorem intersection checks.
/// </summary>
public static class SAT
{
    public const float PolygonContactPointEpsilon = 1e-5f;

    // the fallback normal for any SAT intersect will be up.
    // meaning that if any shapes perfectly overlap with eachother
    // (sharing the same position) one will be pushed up and the other down.
    public const float InitialNormalX = 0;
    public const float InitialNormalY = 1;




    /*******************
    
        Circle.
    
    ********************/




    /// <summary>
    /// Checks for intersection between two circles.
    /// </summary>
    /// <param name="lhs">The left-hand side circle data.</param>
    /// <param name="rhs">The right-hand side circle data.</param>
    /// <param name="normal">The normal of the intersection in relation to the right-hand side circle.</param>
    /// <param name="depth">The depth of the intersection in relation to the right-hand side circle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool CirclesIntersect(
        in Circle lhs,
        in Circle rhs,
        out Vector2 normal,
        out float depth
    )
    {
        bool intersects = CirclesIntersect(lhs, rhs, out float normalX, out float normalY, out depth);
        normal = new Vector2(normalX, normalY);
        return intersects;
    }

    /// <summary>
    /// Checks for intersection between two circles.
    /// </summary>
    /// <param name="lhs">the left-hand side circle.</param>
    /// <param name="rhs">the right-hand side circle.</param>
    /// <param name="normalX">the x-component of the intersection normal in relation to the right-hand side circle.</param>
    /// <param name="normalY">the y-component of the intersection normal in relation to the right-hand side circle.</param>
    /// <param name="depth">the depth of the intersection in relation to the right-hand side circle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool CirclesIntersect(in Circle lhs, in Circle rhs, out float normalX, out float normalY, out float depth)
    {
        return CirclesIntersect(lhs.X, lhs.Y, lhs.Radius, rhs.X, rhs.Y , rhs.Radius, out normalX, out normalY, out depth);
    }

    /// <summary>
    /// Checks for intersection between two circles.
    /// </summary>
    /// <param name="lhsX">the positional x-component of the left-hand side circle.</param>
    /// <param name="lhsY">the positional y-component of the left-hand side circle.</param>
    /// <param name="lhsRadius">the radius of the left-hand side circle.</param>
    /// <param name="rhsX">the positional x-component of the right-hand side circle.</param>
    /// <param name="rhsY">the positional y-component of the right-hand side circle.</param>
    /// <param name="rhsRadius">the radius of the right-hand side circle.</param>
    /// <param name="normalX">the x-component of the intersection normal in relation to the right-hand side circle.</param>
    /// <param name="normalY">the y-component of the intersection normal in relation to the right-hand side circle.</param>
    /// <param name="depth">the depth of the intersection in relation to the right-hand side circle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool CirclesIntersect(float lhsX, float lhsY, float lhsRadius, float rhsX, float rhsY, float rhsRadius, out float normalX, out float normalY, out float depth)
    {
        normalX = InitialNormalX;
        normalY = InitialNormalY;
        depth = 0f;

        float distanceSqrd = DistanceSquared(lhsX, lhsY, rhsX, rhsY);
        float radiusSum = lhsRadius + rhsRadius;
        float radiusSumSq = radiusSum * radiusSum;

        if (distanceSqrd >= radiusSumSq)
            return false;

        // Apply a full up force if the two colliders are in the exact same position.
        // this also stops the whole collision system from exploding.
        if (distanceSqrd < float.Epsilon)
        {
            depth = radiusSum;
            return true;
        }

        float distance = MathF.Sqrt(distanceSqrd);
        Normalise(rhsX - lhsX, rhsY - lhsY, out normalX, out normalY);
        depth = radiusSum - distance;
        return true;        
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
    public static void ProjectCircle(
        in Circle circle,
        float axisX,
        float axisY,
        out float min,
        out float max
    )
    {
        ProjectCircle(circle.X, circle.Y, circle.Radius, axisX, axisY, out min, out max);
    }  

    /// <summary>
    /// Projects the edges of a circle onto a given axis.
    /// </summary>
    /// <param name="circleX">the positional x-component of the circle.</param>
    /// <param name="circleY">the positional y-component of the circle.</param>
    /// <param name="circleRadius">the radius of the circle.</param>
    /// <param name="axisX">the x-component of the axis to project onto.</param>
    /// <param name="axisY">the y-component of the axis to project onto.</param>
    /// <param name="min">the minimum-edge value of the circle.</param>
    /// <param name="max">the maximum-edge value of the circle.</param>
    public static void ProjectCircle(
        float circleX,
        float circleY,
        float circleRadius,
        float axisX,
        float axisY,
        out float min,
        out float max
    )
    {
        float directionAndRadiusX = axisX * circleRadius;
        float directionAndRadiusY = axisY * circleRadius;

        float vAX = circleX + directionAndRadiusX;
        float vAY = circleY + directionAndRadiusY;
        float vBX = circleX - directionAndRadiusX;
        float vBY = circleY - directionAndRadiusY;
        
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
    /// <param name="contactPoint">The calculated contact point relative to circle a.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(in Circle a, in Circle b, out Vector2 contactPoint)
    {
        FindContactPoints(a, b, out float cX, out float cY);
        contactPoint = new Vector2(cX, cY);
    }

    /// <summary>
    /// Finds the contact point between two intersecting circles.
    /// </summary>
    /// <param name="a">circle a.</param>
    /// <param name="b">circle b.</param>
    /// <param name="cX">the x-component of the calculated contact point vector relative to circle a.</param>
    /// <param name="cY">the y-component of the calculated contact point vector relative to circle a.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(in Circle a, in Circle b, out float cX, out float cY)
    {
        FindContactPoints(a.X, a.Y, a.Radius, b.X, b.Y, out cX, out cY);
    }

    /// <summary>
    /// Finds the contact points between two circles.
    /// </summary>
    /// <param name="aX">the positional x-component of circle a.</param>
    /// <param name="aY">the positional y-component of circle a.</param>
    /// <param name="aRadius">the radius of circle a.</param>
    /// <param name="bX">the positional x-component of circle b.</param>
    /// <param name="bY">the positional y-component of circle b.</param>
    /// <param name="cPX">the x-component of the contact point vector relative to circle a.</param>
    /// <param name="cPY">the y-component of the contact point vector relative to circle a.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(float aX, float aY, float aRadius, float bX, float bY, out float cPX, out float cPY)
    {
        float distanceX = bX - aX;
        float distanceY = bY - aY;
        Normalise(distanceX, distanceY, out float directionX, out float directionY);
        
        // check for Nan in case the two circles are perfectly ontop of one another,
        // as normalising a distance of zero gives a NaN.
        cPX = float.IsNaN(directionX)? aX : aX + (directionX * aRadius);
        cPY = float.IsNaN(directionX)? aY : aY + (directionY * aRadius); 
    }




    /*******************
    
        Polygon Rectangle.
    
    ********************/




    /// <summary>
    /// Checks for intersection between two rectangles.
    /// </summary>
    /// <param name="lhs">The left-hand side rectangle.</param>
    /// <param name="rhs">The right-hand side rectangle.</param>
    /// <param name="lhsCentroid">the centroid of the left-hand side rectangle.</param>
    /// <param name="rhsCentroid">the centroid of the right-hand side rectangle.</param>
    /// <param name="normal">The normal of the intersection in relation to the right-hand side rectangle.</param>
    /// <param name="depth">The depth of the intersection in relation to the right-hand side rectangle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    /// <exception cref="ArgumentException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(
        in PolygonRectangle lhs, 
        in PolygonRectangle rhs, 
        Vector2 lhsCentroid, 
        Vector2 rhsCentroid, 
        out Vector2 normal, 
        out float depth
    )   
    {
        bool intersects = PolygonsIntersect(
            VerticesXAsSpan(lhs),
            VerticesYAsSpan(lhs),
            VerticesXAsSpan(rhs),
            VerticesYAsSpan(rhs),
            lhsCentroid.X, 
            lhsCentroid.Y, 
            rhsCentroid.X, 
            rhsCentroid.Y, 
            out float normalX, 
            out float normalY, 
            out depth
        );
        normal = new Vector2(normalX, normalY);
        return intersects;
    }




    /*******************
    
        Polygons.
    
    ********************/




    /// <summary>
    /// Checks for an intersection between two polygons.
    /// </summary>
    /// <param name="lhsX">the x-components of the left-hand side rectangle vertices.</param>
    /// <param name="lhsY">the y-components of the left-hand side rectangle vertices.</param>
    /// <param name="rhsX">the x-components of the right-hand side rectangle vertices.</param>
    /// <param name="rhsY">the y-components of the right-hand side rectangle vertices.</param>
    /// <param name="lhsCentroidX">the x-component of the left-hand side rectangle's centroid.</param>
    /// <param name="lhsCentroidY">the y-component of the left-hand side rectangle's centroid.</param>
    /// <param name="rhsCentroidX">the x-component of the right-hand side rectangle's centroid.</param>
    /// <param name="rhsCentroidY">the y-component of the right-hand side rectangle's centroid.</param>
    /// <param name="normalX">the x-component of the intersection normal in relation to the right-hand side rectangle.</param>
    /// <param name="normalY">the y-component of the intersection normal in relation to the right-hand side rectangle.</param>
    /// <param name="depth">The depth of the intersection in relation to the right-hand side rectangle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool PolygonsIntersect(
        Span<float> lhsX,
        Span<float> lhsY,
        Span<float> rhsX,
        Span<float> rhsY,
        float lhsCentroidX, 
        float lhsCentroidY, 
        float rhsCentroidX, 
        float rhsCentroidY, 
        out float normalX, 
        out float normalY,
        out float depth
    )
    {

        if(lhsX.Length != lhsY.Length)
        {
            throw new ArgumentException($"lhs vertices length do not match: '{lhsX.Length}' != '{lhsY.Length}'");
        }
        if(rhsX.Length != rhsY.Length)
        {
            throw new ArgumentException($"rhs vertices length do not match: '{rhsX.Length}' != '{rhsY.Length}'");
        }

        normalX = InitialNormalX;
        normalY = InitialNormalY;
        float foundNormalX;
        float foundNormalY;
        float foundDepth;
        depth = float.MaxValue;


        if (PolygonOneWayIntersect(lhsX, lhsY, rhsX, rhsY, out foundNormalX, out foundNormalY, out foundDepth))
        {            
            if(depth > foundDepth)
            {
                depth = foundDepth;
                normalX = foundNormalX; 
                normalY = foundNormalY;
            }
        }
        else
        {
            return false;
        }

        if (PolygonOneWayIntersect(rhsX, rhsY, lhsX, lhsY, out foundNormalX, out foundNormalY, out foundDepth))
        {            
            if(depth > foundDepth)
            {
                depth = foundDepth;
                normalX = foundNormalX; 
                normalY = foundNormalY;
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
        if(Dot(rhsCentroidX - lhsCentroidX, rhsCentroidY - lhsCentroidY, normalX, normalY) < 0)
        {
            normalX = -normalX;
            normalY = -normalY;
        }
        
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool PolygonOneWayIntersect(Span<float> polygonVerticesXA, Span<float> polygonVerticesYA, 
        Span<float> polygonVerticesXB, Span<float> polygonVerticesYB, 
        out float normalX, out float normalY, out float depth
    )
    {
        depth = float.MaxValue;
        normalX = 0;
        normalY = 0;

        float minMagSqrd = float.MaxValue;
        float minDepthSqrd = float.MaxValue;
        float minAxisDepth = float.MaxValue;
        float minAxisX = float.MaxValue;
        float minAxisY = float.MaxValue;

        float minEdgeA = float.MaxValue;
        float minEdgeB = float.MaxValue;
        float maxEdgeA = float.MinValue;
        float maxEdgeB = float.MinValue;

        for(int i = 0; i < polygonVerticesXA.Length; i++)
        {
            int vBIndex = (i + 1 == polygonVerticesXA.Length) ? 0 : i + 1;

            // edge.
            float axisX = -(polygonVerticesYA[vBIndex] - polygonVerticesYA[i]);
            float axisY = polygonVerticesXA[vBIndex] - polygonVerticesXA[i];

            // project using axis.
            ProjectPolygon_Sisd(polygonVerticesXA, polygonVerticesYA, axisX, axisY, polygonVerticesXA.Length, ref minEdgeA, ref maxEdgeA);        
            ProjectPolygon_Sisd(polygonVerticesXB, polygonVerticesYB, axisX, axisY, polygonVerticesXB.Length, ref minEdgeB, ref maxEdgeB);        


            if(minEdgeA >= maxEdgeB || minEdgeB >= maxEdgeA)
            {
                return false; // Separation found.
            }

            // Calculate overlap in "scaled space"
            float axisDepth = Min(maxEdgeB - minEdgeA, maxEdgeA - minEdgeB);

            // to compare depths correctly, the squared length of the axis is needed.
            float magSqrd = axisX * axisX + axisY * axisY;

            float axisDepthSqrd = axisDepth * axisDepth;

            // check if this is the minimum translation distance.
            if(minDepthSqrd * magSqrd > axisDepthSqrd)
            {
                minDepthSqrd = axisDepthSqrd / magSqrd; // store relative squared depth.
                minAxisX = axisX;
                minAxisY = axisY;
                minAxisDepth = axisDepth;
                minMagSqrd = magSqrd;
            }
        }

        float mag = MathF.Sqrt(minMagSqrd);
        depth = minAxisDepth / mag; // Only one sqrt if this is the new minimum translation distance.
        normalX = minAxisX / mag;
        normalY = minAxisY / mag;

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
    public static void ProjectPolygon_Sisd(Span<float> verticesX, Span<float> verticesY, float axisX, float axisY, 
        int vertexCount, ref float min, ref float max
    )
    {
        min = float.MaxValue;
        max = float.MinValue;

        for(int i = 0; i < vertexCount; i++)
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
    /// Projects a set of vertices onto a normalised axis.
    /// </summary>
    /// <param name="verticesX">the x-components of a polygons vertices.</param>
    /// <param name="verticesY">the y-components of a polygons vertices.</param>
    /// <param name="axisX">the x-component of the axis vector to project onto.</param>
    /// <param name="axisY">the y-component of the axis vector to project onto.</param>
    /// <param name="vertexCount">the count of vertices.</param>
    /// <param name="minEdge">a float to store the minimum found edge-value of the polygon.</param>
    /// <param name="maxEdge">a float to store the maximum found edge-vclue of the polygon.</param>
    public static void ProjectPolygon_Simd(Span<float> verticesX, Span<float> verticesY, float axisX, float axisY, int vertexCount, 
        ref float minEdge, ref float maxEdge
    )
    {
        int i = 0;
        int simSize = Vector128<float>.Count;
        
        ref float xRef = ref MemoryMarshal.GetReference(verticesX);
        ref float yRef = ref MemoryMarshal.GetReference(verticesY); 

        Vector128<float> vX;
        Vector128<float> vY;
        Vector128<float> vAxisX = Vector128.Create(axisX);
        Vector128<float> vAxisY = Vector128.Create(axisY);
        Vector128<float> minProjections = Vector128.Create(float.MaxValue);
        Vector128<float> maxProjections = Vector128.Create(float.MinValue);
        Vector128<float> projections;
        float projection = float.MaxValue;
        minEdge = float.MaxValue;
        maxEdge = float.MinValue;

        // body.
        for(; i <= vertexCount - simSize; i+= simSize)
        {
            vX = Vector128.LoadUnsafe(ref xRef, (uint)i);
            vY = Vector128.LoadUnsafe(ref yRef, (uint)i);
            projections = Dot(vX,vY, vAxisX, vAxisY);
            minProjections = Vector128.Min(projections, minProjections);
            maxProjections = Vector128.Max(projections, maxProjections);
        }
        
        // tail.
        for(; i < vertexCount; i++)
        {
            projection = Dot(verticesX[i], verticesY[i], axisX, axisY);

            if(projection < minEdge)
            {
                minEdge = projection;
            }
            if(projection > maxEdge)
            {
                maxEdge = projection;
            }
        }

        // final projection.
        for(int j = 0; j < simSize; j++)
        {
            if(minProjections[j] < minEdge)
            {
                minEdge = minProjections[j];
            }
            if(maxProjections[j] > maxEdge)
            {
                maxEdge = maxProjections[j];
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
        Span<float> polygonAVerticesX, 
        Span<float> polygonAVerticesY, 
        Span<float> polygonBVerticesX, 
        Span<float> polygonBVerticesY,
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




    /*******************
    
        Rectangle To Circle.
    
    ********************/



    /// <summary>
    /// Checks whether a rectangle and a circle intersect.
    /// </summary>
    /// <param name="rectangle">The rectangle data.</param>
    /// <param name="circle">The circle data.</param>
    /// <param name="normal">The normal of the intersection in relation to the circle.</param>
    /// <param name="depth">The depth of the intersection in relation to the circle.</param>
    /// <returns>true, if there was an intersection; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Intersect(in PolygonRectangle rectangle, in Circle circle, Vector2 polgonCenter, Vector2 circleCenter, out Vector2 normal, out float depth)
    {
        bool intersects = PolygonAndCircleIntersect(
            VerticesXAsSpan(rectangle), 
            VerticesYAsSpan(rectangle), 
            circle, 
            polgonCenter.X, 
            polgonCenter.Y, 
            circleCenter.X, 
            circleCenter.Y, 
            out float normalX, 
            out float normalY, 
            out depth
        );

        normal = new Vector2(normalX, normalY);

        return intersects;
    }




    /*******************
    
        Polygon To Circle.
    
    ********************/




    /// <summary>
    /// Checks whether a polygon and a circle intersect.
    /// </summary>
    /// <param name="polygonVerticesX">The x-components of a polygon's vertices.</param>
    /// <param name="polygonVerticesY">The y-components of a polygon's vertices.</param>
    /// <param name="circle">The circle data.</param>
    /// <param name="normalX">the x-component of the intersection normal in relation to the right-hand side rectangle.</param>
    /// <param name="normalY">the y-component of the intersection normal in relation to the right-hand side rectangle.</param>
    /// <param name="depth">The depth of the intersection in relation to the circle.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    /// <exception cref="ArgumentException">Throws when the passed in vertex-spans do not match in length.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool PolygonAndCircleIntersect
    (
        Span<float> polygonVerticesX,
        Span<float> polygonVerticesY,
        in Circle circle,
        float polygonCentroidX,
        float polygonCentroidY,
        float circleCenterX,
        float circleCenterY,
        out float normalX,
        out float normalY,
        out float depth
    )
    {
        return PolygonAndCircleIntersect(
            polygonVerticesX,
            polygonVerticesY,
            polygonCentroidX,
            polygonCentroidY,
            circle.X,
            circle.Y,
            circle.Radius,
            circleCenterX,
            circleCenterY,
            out normalX,
            out normalY,
            out depth
        );       
    }

    /// <summary>
    /// Checks whether a polygon and a circle intersect with eachother.
    /// </summary>
    /// <returns>true, if the two shapes are colliding; otherwise false.</returns>
    public static bool PolygonAndCircleIntersect(        
        Span<float> polygonVerticesX,
        Span<float> polygonVerticesY,
        float polygonCentroidX,
        float polygonCentroidY,
        float circleX,
        float circleY,
        float circleRadius,
        float circleCenterX,
        float circleCenterY,
        out float normalX,
        out float normalY,
        out float depth
    )
    {
        depth = float.MaxValue;

        // store normals as floats and operate on them as
        // floats before allocating a Vector as numerical
        // arithematic is faster.
        normalX = InitialNormalX;
        normalY = InitialNormalY;

        float axisX;
        float axisY;
        float axisDepth;
        float minA = float.MaxValue;
        float maxA = float.MaxValue;
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
            ProjectPolygon_Simd(polygonVerticesX, polygonVerticesY, axisX, axisY, polygonVerticesX.Length, ref minA, ref maxA);        
            ProjectCircle(circleX, circleY, circleRadius, axisX, axisY, out minB, out maxB);

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
                normalX = axisX;
                normalY = axisY;
            }
        }

        int closestPointIndex = FindClosestVertexOnPolygon(circleCenterX, circleCenterY, polygonVerticesX, polygonVerticesY);
        float closestPointX = polygonVerticesX[closestPointIndex];
        float closestPointY = polygonVerticesY[closestPointIndex];

        axisX = closestPointX - circleX;
        axisY = closestPointY - circleY;
        Normalise(axisX, axisY, out axisX, out axisY);

        // project all vertices onto the current edge to find the min and max values
        // of the two rectangles along the edge.
        ProjectPolygon_Simd(polygonVerticesX, polygonVerticesY, axisX, axisY, polygonVerticesX.Length, ref minA, ref maxA);        
        ProjectCircle(circleX, circleY, circleRadius, axisX, axisY, out minB, out maxB);
    
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
            normalX = axisX;
            normalY = axisY;
        }

        float distanceX = circleCenterX - polygonCentroidX;
        float distanceY = circleCenterY - polygonCentroidY;

        // when a new smaller   
        // depth is found but in relation to rect B, not A.
        // this is so that the resolution code will always push A out of B
        // and not push the two into each other when a smaller depth is found when 
        // looping through rect B.
        if(Dot(distanceX, distanceY, normalX,  normalY) < 0)
        {
            normalX = -normalX;
            normalY = -normalY;
        }

        return true;
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
        Span<float> polygonVerticesX, 
        Span<float> polygonVerticesY, 
        in Circle circle, 
        out Vector2 contactPoint
    )
    {
        FindContactPoints(
            polygonVerticesX, 
            polygonVerticesY, 
            circle.X, 
            circle.Y, 
            out float contactPointX,
            out float contactPointY
        );

        contactPoint = new(contactPointX, contactPointY);
    }

    /// <summary>
    /// Finds the contact point between an intersecting polygon and circle.
    /// </summary>
    /// <param name="polygonVerticesX">the x-values of the polygon's vertices </param>
    /// <param name="polygonVerticesY">the y-values of the polygon's vertices.</param>
    /// <param name="circle">the circle.</param>
    /// <param name="contactPointX">the x-value of the calculated contact point vector.</param>
    /// <param name="contactPointY">the y-value of the calculated contact point vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void FindContactPoints(
        Span<float> polygonVerticesX, 
        Span<float> polygonVerticesY,
        in Circle circle, 
        out float contactPointX, 
        out float contactPointY
    )
    {
        FindContactPoints(
            polygonVerticesX, 
            polygonVerticesY, 
            circle.X, 
            circle.Y, 
            out contactPointX,
            out contactPointY
        );
    }

    /// <summary>
    /// Finds the contact points between a polygon and a circle.
    /// </summary>
    /// <param name="polygonVerticesX">the x-values of the polygon's vertices.</param>
    /// <param name="polygonVerticesY">the y-values of the polygon's vertices.</param>
    /// <param name="circleX">the x-value of the circle's position.</param>
    /// <param name="circleY">the y-value of the circle's position.</param>
    /// <param name="contactPointX">the x-value of the contact point.</param>
    /// <param name="contactPointY">the y-value of the contact point.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void FindContactPoints(
        Span<float> polygonVerticesX, 
        Span<float> polygonVerticesY, 
        float circleX, 
        float circleY, 
        out float contactPointX,
        out float contactPointY
    )
    {
        if(polygonVerticesX.Length != polygonVerticesY.Length)
            throw new ArgumentException($"polygonVerticesX length '{polygonVerticesX.Length}' is not equal to polygonVerticesY length '{polygonVerticesY.Length}'");

        contactPointX = float.MaxValue;    
        contactPointY = float.MaxValue;
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
            ClosestPoint(
                polygonVerticesX[startIndex], 
                polygonVerticesY[startIndex],
                polygonVerticesX[endIndex], 
                polygonVerticesY[endIndex],
                circleX,
                circleY,
                out float closestPointX,
                out float closestPointY,
                out float distSqrd
            );


            if(distSqrd < minDistSqrd)
            {
                minDistSqrd = distSqrd;
                contactPointX = closestPointX;
                contactPointY = closestPointY;
            }
        } 
    }
}