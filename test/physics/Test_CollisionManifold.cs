using Howl.Physics;

namespace Howl.Test.Physics;

public class Test_CollisionManifold
{

    [Fact]
    public void SetDataOneWay_SuccessTest()
    {
        int expectedIndex = 0;
        CollisionManifoldState state = new(3);

        // == expected values ==.

        float[] expectedNormalX             = [ 0,  0,  0,  2,  0,  9,  -2, -9,  0];
        float[] expectedNormalY             = [ 0,  0,  0,  3,  0,  8,  -3, -8,  0];
        float[] expectedCentroidX           = [ 0,  0,  0,  4,  0,  7,  4,  7,  0];
        float[] expectedCentroidY           = [ 0,  0,  0,  5,  0,  6,  5,  6,  0];
        float[] expectedFirstContactPointX  = [ 0,  0,  0,  6,  0,  5,  6,  5,  0];
        float[] expectedFirstContactPointY  = [ 0,  0,  0,  7,  0,  4,  7,  4,  0];
        float[] expectedSecondContactPointX = [ 0,  0,  0,  0,  0,  3,  0,  3,  0];
        float[] expectedSecondContactPointY = [ 0,  0,  0,  0,  0,  2,  0,  2,  0];
        float[] expectedDepths              = [ 0,  0,  0,  1,  0,  1,  1,  1,  0];
        int[] expectedActiveCollisions      = [ 0,  0,  0,  1,  0,  1,  1,  1,  0];
        int[] expectedActiveIndices         = [ 0,  0,  0,  3,  5,  0,  6,  7,  0];
        int[] expectedActiveCounts          = [0,2,2];
        PhysicsBodyFlags[] expectedColliderFlags = [
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.Active,
            PhysicsBodyFlags.None,
            PhysicsBodyFlags.Active,
            PhysicsBodyFlags.Allocated,
            PhysicsBodyFlags.Allocated,
            PhysicsBodyFlags.None
        ];
        bool[] expectedTwoContactPoints = [false, false, false, false, false, true, false, true, false];


        // == call functions ==.

        expectedIndex = 3;
        CollisionManifold.SetDataOneWay(state, 1, 0, expectedCentroidX[expectedIndex], 
            expectedCentroidY[expectedIndex], expectedNormalX[expectedIndex], expectedNormalY[expectedIndex], 
            expectedFirstContactPointX[expectedIndex], expectedFirstContactPointY[expectedIndex], expectedDepths[expectedIndex], 
            expectedColliderFlags[expectedIndex]
        );

        expectedIndex = 5;
        CollisionManifold.SetDataOneWay(state, 1, 2, expectedCentroidX[expectedIndex], 
            expectedCentroidY[expectedIndex], expectedNormalX[expectedIndex], expectedNormalY[expectedIndex], 
            expectedFirstContactPointX[expectedIndex], expectedFirstContactPointY[expectedIndex], 
            expectedSecondContactPointX[expectedIndex], expectedSecondContactPointY[expectedIndex], expectedDepths[expectedIndex], 
            expectedColliderFlags[expectedIndex]
        );

        expectedIndex = 6;
        CollisionManifold.SetDataOneWay(state, 2, 0, expectedCentroidX[expectedIndex], 
            expectedCentroidY[expectedIndex], expectedNormalX[expectedIndex], expectedNormalY[expectedIndex], 
            expectedFirstContactPointX[expectedIndex], expectedFirstContactPointY[expectedIndex], expectedDepths[expectedIndex], 
            expectedColliderFlags[expectedIndex]
        );

        expectedIndex = 7;
        CollisionManifold.SetDataOneWay(state, 2, 1, expectedCentroidX[expectedIndex], 
            expectedCentroidY[expectedIndex], expectedNormalX[expectedIndex], expectedNormalY[expectedIndex], 
            expectedFirstContactPointX[expectedIndex], expectedFirstContactPointY[expectedIndex], expectedSecondContactPointX[expectedIndex], 
            expectedSecondContactPointY[expectedIndex], expectedDepths[expectedIndex], expectedColliderFlags[expectedIndex]
        );

        // == assert written data ==.
    
        Assert_CollisionManifoldState.Equal(expectedNormalX, expectedNormalY, expectedCentroidX, expectedCentroidY, 
            expectedFirstContactPointX, expectedFirstContactPointY, expectedSecondContactPointX, expectedSecondContactPointY, expectedDepths, 
            expectedColliderFlags, expectedTwoContactPoints, expectedActiveCollisions, expectedActiveIndices, expectedActiveCounts, state
        );
    }

    [Fact]
    public void SetDataTwoWay_SuccessTest()
    {
        CollisionManifoldState state = new(3);

        // == expected values ==.

        float[] expectedCentroidX           = [ 0,  5,  0,  4,  0,  1,  0,  4,  0];
        float[] expectedCentroidY           = [ 0,  6,  0,  5,  0,  2,  0,  3,  0];
        float[] expectedNormalX             = [ 0,  -9, 0,  9,  0,  4,  0, -4,  0];
        float[] expectedNormalY             = [ 0,  -8, 0,  8,  0,  3,  0, -3,  0];
        float[] expectedFirstContactPointX  = [ 0,  3,  0,  3,  0,  5,  0,  5,  0];
        float[] expectedFirstContactPointY  = [ 0,  4,  0,  4,  0,  4,  0,  4,  0];
        float[] expectedSecondContactPointX = [ 0,  0,  0,  0,  0,  3,  0,  3,  0];
        float[] expectedSecondContactPointY = [ 0,  0,  0,  0,  0,  2,  0,  2,  0];
        float[] expectedDepths              = [ 0,  1,  0,  1,  0,  2,  0,  2,  0];
        int[] expectedActiveCollisions      = [ 0,  1,  0,  1,  0,  1,  0,  1,  0];
        int[] expectedActiveIndices         = [ 1,  0,  0,  3,  5,  0,  7,  0,  0];
        int[] expectedActiveCounts          = [1,2,1];
        PhysicsBodyFlags[] expectedColliderFlags = [
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.Allocated,
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.Active,
            PhysicsBodyFlags.None, 
            PhysicsBodyFlags.Active,
            PhysicsBodyFlags.None,
            PhysicsBodyFlags.Allocated,
            PhysicsBodyFlags.None
        ];
        bool[] expectedTwoContactPoints = [false, false, false, false, false, true, false, true, false];
        
        // == call functions ==.
        
        CollisionManifold.SetDataTwoWay(state, indexA: 1, indexB: 0, centroidXA: 5, centroidYA: 6, 
            centroidXB: 4, centroidYB: 5, normalX: 9, normalY: 8, contactPointX: 3, contactPointY: 4, 
            depth: 1, flagsA: PhysicsBodyFlags.Allocated, flagsB: PhysicsBodyFlags.Active
        );

        CollisionManifold.SetDataTwoWay(state, indexA: 1, indexB: 2, centroidXA: 4, centroidYA: 3, centroidXB: 1, 
            centroidYB: 2, normalX: 4, normalY: 3, firstContactPointX: 5, firstContactPointY: 4, secondContactPointX: 3, 
            secondContactPointY: 2, depth: 2, flagsA: PhysicsBodyFlags.Allocated, flagsB: PhysicsBodyFlags.Active
        );

        // == assert written data ==.
    
        Assert_CollisionManifoldState.Equal(expectedNormalX, expectedNormalY, expectedCentroidX, expectedCentroidY, 
            expectedFirstContactPointX, expectedFirstContactPointY, expectedSecondContactPointX, expectedSecondContactPointY, expectedDepths, 
            expectedColliderFlags, expectedTwoContactPoints, expectedActiveCollisions, expectedActiveIndices, expectedActiveCounts, state
        );
    }

    [Fact]
    public static void CompleteStep_Test()
    {
        CollisionManifoldState state = new(3);

        int[] expectedActivePhases;  
        int[] expectedActiveIndices;
        int[] expectedActiveCounts;
        ContactState[] expectedStates;

        // == first step ==.

        state.ActivePhase           = [0,1,0,1,0,1,0,1,0];
        state.ActiveIndices         = [1,0,0,3,5,0,7,0,0];
        state.ActiveIndicesCount    = [1,2,1];

        expectedActivePhases        = [0,2,0,2,0,2,0,2,0];  
        expectedActiveIndices       = [1,0,0,3,5,0,7,0,0];
        expectedActiveCounts        = [1,2,1];
        expectedStates = [
            ContactState.None,
            ContactState.Enter,
            ContactState.None,
            ContactState.Enter,
            ContactState.None,
            ContactState.Enter,
            ContactState.None,
            ContactState.Enter,
            ContactState.None,
        ];
        
        CollisionManifold.CompleteStep(state);

        Assert.Equal(expectedActivePhases, state.ActivePhase);
        Assert.Equal(expectedActiveIndices, state.ActiveIndices);
        Assert.Equal(expectedActiveCounts, state.ActiveIndicesCount);
        Assert.Equal(expectedStates, state.ContactStates);

        CollisionManifold.PrepareForNextStep(state);
        
        // == second step ==.

        state.ActivePhase           = [0,2,0,2,0,1,0,1,0];

        expectedActivePhases        = [0,3,0,3,0,2,0,2,0];  
        expectedActiveCounts        = [1,2,1];
        expectedStates = [
            ContactState.None,
            ContactState.Exit,
            ContactState.None,
            ContactState.Exit,
            ContactState.None,
            ContactState.Sustain,
            ContactState.None,
            ContactState.Sustain,
            ContactState.None,
        ];

        
        CollisionManifold.CompleteStep(state);

        Assert.Equal(expectedActivePhases, state.ActivePhase);
        Assert.Equal(expectedActiveIndices, state.ActiveIndices);
        Assert.Equal(expectedActiveCounts, state.ActiveIndicesCount);
        Assert.Equal(expectedStates, state.ContactStates);

        CollisionManifold.PrepareForNextStep(state);

        // == third step ==.
        state.ActivePhase           = [0,1,0,3,0,2,0,1,0];

        expectedActivePhases        = [0,2,0,0,0,3,0,2,0];  
        expectedActiveIndices       = [1,0,0,5,5,0,7,0,0];// note index 3 performs swapback with index 4.
        expectedActiveCounts        = [1,1,1];
        expectedStates = [
            ContactState.None,
            ContactState.Enter,
            ContactState.None,
            ContactState.None,
            ContactState.None,
            ContactState.Exit,
            ContactState.None,
            ContactState.Sustain,
            ContactState.None,
        ];

        CollisionManifold.CompleteStep(state);

        Assert.Equal(expectedActivePhases, state.ActivePhase);
        Assert.Equal(expectedActiveIndices, state.ActiveIndices);
        Assert.Equal(expectedActiveCounts, state.ActiveIndicesCount);
        Assert.Equal(expectedStates, state.ContactStates);
    }

    [Fact]
    public void Disposed_Test()
    {
        for(int i = 0; i < 12; i++)
        {
            CollisionManifoldState state = new(i);
            CollisionManifold.Dispose(state);
            Assert_CollisionManifoldState.Disposed(state);
        }
    }
}