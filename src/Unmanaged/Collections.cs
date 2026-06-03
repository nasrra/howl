using System.Runtime.CompilerServices;

namespace Howl.Unmanaged.Collections;

public unsafe struct Array<T> where T : unmanaged
{
    public T* Pointer;
    public int Length;

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get
        {
            Debug.Assert(index >= 0 && index < Length, 
                $"Index: '{index}' is Out Of Bounds; Array Length: '{Length}' ."
            );
            return ref Pointer[index];
        } 
    }
}    

public unsafe static class Array
{
    public static void Initialise<T>(ref Array<T> array, T* pointer, int length) where T : unmanaged
    {
        array.Pointer = pointer;
        array.Length = length;
    }

    public static void Initialise<T>(ref Array<T> array, ref Memory.Arena arena, int length) where T : unmanaged
    {
        T* ptr = Memory.PushArrayRaw<T>(ref arena, length);
        Initialise(ref array, ptr, length);    
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static System.Span<T> AsSpan<T>(Array<T> array) where T : unmanaged
    {
        return new System.Span<T>(array.Pointer, array.Length);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static System.ReadOnlySpan<T> AsReadOnlySpan<T>(Array<T> array) where T : unmanaged
    {
        return new System.ReadOnlySpan<T>(array.Pointer, array.Length);
    }    
}

public unsafe struct SwapBackArray<T> where T : unmanaged
{
    public T* Pointer;
    public int Length;
    public int Count;

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

public unsafe static class SwapBackArray
{
    public static void Initialise<T>(ref SwapBackArray<T> array, T* pointer, int length) where T : unmanaged
    {
        array.Pointer = pointer;
        array.Length = length;
    }

    public static void Initialise<T>(ref SwapBackArray<T> array, ref Memory.Arena arena, int length) where T : unmanaged
    {
        T* ptr = Memory.PushArrayRaw<T>(ref arena, length);
        Initialise(ref array, ptr, length);    
    }
    
    /// <summary>
    ///     Appends a value to a swapback array.
    /// </summary>
    /// <param name="array">the swapback array instance to append to.</param>
    /// <param name="value">the value to append.</param>
    /// <returns>the index the value was written to in the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int Append<T>(ref SwapBackArray<T> array, T value) where T : unmanaged
    {
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
    public static int RemoveAt<T>(ref SwapBackArray<T> array, int index) where T : unmanaged
    {
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
    public static void Clear<T>(ref SwapBackArray<T> array) where T : unmanaged
    {
        array.Count = 0;
    }

    /// <summary>
    ///     Gets the underlying array of a swapback array as a span.
    /// </summary>
    /// <param name="array">the swapback array instance to get as a span.</param>
    /// <returns>The span of the underlying array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static System.Span<T> AsSpan<T>(SwapBackArray<T> array) where T : unmanaged
    {
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
    public static System.Span<T> Slice<T>(SwapBackArray<T> array, int start, int length) where T : unmanaged
    {
        return AsSpan(array).Slice(start, length);
    }
}

public unsafe struct StackArray<T> where T : unmanaged
{
    public T* Pointer;
    public int Length;
    public int Count;

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

public unsafe static class StackArray
{
    public static void Initialise<T>(ref StackArray<T> array, T* pointer, int length) where T : unmanaged
    {
        array.Pointer = pointer;
        array.Length = length;
    }

    public static void Initialise<T>(ref StackArray<T> array, ref Memory.Arena arena, int length) where T : unmanaged
    {
        T* ptr = Memory.PushArrayRaw<T>(ref arena, length);
        Initialise(ref array, ptr, length);    
    }
    

    /// <summary>
    ///     Pushes a value to the top of a stack array.
    /// </summary>
    /// <param name="array">the stack array instance to push to.</param>
    /// <param name="value">the value to push.</param>
    public static void Push<T>(ref StackArray<T> array, T value) where T : unmanaged
    {
        array.Count++;
        array[array.Count-1] = value;
    }

    /// <summary>
    ///     Removes and returns the item at the top of the stack.
    /// </summary>
    /// <param name="array">The stack array instance to pop from.</param>
    /// <returns>The element removed from the top of the stack.</returns>
    public static T Pop<T>(ref StackArray<T> array) where T : unmanaged
    {
        T value = array[array.Count-1];
        array.Count-=1; 
        return value;
    }

    /// <summary>
    ///     Sets the <c>Count</c> of a stack array to zero.
    /// </summary>
    /// <param name="array">the stack array instance to clear.</param>
    public static void Clear<T>(ref StackArray<T> array) where T : unmanaged
    {
        array.Count = 0;
    }

    /// <summary>
    ///     Gets the last value added to a stack.
    /// </summary>
    /// <typeparam name="T">the data type</typeparam>
    /// <param name="array">the stack array to peek into.</param>
    /// <returns>the last value added to the stack.</returns>
    public static ref T Peek<T>(StackArray<T> array) where T : unmanaged
    {
        return ref array[array.Count-1];
    }
}

public struct ComponentArray<T> where T : unmanaged
{
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
}

public static class ComponentArray
{
    public const int MinLength = 2;
    public const int MaxLength = int.MaxValue;

    public static void Intialise<T>(ref ComponentArray<T> array, ref Memory.Arena arena, int length) where T : unmanaged
    {
        Debug.Assert(length <= MaxLength && length >= MinLength, 
            $"Component Array length '{length}' is not between '{MinLength}' and '{MaxLength}'."
        );
        
        length = Math.Math.Clamp(length, MinLength, MaxLength);

        Array.Initialise(ref array.Sparse, ref arena, length);
        Array.Initialise(ref array.Allocated, ref arena, length);
        SwapBackArray.Initialise(ref array.Active, ref arena, length);
        Array.Initialise(ref array.DenseIndices, ref arena, length);
        array.Length = length;
    }

    public static bool Allocate<T>(ref ComponentArray<T> array, int index, T value) where T : unmanaged 
    {
        Debug.Assert(index > 0, "Nil access attempted.");
        ref bool isAllocated = ref array.Allocated[index];
        if(isAllocated==true)
        {
            return false;
        }

        isAllocated = true;
        array.Sparse[index] = value;
        SetActiveUnsafe(ref array, index);

        return true;
    }

    public static bool Deallocate<T>(ref ComponentArray<T> array, int index) where T : unmanaged
    {
        Debug.Assert(index > 0, "Nil access attempted.");
        ref bool isAllocated = ref array.Allocated[index];
        if(isAllocated==false)
        {
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
    public static bool SetActive<T>(ref ComponentArray<T> array, int index) where T : unmanaged
    {
        if (array.Allocated[index] == false || array.DenseIndices[index] != 0)
        {
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
    public static void SetActiveUnsafe<T>(ref ComponentArray<T> array, int index) where T : unmanaged
    {
        // append the gen id to the active array and update the associated sparse index.
        array.DenseIndices[index] = array.Active.Count;
        SwapBackArray.Append(ref array.Active, index);
    }


    /// <summary>
    ///     Sets a component element to inactive.
    /// </summary>
    /// <returns>
    ///     true, if the component was set active; otherwise false.
    /// </returns>
    public static bool SetInactive<T>(ref ComponentArray<T> array, int index) where T : unmanaged
    {
        if (array.Allocated[index] == false || array.DenseIndices[index] == 0)
        {
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
    public static void SetInactiveUnsafe<T>(ref ComponentArray<T> array, int index) where T : unmanaged
    {        
        // get the dense index that is going to be swapped.
        int swappedSparseIndex = array.Active[array.Active.Count-1];
        
        ref int denseIndex = ref array.DenseIndices[index];

        // set its sparse index to the one that it will be swapped with during removal in the swapback array.
        array.DenseIndices[swappedSparseIndex] = denseIndex;
        
        // set the newly inactive component's dense index to point to the Nil value.
        denseIndex = 0;

        // remove the requested id.
        SwapBackArray.RemoveAt(ref array.Active, denseIndex);
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
    public static ref T GetData<T>(ref ComponentArray<T> array, int index, ref bool isValidOutput) where T : unmanaged
    {
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
    public static ref T GetDataUnsafe<T>(this ComponentArray<T> array, int index) where T : unmanaged
    {
        System.Diagnostics.Debug.Assert(index != 0, "Nil access attempted.");
        return ref array.Sparse[index];
    }
}