using Howl.ECS;
using Howl.Math.Shapes;
using Howl.Physics;
using static Howl.Physics.SoaPhysicsSystem;
using static Howl.Math.Shapes.Rectangle;
using Howl.Math;

namespace Howl.Test.Physics;

public class SOAPhysicsSystemTest
{
    int maxBodies = 10;
    int maxBodyShapeVertices = 100;
    int maxBodyShapeVerticeCount = 5;




    /*******************
    
        Helpers.
    
    ********************/


    /// <summary>
    /// Ensures that a physics material entry in the physics system state is equal to the specifed physics material.
    /// </summary>
    /// <param name="state">the physics system state that holds the material data.</param>
    /// <param name="physicsMaterial">the specified matieral to check equality against.</param>
    /// <param name="genIndex">the gen index used to look up the stored material data.</param>
    private static void AssertPhysicsMaterial(SoaPhysicsSystemState state, in PhysicsMaterial physicsMaterial, GenIndex genIndex)
    {
        Assert.Equal(physicsMaterial.KineticFriction, state.KineticFrictions[genIndex.Index], precision: 4);
        Assert.Equal(physicsMaterial.StaticFriction, state.StaticFrictions[genIndex.Index], precision: 4);
    }

    /// <summary>
    /// Ensures that a transform ntry in a physics system state is equal to the specified transform struct.
    /// </summary>
    /// <param name="state">the phsyics system state that holds the transform data.</param>
    /// <param name="transform">the transform to check equality against.</param>
    /// <param name="genIndex">the gen index used to look up the stored transform data.</param>
    private static void AssertTransform(SoaPhysicsSystemState state, in Transform transform, GenIndex genIndex)
    {
        Assert.Equal(transform.Position.X, state.Transforms.Position.X[genIndex.Index],   precision: 1);
        Assert.Equal(transform.Position.Y, state.Transforms.Position.Y[genIndex.Index],   precision: 1);
        Assert.Equal(transform.Scale.X,    state.Transforms.Scale.X[genIndex.Index],      precision: 1);
        Assert.Equal(transform.Scale.Y,    state.Transforms.Scale.Y[genIndex.Index],      precision: 1);        
        Assert.Equal(transform.Rotation,   state.Transforms.Rotation[genIndex.Index],     precision: 1);
        Assert.Equal(transform.Cos,        state.Transforms.Cos[genIndex.Index],          precision: 4);
        Assert.Equal(transform.Sin,        state.Transforms.Sin[genIndex.Index],          precision: 4);
    }




    /*******************
    
        Single Procedures.
    
    ********************/




    [Fact]
    public void AddVertices_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());        
        
        // first data set test.

        AddVertices(state, [0,1,2,3], [2,3,4,5], out int firstIndex, out int vertexCount);
        int nextIndex = state.NextVertexIndice[firstIndex];
        
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
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());

        GenIndex genIndex = new GenIndex(1,0);
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

        SetTransform(state.Transforms, state.Generations, genIndex, transform);
    
        Assert.Equal(posX, state.Transforms.Position.X[genIndex.Index], precision: 1);
        Assert.Equal(posY, state.Transforms.Position.Y[genIndex.Index], precision: 3);
        Assert.Equal(scaleX, state.Transforms.Scale.X[genIndex.Index], precision: 1);
        Assert.Equal(scaleY, state.Transforms.Scale.Y[genIndex.Index], precision: 3);        
        Assert.Equal(rotation, state.Transforms.Rotation[genIndex.Index], precision: 1);
        Assert.Equal(cos, state.Transforms.Cos[genIndex.Index], precision: 4);
        Assert.Equal(sin, state.Transforms.Sin[genIndex.Index], precision: 4);
    }




    /*******************
    
        Circle.
    
    ********************/




    [Fact]
    public void AllocateCircleCollider_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies,maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());
        
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
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);

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
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);
    }

    [Fact]
    public void AllocateCircleRigidbodyWithoutPhysicsMaterial_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies,maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());

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
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);

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
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);
    }

    [Fact]
    public void AllocateCircleRigidBodyWithPhysicsMaterial_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());
                
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
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);
        
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
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);

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
        SoaPhysicsSystemState state,
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
            Assert.Equal(expectedX[i], state.Vertices.X[v], precision: 1);
            Assert.Equal(expectedY[i], state.Vertices.Y[v], precision: 1);
            v = state.NextVertexIndice[v];
        }
    }

    /// <summary>
    /// Tests the allocation of a rectangle collider into a physics system state.
    /// </summary>
    [Fact]
    public void AllocateRectangleCollider_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());

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
        Assert.Equal(0, state.FirstVertexIndice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.False(HasRigidBody(state, genIndex));
        Assert.False(HasPhysicsMaterial(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));    
        Assert.True(IsTrigger(state, genIndex));
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);

        firstVertice = state.FirstVertexIndice[genIndex.Index];
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
        Assert.Equal(4, state.FirstVertexIndice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.False(HasRigidBody(state, genIndex));
        Assert.False(HasPhysicsMaterial(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));
        Assert.True(IsTrigger(state, genIndex));
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);

        firstVertice = state.FirstVertexIndice[genIndex.Index];
        AssertRectangleVerticesClockwise(state, firstVertice, rectangle);
    }

    /// <summary>
    /// Tests the allocation of a rectangle rigidbody without a - physics material - into a physics system state.
    /// </summary>
    [Fact]
    public void AllocateRectangleRigidBodyWithoutPhysicsMaterial_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());

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
        Assert.Equal(0, state.FirstVertexIndice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.False(HasPhysicsMaterial(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));    
        Assert.True(IsTrigger(state, genIndex));
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);
        
        firstVertice = state.FirstVertexIndice[genIndex.Index];

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
        Assert.Equal(4, state.FirstVertexIndice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.False(HasPhysicsMaterial(state, genIndex));
        Assert.True(IsKinematic(state, genIndex));
        Assert.False(IsTrigger(state, genIndex));
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);

        firstVertice = state.FirstVertexIndice[genIndex.Index];
        AssertRectangleVerticesClockwise(state, firstVertice, rectangle);
    }

    /// <summary>
    /// Tests the allocation of a recatngle rigidbody into a physics system state.
    /// </summary>
    [Fact]
    public void AllocateRectangleRigidBody_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());

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
        Assert.Equal(0, state.FirstVertexIndice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.True(HasPhysicsMaterial(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));    
        Assert.True(IsTrigger(state, genIndex));
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);

        firstVertice = state.FirstVertexIndice[genIndex.Index];
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
        Assert.Equal(4, state.FirstVertexIndice[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.True(HasPhysicsMaterial(state, genIndex));
        Assert.True(IsKinematic(state, genIndex));
        Assert.False(IsTrigger(state, genIndex));
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);

        firstVertice = state.FirstVertexIndice[genIndex.Index];
        AssertRectangleVerticesClockwise(state, firstVertice, rectangle);
        AssertPhysicsMaterial(state, physicsMaterial, genIndex);
    }




    /*******************
    
        Composite Procedures.
    
    ********************/




    [Fact]
    public void TransformPhysicsBodyVertices_Test()
    {        
        
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());
        
        // define and insert circle.

        float cExpectedX         = 4;
        float cExpectedY         = 6;
        float cExpectedRadius    = 9;
        Circle circle = new Circle(1,1,3);
        Transform cTransform = new Transform(new Vector2(1,3), 3, 0);

        AllocateCircleCollider(state, circle, false, false, out GenIndex cBodyGenIndex);
        SetTransform(state.Transforms, state.Generations, cBodyGenIndex, cTransform);

        // define and insert rect.

        Span<float> rExpectedX = [4, 6, 6, 4];
        Span<float> rExpectedY = [6, 6, 4, 4];
        Rectangle rectangle = new Rectangle(1,1,2,2);
        Transform rTransform = new Transform(new Vector2(3,3), 2, 0);

        AllocateRectangleCollider(state, rectangle, false, false, out GenIndex rBodyGenIndex);
        SetTransform(state.Transforms, state.Generations, rBodyGenIndex, rTransform);

        // transform the shapes.

        TransformPhysicsBodyVertices(
            state.Vertices,
            state.TransformedVertices,
            state.Transforms,
            state.Flags, 
            state.Radii,
            state.TransformedRadii,
            state.FirstVertexIndice,
            state.NextVertexIndice, 
            0, 
            maxBodies
        );

        // assert circle.        
        Assert.Equal(cExpectedX,           state.TransformedVertices.X[cBodyGenIndex.Index],   precision: 1);
        Assert.Equal(cExpectedY,           state.TransformedVertices.Y[cBodyGenIndex.Index],   precision: 1);
        Assert.Equal(cExpectedRadius,      state.TransformedRadii[cBodyGenIndex.Index],        precision: 1);
        AssertTransform(state, cTransform, cBodyGenIndex);

        // assert rect.
        int first = state.FirstVertexIndice[rBodyGenIndex.Index];
        int v = first;
        int count = 0;
        while (true)
        {
            Assert.Equal(rExpectedX[count], state.TransformedVertices.X[v], precision: 1);
            Assert.Equal(rExpectedY[count], state.TransformedVertices.Y[v], precision: 1);
            count++;
            if(v==first)
                break;
        }
        AssertTransform(state, rTransform, rBodyGenIndex);
    }

    [Fact]
    public void SyncTransforms_Test()
    {
        SoaPhysicsSystemState state = new(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, new(), new());
        GenIndexAllocator allocator = new();
        ComponentRegistry registry = new(allocator);
        SoaPhysicsSystem.RegisterComponents(registry);

        // allocate circle entity and rigidbody.
        allocator.Allocate(out GenIndex cEntityGenIndex, out _);

        // allocate circle rigidbody.
        AllocateCircleCollider(state, new Circle(1,-1,1), false, true, out GenIndex cBodyGenIndex);
        GenIndexListProc.Allocate(registry.Get<PhysicsBodyId>(), cEntityGenIndex, new PhysicsBodyId(cBodyGenIndex));

        // set transform of circle.
        Vector2 cPosition = new Vector2(2, 2);
        float cScale = 3;
        float cRotation = 45;
        Transform cTransform = new Transform(cPosition, cScale, cRotation);
        GenIndexListProc.Allocate(registry.Get<Transform>(), cEntityGenIndex, cTransform);

        // allocate rectangle entity.
        allocator.Allocate(out GenIndex rEntityGenIndex, out _);
        
        // allocate rectangle rigidbody.
        AllocateRectangleCollider(state, new Rectangle(-2,2, 2,2), true, false, out GenIndex rBodyGenIndex);
        GenIndexListProc.Allocate(registry.Get<PhysicsBodyId>(), rEntityGenIndex, new PhysicsBodyId(rBodyGenIndex));
    
        // allocate rectangle transform.
        Vector2 rPosition = new Vector2(-3, 3);
        Vector2  rScale = new Vector2(2,4);
        float rRotation = 0;
        Transform rTransform = new Transform(rPosition, rScale, rRotation);
        GenIndexListProc.Allocate(registry.Get<Transform>(), rEntityGenIndex, rTransform);
    
        // sync the physics engine with the transforms.
        SyncPhysicsBodiesToEntityTransforms(registry, state.Transforms, state.Generations);

        // ensure the data was properly set inside the state.
        AssertTransform(state, cTransform, cBodyGenIndex);    
        AssertTransform(state, rTransform, rBodyGenIndex);    
    }
}