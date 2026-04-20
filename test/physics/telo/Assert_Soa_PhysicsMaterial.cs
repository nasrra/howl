using Howl.Physics.Telo;

namespace Howl.Test.Physics.Telo;

public static class Assert_Soa_PhysicsMaterial
{
    /// <summary>
    ///     Asserts the length of a soa instance's backing arrays.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="soa">the soa instance.</param>
    public static void LengthEqual(int length, Soa_PhysicsMaterial soa)
    {
        Assert.Equal(length, soa.StaticFriction.Length);
        Assert.Equal(length, soa.KineticFriction.Length);
        Assert.Equal(length, soa.Density.Length);
        Assert.Equal(length, soa.Restitution.Length);
    }

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

    /// <summary>
    ///     Asserts that a soa instance is disposed.
    /// </summary>
    /// <param name="soa">the soa instance.</param>
    public static void Disposed(Soa_PhysicsMaterial soa)
    {
        Assert.Null(soa.StaticFriction);
        Assert.Null(soa.KineticFriction);
        Assert.Null(soa.Density);
        Assert.Null(soa.Restitution);
        Assert.True(soa.Length == 0);
        Assert.True(soa.Disposed);
    }
}