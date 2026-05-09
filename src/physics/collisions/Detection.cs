using System;
using Howl.Collections;
using Howl.DataStructures.Bvh;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics.Collisions;

public static class Detection
{
    public static (int, int) Polygon_To_Polygon(FsSoa_Vector2 vertices, CollisionManifoldState collisions, 
        int ownerIndex, int otherIndex, float ownerPosX, float otherPosX, float ownerPosY, float otherPosY, ref bool collided
    )
    {
        Span<float> ownerVertsX = default;
        Span<float> ownerVertsY = default;
        Span<float> otherVertsX = default;
        Span<float> otherVertsY = default;

        (int, int) collisionIndices = default;

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


            switch (contactCount)
            {
                case 1:
                    collisionIndices = CollisionManifold.SetDataTwoWay(collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                        normalX, normalY, firstContactPointX, firstContactPointY, depth
                    );
                    break;
                case 2:
                    collisionIndices = CollisionManifold.SetDataTwoWay(collisions, ownerIndex, otherIndex, ownerPosX, ownerPosY, otherPosX, otherPosY, 
                        normalX, normalY, firstContactPointX, firstContactPointY, secondContactPointX, secondContactPointY, depth
                    );
                    break;
            }

            collided = true;
        }
        else
        {
            collided = false;            
        }

        return collisionIndices;
    }




    /******************
    
        Solid Polygon RigidBody.
    
    *******************/




    public static void SolidPolygonRigidBody_To_SolidPolygonRigidBody(Span<int> bvhIndices, CollisionManifoldState collisions, OverlapInfo info, 
        Span<float> centroidsX, Span<float> centroidsY, FsSoa_Vector2 vertices, CategorisedOverlapArray<int> subStepCollisionsToResolve 
    )
    {
                bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

            // detect a collision.
            (int aToB, int bToA) collisionIndices = Polygon_To_Polygon(vertices, collisions, ownerIndex, otherIndex, 
                centroidsX[ownerIndex], centroidsX[otherIndex], centroidsY[ownerIndex], centroidsY[otherIndex], ref collided
            );

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.aToB, subStepCollisionsToResolve, 
                    SubStepResolutionBvhCategory.Solid,
                    SubStepResolutionBvhCategory.Solid
                );
            }
        }
    }

    public static void SolidPolygonRigidBody_To_SolidCircleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_SolidCapsuleRigidbody()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_KinematicPolygonRigidBody(Span<int> bvhIndices, CollisionManifoldState collisions, 
        OverlapInfo info, Span<float> centroidsX, Span<float> centroidsY, FsSoa_Vector2 vertices, 
        CategorisedOverlapArray<int> subStepCollisionsToResolve 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            // detect a collision.
            (int aToB, int bToA) collisionIndices = Polygon_To_Polygon(vertices, collisions, ownerIndex, otherIndex, 
                centroidsX[ownerIndex], centroidsX[otherIndex], centroidsY[ownerIndex], centroidsY[otherIndex], ref collided
            );

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.bToA, subStepCollisionsToResolve, 
                    SubStepResolutionBvhCategory.Solid,
                    SubStepResolutionBvhCategory.Kinematic
                );
            }
        }
    }

    public static void SolidPolygonRigidBody_To_KinematicCircleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_KinematicCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_TriggerPolygonRigidBody(Span<int> bvhIndices, CollisionManifoldState collisions, OverlapInfo info, 
        Span<float> centroidsX, Span<float> centroidsY, FsSoa_Vector2 vertices
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];

            // detect a collision.
            Polygon_To_Polygon(vertices, collisions, ownerIndex, otherIndex, 
                centroidsX[ownerIndex], centroidsX[otherIndex], centroidsY[ownerIndex], centroidsY[otherIndex], ref collided
            );
        }
    }

    public static void SolidPolygonRigidBody_To_TriggerCircleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_TriggerCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_SolidPolygonCollider(Span<int> bvhIndices, CollisionManifoldState collisions, 
        OverlapInfo info, Span<float> centroidsX, Span<float> centroidsY, FsSoa_Vector2 vertices, 
        CategorisedOverlapArray<int> subStepCollisionsToResolve 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            // detect a collision.
            (int aToB, int bToA) collisionIndices = Polygon_To_Polygon(vertices, collisions, ownerIndex, otherIndex, 
                centroidsX[ownerIndex], centroidsX[otherIndex], centroidsY[ownerIndex], centroidsY[otherIndex], ref collided
            );

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.bToA, subStepCollisionsToResolve, 
                    SubStepResolutionBvhCategory.Solid,
                    SubStepResolutionBvhCategory.Solid
                );
            }
        }
    }

    public static void SolidPolygonRigidBody_To_SolidCircleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_SolidCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_KinematicPolygonCollider(Span<int> bvhIndices, CollisionManifoldState collisions, 
        OverlapInfo info, Span<float> centroidsX, Span<float> centroidsY, FsSoa_Vector2 vertices, 
        CategorisedOverlapArray<int> subStepCollisionsToResolve 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            // detect a collision.
            (int aToB, int bToA) collisionIndices = Polygon_To_Polygon(vertices, collisions, ownerIndex, otherIndex, 
                centroidsX[ownerIndex], centroidsX[otherIndex], centroidsY[ownerIndex], centroidsY[otherIndex], ref collided
            );

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.bToA, subStepCollisionsToResolve, 
                    SubStepResolutionBvhCategory.Solid,
                    SubStepResolutionBvhCategory.Kinematic
                );
            }
        }        
    }

    public static void SolidPolygonRigidBody_To_KinematicCircleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_KinematicCapsuleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_TriggerPolygonCollider(Span<int> bvhIndices, CollisionManifoldState collisions, 
        OverlapInfo info, Span<float> centroidsX, Span<float> centroidsY, FsSoa_Vector2 vertices 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            // detect a collision.
            Polygon_To_Polygon(vertices, collisions, ownerIndex, otherIndex, 
                centroidsX[ownerIndex], centroidsX[otherIndex], centroidsY[ownerIndex], centroidsY[otherIndex], ref collided
            );
        }   
    }

    public static void SolidPolygonRigidBody_To_TriggerCircleCollider()
    {
        throw new NotImplementedException();
    }

    public static void SolidPolygonRigidBody_To_TriggerCapsuleCollider()
    {
        throw new NotImplementedException();
    }




    /******************
    
        Kinematic Polygon RigidBody
    
    *******************/




    public static void KinematicPolygonRigidBody_To_KinematicPolygonRigidBody(Span<int> bvhIndices, CollisionManifoldState collisions, 
        OverlapInfo info, Span<float> centroidsX, Span<float> centroidsY, FsSoa_Vector2 vertices 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            // detect a collision.
            Polygon_To_Polygon(vertices, collisions, ownerIndex, otherIndex, 
                centroidsX[ownerIndex], centroidsX[otherIndex], centroidsY[ownerIndex], centroidsY[otherIndex], ref collided
            );
        }           
    }

    public static void KinematicPolygonRigidBody_To_KinematicCircleRigidBody()
    {
        throw new NotImplementedException();
    }


    public static void KinematicPolygonRigidBody_To_KinematicCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public static void KinematicPolygonRigidBody_To_TriggerPolygonRigidBody(Span<int> bvhIndices, CollisionManifoldState collisions, 
        OverlapInfo info, Span<float> centroidsX, Span<float> centroidsY, FsSoa_Vector2 vertices 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            // detect a collision.
            Polygon_To_Polygon(vertices, collisions, ownerIndex, otherIndex, 
                centroidsX[ownerIndex], centroidsX[otherIndex], centroidsY[ownerIndex], centroidsY[otherIndex], ref collided
            );
        }
    }
    
    public static void KinematicPolygonRigidBody_To_TriggerCircleRigidBody()
    {
        throw new NotImplementedException();
    }
    
    public static void KinematicPolygonRigidBody_To_TriggerCapsuleRigidBody()
    {
        throw new NotImplementedException();
    }

    public const int SolidPolygonCollider       = 9;

    public static void KinematicPolygonRigidBody_To_SolidPolygonCollider(Span<int> bvhIndices, CollisionManifoldState collisions, 
        OverlapInfo info, Span<float> centroidsX, Span<float> centroidsY, FsSoa_Vector2 vertices, 
        CategorisedOverlapArray<int> subStepCollisionsToResolve 
    )
    {
        bool collided = false;

        for(int i = 0; i < info.Length; i++)
        {            
            // get the owner and other data.
            int ownerIndex = bvhIndices[info.OwnerLeafIndices[i]];
            int otherIndex = bvhIndices[info.OtherLeafIndices[i]];
            
            // detect a collision.
            (int aToB, int bToA) collisionIndices = Polygon_To_Polygon(vertices, collisions, ownerIndex, otherIndex, 
                centroidsX[ownerIndex], centroidsX[otherIndex], centroidsY[ownerIndex], centroidsY[otherIndex], ref collided
            );

            // resolve the collision.
            if (collided == true)
            {
                CategorisedOverlapArray.Append(collisionIndices.bToA, subStepCollisionsToResolve, 
                    SubStepResolutionBvhCategory.Kinematic,
                    SubStepResolutionBvhCategory.Solid
                );
            }
        }  
    }

    public const int SolidCircleCollider        = 10;
    public const int SolidCapsuleCollider       = 11;
    public const int KinematicPolygonCollider   = 12;
    public const int KinematicCircleCollider    = 13;
    public const int KinematicCapsuleCollider   = 14;
    public const int TriggerPolygonCollider     = 15;
    public const int TriggerCircleCollider      = 16;
    public const int TriggerCapsuleCollider     = 17;
}