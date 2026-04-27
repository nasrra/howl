using System;
using System.Runtime.CompilerServices;
using Howl.Collections;

namespace Howl.Physics;

public class Soa_PhysicsMaterial
{
    /// <summary>
    ///     The static friction values.
    /// </summary>
    /// <remarks>
    ///     Static friction is the resistance of motion before an object is sliding / is already in motion.
    /// </remarks>
    public float[] StaticFriction;

    /// <summary>
    ///     The kinetic friction values.
    /// </summary>
    /// <remarks>
    ///     Kinetic friction is the resistance of motion when an object is sliding / within motion and in contact with another object.
    /// </remarks>
    public float[] KineticFriction;

    /// <summary>
    ///     The density values.
    /// </summary>
    public float[] Density;

    /// <summary>
    ///     The restituion values.
    /// </summary>
    /// <remarks>
    ///     Restitution is how 'bouncy' a body is; specifically the opposing force applied when an object collides with another.
    /// </remarks>
    public float[] Restitution;

    /// <summary>
    ///     The total number of elements in all the dimensions of the backing arrays; zero if there are no elements in the backing arrays.
    /// </summary>
    public int Length;

    /// <summary>
    ///     Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new Structure-Of-Arrays Physics Material instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public Soa_PhysicsMaterial(int length)
    {
        StaticFriction = new float[length];
        KineticFriction = new float[length];
        Density = new float[length];
        Restitution = new float[length];
        Length = length;
    }

    /// <summary>
    ///     Inserts a physics material's values into a soa instance.
    /// </summary>
    /// <remarks>
    ///     All spans will be mutated with the newly set values.
    ///     All spans must be the same length; as entries are associated via <paramref name="index"/>.
    /// </remarks>
    /// <param name="staticFrictions">the span that stores static friction values.</param>
    /// <param name="kineticFrictions">the span that stores kinetic friction values.</param>
    /// <param name="densities">the span that stores density values.</param>
    /// <param name="restitutions">the span that stores restitution values.</param>
    /// <param name="staticFriction">the static friction value to set to.</param>
    /// <param name="kineticFriction">the kinetic friction value to set to.</param>
    /// <param name="density">the density to set to.</param>
    /// <param name="restitution">the restitution to set to.</param>
    /// <param name="index">the index of the entry to modify.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Span<float> staticFrictions, Span<float> kineticFrictions, Span<float> densities, Span<float> restitutions,
        float staticFriction, float kineticFriction, float density, float restitution, int index
    )
    {
        PhysicsMaterial.AssertKineticFrictionInRange(kineticFriction);
        PhysicsMaterial.AssertStaticFrictionInRange(staticFriction, kineticFriction);
        PhysicsMaterial.AssertRestitutionInRange(restitution);
        PhysicsMaterial.AssertDensityInRange(density);

        staticFrictions[index] = staticFriction;
        kineticFrictions[index] = kineticFriction;
        densities[index] = density;
        restitutions[index] = restitution;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_PhysicsMaterial soa, int insertIndex, float staticFriction, float kineticFriction, float density, float restitution)
    {
        PhysicsMaterial.AssertKineticFrictionInRange(kineticFriction);
        PhysicsMaterial.AssertStaticFrictionInRange(staticFriction, kineticFriction);
        PhysicsMaterial.AssertRestitutionInRange(restitution);
        PhysicsMaterial.AssertDensityInRange(density);

        soa.StaticFriction[insertIndex] = staticFriction;
        soa.KineticFriction[insertIndex] = kineticFriction;
        soa.Density[insertIndex] = density;
        soa.Restitution[insertIndex] = restitution;
    }

    /// <summary>
    ///     Inserts a physics material's values into a soa instance.
    /// </summary>
    /// <param name="soa">the soa instance to insert into.</param>
    /// <param name="material">the material value to set to.</param>
    /// <param name="index">the index of the entry to modify.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_PhysicsMaterial soa, PhysicsMaterial material, int index)
    {
        Insert(soa.StaticFriction, soa.KineticFriction, soa.Density, soa.Restitution,
            material.StaticFriction, material.KineticFriction, material.Density, material.Restitution, index
        );  
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_PhysicsMaterial soa, float staticFriction, float kineticFriction, float density, float restitution, int index)
    {
        Insert(soa.StaticFriction, soa.KineticFriction, soa.Density, soa.Restitution,
            staticFriction, kineticFriction, density, restitution, index
        );  
    }

    /// <summary>
    ///     Enforces a <c>Nil</c> entry for all underlying arrays in the soa instance.
    /// </summary>
    /// <param name="soa"></param>
    public static void EnforceNil(Soa_PhysicsMaterial soa)
    {
        Nil.Enforce(soa.StaticFriction);
        Nil.Enforce(soa.KineticFriction);
        Nil.Enforce(soa.Density);
        Nil.Enforce(soa.Restitution);
    }




    /*******************
    
        Disposal.
    
    ********************/




    public static void Dispose(Soa_PhysicsMaterial soa)
    {
        if(soa.Disposed)
            return;
        
        soa.Disposed = true;
        soa.StaticFriction = null;
        soa.KineticFriction = null;
        soa.Density = null;
        soa.Restitution = null;
        soa.Length = 0;

        GC.SuppressFinalize(soa);
    }

    ~Soa_PhysicsMaterial()
    {
        Dispose(this);       
    }
}