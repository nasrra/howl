using System;
using Howl.Math;

namespace Howl.Physics;

public struct CircleRigidBody
{
    public RigidBody RigidBody;

    private float radius;
    public readonly float Radius => radius;

    public float radiusSquared;
    public readonly float RadiusSquared => radiusSquared; 

    public CircleRigidBody(Vector2 position, float restitution, float density, float radius, bool isStatic)
    {
        this.radius = radius;
        this.radiusSquared = radius * radius;
        RigidBody = new RigidBody(
            position,
            restitution, 
            density, 
            radiusSquared, 
            isStatic
        );
    }

    public void SetRadius(float restitution, float density, float radius, bool isStatic)
    {
        this.radius = radius;
        this.radiusSquared = radius * radius;
        RigidBody.SetArea(radiusSquared); 
    }
}