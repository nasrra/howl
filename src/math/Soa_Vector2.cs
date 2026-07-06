using System.Runtime.CompilerServices;
using N_Howl.N_Collections;
using N_Howl.N_Memory;

namespace Howl.Math;

public struct Soa_Vector2
{
    public Array<float> X;
    public Array<float> Y;

    /// <summary>
    ///     The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;

    /// <summary>
    ///     The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    public bool IsIntialised; 

    public static bool Initialise(ref Soa_Vector2 soa, ref MemoryArena arena, int length)
    {
        if (soa.IsIntialised)
        {
            Debug.Assert(false, "Already initialised.");
            return false;
        }
        soa.IsIntialised = true;
        soa.Length = length;
        N_Howl.N_Collections.Collections.Init(ref soa.X, ref arena, length);
        N_Howl.N_Collections.Collections.Init(ref soa.Y, ref arena, length);
        return true;
    }

    /// <summary>
    ///     Inserts elements into a soa instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(ref Soa_Vector2 soa, int insertIndex, float x, float y)
    {
        soa.X[insertIndex] = x;
        soa.Y[insertIndex] = y;
    }

    /// <summary>
    ///     Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
    /// </summary>
    public static void Append(ref Soa_Vector2 soa, float x, float y)
    {
        Insert(ref soa, soa.AppendCount, x, y);
        soa.AppendCount++;
    }

    /// <summary>
    ///     Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa instance to reset.</param>
    public static void ResetCount(ref Soa_Vector2 soa)
    {
        soa.AppendCount = 0;
    }
}