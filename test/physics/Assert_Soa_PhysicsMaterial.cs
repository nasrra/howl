using Howl.Physics;

namespace Howl.Test.Physics;

public static class Assert_Soa_PhysicsMaterial
{
    /// <summary>
    ///     Asserts the equality of values of an entry in a soa instance.
    /// </summary>
    /// <param name="staticFriction">the expected static friction.</param>
    /// <param name="kineticFriction">the expected kinetic friction.</param>
    /// <param name="density">the expected density.</param>
    /// <param name="restitution">the expected restitution.</param>
    /// <param name="entryIndex">the entry index.</param>
    /// <param name="soa">the soa instance.</param>
    public static void EntryEqual(float staticFriction, float kineticFriction, float density, float restitution, int entryIndex,
        Soa_PhysicsMaterial soa
    )
    {
        Assert.Equal(staticFriction, soa.StaticFriction[entryIndex]);
        Assert.Equal(kineticFriction, soa.KineticFriction[entryIndex]);
        Assert.Equal(density, soa.Density[entryIndex]);
        Assert.Equal(restitution, soa.Restitution[entryIndex]);
    }
}