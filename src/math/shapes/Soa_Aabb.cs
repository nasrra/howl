using System;
using System.Numerics;
using System.Runtime.CompilerServices;

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
    /// Creates a new Structure-Of-Array's Axis-Aligned Bounding-Box.
    /// </summary>
    /// <param name="length">the capacity of the backing arrays.</param>
    public Soa_Aabb(int length)
    {
        MinX = new float[length];
        MinY = new float[length];
        MaxX = new float[length];
        MaxY = new float[length];
        Length = length;
    }

    /// <summary>
    /// Inserts an entry into a soa instance.
    /// </summary>
    /// <param name="soa">the soa aabb to insert into.</param>
    /// <param name="insertIndex">the index in the soa arrays to insert into.</param>
    /// <param name="minX">the x-component of the minimum vertex.</param>
    /// <param name="minY">the y-component of the minimum vertex.</param>
    /// <param name="maxX">the x-component of the maximum vertex.</param>
    /// <param name="maxY">the y-component of the maximum vertex.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_Aabb soa, int insertIndex, float minX, float minY, float maxX, float maxY)
    {
        soa.MinX[insertIndex] = minX;
        soa.MinY[insertIndex] = minY;
        soa.MaxX[insertIndex] = maxX;
        soa.MaxY[insertIndex] = maxY;   
    }

    /// <summary>
    /// Appends an entry into an soa at the soa's <c>AppendCount</c> index.
    /// </summary>
    /// <param name="soa">the soa aabb to insert into.</param>
    /// <param name="minX">the x-component of the minimum vertex.</param>
    /// <param name="minY">the y-component of the minimum vertex.</param>
    /// <param name="maxX">the x-component of the maximum vertex.</param>
    /// <param name="maxY">the y-component of the maximum vertex.</param>
    public static void Append(Soa_Aabb soa, float minX, float minY, float maxX, float maxY)
    {
        Insert(soa, soa.AppendCount, minX, minY, maxX, maxY);
        soa.AppendCount++;
    }

    /// <summary>
    /// Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa instance to reset.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResetCount(Soa_Aabb soa)
    {
        soa.AppendCount = 0;
    }

    /// <summary>
    /// Calculates the centroids of aabb's in a soa aabb using SISD.
    /// </summary>
    /// <remarks>
    /// The length of <paramref name="x"/> and <paramref name="y"/> must be equal to the capacity of the soa aabb.
    /// </remarks>
    /// <param name="soa">the soa aabb with the aabb's to get the centroids of.</param>
    /// <param name="x">output span for calculated the x-component of the centroid vectors.</param>
    /// <param name="y">output span for calculated the y-component of the centroid vectors.</param>
    /// <param name="startIndex">the entry index in the soa aabb to start at.</param>
    /// <param name="length">the amount of aabb's to get the centroid of from the starting index.</param>
    public static void CalculateCentroids_Sisd(Soa_Aabb soa, Span<float> x, Span<float> y, int startIndex, int length)
    {
        Span<float> minX = soa.MinX;
        Span<float> minY = soa.MinY;
        Span<float> maxX = soa.MaxX;
        Span<float> maxY = soa.MaxY;

        for(int i = startIndex; i < length; i++)
        {
            Aabb.CalculateCentroid(minX[i], minY[i], maxX[i], maxY[i], out x[i], out y[i]);
        }
    }

    /// <summary>
    /// Calculates the centroids of aabb's in a soa aabb using SIMD.
    /// </summary>
    /// <remarks>
    /// The length of <paramref name="x"/> and <paramref name="y"/> must be equal to the capacity of the soa aabb.
    /// </remarks>
    /// <param name="soa">the soa aabb with the aabb's to get the centroids of.</param>
    /// <param name="x">output span for calculated the x-component of the centroid vectors.</param>
    /// <param name="y">output span for calculated the y-component of the centroid vectors.</param>
    /// <param name="startIndex">the entry index in the soa aabb to start at.</param>
    /// <param name="length">the amount of aabb's to get the centroid of from the starting index.</param>
    /// <param name="tailindex">output for the index the simd operation stopped at.</param>
    public static void CalculateCentroids_Simd(Soa_Aabb soa, Span<float> x, Span<float> y, int startIndex, int length, ref int tailindex)
    {
        Span<float> minX = soa.MinX;
        Span<float> minY = soa.MinY;
        Span<float> maxX = soa.MaxX;
        Span<float> maxY = soa.MaxY;

        int simdSize = System.Numerics.Vector<float>.Count;
        int i = startIndex; 
        for(; i <= length - simdSize; i+= simdSize)
        {
            System.Numerics.Vector<float> vMinX = System.Numerics.Vector.LoadUnsafe(ref minX[i]);
            System.Numerics.Vector<float> vMinY = System.Numerics.Vector.LoadUnsafe(ref minY[i]);
            System.Numerics.Vector<float> vMaxX = System.Numerics.Vector.LoadUnsafe(ref maxX[i]);
            System.Numerics.Vector<float> vMaxY = System.Numerics.Vector.LoadUnsafe(ref maxY[i]);
            System.Numerics.Vector<float> vCentroidX = (vMaxX + vMinX) * 0.5f;
            System.Numerics.Vector<float> vCentroidY = (vMaxY + vMinY) * 0.5f;
            vCentroidX.StoreUnsafe(ref x[i]);
            vCentroidY.StoreUnsafe(ref y[i]);
        }
        tailindex = i;
    }

    /// <summary>
    /// Calculates the centroids of aabb's in a soa aabb.
    /// </summary>
    /// <remarks>
    /// The length of <paramref name="x"/> and <paramref name="y"/> must be equal to the capacity of the soa aabb.
    /// </remarks>
    /// <param name="soa">the soa aabb with the aabb's to get the centroids of.</param>
    /// <param name="x">output span for calculated the x-component of the centroid vectors.</param>
    /// <param name="y">output span for calculated the y-component of the centroid vectors.</param>
    /// <param name="startIndex">the entry index in the soa aabb to start at.</param>
    /// <param name="length">the amount of aabb's to get the centroid of from the starting index.</param>
    public static void CalculateCentroids(Soa_Aabb soa, Span<float> x, Span<float> y, int startIndex, int length)
    {
        int simdTailIndex = 0;

        // perform simd.
        CalculateCentroids_Simd(soa, x, y, startIndex, length, ref simdTailIndex);
        
        // fallback to sisd.
        CalculateCentroids_Sisd(soa, x, y, simdTailIndex, length);
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
        
        soa.AppendCount = 0;
        soa.Length = 0;

        GC.SuppressFinalize(soa);
    }

    ~Soa_Aabb()
    {
        Dispose(this);
    }
}