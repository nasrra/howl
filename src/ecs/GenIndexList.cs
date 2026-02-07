using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Howl.Collections;
using Howl.Generic;

namespace Howl.ECS;

public sealed class GenIndexList<T> : IGenIndexList
{
    List<SparseEntry> sparse;
    public IReadOnlyList<SparseEntry> Sparse => sparse;
    
    SwapbackList<DenseEntry<T>> dense;
    public IReadOnlyList<DenseEntry<T>> Dense => dense;

    private bool disposed;
    public bool IsDisposed => disposed;

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

    private GenIndexResult ValidateGenIndex(in GenIndex genIndex, out Ref<SparseEntry> sparseEntryRef)
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
    public GenIndexResult Allocate(in GenIndex genIndex, in T value)
    {
        if(ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
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
    ///     <list type=="bullet">
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
    public GenIndexResult Deallocate(in GenIndex genIndex)
    {
        if(ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
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
    public void DeallocateAll()
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
    public GenIndexResult GetDenseRef(in GenIndex genIndex, out Ref<T> reference)
    {
        reference = default;

        if(ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
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
    public GenIndexResult GetDenseReadOnlyRef(in GenIndex genIndex, out ReadOnlyRef<T> readonlyReference)
    {
        readonlyReference = default;

        if(ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
            return result;

        ref SparseEntry sparseEntry = ref sparseEntryRef.Value;

        if(sparseEntry.HasDenseEntry() == false)
        {
            return GenIndexResult.DenseNotAllocated;
        }

        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        ref DenseEntry<T> denseEntry = ref denseSpan[sparseEntry.DenseIndex];

        readonlyReference = new(ref denseSpan[sparseEntry.DenseIndex].Value, true);
        
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
    public GenIndexResult GetSparseReadOnlyRef(in GenIndex genIndex, out ReadOnlyRef<SparseEntry> readonlyReference)
    {
        readonlyReference = default;
        
        if(ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef).Fail(out var result))
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
    public void GetSparseReadOnlyRef(int sparseIndex, out ReadOnlyRef<SparseEntry> readonlyReference)
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
    public Span<DenseEntry<T>> GetDenseAsSpan()
    {
        return CollectionsMarshal.AsSpan(dense);
    }

    /// <summary>
    /// Returns a constructed GenIndex from a sparse index.
    /// </summary>
    /// <param name="sparseIndex">The specified sparse index.</param>
    /// <param name="genIndex">The constructed GenIndex.</param>
    public void GetGenIndex(int sparseIndex, out GenIndex genIndex)
    {
        GetSparseReadOnlyRef(sparseIndex, out ReadOnlyRef<SparseEntry> sparseEntry);

        genIndex = new(sparseIndex, sparseEntry.Value.generation);
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


    /// 
    /// Disposal.
    /// 


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            sparse.Clear();
            sparse = null;
            dense.Clear();
            dense = null;
        }

        disposed = true;
    }

    ~GenIndexList(){
        Dispose(false);
    }
}