using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl;
using N_Howl.N_Math;

namespace N_Howl.N_Collections;
public unsafe static class Collections{

/**##########################################################################################################################################
    div: Array.
##########################################################################################################################################**/

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool Init<T>(
    ref Array<T> array, T* pointer, int length
) where T : unmanaged{

    if (array.IsInitialised){
        Debug.Panic("already initialised.");
        return false;
    }
    array.IsInitialised = true;
    array.Pointer = pointer;
    array.Length = length;
    return true;
}

public static bool Init<T>(
    ref Array<T> array, System.Span<T> span
) where T: unmanaged{
fixed(T* ptr = span){

    return Init(ref array, ptr, span.Length);        
}}

public static bool Init<T>(
    ref Array<T> array, ref Memory.Arena arena, int length
) where T : unmanaged{

    if (array.IsInitialised){
        Debug.Panic("already initialised.");
        return false;
    }
    array.IsInitialised = true;
    T* ptr = Memory.PushArrayRaw<T>(ref arena, length);
    array.IsInitialised = true;
    array.Pointer = ptr;
    array.Length = length;
    return true;            
}


[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Span<T> AsSpan<T>(
    Array<T> array
) where T : unmanaged{

    return new System.Span<T>(array.Pointer, array.Length);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Span<T> AsSpan<T>(
    Array<T> array, int startIndex, int length
) where T : unmanaged{

    Debug.Assert(array.Pointer + startIndex + length < array.Pointer + array.Length,
        "Index out of range."
    );
    return new System.Span<T>(array.Pointer+startIndex, length);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.ReadOnlySpan<T> AsReadOnlySpan<T>(
    Array<T> array
) where T : unmanaged{

    return new System.ReadOnlySpan<T>(array.Pointer, array.Length);
}

public static void ClearZeroed<T>(Array<T> array) where T : unmanaged{
    NativeMemory.Clear(array.Pointer, (nuint)Memory.ArraySizeInBytes<T>(array.Length));
}

public static void ClearZeroed<T>(
    Array<T> array, int baseIndex
) where T : unmanaged{
    Debug.Assert(baseIndex < array.Length, $"Index: '{baseIndex}' is Out Of Bounds; Array Length: '{array.Length}' .");
    NativeMemory.Clear(array.Pointer+((nuint)(sizeof(T) * baseIndex)), (nuint)(sizeof(T) * (array.Length-baseIndex)));
}

/**##########################################################################################################################################
    div: SwapBackArray.
##########################################################################################################################################**/

public static bool Init<T>(
    ref SwapBackArray<T> array, T* pointer, int length
) where T : unmanaged{
    
    if (array.IsInitialised){
        Debug.Panic("already initialised.");
        return false;
    }
    array.IsInitialised = true;
    array.Pointer = pointer;
    array.Length = length;
    return true;
}

public static bool Init<T>(
    ref SwapBackArray<T> array, ref Memory.Arena arena, int length
) where T : unmanaged{
    
    if (array.IsInitialised){
        Debug.Panic("already initialised.");
        return false;
    }
    array.IsInitialised = true;
    T* ptr = Memory.PushArrayRaw<T>(ref arena, length);
    array.IsInitialised = true;
    array.Pointer = ptr;
    array.Length = length;
    return true;            
}

/// <summary>
///     Appends a value to a swapback array.
/// </summary>
/// <param name="array">the swapback array instance to append to.</param>
/// <param name="value">the value to append.</param>
/// <returns>the index the value was written to in the array.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int Append<T>(
    ref SwapBackArray<T> array, T value
) where T : unmanaged{
    
    array.Count++;
    array[array.Count-1] = value;
    return array.Count;
}

/// <summary>
///     Removes an entry at a given index from a swapback array.
/// </summary>
/// <param name="array">the swapback array instance.</param>
/// <param name="index">the index to remove at.</param>
/// <returns>the index of the value that was swapped with the value that was removed.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int RemoveAt<T>(
    ref SwapBackArray<T> array, int index
) where T : unmanaged{
    
    System.Diagnostics.Debug.Assert(index >= 0 && index < array.Count, $"Index: '{index}' is Out Of Bounds; Array Count: '{array.Count}' .");

    // decrement the count.
    array.Count--;
    
    // set the data to remove with the last entry.
    array[index] = array[array.Count];

    return array.Count;
}

/// <summary>
///     Sets the <c>Count</c> of a swap back array to zero.
/// </summary>
/// <param name="array">the swap back array instance to clear.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Clear<T>(
    ref SwapBackArray<T> array
) where T : unmanaged{
    
    array.Count = 0;
}

/// <summary>
///     Gets the underlying array of a swapback array as a span.
/// </summary>
/// <param name="array">the swapback array instance to get as a span.</param>
/// <returns>The span of the underlying array.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Span<T> AsSpan<T>(
    SwapBackArray<T> array
) where T : unmanaged{
    
    return new System.Span<T>(array.Pointer, array.Count);
}

/// <summary>
///     Gets a span slice of a swapback array's underlying array.
/// </summary>
/// <param name="array">the swapback array to get a slice of.</param>
/// <param name="start">The zero-based index at which to begin this slice.</param>
/// <param name="length">The desired length for the slice (exclusive).</param>
/// <returns></returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Span<T> Slice<T>(
    SwapBackArray<T> array, int start, int length
) where T : unmanaged{
    return AsSpan(array).Slice(start, length);
}

/**##########################################################################################################################################
    div: StackArray.
##########################################################################################################################################**/

public static bool Init<T>(
    ref StackArray<T> array, T* pointer, int length
) where T : unmanaged{
    
    if (array.IsInitialised){
        Debug.Panic("already initialised.");
        return false;
    }
    array.IsInitialised = true;
    array.Pointer = pointer;
    array.Length = length;
    return true;
}

public static bool Init<T>(
    ref StackArray<T> array, ref Memory.Arena arena, int length
) where T : unmanaged{
    
    if (array.IsInitialised){
        Debug.Panic("already initialised.");
        return false;
    }
    array.IsInitialised = true;
    T* ptr = Memory.PushArrayRaw<T>(ref arena, length);
    array.IsInitialised = true;
    array.Pointer = ptr;
    array.Length = length;
    return true;            
}

/// <summary>
///     Pushes a value to the top of a stack array.
/// </summary>
/// <param name="array">the stack array instance to push to.</param>
/// <param name="value">the value to push.</param>
public static void Push<T>(
    ref StackArray<T> array, T value
) where T : unmanaged{
    
    array.Count++;
    array[array.Count-1] = value;
}

/// <summary>
///     Removes and returns the item at the top of the stack.
/// </summary>
/// <param name="array">The stack array instance to pop from.</param>
/// <returns>The element removed from the top of the stack.</returns>
public static T Pop<T>(
    ref StackArray<T> array
) where T : unmanaged{
    
    T value = array[array.Count-1];
    array.Count-=1; 
    return value;
}

/// <summary>
///     Sets the <c>Count</c> of a stack array to zero.
/// </summary>
/// <param name="array">the stack array instance to clear.</param>
public static void Clear<T>(
    ref StackArray<T> array
) where T : unmanaged{
    
    array.Count = 0;
}

/// <summary>
///     Gets the last value added to a stack.
/// </summary>
/// <typeparam name="T">the data type</typeparam>
/// <param name="array">the stack array to peek into.</param>
/// <returns>the last value added to the stack.</returns>
public static ref T Peek<T>(
    StackArray<T> array
) where T : unmanaged{
    
    return ref array[array.Count-1];
}

/**##########################################################################################################################################
    div: ComponentArray.
##########################################################################################################################################**/

public static bool Init<T>(
    ref ComponentArray<T> array, ref Memory.Arena arena, int length
) where T : unmanaged{

    if (array.IsInitialised){
        Debug.Panic($"Component Array length '{length}' is not between '{ComponentArray.MinLength}' and '{ComponentArray.MaxLength}'.");
        return false;
    }
    array.IsInitialised = true;
    length = Math.Clamp(length, ComponentArray.MinLength, ComponentArray.MaxLength);
    Init(ref array.Sparse, ref arena, length);
    Init(ref array.Allocated, ref arena, length);
    Init(ref array.Active, ref arena, length);
    Init(ref array.DenseIndices, ref arena, length);
    array.Length = length;
    return true;
}

public static bool Allocate<T>(
    ref ComponentArray<T> array, int index, T value
) where T : unmanaged {
    
    if(index<=0){
        Debug.Panic("invalid or Nil access attempted.");
        return false;
    }
    ref bool isAllocated = ref array.Allocated[index];
    if(isAllocated==true){
        return false;
    }

    isAllocated = true;
    array.Sparse[index] = value;

    return true;
}

public static bool Deallocate<T>(
    ref ComponentArray<T> array, int index
) where T : unmanaged{
    
    if(index<=0){
        Debug.Panic("invalid or Nil access attempted.");
        return false;
    }
    ref bool isAllocated = ref array.Allocated[index];
    if(isAllocated==false){
        return false;
    }

    isAllocated = true;
    SetInactiveUnsafe(ref array, index);
    return true;
}

/// <summary>
///     Sets a component element to active.
/// </summary>
/// <returns>
///     true, if the component was set active; otherwise false.
/// </returns>
public static bool SetActive<T>(
    ref ComponentArray<T> array, int index
) where T : unmanaged{

    if (array.Allocated[index] == false || array.DenseIndices[index] != 0){
        return false;
    }
            
    SetActiveUnsafe(ref array, index);

    return true;
}

/// <summary>
///     Sets a component element to active.
/// </summary>
/// <remarks>
///    <para>Remarks:</para>
///    <para>active and allocated checks are not enforced; the index will always run through the set active procedure.</para>
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetActiveUnsafe<T>(
    ref ComponentArray<T> array, int index
) where T : unmanaged{

    // append the gen id to the active array and update the associated sparse index.
    array.DenseIndices[index] = array.Active.Count;
    Append(ref array.Active, index);
}


/// <summary>
///     Sets a component element to inactive.
/// </summary>
/// <returns>
///     true, if the component was set active; otherwise false.
/// </returns>
public static bool SetInactive<T>(
    ref ComponentArray<T> array, int index
) where T : unmanaged{

    if (array.Allocated[index] == false || array.DenseIndices[index] == 0){
        return false;
    }

    return true;
}

/// <summary>
///     Sets a component element to inactive.
/// </summary>
/// <remarks>
///    <para>Remarks:</para>
///    <para>active and allocated checks are not enforced; the index will always run through the set inactive procedure.</para>
/// </remarks>
public static void SetInactiveUnsafe<T>(
    ref ComponentArray<T> array, int index
) where T : unmanaged{        
    
    // get the dense index that is going to be swapped.
    int swappedSparseIndex = array.Active[array.Active.Count-1];
    
    ref int denseIndex = ref array.DenseIndices[index];

    // set its sparse index to the one that it will be swapped with during removal in the swapback array.
    array.DenseIndices[swappedSparseIndex] = denseIndex;
    
    // set the newly inactive component's dense index to point to the Nil value.
    denseIndex = 0;

    // remove the requested id.
    RemoveAt(ref array.Active, denseIndex);
}

/// <summary>
///     Gets the component data associated with a gen id in a components array.
/// </summary>
/// <param name="isValidOutput">output for whether or not the retrieved component data is valid.</param>
/// <returns>
///     A reference to the component data within the components array; note that the data may be
///     the Nil value. Ensure to check the output <c><paramref name="isValidOutput"/></c> before operating
///     on the returned reference.
/// </returns>
public static ref T GetData<T>(
    ref ComponentArray<T> array, int index, ref bool isValidOutput
) where T : unmanaged{

    System.Diagnostics.Debug.Assert(index > 0, "Nil access attempted.");
    
    // ensure that the data in the slot is not garbage.
    if(array.Allocated[index] == false)
    {
        // return the Nil.
        isValidOutput = false;
        return ref array.Sparse[0];
    }

    isValidOutput = true;
    return ref array.Sparse[index];
}

/// <summary>
///     Gets the component data associated with a gen id in a components array.
/// </summary>
/// <remarks>
///     Allocated checks are not enforced; component data at the given gen id slot will always be returned.
/// </remarks>
/// <returns>
///     A reference to the component data within the components array.
/// </returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static ref T GetDataUnsafe<T>(
    this ComponentArray<T> array, int index
) where T : unmanaged{
    
    System.Diagnostics.Debug.Assert(index != 0, "Nil access attempted.");
    return ref array.Sparse[index];
}

public static bool IsAllocated<T>(
    this ComponentArray<T> array, int index
) where T : unmanaged{
    return array.Allocated[index] == true;
}

/**##########################################################################################################################################
    div: Buffer.
##########################################################################################################################################**/

public static bool Init<T>(
    ref Buffer<T> buffer, T* pointer, int length
) where T : unmanaged{
    
    if (buffer.IsInitialised){
        Debug.Panic("Already Initialised.");
        return false;
    }
    buffer.IsInitialised = true;
    buffer.Pointer = pointer;
    buffer.Length = length;
    return true;
}

public static bool Init<T>(
    ref Buffer<T> buffer, ref Memory.Arena arena, int length
) where T : unmanaged{
    
    if (buffer.IsInitialised){
        Debug.Panic("already initialised.");
        return false;
    }
    buffer.IsInitialised = true;
    T* ptr = Memory.PushArrayRaw<T>(ref arena, length);
    buffer.IsInitialised = true;
    buffer.Pointer = ptr;
    buffer.Length = length;
    return true;            
}


public static bool Init<T>(
    ref Buffer<T> buffer, System.Span<T> span
) where T: unmanaged
{fixed(T* ptr = span){

    return Init(ref buffer, ptr, span.Length);        
}}

public static bool Append<T>(
    ref Buffer<T> buffer, T value
) where T : unmanaged{

    if(buffer.Count >= buffer.Length){
        Debug.Panic("Memory Limit Hit.");
        return false;
    }

    buffer[buffer.Count] = value;
    buffer.Count++;
    return true;
}

/// <remarks>
///    <para>Remarks:</para>
///    <para>This only sets the buffer internal count value to zero.</para>
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Clear<T>(
    ref Buffer<T> buffer
) where T : unmanaged{
    
    buffer.Count = 0;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ClearZeroed<T>(
    ref Buffer<T> buffer
) where T : unmanaged{

    buffer.Count = 0;
    for(int i = 0; i < buffer.Length; i++){
        buffer[i] = default;
    }
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Span<T> AsSpan<T>(
    Buffer<T> buffer
) where T : unmanaged{
    
    return new System.Span<T>(buffer.Pointer, buffer.Count);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Span<T> AsSpan<T>(
    Buffer<T> buffer, int startIndex, int length
) where T : unmanaged{
    Debug.Assert(buffer.Pointer + startIndex + length < buffer.Pointer + buffer.Length,
        "Index out of range."
    );
    return new System.Span<T>(buffer.Pointer+startIndex, length);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.ReadOnlySpan<T> AsReadOnlySpan<T>(
    Buffer<T> buffer
) where T : unmanaged{
    
    return new System.ReadOnlySpan<T>(buffer.Pointer, buffer.Count);
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///     <para>
///         When removing a value from the buffer; the data of the last element in the buffer is copied into the element that is being removed.
///     </para>
/// </remarks>
/// <returns>the index of the value that was swapped with the value that was removed.</returns>
public static int UnOrderedRemoveAt<T>(
    ref Buffer<T> buffer, int elementIndex
) where T : unmanaged{
    
    Debug.Assert(elementIndex >= 0 && elementIndex < buffer.Count, 
        $"Index: '{elementIndex}' is Out Of Bounds; Buffer Count: '{buffer.Count}' ."
    );

    // decrement the count.
    buffer.Count--;
    
    // set the data to remove with the last entry.
    buffer[elementIndex] = buffer[buffer.Count];

    return buffer.Count;
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>
///         When removing a value from the buffer; all elements after the removed slot is shifted backward towards element zero.
///    </para>
/// </remarks>
/// <returns>the count of elements of the buffer after the removal.</returns>
public static int OrderedRemoveAt<T>(
    ref Buffer<T> buffer, int elementIndex
) where T : unmanaged{

    Debug.Assert(elementIndex >= 0 && elementIndex < buffer.Count, 
        $"Index: '{elementIndex}' is Out Of Bounds; Buffer Count: '{buffer.Count}' ."
    );

    T* dst = buffer.Pointer + elementIndex;
    T* src = dst+1;

    Memory.Copy<T>((byte*)src, (byte*)dst, buffer.Count-(elementIndex+1));

    /**
        note orderring matters here; if this was above the for loop
        you would get an index out of bounds error.
    **/

    buffer.Count--;
    return buffer.Count;
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>
///         When inserting a value into an element, any data the was previously in that element is 
///         shifted forward - away from element zero - including all elements after the inserted element.
///    </para>
/// </remarks>
public static bool OrderedInsert<T>(
    ref Buffer<T> buffer, T value, int elementIndex
)where T : unmanaged{
    if(buffer.Count >= buffer.Length){
        Debug.Panic("Memory Limit Hit.");
        return false;
    }

    System.Span<T> span = new(buffer.Pointer, buffer.Length);

    buffer.Count++;
    T* src = buffer.Pointer + elementIndex;
    T* dst = src+1;

    // copy the dataset at and after the current slot further into the buffer. 
    Memory.Copy<T>((byte*)src, (byte*)dst, buffer.Count-elementIndex-1);
    // insert the new data.
    buffer[elementIndex] = value;
    
    return true;
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>
///         When inserting a value into an element, any data the was previously in that element is sent forward - away from element zero.
///     </para>
/// </remarks>
public static bool UnOrderedInsert<T>(
    ref Buffer<T> buffer, T value, int elementIndex
)where T : unmanaged{
    
    if(buffer.Count >= buffer.Length){
        Debug.Panic("Memory Limit Hit.");
        return false;
    }

    System.Span<T> span = new(buffer.Pointer, buffer.Length);

    buffer.Count++;

    // copy the data at the current slot to the back of the buffer.
    buffer[buffer.Count-1] = buffer[elementIndex];
    // insert the new data.
    buffer[elementIndex] = value;

    return true;
}

/**##########################################################################################################################################
    div: CategorisedOverlapArray.
##########################################################################################################################################**/

public static bool Init<T>(
    ref CategorisedOverlapArray<T> array, ref Memory.Arena arena, int categoryCount, int maxEntries
) where T : unmanaged{

    if (array.IsInitialised){
        Debug.Panic("Already Initialised");
        return false;
    }
    array.IsInitialised = true;

    array.CategoriesTriangularSum = Math.CalculateTriangularSum(categoryCount);

    Init(ref array.CategoryLengths, ref arena, categoryCount);
    Init(ref array.SubCategoryStartIndices, ref arena, array.CategoriesTriangularSum);
    Init(ref array.SubCategoryCounts, ref arena, array.CategoriesTriangularSum);
    Init(ref array.Data, ref arena, maxEntries);

    return true;

}

/// <summary>
///     Calculates the starting indices for each sub category to write to in the data array's. 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="array">the array instance to build the chunks for.</param>
public static void BuildChunks<T>(
    ref CategorisedOverlapArray<T> array
) where T : unmanaged{

    BuildChunks(ref array.CategoryLengths, ref array.SubCategoryStartIndices, array.MaxElements);
}

/// <summary>
///     Calculates the starting indices for each sub category to write to in the data array's. 
/// </summary>
/// <param name="categoryLengths"></param>
/// <param name="subCategoryStartIndices"></param>
/// <param name="maxElements"></param>
public static void BuildChunks(
    ref Array<int> categoryLengths, ref Array<int> subCategoryStartIndices, int maxElements
){

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
public static int GetCategorisedOverlapArrayElementIndex(
    int categoryIndex, int subCategoryIndex, int categoriesTriangularSum
){

    // ensure that cat is always the maxmimum.
    // Note: if the 'cat' was the min and the 'sub' was the max, the calculated
    //  index would always be incorrect (due to the formatting of the sub category arrays).
    int cat = Math.Max(categoryIndex, subCategoryIndex);
    int sub = Math.Min(categoryIndex, subCategoryIndex);

    return GetCategorisedOverlapArrayElementIndexUnsafe(cat, sub, categoriesTriangularSum);
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
public static int GetCategorisedOverlapArrayElementIndexUnsafe(
    int categoryIndex, int subCategoryIndex, int categoriesTriangularSum
){

    // this add one is very important, do not remove this EVER!!!!
    // Note: this is because categoryIndex is zero indexed, where as 
    //  cal CalculateTriangularSum() is indexed by one.
    categoryIndex++;

    int offset = Math.CalculateTriangularSum(categoryIndex);
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
public static bool Append<T>(
    ref CategorisedOverlapArray<T> array, T data, int categoryA, int categoryB
) where T : unmanaged{
    
    int writeIndex = 0;
    if(IncrementSubCategoryCount(ref array.CategoryLengths, ref array.SubCategoryCounts, ref array.SubCategoryStartIndices, 
        array.CategoriesTriangularSum, categoryA, categoryB, ref writeIndex
    )){
        // write the data to the index.
        array.Data[writeIndex] = data;
        return true;
    }
    else{
        return false;
    }
}

/// <summary>
///     Increments count of elements in a sub category.
/// </summary>
/// <param name="writeIndex">output for the index that should now be written to with valid data.</param>
/// <returns>true, if the count was incremented; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool IncrementSubCategoryCount(
    ref Array<int> categoryLengths, ref Array<int> subCategoryCounts, ref Array<int> subCategoryStartIndices, 
    int categoriesTriangularSum, int categoryA, int categoryB, ref int writeIndex
){
    
    int elementIndex = GetCategorisedOverlapArrayElementIndex(categoryA, categoryB, categoriesTriangularSum);
    int startIndex = subCategoryStartIndices[elementIndex];
    ref int count = ref subCategoryCounts[elementIndex];

    if ((categoryLengths[categoryA] * categoryLengths[categoryB]) - 1 < count){
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
public static System.Span<T> GetOverlaps<T>(
    CategorisedOverlapArray<T> array, int categoryA, int categoryB
) where T : unmanaged{
    
    int elementIndex = GetCategorisedOverlapArrayElementIndex(categoryA, categoryB, array.CategoriesTriangularSum);
    int startIndex = array.SubCategoryStartIndices[elementIndex];
    int count = array.SubCategoryCounts[elementIndex];
    return AsSpan(array.Data, startIndex, count);
}

/// <summary>
///     Sets the count values in a <c>SubCategoryCounts</c> array to zero. 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="array">the array instance to clear.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ClearCounts<T>(
    ref CategorisedOverlapArray<T> array
) where T : unmanaged{

    for(int i = 0; i < array.SubCategoryCounts.Length; i++)
    {
        array.SubCategoryCounts[i] = 0;
    }
}

/**##########################################################################################################################################
    div: FixedStrideSwapBackArray.
##########################################################################################################################################**/

/// <summary>
///     Appends a value to a destination array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="entryIndex">the index of the entry to append to.</param>
/// <returns>true, if the value was successfully appended; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool Append<T>(
    ref FixedStrideSwapbackArray<T> array, int stride, int entryIndex, T value
) where T : unmanaged{

    ref int count = ref array.EntryCounts[entryIndex];
    int next = count + 1;;
    if(next > stride){
        return false;
    }

    array.EntryElements[GetFixedStrideArrayElementIndex(entryIndex, stride, count)] = value;
    count = next;
    return true;
}

/// <summary>
///     Removes an element at a specified index from an array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="entryIndex">the index of the entry to remove from.</param>
/// <param name="elementIndex">the index - relative to the entry - of the element to remove.</param>
/// <returns>true, if the value was successfully removed; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool RemoveAt<T>(
    ref FixedStrideSwapbackArray<T> array, int stride, int entryIndex, int elementIndex
) where T : unmanaged{

    ref int count = ref array.EntryCounts[entryIndex];
    
    if(count == 0 || count < elementIndex){
        return false;
    }
    
    count--;
    
    // set the data to remove with the last entry.
    array.EntryElements[GetFixedStrideArrayElementIndex(entryIndex, stride, elementIndex)] = array.EntryElements[GetFixedStrideArrayElementIndex(entryIndex, stride, count)];
    
    return true;
}

/**##########################################################################################################################################
    div: FixedStrideArray.
##########################################################################################################################################**/

public static bool Init<T>(
    ref FixedStrideSwapbackArray<T> array, ref Memory.Arena arena, int entryElementCount, int entryAmount 
) where T : unmanaged{

    if(array.IsInitialised){
        Debug.Assert(false, "");
        return false;
    }

    Init(ref array.EntryElements, ref arena, entryElementCount * entryAmount);
    Init(ref array.EntryCounts, ref arena, entryAmount);

    return true;
}

/// <summary>
///     Gets the element index of an entry's element in a fixed stride array. 
/// </summary>
/// <param name="entryIndex">the index of the entry in the array.</param>
/// <param name="stride">the stride of each entry in the array.</param>
/// <param name="entryElementIndex">the index of the element in the entry.</param>
/// <returns>the index in the fixed stride array to the entry's element.</returns>
public static int GetFixedStrideArrayElementIndex(
    int entryIndex, int stride, int entryElementIndex
){
    return entryIndex * stride + entryElementIndex;
}

/// <summary>
///     Gets the element index for a value to be appeneded to in a fixed stride array instance.
/// </summary>
/// <remarks>
///     Remarks: it is assumed that the data array that the element index is calculated for is a one dimensional fixed stride array.
/// </remarks>
/// <param name="appendCounts">the count of appended data for all entries in the fixed stride array.</param>
/// <param name="entryIndex">the desired entry index to append to.</param>
/// <param name="stride">the stride of each entry in the one-dimensional data array.</param>
/// <param name="isValid">output, for whether or not the returned append index is valid.</param>
/// <returns>the index in the one-dimensional data array to append to.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int GetFixedStrideArrayAppendIndex(
    int[] appendCounts, int entryIndex, int stride, ref bool isValid){
    // check if a value can be appended to the entry..
    int appendCount = appendCounts[entryIndex];
    if(appendCount >= stride)
    {
        // the entry is full.
        isValid = false;
        return 0;
    }

    // return the element index in the one dimensional data array that the value should be appended to.
    isValid = true;
    return GetFixedStrideArrayElementIndex(entryIndex, stride, appendCount);
}

/**##########################################################################################################################################
    div: RunLengthBuffer.
##########################################################################################################################################**/

public static bool Init<T>(
    ref RunLengthBuffer<T> array, ref Memory.Arena arena, int length
)where T : unmanaged{

    if(array.IsInitialised){
        Debug.Assert(false, "Attempted to init an already initialised dynamic stride array.");
        return false;
    }

    Init(ref array.Data, ref arena, length);
    Init(ref array.Strides, ref arena, length);
    array.StartIndex = length;
    array.IsInitialised = true;
    return true;
}

public static bool Push<T>(
    ref RunLengthBuffer<T> buffer, T data, int runLength
)where T : unmanaged{

    if(buffer.StartIndex==0){
        Debug.Assert(false, "memory limit hit.");
        return false;
    }

    buffer.StartIndex--;
    buffer.Data[buffer.StartIndex] = data;
    buffer.Strides[buffer.StartIndex] = runLength;
    return true;
}

public static bool RemoveAt<T>(
    ref RunLengthBuffer<T> buffer, int elementIndex
)where T : unmanaged{

    if(elementIndex < buffer.StartIndex){
        Debug.Assert(false, "attempted to remove an invalid index.");
        return false;
    }

    int removedStride = buffer.Strides[elementIndex];
    int i = elementIndex-1; 
    for(; i >= buffer.StartIndex; i--){
        // push forward.
        int forward = 1+removedStride;
        int forwardIndex = i+forward;
        buffer.Data[forwardIndex] = buffer.Data[i];
        ref int otherStride = ref buffer.Strides[i]; 
        if(otherStride > removedStride){
            otherStride-=forward;
            buffer.Strides[forwardIndex] = otherStride;
        }
        else{
            buffer.Strides[forwardIndex] = otherStride;
            break;
        }
    }
    for(; i>= buffer.StartIndex; i--){
        // push forward.
        int forwardIndex = i+1+removedStride;
        buffer.Data[forwardIndex] = buffer.Data[i];
        buffer.Strides[forwardIndex] = buffer.Strides[i];
    }
    
    buffer.StartIndex++;

    return true;
}

public static bool MoveToStart<T>(
    ref RunLengthBuffer<T> buffer, int elementIndex
)where T : unmanaged{

    if(elementIndex < buffer.StartIndex){
        Debug.Assert(false, "attempted to remove an invalid index.");
        return false;
    }

    int removedStride = buffer.Strides[elementIndex];
    int i = elementIndex-1; 
    for(; i >= buffer.StartIndex; i--){
        // push forward.
        int forward = 1+removedStride;
        int forwardIndex = i+forward;
        buffer.Data[forwardIndex] = buffer.Data[i];
        ref int otherStride = ref buffer.Strides[i]; 
        if(otherStride > removedStride){
            otherStride-=forward;
            buffer.Strides[forwardIndex] = otherStride;
        }
        else{
            buffer.Strides[forwardIndex] = otherStride;
            break;
        }
    }
    for(; i>= buffer.StartIndex; i--){
        // push forward.
        int forwardIndex = i+1+removedStride;
        buffer.Data[forwardIndex] = buffer.Data[i];
        buffer.Strides[forwardIndex] = buffer.Strides[i];
    }
    

    return true;
}


}