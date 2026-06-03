
using System;
using System.Runtime.CompilerServices;
using Howl.Collections;

namespace Howl.Physics;

public class Soa_PhysicsMaterial
{
    public float[] StaticFriction;
    public float[] KineticFriction;
    public float[] Density;
    public float[] Restitution;
    public int Length;
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
    ///     <para>Remarks:</para>
    ///     <para>All arrays will be mutated with the newly set values.</para>
    ///     <para>All arrays must be the same length; as entries are associated via <paramref name="insertIndex"/>.</para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(float[] staticFrictions, float[] kineticFrictions, float[] densities, float[] restitutions, 
        float staticFriction, float kineticFriction, float density, float restitution,
        int insertIndex
    )
    {
        PhysicsMaterial.AssertKineticFrictionInRange(kineticFriction);
        PhysicsMaterial.AssertStaticFrictionInRange(staticFriction, kineticFriction);
        PhysicsMaterial.AssertRestitutionInRange(restitution);
        PhysicsMaterial.AssertDensityInRange(density);

        staticFrictions[insertIndex] = staticFriction;
        kineticFrictions[insertIndex] = kineticFriction;
        densities[insertIndex] = density;
        restitutions[insertIndex] = restitution;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_PhysicsMaterial soa, int insertIndex, float staticFriction, float kineticFriction, float density, 
        float restitution
    )
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
    /// <param name="insertIndex">the index of the entry to modify.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_PhysicsMaterial soa, PhysicsMaterial material, int insertIndex)
    {
        Insert(soa.StaticFriction, soa.KineticFriction, soa.Density, soa.Restitution,
            material.StaticFriction, material.KineticFriction, material.Density, material.Restitution, insertIndex
        );  
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_PhysicsMaterial soa, float staticFriction, float kineticFriction, float density, float restitution, int index
    )
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