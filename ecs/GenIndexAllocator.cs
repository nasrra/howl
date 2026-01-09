using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace howl.ecs;

public class GenIndexAllocator
{

    List<AllocatorEntry> entries;
    List<int> free;

    public GenIndexAllocator(){
        free = new();
        entries = new();
    }

    /// <summary>
    /// Allocates or reuses a free entry within this allocator.
    /// </summary>
    /// <returns>A GenIndex to the entry.</returns>

    public GenIndex Allocate(out AllocatorResult result)
    {
        if(free.Count <= 0)
        {
            result = AllocatorResult.New;
            
            int generation = 0;
            bool isActive = true;
            entries.Add(new(generation, isActive));
            return new(entries.Count-1, 0); 
        }
        else
        {
            result = AllocatorResult.Reuse;
        
            // get the last entry that was freed and remove it.

            int lastFreeIndex = free.Count-1;
            int freeEntryIndex = free[lastFreeIndex];
            free.RemoveAt(lastFreeIndex);
            
            // update its generational parameters.

            Span<AllocatorEntry> entrySpan = CollectionsMarshal.AsSpan(entries); 
            ref AllocatorEntry reuseEntry = ref entrySpan[freeEntryIndex];
            reuseEntry.generation += 1;
            reuseEntry.isActive = true;

            return new(freeEntryIndex, reuseEntry.generation);
        }
    }

    /// <summary>
    /// Marks a GenIndex as free, setting it as inactive in the entries list.
    /// </summary>
    /// <param name="genIndex">The generational index to deallocate.</param>
    /// <returns>true, if the generational index was successfully deallocated; otherwise false.</returns>

    public bool Deallocate(in GenIndex genIndex)
    {
        if (IsValid(genIndex))
        {
            // retrieve the entry.

            Span<AllocatorEntry> span = CollectionsMarshal.AsSpan(entries);
            ref AllocatorEntry entry = ref span[genIndex.index];
            entry.isActive = false;
            free.Add(genIndex.index);

            return true;
        }
        else
        {
            // entry was nt valid.

            return false;
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