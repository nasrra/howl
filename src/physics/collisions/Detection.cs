using System;
using System.Runtime.CompilerServices;
using Howl.Collections;
using Howl.DataStructures;
using Howl.DataStructures.Bvh;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics.Collisions;

public static class Detection
{




    /******************
    
        Util.
    
    *******************/



    /// <summary>
    /// 
    /// </summary>
    /// <param name="collisions"></param>
    /// <param name="vertices"></param>
    /// <param name="centroidsX"></param>
    /// <param name="centroidsY"></param>
    /// <param name="ownerIndex"></param>
    /// <param name="otherIndex"></param>
    /// <param name="collided"></param>
    /// <returns>
    /// <remarks>
    ///     <list type = "bullet">
    ///         <item><see cref="CollisionIndexPair.AToB"/> = owner to other</item>
    ///         <item><see cref="CollisionIndexPair.BToA"/> = other to owner</item>
    ///     </list>
    /// </remarks>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static CollisionIndexPair Polygon_To_Polygon(CollisionManifoldState collisions, FsSoa_Vector2 vertices,
        Span<float> centroidsX, Span<float> centroidsY, int ownerIndex, int otherIndex, ref bool collided
    )
    {
        ref float ownerPosX = ref centroidsX[ownerIndex]; 
        ref float otherPosX = ref centroidsX[otherIndex]; 
        ref float ownerPosY = ref centroidsY[ownerIndex]; 
        ref float otherPosY = ref centroidsY[otherIndex];

        Span<float> ownerVertsX = default;
        Span<float> ownerVertsY = default;
        Span<float> otherVertsX = default;
        Span<float> otherVertsY = default;

        // gather polygon a vertices.
        PhysicsBody.GetPolygonVerticesUnsafe(vertices, ownerIndex, ref ownerVertsX, ref ownerVertsY);
        PhysicsBody.GetPolygonVerticesUnsafe(vertices, otherIndex, ref otherVertsX, ref otherVertsY);

        // narrow phase SAT intersect check.
        if(SAT.PolygonsIntersect(ownerVertsX, ownerVertsY, otherVertsX, otherVertsY, ownerPosX, ownerPosY, 
            otherPosX, otherPosY, out float normalX, out float normalY, out float depth
        ))
        {
            SAT.FindContactPoints(ownerVertsX, ownerVertsY, otherVertsX, otherVertsY, SAT.PolygonContactPointEpsilon, 
                out float firstContactPointX, out float firstContactPointY, out float secondContactPointX, out float secondContactPointY, 
                out int contactCount
            );

            collided = true;

            switch (contactCount)
            {
                case 1:
                    return CollisionManifold.SetDataTwoWay(collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                        normalX, normalY, firstContactPointX, firstContactPointY, depth
                    );
                case 2:
                    return CollisionManifold.SetDataTwoWay(collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                        normalX, normalY, firstContactPointX, firstContactPointY, secondContactPointX, secondContactPointY, depth
                    );
            }

        }
        
        collided = false;
        return default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collisions"></param>
    /// <param name="vertices"></param>
    /// <param name="centroidsX"></param>
    /// <param name="centroidsY"></param>
    /// <param name="radii"></param>
    /// <param name="polyIndex"></param>
    /// <param name="circIndex"></param>
    /// <param name="collided"></param>
    /// <returns>
    /// <remarks>
    ///     <list type = "bullet">
    ///         <item><see cref="CollisionIndexPair.AToB"/> = poly to circle</item>
    ///         <item><see cref="CollisionIndexPair.BToA"/> = circle to poly</item>
    ///     </list>
    /// </remarks>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static CollisionIndexPair Polygon_To_Circle(CollisionManifoldState collisions, FsSoa_Vector2 vertices, 
        Span<float> centroidsX, Span<float> centroidsY, Span<float> radii, int polyIndex, int circIndex, ref bool collided
    )
    {
        ref float polyPosX = ref centroidsX[polyIndex]; 
        ref float circPosX = ref centroidsX[circIndex]; 
        ref float polyPosY = ref centroidsY[polyIndex]; 
        ref float circPosY = ref centroidsY[circIndex];

        Span<float> polyVertsX = default;
        Span<float> polyVertsY = default;

        // gather polygon a vertices.
        PhysicsBody.GetPolygonVerticesUnsafe(vertices, polyIndex, ref polyVertsX, ref polyVertsY);

        bool intersect = SAT.PolygonAndCircleIntersect(polyVertsX, polyVertsY, polyPosX, polyPosY, circPosX, circPosY, radii[circIndex], 
            circPosX, circPosY, out float normalX, out float normalY, out float depth
        );
        // narrow phase intersect check.
        if(intersect)
        {            
            SAT.FindContactPoints(polyVertsX, polyVertsY, circPosX, circPosY, out float contactPointX, out float contactPointY);
            
            collided = true;

            return CollisionManifold.SetDataTwoWay(collisions, polyIndex, circIndex, polyPosX, polyPosY, circPosX, circPosY, 
                normalX, normalY, contactPointX, contactPointY, depth
            );
        }
        else
        {
            collided = false;
            return default;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collisions"></param>
    /// <param name="centroidsX"></param>
    /// <param name="centroidsY"></param>
    /// <param name="radii"></param>
    /// <param name="ownerIndex"></param>
    /// <param name="otherIndex"></param>
    /// <param name="collided"></param>
    /// <returns>
    /// <remarks>
    ///     <list type = "bullet">
    ///         <item><see cref="CollisionIndexPair.AToB"/> = owner to other</item>
    ///         <item><see cref="CollisionIndexPair.BToA"/> = other to owner</item>
    ///     </list>
    /// </remarks>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static CollisionIndexPair Circle_To_Circle(CollisionManifoldState collisions, Span<float> centroidsX, Span<float> centroidsY, 
        Span<float> radii, int ownerIndex, int otherIndex, ref bool collided
    )
    {
        ref float ownerPosX = ref centroidsX[ownerIndex];
        ref float otherPosX = ref centroidsX[otherIndex];
        ref float ownerPosY = ref centroidsY[ownerIndex];
        ref float otherPosY = ref centroidsY[otherIndex];        
        ref float ownerR = ref radii[ownerIndex];
        ref float otherR = ref radii[otherIndex];

        bool intersects = SAT.CirclesIntersect(ownerPosX, ownerPosY, ownerR, otherPosX, otherPosY, otherR, out float normalX, 
            out float normalY, out float depth
        );
    
        if(intersects)
        {
            collided = true;

            // submit the collision with contact points if one of the colliders needs them.
            SAT.FindContactPoints(ownerPosX, ownerPosY, ownerR, otherPosX, otherPosY, out float contactPointX, out float contactPointY);
            
            return CollisionManifold.SetDataTwoWay(collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                normalX, normalY, contactPointX, contactPointY, depth
            );
        }   

        collided = false;
        return default;
    }

    public static bool BroadPhase(IntrusiveList.Node[] nodes, float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY,
        int shapeIndexA, int shapeIndexB
    )
    {
        // skip if the two shapes are apart of the same body.
        if(nodes[shapeIndexA].Parent == nodes[shapeIndexB].Parent)
        {
            return false;
        }

        return Aabb.Intersect(
            minAabbsX[shapeIndexA], minAabbsX[shapeIndexB], minAabbsY[shapeIndexA], minAabbsY[shapeIndexB], 
            maxAabbsX[shapeIndexA], maxAabbsX[shapeIndexB], maxAabbsY[shapeIndexA], maxAabbsY[shapeIndexB]
        );
    }




    /******************
    
        Dynamic Polygon RigidBody.
    
    *******************/




    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DynamicRigidPolygon_To_DynamicRigidPolygon(OverlapInfo info, Span<int> bvhIndices, CollisionManifoldState collisions, IntrusiveList.Node[] nodes, 
        float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY,
        FsSoa_Vector2 vertices, CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }  
        }
    }

    public static void DynamicRigidPolygon_To_DynamicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY, 
        FsSoa_Vector2 vertices, Span<float> radii, CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }
    }

    public static void DynamicRigidPolygon_To_DynamicRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicRigidPolygon_To_KinematicRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> colliderCollisionsToResolve, 
        CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void DynamicRigidPolygon_To_KinematicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
        CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void DynamicRigidPolygon_To_KinematicRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicRigidPolygon_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }
    }

    public static void DynamicRigidPolygon_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void DynamicRigidPolygon_To_TriggerRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicRigidPolygon_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> subStepCollisionsToResolve 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, subStepCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }
    }

    public static void DynamicRigidPolygon_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }
    }

    public static void DynamicRigidPolygon_To_DynamicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicRigidPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> subStepCollisionsToResolve 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, subStepCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }        
    }

    public static void DynamicRigidPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }    
    }

    public static void DynamicRigidPolygon_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicRigidPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }   
    }

    public static void DynamicRigidPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }     
    }

    public static void DynamicRigidPolygon_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Dynamic Circle RigidBody.
    
    *******************/


    public static void DynamicRigidCircle_To_DynamicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, CategorisedOverlapArray<int> colliderCollisionsToResolve, 
        CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }        
    }

    public static void DynamicRigidCircle_To_DynamicRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicRigidCircle_To_KinematicRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
        CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );

                CategorisedOverlapArray.Append(collisionIndices.BToA, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void DynamicRigidCircle_To_KinematicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }                
    } 

    public static void DynamicRigidCircle_To_KinematicRigidCapsule()
    {
        throw new NotImplementedException();        
    }

    public static void DynamicRigidCircle_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }
    
    public static void DynamicRigidCircle_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }                
    } 

    public static void DynamicRigidCircle_To_TriggerRigidCapsule()
    {
        throw new NotImplementedException();
    } 

    public static void DynamicRigidCircle_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }
    }

    public static void DynamicRigidCircle_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }          
    }

    public static void DynamicRigidCircle_To_DynamicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicRigidCircle_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void DynamicRigidCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void DynamicRigidCircle_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicRigidCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void DynamicRigidCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void DynamicRigidCircle_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Kinematic Polygon RigidBody
    
    *******************/




    public static void KinematicRigidPolygon_To_KinematicRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }           
    }

    public static void KinematicRigidPolygon_To_KinematicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
        }
    }


    public static void KinematicRigidPolygon_To_KinematicRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void KinematicRigidPolygon_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }
    }
    
    public static void KinematicRigidPolygon_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
        }    
    }
    
    public static void KinematicRigidPolygon_To_TriggerRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void KinematicRigidPolygon_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, 
        CategorisedOverlapArray<int> subStepCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.BToA, subStepCollisionsToResolve, 
                    CollisionResolutionCategory.Kinematic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }  
    }

    public static void KinematicRigidPolygon_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void KinematicRigidPolygon_To_DynamicColliderCapsule()
    {
        throw new NotImplementedException();        
    }

    public static void KinematicRigidPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }          
    } 

    public static void KinematicRigidPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }


    public static void KinematicRigidPolygon_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void KinematicRigidPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void KinematicRigidPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicRigidPolygon_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    } 




    /******************
    
        Kinematic Circle RigidBody.
    
    *******************/




    public static void KinematicRigidCircle_To_KinematicRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicRigidCircle_To_KinematicRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void KinematicRigidCircle_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicRigidCircle_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicRigidCircle_To_TriggerRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void KinematicRigidCircle_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void KinematicRigidCircle_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);

            if (collided)
            {
                CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void KinematicRigidCircle_To_DynamicColliderCapsule()
    {
        throw new NotImplementedException();        
    }

    public static void KinematicRigidCircle_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicRigidCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicRigidCircle_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void KinematicRigidCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicRigidCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicRigidCircle_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Trigger Polygon Rigidbody.   
    
    *******************/



    public static void TriggerRigidPolygon_To_TriggerRigidPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerRigidPolygon_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void TriggerRigidPolygon_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerRigidPolygon_To_DynamicColliderCircle (OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerRigidPolygon_To_DynamicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void TriggerRigidPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerRigidPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerRigidPolygon_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void TriggerRigidPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerRigidPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerRigidPolygon_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Trigger Circle RigidBody.
    
    *******************/




    public static void TriggerRigidCircle_To_TriggerRigidCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerRigidCircle_To_TriggerRigidCapsule()
    {
        throw new NotImplementedException();
    }

    public static void TriggerRigidCircle_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerRigidCircle_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerRigidCircle_To_DynamicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void TriggerRigidCircle_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerRigidCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerRigidCircle_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void TriggerRigidCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerRigidCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerRigidCircle_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Dynamic Polygon Collider.
    
    *******************/





    public static void DynamicColliderPolygon_To_DynamicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }
    }

    public static void DynamicColliderPolygon_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }
    }

    public static void DynamicColliderPolygon_To_DynamicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicColliderPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void DynamicColliderPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void DynamicColliderPolygon_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicColliderPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }
    }

    public static void DynamicColliderPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
        }
    }

    public static void DynamicColliderPolygon_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Dynamic Circle Collider
    
    *******************/




    public static void DynamicColliderCircle_To_DynamicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Dynamic
                );
            }
        }        
    }

    public static void DynamicColliderCircle_To_DynamicColliderCapsule()
    {
        throw new NotImplementedException(); 
    }

    public static void DynamicColliderCircle_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii,
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void DynamicColliderCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii, 
        CategorisedOverlapArray<int> colliderCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }
        
            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Dynamic,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }        
    }

    public static void DynamicColliderCircle_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void DynamicColliderCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void DynamicColliderCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }        
    }

    public static void DynamicColliderCircle_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }

    /******************
    
        Kinematic Polygon Collider
    
    *******************/

    public static void KinematicColliderPolygon_To_KinematicColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void KinematicColliderPolygon_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicColliderPolygon_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void KinematicColliderPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void KinematicColliderPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicColliderPolygon_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Kinematic Circle Collider.
    
    *******************/




    public static void KinematicColliderCircle_To_KinematicColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicColliderCircle_To_KinematicColliderCapsule()
    {
        throw new NotImplementedException();
    }

    public static void KinematicColliderCircle_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicColliderCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }
        
            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicColliderCircle_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Trigger Polygon Collider.
    
    *******************/




    public static void TriggerColliderPolygon_To_TriggerColliderPolygon(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerColliderPolygon_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, polyIndex, circIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerColliderPolygon_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Trigger Circle Collider.
    
    *******************/




    public static void TriggerColliderCircle_To_TriggerColliderCircle(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, IntrusiveList.Node[] nodes, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            if(BroadPhase(nodes, minAabbsX, minAabbsY, maxAabbsX, maxAabbsY, ownerIndex, otherIndex) == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerColliderCircle_To_TriggerColliderCapsule()
    {
        throw new NotImplementedException();        
    }
}