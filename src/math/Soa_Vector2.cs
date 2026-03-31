
using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public class Soa_Vector2 : IDisposable
{
    /// <summary>
    /// Gets and sets the x-coordinate values.
    /// </summary>
    public float[] X;

    /// <summary>
    /// Gets and sets the y-coordinate values.
    /// </summary>
    public float[] Y;

    /// <summary>
    /// The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;

    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new SoaVector2 instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public Soa_Vector2(int length)
    {
        X = new float[length];
        Y = new float[length];
        Length = length;
    }

    /// <summary>
    /// Inserts an entry into a soa instance.
    /// </summary>
    /// <param name="soa">the soa instance.</param>
    /// <param name="insertIndex">the index in the soa to insert into.</param>
    /// <param name="x">the x-component of the vector to append.</param>
    /// <param name="y">the y-component of the vector to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_Vector2 soa, int insertIndex, float x, float y)
    {
        soa.X[insertIndex] = x;
        soa.Y[insertIndex] = y;
    }

    /// <summary>
    /// Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
    /// </summary>
    /// <param name="soa">the soa instance to append to.</param>
    /// <param name="x">the x-component of the vector to append.</param>
    /// <param name="y">the y-component of the vector to append.</param>
    public static void Append(Soa_Vector2 soa, float x, float y)
    {
        Insert(soa, soa.AppendCount, x,y);
        soa.AppendCount++;
    }

    /// <summary>
    /// Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa instance to reset.</param>
    public static void ResetCount(Soa_Vector2 soa)
    {
        soa.AppendCount = 0;
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_Vector2 soa)
    {
        if (soa.Disposed)
        {
            return;
        }

        soa.Disposed = true;
        soa.X = null;
        soa.Y = null;
        soa.Length = 0;
        
        GC.SuppressFinalize(soa);
    }

    ~Soa_Vector2()
    {
        Dispose(this);
    }
}