using Howl.ECS;
using Howl.Math.Shapes;
using Howl.Physics;
using static Howl.Physics.SOAPhysicsSystem;
using static Howl.Math.Shapes.Rectangle;

namespace Howl.Test.Physics;

public class SOAPhysicsSystemTest
{
    int maxBodies = 10;
    int maxVertices = 100;




    /*******************
    
        Helpers.
    
    ********************/



    private static void AssertPhysicsMaterial(SOAPhysicsSystemState state, PhysicsMaterial physicsMaterial, GenIndex genIndex)
    {
        Assert.Equal(physicsMaterial.KineticFriction, state.KineticFriction[genIndex.Index], precision: 4);
        Assert.Equal(physicsMaterial.StaticFriction, state.StaticFriction[genIndex.Index], precision: 4);
    }



    /*******************
    
        Circle.
    
    ********************/




    [Fact]
    public void AllocateCircleCollider_Test()
    {
        SOAPhysicsSystemState state = new SOAPhysicsSystemState(maxBodies,maxVertices, new(), new());
        
        float posX;
        float posY;
        float radius;
        Circle circle;

        // first data set test.

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
        Assert.False(HasPhysicsMaterial(state, genIndex1));
        Assert.True(IsKinematic(state, genIndex1));
        Assert.False(IsTrigger(state, genIndex1));

        // second data set test.

        posX = 32;
        posY = -54;
        radius = 67;
        circle = new(posX, posY, radius);

        AllocateCircleCollider(state, circle, false, true, out GenIndex genIndex2);

        Assert.Equal(1, genIndex2.Index);
        Assert.Equal(0, genIndex2.Generation);
        Assert.True(IsActive(state, genIndex2));
        Assert.True(IsAllocated(state, genIndex2));
        Assert.False(HasRigidBody(state, genIndex2));
        Assert.False(HasPhysicsMaterial(state, genIndex2));
        Assert.False(IsKinematic(state, genIndex2));
        Assert.True(IsTrigger(state, genIndex2));
    }

    [Fact]
    public void AllocateCircleRigidbodyWithoutPhysicsMaterial_Test()
    {
        SOAPhysicsSystemState state = new SOAPhysicsSystemState(maxBodies,maxVertices, new(), new());

        float posX;
        float posY;
        float radius;
        Circle circle;

        // first data set test.

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
        Assert.False(HasPhysicsMaterial(state, genIndex1));
        Assert.True(IsKinematic(state, genIndex1));
        Assert.False(IsTrigger(state, genIndex1));

        // second data set test.

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
        Assert.False(HasPhysicsMaterial(state, genIndex2));
        Assert.False(IsKinematic(state, genIndex2));
        Assert.True(IsTrigger(state, genIndex2));
    }

    [Fact]
    public void AllocateCircleRigidBodyWithPhysicsMaterial_Test()
    {
        SOAPhysicsSystemState state = new SOAPhysicsSystemState(maxBodies,maxVertices, new(), new());
                
        float posX;
        float posY;
        float radius;
        PhysicsMaterial physicsMaterial;
        Circle circle;
        GenIndex genIndex;

        // first data set test.

        posX = -123;
        posY = 23;
        radius = 32;
        circle = new(posX, posY, radius);        
        physicsMaterial = new PhysicsMaterial(0.25f, 0.1f);

        AllocateCircleRigidBody(state, circle, physicsMaterial, true, false, out genIndex);
        
        Assert.Equal(0, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.True(HasPhysicsMaterial(state, genIndex));
        Assert.True(IsKinematic(state, genIndex));
        Assert.False(IsTrigger(state, genIndex));
        
        AssertPhysicsMaterial(state, physicsMaterial, genIndex);

        // second data set test.

        posX = 234;
        posY = 567;
        radius = 12;
        circle = new(posX, posY, radius);
        physicsMaterial = new PhysicsMaterial(0.30f,0.2f);

        AllocateCircleRigidBody(state, circle, physicsMaterial, false, true, out genIndex);

        Assert.Equal(1, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.True(HasPhysicsMaterial(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));
        Assert.True(IsTrigger(state, genIndex));

        AssertPhysicsMaterial(state, physicsMaterial, genIndex);
    }




    /*******************
    
        Rectangle.
    
    ********************/




    /// <summary>
    /// Ensures that an inserted rectangles vertices are in a clockwise manner.
    /// </summary>
    /// <remarks>
    /// Note: this function assumes that the first vertice passed through is linked to a rectangle body in the simulation.
    /// there are no checks for this and will provide undfined behaviour if the passsed first vertice index is not for a 
    /// rectangle shape body.
    /// </remarks>
    /// <param name="state">the physics system state storing the rectnagle physics body.</param>
    /// <param name="firstVerticeIndex">the index of the first vertice of a physics body shape.</param>
    /// <param name="rectangle">the rectangle shape to check against the inserted vertices.</param>
    private static void AssertRectangleVerticesClockwise(
        SOAPhysicsSystemState state,
        int firstVerticeIndex,
        in Rectangle rectangle
    )
    {
        float[] expectedX =
        [
            Left(rectangle),
            Right(rectangle),
            Right(rectangle),
            Left(rectangle)
        ];

        float[] expectedY =
        [
            Top(rectangle),
            Top(rectangle),
            Bottom(rectangle),
            Bottom(rectangle)
        ];

        int v = firstVerticeIndex;
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(expectedX[i], state.VerticeX[v], precision: 1);
            Assert.Equal(expectedY[i], state.VerticeY[v], precision: 1);
            v = state.NextVertice[v];
        }
    }

    /// <summary>
    /// Tests the allocation of a rectangle collider into a physics system state.
    /// </summary>
    [Fact]
    public void AllocateRectangleCollider_Test()
    {
        SOAPhysicsSystemState state = new SOAPhysicsSystemState(maxBodies, maxVertices, new(), new());

        float posX;
        float posY;
        float width;
        float height;
        int firstVertice;
        Rectangle rectangle;
        GenIndex genIndex;

        posX = 98;
        posY = 65;
        height = 12;
        width = 32;
        rectangle = new Rectangle(posX, posY, width, height);

        // first data set test.

        AllocateRectangleCollider(state, rectangle, false, true, out genIndex);

        Assert.Equal(0, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(4, state.VerticeCount[genIndex.Index]);
        Assert.Equal(0, state.FirstVertice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.False(HasRigidBody(state, genIndex));
        Assert.False(HasPhysicsMaterial(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));    
        Assert.True(IsTrigger(state, genIndex));

        firstVertice = state.FirstVertice[genIndex.Index];
        AssertRectangleVerticesClockwise(state, firstVertice, rectangle);

        posX = 123;
        posY = -45;
        height = 98;
        width = 54;
        rectangle = new Rectangle(posX, posY, width, height);    

        // second data set test.

        AllocateRectangleCollider(state, rectangle, false, true, out genIndex);

        Assert.Equal(1, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(4, state.VerticeCount[genIndex.Index]);
        Assert.Equal(4, state.FirstVertice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.False(HasRigidBody(state, genIndex));
        Assert.False(HasPhysicsMaterial(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));
        Assert.True(IsTrigger(state, genIndex));

        firstVertice = state.FirstVertice[genIndex.Index];
        AssertRectangleVerticesClockwise(state, firstVertice, rectangle);
    }

    /// <summary>
    /// Tests the allocation of a rectangle rigidbody without a - physics material - into a physics system state.
    /// </summary>
    [Fact]
    public void AllocateRectangleRigidBodyWithoutPhysicsMaterial_Test()
    {
        SOAPhysicsSystemState state = new SOAPhysicsSystemState(maxBodies, maxVertices, new(), new());

        float posX;
        float posY;
        float width;
        float height;
        int firstVertice;
        Rectangle rectangle;
        GenIndex genIndex;

        posX = -24;
        posY = 123;
        height = 345;
        width = 56;
        rectangle = new Rectangle(posX, posY, width, height);

        // first data set test.

        AllocateRectangleRigidBody(state, rectangle, false, true, out genIndex);
    
        Assert.Equal(0, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(4, state.VerticeCount[genIndex.Index]);
        Assert.Equal(0, state.FirstVertice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.False(HasPhysicsMaterial(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));    
        Assert.True(IsTrigger(state, genIndex));
        
        firstVertice = state.FirstVertice[genIndex.Index];

        AssertRectangleVerticesClockwise(state, firstVertice, rectangle);

        // second data set test.

        posX = 4;
        posY = -56;
        height = 12;
        width = 43;
        rectangle = new Rectangle(posX, posY, width, height);

        AllocateRectangleRigidBody(state, rectangle, true, false, out genIndex);

        Assert.Equal(1, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(4, state.VerticeCount[genIndex.Index]);
        Assert.Equal(4, state.FirstVertice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.False(HasPhysicsMaterial(state, genIndex));
        Assert.True(IsKinematic(state, genIndex));
        Assert.False(IsTrigger(state, genIndex));

        firstVertice = state.FirstVertice[genIndex.Index];
        AssertRectangleVerticesClockwise(state, firstVertice, rectangle);
    }

    /// <summary>
    /// Tests the allocation of a recatngle rigidbody into a physics system state.
    /// </summary>
    [Fact]
    public void AllocateRectangleRigidBody_Test()
    {
        SOAPhysicsSystemState state = new SOAPhysicsSystemState(maxBodies, maxVertices, new(), new());

        float posX;
        float posY;
        float width;
        float height;
        Rectangle rectangle;
        GenIndex genIndex;
        PhysicsMaterial physicsMaterial;
        int firstVertice;

        // first data set test.

        posX = -24;
        posY = 123;
        height = 345;
        width = 56;
        rectangle = new Rectangle(posX, posY, width, height);
        physicsMaterial = new(0.2f, 0.05f);

        AllocateRectangleRigidBody(state, rectangle, physicsMaterial, false, true, out genIndex);

        Assert.Equal(0, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(4, state.VerticeCount[genIndex.Index]);
        Assert.Equal(0, state.FirstVertice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.True(HasPhysicsMaterial(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));    
        Assert.True(IsTrigger(state, genIndex));

        firstVertice = state.FirstVertice[genIndex.Index];
        AssertRectangleVerticesClockwise(state, firstVertice, rectangle);
        AssertPhysicsMaterial(state, physicsMaterial, genIndex);
        
        // second data set test.

        posX = 4;
        posY = -56;
        height = 12;
        width = 43;
        rectangle = new Rectangle(posX, posY, width, height);
        physicsMaterial = new(0.5f, 0.25f);

        AllocateRectangleRigidBody(state, rectangle, physicsMaterial, true, false, out genIndex);

        Assert.Equal(1, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(4, state.VerticeCount[genIndex.Index]);
        Assert.Equal(4, state.FirstVertice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.True(HasPhysicsMaterial(state, genIndex));
        Assert.True(IsKinematic(state, genIndex));
        Assert.False(IsTrigger(state, genIndex));

        firstVertice = state.FirstVertice[genIndex.Index];
        AssertRectangleVerticesClockwise(state, firstVertice, rectangle);
        AssertPhysicsMaterial(state, physicsMaterial, genIndex);
    }
}