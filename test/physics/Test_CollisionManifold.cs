using Howl.Physics;

namespace Howl.Test.Physics;

public class Test_CollisionManifold
{

    [Fact]
    public void AppendOneWay_SuccessTest()
    {
        CollisionManifoldStateNew state = new(3);

        // == expected values ==.

        int[] expectedColliderIndex         = [ 0,  0,  1,  1, -1, -1];
        float[] expectedNormalX             = [ 0,  0,  2,  9, -2, -9];
        float[] expectedNormalY             = [ 0,  0,  3,  8, -3, -8];
        float[] expectedCentroidX           = [ 0,  0,  4,  7, -4, -7];
        float[] expectedCentroidY           = [ 0,  0,  5,  6, -5, -6];
        float[] expectedFirstContactPointX  = [ 0,  0,  6,  5, -6, -5];
        float[] expectedFirstContactPointY  = [ 0,  0,  7,  4, -7, -4];
        float[] expectedSecondContactPointX = [ 0,  0,  0,  3, -0, -3];
        float[] expectedSecondContactPointY = [ 0,  0,  0,  2, -0, -2];
        float[] expectedDepths              = [ 0,  0,  1,  1, -1, -1];
        PhysicsBodyFlags[] expectedColliderFlags = [
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.Active, 
            PhysicsBodyFlags.Active,
            PhysicsBodyFlags.Allocated,
            PhysicsBodyFlags.Allocated
        ];
        bool[] expectedTwoContactPoints = [false, false, false, true, false, true];
        int[] expectedAppendCounts = [0,2,2];

        // == call functions ==.

        Assert.True(CollisionManifoldNew.AppendOneWay(state, 1, expectedColliderIndex[2], expectedCentroidX[2], expectedCentroidY[2], 
            expectedNormalX[2], expectedNormalY[2], expectedFirstContactPointX[2], expectedFirstContactPointY[2], expectedDepths[2], 
            expectedColliderFlags[2]
        ));

        Assert.True(CollisionManifoldNew.AppendOneWay(state, 1, expectedColliderIndex[3], expectedCentroidX[3], expectedCentroidY[3], 
            expectedNormalX[3], expectedNormalY[3], expectedFirstContactPointX[3], expectedFirstContactPointY[3], 
            expectedSecondContactPointX[3], expectedSecondContactPointY[3], expectedDepths[3], expectedColliderFlags[3]
        ));

        Assert.True(CollisionManifoldNew.AppendOneWay(state, 2, expectedColliderIndex[4], expectedCentroidX[4], expectedCentroidY[4], 
            expectedNormalX[4], expectedNormalY[4], expectedFirstContactPointX[4], expectedFirstContactPointY[4], expectedDepths[4], 
            expectedColliderFlags[4]
        ));

        Assert.True(CollisionManifoldNew.AppendOneWay(state, 2, expectedColliderIndex[5], expectedCentroidX[5], expectedCentroidY[5], 
            expectedNormalX[5], expectedNormalY[5], expectedFirstContactPointX[5], expectedFirstContactPointY[5], 
            expectedSecondContactPointX[5], expectedSecondContactPointY[5], expectedDepths[5], expectedColliderFlags[5]
        ));

        // == assert written data ==.

        Assert_CollisionManifoldState.Equal(expectedColliderIndex, expectedNormalX, expectedNormalY, expectedCentroidX, expectedCentroidY, 
            expectedFirstContactPointX, expectedFirstContactPointY, expectedSecondContactPointX, expectedSecondContactPointY,
            expectedDepths, expectedAppendCounts, expectedColliderFlags, expectedTwoContactPoints, state
        );
    }

    [Fact]
    public void AppendOneWay_FailTest()
    {
        CollisionManifoldStateNew state = new(2);

        int expectedColliderIndex         = 1;
        float expectedNormalX             = 5;
        float expectedNormalY             = 9;
        float expectedCentroidX           = 13;
        float expectedCentroidY           = 17;
        float expectedFirstContactPointX  = 21;
        float expectedFirstContactPointY  = 25;
        float expectedDepths              = 33;
        PhysicsBodyFlags expectedColliderFlags = PhysicsBodyFlags.Active;
        bool expectedTwoContactPoints = false;
        float expectedAppendCounts = 1;

        
        Assert.True(CollisionManifoldNew.AppendOneWay(state, 0, expectedColliderIndex, expectedCentroidX, expectedCentroidY, expectedNormalX, 
            expectedNormalY, expectedFirstContactPointX, expectedFirstContactPointY, expectedDepths, expectedColliderFlags
        ));

        Assert.False(CollisionManifoldNew.AppendOneWay(state, 0, 0,0,0,0,0,0,0,0,0)); 

        // ensure data wasnt overwritten.
        Assert.Equal(expectedAppendCounts, state.AppendCounts[0]);
        Assert_CollisionManifoldState.ElementEqual(expectedColliderIndex, expectedNormalX, expectedNormalY, expectedCentroidX, 
            expectedCentroidY, expectedFirstContactPointX, expectedFirstContactPointY, expectedDepths, expectedColliderFlags, 
            expectedTwoContactPoints, 0, state
        );

        Assert.False(CollisionManifoldNew.AppendOneWay(state, 0, 0,0,0,0,0,0,0,0,0,0,0));  

        // ensure data wasnt overwritten.
        Assert.Equal(expectedAppendCounts, state.AppendCounts[0]);
        Assert_CollisionManifoldState.ElementEqual(expectedColliderIndex, expectedNormalX, expectedNormalY, expectedCentroidX, 
            expectedCentroidY, expectedFirstContactPointX, expectedFirstContactPointY, expectedDepths, expectedColliderFlags, 
            expectedTwoContactPoints, 0, state
        );
    }

    [Fact]
    public void AppendTwoWay_SuccessTest()
    {
        CollisionManifoldStateNew state = new(3);

        // == expected values ==.

        int[] expectedColliderIndex         = [ 0,  0,  2,  2,  1,  1];
        float[] expectedCentroidX           = [ 0,  0,  1,  9,  3, 10];
        float[] expectedCentroidY           = [ 0,  0,  2,  8,  4, 11];
        float[] expectedNormalX             = [ 0,  0,  3,  7, -3, -7];
        float[] expectedNormalY             = [ 0,  0,  4,  6, -4, -6];
        float[] expectedFirstContactPointX  = [ 0,  0,  5,  5,  5,  5];
        float[] expectedFirstContactPointY  = [ 0,  0,  6,  4,  6,  4];
        float[] expectedSecondContactPointX = [ 0,  0,  7,  0,  7,  0];
        float[] expectedSecondContactPointY = [ 0,  0,  8,  0,  8,  0];
        float[] expectedDepths              = [ 0,  0,  9,  1,  9,  1];
        PhysicsBodyFlags[] expectedColliderFlags = [
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.Active, 
            PhysicsBodyFlags.Active, 
            PhysicsBodyFlags.Allocated, 
            PhysicsBodyFlags.Allocated, 
        ];
        bool[] expectedTwoContactPoints = [false, false, true, false, true, false];
        int[] expectedAppendCounts = [0,2,2];
        
        // == call functions ==.
 
        Assert.True(CollisionManifoldNew.AppendTwoWay(state, indexA: 1, indexB: 2, centroidXA: 3, centroidYA: 4, 
            centroidXB: 1, centroidYB: 2, normalX: 3, normalY: 4, firstContactPointX: 5, firstContactPointY: 6, 
            secondContactPointX: 7, secondContactPointY: 8, depth: 9, flagsA: PhysicsBodyFlags.Allocated, flagsB: PhysicsBodyFlags.Active
        ));
        Assert.True(CollisionManifoldNew.AppendTwoWay(state, indexA: 1, indexB: 2, centroidXA: 10, centroidYA: 11, 
            centroidXB: 9, centroidYB: 8, normalX: 7, normalY: 6, contactPointX: 5, contactPointY: 4, 
            depth: 1, flagsA: PhysicsBodyFlags.Allocated, flagsB: PhysicsBodyFlags.Active
        ));

        Assert_CollisionManifoldState.Equal(expectedColliderIndex, expectedNormalX, expectedNormalY, expectedCentroidX, expectedCentroidY, 
            expectedFirstContactPointX, expectedFirstContactPointY, expectedSecondContactPointX, expectedSecondContactPointY,
            expectedDepths, expectedAppendCounts, expectedColliderFlags, expectedTwoContactPoints, state
        );
    }
}