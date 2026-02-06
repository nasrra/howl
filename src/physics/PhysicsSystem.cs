using System.Reflection.PortableExecutable;
using Howl.ECS;
using Howl.Graphics;

namespace Howl.Physics;

public static class PhysicsSystem
{
    /// <summary>
    /// Registers all necessary components for this system.
    /// </summary>
    /// <param name="componentRegistry">The component registry to register to.</param>
    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.ThrowIfDisposed();

        CollisionSystem.RegisterComponents(componentRegistry);
        RigidBodySystem.RegisterComponents(componentRegistry);
    }

    /// <summary>
    /// Creates a new FixedUpdateSystem instance.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static FixedUpdateSystem FixedUpdateSystem(ComponentRegistry componentRegistry, PhysicsSystemState state, int subSteps)
    => deltaTime =>
    {
        FixedUpdateStep(componentRegistry, state, deltaTime, subSteps);
    };

    /// <summary>
    /// FixedUpdate step for the Physics System.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <param name="deltaTime"></param>
    public static void FixedUpdateStep(ComponentRegistry componentRegistry, PhysicsSystemState state, float deltaTime, int subSteps)
    {
        state.FixedUpdateStepStopwatch.Restart();

        CollisionSystemState collisionSystemState = state.CollisionSystemState;
        RigidbodySystemState rigidbodySystemState = state.RigidbodySystemState;
        
        deltaTime /= (float)subSteps;

        for(int i = 0; i < subSteps; i++)
        {   
            state.FixedUpdateSubStepStopwatch.Restart();

                RigidBodySystem.MovementStep(componentRegistry, state.RigidbodySystemState, deltaTime);
                
                if(collisionSystemState.CollisionManifold.Collisions.Count > 0)
                {
                    collisionSystemState.CollisionManifold.Clear();
                }
                
                collisionSystemState.IntersectStepStopwatch.Restart();            
                    // reconstruct the bvh.
                    CollisionSystem.ReconstructBvhTree(componentRegistry, collisionSystemState.Bvh);
                    CollisionSystem.FindCollisions(componentRegistry, collisionSystemState);
                collisionSystemState.IntersectStepStopwatch.Stop();

                collisionSystemState.ResolutionStepStopwatch.Restart();
                    // NOTE: ordering matters here, make sure to resolve 
                    // collisions before sorting the collision manifold.
                    CollisionSystem.ResolveCollisions(componentRegistry, collisionSystemState);
                collisionSystemState.ResolutionStepStopwatch.Stop();            

                // NOTE: ordering matters here, make sure to resolve 
                // collisions before sorting the collision manifold.
                RigidBodySystem.ResolveCollisionsStep(componentRegistry, state.CollisionSystemState, deltaTime);
                
                // sort the collision manifold after resolution step.
                // this is to ensure that binary searching for collisions
                // using a GenIndex work outside of this function.
                collisionSystemState.CollisionManifold.Sort();            

            state.FixedUpdateSubStepStopwatch.Stop();
        }

        // clear added forces at the end so that the forces are fully
        // applied over the course of the fixed update step, and not 
        // sub-step dependent.
        RigidBodySystem.ClearForces(componentRegistry);

        state.FixedUpdateStepStopwatch.Stop();
    }

    /// <summary>
    /// Creates a new DrawSystem instance.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="render"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static DrawSystem DrawSystem(ComponentRegistry componentRegistry, IRenderer render, PhysicsSystemState state)
    => deltaTime =>
    {
        DrawStep(componentRegistry, render, state, deltaTime);
    };

    /// <summary>
    /// Draw step for the Physics System.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="renderer"></param>
    /// <param name="state"></param>
    /// <param name="deltaTime"></param>
    public static void DrawStep(ComponentRegistry componentRegistry, IRenderer renderer, PhysicsSystemState state, float deltaTime)
    {
        state.ThrowIfDisposed();
        CollisionSystem.DrawStep(componentRegistry, renderer, state.CollisionSystemState, deltaTime);
    }
}