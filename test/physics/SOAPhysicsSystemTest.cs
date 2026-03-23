using Howl.ECS;
using Howl.Math.Shapes;
using Howl.Physics;
using Howl.Math;
using Howl.DataStructures;
using Howl.Generic;
using static Howl.Physics.SoaPhysicsSystem;
using static Howl.Math.Shapes.Rectangle;
using static System.Runtime.InteropServices.CollectionsMarshal;
using static Howl.Test.Physics.SoaSpatialPairHelpers;
using static Howl.Physics.PhysicsBody;
using Howl.Test.Math;


namespace Howl.Test.Physics;

public class SOAPhysicsSystemTest
{
    int maxBodies = 10;
    int maxBodyShapeVertices = 100;
    int maxBodyShapeVerticeCount = 5;
    int maxCollisions = 65535;




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
        Assert.Equal(physicsMaterial.KineticFriction, GetKinematicFriction(state, genIndex), precision: 4);
        Assert.Equal(physicsMaterial.StaticFriction, GetStaticFriction(state, genIndex), precision: 4);
        Assert.Equal(physicsMaterial.Density, GetDensity(state, genIndex), precision: 4);
        Assert.Equal(physicsMaterial.Restitution, GetRestitution(state, genIndex), precision: 4);
    }




    /*******************
    
        Single Procedures.
    
    ********************/




    [Fact]
    public void AddVertices_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, maxCollisions);        
        
        // first data set test.

        AddVertices(state, [0,1,2,3], [2,3,4,5], out int firstIndex, out int vertexCount);
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
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, maxCollisions);

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
    
        Assert.Equal(posX, state.Transforms.Positions.X[genIndex.Index], precision: 1);
        Assert.Equal(posY, state.Transforms.Positions.Y[genIndex.Index], precision: 3);
        Assert.Equal(scaleX, state.Transforms.Scales.X[genIndex.Index], precision: 1);
        Assert.Equal(scaleY, state.Transforms.Scales.Y[genIndex.Index], precision: 3);        
        Assert.Equal(cos, state.Transforms.Coses[genIndex.Index], precision: 4);
        Assert.Equal(sin, state.Transforms.Sins[genIndex.Index], precision: 4);
    }




    /*******************
    
        Circle.
    
    ********************/




    [Fact]
    public void AllocateCircleCollider_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies,maxBodyShapeVertices, maxBodyShapeVerticeCount, maxCollisions);
        
        float posX;
        float posY;
        float radius;
        Circle circle;
        GenIndex genIndex = default;
        Transform transform;

        // first data set test.

        posX = 12;
        posY = 13;
        radius = 3;
        circle = new(posX, posY, radius);
        transform = new(1,2,3,4,5,6,7);
        AllocateCircleCollider(state, circle, transform, true, false, ref genIndex);
        Assert.Equal(0, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.False(HasRigidBody(state, genIndex));
        Assert.True(IsKinematic(state, genIndex));
        Assert.False(IsTrigger(state, genIndex));
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);
        Soa_TransformAssert.EntryEquals(state.Transforms, ref transform, genIndex.Index, 4);

        // second data set test.

        posX = 32;
        posY = -54;
        radius = 67;
        circle = new(posX, posY, radius);
        transform = new(9,8,7,6,5,4,3);

        AllocateCircleCollider(state, circle, transform, false, true, ref genIndex);

        Assert.Equal(1, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.False(HasRigidBody(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));
        Assert.True(IsTrigger(state, genIndex));
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);
        Soa_TransformAssert.EntryEquals(state.Transforms, ref transform, genIndex.Index, 4);
    }

    [Fact]
    public void AllocateCircleRigidBody_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, maxCollisions);
                
        float posX;
        float posY;
        float radius;
        PhysicsMaterial physicsMaterial;
        Circle circle;
        GenIndex genIndex = default;
        Transform transform;

        // first data set test.

        posX = -123;
        posY = 23;
        radius = 32;
        circle = new(posX, posY, radius);
        physicsMaterial = new PhysicsMaterial(0.2f, 0.1f, 15, 0.12f);
        transform = new(1,2,3,4,5,6,7);

        AllocateCircleRigidBody(state, circle, physicsMaterial, transform, true, false, true, ref genIndex);
        Assert.Equal(0, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.True(IsKinematic(state, genIndex));
        Assert.False(IsTrigger(state, genIndex));
        Assert.True(UsesRotationalPhysics(state, genIndex));
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);
        AssertPhysicsMaterial(state, physicsMaterial, genIndex);
        Soa_TransformAssert.EntryEquals(state.Transforms, ref transform, genIndex.Index, 4);


        // second data set test.

        posX = 234;
        posY = 567;
        radius = 12;
        circle = new(posX, posY, radius);
        physicsMaterial = new PhysicsMaterial(0.9f, 0.5f, 18, 0.1f);
        transform = new(0,9,8,7,6,5,4);

        AllocateCircleRigidBody(state, circle, physicsMaterial, transform, false, true, false, ref genIndex);
        Assert.Equal(1, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));
        Assert.True(IsTrigger(state, genIndex));
        Assert.False(UsesRotationalPhysics(state, genIndex));
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);
        AssertPhysicsMaterial(state, physicsMaterial, genIndex);
        Soa_TransformAssert.EntryEquals(state.Transforms, ref transform, genIndex.Index, 4);
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
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, maxCollisions);

        float posX;
        float posY;
        float width;
        float height;
        Rectangle rectangle;
        GenIndex genIndex = default;
        Transform transform;


        // first data set test.
        
        posX = 98;
        posY = 65;
        height = 12;
        width = 32;
        rectangle = new Rectangle(posX, posY, width, height);
        transform = new(1,2,3,4,5,6,7);
        AllocateRectangleCollider(state, rectangle, transform, false, true, ref genIndex);
        
        Assert.Equal(0, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(0, state.FirstVertexIndices[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.False(HasRigidBody(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));    
        Assert.True(IsTrigger(state, genIndex));
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);
        AssertRectangleVerticesClockwise(state, state.FirstVertexIndices[genIndex.Index], rectangle);
        Soa_TransformAssert.EntryEquals(state.Transforms, ref transform, genIndex.Index, 4);


        // second data set test.

        posX = 123;
        posY = -45;
        height = 98;
        width = 54;
        rectangle = new Rectangle(posX, posY, width, height);
        transform = new(0,9,8,7,6,5,4);  
        AllocateRectangleCollider(state, rectangle, transform, false, true, ref genIndex);
        
        Assert.Equal(1, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(4, state.FirstVertexIndices[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.False(HasRigidBody(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));
        Assert.True(IsTrigger(state, genIndex));
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);
        AssertRectangleVerticesClockwise(state, state.FirstVertexIndices[genIndex.Index], rectangle);
        Soa_TransformAssert.EntryEquals(state.Transforms, ref transform, genIndex.Index, 4);
    }

    /// <summary>
    /// Tests the allocation of a recatngle rigidbody into a physics system state.
    /// </summary>
    [Fact]
    public void AllocateRectangleRigidBody_Test()
    {
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, maxCollisions);

        float posX;
        float posY;
        float width;
        float height;
        Rectangle rectangle;
        GenIndex genIndex = default;
        PhysicsMaterial physicsMaterial;
        Transform transform;
        
        // first data set test.

        posX = -24;
        posY = 123;
        height = 345;
        width = 56;
        rectangle = new Rectangle(posX, posY, width, height);
        physicsMaterial = new(0.2f, 0.1f, 0.25f, 0.5f);
        transform = new(1,2,3,4,5,6,7);
        AllocateRectangleRigidBody(state, rectangle, physicsMaterial, transform, false, true, false, ref genIndex);        

        Assert.Equal(0, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(0, state.FirstVertexIndices[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.False(IsKinematic(state, genIndex));    
        Assert.True(IsTrigger(state, genIndex));
        Assert.Equal(1, state.AlloctedPhysicsBodyCount);
        AssertPhysicsMaterial(state, physicsMaterial, genIndex);
        AssertRectangleVerticesClockwise(state, state.FirstVertexIndices[genIndex.Index], rectangle);
        Soa_TransformAssert.EntryEquals(state.Transforms, ref transform, genIndex.Index, 4);
        
        // second data set test.

        posX = 4;
        posY = -56;
        height = 12;
        width = 43;
        rectangle = new Rectangle(posX, posY, width, height);
        physicsMaterial = new(0.64f, 0.12f, 12, 0.3f);
        transform = new(0,9,8,7,6,5,4);
        AllocateRectangleRigidBody(state, rectangle, physicsMaterial, transform, true, false, true, ref genIndex);        

        Assert.Equal(1, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(4, state.FirstVertexIndices[genIndex.Index]);
        Assert.True(IsActive(state, genIndex));
        Assert.True(IsAllocated(state, genIndex));
        Assert.True(HasRigidBody(state, genIndex));
        Assert.True(IsKinematic(state, genIndex));
        Assert.False(IsTrigger(state, genIndex));
        Assert.Equal(2, state.AlloctedPhysicsBodyCount);
        AssertPhysicsMaterial(state, physicsMaterial, genIndex);
        AssertRectangleVerticesClockwise(state, state.FirstVertexIndices[genIndex.Index], rectangle);
        Soa_TransformAssert.EntryEquals(state.Transforms, ref transform, genIndex.Index, 4);
    }




    /*******************
    
        Composite Procedures.
    
    ********************/




    [Fact]
    public void TransformPhysicsBodyVertices_Test()
    {        
        
        SoaPhysicsSystemState state = new SoaPhysicsSystemState(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, maxCollisions);
        
        // define and insert circle.

        float cExpectedX         = 4;
        float cExpectedY         = 6;
        Circle circle = new Circle(1,1,3);
        Transform cTransform = new Transform(new Vector2(1,3), 3, 0);
        GenIndex cBodyGenIndex = default;

        AllocateCircleCollider(state, circle, cTransform, false, false, ref cBodyGenIndex);

        // define and insert rect.

        Span<float> rExpectedX = [5, 9, 9, 5];
        Span<float> rExpectedY = [5, 5, 1, 1];
        Rectangle rectangle = new Rectangle(1,1,2,2);
        Transform rTransform = new Transform(new Vector2(3,3), 2, 0);
        GenIndex rBodyGenIndex = default;

        AllocateRectangleCollider(state, rectangle, rTransform, false, false, ref rBodyGenIndex);

        // transform the shapes.

        TransformPhysicsBodyVertices(state.Centroids, state.MinAABBVertices, state.MaxAABBVertices,
            state.LocalVertices, state.WorldVertices, state.Transforms, state.Flags, 
            state.LocalRadii, state.WorldRadii, state.LocalWidths, state.LocalHeights, state.FirstVertexIndices, 
            state.NextVertexIndices, state.MaxPhysicsBodyVertexCount, state.MaxPhysicsBodyCount, state.AlloctedPhysicsBodyCount
        );

        // assert circle.        
        Assert.Equal(cExpectedX,           state.WorldVertices.X[cBodyGenIndex.Index],   precision: 1);
        Assert.Equal(cExpectedY,           state.WorldVertices.Y[cBodyGenIndex.Index],   precision: 1);
        Soa_TransformAssert.EntryEquals(state.Transforms, ref cTransform, cBodyGenIndex.Index, 4);

        // assert rect.
        int first = state.FirstVertexIndices[rBodyGenIndex.Index];
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
        Soa_TransformAssert.EntryEquals(state.Transforms, ref rTransform, rBodyGenIndex.Index, 4);
    }

    [Fact]
    public void SyncEntityTransformsToPhysicsBodies_Test()
    {
        SoaPhysicsSystemState state = new(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, maxCollisions);
        GenIndexAllocator allocator = new();
        ComponentRegistry registry = new(allocator);
        SoaPhysicsSystem.RegisterComponents(registry);

        GenIndexList<PhysicsBodyId> physicsBodyIds = registry.Get<PhysicsBodyId>();
        GenIndexList<Transform> transforms = registry.Get<Transform>();

        Transform expected = new Transform(0,9,8,7,6,5,4);

        allocator.Allocate(out GenIndex entityGenIndex, out _);
        GenIndex bodyGenIndex = default;

        // allocate entity.
        AllocateRectangleCollider(state, new Rectangle(0,0,2,2), expected, true, false, ref bodyGenIndex);
        GenIndexListProc.Allocate(physicsBodyIds, entityGenIndex, new(bodyGenIndex));
        GenIndexListProc.Allocate(transforms, entityGenIndex, new Transform(1,2,3,4,5,6,7));
    
        SyncEntityTransformsToPhysicsBodies(transforms, physicsBodyIds, state.Transforms, state.Generations);

        GenIndexListProc.GetDenseRef(transforms, entityGenIndex, out Ref<Transform> transformRef);
        TransformAssert.Equals(ref expected, ref transformRef.Value, 4);
    }

    [Fact]
    public void SyncPhysicsBodiesToEntityTransforms_Test()
    {
        SoaPhysicsSystemState state = new(maxBodies, maxBodyShapeVertices, maxBodyShapeVerticeCount, maxCollisions);
        GenIndexAllocator allocator = new();
        ComponentRegistry registry = new(allocator);
        SoaPhysicsSystem.RegisterComponents(registry);
        GenIndex cBodyGenIndex = default;

        // allocate circle entity and rigidbody.
        allocator.Allocate(out GenIndex cEntityGenIndex, out _);

        // allocate circle rigidbody.
        AllocateCircleCollider(state, new Circle(1,-1,1), default, false, true, ref cBodyGenIndex);
        GenIndexListProc.Allocate(registry.Get<PhysicsBodyId>(), cEntityGenIndex, new PhysicsBodyId(cBodyGenIndex));

        // set transform of circle.
        Vector2 cPosition = new Vector2(2, 2);
        float cScale = 3;
        float cRotation = 45;
        Transform cTransform = new Transform(cPosition, cScale, cRotation);
        GenIndexListProc.Allocate(registry.Get<Transform>(), cEntityGenIndex, cTransform);
        GenIndex rBodyGenIndex = default;

        // allocate rectangle entity.
        allocator.Allocate(out GenIndex rEntityGenIndex, out _);
        
        // allocate rectangle rigidbody.
        AllocateRectangleCollider(state, new Rectangle(-2,2, 2,2), default, true, false, ref rBodyGenIndex);
        GenIndexListProc.Allocate(registry.Get<PhysicsBodyId>(), rEntityGenIndex, new PhysicsBodyId(rBodyGenIndex));
    
        // allocate rectangle transform.
        Vector2 rPosition = new Vector2(-3, 3);
        Vector2  rScale = new Vector2(2,4);
        float rRotation = 0;
        Transform rTransform = new Transform(rPosition, rScale, rRotation);
        GenIndexListProc.Allocate(registry.Get<Transform>(), rEntityGenIndex, rTransform);
    
        // sync the physics engine with the transforms.
        SyncTransformsToEntityTransforms(registry.Get<Transform>(), registry.Get<PhysicsBodyId>(), state.Transforms, state.Generations);

        // ensure the data was properly set inside the state.
        Soa_TransformAssert.EntryEquals(state.Transforms, ref cTransform, cBodyGenIndex.Index, 4);    
        Soa_TransformAssert.EntryEquals(state.Transforms, ref rTransform, rBodyGenIndex.Index, 4);    
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