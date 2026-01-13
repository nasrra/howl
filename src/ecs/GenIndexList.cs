using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Howl.Collections;
using Howl.Generic;

namespace Howl.ECS;

public class GenIndexList<T>
{

    List<SparseEntry> sparse;
    public IReadOnlyList<SparseEntry> Sparse => sparse;
    
    SwapbackList<DenseEntry<T>> dense;
    public IReadOnlyList<DenseEntry<T>> Dense => dense;

    public GenIndexList(){
        sparse = new();
        dense = new();
    }

    /// <summary>
    /// resizes the sparse entry list.
    /// 
    /// Note: sparse entries can only grow, not shrink.
    /// A 'length' that is lower than the current length will not cause a resize;
    /// returning false.     
    /// </summary>
    /// <param name="count">The length to resize to.</param>
    /// <returns>true, when the operation successfully increased </returns>

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
            
            denseEntry.value = value;
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

    public GenIndexResult GetDenseRef(ref GenIndex index, out Ref<T> reference)
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

        reference = new(ref denseSpan[sparseEntry.DenseIndex].value, true);
        return GenIndexResult.Success;
    }

    /// <summary>
    /// Returns a reference to stored data associated with a GenIndex.
    /// Note: When deallocating and allocating data from this structure, this ref is then invalidated.
    /// Do not store the ref data, make sure to use immeditely or before modifiying the allocations list.
    /// </summary>
    /// <returns>A ref handle that can be valid or invalid if data was found to be associated with the GenIndex.</returns>

    public GenIndexResult GetDenseReadonlyRef(ref GenIndex index, out ReadonlyRef<T> readonlyReference)
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

        readonlyReference = new(ref denseSpan[sparseEntry.DenseIndex].value, true);
        return GenIndexResult.Success;
    }

    /// <summary>
    /// Returns a reference to a SparseEntry associated with a GenIndex.
    /// Note: When deallocating and allocating data from this structure, this ref is then invalidated.
    /// Do not store the ref data, make sure to use immeditely or before modifiying the allocations list.
    /// </summary>
    /// <returns>A ref handle that can be valid or invalid if data was found to be associated with the GenIndex.</returns>

    public GenIndexResult GetSparseRef(in GenIndex genIndex, out ReadonlyRef<SparseEntry> readonlyReference)
    {
        readonlyReference = default;
        
        if(sparse.Count < genIndex.index)
        {
            return GenIndexResult.InvalidGenIndex;
        }

        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref SparseEntry sparseEntry = ref sparseSpan[genIndex.index];

        if(sparseEntry.generation != genIndex.generation)
        {
            return GenIndexResult.StaleAllocationFound;
        }

        readonlyReference = new(ref sparseEntry, true);
        return GenIndexResult.Success;
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