using System.Runtime.CompilerServices;
using Howl.Math.Shapes;
using Howl.Unmanaged.Collections;

namespace Howl.Text.Bvh;

public struct Soa_QueryResult
{
    /// <summary>
    ///     The index of the <c>owner</c> leaves of a query result.
    /// </summary>
    /// <remarks>
    ///     Use a <c>queryResult</c> integer to access elements.
    /// </remarks>
    public Array<int> OwnerLeafIndices;

    /// <summary>
    ///     The index of the <c>other</c> leaves of a query result.
    /// </summary>
    /// <remarks>
    ///     Use a <c>queryResult</c> integer to access elements.
    /// </remarks>
    public Array<int> OtherLeafIndices;

    /// <summary>
    ///     The index of the <c>owner</c> leaves of a query result.
    /// </summary>
    /// <remarks>
    ///     Use a <c>queryResult</c> integer to access elements.
    /// </remarks>
    public Soa_Aabb OwnerAabbs;

    /// <summary>
    ///     The index of the <c>other</c> leaves of a query result.
    /// </summary>
    /// <remarks>
    ///     Use a <c>queryResult</c> integer to access elements.
    /// </remarks>
    public Soa_Aabb OtherAabbs;

    /// <summary>
    ///     The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;

    /// <summary>
    ///     The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    public bool IsInitialised;

    public static bool Initialise(ref Soa_QueryResult soa, ref Memory.Arena arena, int length)
    {
        if (soa.IsInitialised)
        {
            Debug.Panic("Already Initialised!");
            return false;
        }

        Array.Initialise(ref soa.OwnerLeafIndices, ref arena, length);
        Array.Initialise(ref soa.OtherLeafIndices, ref arena, length);
        Soa_Aabb.Initialise(ref soa.OwnerAabbs, ref arena, length);
        Soa_Aabb.Initialise(ref soa.OtherAabbs, ref arena, length);
        soa.Length = length;
        soa.IsInitialised = true;
        return true;
    }

    /// <summary>
    ///     Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
    /// </summary>
    /// <param name="soa">the soa instance to append to.</param>
    /// <param name="ownerLeafIndex">the index of the <c>owner</c> leaf in the bvh.</param>
    /// <param name="ownerMinX">the x-compoent of the <c>owner</c> aabb's minimum vertex.</param>
    /// <param name="ownerMinY">the y-compoent of the <c>owner</c> aabb's minimum vertex.</param>
    /// <param name="ownerMaxX">the x-compoent of the <c>owner</c> aabb's maximum vertex.</param>
    /// <param name="ownerMaxY">the y-compoent of the <c>owner</c> aabb's maximum vertex.</param>
    /// <param name="otherLeafIndex">the index of the <c>other</c> leaf in the bvh.</param>
    /// <param name="otherMinX">the x-compoent of the <c>other</c> aabb's minimum vertex.</param>
    /// <param name="otherMinY">the y-compoent of the <c>other</c> aabb's minimum vertex.</param>
    /// <param name="otherMaxX">the x-compoent of the <c>other</c> aabb's maximum vertex.</param>
    /// <param name="otherMaxY">the y-compoent of the <c>other</c> aabb's maximum vertex.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(ref Soa_QueryResult soa, int ownerLeafIndex, float ownerMinX, float ownerMinY, float ownerMaxX, 
        float ownerMaxY, int otherLeafIndex, float otherMinX, float otherMinY, float otherMaxX, float otherMaxY
    )
    {
        int count = soa.AppendCount;
        
        soa.OwnerLeafIndices[count] = ownerLeafIndex;
        soa.OwnerAabbs.MinX[count]  = ownerMinX;
        soa.OwnerAabbs.MinY[count]  = ownerMinY;
        soa.OwnerAabbs.MaxX[count]  = ownerMaxX;
        soa.OwnerAabbs.MaxY[count]  = ownerMaxY;
        
        soa.OtherLeafIndices[count] = otherLeafIndex;
        soa.OtherAabbs.MinX[count]  = otherMinX;
        soa.OtherAabbs.MinY[count]  = otherMinY;
        soa.OtherAabbs.MaxX[count]  = otherMaxX;
        soa.OtherAabbs.MaxY[count]  = otherMaxY;
        
        soa.AppendCount++;
    }

    /// <summary>
    ///     Sets a soa instance's <c>AppendCount</c> to zero.
    /// </summary>
    /// <param name="soa">the soa to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(ref Soa_QueryResult soa)
    {
        soa.AppendCount = 0; 
    }
}