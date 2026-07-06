using System.Runtime.CompilerServices;
using N_Howl.N_Collections;
using System.Numerics;
using N_Howl.N_Memory;

namespace Howl.Math.Shapes;

public struct Soa_Aabb
{
    /// <summary>
    ///     The x-components of the minimum vertex.
    /// </summary>
    public Array<float> MinX;

    /// <summary>
    ///     the y-components of the minimum vertex.
    /// </summary>
    public Array<float> MinY;
    
    /// <summary>
    /// the x-components of the maximum vertex.
    /// </summary>
    public Array<float> MaxX;

    /// <summary>
    ///     The y-components of the maximum vertex.
    /// </summary>
    public Array<float> MaxY;

    /// <summary>
    /// The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;

    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    public bool IsIntialised;

    public static bool Initialise(ref Soa_Aabb soa, ref MemoryArena arena, int length)
    {
        if (soa.IsIntialised)
        {
            Debug.Panic("Already Intialised.");
            return false;
        }

        N_Howl.N_Collections.Collections.Init(ref soa.MinX, ref arena, length);
        N_Howl.N_Collections.Collections.Init(ref soa.MinY, ref arena, length);
        N_Howl.N_Collections.Collections.Init(ref soa.MaxX, ref arena, length);
        N_Howl.N_Collections.Collections.Init(ref soa.MaxY, ref arena, length);
        soa.Length = length;

        soa.IsIntialised = true;
        return true;
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
    public static void Insert(ref Soa_Aabb soa, int insertIndex, float minX, float minY, float maxX, float maxY)
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
    public static void Append(ref Soa_Aabb soa, float minX, float minY, float maxX, float maxY)
    {
        Insert(ref soa, soa.AppendCount, minX, minY, maxX, maxY);
        soa.AppendCount++;
    }

    /// <summary>
    /// Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa instance to reset.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ResetCount(ref  Soa_Aabb soa)
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
    public static void CalculateCentroids_Sisd(ref Soa_Aabb soa, System.Span<float> x, System.Span<float> y, int startIndex, int length)
    {
        System.Span<float> minX = N_Howl.N_Collections.Collections.AsSpan(soa.MinX);
        System.Span<float> minY = N_Howl.N_Collections.Collections.AsSpan(soa.MinY);
        System.Span<float> maxX = N_Howl.N_Collections.Collections.AsSpan(soa.MaxX);
        System.Span<float> maxY = N_Howl.N_Collections.Collections.AsSpan(soa.MaxY);

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
    public static void CalculateCentroids_Simd(ref Soa_Aabb soa, System.Span<float> x, System.Span<float> y, int startIndex, int length, 
        ref int tailindex
    )
    {
        System.Span<float> minX = N_Howl.N_Collections.Collections.AsSpan(soa.MinX);
        System.Span<float> minY = N_Howl.N_Collections.Collections.AsSpan(soa.MinY);
        System.Span<float> maxX = N_Howl.N_Collections.Collections.AsSpan(soa.MaxX);
        System.Span<float> maxY = N_Howl.N_Collections.Collections.AsSpan(soa.MaxY);

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
    public static void CalculateCentroids(ref Soa_Aabb soa, System.Span<float> x, System.Span<float> y, int startIndex, int length)
    {
        int simdTailIndex = 0;

        // perform simd.
        CalculateCentroids_Simd(ref soa, x, y, startIndex, length, ref simdTailIndex);
        
        // fallback to sisd.
        CalculateCentroids_Sisd(ref soa, x, y, simdTailIndex, length);
    }

}
