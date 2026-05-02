using Howl.Physics;

namespace Howl.Test.Physics;

public static class Assert_CollisionManifoldState
{
    /// <summary>
    ///     Asserts the equality of a state instance's collision data element.
    /// </summary>
    /// <param name="colliderIndex">the expected collider index.</param>
    /// <param name="normalX">the expected x-component of the normal.</param>
    /// <param name="normalY">the expected y-component of the normal.</param>
    /// <param name="colliderCentroidX">the expected x-component of the collider centroid.</param>
    /// <param name="colliderCentroidY">the expected y-component of the collider centroid.</param>
    /// <param name="firstContactPointX">the expected x-component of the contact point.</param>
    /// <param name="firstContactPointY">the expected y-component of the contact point.</param>
    /// <param name="depth">the expected depth.</param>
    /// <param name="colliderFlag">the expected collider flag.</param>
    /// <param name="twoContactPoints">the expected two contact points bool.</param>
    /// <param name="elementIndex">the index of the element to assert against.</param>
    /// <param name="state">the state instance that contains the element.</param>
    public static void ElementEqual(float normalX, float normalY, float colliderCentroidX, float colliderCentroidY, 
        float contactPointX, float contactPointY, float depth, PhysicsBodyFlags colliderFlag, bool twoContactPoints,
        int elementIndex, CollisionManifoldState state
    )
    {
        Assert.Equal(normalX, state.Normals.X[elementIndex]);
        Assert.Equal(normalY, state.Normals.Y[elementIndex]);
        Assert.Equal(colliderCentroidX, state.ColliderCentroids.X[elementIndex]);
        Assert.Equal(colliderCentroidY, state.ColliderCentroids.Y[elementIndex]);
        Assert.Equal(contactPointX, state.FirstContactPoints.X[elementIndex]);
        Assert.Equal(contactPointY, state.FirstContactPoints.Y[elementIndex]);
        Assert.Equal(depth, state.Depths[elementIndex]);
        Assert.Equal(colliderFlag, state.ColliderFlags[elementIndex]);
        Assert.Equal(twoContactPoints, state.TwoContactPoints[elementIndex]);
    }

    /// <summary>
    ///     Asserts the equality of a state instance's collision data element.
    /// </summary>
    /// <param name="normalX">the expected x-component of the normal.</param>
    /// <param name="normalY">the expected y-component of the normal.</param>
    /// <param name="colliderCentroidX">the expected x-component of the collider centroid.</param>
    /// <param name="colliderCentroidY">the expected y-component of the collider centroid.</param>
    /// <param name="firstContactPointX">the expected x-component of the first contact point.</param>
    /// <param name="firstContactPointY">the expected y-component of the first contact point.</param>
    /// <param name="secondContactPointX">the expected x-component of the second contact point.</param>
    /// <param name="secondContactPointY">the expected y-component of the second contact point.</param>
    /// <param name="depth">the expected depth.</param>
    /// <param name="colliderFlag">the expected collider flag.</param>
    /// <param name="twoContactPoints">the expected two contact points bool.</param>
    /// <param name="elementIndex">the index of the element to assert against.</param>
    /// <param name="state">the state instance that contains the element.</param>
    public static void ElementEqual(float normalX, float normalY, float colliderCentroidX, float colliderCentroidY, 
        float firstContactPointX, float firstContactPointY, float secondContactPointX, float secondContactPointY,
        float depth, PhysicsBodyFlags colliderFlag, bool twoContactPoints, int elementIndex, CollisionManifoldState state
    )
    {
        Assert.Equal(normalX, state.Normals.X[elementIndex]);
        Assert.Equal(normalY, state.Normals.Y[elementIndex]);
        Assert.Equal(colliderCentroidX, state.ColliderCentroids.X[elementIndex]);
        Assert.Equal(colliderCentroidY, state.ColliderCentroids.Y[elementIndex]);
        Assert.Equal(firstContactPointX, state.FirstContactPoints.X[elementIndex]);
        Assert.Equal(firstContactPointY, state.FirstContactPoints.Y[elementIndex]);
        Assert.Equal(secondContactPointX, state.SecondContactPoints.X[elementIndex]);
        Assert.Equal(secondContactPointY, state.SecondContactPoints.Y[elementIndex]);
        Assert.Equal(depth, state.Depths[elementIndex]);
        Assert.Equal(colliderFlag, state.ColliderFlags[elementIndex]);
        Assert.Equal(twoContactPoints, state.TwoContactPoints[elementIndex]);
    }

    public static void Equal(float[] normalsX, float[] normalsY, float[] colliderCentroidsX, float[] colliderCentroidsY, 
        float[] firstContactPointsX, float[] firstContactPointsY, float[] secondContactPointsX, float[] secondContactPointsY,
        float[] depths, PhysicsBodyFlags[] colliderFlags, bool[] twoContactPoints, int[] activeCollisions, int[] activeIndices, 
        int[] activeIndicesCount, CollisionManifoldState state
    )
    {
        Assert.Equal(normalsX, state.Normals.X);
        Assert.Equal(normalsY, state.Normals.Y);
        Assert.Equal(colliderCentroidsX, state.ColliderCentroids.X);
        Assert.Equal(colliderCentroidsY, state.ColliderCentroids.Y);
        Assert.Equal(firstContactPointsX, state.FirstContactPoints.X);
        Assert.Equal(firstContactPointsY, state.FirstContactPoints.Y);
        Assert.Equal(secondContactPointsX, state.SecondContactPoints.X);
        Assert.Equal(secondContactPointsY, state.SecondContactPoints.Y);
        Assert.Equal(depths, state.Depths);
        Assert.Equal(colliderFlags, state.ColliderFlags);
        Assert.Equal(twoContactPoints, state.TwoContactPoints);
        Assert.Equal(activeCollisions, state.ActivePhase);
        Assert.Equal(activeIndices, state.ActiveIndices);
        Assert.Equal(activeIndicesCount, state.ActiveIndicesCount);
    }

    public static void Disposed(CollisionManifoldState state)
    {
        Assert.True(state.Disposed);
        Assert.Null(state.Normals);
        Assert.Null(state.ColliderCentroids);
        Assert.Null(state.FirstContactPoints);
        Assert.Null(state.SecondContactPoints); 
        Assert.Null(state.Depths);
        Assert.Null(state.ColliderFlags);
        Assert.Null(state.TwoContactPoints);
        Assert.Null(state.ActiveIndices);
        Assert.Null(state.ActiveIndicesCount);
        Assert.Null(state.ActivePhase);
        Assert.Null(state.ContactStates);
        Assert.Null(state.PreviousContactStates);
        Assert.Equal(0, state.Stride);
        Assert.Equal(0, state.MaxEntries);
    }
}