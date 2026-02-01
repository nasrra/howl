using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Howl.ECS;

public class ComponentRegistry : IDisposable
{
    private bool disposed = false;
    public bool IsDisposed => disposed;

    /// <summary>
    /// The registered ComponentStorages.
    /// </summary>
    private List<IGenIndexList> storage;

    /// <summary>
    /// Gets a readonly view of the internal component storage list.
    /// </summary>
    internal IReadOnlyList<IGenIndexList> Storage => storage;

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
        int id;

        if(ComponentType<T>.IsInitialised == false)
        {
            // intialise the component type id if it hasnt been already.
            ComponentType<T>.Initialise();
        }
        id = ComponentType<T>.GetId();

        Span<IGenIndexList> span = CollectionsMarshal.AsSpan(storage);
        
        if(span[id] == null){            
            
            // heap alocate a new GenIndexList of components
            // if one hasnt already been made.

            IGenIndexList list = new GenIndexList<T>();

            // make sure to resize sparse so its up to date with the allocators entries;
            // so that it doesnt have a sparse count of 0.
            list.ResizeSparseEntries(genIndexAllocator.Entries.Count);
            
            span[id] = list;
        }

        return id;
    }

    /// <summary>
    /// Gets the GenIndexList of components.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <returns>The GenIndexList storing all the components; may return null if there the type was not registered in this component registry.</returns>
    public GenIndexList<T> Get<T>()
    {
        Span<IGenIndexList> span = CollectionsMarshal.AsSpan(storage);   
        return span[ComponentType<T>.GetId()] as GenIndexList<T>;
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
        Span<IGenIndexList> span = CollectionsMarshal.AsSpan(storage);

        for(int i = 0; i < span.Length; i++)
        {
            if (span[i]?.ResizeSparseEntries(count) == false)
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
        ComponentTypeId.ComponentTypeIdAllocated += OnComponentTypeIdAllocated; 
    }

    private void UnlinkEvents()
    {
        genIndexAllocator.OnAllocated -= OnGenIndexAllocatorAllocated;            
        ComponentTypeId.ComponentTypeIdAllocated -= OnComponentTypeIdAllocated; 
    }

    private void OnGenIndexAllocatorAllocated(bool reusedFreeIndex)
    {
        if (reusedFreeIndex == false)
        {
            ResizeSparseEntries(genIndexAllocator.Entries.Count);
        }
    }

    private void OnComponentTypeIdAllocated(int allocatedId)
    {
        // add a null entry (8 bytes instead of a full heap allocated GenIndexList)
        // to save memory space and have less GC pressure.
        // For instance: if there are multiple component registries, like Gui and World registries,
        // there may be specific GUI components or World components that just do not need to be 
        // fully registered an allocated in memory if they are never used.
        storage.Add(null);
    }

    /// <summary>
    /// Throws an exception if this instance is disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException($"{nameof(ComponentRegistry)}");
        }
    }

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
            UnlinkEvents();

            storage.Clear();
            storage = null;
            
            genIndexAllocator.Dispose();
            genIndexAllocator = null;
        }

        disposed = true;
    }

    ~ComponentRegistry()
    {
        Dispose(false);
    }
}

internal class ReadOnlyList<T>
{
}