using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Howl.DataStructures;
using Howl.Graphics;

namespace Howl.Physics;

public sealed class CollisionSystemState : IDisposable
{
    /// <summary>
    /// Gets the bounding volume hierarchy for a collision system.
    /// </summary>
    public BoundingVolumeHierarchy Bvh;

    /// <summary>
    /// Gets the debug stopwatch for timing a collision system intersect step.
    /// </summary>
    public readonly Stopwatch IntersectionStopwatch;

    /// <summary>
    /// Gets the debug stopwatch for timing a collision system resolution step.
    /// </summary>
    public readonly Stopwatch ResolutionStopwatch;

    /// <summary>
    /// Gets the debug stop watch for timing a bvh reconstruction step.
    /// </summary>
    public readonly Stopwatch BvhReconstructionStopwatch;

    /// <summary>
    /// Gets the debug stopwatch for timing a collision manifold sort step.
    /// </summary>
    public readonly Stopwatch CollisionManifoldSortStopwatch;

    /// <summary>
    /// Gets the collision manifold.
    /// </summary>
    public readonly CollisionManifold CollisionManifold;

    /// <summary>
    /// Gets and sets the debug draw colour for the solid-colliders.
    /// </summary>
    public Colour SolidColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for the trigger-colliders.
    /// </summary>
    public Colour TriggerColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for kinematic-colliders.
    /// </summary>
    public Colour KinematicColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for trigger colliders when triggered.
    /// </summary>
    public Colour TriggerColliderTriggeredColour;

    /// <summary>
    /// Gets and sets the debug draw colour for AABB's.
    /// </summary>
    public Colour AABBColour;

    /// <summary>
    /// Gets and sets the fallback debug draw colour for colliders.
    /// </summary>
    public Colour FallbackColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for inactive colliders.
    /// </summary>
    public Colour InactiveColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for bvh-tree leaf aabb's.
    /// </summary>
    public Colour BvhLeafAABBColour;

    /// <summary>
    /// Gets and sets the debug draw colour for bvh-treee branch aabb's
    /// </summary>
    public Colour BvhBranchAABBColour;

    /// <summary>
    /// Gets and sets the debug draw colour for contact-points;
    /// </summary>
    public Colour ContactPointColour;

    /// <summary>
    /// Gets and sets whether or not to draw collider wireframes.
    /// </summary>
    public bool DrawColliderWireframes;

    /// <summary>
    /// Gets and sets whether or not to draw collider AABB wireframes.
    /// </summary>
    public bool DrawAABBWireframes;

    /// <summary>
    /// Gets and sets whether or not to draw bvh branches.
    /// </summary>
    public bool DrawBvhBranches;

    /// <summary>
    /// Gets and sets whether or not to draw contact points.
    /// </summary>
    public bool DrawContactPoints;

    /// <summary>
    /// Gets and sets whether or not this instance has been disposed.
    /// </summary>
    private bool disposed;

    /// <summary>
    /// Gets whether or not this instance has been disposed.
    /// </summary>
    public bool IsDisposed => disposed;

    public CollisionSystemState()
    {
        IntersectionStopwatch           = new Stopwatch();
        ResolutionStopwatch             = new Stopwatch();
        BvhReconstructionStopwatch      = new Stopwatch();
        CollisionManifoldSortStopwatch  = new Stopwatch();
        CollisionManifold               = new();
        
        SolidColliderColour             = Colour.Green;
        KinematicColliderColour         = Colour.Orange;
        TriggerColliderColour           = Colour.LightBlue;
        TriggerColliderTriggeredColour  = Colour.Red;
        AABBColour                      = new Colour(Colour.Pink.R, Colour.Pink.G, Colour.Pink.B, 50);
        FallbackColliderColour          = Colour.White;
        InactiveColliderColour          = Colour.Black;
        BvhLeafAABBColour               = Colour.Purple;
        BvhBranchAABBColour             = Colour.Yellow;
        ContactPointColour              = Colour.Red;

        DrawColliderWireframes  = false;
        DrawAABBWireframes      = false;
        DrawBvhBranches         = false;
        DrawContactPoints       = false;

        Bvh = new();
    }

    /// <summary>
    /// Throws an exception if this instance is disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException($"{nameof(CollisionSystemState)}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            CollisionManifold.Clear();
            Bvh.Dispose();
        }

        disposed = true;
    }

    ~CollisionSystemState()
    {
        Dispose(false);
    }

    public Colour GetColliderColour(ColliderParameters colliderParameters)
    {
        return colliderParameters.Mode switch{
            ColliderMode.Solid => SolidColliderColour,
            ColliderMode.Trigger => TriggerColliderColour,
            ColliderMode.Kinematic => KinematicColliderColour, 
            _ => FallbackColliderColour
        };
    }
}