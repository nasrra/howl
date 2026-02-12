using Howl.Math.Shapes;
using Howl.Physics;

namespace Howl.Test.Physics;

public class RigidBodyTest
{
    [Fact]
    public void Constructor_Test()
    {
        RigidBody rigidBody;
        
        rigidBody = new RigidBody(0, 1, RigidBodyMode.Kinematic);
        Assert.Equal(0, rigidBody.Restitution);
        Assert.Equal(1, rigidBody.Density);
        Assert.Equal(RigidBodyMode.Kinematic, rigidBody.Mode);
    
        rigidBody = new RigidBody(1, 20, RigidBodyMode.Dynamic);
        Assert.Equal(1, rigidBody.Restitution);
        Assert.Equal(20, rigidBody.Density);
        Assert.Equal(RigidBodyMode.Dynamic, rigidBody.Mode);
    }

    [Fact]
    public void SetShapeRectangle_Test()
    {
        RigidBody rigidBody;

        rigidBody = new RigidBody(1,12,RigidBodyMode.Dynamic);
        Assert.Equal(0, rigidBody.Area);
        Assert.Equal(12, rigidBody.Density);
        Assert.Equal(1, rigidBody.Restitution);
        Assert.Equal(0, rigidBody.RotationalInertia);
        rigidBody.SetShape(new Rectangle(0,0,10,10));
        Assert.Equal(100, rigidBody.Area);
        Assert.Equal(12, rigidBody.Density);
        Assert.Equal(1, rigidBody.Restitution);
        Assert.Equal(20000, rigidBody.RotationalInertia);

        rigidBody = new RigidBody(0.1f,3,RigidBodyMode.Dynamic);
        Assert.Equal(0, rigidBody.Area);
        Assert.Equal(3, rigidBody.Density);
        Assert.Equal(0.1f, rigidBody.Restitution);
        Assert.Equal(0, rigidBody.RotationalInertia);
        rigidBody.SetShape(new Rectangle(12,33,20,10));
        Assert.Equal(200, rigidBody.Area);
        Assert.Equal(3, rigidBody.Density);
        Assert.Equal(0.1f, rigidBody.Restitution, precision: 2);
        Assert.Equal(25000, rigidBody.RotationalInertia);

        rigidBody = new RigidBody(0.1f,16.75f,RigidBodyMode.Dynamic);
        Assert.Equal(0, rigidBody.Area);
        Assert.Equal(16.75f, rigidBody.Density);
        Assert.Equal(0.1f, rigidBody.Restitution);
        Assert.Equal(0, rigidBody.RotationalInertia);
        rigidBody.SetShape(new Rectangle(12,33,20,10), 3);
        Assert.Equal(200, rigidBody.Area);
        Assert.Equal(3, rigidBody.Density);
        Assert.Equal(0.1f, rigidBody.Restitution, precision: 2);
        Assert.Equal(25000, rigidBody.RotationalInertia);
    }

    [Fact]
    public void SetShapeCircle_Test()
    {
        RigidBody rigidBody;

        rigidBody = new RigidBody(1,12,RigidBodyMode.Dynamic);
        Assert.Equal(0, rigidBody.Area);
        Assert.Equal(12, rigidBody.Density);
        Assert.Equal(1, rigidBody.Restitution);
        Assert.Equal(0, rigidBody.RotationalInertia);
        rigidBody.SetShape(new Circle(0,0,10));
        Assert.Equal(314.159f, rigidBody.Area, precision: 3);
        Assert.Equal(12, rigidBody.Density);
        Assert.Equal(1, rigidBody.Restitution);
        Assert.Equal(188495.56f, rigidBody.RotationalInertia, precision: 3);

        rigidBody = new RigidBody(0.1f,3,RigidBodyMode.Dynamic);
        Assert.Equal(0, rigidBody.Area);
        Assert.Equal(3, rigidBody.Density);
        Assert.Equal(0.1f, rigidBody.Restitution);
        Assert.Equal(0, rigidBody.RotationalInertia);
        rigidBody.SetShape(new Circle(0,0,20));
        Assert.Equal(1256.637f, rigidBody.Area, precision: 3);
        Assert.Equal(3, rigidBody.Density);
        Assert.Equal(0.1f, rigidBody.Restitution, precision: 2);
        Assert.Equal(753982.24f, rigidBody.RotationalInertia, precision: 2);

        rigidBody = new RigidBody(0.1f,16.75f,RigidBodyMode.Dynamic);
        Assert.Equal(0, rigidBody.Area);
        Assert.Equal(16.75f, rigidBody.Density);
        Assert.Equal(0.1f, rigidBody.Restitution);
        Assert.Equal(0, rigidBody.RotationalInertia);
        rigidBody.SetShape(new Circle(0,0,20), 3);
        Assert.Equal(1256.637f, rigidBody.Area, precision: 3);
        Assert.Equal(3, rigidBody.Density);
        Assert.Equal(0.1f, rigidBody.Restitution, precision: 2);
        Assert.Equal(753982.24f, rigidBody.RotationalInertia);
    }
}