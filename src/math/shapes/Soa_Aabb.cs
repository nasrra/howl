using System;

namespace Howl.Math.Shapes;

public class Soa_Aabb : IDisposable
{
    /// <summary>
    /// The x-components of the minimum vertex.
    /// </summary>
    public float[] MinX;

    /// <summary>
    /// the y-components of the minimum vertex.
    /// </summary>
    public float[] MinY;
    
    /// <summary>
    /// the x-components of the maximum vertex.
    /// </summary>
    public float[] MaxX;

    /// <summary>
    /// The y-components of the maximum vertex.
    /// </summary>
    public float[] MaxY;

    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new Structure-Of-Array's Axis-Aligned Bounding-Box.
    /// </summary>
    /// <param name="capacity">the capacity of the backing arrays.</param>
    public Soa_Aabb(int capacity)
    {
        MinX = new float[capacity];
        MinY = new float[capacity];
        MaxX = new float[capacity];
        MaxY = new float[capacity];
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_Aabb soa)
    {
        if(soa.Disposed)
            return;
        
        soa.Disposed = true;

        soa.MinX = null;
        soa.MinY = null;
        soa.MaxX = null;
        soa.MaxY = null;

        GC.SuppressFinalize(soa);
    }

    ~Soa_Aabb()
    {
        Dispose(this);
    }
}