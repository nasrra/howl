
namespace Howl.Physics;


/// <summary>
///     Note: ordering matters here, value 0 is highest precedence in <see cref="PhysicsSystem.FormatCategorisedOverlaps(Howl.CategorisedLeafOverlaps, System.Span{int}, System.Span{int})"/>.
/// </summary>
public static class BvhCategory
{
    public const int SolidPolygonRigidBody      = 0;
    public const int SolidCircleRigidBody       = 1;
    public const int SolidCapsuleRigidBody      = 2;
    public const int KinematicPolygonRigidBody  = 3;
    public const int KinematicCircleRigidBody   = 4;
    public const int KinematicCapsuleRigidBody  = 5;
    public const int TriggerPolygonRigidBody    = 6;
    public const int TriggerCircleRigidBody     = 7;
    public const int TriggerCapsuleRigidBody    = 8;
    public const int SolidPolygonCollider       = 9;
    public const int SolidCircleCollider        = 10;
    public const int SolidCapsuleCollider       = 11;
    public const int KinematicPolygonCollider   = 12;
    public const int KinematicCircleCollider    = 13;
    public const int KinematicCapsuleCollider   = 14;
    public const int TriggerPolygonCollider     = 15;
    public const int TriggerCircleCollider      = 16;
    public const int TriggerCapsuleCollider     = 17;




    /******************
    
        Util
    
    *******************/




    public const int Count = 18;
}