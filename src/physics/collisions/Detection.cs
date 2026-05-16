using System;
using System.Runtime.CompilerServices;
using Howl.Collections;
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




    /******************
    
        Solid Polygon RigidBody.
    
    *******************/




    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SolidPolygonRigidBody_To_SolidPolygonRigidBody(OverlapInfo info, Span<int> bvhIndices, CollisionManifoldState collisions, 
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

            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }
    }

    public static void SolidPolygonRigidBody_To_SolidCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, float[] maxAabbsX, float[] maxAabbsY, 
        FsSoa_Vector2 vertices, Span<float> radii, CategorisedOverlapArray<int> colliderCollisionsToResolve, CategorisedOverlapArray<int> rigidBodyCollisionsToResolve
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }
    }

    public static void SolidPolygonRigidBody_To_SolidCapsuleRigidbody()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_KinematicPolygonRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void SolidPolygonRigidBody_To_KinematicCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void SolidPolygonRigidBody_To_KinematicCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_TriggerPolygonRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }
    }

    public static void SolidPolygonRigidBody_To_TriggerCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void SolidPolygonRigidBody_To_TriggerCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_SolidPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> subStepCollisionsToResolve 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, subStepCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }
    }

    public static void SolidPolygonRigidBody_To_SolidCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }
    }

    public static void SolidPolygonRigidBody_To_SolidCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_KinematicPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> subStepCollisionsToResolve 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, subStepCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }        
    }

    public static void SolidPolygonRigidBody_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }    
    }

    public static void SolidPolygonRigidBody_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }   
    }

    public static void SolidPolygonRigidBody_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }     
    }

    public static void SolidPolygonRigidBody_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Solid Circle RigidBody.
    
    *******************/


    public static void SolidCircleRigidBody_To_SolidCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }        
    }

    public static void SolidCircleRigidBody_To_SolidCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void SolidCircleRigidBody_To_KinematicPolygonRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );

                CategorisedOverlapArray.Append(collisionIndices.BToA, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void SolidCircleRigidBody_To_KinematicCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );

                CategorisedOverlapArray.Append(collisionIndices.AToB, rigidBodyCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }                
    } 

    public static void SolidCircleRigidBody_To_KinematicCapsuleRigidBody()
    {
        throw new NotImplementedException();        
    }

    public static void SolidCircleRigidBody_To_TriggerPolygonRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }
    
    public static void SolidCircleRigidBody_To_TriggerCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }                
    } 

    public static void SolidCircleRigidBody_To_TriggerCapsuleRigidBody()
    {
        throw new NotImplementedException();
    } 

    public static void SolidCircleRigidBody_To_SolidPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }
    }

    public static void SolidCircleRigidBody_To_SolidCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }          
    }

    public static void SolidCircleRigidBody_To_SolidCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidCircleRigidBody_To_KinematicPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void SolidCircleRigidBody_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void SolidCircleRigidBody_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidCircleRigidBody_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void SolidCircleRigidBody_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void SolidCircleRigidBody_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Kinematic Polygon RigidBody
    
    *******************/




    public static void KinematicPolygonRigidBody_To_KinematicPolygonRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }           
    }

    public static void KinematicPolygonRigidBody_To_KinematicCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
        }
    }


    public static void KinematicPolygonRigidBody_To_KinematicCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void KinematicPolygonRigidBody_To_TriggerPolygonRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }
    }
    
    public static void KinematicPolygonRigidBody_To_TriggerCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
        }    
    }
    
    public static void KinematicPolygonRigidBody_To_TriggerCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void KinematicPolygonRigidBody_To_SolidPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid
                );
            }
        }  
    }

    public static void KinematicPolygonRigidBody_To_SolidCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void KinematicPolygonRigidBody_To_SolidCapsuleCollider()
    {
        throw new NotImplementedException();        
    }

    public static void KinematicPolygonRigidBody_To_KinematicPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }          
    } 

    public static void KinematicPolygonRigidBody_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }


    public static void KinematicPolygonRigidBody_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void KinematicPolygonRigidBody_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void KinematicPolygonRigidBody_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicPolygonRigidBody_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    } 




    /******************
    
        Kinematic Circle RigidBody.
    
    *******************/




    public static void KinematicCircleRigidBody_To_KinematicCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicCircleRigidBody_To_KinematicCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void KinematicCircleRigidBody_To_TriggerPolygonRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicCircleRigidBody_To_TriggerCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicCircleRigidBody_To_TriggerCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void KinematicCircleRigidBody_To_SolidPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void KinematicCircleRigidBody_To_SolidCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);

            if (collided)
            {
                CategorisedOverlapArray.Append(collisionIndices.BToA, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void KinematicCircleRigidBody_To_SolidCapsuleCollider()
    {
        throw new NotImplementedException();        
    }

    public static void KinematicCircleRigidBody_To_KinematicPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicCircleRigidBody_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicCircleRigidBody_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void KinematicCircleRigidBody_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicCircleRigidBody_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicCircleRigidBody_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Trigger Polygon Rigidbody.   
    
    *******************/



    public static void TriggerPolygonRigidBody_To_TriggerPolygonRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerPolygonRigidBody_To_TriggerCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void TriggerPolygonRigidBody_To_SolidPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerPolygonRigidBody_To_SolidCircleCollider (OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerPolygonRigidBody_To_SolidCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void TriggerPolygonRigidBody_To_KinematicPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerPolygonRigidBody_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerPolygonRigidBody_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void TriggerPolygonRigidBody_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerPolygonRigidBody_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerPolygonRigidBody_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Trigger Circle RigidBody.
    
    *******************/




    public static void TriggerCircleRigidBody_To_TriggerCircleRigidBody(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerCircleRigidBody_To_TriggerCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void TriggerCircleRigidBody_To_SolidPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerCircleRigidBody_To_SolidCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerCircleRigidBody_To_SolidCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void TriggerCircleRigidBody_To_KinematicPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerCircleRigidBody_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerCircleRigidBody_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void TriggerCircleRigidBody_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerCircleRigidBody_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerCircleRigidBody_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Solid Polygon Collider.
    
    *******************/





    public static void SolidPolygonCollider_To_SolidPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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

            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }
    }

    public static void SolidPolygonCollider_To_SolidCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }
    }

    public static void SolidPolygonCollider_To_SolidCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonCollider_To_KinematicPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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

            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void SolidPolygonCollider_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void SolidPolygonCollider_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonCollider_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }
    }

    public static void SolidPolygonCollider_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );
        }
    }

    public static void SolidPolygonCollider_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Solid Circle Collider
    
    *******************/




    public static void SolidCircleCollider_To_SolidCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Solid
                );
            }
        }        
    }

    public static void SolidCircleCollider_To_SolidCapsuleCollider()
    {
        throw new NotImplementedException(); 
    }

    public static void SolidCircleCollider_To_KinematicPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
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
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }
    }

    public static void SolidCircleCollider_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
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
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }
        
            // detect a collision.
            CollisionIndexPair collisionIndices = Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);
    
            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.AToB, colliderCollisionsToResolve, 
                    CollisionResolutionCategory.Solid,
                    CollisionResolutionCategory.Kinematic
                );
            }
        }        
    }

    public static void SolidCircleCollider_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidCircleCollider_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void SolidCircleCollider_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }        
    }

    public static void SolidCircleCollider_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    /******************
    
        Kinematic Polygon Collider
    
    *******************/

    public static void KinematicPolygonCollider_To_KinematicPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void KinematicPolygonCollider_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicPolygonCollider_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void KinematicPolygonCollider_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void KinematicPolygonCollider_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int circIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int polyIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicPolygonCollider_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Kinematic Circle Collider.
    
    *******************/




    public static void KinematicCircleCollider_To_KinematicCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicCircleCollider_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void KinematicCircleCollider_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void KinematicCircleCollider_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int otherIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int ownerIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }
        
            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void KinematicCircleCollider_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Trigger Polygon Collider.
    
    *******************/




    public static void TriggerPolygonCollider_To_TriggerPolygonCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Polygon(collisions, vertices, centroidsX, centroidsY, ownerIndex, otherIndex, ref collided);
        }        
    }

    public static void TriggerPolygonCollider_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, FsSoa_Vector2 vertices, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int polyIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int circIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[polyIndex], minAabbsX[circIndex], minAabbsY[polyIndex], minAabbsY[circIndex], 
                maxAabbsX[polyIndex], maxAabbsX[circIndex], maxAabbsY[polyIndex], maxAabbsY[circIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Polygon_To_Circle(collisions, vertices, 
                centroidsX, centroidsY, radii, polyIndex, circIndex, ref collided
            );    
        }
    }

    public static void TriggerPolygonCollider_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Trigger Circle Collider.
    
    *******************/




    public static void TriggerCircleCollider_To_TriggerCircleCollider(OverlapInfo info, Span<int> bvhIndices, 
        CollisionManifoldState collisions, float[] centroidsX, float[] centroidsY, float[] minAabbsX, float[] minAabbsY, 
        float[] maxAabbsX, float[] maxAabbsY, Span<float> radii
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
        
            bool broadPhasePassed = Aabb.Intersect(
                minAabbsX[ownerIndex], minAabbsX[otherIndex], minAabbsY[ownerIndex], minAabbsY[otherIndex], 
                maxAabbsX[ownerIndex], maxAabbsX[otherIndex], maxAabbsY[ownerIndex], maxAabbsY[otherIndex]
            ); 

            if (broadPhasePassed == false)
            {
                continue;
            }

            // detect a collision.
            Circle_To_Circle(collisions, centroidsX, centroidsY, radii, ownerIndex, otherIndex, ref collided);    
        }
    }

    public static void TriggerCircleCollider_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();        
    }
}