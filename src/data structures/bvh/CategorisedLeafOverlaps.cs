
using System.Runtime.CompilerServices;
using Howl.DataStructures.Bvh;
using Howl.Unmanaged.Collections;

namespace Howl;

public struct CategorisedLeafOverlaps
{

    /// <summary>
    ///     The amount of leaves within a category.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed by <c>categoryIndex</c>.
    /// </remarks>
    public Array<int> CategoryLengths;

    /// <summary>
    ///     The starting indices for a sub category within the <c>overlap arrays</c>.
    /// </summary>
    /// <remarks>
    ///     Remarks:
    ///     <para> <c>Elements</c> are arranged in a fixed-stride-like format.</para>
    ///     <para> <c>Entry</c> indices are layed out in a descending order</para>
    ///     <para> Example: (2,1,0) or (4,3,2,etc...). </para>
    ///     <para> 
    ///         The <c>stride</c> of each <c>entry</c> are the subcategories in a arithermatic series/triangular sum format - in ascending order.
    ///         Note: this format removes duplicate entries when a storing a category overlaps with another.
    ///     </para>
    ///     <para> Example(with 3 categories): </para>
    ///     <list type = "bullet">
    ///         <item>index = [0], entry = 2, sub categories = 0,1,2</item>
    ///         <item>index = [1], entry = 1, sub categories = 0,1</item>
    ///         <item>index = [2], entry = 0, sub categories = 0</item>
    ///     </list>
    /// </remarks>
    public Array<int> SubCategoryStartIndices;

    /// <summary>
    ///     The count of valid overlap elements after a sub category's start index within the <c>overlap arrays</c>.
    /// </summary>
    /// <remarks>
    ///     Remarks:
    ///     <para> <c>Elements</c> are arranged in a fixed-stride-like format.</para>
    ///     <para> <c>Entry</c> indices are layed out in a descending order</para>
    ///     <para> Example: (2,1,0) or (4,3,2,etc...). </para>
    ///     <para> 
    ///         The <c>stride</c> of each <c>entry</c> are the subcategories in a arithermatic series/triangular sum format - in ascending order.
    ///         Note: this format removes duplicate entries when a storing a category overlaps with another.
    ///     </para>
    ///     <para> Example(with 3 categories): </para>
    ///     <list type = "bullet">
    ///         <item>index = [0], entry = 2, sub categories = 0,1,2</item>
    ///         <item>index = [1], entry = 1, sub categories = 0,1</item>
    ///         <item>index = [2], entry = 0, sub categories = 0</item>
    ///     </list>
    /// </remarks>
    public Array<int> SubCategoryCounts;

    /// <summary>
    ///     The indices of the <c>owner</c> leaf of a given overlap.
    /// </summary>
    /// <remarks>
    ///     <para>Elements are associated via index to <c><see cref="OtherLeafIndices"/></c></para>
    ///     <para>Elements should be accessed via the calculated index provided by <see cref="GetElementIndex(int, int, int)"/>.</para>
    ///     <code>
    ///     int index = GetElementIndex(categoryIndex, subCategoryIndex, categoriesTriangularSum);
    ///     var element = myElements[index];
    ///     </code>
    /// </remarks>
    public Array<int> OwnerLeafIndices;

    /// <summary>
    ///     The indices of the <c>other</c> leaf of a given overlap.
    /// </summary>
    /// <remarks>
    ///     <para>Elements are associated via index to <c><see cref="OwnerLeafIndices"/></c></para>
    ///     <para>Elements should be accessed via the calculated index provided by <see cref="GetElementIndex(int, int, int)"/>.</para>
    ///     <code>
    ///     int index = GetElementIndex(categoryIndex, subCategoryIndex, categoriesTriangularSum);
    ///     var element = myElements[index];
    ///     </code>
    /// </remarks>
    public Array<int> OtherLeafIndices;
    
    /// <summary>
    ///     The triangular sum of the amount of categories the overlap data can be filtered into..
    /// </summary>
    public int CategoriesTriangularSum;

    /// <summary>
    ///     The maximum amount of overlaps this state instance can store.
    /// </summary>
    public int MaxOverlaps;

    public bool IsInitialised;

    /// <param name="categoryCount">the amount of categories the overlap data can be filtered into.</param>
    /// <param name="maxOverlaps">the maximum amount of overlap data that this instance can hold.</param>
    public static bool Initialise(ref CategorisedLeafOverlaps overlaps, ref Memory.Arena arena, int categoryCount, int maxOverlaps)
    {
        if (overlaps.IsInitialised)
        {
            Debug.Panic("Already Initialised");
            return false;
        }

        overlaps.CategoriesTriangularSum = Math.Math.CalculateTriangularSum(categoryCount);

        Array.Initialise(ref overlaps.CategoryLengths, ref arena, categoryCount);
        Array.Initialise(ref overlaps.SubCategoryStartIndices, ref arena, overlaps.CategoriesTriangularSum);
        Array.Initialise(ref overlaps.SubCategoryCounts, ref arena, overlaps.CategoriesTriangularSum);
        Array.Initialise(ref overlaps.OwnerLeafIndices, ref arena, maxOverlaps);
        Array.Initialise(ref overlaps.OtherLeafIndices, ref arena, maxOverlaps);

        overlaps.IsInitialised = true;
        return true;
    }

    /// <summary>
    ///     Calculates the starting indices for each sub category to write to in the data array's. 
    /// </summary>
    /// <param name="state"></param>
    public static void BuildChunks(CategorisedLeafOverlaps state)
    {
        CategorisedOverlapArray.BuildChunks(ref state.CategoryLengths, ref state.SubCategoryStartIndices, state.MaxOverlaps);        
    }

    /// <summary>
    ///     Appends an overlap to an instance.
    /// </summary>
    /// <param name="overlaps">the instance to append to.</param>
    /// <param name="ownerLeafIndex">the index of the leaf that is the <c>owner</c> of the overlap.</param>
    /// <param name="otherLeafIndex">the index of the leaf that is the <c>other</c> of the overlap.</param>
    /// <param name="ownerCategory">the category of the <c>owner</c> leaf.</param>
    /// <param name="otherCategory">the category of the <c>other</c> leaf.</param>
    /// <returns>true, if the overlap was successfully appended; otherwise false.</returns>
    public static bool Append(CategorisedLeafOverlaps overlaps, int ownerLeafIndex, int otherLeafIndex, 
        int ownerCategory, int otherCategory
    )
    {
        int writeIndex = 0;
        if(CategorisedOverlapArray.IncrementSubCategoryCount(ref overlaps.CategoryLengths, ref overlaps.SubCategoryCounts, 
            ref overlaps.SubCategoryStartIndices, overlaps.CategoriesTriangularSum, ownerCategory, otherCategory, ref writeIndex
        ))
        {
            // write the data to the index.
            overlaps.OwnerLeafIndices[writeIndex] = ownerLeafIndex;
            overlaps.OtherLeafIndices[writeIndex] = otherLeafIndex;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    ///     Sets the count values in a overlaps instance to zero. 
    /// </summary>
    /// <param name="overlaps">the instance to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearCounts(ref CategorisedLeafOverlaps overlaps)
    {
        for(int i = 0; i < overlaps.SubCategoryCounts.Length; i++)
        {
            overlaps.SubCategoryCounts[i] = 0;
        }
    }

    /// <summary>
    ///     Gets the overlap info between two categories.
    /// </summary>
    /// <param name="overlaps"></param>
    /// <param name="categoryA"></param>
    /// <param name="categoryB"></param>
    /// <returns>the data that overlaps between the two categories.</returns>
    public static OverlapInfo GetOverlaps(CategorisedLeafOverlaps overlaps, int categoryA, int categoryB)
    {
        int elementIndex = CategorisedOverlapArray.GetElementIndex(categoryA, categoryB, overlaps.CategoriesTriangularSum);
        int startIndex = overlaps.SubCategoryStartIndices[elementIndex];
        int count = overlaps.SubCategoryCounts[elementIndex];
        System.Span<int> ownerIndices = Array.AsSpan(overlaps.OwnerLeafIndices, startIndex, count);
        System.Span<int> otherIndices = Array.AsSpan(overlaps.OtherLeafIndices, startIndex, count);
        return new OverlapInfo(ownerIndices, otherIndices, count);
    }
}