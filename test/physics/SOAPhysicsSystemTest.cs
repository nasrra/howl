using Howl.ECS;
using Howl.Math.Shapes;
using Howl.Physics;
using static Howl.Physics.SOAPhysicsSystem;

namespace Howl.Test.Physics;

public class SOAPhysicsSystemTest
{
    int maxBodies = 10;
    int maxVertices = 100;

    [Fact]
    public void AllocateCircleCollider_Test()
    {
        SOAPhysicsSystemState state = new SOAPhysicsSystemState(maxBodies,maxVertices, new(), new());
        
        float posX;
        float posY;
        float radius;
        Circle circle;

        posX = 12;
        posY = 13;
        radius = 3;
        circle = new(posX, posY, radius);

        AllocateCircleCollider(state, circle, true, false, out GenIndex genIndex1);
        
        Assert.Equal(0, genIndex1.Index);
        Assert.Equal(0, genIndex1.Generation);
        Assert.True(IsActive(state, genIndex1));
        Assert.True(IsAllocated(state, genIndex1));
        Assert.False(HasRigidBody(state, genIndex1));
        Assert.False(UsesFriction(state, genIndex1));
        Assert.False(IsTrigger(state, genIndex1));
        Assert.True(IsKinematic(state, genIndex1));

        posX = 32;
        posY = 54;
        radius = 67;
        circle = new(posX, posY, radius);

        AllocateCircleCollider(state, circle, false, true, out GenIndex genIndex2);

        Assert.Equal(1, genIndex2.Index);
        Assert.Equal(0, genIndex2.Generation);
        Assert.True(IsActive(state, genIndex2));
        Assert.True(IsAllocated(state, genIndex2));
        Assert.False(HasRigidBody(state, genIndex2));
        Assert.False(UsesFriction(state, genIndex2));
        Assert.True(IsTrigger(state, genIndex2));
        Assert.False(IsKinematic(state, genIndex2));
    }

    [Fact]
    public void AllocateCircleRigidbodyWithoutFriction_Test()
    {
        SOAPhysicsSystemState state = new SOAPhysicsSystemState(maxBodies,maxVertices, new(), new());

        float posX;
        float posY;
        float radius;
        Circle circle;

        posX = -123;
        posY = 23;
        radius = 32;
        circle = new(posX, posY, radius);        
        
        AllocateCircleRigidBody(state, circle, true, false, out GenIndex genIndex1);
        
        Assert.Equal(0, genIndex1.Index);
        Assert.Equal(0, genIndex1.Generation);
        Assert.True(IsActive(state, genIndex1));
        Assert.True(IsAllocated(state, genIndex1));
        Assert.True(HasRigidBody(state, genIndex1));
        Assert.False(UsesFriction(state, genIndex1));
        Assert.False(IsTrigger(state, genIndex1));
        Assert.True(IsKinematic(state, genIndex1));

        posX = 234;
        posY = 567;
        radius = 12;
        circle = new(posX, posY, radius);

        AllocateCircleRigidBody(state, circle, false, true, out GenIndex genIndex2);

        Assert.Equal(1, genIndex2.Index);
        Assert.Equal(0, genIndex2.Generation);
        Assert.True(IsActive(state, genIndex2));
        Assert.True(IsAllocated(state, genIndex2));
        Assert.True(HasRigidBody(state, genIndex2));
        Assert.False(UsesFriction(state, genIndex2));
        Assert.True(IsTrigger(state, genIndex2));
        Assert.False(IsKinematic(state, genIndex2));
    }

    [Fact]
    public void AllocateCircleRigidBodyWithFriction()
    {
        SOAPhysicsSystemState state = new SOAPhysicsSystemState(maxBodies,maxVertices, new(), new());
    
        float posX;
        float posY;
        float radius;
        PhysicsMaterial physicsMaterial;
        Circle circle;

        posX = -123;
        posY = 23;
        radius = 32;
        circle = new(posX, posY, radius);        
        physicsMaterial = new PhysicsMaterial(0.25f, 0.1f);

        AllocateCircleRigidBody(state, circle, physicsMaterial, true, false, out GenIndex genIndex1);
        
        Assert.Equal(0, genIndex1.Index);
        Assert.Equal(0, genIndex1.Generation);
        Assert.True(IsActive(state, genIndex1));
        Assert.True(IsAllocated(state, genIndex1));
        Assert.True(HasRigidBody(state, genIndex1));
        Assert.True(UsesFriction(state, genIndex1));
        Assert.False(IsTrigger(state, genIndex1));
        Assert.True(IsKinematic(state, genIndex1));

        posX = 234;
        posY = 567;
        radius = 12;
        circle = new(posX, posY, radius);
        physicsMaterial = new PhysicsMaterial(0.30f,0.2f);

        AllocateCircleRigidBody(state, circle, physicsMaterial, false, true, out GenIndex genIndex2);

        Assert.Equal(1, genIndex2.Index);
        Assert.Equal(0, genIndex2.Generation);
        Assert.True(IsActive(state, genIndex2));
        Assert.True(IsAllocated(state, genIndex2));
        Assert.True(HasRigidBody(state, genIndex2));
        Assert.True(UsesFriction(state, genIndex2));
        Assert.True(IsTrigger(state, genIndex2));
        Assert.False(IsKinematic(state, genIndex2));
    }
}