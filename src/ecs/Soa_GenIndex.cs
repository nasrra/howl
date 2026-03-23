using System;

namespace Howl.ECS;

public class Soa_GenIndex : IDisposable
{
    /// <summary>
    /// Gets and sets the indices.
    /// </summary>
    public int[] Indices;

    /// <summary>
    /// Gets and sets the generations.
    /// </summary>
    public int[] Generations;

    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new Structure-Of-Array's GenIndex instance.
    /// </summary>
    /// <param name="capacity">the capacity of the underling array's.</param>
    public Soa_GenIndex(int capacity)
    {
        Indices = new int[capacity];
        Generations = new int[capacity];
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_GenIndex soa)
    {
        if(soa.Disposed)
            return;

        soa.Disposed = true;
        
        soa.Indices = null;
        soa.Generations = null;

        GC.SuppressFinalize(soa);
    }

    ~Soa_GenIndex()
    {
        Dispose(this);
    }
}