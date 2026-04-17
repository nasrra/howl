using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl.Collections;
using Howl.Ecs;
using Howl.Generic;

namespace Howl.Ecs;

public static class GenIndexListProc
{

    public static bool ResizeSparseEntries(
        List<SparseEntry> sparse, 
        int count
    )
    {
        if(count <= sparse.Count)
        {
            return false;
        }
        int difference = count - sparse.Count;
        for(int i = 0; i < difference; i++)
        {
            sparse.Add(new());
        }
        return true;
    }

    private static GenIndexResult ValidateGenIndex(
        List<SparseEntry> sparse, 
        in GenIndex genIndex, 
        out Ref<SparseEntry> sparseEntryRef
    )
    {
        if(sparse.Count <= genIndex.Index || genIndex.Index < 0)
        {
            sparseEntryRef = new();
            return GenIndexResult.InvalidGenIndex;
        }

        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref SparseEntry sparseEntry = ref sparseSpan[genIndex.Index];

        if (sparseEntry.HasDenseEntry())
        {            
            if(sparseEntry.generation < genIndex.Generation)
            {
                sparseEntryRef = new();
                return GenIndexResult.StaleDenseAllocation;
            }
            else if(sparseEntry.generation > genIndex.Generation)
            {
                sparseEntryRef = new(ref sparseEntry, true);
                return GenIndexResult.StaleGenIndex;            
            }
        }
        
        sparseEntryRef = new(ref sparseEntry, true);
        return GenIndexResult.Ok;
    }


    /// <summary>
    /// Allocates a dense entry data and associates it with a sparse entry.
    /// </summary>
    /// <param name="genIndex">The GenIndex associated with the sparse entry; used to retrieve the dense data.</param>
    /// <param name="value">The data to assign to the dense entry.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <description>
    ///                 <see cref="GenIndexResult.DenseAlreadyAllocated"/>
    ///             </description>
    ///         </item>
    ///         <item> 
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>
    ///         </item>
    ///         <item> 
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIndexResult Allocate<T>(
        SwapbackList<DenseEntry<T>> dense,
        List<SparseEntry> sparse,
        in GenIndex genIndex, 
        in T value
    )
    {
        if(ValidateGenIndex(sparse, genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
            return result;
    
        if(sparseEntryRef.Value.HasDenseEntry() && sparseEntryRef.Value.generation == genIndex.Generation)
        {
            return GenIndexResult.DenseAlreadyAllocated;
        }

        // update the generation.
        
        sparseEntryRef.Value.generation = genIndex.Generation;
    
        // allocate a new dense entry.
        
        dense.Add(default);
        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        int denseIndex = denseSpan.Length - 1;
        ref DenseEntry<T> denseEntry = ref denseSpan[denseIndex];
        sparseEntryRef.Value.SetDenseIndex(denseIndex);

        // set the dense entry's data.
        
        denseEntry.Value = value;
        denseEntry.sparseIndex = genIndex.Index;
        
        return GenIndexResult.Ok;            
    }

    /// <summary>
    /// Deallocates an dense entry that is associated with sparse entry, setting the sparse entry to none. 
    /// </summary>
    /// <param name="genIndex">The GenIndex associated with the sparse entry; used to retrieve the dense data.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <description>
    ///                 <see cref="GenIndexResult.DenseNotAllocated"/>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIndexResult Deallocate<T>(
        SwapbackList<DenseEntry<T>> dense,
        List<SparseEntry> sparse,
        in GenIndex genIndex
    )
    {
        if(ValidateGenIndex(sparse, genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
            return result;

        // get the entry that will be swapped with the data that is to be removed.
        ref SparseEntry sparseEntryToRemove = ref sparseEntryRef.Value;

        if (sparseEntryToRemove.HasDenseEntry() == false)
        {
            return GenIndexResult.DenseNotAllocated;
        }

        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref DenseEntry<T> denseEntryToSwap = ref denseSpan[denseSpan.Length-1];
        ref SparseEntry sparseEntryToSwap = ref sparseSpan[denseEntryToSwap.sparseIndex];

        // make sure the sparse entry points to the  dense slot that its dense data would move to after the swap.  

        sparseEntryToSwap.SetDenseIndex(sparseEntryToRemove.DenseIndex);

        // make sure that the sparse entry that used to have a dense entry is no longer linked to the dense slot.

        int denseIndexToRemove = sparseEntryToRemove.DenseIndex; 
        sparseEntryToRemove.ClearDenseIndex();

        // remove the entry to remove, swapping it with the final entry via SwapbackList.

        dense.RemoveAt(denseIndexToRemove);
        
        return GenIndexResult.Ok;            
    }

    /// <summary>
    /// Deallocates all Dense entries.
    /// </summary>
    public static void DeallocateAll<T>(SwapbackList<DenseEntry<T>> dense, List<SparseEntry> sparse)
    {
        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);

        for(int i = 0; i < denseSpan.Length; i++)
        {
            ref DenseEntry<T> denseEntry = ref denseSpan[i];
            ref SparseEntry sparseEntry = ref sparseSpan[denseEntry.sparseIndex];
            sparseEntry.ClearDenseIndex();
        }

        dense.Clear();
    }


    /// <summary>
    /// Gets a reference to stored data associated with a GenIndex.
    /// </summary>
    /// <remarks>
    /// The returned reference becomes invalid if the underlying allocation is deallocated
    /// or if the allocations list is modified. Do **not** store this reference; use it immediately.
    /// </remarks>
    /// <param name="genIndex">The GenIndex used to look up the dense data.</param>
    /// <param name="reference">A ref handle that can be valid or invalid if data was found to be associated with the GenIndex.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>    
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.DenseNotAllocated"/>
    ///             </description>    
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description> 
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIndexResult GetDenseRef<T>(
        SwapbackList<DenseEntry<T>> dense,
        List<SparseEntry> sparse,
        in GenIndex genIndex, 
        out Ref<T> reference
    )
    {
        reference = default;

        if(ValidateGenIndex(sparse, genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
            return result;

        ref SparseEntry sparseEntry = ref sparseEntryRef.Value;

        if(sparseEntry.HasDenseEntry() == false)
        {
            return GenIndexResult.DenseNotAllocated;
        }

        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        ref DenseEntry<T> denseEntry = ref denseSpan[sparseEntry.DenseIndex];

        reference = new(ref denseSpan[sparseEntry.DenseIndex].Value, true);
        
        return GenIndexResult.Ok;
    }


    /// <summary>
    /// Gets a reference to stored data associated with a GenIndex.
    /// </summary>
    /// <remarks>
    /// The returned reference becomes invalid if the underlying allocation is deallocated
    /// or if the allocations list is modified. Do **not** store this reference; use it immediately.
    /// </remarks>
    /// <param name="genIndex">The GenIndex used to look up the dense data.</param>
    /// <param name="readOnlyReference">A readonly ref handle that can be valid or invalid if data was found to be associated with the GenIndex.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>    
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.DenseNotAllocated"/>
    ///             </description>    
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description>    
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIndexResult GetDenseReadOnlyRef<T>(
        SwapbackList<DenseEntry<T>> dense,
        List<SparseEntry> sparse,
        in GenIndex genIndex,
        out ReadOnlyRef<T> readOnlyReference
    )
    {
        readOnlyReference = default;

        if(ValidateGenIndex(sparse, genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
            return result;

        ref SparseEntry sparseEntry = ref sparseEntryRef.Value;

        if(sparseEntry.HasDenseEntry() == false)
        {
            return GenIndexResult.DenseNotAllocated;
        }

        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        ref DenseEntry<T> denseEntry = ref denseSpan[sparseEntry.DenseIndex];

        readOnlyReference = new(ref denseSpan[sparseEntry.DenseIndex].Value, true);
        
        return GenIndexResult.Ok;
    }


    /// <summary>
    /// Gets a reference to a SparseEntry associated with a GenIndex.
    /// </summary>
    /// <param name="genIndex">The GenIndex used to look up the sparse data.</param>
    /// <param name="readonlyReference">A readonly ref handle that can be valid or invalid if data was found to be associated with the GenIndex.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>    
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description>    
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIndexResult GetSparseReadOnlyRef(
        List<SparseEntry> sparse,
        in GenIndex genIndex, 
        out ReadOnlyRef<SparseEntry> readonlyReference
    )
    {
        readonlyReference = default;
        
        if(ValidateGenIndex(sparse, genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
            return result;
        
        ref SparseEntry sparseEntry = ref sparseEntryRef.Value;

        if(sparseEntry.generation != genIndex.Generation)
        {
            return GenIndexResult.StaleGenIndex;
        }

        readonlyReference = new(ref sparseEntry, true);
        return GenIndexResult.Ok;
    }

    /// <summary>
    /// Gets a reference to a SparseEntry associated with a GenIndex.
    /// </summary>
    /// <param name="sparseIndex">The index of the sparse data.</param>
    /// <param name="readonlyReference">A readonly ref handle that can be valid or invalid if data was found to be associated with the GenIndex.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description>    
    ///         </item>
    ///     </list>
    /// </returns> 
    public static void GetSparseReadOnlyRef(
        List<SparseEntry> sparse,
        int sparseIndex, 
        out ReadOnlyRef<SparseEntry> readonlyReference
    )
    {
        readonlyReference = default;

        if(sparse.Count < sparseIndex || sparseIndex >= sparse.Count)
        {
            throw new InvalidOperationException();
        }

        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref SparseEntry sparseEntry = ref sparseSpan[sparseIndex];

        readonlyReference = new(ref sparseEntry, true);
    }

    /// <summary>
    /// Gets a span of the underlying dense collection.
    /// Note: When deallocating and allocating data from this structure, this span is then invalidated.
    /// Do not store the span, make sure to use immeditely or before modifiying the allocations list.
    /// </summary>
    /// <returns>The span od the underlying dense collection.</returns>
    public static Span<DenseEntry<T>> GetDenseAsSpan<T>(SwapbackList<DenseEntry<T>> dense)
    {
        return CollectionsMarshal.AsSpan(dense);
    }

    /// <summary>
    /// Returns a constructed GenIndex from a sparse index.
    /// </summary>
    /// <param name="sparseIndex">The specified sparse index.</param>
    /// <param name="genIndex">The constructed GenIndex.</param>
    public static void GetGenIndex(
        List<SparseEntry> sparse,
        int sparseIndex, 
        out GenIndex genIndex
    )
    {
        GetSparseReadOnlyRef(sparse, sparseIndex, out ReadOnlyRef<SparseEntry> sparseEntry);

        genIndex = new(sparseIndex, sparseEntry.Value.generation);
    }





    /*******************
    
        Gen Index List Overrides.
    
    ********************/




    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool ResizeSparseEntries<T>(GenIndexList<T> genIndexList, int count)
    {
        return ResizeSparseEntries(genIndexList.Sparse, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIndexResult ValidateGenIndex<T>(
        GenIndexList<T> genIndexList, 
        in GenIndex genIndex,
        out Ref<SparseEntry> sparseEntryRef
    )
    {
        return ValidateGenIndex(genIndexList.Sparse, genIndex, out sparseEntryRef);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIndexResult Allocate<T>(
        GenIndexList<T> genIndexList,
        in GenIndex genIndex,
        in T value
    )
    {
        return Allocate(genIndexList.Dense, genIndexList.Sparse, in genIndex, in value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIndexResult Deallocate<T>(
        GenIndexList<T> genIndexList,
        in GenIndex genIndex
    )
    {
        return Deallocate(genIndexList.Dense, genIndexList.Sparse, in genIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DeallocateAll<T>(
        GenIndexList<T> genIndexList
    )
    {
        DeallocateAll(genIndexList.Dense, genIndexList.Sparse);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIndexResult GetDenseRef<T>(
        GenIndexList<T> genIndexList,
        in GenIndex genIndex,
        out Ref<T> reference
    )
    {
        return GetDenseRef(genIndexList.Dense, genIndexList.Sparse, in genIndex, out reference);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIndexResult GetDenseReadOnlyRef<T>(
        GenIndexList<T> genIndexList,
        in GenIndex genIndex,
        out ReadOnlyRef<T> readOnlyReference
    )
    {
        return GetDenseReadOnlyRef(genIndexList.Dense, genIndexList.Sparse, in genIndex, out readOnlyReference);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static GenIndexResult GetSparseReadOnlyRef<T>(
        GenIndexList<T> genIndexList,
        in GenIndex genIndex,
        out ReadOnlyRef<SparseEntry> readonlyReference
    )
    {
        return GetSparseReadOnlyRef(genIndexList.Sparse, in genIndex, out readonlyReference);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetSparseReadOnlyRef<T>(
        GenIndexList<T> genIndexList,
        int sparseIndex,
        out ReadOnlyRef<SparseEntry> readonlyReference
    )
    {
        GetSparseReadOnlyRef(genIndexList.Sparse, sparseIndex, out readonlyReference);        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Span<DenseEntry<T>> GetDenseAsSpan<T>(GenIndexList<T> genIndexList)
    {
        return GetDenseAsSpan(genIndexList.Dense);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetGenIndex<T>(
        GenIndexList<T> genIndexList,
        int sparseIndex,
        out GenIndex genIndex
    )
    {
        GetGenIndex(genIndexList.Sparse, sparseIndex, out genIndex);
    }
}