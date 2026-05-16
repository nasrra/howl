
namespace Howl.Physics;


/// <summary>
///     Note: ordering matters here, value 0 is highest precedence in <see cref="PhysicsSystem.FormatCategorisedOverlaps(Howl.CategorisedLeafOverlaps, System.Span{int}, System.Span{int})"/>.
/// </summary>
public static class PhysicsBodyCategory
{
    public const int SolidPolygonRigidBody      = 0;
    public const int SolidCircleRigidBody       = 1;
    public const int SolidCapsuleRigidBody      = 2;
    
    public const int TriggerPolygonRigidBody    = 3;
    public const int TriggerCircleRigidBody     = 4;
    public const int TriggerCapsuleRigidBody    = 5;

    // note: everything greater than KinematicPolygonRigidBody
    // is not apart of the rigid body movement step.

    public const int KinematicPolygonRigidBody  = 6;
    public const int KinematicCircleRigidBody   = 7;
    public const int KinematicCapsuleRigidBody  = 8;
    
    public const int SolidPolygonCollider       = 9;
    public const int SolidCircleCollider        = 10;
    public const int SolidCapsuleCollider       = 11;
    
    public const int TriggerPolygonCollider     = 12;
    public const int TriggerCircleCollider      = 13;
    public const int TriggerCapsuleCollider     = 14;

    public const int KinematicPolygonCollider   = 15;
    public const int KinematicCircleCollider    = 16;
    public const int KinematicCapsuleCollider   = 17;



    /******************
    
        Util
    
    *******************/




    public const int Count = 18;
}