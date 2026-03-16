using System;
using System.Runtime.CompilerServices;
using Howl.ECS;
using Howl.Math;
using Howl.Physics;

namespace Howl.Physics;

public class Soa_Collision
{
    /// <summary>
    /// Gets and sets the gen indices of the owner physics body of a collision.
    /// </summary>
    public Soa_GenIndex OwnerGenIndices;

    /// <summary>
    /// Gets and sets the gen indices of the other physics body of a collision.
    /// </summary>
    public Soa_GenIndex OtherGenIndices;

    /// <summary>
    /// Gets and sets the normal vector of a collision.
    /// </summary>
    public Soa_Vector2 Normals;

    /// <summary>
    /// Gets and sets the owner physics body shape center vector of a collision. 
    /// </summary>
    /// <remarks>
    /// Note: the shape must not be transformed in any way, it must be the untransformed shape.
    /// </remarks>
    public Soa_Vector2 OwnerShapeCenters;

    /// <summary>
    /// Gets and sets the other physics body shape center vector of a collision. 
    /// </summary>
    /// <remarks>
    /// Note: the shape must not be transformed in any way, it must be the untransformed shape.
    /// </remarks>
    public Soa_Vector2 OtherShapeCenters;

    /// <summary>
    /// Gets and sets the first contact point of a collision.
    /// </summary>
    public Soa_Vector2 FirstContactPoints;

    /// <summary>
    /// Gets and sets the second contact point of a collision.
    /// </summary>
    public Soa_Vector2 SecondContactPoints;

    /// <summary>
    /// Gets and sets the depth of the collision.
    /// </summary>
    public float[] Depths;

    /// <summary>
    /// Gets and sets a copy of an owner's physics body flags for a collision.
    /// </summary>
    public PhysicsBodyFlags[] OwnerFlags;

    /// <summary>
    /// Gets and sets a copy of an other's physics body flags for a collision.
    /// </summary>
    public PhysicsBodyFlags[] OtherFlags;

    /// <summary>
    /// Gets and sets whether or not a collision has a second contact point.
    /// </summary>
    public bool[] TwoContactPoints;

    /// <summary>
    /// Gets and sets the amount of valid entries; starting from index 0.
    /// </summary>
    public int Count;

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
    public Soa_Collision(int maxCollisions)
    {
        OwnerGenIndices             = new Soa_GenIndex(maxCollisions);
        OtherGenIndices             = new Soa_GenIndex(maxCollisions);
        Normals                     = new Soa_Vector2(maxCollisions);
        OwnerShapeCenters           = new Soa_Vector2(maxCollisions);
        OtherShapeCenters           = new Soa_Vector2(maxCollisions);
        FirstContactPoints          = new Soa_Vector2(maxCollisions);
        SecondContactPoints         = new Soa_Vector2(maxCollisions);
        Depths                       = new float[maxCollisions];
        OwnerFlags                  = new PhysicsBodyFlags[maxCollisions];
        OtherFlags                  = new PhysicsBodyFlags[maxCollisions];
        TwoContactPoints            = new bool[maxCollisions];
    }

    /// <summary>
    /// Appends a collision with a single contact point.
    /// </summary>
    /// <param name="buffer">the SOA collision buffer to append to.</param>
    /// <param name="ownerIndex">the index of the owner.</param>
    /// <param name="ownerGeneration">the generation of the owner.</param>
    /// <param name="otherIndex">the index of the other.</param>
    /// <param name="otherGeneration">the generation of the other.</param>
    /// <param name="normalX">the x-component of the normal vector.</param>
    /// <param name="normalY">the y-component of the normal vector.</param>
    /// <param name="ownerShapeCenterX">the x-component of the owner's shape center vector.</param>
    /// <param name="ownerShapeCenterY">the y-component of the owner's shape center vector.</param>
    /// <param name="otherShapeCenterX">the x-component of the other's shape center vector.</param>
    /// <param name="otherShapeCenterY">the y-component of the other's shape center vector.</param>
    /// <param name="contactPointX">the x-component of the contact point vector.</param>
    /// <param name="contactPointY">the y-component of the contact point vector.</param>
    /// <param name="depth"></param>
    /// <param name="ownerFlags"></param>
    /// <param name="otherFlags"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCollision(
        Soa_Collision buffer, 
        int ownerIndex, 
        int ownerGeneration, 
        int otherIndex,
        int otherGeneration,
        float normalX,
        float normalY,
        float ownerShapeCenterX,
        float ownerShapeCenterY,
        float otherShapeCenterX,
        float otherShapeCenterY,
        float contactPointX,
        float contactPointY,
        float depth,
        PhysicsBodyFlags ownerFlags,
        PhysicsBodyFlags otherFlags
    )
    {
        int count = buffer.Count;
        
        buffer.OwnerGenIndices.Indices[count] = ownerIndex;
        buffer.OwnerGenIndices.Generations[count] = ownerGeneration;
        buffer.OtherGenIndices.Indices[count] = otherIndex;
        buffer.OtherGenIndices.Generations[count] = otherGeneration;
        buffer.Normals.X[count] = normalX;
        buffer.Normals.Y[count] = normalY;
        buffer.OwnerShapeCenters.X[count] = ownerShapeCenterX;
        buffer.OwnerShapeCenters.Y[count] = ownerShapeCenterY;
        buffer.OtherShapeCenters.X[count] = otherShapeCenterX;
        buffer.OtherShapeCenters.Y[count] = otherShapeCenterY;
        buffer.FirstContactPoints.X[count] = contactPointX;
        buffer.FirstContactPoints.Y[count] = contactPointY;
        buffer.Depths[count] = depth;
        buffer.OwnerFlags[count] = ownerFlags;
        buffer.OtherFlags[count] = otherFlags;
        buffer.TwoContactPoints[count] = false;
        
        buffer.Count++;
    }

    /// <summary>
    /// Appends a collision with two contact points.
    /// </summary>
    /// <param name="buffer">the SOA collision buffer to append to.</param>
    /// <param name="ownerIndex">the index of the owner.</param>
    /// <param name="ownerGeneration">the generation of the owner.</param>
    /// <param name="otherIndex">the index of the other.</param>
    /// <param name="otherGeneration">the generation of the other.</param>
    /// <param name="normalX">the x-component of the normal vector.</param>
    /// <param name="normalY">the y-component of the normal vector.</param>
    /// <param name="ownerShapeCenterX">the x-component of the owner's shape center vector.</param>
    /// <param name="ownerShapeCenterY">the y-component of the owner's shape center vector.</param>
    /// <param name="otherShapeCenterX">the x-component of the other's shape center vector.</param>
    /// <param name="otherShapeCenterY">the y-component of the other's shape center vector.</param>
    /// <param name="firstContactPointX">the x-component of the first contact point vector.</param>
    /// <param name="firstContactPointY">the y-component of the first contact point vector.</param>
    /// <param name="secondContactPointX">the x-component of the second contact point vector.</param>
    /// <param name="secondContactPointY">the y-component of the second contact point vector.</param>
    /// <param name="depth">the depth of the collision.</param>
    /// <param name="ownerFlags">the flags of the owner physics body.</param>
    /// <param name="otherFlags">the flags of the other physics body.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCollision(
        Soa_Collision buffer, 
        int ownerIndex, 
        int ownerGeneration, 
        int otherIndex,
        int otherGeneration,
        float normalX,
        float normalY,
        float ownerShapeCenterX,
        float ownerShapeCenterY,
        float otherShapeCenterX,
        float otherShapeCenterY,
        float firstContactPointX,
        float firstContactPointY,
        float secondContactPointX,
        float secondContactPointY,
        float depth,
        PhysicsBodyFlags ownerFlags,
        PhysicsBodyFlags otherFlags
    )
    {
        int count = buffer.Count;
        
        buffer.OwnerGenIndices.Indices[count] = ownerIndex;
        buffer.OwnerGenIndices.Generations[count] = ownerGeneration;
        buffer.OtherGenIndices.Indices[count] = otherIndex;
        buffer.OtherGenIndices.Generations[count] = otherGeneration;
        buffer.Normals.X[count] = normalX;
        buffer.Normals.Y[count] = normalY;
        buffer.OwnerShapeCenters.X[count] = ownerShapeCenterX;
        buffer.OwnerShapeCenters.Y[count] = ownerShapeCenterY;
        buffer.OtherShapeCenters.X[count] = otherShapeCenterX;
        buffer.OtherShapeCenters.Y[count] = otherShapeCenterY;
        buffer.FirstContactPoints.X[count] = firstContactPointX;
        buffer.FirstContactPoints.Y[count] = firstContactPointY;
        buffer.SecondContactPoints.X[count] = secondContactPointX;
        buffer.SecondContactPoints.Y[count] = secondContactPointY;
        buffer.Depths[count] = depth;
        buffer.OwnerFlags[count] = ownerFlags;
        buffer.OtherFlags[count] = otherFlags;
        buffer.TwoContactPoints[count] = true;
        
        buffer.Count++;
    }

    public static void SortByOwnerGenIndex(Soa_Collision collisions)
    {
        throw new NotImplementedException();
    }

    public static void Clear(Soa_Collision soa)
    {
        soa.Count = 0;
    }

}