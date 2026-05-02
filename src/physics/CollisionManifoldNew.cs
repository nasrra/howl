using System;
using System.Runtime.CompilerServices;
using Howl.Math;

namespace Howl.Physics;

public static class CollisionManifoldNew
{




    /******************
    
        One-way appending.
    
    *******************/




    /// <summary>
    ///     Appends a one-way collision with two contact points.
    /// </summary>
    /// <remarks>
    ///     Remarks:
    ///     <list type = "bullet">
    ///         <item>Bypasses out-of-bounds stride checks for the fixed size array.</item>
    ///         <item>Should only be used if the <c><paramref name="appendIndex"/></c> of the collision data is known.</item>    
    ///     </list>
    /// </remarks>
    /// <param name="state">the state instance to append to.</param>
    /// <param name="recipientIndex">the physics body index of the <c>recipient</c> collider.</param>
    /// <param name="appendIndex">the entry's element index in the state instance to insert to the collision data to.</param>
    /// <param name="colliderIndex">the physics body index of the of the <c>colliding</c> collider.</param>
    /// <param name="colliderCentroidX">the x-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="colliderCentroidY">the y-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
    /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
    /// <param name="contactPointX">the x-component of the contact point.</param>
    /// <param name="contactPointY">the y-component of the contact point.</param>
    /// <param name="depth">the depth of the collision.</param>
    /// <param name="colliderFlags">the physics body flags of the <c>colliding</c> collider.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendOneWayUnsafe(CollisionManifoldStateNew state, int recipientIndex, int appendIndex, int colliderIndex, 
        float colliderCentroidX, float colliderCentroidY, float normalX, float normalY, float contactPointX, float contactPointY, float depth, 
        PhysicsBodyFlags colliderFlags
    )
    {
        // update context.
        state.AppendCounts[recipientIndex]++;
        StackArray.Push(state.Active, appendIndex);

        // write data.
        state.ColliderIndices[appendIndex]        = colliderIndex;
        state.Normals.X[appendIndex]              = normalX;
        state.Normals.Y[appendIndex]              = normalY;
        state.ColliderCentroids.X[appendIndex]    = colliderCentroidX;
        state.ColliderCentroids.Y[appendIndex]    = colliderCentroidY;
        state.FirstContactPoints.X[appendIndex]   = contactPointX;
        state.FirstContactPoints.Y[appendIndex]   = contactPointY;
        state.Depths[appendIndex]                 = depth;
        state.ColliderFlags[appendIndex]          = colliderFlags;
        state.TwoContactPoints[appendIndex]       = false;
    }

    /// <summary>
    ///     Appends a one way collision with a single contact point.
    /// </summary>
    /// <param name="state">the state instance to append to.</param>
    /// <param name="recipientIndex">the physics body index of the <c>recipient</c> collider.</param>
    /// <param name="colliderIndex">the physics body index of the of the <c>colliding</c> collider.</param>
    /// <param name="colliderCentroidX">the x-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="colliderCentroidY">the y-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
    /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
    /// <param name="contactPointX">the x-component of the contact point.</param>
    /// <param name="contactPointY">the y-component of the contact point.</param>
    /// <param name="depth">the depth of the collision.</param>
    /// <param name="colliderFlags">the physics body flags of the <c>colliding</c> collider.</param>
    /// <returns>true, if the collision was successfully appended; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool AppendOneWay(CollisionManifoldStateNew state, int recipientIndex, int colliderIndex, float colliderCentroidX, 
        float colliderCentroidY, float normalX, float normalY, float contactPointX,float contactPointY, float depth, PhysicsBodyFlags colliderFlags
    )
    {
        bool isValid = false;
        int appendIndex = FixedStrideArray.GetAppendIndex(state.AppendCounts, recipientIndex, state.Stride, ref isValid);

        if(isValid == false)
        {
            return false;
        }

        AppendOneWayUnsafe(state, recipientIndex, appendIndex, colliderIndex, colliderCentroidX, colliderCentroidY, normalX, normalY, 
            contactPointX, contactPointY, depth, colliderFlags
        );

        return true;
    }

    /// <summary>
    ///     Appends a one-way collision with two contact points.
    /// </summary>
    /// <remarks>
    ///     Remarks:
    ///     <list type = "bullet">
    ///         <item>Bypasses out-of-bounds stride checks for the fixed size array.</item>
    ///         <item>Should only be used if the <c><paramref name="appendIndex"/></c> of the collision data is known.</item>    
    ///     </list>
    /// </remarks>
    /// <param name="state">the state instance to append to.</param>
    /// <param name="recipientIndex">the physics body index of the <c>recipient</c> collider.</param>
    /// <param name="appendIndex">the entry's element index in the state instance to insert to the collision data to.</param>
    /// <param name="colliderIndex">the physics body index of the of the <c>colliding</c> collider.</param>
    /// <param name="colliderCentroidX">the x-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="colliderCentroidY">the y-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
    /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
    /// <param name="firstContactPointX">the x-component of the first contact point.</param>
    /// <param name="firstContactPointY">the y-component of the first contact point.</param>
    /// <param name="secondContactPointX">the x-component of the second contact point.</param>
    /// <param name="secondContactPointY">the y-component of the second contact point.</param>
    /// <param name="depth">the depth of the collision.</param>
    /// <param name="colliderFlags">the physics body flags of the <c>colliding</c> collider.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendOneWayUnsafe(CollisionManifoldStateNew state, int recipientIndex, int appendIndex, int colliderIndex, 
        float colliderCentroidX, float colliderCentroidY, float normalX, float normalY, float firstContactPointX, float firstContactPointY, 
        float secondContactPointX, float secondContactPointY, float depth, PhysicsBodyFlags colliderFlags)
    {
        // update context.
        state.AppendCounts[recipientIndex]++;
        StackArray.Push(state.Active, appendIndex);

        // write data.
        state.ColliderIndices[appendIndex]        = colliderIndex;
        state.Normals.X[appendIndex]              = normalX;
        state.Normals.Y[appendIndex]              = normalY;
        state.ColliderCentroids.X[appendIndex]    = colliderCentroidX;
        state.ColliderCentroids.Y[appendIndex]    = colliderCentroidY;
        state.FirstContactPoints.X[appendIndex]   = firstContactPointX;
        state.FirstContactPoints.Y[appendIndex]   = firstContactPointY;
        state.SecondContactPoints.X[appendIndex]  = secondContactPointX;
        state.SecondContactPoints.Y[appendIndex]  = secondContactPointY;
        state.Depths[appendIndex]                 = depth;
        state.ColliderFlags[appendIndex]          = colliderFlags;
        state.TwoContactPoints[appendIndex]       = true;
    }

    /// <summary>
    ///     Appends a one way collision with a two contact point.
    /// </summary>
    /// <param name="state">the state instance to append to.</param>
    /// <param name="recipientIndex">the physics body index of the <c>recipient</c> collider.</param>
    /// <param name="colliderIndex">the physics body index of the of the <c>colliding</c> collider.</param>
    /// <param name="colliderCentroidX">the x-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="colliderCentroidY">the y-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
    /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
    /// <param name="firstContactPointX">the x-component of the first contact point.</param>
    /// <param name="firstContactPointY">the y-component of the first contact point.</param>
    /// <param name="secondContactPointX">the x-component of the second contact point.</param>
    /// <param name="secondContactPointY">the y-component of the second contact point.</param>
    /// <param name="depth">the depth of the collision.</param>
    /// <param name="colliderFlags">the physics body flags of the <c>colliding</c> collider.</param>
    /// <returns>true, if the collision was successfully appended; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool AppendOneWay(CollisionManifoldStateNew state, int recipientIndex, int colliderIndex, float colliderCentroidX, float colliderCentroidY, 
        float normalX, float normalY, float firstContactPointX, float firstContactPointY, float secondContactPointX, float secondContactPointY, 
        float depth, PhysicsBodyFlags colliderFlags
    )
    {
        bool isValid = false;
        int appendIndex = FixedStrideArray.GetAppendIndex(state.AppendCounts, recipientIndex, state.Stride, ref isValid);

        if(isValid == false)
        {
            return false;
        }

        AppendOneWayUnsafe(state, recipientIndex, appendIndex, colliderIndex, colliderCentroidX, colliderCentroidY, normalX, normalY, 
            firstContactPointX, firstContactPointY, secondContactPointX, secondContactPointY, depth, colliderFlags
        );

        return true;
    }




    /******************
    
        Two-Way appending.
    
    *******************/



    /// <summary>
    ///     Appends a two-way collision between colliders.
    /// </summary>
    /// <param name="state">the state instance to append to.</param>
    /// <param name="indexA">the index of collider A.</param>
    /// <param name="indexB">the index of collider B.</param>
    /// <param name="centroidXA">the x-component of collider A's centroid.</param>
    /// <param name="centroidYA">the y-component of collider A's centroid.</param>
    /// <param name="centroidXB">the x-component of collider B's centroid.</param>
    /// <param name="centroidYB">the y-component of collider B's centroid.</param>
    /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
    /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
    /// <param name="contactPointX">the x-component of the contact point.</param>
    /// <param name="contactPointY">the y-component of the contact point.</param>
    /// <param name="depth">the depth of the collision.</param>
    /// <param name="flagsA">the physics body flags of collider A.</param>
    /// <param name="flagsB">the physics body flags of collider B.</param>
    /// <returns>true, if the collision was successfully appended; otherwise false.</returns>
    public static bool AppendTwoWay(CollisionManifoldStateNew state, int indexA, int indexB, float centroidXA, float centroidYA, 
        float centroidXB, float centroidYB, float normalX, float normalY, float contactPointX, float contactPointY, 
        float depth, PhysicsBodyFlags flagsA, PhysicsBodyFlags flagsB
    )
    {

        // == validate that there is space to append the collision ==.

        bool isValid = false;
        
        int appendIndexA = FixedStrideArray.GetAppendIndex(state.AppendCounts, indexA, state.Stride, ref isValid);
        
        if(isValid == false)
        {
            return false;
        }

        int appendIndexB = FixedStrideArray.GetAppendIndex(state.AppendCounts, indexB, state.Stride, ref isValid);

        if(isValid == false)
        {
            return false;
        }

        // == write the collision data ==.

        AppendOneWayUnsafe(state, indexA, appendIndexA, indexB, centroidXB, centroidYB, 
            normalX, normalY, contactPointX, contactPointY, depth, flagsB
        );

        // note: the normal reversing.
        AppendOneWayUnsafe(state, indexB, appendIndexB, indexA, centroidXA, centroidYA, 
            -normalX, -normalY, contactPointX, contactPointY, depth, flagsA
        );

        return true;
    }

    /// <summary>
    ///     Appends a two-way collision between colliders.
    /// </summary>
    /// <param name="state">the state instance to append to.</param>
    /// <param name="indexA">the index of collider A.</param>
    /// <param name="indexB">the index of collider B.</param>
    /// <param name="centroidXA">the x-component of collider A's centroid.</param>
    /// <param name="centroidYA">the y-component of collider A's centroid.</param>
    /// <param name="centroidXB">the x-component of collider B's centroid.</param>
    /// <param name="centroidYB">the y-component of collider B's centroid.</param>
    /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
    /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
    /// <param name="firstContactPointX">the x-component of the first contact point.</param>
    /// <param name="firstContactPointY">the y-component of the first contact point.</param>
    /// <param name="secondContactPointX">the x-component of the first contact point.</param>
    /// <param name="secondContactPointY">the y-component of the first contact point.</param>
    /// <param name="depth">the depth of the collision.</param>
    /// <param name="flagsA">the physics body flags of collider A.</param>
    /// <param name="flagsB">the physics body flags of collider B.</param>
    /// <returns>true, if the collision was successfully appended; otherwise false.</returns>
    public static bool AppendTwoWay(CollisionManifoldStateNew state, int indexA, int indexB, float centroidXA, float centroidYA, 
        float centroidXB, float centroidYB, float normalX, float normalY, float firstContactPointX, float firstContactPointY, 
        float secondContactPointX, float secondContactPointY, float depth, PhysicsBodyFlags flagsA, PhysicsBodyFlags flagsB
    )
    {
        // == validate that there is space to append the collision ==.

        bool isValid = false;
        
        int appendIndexA = FixedStrideArray.GetAppendIndex(state.AppendCounts, indexA, state.Stride, ref isValid);
        
        if(isValid == false)
        {
            return false;
        }

        int appendIndexB = FixedStrideArray.GetAppendIndex(state.AppendCounts, indexB, state.Stride, ref isValid);

        if(isValid == false)
        {
            return false;
        }

        // == write the collision data ==.

        AppendOneWayUnsafe(state, indexA, appendIndexA, indexB, centroidXB, centroidYB, normalX, normalY, firstContactPointX, firstContactPointY, 
            secondContactPointX, secondContactPointY, depth, flagsB
        );

        // note: the normal reversing.
        AppendOneWayUnsafe(state, indexB, appendIndexB, indexA, centroidXA, centroidYA, -normalX, -normalY, firstContactPointX, firstContactPointY, 
            secondContactPointX, secondContactPointY, depth, flagsA
        );

        return true;
    }




    /******************
    
        Disposal.
    
    *******************/




    /// <summary>
    ///     Sets the <c>AppendCounts</c> and <c>Active</c> array counts to zero.
    /// </summary>
    /// <param name="state">the state instance to clear.</param>
    public static void Clear(CollisionManifoldStateNew state)
    {
        int[] appendCounts = state.AppendCounts;

        for(int i = 0; i < appendCounts.Length; i++)
        {
            appendCounts[i] = 0;
        }

        StackArray.ClearCount(state.Active);
    }

    /// <summary>
    ///     Disposes of a state instance.
    /// </summary>
    /// <param name="state">the state instance to dispose of.</param>
    public static void Dispose(CollisionManifoldStateNew state)
    {
        if (state.Disposed)
        {
            return;
        }

        state.Disposed = true;

        state.ColliderIndices = null;
        
        Soa_Vector2.Dispose(state.Normals);
        state.Normals = null;

        Soa_Vector2.Dispose(state.ColliderCentroids);
        state.ColliderCentroids = null;

        Soa_Vector2.Dispose(state.FirstContactPoints);
        state.FirstContactPoints = null;

        Soa_Vector2.Dispose(state.SecondContactPoints);
        state.SecondContactPoints = null;

        state.Depths = null;

        state.ColliderFlags = null;

        state.TwoContactPoints = null;

        state.AppendCounts = null;

        StackArray.Dispose(state.Active);
        state.Active = null;

        state.Stride = 0;

        state.MaxEntries = 0;

        GC.SuppressFinalize(state);
    }
}