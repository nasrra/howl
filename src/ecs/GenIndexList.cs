using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Howl.Collections;
using Howl.Generic;

namespace Howl.ECS;

public class GenIndexList<T> : IGenIndexList
{

    List<SparseEntry> sparse;
    public IReadOnlyList<SparseEntry> Sparse => sparse;
    
    SwapbackList<DenseEntry<T>> dense;
    public IReadOnlyList<DenseEntry<T>> Dense => dense;

    public GenIndexList(){
        sparse = new();
        dense = new();
    }

    public bool ResizeSparseEntries(int count)
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

    /// <summary>
    /// Allocates a dense entry data and associates it with a sparse entry.
    /// </summary>
    /// <param name="index">The GenIndex associated with the sparse entry; used to retrieve the dense data.</param>
    /// <param name="value">The data to assign to the dense entry.</param>
    /// <param name="result">A detailed result of what heppend dureing the allocation.</param>
    /// <returns>true, if the allocation was successful; otherwise false.</returns>
    public GenIndexResult Allocate(in GenIndex index, T value)
    {
        if(sparse.Count <= index.index)
        {
            return GenIndexResult.InvalidGenIndex;
        }
        else{
            Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);

            ref SparseEntry sparseEntry = ref sparseSpan[index.index];

            if (sparseEntry.LinkedToADenseEntry() == true)
            {
                return GenIndexResult.DoubleAllocationAttempted;
            }

            if(sparseEntry.generation != index.generation)
            {
                return GenIndexResult.StaleAllocationFound;
            }

            // allocate a new dense entry.
            
            dense.Add(default);
            Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
            int denseIndex = denseSpan.Length - 1;
            ref DenseEntry<T> denseEntry = ref denseSpan[denseIndex];
            sparseEntry.SetDenseIndex(denseIndex);

            // set the dense entry's data.
            
            denseEntry.Value = value;
            denseEntry.sparseIndex = index.index;
            
            return GenIndexResult.Success;
        }
    }

    /// <summary>
    /// Deallocates an dense entry that is associated with sparse entry, setting the sparse entry to none. 
    /// </summary>
    /// <param name="index">The GenIndex associated with the sparse entry; used to retrieve the dense data.</param>
    /// <param name="result">A detailed result of what heppend dureing the allocation.</param>
    /// <returns>true, if the allocation was successful; otherwise false.</returns>

    public GenIndexResult Deallocate(in GenIndex index)
    {
        
        if(sparse.Count <= index.index)
        {
            return GenIndexResult.InvalidGenIndex;
        }

        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref SparseEntry sparseEntryToRemove = ref sparseSpan[index.index];

        if (sparseEntryToRemove.LinkedToADenseEntry() == false)
        {
            return GenIndexResult.DenseNotAllocated;
        }

        if(sparseEntryToRemove.generation != index.generation)
        {
            return GenIndexResult.StaleAllocationFound;
        }

        // get the entry that will be swapped with the data that is to be removed.

        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        ref DenseEntry<T> denseEntryToSwap = ref denseSpan[denseSpan.Length-1];
        ref SparseEntry sparseEntryToSwap = ref sparseSpan[denseEntryToSwap.sparseIndex];

        // make sure the sparse entry points to the  dense slot that its dense data would move to after the swap.  

        sparseEntryToSwap.SetDenseIndex(sparseEntryToRemove.DenseIndex);

        // remove the entry to remove, swapping it with the final entry via SwapbackList.

        Debug.WriteLine(sparseEntryToRemove.DenseIndex);

        dense.RemoveAt(sparseEntryToRemove.DenseIndex);

        // make sure that the sparse entry that used to have a dense entry is no longer linked to the dense slot.

        sparseEntryToRemove.ClearDenseIndex();
        
        return GenIndexResult.Success;
    }

    /// <summary>
    /// Returns a reference to stored data associated with a GenIndex.
    /// Note: When deallocating and allocating data from this structure, this ref is then invalidated.
    /// Do not store the ref data, make sure to use immeditely or before modifiying the allocations list.
    /// </summary>
    /// <returns>A ref handle that can be valid or invalid if data was found to be associated with the GenIndex.</returns>

    public GenIndexResult GetDenseRef(in GenIndex index, out Ref<T> reference)
    {
        reference = default;

        if(sparse.Count <= index.index)
        {
            return GenIndexResult.InvalidGenIndex;
        }

        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref SparseEntry sparseEntry = ref sparseSpan[index.index];

        if(index.generation != sparseEntry.generation)
        {
            return GenIndexResult.StaleAllocationFound;
        }

        if(sparseEntry.LinkedToADenseEntry() == false)
        {
            return GenIndexResult.DenseNotAllocated;
        }

        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        ref DenseEntry<T> denseEntry = ref denseSpan[sparseEntry.DenseIndex];

        reference = new(ref denseSpan[sparseEntry.DenseIndex].Value, true);
        return GenIndexResult.Success;
    }

    /// <summary>
    /// Returns a reference to stored data associated with a GenIndex.
    /// Note: When deallocating and allocating data from this structure, this ref is then invalidated.
    /// Do not store the ref data, make sure to use immeditely or before modifiying the allocations list.
    /// </summary>
    /// <returns>A ref handle that can be valid or invalid if data was found to be associated with the GenIndex.</returns>

    public GenIndexResult GetDenseReadonlyRef(in GenIndex index, out ReadOnlyRef<T> readonlyReference)
    {
        readonlyReference = default;

        if(sparse.Count <= index.index)
        {
            return GenIndexResult.InvalidGenIndex;
        }

        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref SparseEntry sparseEntry = ref sparseSpan[index.index];

        if(index.generation != sparseEntry.generation)
        {
            return GenIndexResult.StaleAllocationFound;
        }

        if(sparseEntry.LinkedToADenseEntry() == false)
        {
            return GenIndexResult.DenseNotAllocated;
        }

        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        ref DenseEntry<T> denseEntry = ref denseSpan[sparseEntry.DenseIndex];

        readonlyReference = new(ref denseSpan[sparseEntry.DenseIndex].Value, true);
        return GenIndexResult.Success;
    }

    /// <summary>
    /// Returns a reference to a SparseEntry associated with a GenIndex.
    /// Note: When deallocating and allocating data from this structure, this ref is then invalidated.
    /// Do not store the ref data, make sure to use immeditely or before modifiying the allocations list.
    /// </summary>
    /// <returns>A ref handle that can be valid or invalid if data was found to be associated with the GenIndex.</returns>
    public GenIndexResult GetSparseReadonlyRef(in GenIndex genIndex, out ReadOnlyRef<SparseEntry> readonlyReference)
    {
        readonlyReference = default;
        
#if DEBUG
        if(sparse.Count < genIndex.index || sparse.Count >= genIndex.index)
        {
            return GenIndexResult.InvalidGenIndex;
        }
#endif

        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref SparseEntry sparseEntry = ref sparseSpan[genIndex.index];

        if(sparseEntry.generation != genIndex.generation)
        {
            return GenIndexResult.StaleAllocationFound;
        }

        readonlyReference = new(ref sparseEntry, true);
        return GenIndexResult.Success;
    }

    public GenIndexResult GetSparseReadonlyRef(int sparseIndex, out ReadOnlyRef<SparseEntry> readonlyReference)
    {
        readonlyReference = default;

#if DEBUG
        if(sparse.Count < sparseIndex || sparseIndex >= sparse.Count)
        {
            return GenIndexResult.InvalidGenIndex;
        }
#endif

        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref SparseEntry sparseEntry = ref sparseSpan[sparseIndex];

        readonlyReference = new(ref sparseEntry, true);
        return GenIndexResult.Success;
    }

    /// <summary>
    /// Gets a span of the underlying dense collection.
    /// Note: When deallocating and allocating data from this structure, this span is then invalidated.
    /// Do not store the span, make sure to use immeditely or before modifiying the allocations list.
    /// </summary>
    /// <returns>The span od the underlying dense collection.</returns>
    public Span<DenseEntry<T>> GetDenseAsSpan()
    {
        return CollectionsMarshal.AsSpan(dense);
    }

    /// <summary>
    /// Returns a constructed GenIndex from a sparse index.
    /// </summary>
    /// <param name="sparseIndex">The specified sparse index.</param>
    /// <returns>The constructed GenIndex.</returns>
    public GenIndex GetGenIndex(int sparseIndex)
    {
        // reconstruct gen index.
        GetSparseReadonlyRef(sparseIndex, out ReadOnlyRef<SparseEntry> sparseEntry);
        return new(sparseIndex, sparseEntry.Value.generation);
    }

    public void PrintAll()
    {
        Debug.WriteLine("[GenIndexList]");
        Debug.WriteLine("$\tSparse");
        for(int i = 0; i < sparse.Count; i++)
        {
            Debug.WriteLine($"\t\t{sparse[i]}");
        }
        Debug.WriteLine("$\tDense");
        for(int i = 0; i < dense.Count; i++)
        {
            Debug.WriteLine($"\t\t{dense[i]}");
        }
    }
}