using Howl.DataStructures;

namespace Howl.Test.Physics;

public static class SoaSpatialPairHelpers
{
    public static void AssertEntry(Soa_SpatialPair soa, int index, int ownerIndex, int ownerGeneration, int otherIndex,
        int otherGeneration, byte ownerFlags, byte otherFlags)
    {
        Assert.Equal(ownerFlags, soa.OwnerFlags[index]);
        Assert.Equal(otherFlags, soa.OtherFlags[index]);
        Assert.Equal(ownerIndex, soa.OwnerGenIndices.Indices[index]);
        Assert.Equal(otherIndex, soa.OtherGenIndices.Indices[index]);
        Assert.Equal(ownerGeneration, soa.OwnerGenIndices.Generations[index]);
        Assert.Equal(otherGeneration, soa.OtherGenIndices.Generations[index]);
    }
}