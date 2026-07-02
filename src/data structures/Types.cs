using Howl.Algorithms.Sorting;
using Howl.Unmanaged.Collections;
using N_Howl.N_Math;

namespace N_Howl.N_DataStructures;

public ref struct OverlapInfo{
    /// <summary>
    ///     The indices of the <c>owner</c> leaf in the overlaps.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>overlapIndex</c>.
    /// </remarks>
    public System.Span<int> OwnerLeafIndices;
    /// <summary>
    ///     The indices of the <c>other</c> leaf in the overlaps. 
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>overlapIndex</c>.
    /// </remarks>
    public System.Span<int> OtherLeafIndices;
    /// <summary>
    ///     The length of elements in the spans of this instance.
    /// </summary>
    public int Length;
}

public struct CategorisedLeafOverlaps{
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
}


public struct Soa_Leaf{
    /// <summary>
    ///     The Axis-Aligned Bounding-Boxes.
    /// </summary>
    public Soa_Aabb Aabbs;
    /// <summary>
    ///     The centroids of the Aabbs.
    /// </summary>
    public Soa_Vector2 Centroids;
    /// <summary>
    ///     The user-defined categories of the leaves (used to filter overlap results).
    /// </summary>
    public Array<int> Categories;
    /// <summary>
    ///     Gets the indices of branches that leaves are parented to.
    /// </summary>
    /// <remarks>
    ///     Elements in this array should be valid after a Bounding Volume Hierarchy has been constructed.
    /// </remarks>
    public Array<int> BranchIndices;
    /// <summary>
    ///     The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;
    /// <summary>
    ///     The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;
    public bool IsInitialised;
}

public struct Soa_Branch{
    /// <summary>
    ///     The Axis-Aligned Bounding-Boxes of all branches.
    /// </summary>
    public Soa_Aabb Aabbs;
    /// <summary>
    ///     The left leaf indices of all branches.
    /// </summary>
    public Array<int> LeftLeafIndices;
    /// <summary>
    ///     The right leaf indices of all branches.
    /// </summary>
    public Array<int> RightLeafIndices;
    /// <summary>
    ///     The number of child branches (including the branch itself) of all branches.
    /// </summary>
    /// <remarks>
    ///     E.g a branch that has three 4 children will have a subtree size of 5; as the subtree size counts the branch as well.
    /// </remarks>
    public Array<int> SubtreeSizes;
    /// <summary>
    ///     The amount of leaves attatched of all branches.
    /// </summary>
    /// <remarks>
    ///     Specifically, the amount of immediate leaves attatched to a branch; not counting children or parents.
    /// </remarks>
    public Array<int> LeafCounts;
    /// <summary>
    /// The indices for the parent branch of all branches.
    /// </summary>
    public Array<int> ParentIndices;
    /// <summary>
    /// The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;
    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;
    public bool IsInitialised;
}

public struct Soa_Overlap{
    /// <summary>
    ///     The leaf indices of the <c>owner</c> of an overlap between to leaves.
    /// </summary>
    /// <remarks>
    ///     Use a <c>overlapIndex</c> integer to access elements.
    /// </remarks>
    public Array<int> OwnerLeafIndices;
    /// <summary>
    ///     The leaf indices of the <c>owner</c> of an overlap between to leaves.
    /// </summary>
    /// <remarks>
    ///     Use a <c>overlapIndex</c> integer to access elements.
    /// </remarks>
    public Array<int> OtherLeafIndices;
    /// <summary>
    ///     The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;
    /// <summary>
    ///     The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;
}

public struct Soa_QueryResult{
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
}

public struct BoundingVolumeHierarchy{
    /// <summary>
    ///     The radix sort buffer used when sorting this leaf buffer.
    /// </summary>
    public RadixSortBuffer RadixSortBuffer;
    /// <summary>
    ///     The constructed branches from the inserted leaves.
    /// </summary>
    /// <remarks>
    ///     Use a <c>branchIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Branch Branches;
    /// <summary>
    ///     The leaves to construct branches from.
    /// </summary>
    /// <remarks>
    ///     Use a <c>leafIndex</c> integer to get access elements.
    /// </remarks>
    public Soa_Leaf Leaves;
    /// <summary>
    ///     The morton codes for all leaf centroids.
    /// </summary>
    /// <remarks>
    ///     Use a <c>MortonLeafIds</c> entry to access elements.
    /// </remarks>
    public Array<uint> MortonCentroids;
    /// <summary>
    ///     Used as an index for an element in <c>MortonCentroids</c> to get its associated leaf data in <c>Leaves</c>.
    /// </summary>
    /// <remarks>
    ///     Elements in <c>MortonLeafIds</c> and <c>MortonCentroids</c> are associated via index.
    /// </remarks>
    public Array<int> MortonLeafIds;
    public bool IsInitialised;
}
