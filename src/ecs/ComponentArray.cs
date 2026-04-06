using System;
using System.Formats.Tar;
using System.Runtime.CompilerServices;
using Howl.Collections;
using Howl.ECS;
using Howl.Generic;

public class ComponentArray<T> : IDisposable
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
    
        Allocation and Deallocation.
    
    ********************/




    /// <summary>
    ///     Allocates data into the backing data array.
    /// </summary>
    /// <param name="array">the gen index array to allocate into.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <param name="genId">the gen id of the component to allocate.</param>
    /// <param name="component">the component to allocate.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIdResult Allocate(ComponentArray<T> array, EntityRegistry entities, GenId genId, T component)
    {
        if (EntityRegistry.GenIdIsStale(entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        int sparseIndex = GetSparseIndex(genId);

        // note: you may want to add a check here for allocated,
        // is it really a bug if you allocate data into the same gen id twice???

        array.Sparse[sparseIndex] = component;
        array.Allocated[sparseIndex] = true;
        
        // order matters here, the component needs to be
        // allocated before it can be set to active.
        SetActiveUnsafe(array, genId, sparseIndex);

        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Sets the allocated bool at a given index to false.
    /// </summary>
    /// <param name="array">the component array to deallocate from.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <param name="genId">the gen id of the component to deallocate.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.ComponentNotAllocated"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIdResult Deallocate(ComponentArray<T> array, EntityRegistry entities, GenId genId)
    {
        if (EntityRegistry.GenIdIsStale(entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        int sparseIndex = GetSparseIndex(genId);

        if(array.Allocated[sparseIndex] == false)
        {
            return GenIdResult.ComponentNotAllocated;
        }

        // order matters here, set inactive before deallocation so that no systems access 
        // stale data that is 'Active'. 
        SetInactiveUnsafe(array, sparseIndex);

        array.Allocated[sparseIndex] = false;
        return GenIdResult.Ok;
    }




    /*******************
    
        Active and Inactive States.
    
    ********************/




    /// <summary>
    ///     Sets a component in a component array to <c>Active</c> and will be processed by systems.
    /// </summary>
    /// <remarks>
    ///     Note: There is no check for generation discrepencies in the Gen Id, meaning the generational value is bypassed and only used for book keeping purposes. 
    /// </remarks>
    /// <param name="array">the components array containing the component.</param>
    /// <param name="genId">the gen id of the component to set <c>'Active'</c>.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <returns>
    /// <list type="bullet">
    ///     <item>
    ///         <see cref="GenIdResult.Ok"/>
    ///     </item>
    ///     <item>
    ///         <see cref="GenIdResult.ComponentNotAllocated"/>
    ///     </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    /// </returns>
    public static GenIdResult SetActive(ComponentArray<T> array, EntityRegistry entities, GenId genId)
    {
        int sparseIndex = GenId.GetIndex(genId);

        if(EntityRegistry.GenIdIsStale(entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        if (array.Allocated[sparseIndex] == false)
        {
            return GenIdResult.ComponentNotAllocated;
        }
                
        SetActiveUnsafe(array, genId, sparseIndex);

        return GenIdResult.Ok;
    }


    /// <summary>
    ///     Sets a component in a component array to <c>Inactive</c>, removing it from being processed by systems.
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
    /// <param name="denseIndex">the sparseIndex of the component to set <c>'Active'</c>.</param>
    public static void SetActiveUnsafe(ComponentArray<T> array, GenId genId, int sparseIndex)
    {
        int denseIndex = array.DenseIndices[sparseIndex];

        // nothing needs to be done as it is already active.
        if(denseIndex != 0)
        {
            return;
        }

        // append the gen id to the active array and update the associated sparse index.
        array.DenseIndices[sparseIndex] = array.Active.Count;
        SwapBackArray.Append(array.Active, genId);
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
    ///             <see cref="GenIdResult.ComponentNotAllocated"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIdResult SetInactive(ComponentArray<T> array, EntityRegistry entities, GenId genId)
    {
        if(EntityRegistry.GenIdIsStale(entities, genId))
        {
            return GenIdResult.StaleGenId;
        }

        int sparseIndex = GenId.GetIndex(genId);

        if (array.Allocated[sparseIndex] == false)
        {
            return GenIdResult.ComponentNotAllocated;
        }
        
        SetInactiveUnsafe(array, sparseIndex);

        return GenIdResult.Ok;
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
    public static void SetInactiveUnsafe(ComponentArray<T> array, int sparseIndex)
    {        
        int denseIndex = array.DenseIndices[sparseIndex];

        // nothing needs to be done as it is already inactive.
        if(denseIndex == 0)
        {
            return;
        }

        // get the dense index that is going to be swapped.
        int swappedSparseIndex = GenId.GetIndex(array.Active[array.Active.Count-1]);
        
        // set its sparse index to the one that it will be swapped with during removal in the swapback array.
        array.DenseIndices[swappedSparseIndex] = denseIndex;
        
        // set the newly inactive component's dense index to point to the Nil value.
        array.DenseIndices[sparseIndex] = 0;

        // remove the requested id.
        SwapBackArray.RemoveAt(array.Active, denseIndex);
    }




    /*******************
    
        Data retrieval.
    
    ********************/




    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <param name="genId">the gen id of the component data to retrieve.</param>
    /// <param name="result">output for whether or not the retrieved component data is valid.</param>
    /// <returns>
    ///     A reference to the component data within the components array; note that the data may be
    ///     the Nil value. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned reference.
    /// </returns>
    public static ref T GetData(ComponentArray<T> components, EntityRegistry entities, GenId genId, ref GenIdResult result)
    {
        if (EntityRegistry.GenIdIsStale(entities, genId))
        {
            // return the Nil.
            result = GenIdResult.StaleGenId;
            return ref GetDataUnsafe(components, 0);
        }

        int sparseIndex = GetSparseIndex(genId);

        // ensure that the data in the slot is not garbage.
        if(components.Allocated[sparseIndex] == false)
        {
            // return the Nil.
            result = GenIdResult.ComponentNotAllocated;
            return ref GetDataUnsafe(components, 0);
        }

        result = GenIdResult.Ok;
        return ref GetDataUnsafe(components, sparseIndex);
    }

    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <remarks>
    ///     <c>Allocated</c> and stale gen id checks are not enforced; component data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="genId">the gen id of the component data to retrieve.</param>
    /// <returns>
    ///     A reference to the component data within the components array.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref T GetDataUnsafe(ComponentArray<T> components, GenId genId)
    {
        return ref GetDataUnsafe(components, GetSparseIndex(genId));
    }

    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <remarks>
    ///     <c>Allocated</c> and stale gen id checks are not enforced; component data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="sparseIndex">the sparse index of the component data to retrieve.</param>
    /// <returns>
    ///     A reference to the component data within the components array.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref T GetDataUnsafe(ComponentArray<T> components, int sparseIndex)
    {        
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
    public static int GetDenseIndex(ComponentArray<T> array, int sparseIndex)
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
        return ComponentArray.GetSparseIndex(genId);
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
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIdResult Allocate<T>(this ComponentArray<T> array, EntityRegistry entities, GenId genId, T component)
    {
        return ComponentArray<T>.Allocate(array, entities, genId, component);
    }

    /// <summary>
    ///     Sets the allocated bool at a given index to false.
    /// </summary>
    /// <param name="array">the component array to deallocate from.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <param name="genId">the gen id of the component to deallocate.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.ComponentNotAllocated"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIdResult Deallocate<T>(this ComponentArray<T> array, EntityRegistry entities, GenId genId)
    {
        return ComponentArray<T>.Deallocate(array, entities, genId);
    }




    /*******************
    
        Active and Inactive States.
    
    ********************/




    /// <summary>
    ///     Sets a component in a component array to <c>Active</c> and will be processed by systems.
    /// </summary>
    /// <remarks>
    ///     Note: There is no check for generation discrepencies in the Gen Id, meaning the generational value is bypassed and only used for book keeping purposes. 
    /// </remarks>
    /// <param name="array">the components array containing the component.</param>
    /// <param name="genId">the gen id of the component to set <c>'Active'</c>.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <returns>
    /// <list type="bullet">
    ///     <item>
    ///         <see cref="GenIdResult.Ok"/>
    ///     </item>
    ///     <item>
    ///         <see cref="GenIdResult.ComponentNotAllocated"/>
    ///     </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    /// </returns>
    public static GenIdResult SetActive<T>(this ComponentArray<T> array, EntityRegistry entities, GenId genId)
    {
        return ComponentArray<T>.SetActive(array, entities, genId);
    }


    /// <summary>
    ///     Sets a component in a component array to <c>Inactive</c>, removing it from being processed by systems.
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
    /// <param name="denseIndex">the sparseIndex of the component to set <c>'Active'</c>.</param>
    public static void SetActiveUnsafe<T>(this ComponentArray<T> array, GenId genId, int sparseIndex)
    {
        ComponentArray<T>.SetActiveUnsafe(array, genId, sparseIndex);
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
    ///             <see cref="GenIdResult.ComponentNotAllocated"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIdResult SetInactive<T>(this ComponentArray<T> array, EntityRegistry entities, GenId genId)
    {
        return ComponentArray<T>.SetInactive(array, entities, genId);
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
    public static void SetInactiveUnsafe<T>(this ComponentArray<T> array, int sparseIndex)
    {
        ComponentArray<T>.SetInactiveUnsafe(array, sparseIndex);
    }




    /*******************
    
        Data retrieval.
    
    ********************/




    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="entities">the allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <param name="genId">the gen id of the component data to retrieve.</param>
    /// <param name="result">output for whether or not the retrieved component data is valid.</param>
    /// <returns>
    ///     A reference to the component data within the components array; note that the data may be
    ///     the Nil value. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned reference.
    /// </returns>
    public static ref T GetData<T>(this ComponentArray<T> components, EntityRegistry entities, GenId genId, ref GenIdResult result)
    {
        return ref ComponentArray<T>.GetData(components, entities, genId, ref result);
    }

    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="ecs">the ecs state containing a gen-id allocator instance where the <c><paramref name="genId"/></c> comes from.</param>
    /// <param name="genId">the gen id of the component data to retrieve.</param>
    /// <param name="result">output for whether or not the retrieved component data is valid.</param>
    /// <returns>
    ///     A reference to the component data within the components array; note that the data may be
    ///     the Nil value. Ensure to check the output <c><paramref name="result"/></c> before operating
    ///     on the returned reference.
    /// </returns>
    public static ref T GetData<T>(this ComponentArray<T> components, EcsState ecs, GenId genId, ref GenIdResult result)
    {
        return ref ComponentArray<T>.GetData(components, ecs.Entities, genId, ref result);
    }

    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <remarks>
    ///     <c>Allocated</c> and stale gen id checks are not enforced; component data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="genId">the gen id of the component data to retrieve.</param>
    /// <returns>
    ///     A reference to the component data within the components array.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref T GetDataUnsafe<T>(this ComponentArray<T> components, GenId genId)
    {
        return ref ComponentArray<T>.GetDataUnsafe(components, genId);
    }

    /// <summary>
    ///     Gets the component data associated with a gen id in a components array.
    /// </summary>
    /// <remarks>
    ///     <c>Allocated</c> and stale gen id checks are not enforced; component data at the given gen id slot will always be returned.
    /// </remarks>
    /// <param name="components">the components array storing the component data.</param>
    /// <param name="sparseIndex">the sparse index of the component data to retrieve.</param>
    /// <returns>
    ///     A reference to the component data within the components array.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref T GetDataUnsafe<T>(this ComponentArray<T> components, int sparseIndex)
    {
        return ref ComponentArray<T>.GetDataUnsafe(components, sparseIndex);
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
        return ComponentArray<T>.GetDenseIndex(array, sparseIndex);
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