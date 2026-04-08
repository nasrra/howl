using System;
using System.Diagnostics;
using Howl.Ecs;
using Howl.Generic;


/// <summary>
/// This is an isolated collection of gen-indexed classes that are disposable; outside of the application ecs sytem. 
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class DisposableClassComponentCollection<T> : IDisposable where T : class, IDisposable
{
    /// <summary>
    /// Gets and sets whether this instance has been disposed.
    /// </summary>
    private bool disposed;

    /// <summary>
    /// Gets whether this instance has been disposed.
    /// </summary>
    public bool IsDisposed => disposed;

    /// <summary>
    /// Gets and sets the allocator for allocating gen indices.
    /// </summary>
    private GenIndexAllocator indices;

    /// <summary>
    /// Gets the allocator for allocating gen indices.
    /// </summary>
    public GenIndexAllocator Indices => indices;
    
    /// <summary>
    /// Gets and sets the collection for holding associated data with gen indices.
    /// </summary>
    private GenIndexList<T> data;

    /// <summary>
    /// Gets the collection for holding associated data with gen indices.
    /// </summary>
    public GenIndexList<T> Data => data;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public DisposableClassComponentCollection()
    {
        indices = new();
        data = new();
    }

    /// <summary>
    /// Allocates a value to a new gen index slot.
    /// </summary>
    /// <param name="value">the data-value to allocate.</param>
    /// <param name="genIndex">the gen index to the slot it was allocated to.</param>
    /// <returns></returns>
    public GenIndexResult AllocateNew(T value, out GenIndex genIndex)
    {
        indices.Allocate(out genIndex, out bool reusedFreeGenIndex);
        if (reusedFreeGenIndex == false)
        {
            // resize the sparse entries to match the texture ids so every 
            // id has a possible entry point into the data storage.
            GenIndexListProc.ResizeSparseEntries(data, indices.Entries.Count);
        }

        GenIndexListProc.Allocate(data, genIndex, value).Ok(out GenIndexResult result);
        return result;
    }

    /// <summary>
    /// Deallocates a data entry in at the specified gen index.
    /// </summary>
    /// <param name="genIndex">the specified gen index.</param>
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
    public GenIndexResult Deallocate(GenIndex genIndex)
    {
        GenIndexResult result;
    
        // ensure to dispose of the data before deallocation. 
        if(GenIndexListProc.GetDenseRef(data, genIndex, out Ref<T> reference).Fail(out result))
        {
            goto Fail;
        }
        else
        {
            reference.Value.Dispose();
        }
        
        if(GenIndexListProc.Deallocate(data, genIndex).Fail(out result))
            goto Fail;

        if(GenIndexListProc.Deallocate(data, genIndex).Fail(out result))
            goto Fail;

        return result;

        Fail:
            return result;
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
        GenIndexListProc.GetDenseRef(data, genIndex, out reference).Ok(out var result);
        return result;
    }

    /// <summary>
    /// Disposes this instace.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            Debug.Assert(false);
            return;
        }

        if (disposing)
        {
            // dispose the indices.
            indices.Dispose();
            indices = null;

            // dispose all dense entries.
            Span<DenseEntry<T>> denseEntries = GenIndexListProc.GetDenseAsSpan(data);
            for(int i = 0; i < denseEntries.Length; i++)
            {
                denseEntries[i].Value.Dispose();
            }

            // dispose the data collection. 
            data.Dispose();

            data = null;
        }

        disposed = true;
        GC.SuppressFinalize(this);
    }

    ~DisposableClassComponentCollection()
    {
        Dispose(false);
    }
}