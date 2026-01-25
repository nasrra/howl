using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Howl.ECS;

public class ComponentRegistry : IDisposable
{
    private bool disposed = false;
    public bool IsDisposed => disposed;

    /// <summary>
    /// The registered ComponentStorages.
    /// </summary>
    private List<ComponentStorage> storage;

    /// <summary>
    /// The GenIndexAllocator associated with this ComponentRegistery.
    /// </summary>
    private GenIndexAllocator genIndexAllocator;

    /// <summary>
    /// Creates a new ComponentRegistery instance.
    /// </summary>
    public ComponentRegistry(GenIndexAllocator genIndexAllocator)
    {
        storage = new();
        this.genIndexAllocator = genIndexAllocator;
        LinkEvents();
    }

    /// <summary>
    /// Registers a component and creates a storage slot for it.
    /// </summary>
    /// <typeparam name="T">The type of the component to register.</typeparam>
    /// <returns>The id of the registered component.</returns>
    public int RegisterComponent<T>()
    {
        // Accessing Id ensures the type gets a unique ID
        int id = ComponentType<T>.Id;
        storage.Add(new ComponentStorage(new GenIndexList<T>()));
        return id;
    }

    /// <summary>
    /// Gets the GenIndexList of components.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <returns>The GenIndexList storing all the components.</returns>
    public GenIndexList<T> Get<T>()
    {
        Span<ComponentStorage> span = CollectionsMarshal.AsSpan(storage);   
        return span[ComponentType<T>.Id].GenIndexList as GenIndexList<T>;
    }

    /// <summary>
    /// resizes the sparse entry list for every component storage stored in this instance..
    /// 
    /// Note: sparse entries can only grow, not shrink.
    /// A 'length' that is lower than the current length will not cause a resize;
    /// returning false.
    /// </summary>
    /// <param name="count">The length to resize to.</param>
    /// <returns>true, when the operation successfully increased </returns>
    public bool ResizeSparseEntries(int count)
    {
        Span<ComponentStorage> span = CollectionsMarshal.AsSpan(storage);

        for(int i = 0; i < span.Length; i++)
        {
            if (span[i].GenIndexList.ResizeSparseEntries(count) == false)
            {
                return false;
            }
        }
        
        return true;
    }


    ///
    /// Event Linkage.
    /// 

    
    private void LinkEvents()
    {
        genIndexAllocator.OnAllocated += OnGenIndexAllocatorAllocated;    
    }

    private void UnlinkEvents()
    {
        genIndexAllocator.OnAllocated -= OnGenIndexAllocatorAllocated;            
    }

    private void OnGenIndexAllocatorAllocated(bool reusedFreeIndex)
    {
        if (reusedFreeIndex == false)
        {
            ResizeSparseEntries(genIndexAllocator.Entries.Count);
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

    protected void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            storage.Clear();
            UnlinkEvents();
        }

        disposed = true;
    }

    ~ComponentRegistry()
    {
        Dispose(false);
    }
}