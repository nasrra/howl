using Howl.Ecs;
using Howl.Math.Shapes;
using Howl.Physics;
using Howl.Math;
using Howl.DataStructures;
using Howl.Generic;
using static System.Runtime.InteropServices.CollectionsMarshal;
using Howl.Test.Math;


namespace Howl.Test.Physics;

public class Test_PhysicsSystem
{
    int maxBodies = 10;
    int maxBodyShapeVertices = 100;
    int maxBodyShapeVerticeCount = 5;




    /*******************
    
        Single Procedures.
    
    ********************/




    [Fact]
    public void AddVertices_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        // first data set test.

        PhysicsSystem.AddVertices(state, [0,1,2,3], [2,3,4,5], out int firstIndex, out int vertexCount);
        int nextIndex = state.NextVertexIndices[firstIndex];
        
        bool circular = false;
        for(int i = 0; i < vertexCount; i++)
        {
            if(nextIndex != firstIndex)
            {
                circular = true;
                break;
            }            
        }
        Assert.True(circular);
    }

    [Fact]
    public void SetTransform_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);

        GenId genId = default;
        EntityRegistry.Allocate(state.Entities, ref genId);

        float posX = -2.5f;
        float posY = 3.325f;
        float scaleX = 1f;
        float scaleY = -2f;
        float rotation = 75f;
        float cos = 0.9217512697f; // cos of rotation.
        float sin = -0.3877816354f; // sin of rotation.
        
        Transform transform = new Transform(
            new Vector2(posX, posY), 
            new Vector2(scaleX, scaleY), 
            rotation
        );

        PhysicsBody.SetTransform(state, genId, transform);
        Assert_Soa_Transform.EntryEqual(posX, posY, scaleX, scaleY, cos, sin, 4, PhysicsBody.GetPhysicsBodyIndex(genId), state.Transforms);    
    }

    [Fact]
    public void GetAndSetActive_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        GenIdResult result = default;
        GenId validId = default;
        EntityRegistry.Allocate(state.Entities, ref validId);

        // sucess cases.

        result = PhysicsBody.SetActive(state, validId, true);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.True(PhysicsBody.IsActive(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        result = PhysicsBody.SetActive(state, validId, false);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.False(PhysicsBody.IsActive(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
    
        // fail cases.
        GenId staleId = new GenId(1,100);

        result = PhysicsBody.SetActive(state, staleId, true);
        Assert.Equal(GenIdResult.StaleGenId, result);
        Assert.False(PhysicsBody.IsActive(state, staleId, ref result));
        Assert.Equal(GenIdResult.StaleGenId, result);

        // ensure the entry wasnt changed.
        Assert.False(PhysicsBody.IsActive(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
    }

    [Fact]
    public void GetAndSetAllocated_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        GenIdResult result = default;
        GenId validId = default;
        EntityRegistry.Allocate(state.Entities, ref validId);

        // sucess cases.

        result = PhysicsBody.SetAllocated(state, validId, true);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.True(PhysicsBody.IsAllocated(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        result = PhysicsBody.SetAllocated(state, validId, false);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.False(PhysicsBody.IsAllocated(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
    
        // fail cases.
        GenId staleId = new GenId(1,100);

        result = PhysicsBody.SetAllocated(state, staleId, true);
        Assert.Equal(GenIdResult.StaleGenId, result);
        Assert.False(PhysicsBody.IsAllocated(state, staleId, ref result));
        Assert.Equal(GenIdResult.StaleGenId, result);

        // ensure the entry wasnt changed.
        Assert.False(PhysicsBody.IsAllocated(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);        
    }

    [Fact]
    public void GetAndSetTrigger_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        GenIdResult result = default;
        GenId validId = default;
        EntityRegistry.Allocate(state.Entities, ref validId);

        // sucess cases.

        result = PhysicsBody.SetTrigger(state, validId, true);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.True(PhysicsBody.IsTrigger(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        result = PhysicsBody.SetTrigger(state, validId, false);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.False(PhysicsBody.IsTrigger(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
    
        // fail cases.
        GenId staleId = new GenId(1,100);

        result = PhysicsBody.SetTrigger(state, staleId, true);
        Assert.Equal(GenIdResult.StaleGenId, result);
        Assert.False(PhysicsBody.IsTrigger(state, staleId, ref result));
        Assert.Equal(GenIdResult.StaleGenId, result);

        // ensure the entry wasnt changed.
        Assert.False(PhysicsBody.IsTrigger(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
    }

    [Fact]
    public void GetAndSetKinematic_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        GenIdResult result = default;
        GenId validId = default;
        EntityRegistry.Allocate(state.Entities, ref validId);

        // sucess cases.

        result = PhysicsBody.SetKinematic(state, validId, true);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.True(PhysicsBody.IsKinematic(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        result = PhysicsBody.SetKinematic(state, validId, false);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.False(PhysicsBody.IsKinematic(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
    
        // fail cases.
        GenId staleId = new GenId(1,100);

        result = PhysicsBody.SetKinematic(state, staleId, true);
        Assert.Equal(GenIdResult.StaleGenId, result);
        Assert.False(PhysicsBody.IsKinematic(state, staleId, ref result));
        Assert.Equal(GenIdResult.StaleGenId, result);

        // ensure the entry wasnt changed.
        Assert.False(PhysicsBody.IsKinematic(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
    }

    [Fact]
    public void GetAndSetRigidBody_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        GenIdResult result = default;
        GenId validId = default;
        EntityRegistry.Allocate(state.Entities, ref validId);

        // sucess cases.

        result = PhysicsBody.SetRigidBody(state, validId, true);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.True(PhysicsBody.IsRigidBody(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        result = PhysicsBody.SetRigidBody(state, validId, false);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.False(PhysicsBody.IsRigidBody(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
    
        // fail cases.
        GenId staleId = new GenId(1,100);

        result = PhysicsBody.SetRigidBody(state, staleId, true);
        Assert.Equal(GenIdResult.StaleGenId, result);
        Assert.False(PhysicsBody.IsRigidBody(state, staleId, ref result));
        Assert.Equal(GenIdResult.StaleGenId, result);

        // ensure the entry wasnt changed.
        Assert.False(PhysicsBody.IsRigidBody(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);        
    }

    [Fact]
    public void SetAndGetRotationalPhysics()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        GenIdResult result = default;
        GenId validId = default;
        EntityRegistry.Allocate(state.Entities, ref validId);

        // sucess cases.

        result = PhysicsBody.SetRotationalPhysics(state, validId, true);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.True(PhysicsBody.UsesRotationalPhysics(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        result = PhysicsBody.SetRotationalPhysics(state, validId, false);
        Assert.Equal(GenIdResult.Ok, result);
        Assert.False(PhysicsBody.UsesRotationalPhysics(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
    
        // fail cases.
        GenId staleId = new GenId(1,100);

        result = PhysicsBody.SetRotationalPhysics(state, staleId, true);
        Assert.Equal(GenIdResult.StaleGenId, result);
        Assert.False(PhysicsBody.UsesRotationalPhysics(state, staleId, ref result));
        Assert.Equal(GenIdResult.StaleGenId, result);

        // ensure the entry wasnt changed.
        Assert.False(PhysicsBody.UsesRotationalPhysics(state, validId, ref result));
        Assert.Equal(GenIdResult.Ok, result);    
    }




    /*******************
    
        Circle.
    
    ********************/




    [Fact]
    public void AllocateCircleCollider_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies,maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        float posX;
        float posY;
        float radius;
        Circle circle;
        GenId genId = default;
        GenIdResult result = default;
        Transform transform;

        // first data set test.

        posX = 12;
        posY = 13;
        radius = 3;
        circle = new(posX, posY, radius);
        transform = new(1,2,3,4,5,6,7);
        PhysicsBody.AllocateCircleCollider(state, circle, transform, true, false, ref genId);
        
        Assert.Equal(1, GenId.GetIndex(genId));
        Assert.Equal(0, GenId.GetGeneration(genId));
        
        Assert.True(PhysicsBody.IsActive(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.True(PhysicsBody.IsAllocated(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.False(PhysicsBody.IsRigidBody(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsKinematic(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.False(PhysicsBody.IsTrigger(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.Equal(1, state.AlloctedPhysicsBodyCount);
        Assert_Soa_Transform.EntryEqual(transform, 4, PhysicsBody.GetPhysicsBodyIndex(genId), state.Transforms);

        // second data set test.

        posX = 32;
        posY = -54;
        radius = 67;
        circle = new(posX, posY, radius);
        transform = new(9,8,7,6,5,4,3);

        PhysicsBody.AllocateCircleCollider(state, circle, transform, false, true, ref genId);

        Assert.Equal(2, GenId.GetIndex(genId));
        Assert.Equal(0, GenId.GetGeneration(genId));
        
        Assert.True(PhysicsBody.IsActive(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.True(PhysicsBody.IsAllocated(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.False(PhysicsBody.IsRigidBody(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.False(PhysicsBody.IsKinematic(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsTrigger(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.Equal(2, state.AlloctedPhysicsBodyCount);
        Assert_Soa_Transform.EntryEqual(transform, 4, PhysicsBody.GetPhysicsBodyIndex(genId), state.Transforms);
    }

    [Fact]
    public void AllocateCircleRigidBody_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
                
        float posX;
        float posY;
        float radius;
        PhysicsMaterial physicsMaterial;
        Circle circle;
        Transform transform;

        GenId genId = default;
        GenIdResult result = default;

        // first data set test.

        posX = -123;
        posY = 23;
        radius = 32;
        circle = new(posX, posY, radius);
        physicsMaterial = new PhysicsMaterial(0.2f, 0.1f, 15, 0.12f);
        transform = new(1,2,3,4,5,6,7);

        PhysicsBody.AllocateCircleRigidBody(state, circle, physicsMaterial, transform, true, false, true, ref genId);
        
        Assert.Equal(1, PhysicsBody.GetPhysicsBodyIndex(genId));
        Assert.Equal(0, GenId.GetGeneration(genId));

        Assert.True(PhysicsBody.IsActive(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.True(PhysicsBody.IsAllocated(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsRigidBody(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsKinematic(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.False(PhysicsBody.IsTrigger(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.UsesRotationalPhysics(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.Equal(1, state.AlloctedPhysicsBodyCount);

        Assert_Soa_PhysicsMaterial.EntryEqual(physicsMaterial.StaticFriction, physicsMaterial.KineticFriction, physicsMaterial.Density,
            physicsMaterial.Restitution, PhysicsBody.GetPhysicsBodyIndex(genId), state.PhysicsMaterials 
        );

        Assert_Soa_Transform.EntryEqual(transform, 4, PhysicsBody.GetPhysicsBodyIndex(genId), state.Transforms);


        // second data set test.

        posX = 234;
        posY = 567;
        radius = 12;
        circle = new(posX, posY, radius);
        physicsMaterial = new PhysicsMaterial(0.9f, 0.5f, 18, 0.1f);
        transform = new(0,9,8,7,6,5,4);

        PhysicsBody.AllocateCircleRigidBody(state, circle, physicsMaterial, transform, false, true, false, ref genId);
        
        Assert.Equal(2, GenId.GetIndex(genId));
        Assert.Equal(0, GenId.GetGeneration(genId));
        
        Assert.True(PhysicsBody.IsActive(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsAllocated(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsRigidBody(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.False(PhysicsBody.IsKinematic(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsTrigger(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.False(PhysicsBody.UsesRotationalPhysics(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);
        
        Assert_Soa_PhysicsMaterial.EntryEqual(physicsMaterial.StaticFriction, physicsMaterial.KineticFriction, physicsMaterial.Density,
            physicsMaterial.Restitution, PhysicsBody.GetPhysicsBodyIndex(genId), state.PhysicsMaterials 
        );

        Assert_Soa_Transform.EntryEqual(transform, 4, PhysicsBody.GetPhysicsBodyIndex(genId), state.Transforms);
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
    private static void AssertRectangleVerticesClockwise(PhysicsSystemState state, int firstVerticeIndex, in Rectangle rectangle)
    {
        float[] expectedX =
        [
            Rectangle.Left(rectangle),
            Rectangle.Right(rectangle),
            Rectangle.Right(rectangle),
            Rectangle.Left(rectangle)
        ];

        float[] expectedY =
        [
            Rectangle.Top(rectangle),
            Rectangle.Top(rectangle),
            Rectangle.Bottom(rectangle),
            Rectangle.Bottom(rectangle)
        ];

        int v = firstVerticeIndex;
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(expectedX[i], state.LocalVertices.X[v], precision: 1);
            Assert.Equal(expectedY[i], state.LocalVertices.Y[v], precision: 1);
            v = state.NextVertexIndices[v];
        }
    }

    /// <summary>
    /// Tests the allocation of a rectangle collider into a physics system state.
    /// </summary>
    [Fact]
    public void AllocateRectangleCollider_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);

        float posX;
        float posY;
        float width;
        float height;
        Rectangle rectangle;
        Transform transform;

        GenId genId = default;
        GenIdResult result = default;

        // first data set test.
        
        posX = 98;
        posY = 65;
        height = 12;
        width = 32;
        rectangle = new Rectangle(posX, posY, width, height);
        transform = new(1,2,3,4,5,6,7);
        PhysicsBody.AllocateRectangleCollider(state, rectangle, transform, false, true, ref genId);
        
        int physicsBodyIndex = PhysicsBody.GetPhysicsBodyIndex(genId);

        Assert.Equal(1, physicsBodyIndex);
        Assert.Equal(0, GenId.GetGeneration(genId));
        
        Assert.Equal(1, state.FirstVertexIndices[physicsBodyIndex]);
        AssertRectangleVerticesClockwise(state, state.FirstVertexIndices[physicsBodyIndex], rectangle);
        
        Assert.True(PhysicsBody.IsActive(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsAllocated(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.False(PhysicsBody.IsRigidBody(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.False(PhysicsBody.IsKinematic(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.True(PhysicsBody.IsTrigger(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
 
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);
        
        Assert_Soa_Transform.EntryEqual(transform, 4, physicsBodyIndex, state.Transforms);


        // second data set test.

        posX = 123;
        posY = -45;
        height = 98;
        width = 54;
        rectangle = new Rectangle(posX, posY, width, height);
        transform = new(0,9,8,7,6,5,4);  

        PhysicsBody.AllocateRectangleCollider(state, rectangle, transform, false, true, ref genId);
        
        physicsBodyIndex = PhysicsBody.GetPhysicsBodyIndex(genId);

        Assert.Equal(2, physicsBodyIndex);
        Assert.Equal(0, GenId.GetGeneration(genId));

        Assert.Equal(5, state.FirstVertexIndices[physicsBodyIndex]);
        AssertRectangleVerticesClockwise(state, state.FirstVertexIndices[physicsBodyIndex], rectangle);
        
        Assert.True(PhysicsBody.IsActive(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsAllocated(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.False(PhysicsBody.IsRigidBody(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.False(PhysicsBody.IsKinematic(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.True(PhysicsBody.IsTrigger(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);        
        
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);

        
        Assert_Soa_Transform.EntryEqual(transform, 4, physicsBodyIndex, state.Transforms);
    }

    /// <summary>
    /// Tests the allocation of a recatngle rigidbody into a physics system state.
    /// </summary>
    [Fact]
    public void AllocateRectangleRigidBody_Test()
    {
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);

        float posX;
        float posY;
        float width;
        float height;
        Rectangle rectangle;
        PhysicsMaterial physicsMaterial;
        Transform transform;
        
        GenId genId = default;
        GenIdResult result = default;

        // first data set test.

        posX = -24;
        posY = 123;
        height = 345;
        width = 56;
        rectangle = new Rectangle(posX, posY, width, height);
        physicsMaterial = new(0.2f, 0.1f, 0.25f, 0.5f);
        transform = new(1,2,3,4,5,6,7);
        
        PhysicsBody.AllocateRectangleRigidBody(state, rectangle, physicsMaterial, transform, false, true, false, ref genId);        

        int physicsBodyIndex = PhysicsBody.GetPhysicsBodyIndex(genId);

        Assert.Equal(1, physicsBodyIndex);
        Assert.Equal(0, GenId.GetGeneration(genId));

        Assert.Equal(1, state.FirstVertexIndices[physicsBodyIndex]);
        AssertRectangleVerticesClockwise(state, state.FirstVertexIndices[physicsBodyIndex], rectangle);
        
        Assert.True(PhysicsBody.IsActive(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.True(PhysicsBody.IsAllocated(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsRigidBody(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.False(PhysicsBody.IsKinematic(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsTrigger(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.Equal(1, state.AlloctedPhysicsBodyCount);
        
        Assert_Soa_PhysicsMaterial.EntryEqual(physicsMaterial.StaticFriction, physicsMaterial.KineticFriction, physicsMaterial.Density,
            physicsMaterial.Restitution, PhysicsBody.GetPhysicsBodyIndex(genId), state.PhysicsMaterials 
        );

        Assert_Soa_Transform.EntryEqual(transform, 4, PhysicsBody.GetPhysicsBodyIndex(genId), state.Transforms);
        
        // second data set test.

        posX = 4;
        posY = -56;
        height = 12;
        width = 43;
        rectangle = new Rectangle(posX, posY, width, height);
        physicsMaterial = new(0.64f, 0.12f, 12, 0.3f);
        transform = new(0,9,8,7,6,5,4);
        
        PhysicsBody.AllocateRectangleRigidBody(state, rectangle, physicsMaterial, transform, true, false, true, ref genId);        

        physicsBodyIndex = PhysicsBody.GetPhysicsBodyIndex(genId);

        Assert.Equal(2, physicsBodyIndex);
        Assert.Equal(0, GenId.GetGeneration(genId));
        
        Assert.Equal(5, state.FirstVertexIndices[physicsBodyIndex]);
        AssertRectangleVerticesClockwise(state, state.FirstVertexIndices[physicsBodyIndex], rectangle);
        
        Assert.True(PhysicsBody.IsActive(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.True(PhysicsBody.IsAllocated(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.True(PhysicsBody.IsRigidBody(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);

        Assert.True(PhysicsBody.IsKinematic(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.False(PhysicsBody.IsTrigger(state, genId, ref result));
        Assert.Equal(GenIdResult.Ok, result);
        
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);
        
        Assert_Soa_PhysicsMaterial.EntryEqual(physicsMaterial.StaticFriction, physicsMaterial.KineticFriction, physicsMaterial.Density,
            physicsMaterial.Restitution, PhysicsBody.GetPhysicsBodyIndex(genId), state.PhysicsMaterials 
        );

        Assert_Soa_Transform.EntryEqual(transform, 4, PhysicsBody.GetPhysicsBodyIndex(genId), state.Transforms);
    }




    /*******************
    
        Composite Procedures.
    
    ********************/




    [Fact]
    public void TransformPhysicsBodyVertices_Test()
    {        
        
        PhysicsSystemState state = new PhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        // define and insert circle.

        float expectedXC         = 4;
        float expectedYC         = 6;
        Circle circle = new Circle(1,1,3);
        Transform transformC = new Transform(new Vector2(1,3), 3, 0);

        GenId genIdC = default;

        PhysicsBody.AllocateCircleCollider(state, circle, transformC, false, false, ref genIdC);

        // define and insert rect.

        Span<float> rExpectedX = [5, 9, 9, 5];
        Span<float> rExpectedY = [5, 5, 1, 1];
        Rectangle rectangle = new Rectangle(1,1,2,2);
        Transform transformR = new Transform(new Vector2(3,3), 2, 0);
        GenId genIdR = default;

        PhysicsBody.AllocateRectangleCollider(state, rectangle, transformR, false, false, ref genIdR);

        // transform the shapes.

        PhysicsSystem.TransformPhysicsBodyVertices(state.Centroids, state.MinAABBVertices, state.MaxAABBVertices,
            state.LocalVertices, state.WorldVertices, state.Transforms, state.Flags, 
            state.LocalRadii, state.WorldRadii, state.LocalWidths, state.LocalHeights, state.FirstVertexIndices, 
            state.NextVertexIndices, state.MaxPhysicsBodyVertexCount, state.MaxPhysicsBodyCount, state.AlloctedPhysicsBodyCount
        );

        // assert circle.        
        int physicsBodyIndexC = PhysicsBody.GetPhysicsBodyIndex(genIdC);
        Assert.Equal(expectedXC, state.WorldVertices.X[physicsBodyIndexC], precision: 1);
        Assert.Equal(expectedYC, state.WorldVertices.Y[physicsBodyIndexC], precision: 1);
        Assert_Soa_Transform.EntryEqual(transformC, 4, physicsBodyIndexC, state.Transforms);

        // assert rect.
        int physicsBodyIndexR = PhysicsBody.GetPhysicsBodyIndex(genIdR);
        int first = state.FirstVertexIndices[physicsBodyIndexR];
        int v = first;
        int count = 0;
        while (true)
        {
            Assert.Equal(rExpectedX[count], state.WorldVertices.X[v], precision: 1);
            Assert.Equal(rExpectedY[count], state.WorldVertices.Y[v], precision: 1);
            count++;
            if(v==first)
                break;
        }
        Assert_Soa_Transform.EntryEqual(transformR, 4, physicsBodyIndexR, state.Transforms);
    }

    [Fact]
    public void SyncEntityTransformsToPhysicsBodies_Test()
    {
        PhysicsSystemState state = new(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        
        EcsState ecs = new(8);
        PhysicsSystem.RegisterComponents(ecs.Components);

        // pull components into scope.
        ComponentArray<PhysicsBodyId> tags = EcsState.GetComponents<PhysicsBodyId>(ecs);
        ComponentArray<Transform> transforms = EcsState.GetComponents<Transform>(ecs);

        Transform expected = new Transform(0,9,8,7,6,5,4);

        GenId genId = default;
        EntityRegistry.Allocate(ecs.Entities, ref genId);

        // allocate entity.
        GenId bodyGenId = default;
        PhysicsBody.AllocateRectangleCollider(state, new Rectangle(0,0,2,2), expected, true, false, ref bodyGenId);
        ComponentArray.Allocate(tags, ecs.Entities, genId, new(bodyGenId));
        ComponentArray.Allocate(transforms, ecs.Entities, genId, new Transform(1,2,3,4,5,6,7));
    
        PhysicsSystem.SyncEntityTransformsToPhysicsBodies(ecs, state.Transforms, state.Generations);

        GenIdResult result = default;
        ref Transform transform = ref ComponentArray.GetData(transforms, ecs ,genId, ref result);
        Assert.Equal(GenIdResult.Ok, result);
        Assert_Transform.Equals(ref expected, ref transform, 4);
    }

    [Fact]
    public void SyncPhysicsBodiesToEntityTransforms_Test()
    {
        PhysicsSystemState state = new(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount);
        EcsState ecs = new EcsState(12);

        PhysicsSystem.RegisterComponents(ecs.Components);

        GenId circleBody = default;
        GenId circleEntity = default;

        ComponentArray<PhysicsBodyId> tags = EcsState.GetComponents<PhysicsBodyId>(ecs);
        ComponentArray<Transform> transforms = EcsState.GetComponents<Transform>(ecs);

        // allocate circle entity and rigidbody.
        EntityRegistry.Allocate(ecs.Entities, ref circleEntity);

        // allocate circle rigidbody.
        PhysicsBody.AllocateCircleCollider(state, new Circle(1,-1,1), default, false, true, ref circleBody);
        ComponentArray.Allocate(tags, ecs.Entities, circleEntity, new(circleBody));

        // set transform of circle.
        Vector2 circlePosition = new Vector2(2, 2);
        float circleScale = 3;
        float circleRotation = 45;
        Transform circleTransform = new Transform(circlePosition, circleScale, circleRotation);
        ComponentArray.Allocate(transforms, ecs.Entities, circleEntity, circleTransform);

        // allocate rectangle entity.
        GenId rectEntity = default;
        GenId rectBody = default;
        EntityRegistry.Allocate(ecs.Entities, ref rectEntity);
        
        // allocate rectangle rigidbody.
        PhysicsBody.AllocateRectangleCollider(state, new Rectangle(-2,2, 2,2), default, true, false, ref rectBody);
        ComponentArray.Allocate(tags, ecs.Entities, rectEntity, new(rectBody));
    
        // allocate rectangle transform.
        Vector2 rectPosition = new Vector2(-3, 3);
        Vector2  rectScale = new Vector2(2,4);
        float rectRotation = 0;
        Transform rectTransform = new Transform(rectPosition, rectScale, rectRotation);
        ComponentArray.Allocate(transforms, ecs.Entities, rectEntity, rectTransform);
    
        // sync the physics engine with the transforms.
        PhysicsSystem.SyncTransformsToEntityTransforms(ecs, state.Transforms, state.Generations);

        // ensure the data was properly set inside the state.
        Assert_Soa_Transform.EntryEqual(circleTransform, 4, PhysicsBody.GetPhysicsBodyIndex(circleBody), state.Transforms);
        Assert_Soa_Transform.EntryEqual(rectTransform, 4, PhysicsBody.GetPhysicsBodyIndex(rectBody), state.Transforms);
    }

    // [Fact]
    // public void FilterBvhIntoCollisionManifold_Test()
    // {
    //     SoaPhysicsSystemState state = new SoaPhysicsSystemState(10, 1024, 4, 1024);
        

    //     // spatial data.
    //     PhysicsBodyFlags polygonFlag = PhysicsBodyFlags.Active | PhysicsBodyFlags.Allocated | PhysicsBodyFlags.RectangleShape;
    //     PhysicsBodyFlags circleFlag = PhysicsBodyFlags.Active | PhysicsBodyFlags.Allocated;  
    //     GenIndex polygonA   = new GenIndex(0,0);
    //     GenIndex polygonB   = new GenIndex(1,0);
    //     GenIndex circleA    = new GenIndex(2,0);
    //     GenIndex circleB    = new GenIndex(3,0);

    //     // polygon to polygon.
    //     spatialPairs.Add(
    //         new SpatialPair(
    //             new QueryResult(polygonA,(byte)polygonFlag),
    //             new QueryResult(polygonB,(byte)polygonFlag)
    //         )
    //     );

    //     // circle to circle.
    //     spatialPairs.Add(
    //         new SpatialPair(
    //             new QueryResult(circleA, (byte)circleFlag),
    //             new QueryResult(circleB, (byte)circleFlag)
    //         )
    //     );

    //     // polygon to circle.
    //     spatialPairs.Add(
    //         new SpatialPair(
    //             new QueryResult(polygonA, (byte)polygonFlag),
    //             new QueryResult(circleB, (byte)circleFlag)
    //         )
    //     );
    //     spatialPairs.Add(
    //         new SpatialPair(
    //             new QueryResult(polygonB, (byte)polygonFlag),
    //             new QueryResult(circleA, (byte)circleFlag)
    //         )
    //     );


    //     FilterBvhIntoCollisionManifold(circleSpatialPairs, polygonSpatialPairs, polygonToCircleSpatialPairs, AsSpan(spatialPairs));

    //     // assert circle spatial pairs.
    //     Assert.Equal(1, circleSpatialPairs.Count);
    //     AssertEntry(circleSpatialPairs, 0, circleA.Index, circleA.Generation, circleB.Index, circleA.Generation, 
    //         (byte)circleFlag, (byte)circleFlag
    //     );

    //     // assert polygon spatial pairs.
    //     Assert.Equal(1, polygonSpatialPairs.Count);        
    //     AssertEntry(polygonSpatialPairs, 0, polygonA.Index, polygonA.Generation, polygonB.Index, polygonB.Generation, 
    //         (byte)polygonFlag, (byte)polygonFlag
    //     );
        
    //     // assert polygon to circle spatial pairs.
    //     Assert.Equal(2, polygonToCircleSpatialPairs.Count);        
    //     // entry 0.
    //     AssertEntry(polygonToCircleSpatialPairs, 0, polygonA.Index, polygonA.Generation, circleB.Index, circleB.Generation, 
    //         (byte)polygonFlag, (byte)circleFlag
    //     );
    //     // entry 1.
    //     AssertEntry(polygonToCircleSpatialPairs, 1, polygonB.Index, polygonB.Generation, circleA.Index, circleA.Generation, 
    //         (byte)polygonFlag, (byte)circleFlag
    //     );
    // }

    // [Fact]
    // public void FindCircleCollisions_Test()
    // {
    //     int soaCapacity = 10;
    //     int verticesCapacity = 40;
    //     Soa_Collision collisionsToResolve = new(soaCapacity);
    //     Soa_SpatialPair spatialPairs = new(soaCapacity);
    //     Soa_Vector2 vertices = new(verticesCapacity);
    //     Span<float> radii = stackalloc float[soaCapacity];
    //     Span<int> firstVertices = stackalloc int[soaCapacity];

    //     PhysicsBodyFlags flags = PhysicsBodyFlags.Active | PhysicsBodyFlags.Allocated;

    //     // set gen indices.        
    //     GenIndex circleA = new GenIndex(1, 0);
    //     GenIndex circleB = new GenIndex(2, 0);
    //     GenIndex circleC = new GenIndex(3, 0);

    //     // set radii.
    //     radii[0] = 0; // Nil Value.
    //     radii[circleA.Index] = 2;
    //     radii[circleB.Index] = 3;
    //     radii[circleC.Index] = 4;

    //     // set vertices.
    //     AppendVector2(vertices, 0f, 0f); // Nil value.
    //     AppendVector2(vertices, 0.5f, -0.5f);
    //     AppendVector2(vertices, 0f, 0.5f);
    //     AppendVector2(vertices, 100, -230);
        
    //     // set first vertices.
    //     firstVertices[circleA.Index] = 1;
    //     firstVertices[circleB.Index] = 2;
    //     firstVertices[circleC.Index] = 3;

    //     // set spatial pairs.
    //     AppendSpatialPair(spatialPairs, circleA, circleB, (byte)flags, (byte)flags);
    //     AppendSpatialPair(spatialPairs, circleB, circleC, (byte)flags, (byte)flags);

    //     FindCircleCollisions(collisionsToResolve, spatialPairs, vertices, radii, firstVertices);
    //     // FindCircleCollisions(collisionsToResolve, spatialPairs,
    //     //     vertices, Soa_Vector2 minAABBVectors, Soa_Vector2 maxAABBVectors, radii,
    //     //     firstVertexIndices
    //     // );
        
    //     // assert the collision to resolve.
    //     Assert.Equal(1, collisionsToResolve.Count);
    //     Assert.Equal(circleA.Index, collisionsToResolve.OwnerGenIndices.Indices[0]);
    //     Assert.Equal(circleA.Generation, collisionsToResolve.OwnerGenIndices.Generations[0]);
    //     Assert.Equal(circleB.Index, collisionsToResolve.OtherGenIndices.Indices[0]);
    //     Assert.Equal(circleB.Generation, collisionsToResolve.OtherGenIndices.Generations[0]);
    // }

    // [Fact]
    // public void FindPolygonCollisions_Test()
    // {
    //     int soaCapacity = 10;
    //     int verticesCapacity = 40;
    //     int maxPolygonVerticesCount = 4;
    //     Soa_Collision collisionsToResolve = new(soaCapacity);
    //     Soa_SpatialPair spatialPairs = new(soaCapacity);
    //     Soa_Vector2 vertices = new(verticesCapacity);
    //     List<int> firstVertexIndices = new List<int>();
    //     List<int> nextVertexIndices = new List<int>();

    //     PhysicsBodyFlags flags = PhysicsBodyFlags.Active | PhysicsBodyFlags.Allocated | PhysicsBodyFlags.RectangleShape;

    //     // set gen indices.        
    //     GenIndex polygonA = new GenIndex(1, 0);
    //     GenIndex polygonB = new GenIndex(2, 0);
    //     GenIndex polygonC = new GenIndex(3, 0);
    
        
    //     // first vertex indices.
    //     firstVertexIndices.Add(0); // nill.
    //     firstVertexIndices.Add(1);
    //     firstVertexIndices.Add(5);
    //     firstVertexIndices.Add(9);

    //     // next vertex indices.
    //     nextVertexIndices.Add(0); // nill.
        
    //     // polygonA.
    //     nextVertexIndices.Add(2);
    //     nextVertexIndices.Add(3);
    //     nextVertexIndices.Add(4);
    //     nextVertexIndices.Add(1);

    //     // polygon B.
    //     nextVertexIndices.Add(6);
    //     nextVertexIndices.Add(7);
    //     nextVertexIndices.Add(8);
    //     nextVertexIndices.Add(5);

    //     // polygon C.
    //     nextVertexIndices.Add(10);
    //     nextVertexIndices.Add(11);
    //     nextVertexIndices.Add(12);
    //     nextVertexIndices.Add(9);

    //     // set vertices.
    //     AppendVector2(vertices, 0f, 0f); // Nil value.
        
    //     // polygon A.
    //     AppendVector2(vertices, -2.5f, 2.5f);
    //     AppendVector2(vertices, 0.5f, 2.5f);
    //     AppendVector2(vertices, 0.5f, -2.5f);
    //     AppendVector2(vertices, -2.5f, -2.5f);

    //     // polygon B.
    //     AppendVector2(vertices, -400f, 300f);
    //     AppendVector2(vertices, -150f, 300f);
    //     AppendVector2(vertices, -150f, 100);
    //     AppendVector2(vertices, -400f, 100);

    //     // polygon C.
    //     AppendVector2(vertices, -4f, 3f);
    //     AppendVector2(vertices, -1.5f, 3f);
    //     AppendVector2(vertices, -1.5f, 1);
    //     AppendVector2(vertices, -4f, 1);

    //     // set spatial pairs
    //     AppendSpatialPair(spatialPairs, polygonA, polygonC, (byte)flags, (byte)flags);
    //     AppendSpatialPair(spatialPairs, polygonB, polygonC, (byte)flags, (byte)flags);

    //     FindPolygonCollisions(collisionsToResolve, spatialPairs, vertices, 
    //         AsSpan(firstVertexIndices), AsSpan(nextVertexIndices), maxPolygonVerticesCount
    //     );

    //     Assert.Equal(1, collisionsToResolve.Count);
    //     Assert.Equal(polygonA.Index, collisionsToResolve.OwnerGenIndices.Indices[0]);
    //     Assert.Equal(polygonA.Generation, collisionsToResolve.OwnerGenIndices.Generations[0]);
    //     Assert.Equal(polygonC.Index, collisionsToResolve.OtherGenIndices.Indices[0]);
    //     Assert.Equal(polygonC.Generation, collisionsToResolve.OtherGenIndices.Generations[0]);
    // }

    // [Fact]
    // public void FindPolygonToCircleCollisions_Test()
    // {
    //     int soaCapacity = 10;
    //     int verticesCapacity = 40;
    //     int maxPolygonVertexCount = 4;
    //     Soa_Collision collisionsToResolve = new(soaCapacity);
    //     Soa_SpatialPair spatialPairs = new(soaCapacity);
    //     Soa_Vector2 vertices = new(verticesCapacity);
    //     List<int> firstVertexIndices = new List<int>();
    //     List<int> nextVertexIndices = new List<int>();
    //     Span<float> radii = stackalloc float[soaCapacity];

    //     PhysicsBodyFlags polygonFlag = PhysicsBodyFlags.Active | PhysicsBodyFlags.Allocated | PhysicsBodyFlags.RectangleShape;
    //     PhysicsBodyFlags circleFlag = PhysicsBodyFlags.Active | PhysicsBodyFlags.Allocated;

    //     GenIndex polygonA = new GenIndex(1, 0);
    //     GenIndex polygonB = new GenIndex(2, 0);
    //     GenIndex circleA = new GenIndex(3, 0);

    //     // first vertex indices.
    //     firstVertexIndices.Add(0); // nill.
    //     firstVertexIndices.Add(1); // polygon A.
    //     firstVertexIndices.Add(5); // polygon B.
    //     firstVertexIndices.Add(9); // circle A.

    //     // next vertex indices.
    //     nextVertexIndices.Add(0); // nill.
        
    //     // polygonA.
    //     nextVertexIndices.Add(2);
    //     nextVertexIndices.Add(3);
    //     nextVertexIndices.Add(4);
    //     nextVertexIndices.Add(1);

    //     // polygon B.
    //     nextVertexIndices.Add(6);
    //     nextVertexIndices.Add(7);
    //     nextVertexIndices.Add(8);
    //     nextVertexIndices.Add(5);

    //     // set vertices
    //     AppendVector2(vertices, 0f, 0f); // Nil value.

    //     // polygon A.
    //     AppendVector2(vertices, -2.5f, 2.5f);
    //     AppendVector2(vertices, 0.5f, 2.5f);
    //     AppendVector2(vertices, 0.5f, -2.5f);
    //     AppendVector2(vertices, -2.5f, -2.5f);

    //     // polygon B.
    //     AppendVector2(vertices, -400f, 300f);
    //     AppendVector2(vertices, -150f, 300f);
    //     AppendVector2(vertices, -150f, 100);
    //     AppendVector2(vertices, -400f, 100);

    //     // circle
    //     AppendVector2(vertices, -300f, 100f);
    //     radii[circleA.Index] = 30;

    //     // set spatial pairs.
    //     AppendSpatialPair(spatialPairs, polygonA, circleA, (byte)polygonFlag, (byte)circleFlag);
    //     AppendSpatialPair(spatialPairs, polygonB, circleA, (byte)polygonFlag, (byte)circleFlag);
    
    //     FindPolygonToCircleCollisions(collisionsToResolve, spatialPairs, vertices, AsSpan(firstVertexIndices),
    //         AsSpan(nextVertexIndices), radii, maxPolygonVertexCount
    //     );

    //     Assert.Equal(1, collisionsToResolve.Count);
    //     Assert.Equal(polygonB.Index, collisionsToResolve.OwnerGenIndices.Indices[0]);
    //     Assert.Equal(polygonB.Generation, collisionsToResolve.OwnerGenIndices.Generations[0]);
    //     Assert.Equal(circleA.Index, collisionsToResolve.OtherGenIndices.Indices[0]);
    //     Assert.Equal(circleA.Generation, collisionsToResolve.OtherGenIndices.Generations[0]);        
    // }
}