using Howl.Ecs;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Physics.Telo;

namespace Howl.Physics;

public static class PhysicsSystem
{
    /// <summary>
    ///     Registers the nexessary components for the a Howl App to interface with the physics system.
    /// </summary>
    /// <param name="app">the howl app instance to register the components into.</param>
    public static void RegisterComponents(HowlApp app)
    {
        TeloPhysics.RegisterComponents(app.EcsState.Components);
    }

    /// <summary>
    ///     Ticks the intialised physics system in a Howl app instance forwards.
    /// </summary>
    /// <param name="app">the howl app instance that contains the physics system state to tick forward.</param>
    /// <param name="deltaTime">the time that has passed since the previous tick.</param>
    /// <param name="subSteps">the amount of substeps for this update.</param>
    public static void FixedUpdate(HowlApp app, float deltaTime, int subSteps)
    {
        TeloPhysics.FixedUpdate(app.EcsState, app.TeloPhysicsState, deltaTime, subSteps);
    }

    /// <summary>
    ///     Calls any draw functionality an intialised Howl App physics system may have.
    /// </summary>
    /// <param name="app">the howl app that contains the physics system state to draw.</param>
    /// <param name="deltaTime">thet ime that has passed since the previous tick.</param>
    public static void Draw(HowlApp app, float deltaTime)
    {
        TeloPhysics.Draw(app, app.TeloPhysicsState, deltaTime);
    }

    /// <summary>
    /// Allocates a circle collider into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static GenIdResult AllocateCircleCollider(HowlApp app, Circle shape, Transform transform, bool isKinematic, bool isTrigger, 
        ref GenId genId
    )
    {
        return PhysicsBody.AllocateCircleCollider(app.TeloPhysicsState, shape, transform, isKinematic, isTrigger, ref genId);
    }

    /// <summary>
    /// Allocates a circle rigidbody into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="physicsMaterial">the physics material to apply to the physics body.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static GenIdResult AllocateCircleRigidBody(HowlApp app, Circle shape, PhysicsMaterial physicsMaterial, Transform transform,
        bool isKinematic, bool isTrigger, bool rotationalPhysics, ref GenId genId
    )
    {
        return PhysicsBody.AllocateCircleRigidBody(app.TeloPhysicsState, shape, transform, physicsMaterial.StaticFriction, 
            physicsMaterial.KineticFriction, physicsMaterial.Density, physicsMaterial.Restitution, isKinematic, isTrigger, rotationalPhysics, 
            ref genId
        );
    }

    /// <summary>
    /// Allocates a rectangle collider into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static GenIdResult AllocateRectangleCollider(HowlApp app, Rectangle shape, Transform transform, 
        bool isKinematic, bool isTrigger, ref GenId genId
    )
    {
        return PhysicsBody.AllocateRectangleCollider(app.TeloPhysicsState, shape, transform, isKinematic, isTrigger, ref genId);
    }

    /// <summary>
    /// Allocates a rectangle rigidbody into a physics system state.
    /// </summary>
    /// <param name="state">the physics system state to allocate into.</param>
    /// <param name="shape">the local-space shape data.</param>
    /// <param name="physicsMaterial">the physics material to apply to the physics body.</param>
    /// <param name="transform">the world-space transform to convert the shape from local-space into world-space.</param>
    /// <param name="isKinematic">whether 'trigger' behaviour is enabled.</param>
    /// <param name="isTrigger">whether 'kinematic' behaviour is enabled.</param>
    /// <param name="genIndex">the associated gen index to the newly allocated body.</param>
    public static GenIdResult AllocateRectangleRigidBody(HowlApp app, Rectangle shape, PhysicsMaterial physicsMaterial, Transform transform, 
        bool isKinematic, bool isTrigger, bool rotationalPhysics, ref GenId genId
    )
    {
        return PhysicsBody.AllocateRectangleRigidBody(app.TeloPhysicsState, shape, transform, physicsMaterial.StaticFriction, 
            physicsMaterial.KineticFriction, physicsMaterial.Density, physicsMaterial.Restitution, isKinematic, isTrigger, rotationalPhysics, 
            ref genId
        );
    }
}