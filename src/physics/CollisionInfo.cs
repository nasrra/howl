using System;
using System.Collections.Generic;
using Howl.Math;

namespace Howl.Physics;

public ref struct CollisionInfo
{
    ref float NormalX;
    ref float NormalY;
    ref float FirstContactPointX;
    ref float FirstContactPointY;
    ref float SecondContactPointX;
    ref float SecondContactPointY;
    ref float Depth;
    ref bool TwoContactPoints;
    public GenId ColliderPhysicsId;
    public GenId ColliderEntityId;

    public CollisionInfo(ref float normalX, ref float normalY, ref float firstContactPointX, ref float firstContactPointY, 
        ref float secondContactPointX, ref float secondContactPointY, ref float depth, ref bool twoContactPoints, 
        GenId colliderPhysicsId, GenId colliderEntityId
    )
    {
        NormalX = ref normalX;
        NormalY = ref normalY;
        FirstContactPointX = ref firstContactPointX;
        FirstContactPointY = ref firstContactPointY;
        SecondContactPointX = ref secondContactPointX;
        SecondContactPointY = ref secondContactPointY;
        Depth = ref depth;
        TwoContactPoints = ref twoContactPoints;
        ColliderPhysicsId = colliderPhysicsId;
        ColliderPhysicsId = colliderEntityId;
    }
}