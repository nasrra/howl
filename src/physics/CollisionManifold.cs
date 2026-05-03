using System;
using System.Runtime.CompilerServices;
using Howl.Collections;
using Howl.Math;

namespace Howl.Physics;

public static class CollisionManifold
{




    /******************
    
        Setters.
    
    *******************/




    /// <summary>
    ///     Sets a one-way collision data entry at a given index.
    /// </summary>
    /// <param name="state">the state instance to set.</param>
    /// <param name="recipientIndex">the index in the state instance arrays to write to.</param>
    /// <param name="colliderIndex">the physics body index of the of the <c>colliding</c> collider.</param>
    /// <param name="colliderCentroidX">the x-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="colliderCentroidY">the y-component of the <c>colliding</c> collider's centroid.</param>
    /// <param name="normalX">the x-component of the normal vector in relation to collider A to B.</param>
    /// <param name="normalY">the y-component of the normal vector in relation to collider A to B.</param>
    /// <param name="contactPointX">the x-component of the contact point.</param>
    /// <param name="contactPointY">the y-component of the contact point.</param>
    /// <param name="depth">the depth of the collision.</param>
    /// <param name="colliderFlags">the physics body flags of the <c>colliding</c> collider.</param>
    /// <returns>the collision index that the data was written to.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int SetDataOneWay(CollisionManifoldState state, int recipientIndex, int colliderIndex, 
        float colliderCentroidX, float colliderCentroidY, float normalX, float normalY, float contactPointX, float contactPointY, float depth, 
        PhysicsBodyFlags colliderFlags
    )
    {
        int elementIndex = FixedStrideArray.GetElementIndex(recipientIndex, state.Stride, colliderIndex);

        ref int phase = ref state.ActivePhase[elementIndex];
        if(phase <= 0)
        {
            FixedStrideSwapBackArray.Append(elementIndex, state.ActiveIndices, state.ActiveIndicesCount, 
                state.Stride, recipientIndex
            );
        }
        phase = 1;

        // write data.
        state.Normals.X[elementIndex]              = normalX;
        state.Normals.Y[elementIndex]              = normalY;
        state.ColliderCentroids.X[elementIndex]    = colliderCentroidX;
        state.ColliderCentroids.Y[elementIndex]    = colliderCentroidY;
        state.FirstContactPoints.X[elementIndex]   = contactPointX;
        state.FirstContactPoints.Y[elementIndex]   = contactPointY;
        state.Depths[elementIndex]                 = depth;
        state.ColliderFlags[elementIndex]          = colliderFlags;
        state.TwoContactPoints[elementIndex]       = false;

        return elementIndex;
    }

    /// <summary>
    ///     Sets a one-way collision data entry at a given index.
    /// </summary>
    /// <param name="state">the state instance to set.</param>
    /// <param name="recipientIndex">the index in the state instance arrays to write to.</param>
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
    /// <returns>the collision index the data was written to.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int SetDataOneWay(CollisionManifoldState state, int recipientIndex, int colliderIndex, 
        float colliderCentroidX, float colliderCentroidY, float normalX, float normalY, float firstContactPointX, float firstContactPointY, 
        float secondContactPointX, float secondContactPointY, float depth, PhysicsBodyFlags colliderFlags
    )
    {
        int elementIndex = FixedStrideArray.GetElementIndex(recipientIndex, state.Stride, colliderIndex);

        ref int phase = ref state.ActivePhase[elementIndex];
        if(phase <= 0)
        {
            FixedStrideSwapBackArray.Append(elementIndex, state.ActiveIndices, state.ActiveIndicesCount, 
                state.Stride, recipientIndex
            );
        }
        phase = 1;

        state.Normals.X[elementIndex]              = normalX;
        state.Normals.Y[elementIndex]              = normalY;
        state.ColliderCentroids.X[elementIndex]    = colliderCentroidX;
        state.ColliderCentroids.Y[elementIndex]    = colliderCentroidY;
        state.FirstContactPoints.X[elementIndex]   = firstContactPointX;
        state.FirstContactPoints.Y[elementIndex]   = firstContactPointY;
        state.SecondContactPoints.X[elementIndex]  = secondContactPointX;
        state.SecondContactPoints.Y[elementIndex]  = secondContactPointY;
        state.Depths[elementIndex]                 = depth;
        state.ColliderFlags[elementIndex]          = colliderFlags;
        state.TwoContactPoints[elementIndex]       = true;

        return elementIndex;
    }

    /// <summary>
    ///     Sets a two-way collision data entry at a given entry.
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
    /// <param name="secondContactPointX">the x-component of the second contact point.</param>
    /// <param name="secondContactPointY">the y-component of the second contact point.</param>
    /// <param name="depth">the depth of the collision.</param>
    /// <param name="flagsA">the physics body flags of collider A.</param>
    /// <param name="flagsB">the physics body flags of collider B.</param>
    /// <returns>
    ///     A tuple of the collision indices the data was written to.
    ///     <list type = "bullet">
    ///         <item> Item1 is the collision index that <c><paramref name="indexA"/></c> wrote to. </item>    
    ///         <item> Item2 is the collision index that <c><paramref name="indexB"/></c> wrote to. </item>    
    ///     </list>
    /// </returns>
    public static (int collisionIndexA, int collisionIndexB) SetDataTwoWay(CollisionManifoldState state, int indexA, int indexB, float centroidXA, float centroidYA, 
        float centroidXB, float centroidYB, float normalX, float normalY, float firstContactPointX, float firstContactPointY, 
        float secondContactPointX, float secondContactPointY, float depth, PhysicsBodyFlags flagsA, PhysicsBodyFlags flagsB
    )
    {
        int a = SetDataOneWay(state, indexA, indexB, centroidXB, centroidYB, normalX, normalY, firstContactPointX, firstContactPointY, 
            secondContactPointX, secondContactPointY, depth, flagsB
        );

        // note: the normal reversing.
        int b = SetDataOneWay(state, indexB, indexA, centroidXA, centroidYA, -normalX, -normalY, firstContactPointX, firstContactPointY, 
            secondContactPointX, secondContactPointY, depth, flagsA
        );

        return(a,b);  
    }

    /// <summary>
    ///     Sets a two-way collision data entry at a given entry.
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
    /// <returns>
    ///     A tuple of the collision indices the data was written to.
    ///     <list type = "bullet">
    ///         <item> Item1 is the collision index that <c><paramref name="indexA"/></c> wrote to. </item>    
    ///         <item> Item2 is the collision index that <c><paramref name="indexB"/></c> wrote to. </item>  
    ///     </list>
    /// </returns>
    public static (int collisionIndexA, int collisionIndexB) SetDataTwoWay(CollisionManifoldState state, int indexA, int indexB, float centroidXA, float centroidYA, 
        float centroidXB, float centroidYB, float normalX, float normalY, float contactPointX, float contactPointY, 
        float depth, PhysicsBodyFlags flagsA, PhysicsBodyFlags flagsB
    )
    {
        int a = SetDataOneWay(state, indexA, indexB, centroidXB, centroidYB, normalX, normalY, contactPointX, contactPointY, depth, flagsB);

        // note: the normal reversing.
        int b = SetDataOneWay(state, indexB, indexA, centroidXA, centroidYA, -normalX, -normalY, contactPointX, contactPointY, depth, flagsA);
        
        return (a,b);
    }




    /******************
    
        State Handling.
    
    *******************/




    /// <summary>
    ///     Swaps the previous and current contact state context pointers.
    /// </summary>
    /// <param name="state">the state instance to swap.</param>
    public static void SwapContactStateContexts(CollisionManifoldState state)
    {
        ContactState[] tempContactStates = state.PreviousContactStates;
        state.PreviousContactStates = state.ContactStates;
        state.ContactStates = tempContactStates;
    }

    /// <summary>
    ///     Prepares a state instance for the next step.
    /// </summary>
    /// <parsam name="state">the state instance to prepare.</param>
    public static void PrepareForNextStep(CollisionManifoldState state)
    {
        SwapContactStateContexts(state);
    }

    /// <summary>
    ///     Completes the update step for a state instance.
    /// </summary>
    /// <param name="state">the state instance to complete the step for.</param>
    public static void CompleteStep(CollisionManifoldState state)
    {        
        Span<ContactState> contactStates = state.ContactStates;
        Span<ContactState> previousContactStates = state.PreviousContactStates;
        int[] activeIndicesCounts = state.ActiveIndicesCount;
        int[] activeIndices = state.ActiveIndices;
        int[] active = state.ActivePhase;
        int stride = state.Stride;
        int maxEntries = state.MaxEntries;

        // update active counts.
        for(int entryIndex = 0; entryIndex < maxEntries; entryIndex++)
        {
            int swapBackCount = activeIndicesCounts[entryIndex];
            if (swapBackCount == 0)
            {
                continue;
            }

            for(int entryElementIndex = 0; entryElementIndex < swapBackCount; entryElementIndex++)
            {
                // get the active phase of the collision.

                int elementIndex = FixedStrideArray.GetElementIndex(entryIndex, stride, entryElementIndex);;
                int collisionIndex = activeIndices[elementIndex];
                ref int phase = ref active[collisionIndex];

                // update the collision state.
                ref ContactState previousState = ref previousContactStates[collisionIndex];
                ref ContactState currentState = ref contactStates[collisionIndex];

                switch (phase)
                {
                    case 1:
                        // the collider has began contacting the 
                        switch (previousState)
                        {
                            case ContactState.Enter:
                                currentState = ContactState.Sustain;
                            break;
                            case ContactState.Exit:
                                currentState = ContactState.Enter;
                            break;
                            case ContactState.None:
                                currentState = ContactState.Enter;
                            break;
                            case ContactState.Sustain:
                                currentState = ContactState.Sustain;
                            break;
                            default:
                            break;
                        }
                    break;
                    case 2:
                        currentState = ContactState.Exit;
                    break;
                    case 3:
                        currentState = ContactState.None;
                    break;
                    default:
                    break;
                }

                // update the active phase of the collision.
                
                phase++;
                phase%=4;
                if (phase == 0)
                {
                    FixedStrideSwapBackArray.RemoveAt(activeIndices, activeIndicesCounts, stride, entryIndex, entryElementIndex);
                }
            }
        }
    }




    /******************
    
        Disposal.
    
    *******************/




    /// <summary>
    ///     Disposes of a state instance.
    /// </summary>
    /// <param name="state">the state instance to dispose of.</param>
    public static void Dispose(CollisionManifoldState state)
    {
        if (state.Disposed)
        {
            return;
        }

        state.Disposed = true;
        
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

        state.ActiveIndices = null;

        state.ActiveIndicesCount = null;

        state.ActivePhase = null;

        state.ContactStates = null;

        state.PreviousContactStates = null;

        state.Stride = 0;

        state.MaxEntries = 0;

        GC.SuppressFinalize(state);
    }
}