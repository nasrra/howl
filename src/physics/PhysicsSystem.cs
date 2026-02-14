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
    /// FixedUpdate step for the Physics System.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="state"></param>
    /// <param name="deltaTime"></param>
    public static void FixedUpdate(ComponentRegistry componentRegistry, PhysicsSystemState state, float deltaTime, int subSteps)
    {
        state.FixedUpdateStepStopwatch.Restart();

        CollisionSystemState collisionSystemState = state.CollisionSystemState;
        RigidBodySystemState rigidbodySystemState = state.RigidbodySystemState;
        
        deltaTime /= (float)subSteps;

        for(int i = 0; i < subSteps; i++)
        {   
            state.FixedUpdateSubStepStopwatch.Restart();

                if(collisionSystemState.CollisionManifold.Collisions.Count > 0)
                {
                    collisionSystemState.CollisionManifold.Clear();
                }
                
                rigidbodySystemState.MovementStepStopwatch.Restart();
                    RigidBodySystem.MovementStep(componentRegistry, state.RigidbodySystemState, deltaTime);
                rigidbodySystemState.MovementStepStopwatch.Stop();

                collisionSystemState.SyncCollidersToTransformsStopwatch.Restart();
                    CollisionSystem.SyncCollidersToTransforms(componentRegistry);
                collisionSystemState.SyncCollidersToTransformsStopwatch.Stop();

                // reconstruct the bvh.
                collisionSystemState.BvhReconstructionStopwatch.Restart();
                    CollisionSystem.ReconstructBvhTree(componentRegistry, collisionSystemState.Bvh);
                collisionSystemState.BvhReconstructionStopwatch.Stop();

                collisionSystemState.FindNearColliderPairsStopwatch.Restart();            
                    CollisionSystem.FindNearColliderPairs(collisionSystemState);
                collisionSystemState.FindNearColliderPairsStopwatch.Stop();

                collisionSystemState.ProcessNearColliderPairsStopwatch.Restart();            
                    CollisionSystem.ProcessNearColliderPairs(componentRegistry, collisionSystemState);
                collisionSystemState.ProcessNearColliderPairsStopwatch.Stop();

                // NOTE: ordering matters here, make sure to resolve 
                // collisions before sorting the collision manifold.
                // Also make sure that this is above rigidbody collision resolution.
                // this function also moves the transforms of the colliders.
                collisionSystemState.ResolutionStopwatch.Restart();
                    CollisionSystem.ResolveCollisions(componentRegistry, collisionSystemState);
                collisionSystemState.ResolutionStopwatch.Stop();            

                // NOTE: ordering matters here, make sure to resolve 
                // collisions before sorting the collision manifold.
                // Also make sure that this is below collision resolution.
                // this function also moves the transforms of the colliders.
                rigidbodySystemState.CollisionResolutionStepStopwatch.Restart();
                    RigidBodySystem.ResolveCollisionsStep(componentRegistry, state.CollisionSystemState, deltaTime);
                rigidbodySystemState.CollisionResolutionStepStopwatch.Stop();
                
                // sort the collision manifold after resolution step.
                // this is to ensure that binary searching for collisions
                // using a GenIndex work outside of this function.
                collisionSystemState.CollisionManifoldSortStopwatch.Restart();
                    collisionSystemState.CollisionManifold.Sort();            
                collisionSystemState.CollisionManifoldSortStopwatch.Stop();
            
            state.FixedUpdateSubStepStopwatch.Stop();
        }

        // clear added forces at the end so that the forces are fully
        // applied over the course of the fixed update step, and not 
        // sub-step dependent.
        RigidBodySystem.ClearForces(componentRegistry);

        state.FixedUpdateStepStopwatch.Stop();
    }

    /// <summary>
    /// Draw step for the Physics System.
    /// </summary>
    /// <param name="componentRegistry"></param>
    /// <param name="renderer"></param>
    /// <param name="state"></param>
    /// <param name="deltaTime"></param>
    public static void Draw(ComponentRegistry componentRegistry, PhysicsSystemState state, float deltaTime)
    {
        state.ThrowIfDisposed();
        CollisionSystem.Draw(componentRegistry, state.CollisionSystemState, deltaTime);
    }
}