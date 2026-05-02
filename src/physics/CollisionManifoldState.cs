using System;
using Howl.Math;

namespace Howl.Physics;

/// <summary>
///     
/// </summary>
/// <remarks>
///     Remarks: this class is structured in a Fixed-Stride Structure-of-Arrays format.
/// </remarks>
public class CollisionManifoldState
{

    /// <summary>
    ///     The normal vector of a collision.
    /// </summary>
    public Soa_Vector2 Normals;

    /// <summary>
    ///     The centroids of the colliding physics bodys. 
    /// </summary>
    public Soa_Vector2 ColliderCentroids; // this should be removed and use the physics system state centroids instead.

    /// <summary>
    ///     The first contact point of all collisions.
    /// </summary>
    public Soa_Vector2 FirstContactPoints;

    /// <summary>
    ///     The second contact point of all collisions.
    /// </summary>
    public Soa_Vector2 SecondContactPoints;

    /// <summary>
    ///     The depth of the collisions.
    /// </summary>
    public float[] Depths;

    /// <summary>
    ///     Copies of the colliding physics body flags.
    /// </summary>
    public PhysicsBodyFlags[] ColliderFlags;

    /// <summary>
    ///     Whether or not a collision has a second contact point.
    /// </summary>
    public bool[] TwoContactPoints;

    /// <summary>
    ///     The indices of <c>active</c> collision elements separated by <c>entry</c> in the current step.
    /// </summary>
    /// <remarks>
    ///     Remarks: this array is a fixed-stride swapback array.
    /// </remarks>
    public int[] ActiveIndices;

    /// <summary>
    ///     The count of indices an entry has in the <c>ActiveIndices</c> fixed stride swapwback array.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>entryIndex</c>.
    /// </remarks>
    public int[] ActiveIndicesCount;

    /// <summary>
    ///     The <c>phase</c> a collision element is of being <c>active</c>.
    /// </summary>
    /// <remarks>
    ///     Value Key:
    ///     <list type = "bullet">
    ///         <item>0: the element has not been active at all.</item>
    ///         <item>1: the element is active in the <c>current</c> step; meaning there is contact between the two colliders.</item>
    ///         <item>2: the element is active in the <c>previous</c> step; meaning the contact between the two colliders has just stopped.</item>
    ///         <item>3: the element is active in the <c>preultimate</c> step; meaning the contact between the two colliders has completely ceased.</item>
    ///     </list>
    /// </remarks>
    public int[] ActivePhase;

    /// <summary>
    ///     The state of all collisions this step.
    /// </summary>
    public ContactState[] ContactStates;

    /// <summary>
    ///     The state of all collisions in the previous step.
    /// </summary>
    public ContactState[] PreviousContactStates;

    /// <summary>
    ///     The fixed stride of each entry.
    /// </summary>
    public int Stride;

    /// <summary>
    ///     The amount of entries this collection can hold.
    /// </summary>
    public int MaxEntries;

    /// <summary>
    ///     Whether this instance has been disposed of.
    /// </summary>
    public bool Disposed;




    /******************
    
        Contstructor & Finaliser.
    
    *******************/




    /// <summary>
    /// Creates a Structure-Of-Arrays Collision instance.
    /// </summary>
    /// <param name="owner">The owner of this collision.</param>
    /// <param name="other">The other collider of this collision.</param>
    /// <param name="ownerParameters">the owner's parameters.</param>
    /// <param name="otherParameters">the other's parameters.</param>
    /// <param name="xContactPoints">the x-positional value for the contact points.</param>
    /// <param name="yContactPoints">the y-positional value for the contact points.</param>
    /// <param name="ownerColliderShapeCenter">the center of the owner's collider shape</param>
    /// <param name="otherColliderShapeCenter">the center of the other's collider shape</param>
    /// <param name="normalY">the normal of the collision.</param>
    /// <param name="depth">the depth of the collision.</param>
    public CollisionManifoldState(int totalColliders)
    {
        System.Diagnostics.Debug.Assert(totalColliders <= Constants.MaxColliders, 
            $"Collision Manifold total colliders '{totalColliders}' exceeds max collisions colliders  '{Constants.MaxColliders}'"
        );

        Math.Math.Clamp(totalColliders, 0, Constants.MaxColliders);

        Stride = totalColliders;
        MaxEntries = totalColliders;
        int dataLength = Stride * MaxEntries;

        Normals                     = new Soa_Vector2(dataLength);
        ColliderCentroids           = new Soa_Vector2(dataLength);
        FirstContactPoints          = new Soa_Vector2(dataLength);
        SecondContactPoints         = new Soa_Vector2(dataLength);
        Depths                      = new float[dataLength];
        ColliderFlags               = new PhysicsBodyFlags[dataLength];
        TwoContactPoints            = new bool[dataLength];
        ContactStates               = new ContactState[dataLength];
        PreviousContactStates       = new ContactState[dataLength];
        ActivePhase                 = new int[dataLength];
        ActiveIndices               = new int[dataLength];
        ActiveIndicesCount          = new int[totalColliders];
    }

    ~CollisionManifoldState()
    {
        CollisionManifold.Dispose(this);
    }
}