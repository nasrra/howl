using System;
using System.Runtime.CompilerServices;

namespace Howl.Collections;

public class CategorisedOverlapArray<T>
{
    /// <summary>
    ///     The amount of elements within a category.
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
    ///     The count of valid elements after a sub category's start index within the <c>Data</c> array.
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
    ///     The triangular sum of the amount of categories the overlap data can be filtered into..
    /// </summary>
    public int CategoriesTriangularSum;

    /// <summary>
    ///     The maximum amount of elements this state instance can store.
    /// </summary>
    public int MaxElements;

    /// <summary>
    ///     The element data.
    /// </summary>
    /// <remarks>
    ///     <para>Elements should be accessed via the calculated index provided by <see cref="GetElementIndex(int, int, int)"/>.</para>
    ///     <code>
    ///     int index = GetElementIndex(categoryIndex, subCategoryIndex, categoriesTriangularSum);
    ///     var element = myElements[index];
    ///     </code>
    /// </remarks>
    public T[] Data;

    /// <summary>
    ///     Creates a new categorised array instance.
    /// </summary>
    /// <param name="categoryCount">the amount of categories the overlap data can be filtered into.</param>
    /// <param name="maxElements">the maximum amount of elements that this instance can hold.</param>
    public CategorisedOverlapArray(int categoryCount, int maxEntries)
    {
        CategoriesTriangularSum = Math.Math.CalculateTriangularSum(categoryCount);

        CategoryLengths = new int[categoryCount];

        SubCategoryStartIndices = new int [CategoriesTriangularSum];
        SubCategoryCounts = new int [CategoriesTriangularSum];

        Data = new T[maxEntries];
    }
}

public static class CategorisedOverlapArray
{
    /// <summary>
    ///     Calculates the starting indices for each sub category to write to in the data array's. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">the array instance to build the chunks for.</param>
    public static void BuildChunks<T>(this CategorisedOverlapArray<T> array)
    {
        BuildChunks(array.CategoryLengths, array.SubCategoryStartIndices, array.MaxElements);
    }

    /// <summary>
    ///     Calculates the starting indices for each sub category to write to in the data array's. 
    /// </summary>
    /// <param name="categoryLengths"></param>
    /// <param name="subCategoryStartIndices"></param>
    /// <param name="maxElements"></param>
    public static void BuildChunks(Span<int> categoryLengths, Span<int> subCategoryStartIndices, int maxElements)
    {
        // get the amount of categories this state instance can filter into.
        int categoryAmount = categoryLengths.Length;

        // the start index of the sub category.
        int startIndex = 0;
        
        // the index of the sub category to write the start index to.
        int writeIndex = 0;

        for(int categoryIndex = categoryAmount-1; categoryIndex >= 0; categoryIndex--)
        { 
            int categoryCount = categoryLengths[categoryIndex];
            for(int subCategoryIndex = 0; subCategoryIndex <= categoryIndex; subCategoryIndex++)
            {
                // set the start index in the overlap arrays.
                subCategoryStartIndices[writeIndex] = startIndex;
                
                writeIndex++;

                // add the stride/amount of overlaps that can possibly happen between these categories.
                startIndex += categoryLengths[categoryIndex] * categoryLengths[subCategoryIndex];
#if DEBUG
                System.Diagnostics.Debug.Assert(startIndex >= maxElements, "StartIndex exceeded max elements count! state instance cannot store the required amount of possible elements.");
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
        int cat = Math.Math.Max(categoryIndex, subCategoryIndex);
        int sub = Math.Math.Min(categoryIndex, subCategoryIndex);

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
    ///     Appends data to a pair of overlapping categories.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">the data to copy into the array.</param>
    /// <param name="array">the array instance to append to.</param>
    /// <param name="categoryA"></param>
    /// <param name="categoryB"></param>
    /// <returns>true, if the data was successfuly appended; otherwise false.</returns>
    public static bool Append<T>(this T data, CategorisedOverlapArray<T> array, int categoryA, int categoryB)
    {
        int writeIndex = 0;
        if(IncrementSubCategoryCount(array.CategoryLengths, array.SubCategoryCounts, array.SubCategoryStartIndices, 
            array.CategoriesTriangularSum, categoryA, categoryB, ref writeIndex
        ))
        {
            // write the data to the index.
            array.Data[writeIndex] = data;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    ///     Increments count of elements in a sub category.
    /// </summary>
    /// <param name="categoryLengths"></param>
    /// <param name="subCategoryCounts"></param>
    /// <param name="subCategoryStartIndices"></param>
    /// <param name="categoriesTriangularSum"></param>
    /// <param name="categoryA"></param>
    /// <param name="categoryB"></param>
    /// <param name="writeIndex">output for the index that should now be written to with valid data.</param>
    /// <returns>true, if the count was incremented; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IncrementSubCategoryCount(Span<int> categoryLengths, Span<int> subCategoryCounts, Span<int> subCategoryStartIndices, 
        int categoriesTriangularSum, int categoryA, int categoryB, ref int writeIndex
    )
    {
        int elementIndex = GetElementIndex(categoryA, categoryB, categoriesTriangularSum);
        int startIndex = subCategoryStartIndices[elementIndex];
        ref int count = ref subCategoryCounts[elementIndex];

        if ((categoryLengths[categoryA] * categoryLengths[categoryB]) - 1 < count)
        {
            return false;
        }

        writeIndex = startIndex + count;
        count++;
        return true;
    }

    /// <summary>
    ///     Gets the data that overlaps between two categories.
    /// </summary>
    /// <param name="overlaps"></param>
    /// <param name="categoryA"></param>
    /// <param name="categoryB"></param>
    /// <returns>the data that overlaps between two categories.</returns>
    public static Span<T> GetOverlaps<T>(this CategorisedOverlapArray<T> array, int categoryA, int categoryB)
    {
        int elementIndex = GetElementIndex(categoryA, categoryB, array.CategoriesTriangularSum);
        int startIndex = array.SubCategoryStartIndices[elementIndex];
        int count = array.SubCategoryCounts[elementIndex];
        return array.Data.AsSpan(startIndex, count);
    }

    /// <summary>
    ///     Sets the count values in a <c>SubCategoryCounts</c> array to zero. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">the array instance to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearCounts<T>(this CategorisedOverlapArray<T> array)
    {
        ClearCounts(array.SubCategoryCounts);
    }

    /// <summary>
    ///     Sets the count values in a <c>SubCategoryCounts</c> array to zero. 
    /// </summary>
    /// <param name="counts">the array instance to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearCounts(Span<int> counts)
    {
        for(int i = 0; i < counts.Length; i++)
        {
            counts[i] = 0;
        }
    }
}