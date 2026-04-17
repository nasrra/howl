using Howl.Math;
using Howl.Physics;
using Howl.Test.Math;

namespace Howl.Test.Physics;

public class Test_PhysicsSystemState
{
    [Fact]
    public void EnforceNil_Test()
    {
        Debug.Log.Suppress = true;

        PhysicsSystemState state = new(12, 12);
        
        int insertIndex = 0;
        
        state.Flags[insertIndex] = PhysicsBodyFlags.Allocated;
        state.LocalVertices.X[insertIndex] = 2;
        state.LocalVertices.Y[insertIndex] = 3;
        state.WorldVertices.X[insertIndex] = 2;
        state.WorldVertices.Y[insertIndex] = 3;
        Soa_Transform.Insert(state.Transforms, insertIndex, 1, 2, 3, 4, 5, 6);
        Soa_Vector2.Insert(state.Forces, insertIndex, 1, 2);
        Soa_Vector2.Insert(state.LinearVelocities, insertIndex, 2, 3);
        Soa_Vector2.Insert(state.Centroids, insertIndex, 4, 5);
        Soa_Vector2.Insert(state.MaxAABBVertices, insertIndex, 12, 32);
        Soa_Vector2.Insert(state.MinAABBVertices, insertIndex, 5, 34);
        Soa_PhysicsMaterial.Insert(state.PhysicsMaterials, insertIndex, 0.12f, 0.1f, 2f, 1f);
        state.AngularVelocities[insertIndex] = 43;
        state.Masses[insertIndex] = 123;
        state.InverseMasses[insertIndex] = 32;
        state.Masses[insertIndex] = 54;
        state.InverseMasses[insertIndex] = 89;
        state.LocalWidths[insertIndex] = 43;
        state.LocalHeights[insertIndex] = 98;
        state.LocalRadii[insertIndex] = 9;
        state.WorldRadii[insertIndex] = 32;
        state.RotationalInertia[insertIndex] = 1;
        state.InverseRotationalInertia[insertIndex] = 99;
        state.Generations[insertIndex] = 3;

        PhysicsSystemState.EnforceNil(state);

        Assert.Equal(PhysicsBodyFlags.None, state.Flags[insertIndex]);
        state.LocalVertices.X[insertIndex] = 0;
        state.LocalVertices.Y[insertIndex] = 0;
        state.WorldVertices.X[insertIndex] = 0;
        state.WorldVertices.Y[insertIndex] = 0;
        Assert_Soa_Transform.EntryEqual(0,0,0,0,0,0,4,insertIndex, state.Transforms);
        Assert_Soa_Vector2.EntryEqual(0, 0, insertIndex, state.Forces);
        Assert_Soa_Vector2.EntryEqual(0, 0, insertIndex, state.LinearVelocities);
        Assert_Soa_Vector2.EntryEqual(0, 0, insertIndex, state.Centroids);
        Assert_Soa_Vector2.EntryEqual(0, 0, insertIndex, state.MaxAABBVertices);
        Assert_Soa_Vector2.EntryEqual(0, 0, insertIndex, state.MinAABBVertices);
        Assert_Soa_PhysicsMaterial.EntryEqual(0,0,0,0,insertIndex, state.PhysicsMaterials);
        Assert.Equal(0, state.AngularVelocities[insertIndex]);
        Assert.Equal(0, state.Masses[insertIndex]);
        Assert.Equal(0, state.InverseMasses[insertIndex]);
        Assert.Equal(0, state.LocalWidths[insertIndex]);
        Assert.Equal(0, state.LocalHeights[insertIndex]);
        Assert.Equal(0, state.LocalRadii[insertIndex]);
        Assert.Equal(0, state.WorldRadii[insertIndex]);
        Assert.Equal(0, state.RotationalInertia[insertIndex]);
        Assert.Equal(0, state.InverseRotationalInertia[insertIndex]);        
        Assert.Equal(0, state.Generations[insertIndex]);

        Debug.Log.Suppress = false;
    }

}