using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Howl.ECS;

public class GenIndexAllocator
{
    List<AllocatorEntry> entries;
    public IReadOnlyList<AllocatorEntry> Entries => entries;
    
    List<int> free;
    public IReadOnlyList<int> Free => free;

    public GenIndexAllocator(){
        free = new();
        entries = new();
    }

    /// <summary>
    /// Allocates a new entry or reuses a previously freed one.
    /// </summary>
    /// <param name="genIndex">
    /// The generation index associated with the allocated entry.
    /// </param>
    /// <returns>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <see cref="AllocatorResult.AllocatedNewGenIndex"/> — a new entry was created.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="AllocatorResult.ReusedGenIndex"/> — a free entry was reused.
    ///     </description>
    ///   </item>
    /// </list>
    /// </returns>

    public AllocatorResult Allocate(out GenIndex genIndex)
    {
        if(free.Count <= 0)
        {
            
            int generation = 0;
            bool isActive = true;
            entries.Add(new(generation, isActive));
            genIndex = new(entries.Count-1, 0); 
            return AllocatorResult.AllocatedNewGenIndex;
        }
        else
        {
        
            // get the last entry that was freed and remove it.

            int lastFreeIndex = free.Count-1;
            int freeEntryIndex = free[lastFreeIndex];
            free.RemoveAt(lastFreeIndex);
            
            // update its generational parameters.

            Span<AllocatorEntry> entrySpan = CollectionsMarshal.AsSpan(entries); 
            ref AllocatorEntry reuseEntry = ref entrySpan[freeEntryIndex];
            reuseEntry.generation += 1;
            reuseEntry.isActive = true;

            genIndex = new(freeEntryIndex, reuseEntry.generation);
            return AllocatorResult.ReusedGenIndex;
        }
    }

    /// <summary>
    /// Marks a GenIndex as free, setting it as inactive in the entries list.
    /// </summary>
    /// <param name="genIndex">The generational index to deallocate.</param>
    /// <returns>
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             <see cref="AllocatorResult.DeallocatedGenIndex"/> - Then GenIndex was successfully freed for reuse.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             <see cref="AllocatorResult.InvalidGenIndex"/> - The GenIndex is not a valid handle within this allocator.
    ///         </description>
    ///     </item> 
    /// </list>
    /// </returns>
    public AllocatorResult Deallocate(in GenIndex genIndex)
    {
        if (IsValid(genIndex))
        {
            // retrieve the entry.

            Span<AllocatorEntry> span = CollectionsMarshal.AsSpan(entries);
            ref AllocatorEntry entry = ref span[genIndex.index];
            entry.isActive = false;
            free.Add(genIndex.index);

            return AllocatorResult.DeallocatedGenIndex;
        }
        else
        {
            // entry was nt valid.

            return AllocatorResult.InvalidGenIndex;
        }
    }

    /// <summary>
    /// Checks whether o  not a specified GenIndex matches an entry within this allocator.
    /// </summary>
    /// <param name="genIndex">The generational index to check.</param>
    /// <returns>true, if the generational index is valid; otherwise false.</returns>

    public bool IsValid(in GenIndex genIndex)
    {
        if(entries.Count > genIndex.index)
        {
            Span<AllocatorEntry> span = CollectionsMarshal.AsSpan(entries);
            ref AllocatorEntry entry = ref span[genIndex.index];
            if(entry.generation == genIndex.generation && entry.isActive == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public int GetEntriesCount()
    {
        return entries.Count;
    }

    public void PrintAll()
    {
        Debug.WriteLine("[Generational Index Allocator]");
        Span<AllocatorEntry> span = CollectionsMarshal.AsSpan(entries);
        for(int i = 0; i < span.Length; i++)
        {
            Debug.WriteLine(span[i]);
        }
    }
}