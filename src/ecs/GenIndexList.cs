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

    private GenIndexResult ValidateGenIndex(in GenIndex genIndex, out Ref<SparseEntry> sprarseEntryRef)
    {
        if(sparse.Count <= genIndex.index || genIndex.index < 0)
        {
            throw new InvalidGenIndexException(genIndex);
        }

        Span<SparseEntry> sparseSpan = CollectionsMarshal.AsSpan(sparse);
        ref SparseEntry sparseEntry = ref sparseSpan[genIndex.index];

        if (sparseEntry.HasDenseEntry())
        {            
            if(sparseEntry.generation < genIndex.generation)
            {
                throw new StaleDenseAllocationException(new(genIndex.index, sparseEntry.generation), genIndex);
            }
            else if(sparseEntry.generation > genIndex.generation)
            {
                sprarseEntryRef = new(ref sparseEntry, true);
                return GenIndexResult.StaleGenIndex;            
            }
        }
        
        sprarseEntryRef = new(ref sparseEntry, true);
        return GenIndexResult.Success;
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
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>
    ///         </item>
    ///         <item> 
    ///             <description>
    ///                 <see cref="GenIndexResult.Success"/>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public GenIndexResult Allocate(in GenIndex genIndex, T value)
    {
        GenIndexResult result = ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef);
    
        if(result != GenIndexResult.Success)
        {
            return result;
        }

        if(sparseEntryRef.Value.HasDenseEntry() && sparseEntryRef.Value.generation == genIndex.generation)
        {
            throw new DuplicateDenseAllocationException(genIndex);
        }

        // update the generation.
        
        sparseEntryRef.Value.generation = genIndex.generation;
    
        // allocate a new dense entry.
        
        dense.Add(default);
        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        int denseIndex = denseSpan.Length - 1;
        ref DenseEntry<T> denseEntry = ref denseSpan[denseIndex];
        sparseEntryRef.Value.SetDenseIndex(denseIndex);

        // set the dense entry's data.
        
        denseEntry.Value = value;
        denseEntry.sparseIndex = genIndex.index;
        
        return GenIndexResult.Success;            
    }

    /// <summary>
    /// Deallocates an dense entry that is associated with sparse entry, setting the sparse entry to none. 
    /// </summary>
    /// <param name="genIndex">The GenIndex associated with the sparse entry; used to retrieve the dense data.</param>
    /// <returns>
    ///     <list type=="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.Success"/>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public GenIndexResult Deallocate(in GenIndex genIndex)
    {
        
        GenIndexResult result = ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef);

        if(result != GenIndexResult.Success)
        {
            return result;
        }

        // get the entry that will be swapped with the data that is to be removed.
        ref SparseEntry sparseEntryToRemove = ref sparseEntryRef.Value;

        if (sparseEntryToRemove.HasDenseEntry() == false)
        {
            throw new DenseNotAllocatedException(genIndex);
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
        
        return GenIndexResult.Success;            
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
    ///                 <see cref="GenIndexResult.Success"/>
    ///             </description> 
    ///         </item>
    ///     </list>
    /// </returns>
    public GenIndexResult GetDenseRef(in GenIndex genIndex, out Ref<T> reference)
    {
        reference = default;

        GenIndexResult result = ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef);

        if(result != GenIndexResult.Success)
        {
            return result;
        }

        ref SparseEntry sparseEntry = ref sparseEntryRef.Value;

        if(sparseEntry.HasDenseEntry() == false)
        {
            return GenIndexResult.DenseNotAllocated;
        }

        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        ref DenseEntry<T> denseEntry = ref denseSpan[sparseEntry.DenseIndex];

        reference = new(ref denseSpan[sparseEntry.DenseIndex].Value, true);
        
        return GenIndexResult.Success;
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
    ///                 <see cref="GenIndexResult.Success"/>
    ///             </description>    
    ///         </item>
    ///     </list>
    /// </returns>
    public GenIndexResult GetDenseReadOnlyRef(in GenIndex genIndex, out ReadOnlyRef<T> readonlyReference)
    {
        readonlyReference = default;

        GenIndexResult result = ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef);

        if(result != GenIndexResult.Success)
        {
            return result;
        }

        ref SparseEntry sparseEntry = ref sparseEntryRef.Value;

        if(sparseEntry.HasDenseEntry() == false)
        {
            return GenIndexResult.DenseNotAllocated;
        }

        Span<DenseEntry<T>> denseSpan = CollectionsMarshal.AsSpan(dense);
        ref DenseEntry<T> denseEntry = ref denseSpan[sparseEntry.DenseIndex];

        readonlyReference = new(ref denseSpan[sparseEntry.DenseIndex].Value, true);
        
        return GenIndexResult.Success;
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
    ///                 <see cref="GenIndexResult.Success"/>
    ///             </description>    
    ///         </item>
    ///     </list>
    /// </returns>
    public GenIndexResult GetSparseReadOnlyRef(in GenIndex genIndex, out ReadOnlyRef<SparseEntry> readonlyReference)
    {
        readonlyReference = default;
        
        GenIndexResult result = ValidateGenIndex(genIndex, out Ref<SparseEntry> sparseEntryRef);
        
        if(result != GenIndexResult.Success)
        {
            return result;    
        }

        ref SparseEntry sparseEntry = ref sparseEntryRef.Value;

        if(sparseEntry.generation != genIndex.generation)
        {
            return GenIndexResult.StaleGenIndex;
        }

        readonlyReference = new(ref sparseEntry, true);
        return GenIndexResult.Success;
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
    ///                 <see cref="GenIndexResult.Success"/>
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
}