using System;
using System.Collections.Generic;
using System.Diagnostics;
using Howl.Graphics;

namespace Howl.Physics;

public sealed class CollisionSystemState : IDisposable
{
    private const int CollisionManifoldInitialCapacity = 4096;

    /// <summary>
    /// Gets and sets the debug stopwatch for timing a collision system intersect step.
    /// </summary>
    public Stopwatch IntersectStep;

    /// <summary>
    /// Gets and sets the debug stopewatch for timing a collision system resolution step.
    /// </summary>
    public Stopwatch ResolutionStep;

    /// <summary>
    /// Gets and sets the collision manifold for.
    /// </summary>
    public List<Collision> CollisionManifold;

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
    /// Gets and sets the fallback debug draw colour for colliders.
    /// </summary>
    public Colour FallbackColliderColour;

    /// <summary>
    /// Gets and sets the debug draw colour for inactive colliders.
    /// </summary>
    public Colour InactiveColliderColour;

    private bool disposed;
    public bool IsDisposed => disposed;

    public CollisionSystemState()
    {
        IntersectStep = new Stopwatch();
        ResolutionStep = new Stopwatch();
        CollisionManifold = new(CollisionManifoldInitialCapacity);
        SolidColliderColour             = Colour.Green;
        KinematicColliderColour         = Colour.Orange;
        TriggerColliderColour           = Colour.LightBlue;
        TriggerColliderTriggeredColour  = Colour.Red;
        FallbackColliderColour          = Colour.White;
        InactiveColliderColour          = Colour.Black;    
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
            IntersectStep = null;
            ResolutionStep = null;
            CollisionManifold.Clear();
            CollisionManifold = null;
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