using System;
using System.Runtime.CompilerServices;

namespace Howl.Physics;

public class Soa_PhysicsMaterial : IDisposable
{
    /// <summary>
    /// The static friction values.
    /// </summary>
    /// <remarks>
    /// Static friction is the resistance of motion before an object is sliding / is already in motion.
    /// </remarks>
    public float[] StaticFriction;

    /// <summary>
    /// The kinetic friction values.
    /// </summary>
    /// <remarks>
    /// Kinetic friction is the resistance of motion when an object is sliding / within motion and in contact with another object.
    /// </remarks>
    public float[] KineticFriction;

    /// <summary>
    /// The density values.
    /// </summary>
    public float[] Density;

    /// <summary>
    /// The restituion values.
    /// </summary>
    /// <remarks>
    /// Restitution is how 'bouncy' a body is; specifically the opposing force applied when an object collides with another.
    /// </remarks>
    public float[] Restitution;

    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new Structure-Of-Arrays Physics Material instance.
    /// </summary>
    /// <param name="capacity">the capacity of the backing arrays.</param>
    public Soa_PhysicsMaterial(int capacity)
    {
        StaticFriction = new float[capacity];
        KineticFriction = new float[capacity];
        Density = new float[capacity];
        Restitution = new float[capacity];
    }

    /// <summary>
    /// Sets a physic's material's values.
    /// </summary>
    /// <remarks>
    /// All spans will be mutated with the newly set values.
    /// All spans must be the same length; as entries are associated via <paramref name="index"/>.
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
    public static void SetPhysicsMaterial(Span<float> staticFrictions, Span<float> kineticFrictions, Span<float> densities, Span<float> restitutions,
        float staticFriction, float kineticFriction, float density, float restitution, int index
    )
    {
        staticFrictions[index] = staticFriction;
        kineticFrictions[index] = kineticFriction;
        densities[index] = density;
        restitutions[index] = restitution;
    }

    /// <summary>
    /// Sets a physics material's values in a soa physics material.
    /// </summary>
    /// <param name="soa">the soa physics material to mutate.</param>
    /// <param name="material">the material value to set to.</param>
    /// <param name="index">the index of the entry to modify.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetPhysicsMaterial(Soa_PhysicsMaterial soa, PhysicsMaterial material, int index)
    {
        SetPhysicsMaterial(soa.StaticFriction, soa.KineticFriction, soa.Density, soa.Restitution,
            material.StaticFriction, material.KineticFriction, material.Density, material.Restitution, index
        );  
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

        GC.SuppressFinalize(soa);
    }

    public void Dispose()
    {
        Dispose(this);
    }

    ~Soa_PhysicsMaterial()
    {
        Dispose(this);       
    }
}