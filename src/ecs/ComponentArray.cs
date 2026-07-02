using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Howl.Collections;
using Howl.Ecs;

namespace Howl.Ecs;

/// <typeparam name="T">The type of component to store.</typeparam>
public class ComponentArray<T>
{
    /// <summary>
    ///     The backing storage for actual elements.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             Index 0 is reserved as a <c>Nil</c> sentinel and should not be used for data.
    ///         </item>
    ///         <item>
    ///             This is a parallel array associated with <c>Flags</c>, <c>Generations</c>, and <c>Allocated</c> by index.
    ///         </item>
    ///     </list>
    /// </remarks>
    public T[] Sparse;

    /// <summary>
    ///     Whether or not an element in the collection is valid (has been allocated/is in use). 
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             Index 0 is reserved as a <c>Nil</c> sentinel and should not be used for data.
    ///         </item>
    ///         <item>
    ///             This is a parallel array associated with <c>Data</c>, <c>Flags</c>, and <c>Generations</c> by index.
    ///         </item>
    ///     </list>
    /// </remarks>
    public bool[] Allocated;

    /// <summary>
    ///     An array of gen id's that are associated with an allocated component that is <c>Active</c> and ready to be processed.
    /// </summary>
    /// <remarks>
    ///     This collection is not 0 indexed as it has a Nil. When looping: index starting from 1 rather than 0.
    /// </remarks>
    public SwapBackArray<GenId> Active;

    /// <summary>
    ///     An array of associative indices, pointing a <c>Sparse</c> element to a <c>Active</c> element. 
    /// </summary>
    public int[] DenseIndices;

    /// <summary>
    ///     The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    ///     Whether or not this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new component array instance. 
    /// </summary>
    /// <param name="length">the lengths of the backing arrays.</param>
    public ComponentArray(int length){

#if DEBUG
        System.Diagnostics.Debug.Assert(length >= ComponentArray.MinLength && length <= ComponentArray.MaxLength, 
            $"ComponentArray length '{length}' is not between minimum '{ComponentArray.MinLength}' and maximum value '{ComponentArray.MaxLength}'"    
        );
#endif

        length = Howl.Math.Math.Clamp(length, ComponentArray.MinLength, ComponentArray.MaxLength);

        Sparse = new T[length];
        Allocated = new bool[length];
        DenseIndices = new int[length];
        Active = new(length);
        Length = length;
        
        // append Nil to the first entry.
        SwapBackArray.Append(Active, default);
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(ComponentArray<T> array)
    {
        if (array.Disposed)
        {
            return;
        }

        array.Disposed = true;
        
        array.Allocated = null;
        
        array.Sparse = null;

        SwapBackArray.Dispose(array.Active);
        array.Active = null;

        array.DenseIndices = null;
        
        array.Length = 0;

        GC.SuppressFinalize(array);
    }

    public void EnforceNil()
    {
        Nil.Enforce(Sparse);
    }

    ~ComponentArray()
    {
        Dispose(this);
    }
}

public static class ComponentArray
{




    /*******************
    
        Constants.
    
    ********************/




    public const int MinLength = 2;
    public const int MaxLength = GenId.UniqueIndicesCount;




    /*******************
    
        Allocate and Deallocate.
    
    ********************/




    /// <summary>
    ///     Allocates data into the backing data array.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <param name="genId">the gen id of the component to allocate.</param>
    /// <param name="component">the component to allocate.</param>
    /// <returns>
    ///     true, if the component was allocate; otherwise false if there is already a component allocated.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Allocate<T>(this ComponentArray<T> array, GenId genId, T component)
    {
        bool[] allocated = array.Allocated;

        int sparseIndex = GetSparseIndex(genId);
        ref bool isAllocated = ref allocated[sparseIndex];
        if (isAllocated)
        {
            Debug.LogError("Double component allocation", stackDepth: 4);
            return false;
        } 

        isAllocated = true;

        array.Sparse[sparseIndex] = component;
        
        // order matters here, the component needs to be
        // allocated before it can be set to active.
        SetActiveUnsafe(array.DenseIndices, array.Active, genId);

        return true;
    }

    /// <summary>
    ///     Sets the allocated bool at a given index to false.
    /// </summary>
    /// <param name="array">the component array to deallocate from.</param>
    /// <param name="genId">the gen id of the component to deallocate.</param>
    /// <returns>
    ///     true, if the component was deallocated; otherwise false if the component is deallocated.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Deallocate<T>(this ComponentArray<T> array, GenId genId)
    {        

        int sparseIndex = GetSparseIndex(genId);
        bool[] allocated = array.Allocated;
        ref bool isAllocated = ref allocated[sparseIndex];;

        if (isAllocated == false)
        {
            Debug.LogError("Double component deallocation", stackDepth: 4);
            return false;
        }

        // order matters here, set inactive before deallocation so that no systems access 
        // stale data that is 'Active'. 
        SetInactiveUnsafe(array.DenseIndices, array.Active, genId);

        allocated[sparseIndex] = false;
        return true;
    }




    /*******************
    
        Active and Inactive States.
    
    ********************/




    /// <summary>
    ///     Sets a component in a component array to <c>Active</c> and will be processed by systems.
    /// </summary>
    /// <param name="array">the components array containing the component.</param>
    /// <param name="genId">the gen id of the component to set <c>'Active'</c>.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <returns>
    /// <list type="bullet">
    ///     <item>
    ///         <see cref="GenIdResult.Ok"/>
    ///     </item>
    ///     <item>
    ///         <see cref="GenIdResult.NotAllocated"/>
    ///     </item>
    ///         <item>
    ///             <see cref="GenIdResult.InvalidGenId"/>
    ///         </item>
    /// </returns>
    public static bool SetActive<T>(this ComponentArray<T> array, GenIdAllocator entities, GenId genId)
    {

        if(GenIdAllocator.IsGenIdStale(entities, genId))
        {
            return false;
        }

        if (array.Allocated[GenId.GetIndex(genId)] == false)
        {
            return false;
        }
                
        SetActiveUnsafe(array, genId);

        return true;
    }


    /// <summary>
    ///     Sets a component in a component array to <c>Active</c> and will be processed by systems.
    /// </summary>
    /// <remarks>
    ///     Safety checks that are bypassed:
    ///     <list type="bullet">
    ///         <item> 
    ///             Generational component of a <c>GenId</c>.
    ///         </item>
    ///         <item>
    ///              <c>Allocated</c> flag being true or false.   
    ///         </item>
    ///     </list> 
    /// </remarks>    
    /// <param name="array">the components array containing the component.</param>
    /// <param name="genId">the generational-index packed '<paramref name="denseIndex"/>'.</param>
    public static void SetActiveUnsafe<T>(this ComponentArray<T> array, GenId genId)
    {
        SetActiveUnsafe(array.DenseIndices, array.Active, genId);
    }

    /// <summary>
    ///     Sets a component in a component array to <c>Active</c> and will be processed by systems.
    /// </summary>
    /// <remarks>
    ///     Safety checks that are bypassed:
    ///     <list type="bullet">
    ///         <item> 
    ///             Generational component of a <c>GenId</c>.
    ///         </item>
    ///         <item>
    ///              <c>Allocated</c> flag being true or false.   
    ///         </item>
    ///     </list> 
    /// </remarks> 
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetActiveUnsafe(int[] denseIndices, SwapBackArray<GenId> activeIndices, GenId genId)
    {        
        int sparseIndex = GenId.GetIndex(genId);
        int denseIndex = denseIndices[sparseIndex];

        // nothing needs to be done as it is already active.
        if(denseIndex != 0)
        {
            return;
        }

        // append the gen id to the active array and update the associated sparse index.
        denseIndices[sparseIndex] = activeIndices.Count;
        SwapBackArray.Append(activeIndices, genId);
    }

    /// <summary>
    ///     Sets a component in a component array to <c>Inactive</c> removing it from being processed by systems.
    /// </summary>
    /// <param name="array">the components array containing the component.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <param name="genId">the gen id of the component to set <c>'Inactive'</c>.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.NotAllocated"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.InvalidGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static bool SetInactive<T>(this ComponentArray<T> array, GenIdAllocator entities, GenId genId)
    {
        if(GenIdAllocator.IsGenIdStale(entities, genId))
        {
            return false;
        }

        if (array.Allocated[GenId.GetIndex(genId)] == false)
        {
            return false;
        }
        
        SetInactiveUnsafe(array, genId);

        return true;
    }

    /// <summary>
    ///     Sets a component in a component array to <c>Inactive</c> removing it from being processed by systems.
    /// </summary>
    /// <remarks>
    ///     Safety checks that are bypassed:
    ///     <list type="bullet">
    ///         <item> 
    ///             Generational component of a <c>GenId</c>.
    ///         </item>
    ///         <item>
    ///              <c>Allocated</c> flag being true or false.   
    ///         </item>
    ///     </list> 
    /// </remarks>
    /// <param name="array">the components array containing the component.</param>
    /// <param name="sparseIndex">the sparse index of the component to set <c>'Inactive'</c>.</param>
    public static void SetInactiveUnsafe<T>(this ComponentArray<T> array, GenId genId)
    {
        SetInactiveUnsafe(array.DenseIndices, array.Active, genId);
    }

    public static void SetInactiveUnsafe(int[] denseIndices, SwapBackArray<GenId> activeIndices, GenId genId)
    {        
        int sparseIndex = GenId.GetIndex(genId);
        int denseIndex = denseIndices[sparseIndex];

        // nothing needs to be done as it is already inactive.
        if(denseIndex == 0)
        {
            return;
        }

        // get the dense index that is going to be swapped.
        int swappedSparseIndex = GenId.GetIndex(activeIndices[activeIndices.Count-1]);
        
        // set its sparse index to the one that it will be swapped with during removal in the swapback array.
        denseIndices[swappedSparseIndex] = denseIndex;
        
        // set the newly inactive component's dense index to point to the Nil value.
        denseIndices[sparseIndex] = 0;

        // remove the requested id.
        SwapBackArray.RemoveAt(activeIndices, denseIndex);
    }




    /*******************
    
        Data retrieval.
    
    ********************/




    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="genId">the gen id of the component data to retrieve.</param>
    /// <param name="isValid">output for whether or not the retrieved component data is valid.</param>
    /// <returns>
    ///     A reference to the component data within the components array; note that the data may be
    ///     the Nil value. Ensure to check the output <c><paramref name="isValid"/></c> before operating
    ///     on the returned reference.
    /// </returns>
    public static ref T GetData<T>(this ComponentArray<T> components, GenId genId, ref bool isValid)
    {
        System.Diagnostics.Debug.Assert(genId != default, "nil element access attempted.");
        
        int sparseIndex = GetSparseIndex(genId);

        // ensure that the data in the slot is not garbage.
        if(components.Allocated[sparseIndex] == false)
        {
            // return the Nil.
            isValid = false;
            return ref GetDataUnsafe(components, 0);
        }

        isValid = true;
        return ref GetDataUnsafe(components, sparseIndex);
    }
    
    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <remarks>
    ///     <c>Allocated</c> checks are not enforced; component data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="genId">the gen id of the component data to retrieve.</param>
    /// <returns>
    ///     A reference to the component data within the components array.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref T GetDataUnsafe<T>(this ComponentArray<T> components, GenId genId)
    {
        return ref GetDataUnsafe(components, GetSparseIndex(genId));
    }

    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <remarks>
    ///     Allocated checks are not enforced; component data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="sparseIndex">the sparse index of the component data to retrieve.</param>
    /// <returns>
    ///     A reference to the component data within the components array.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref T GetDataUnsafe<T>(this ComponentArray<T> components, int sparseIndex)
    {
        System.Diagnostics.Debug.Assert(sparseIndex != 0, "nil element access attempted.");
        return ref components.Sparse[sparseIndex];
    }




    /*******************
    
        Utility.
    
    ********************/




    /// <summary>
    /// Gets the dense index of a given sparse entry within a component array instance.
    /// </summary>
    /// <param name="array">the component array instance.</param>
    /// <param name="sparseIndex">the index of the sparse entry in the component array instance. </param>
    /// <returns>the dense index of the sparse index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetDenseIndex<T>(this ComponentArray<T> array, int sparseIndex)
    {
        return array.DenseIndices[sparseIndex];
    }

    /// <summary>
    /// Gets the sparse index of a given gen id.
    /// </summary>
    /// <param name="genId">the specified gen id.</param>
    /// <returns>the sparse index of the gen id.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetSparseIndex(GenId genId)
    {
        return GenId.GetIndex(genId);
    }




    /*******************
    
        Disposal.
    
    ********************/




    public static void Dispose<T>(this ComponentArray<T> array)
    {
        ComponentArray<T>.Dispose(array);
    }
}