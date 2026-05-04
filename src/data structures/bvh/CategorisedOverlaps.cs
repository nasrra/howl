using System;
using System.Runtime.CompilerServices;
using Howl.DataStructures.Bvh;

namespace Howl;

public class CategorisedOverlaps
{

    /// <summary>
    ///     The amount of leaves within a category.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed by <c>categoryIndex</c>.
    /// </remarks>
    public int[] CategoryLengths;

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
    public int[] SubCategoryStartIndices;

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
    public int[] SubCategoryCounts;

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
    public int[] OwnerLeafIndices;

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
    public int[] OtherLeafIndices;
    
    /// <summary>
    ///     The triangular sum of the amount of categories the overlap data can be filtered into..
    /// </summary>
    public int CategoriesTriangularSum;

    /// <summary>
    ///     The maximum amount of overlaps this state instance can store.
    /// </summary>
    public int MaxOverlaps;

    /// <summary>
    ///     Creates a new state instance.
    /// </summary>
    /// <param name="categoryCount">the amount of categories the overlap data can be filtered into.</param>
    /// <param name="maxOverlaps">the maximum amount of overlap data that this instance can hold.</param>
    public CategorisedOverlaps(int categoryCount, int maxOverlaps)
    {
        CategoriesTriangularSum = Math.Math.CalculateTriangularSum(categoryCount);

        CategoryLengths = new int[categoryCount];

        SubCategoryStartIndices = new int [CategoriesTriangularSum];
        SubCategoryCounts = new int [CategoriesTriangularSum];

        OwnerLeafIndices = new int[maxOverlaps];
        OtherLeafIndices = new int[maxOverlaps];
    }

    /// <summary>
    ///     Builds the
    /// </summary>
    /// <param name="state"></param>
    public static void BuildChunks(CategorisedOverlaps state)
    {
        // get the amount of categories this state instance can filter into.
        int categoryAmount = state.CategoryLengths.Length;

        // the start index of the sub category.
        int startIndex = 0;
        
        // the index of the sub category to write the start index to.
        int writeIndex = 0;

        for(int categoryIndex = categoryAmount-1; categoryIndex >= 0; categoryIndex--)
        { 
            int categoryCount = state.CategoryLengths[categoryIndex];
            for(int subCategoryIndex = 0; subCategoryIndex <= categoryIndex; subCategoryIndex++)
            {
                // set the start index in the overlap arrays.
                state.SubCategoryStartIndices[writeIndex] = startIndex;
                
                writeIndex++;

                // add the stride/amount of overlaps that can possibly happen between these categories.
                startIndex += state.CategoryLengths[categoryIndex] * state.CategoryLengths[subCategoryIndex];
#if DEBUG
                System.Diagnostics.Debug.Assert(startIndex >= state.MaxOverlaps, "StartIndex exceeded MaxOverlaps count! state instance cannot store the required amount of possible overlaps.");
#endif
            }
        }
    }

    /// <summary>
    ///     Gets the starting element index for a sub category within a state instances sub category arrays.
    /// </summary>
    /// <param name="categoryIndex">the category index.</param>
    /// <param name="subCategoryIndex">the sub category index.</param>
    /// <returns>the starting element index in the sub category arrays..</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetElementIndex(int categoryIndex, int subCategoryIndex, int categoriesTriangularSum)
    {
        // ensure that cat is always the maxmimum.
        // Note: if the 'cat' was the min and the 'sub' was the max, the calculated
        //  index would always be incorrect (due to the formatting of the sub category arrays).
        int cat = (int)Math.Math.Max(categoryIndex, subCategoryIndex);
        int sub = (int)Math.Math.Min(categoryIndex, subCategoryIndex);

        return GetElementIndexUnsafe(cat, sub, categoriesTriangularSum);
    }

    /// <summary>
    ///     Gets the starting element index for a sub category within a state instance's sub category arrays.
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para> 
    ///     <para>Min and Max value checks are not enforced;</para> 
    ///     <para>It is assumed that <paramref name="categoryIndex"/> is greater than (or equal to) <paramref name="subCategoryIndex"/></para>
    /// </remarks>
    /// <param name="categoryIndex"></param>
    /// <param name="subCategoryIndex"></param>
    /// <param name="categoriesTriangularSum"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetElementIndexUnsafe(int categoryIndex, int subCategoryIndex, int categoriesTriangularSum)
    {
        // this add one is very important, do not remove this EVER!!!!
        // Note: this is because categoryIndex is zero indexed, where as 
        //  cal CalculateTriangularSum() is indexed by one.
        categoryIndex++;

        int offset = Math.Math.CalculateTriangularSum(categoryIndex);
        offset -= subCategoryIndex;
        return categoriesTriangularSum - offset;        
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
    public static bool AppendOverlap(CategorisedOverlaps overlaps, int ownerLeafIndex, int otherLeafIndex, 
        int ownerCategory, int otherCategory
    )
    {
        int elementIndex = GetElementIndex(ownerCategory, otherCategory, overlaps.CategoriesTriangularSum);
        int startIndex = overlaps.SubCategoryStartIndices[elementIndex];
        ref int count = ref overlaps.SubCategoryCounts[elementIndex];

        if ((overlaps.CategoryLengths[ownerCategory] * overlaps.CategoryLengths[otherCategory]) - 1 < count)
        {
            return false;            
        }

        int writeIndex = startIndex + count;


        // write the data to the index.
        overlaps.OwnerLeafIndices[writeIndex] = ownerLeafIndex;
        overlaps.OtherLeafIndices[writeIndex] = otherLeafIndex;

        // increment the count.
        count++;

        return true;
    }

    /// <summary>
    ///     Sets the count values in a overlaps instance to zero. 
    /// </summary>
    /// <param name="overlaps">the instance to clear.</param>
    public static void ClearCounts(CategorisedOverlaps overlaps)
    {
        Span<int> counts = overlaps.SubCategoryCounts;
        for(int i = 0; i < counts.Length; i++)
        {
            counts[i] = 0;
        }
    }

    /// <summary>
    ///     Gets the overlap info for .
    /// </summary>
    /// <param name="overlaps"></param>
    /// <param name="categoryA"></param>
    /// <param name="categoryB"></param>
    /// <returns></returns>
    public static OverlapInfo GetOverlaps(CategorisedOverlaps overlaps, int categoryA, int categoryB)
    {
        int elementIndex = GetElementIndex(categoryA, categoryB, overlaps.CategoriesTriangularSum);
        int startIndex = overlaps.SubCategoryStartIndices[elementIndex];
        int count = overlaps.SubCategoryCounts[elementIndex];
        Span<int> ownerIndices = overlaps.OwnerLeafIndices.AsSpan(startIndex, count);
        Span<int> otherIndices = overlaps.OtherLeafIndices.AsSpan(startIndex, count);
        return new OverlapInfo(ownerIndices, otherIndices);
    }
}