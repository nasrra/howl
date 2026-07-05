using System.Runtime.CompilerServices;

namespace N_Howl.N_Collections;

public unsafe struct Array<T> where T : unmanaged{
    public T* Pointer;
    public int Length;
    public bool IsInitialised;

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get
        {
            Howl.Debug.Assert(index >= 0 && index < Length, 
                $"Index: '{index}' is Out Of Bounds; Array Length: '{Length}' ."
            );
            return ref Pointer[index];
        } 
    }
}    

public unsafe struct SwapBackArray<T> where T : unmanaged{
    public T* Pointer;
    public int Length;
    public int Count;
    public bool IsInitialised;

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get
        {
            System.Diagnostics.Debug.Assert(index >= 0 && index < Count, $"Index: '{index}' is Out Of Bounds; Array Count: '{Count}' .");
            return ref Pointer[index];
        } 
    }
}

public unsafe struct StackArray<T> where T : unmanaged
{
    public T* Pointer;
    public int Length;
    public int Count;
    public bool IsInitialised;

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get
        {
            System.Diagnostics.Debug.Assert(index >= 0 && index < Count, $"Index: '{index}' is Out Of Bounds; Array Count: '{Count}' .");
            return ref Pointer[index];
        } 
    }
}

public static class ComponentArray{
    public const int MinLength = 2;
    public const int MaxLength = int.MaxValue;
}

public struct ComponentArray<T> where T : unmanaged{
    /// <summary>
    ///     The collections containing component data.
    /// </summary>
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>Contains a <c>Nil</c> element.</para>
    /// </remarks>
    public Array<T> Sparse;
    /// <summary>
    ///     Whether or not an element in <c>Sparse</c> has been allocated.
    /// </summary>
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>Contains a <c>Nil</c> element.</para>
    /// </remarks>
    public Array<bool> Allocated;
    /// <summary>
    ///     The indices of components in <c>Sparse</c> that are active.
    /// </summary>
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>Contains a <c>Nil</c> element.</para>
    /// </remarks>
    public SwapBackArray<int> Active;
    /// <summary>
    ///     The associative array of indices pointing a <c>Sparse</c> element to an <c>Active</c> element.
    /// </summary>
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para></para>
    /// </remarks>
    public Array<int> DenseIndices;
    /// <summary>
    ///     The length of all backing arrays of this instnace.
    /// </summary>
    public int Length;
    public bool IsInitialised;
}

public unsafe struct Buffer<T> where T : unmanaged
{
    public T* Pointer;
    public int Length;
    public int Count;
    public bool IsInitialised;

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get
        {
            Howl.Debug.Assert(index >= 0 && index < Length, 
                $"Index: '{index}' is Out Of Bounds; Array Length: '{Length}' ."
            );
            return ref Pointer[index];
        } 
    }
}    

public struct CategorisedOverlapArray<T> where T : unmanaged{
    /// <summary>
    ///     The amount of elements within a category.
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
    public Array<int> SubCategoryCounts;
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
    public Array<T> Data;
    public bool IsInitialised;
}

public struct FixedStrideSwapbackArray<T> where T : unmanaged{
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Access elements via [<c>entryIndex</c>+<c>elementIndex</c>]</para>
    /// </remarks>
    public Array<T> EntryElements;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Access elements via <c>entryIndex.</c></para>
    /// </remarks>
    public Array<int> EntryCounts;
    public bool IsInitialised;
}

public struct RunLengthBuffer<T> where T : unmanaged{
    /// <summary>
    ///     The stride of elements after an element in <c>Data</c> that is associated with that element.
    /// </summary>
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Elements are vertically associated with <c>Data</c></para>
    /// </remarks>
    public Array<int> Strides;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Elements are vrtically associated with <c>Strides</c>.</para>
    /// </remarks>
    public Array<T> Data;
    /// <summary>
    ///     The the index of the first valid element in the internal arrays; all subsequent elements are also valid.
    /// </summary>
    public int StartIndex;
    public bool IsInitialised;
}
