using Howl.Math;

namespace Howl.Physics;

/// <summary>
///     
/// </summary>
/// <remarks>
///     Remarks: this class is structured in a Fixed-Stride Structure-of-Arrays format.
/// </remarks>
public class CollisionManifoldStateNew
{
    /// <summary>
    ///     The maximum amount of colliders 
    /// </summary>
    /// <remarks>
    ///     Remarks: This is because collisions are stored in a one dimensional array, meaning anything higher than 46340 * 46340 will cause an integer overflow.
    /// </remarks>
    public const int MaxCollisionsPerCollider = 46340;

    /// <summary>
    ///     The indices of the <c>colliding</c> physic body entry in the physics system state.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>collisionIndex</c>.
    /// </remarks>
    public int[] ColliderIndices;

    /// <summary>
    ///     The normal vector of a collision.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>collisionIndex</c>.
    /// </remarks>
    public Soa_Vector2 Normals;

    /// <summary>
    ///     The centroids of the colliding physics bodys. 
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>collisionIndex</c>.
    /// </remarks>
    public Soa_Vector2 ColliderCentroids;

    /// <summary>
    ///     The first contact point of all collisions.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>collisionIndex</c>.
    /// </remarks>
    public Soa_Vector2 FirstContactPoints;

    /// <summary>
    ///     The second contact point of all collisions.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>collisionIndex</c>.
    /// </remarks>
    public Soa_Vector2 SecondContactPoints;

    /// <summary>
    ///     The depth of the collisions.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>collisionIndex</c>.
    /// </remarks>
    public float[] Depths;

    /// <summary>
    ///     Copies of the colliding physics body flags.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>collisionIndex</c>.
    /// </remarks>
    public PhysicsBodyFlags[] ColliderFlags;

    /// <summary>
    ///     Whether or not a collision has a second contact point.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>collisionIndex</c>.
    /// </remarks>
    public bool[] TwoContactPoints;

    /// <summary>
    ///     The amount of collisions appended to an entry.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>colliderIndex</c>.
    /// </remarks>
    public int[] AppendCounts;

    /// <summary>
    ///     The active collision elements in the arrays.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>collisionIndex</c>.
    /// </remarks>
    public StackArray<int> Active;

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
    public CollisionManifoldStateNew(int totalColliders)
    {
        int totalCollisionsPerCollider = totalColliders - 1;

        System.Diagnostics.Debug.Assert(totalCollisionsPerCollider <= MaxCollisionsPerCollider, 
            $"Collision Manifold total collisions per collider '{totalCollisionsPerCollider}' exceeds max collisions per collider  '{MaxCollisionsPerCollider}'"
        );

        Math.Math.Clamp(totalCollisionsPerCollider, 0, MaxCollisionsPerCollider);

        Stride = totalCollisionsPerCollider;
        MaxEntries = totalColliders;
        
        int totalCollisions = totalCollisionsPerCollider * totalColliders;

        AppendCounts                = new int[totalColliders];
        ColliderIndices             = new int[totalCollisions];
        Normals                     = new Soa_Vector2(totalCollisions);
        ColliderCentroids           = new Soa_Vector2(totalCollisions);
        FirstContactPoints          = new Soa_Vector2(totalCollisions);
        SecondContactPoints         = new Soa_Vector2(totalCollisions);
        Depths                      = new float[totalCollisions];
        ColliderFlags               = new PhysicsBodyFlags[totalCollisions];
        TwoContactPoints            = new bool[totalCollisions];
        Active                      = new (totalCollisions);
    }

    ~CollisionManifoldStateNew()
    {
        CollisionManifoldNew.Dispose(this);
    }
}